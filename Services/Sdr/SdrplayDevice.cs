// FTdx101 WebApp – SDRplay Device (P/Invoke into sdrplay_api.dll)
// Implements ISdrDevice using the SDRplay API v3.
// The SDRplay API is callback-based; this class bridges the native callback
// thread to the managed consumer via a System.Threading.Channels.Channel<float[]>.
//
// Native struct layout (sdrplay_api_DeviceT, 96 bytes, x64):
//   Offset  0 : SerNo[64]          — ANSI serial number
//   Offset 64 : hwVer (byte)       — hardware version code
//   Offset 68 : tuner  (int)
//   Offset 72 : rspDuoMode (int)
//   Offset 76 : valid  (byte)
//   Offset 80 : rspDuoSampleFreq (double)
//   Offset 88 : Dev  (HANDLE / IntPtr)
//
// sdrplay_api_DeviceParamsT pointer layout (returned by GetDeviceParams):
//   Offset  0 : devParams*        → sdrplay_api_DevParamsT*
//   Offset  8 : rxChannelA*       → sdrplay_api_RxChannelParamsT*
//   Offset 16 : rxChannelB*       → sdrplay_api_RxChannelParamsT* (null for non-RSPduo)
//
// Within sdrplay_api_DevParamsT:
//   Offset 0 : ppm (double)
//   Offset 8 : fsFreq.fsHz (double)  ← sample rate
//
// Within sdrplay_api_RxChannelParamsT.tunerParams:
//   Offset  0 : bwType (int)
//   Offset  4 : ifType (int)
//   Offset  8 : loMode (int)
//   Offset 12 : gain   (52 bytes)
//   Offset 64 : rfFreq.rfHz (double)  ← centre frequency

using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace FTdx101_WebApp.Services.Sdr
{
    public sealed class SdrplayDevice : ISdrDevice
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const string DllName             = "sdrplay_api";
        private const int    DeviceStructSize    = 96;
        private const int    MaxDevices          = 4;
        private const int    DevHandleOffset     = 88;
        private const int    HwVerOffset         = 64;
        private const int    DevParamsOffset     = 0;   // within DeviceParamsT
        private const int    RxChannelAOffset    = 8;   // within DeviceParamsT
        private const int    FsHzOffset          = 8;   // within DevParamsT
        private const int    RfHzOffset          = 64;  // within RxChannelParamsT
        private const int    CallbackFnsSize     = 24;  // 3 × IntPtr

        // Key prefix that identifies SDRplay devices across the codebase.
        public const string KeyPrefix = "sdrplay:";

        // ── P/Invoke declarations ────────────────────────────────────────────────

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sdrplay_api_Open();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sdrplay_api_Close();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sdrplay_api_GetDevices(
            IntPtr devices, ref uint numDevices, uint maxNumDevices);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sdrplay_api_SelectDevice(IntPtr device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sdrplay_api_ReleaseDevice(IntPtr device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sdrplay_api_GetDeviceParams(
            IntPtr dev, out IntPtr deviceParams);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sdrplay_api_Init(
            IntPtr dev, IntPtr callbackFns, IntPtr cbContext);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sdrplay_api_Uninit(IntPtr dev);

        // ── Callback delegates (must stay rooted to prevent GC) ─────────────────

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void StreamCallbackDelegate(
            IntPtr xi, IntPtr xq,
            IntPtr streamCbParams,
            uint numSamples,
            uint reset,
            IntPtr cbContext);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EventCallbackDelegate(
            int eventId, int tuner, IntPtr eventParams, IntPtr cbContext);

        // ── Instance state ────────────────────────────────────────────────────────

        public string Key   { get; }
        public string Label { get; private set; }

        private readonly Channel<float[]> _channel =
            Channel.CreateBounded<float[]>(new BoundedChannelOptions(4)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });

        // Config (set in Configure, read in callback)
        private int     _fftSize;
        private float[] _accumBuffer = [];
        private int     _accumOffset;

        // Native handles
        private IntPtr _deviceStructPtr;  // unmanaged copy of the selected DeviceT
        private IntPtr _devHandle;        // Dev field from DeviceT (HANDLE)
        private IntPtr _callbackFnsPtr;   // unmanaged CallbackFnsT struct
        private GCHandle _selfHandle;     // pins 'this' for native callback context

        // Delegate fields — kept alive by the instance
        private StreamCallbackDelegate? _streamDelegate;
        private EventCallbackDelegate?  _eventDelegate;

        private bool _apiOpen;
        private bool _streaming;
        private bool _disposed;

        // ── Constructor ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a device wrapper for the given key.
        /// Key format: "sdrplay:&lt;serialNumber&gt;"
        /// </summary>
        public SdrplayDevice(string key)
        {
            if (!key.StartsWith(KeyPrefix, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Key must start with '{KeyPrefix}'.", nameof(key));

            Key   = key;
            Label = key;   // Placeholder; updated to the full model name in Configure().
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all connected SDRplay devices, or an empty list when
        /// sdrplay_api.dll is not found or the API is already in use.
        /// </summary>
        public static IReadOnlyList<SdrDeviceInfo> EnumerateDevices()
        {
            var devices = new List<SdrDeviceInfo>();
            try
            {
                int err = sdrplay_api_Open();
                if (err != 0) return devices;   // API busy or unavailable

                try
                {
                    IntPtr deviceArray = Marshal.AllocHGlobal(DeviceStructSize * MaxDevices);
                    try
                    {
                        uint count = 0;
                        err = sdrplay_api_GetDevices(deviceArray, ref count, MaxDevices);
                        if (err == 0 && count > 0)
                            ReadDeviceList(deviceArray, count, devices);
                    }
                    finally { Marshal.FreeHGlobal(deviceArray); }
                }
                finally { sdrplay_api_Close(); }
            }
            catch (DllNotFoundException) { /* sdrplay_api.dll not installed */ }
            catch (Exception) { /* API unavailable for any other reason */ }

            return devices;
        }

        /// <inheritdoc/>
        public void Configure(long centreFrequencyHz, double sampleRateHz, int fftSize)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(SdrplayDevice));

            _fftSize     = fftSize;
            _accumBuffer = new float[fftSize * 2];
            _accumOffset = 0;

            // Open the API
            ThrowIfError(sdrplay_api_Open(), "sdrplay_api_Open");
            _apiOpen = true;

            // Enumerate and find our device by serial number
            string serial = Key[KeyPrefix.Length..];

            IntPtr deviceArray = Marshal.AllocHGlobal(DeviceStructSize * MaxDevices);
            try
            {
                uint count = 0;
                ThrowIfError(sdrplay_api_GetDevices(deviceArray, ref count, MaxDevices), "GetDevices");

                IntPtr matchPtr = FindDeviceBySerial(deviceArray, count, serial);
                if (matchPtr == IntPtr.Zero)
                    throw new InvalidOperationException(
                        $"SDRplay device '{serial}' not found. Check it is connected.");

                // Update Label now that we know the hardware version.
                byte hwVer = Marshal.ReadByte(matchPtr, HwVerOffset);
                Label = $"SDRplay {HwVerToModel(hwVer)} ({serial})";

                // Copy the struct to our own buffer before freeing the array.
                _deviceStructPtr = Marshal.AllocHGlobal(DeviceStructSize);
                for (int i = 0; i < DeviceStructSize; i++)
                    Marshal.WriteByte(_deviceStructPtr, i, Marshal.ReadByte(matchPtr, i));
            }
            finally { Marshal.FreeHGlobal(deviceArray); }

            // Select the device (populates _deviceStructPtr.Dev)
            ThrowIfError(sdrplay_api_SelectDevice(_deviceStructPtr), "SelectDevice");
            _devHandle = Marshal.ReadIntPtr(_deviceStructPtr, DevHandleOffset);

            // Retrieve parameter pointers and set frequency + sample rate
            ThrowIfError(sdrplay_api_GetDeviceParams(_devHandle, out IntPtr deviceParamsPtr), "GetDeviceParams");

            IntPtr devParams  = Marshal.ReadIntPtr(deviceParamsPtr, DevParamsOffset);
            IntPtr rxChannelA = Marshal.ReadIntPtr(deviceParamsPtr, RxChannelAOffset);

            WriteDouble(devParams,  FsHzOffset, sampleRateHz);
            WriteDouble(rxChannelA, RfHzOffset, (double)centreFrequencyHz);
        }

        /// <inheritdoc/>
        public void StartStreaming()
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(SdrplayDevice));
            if (_streaming) return;

            // Pin 'this' so the native callback can recover the instance.
            _selfHandle = GCHandle.Alloc(this);

            // Create delegates and keep references alive on the instance.
            _streamDelegate = new StreamCallbackDelegate(StreamACallback);
            _eventDelegate  = new EventCallbackDelegate(EventCallback);

            // Build the unmanaged CallbackFnsT struct: [StreamA, StreamB, Event]
            _callbackFnsPtr = Marshal.AllocHGlobal(CallbackFnsSize);
            Marshal.WriteIntPtr(_callbackFnsPtr, 0,  Marshal.GetFunctionPointerForDelegate(_streamDelegate));
            Marshal.WriteIntPtr(_callbackFnsPtr, 8,  Marshal.GetFunctionPointerForDelegate(_streamDelegate));
            Marshal.WriteIntPtr(_callbackFnsPtr, 16, Marshal.GetFunctionPointerForDelegate(_eventDelegate));

            ThrowIfError(sdrplay_api_Init(
                _devHandle,
                _callbackFnsPtr,
                GCHandle.ToIntPtr(_selfHandle)), "sdrplay_api_Init");

            _streaming = true;
        }

        /// <inheritdoc/>
        public async ValueTask<bool> TryReadIqFrameAsync(
            float[] buffer, int timeoutMs, CancellationToken ct = default)
        {
            using var linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(ct);
            linkedCts.CancelAfter(timeoutMs);
            try
            {
                var frame = await _channel.Reader.ReadAsync(linkedCts.Token).ConfigureAwait(false);
                Array.Copy(frame, buffer, Math.Min(frame.Length, buffer.Length));
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (ChannelClosedException)     { return false; }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (!_streaming) return;
            _streaming = false;

            try { sdrplay_api_Uninit(_devHandle); }
            catch { /* ignore errors during stop */ }

            if (_callbackFnsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_callbackFnsPtr);
                _callbackFnsPtr = IntPtr.Zero;
            }

            if (_selfHandle.IsAllocated)
                _selfHandle.Free();

            // Release delegate references so the GC can collect them.
            _streamDelegate = null;
            _eventDelegate  = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Stop();

            if (_devHandle != IntPtr.Zero)
            {
                try { sdrplay_api_ReleaseDevice(_deviceStructPtr); } catch { }
                _devHandle = IntPtr.Zero;
            }

            if (_deviceStructPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_deviceStructPtr);
                _deviceStructPtr = IntPtr.Zero;
            }

            if (_apiOpen)
            {
                try { sdrplay_api_Close(); } catch { }
                _apiOpen = false;
            }

            _channel.Writer.TryComplete();
        }

        // ── Native callbacks ─────────────────────────────────────────────────────

        private void StreamACallback(
            IntPtr xi, IntPtr xq,
            IntPtr _streamCbParams,
            uint numSamples, uint reset,
            IntPtr _cbContext)
        {
            if (reset != 0)
            {
                _accumOffset = 0;
                return;
            }

            float[] accum = _accumBuffer;
            int fftSize   = _fftSize;
            if (accum.Length == 0 || fftSize <= 0) return;

            int srcIdx    = 0;
            int available = (int)numSamples;

            while (srcIdx < available)
            {
                int space  = fftSize - _accumOffset;
                int toCopy = Math.Min(available - srcIdx, space);

                for (int i = 0; i < toCopy; i++)
                {
                    accum[(_accumOffset + i) * 2]     =
                        Marshal.ReadInt16(xi, (srcIdx + i) * 2) / 32768f;
                    accum[(_accumOffset + i) * 2 + 1] =
                        Marshal.ReadInt16(xq, (srcIdx + i) * 2) / 32768f;
                }

                _accumOffset += toCopy;
                srcIdx       += toCopy;

                if (_accumOffset >= fftSize)
                {
                    var frame = new float[fftSize * 2];
                    Array.Copy(accum, frame, fftSize * 2);
                    _channel.Writer.TryWrite(frame);
                    _accumOffset = 0;
                }
            }
        }

        private static void EventCallback(
            int _eventId, int _tuner, IntPtr _eventParams, IntPtr _cbContext)
        {
            // SDRplay events (gain change, overload, etc.) — not needed for spectrum display.
        }

        // ── Static helpers ────────────────────────────────────────────────────────

        private static void ReadDeviceList(IntPtr deviceArray, uint count, List<SdrDeviceInfo> list)
        {
            for (uint i = 0; i < count; i++)
            {
                IntPtr ptr    = deviceArray + (int)(i * DeviceStructSize);
                string serial = Marshal.PtrToStringAnsi(ptr) ?? $"device{i}";
                byte   hwVer  = Marshal.ReadByte(ptr, HwVerOffset);
                string model  = HwVerToModel(hwVer);
                string label  = $"SDRplay {model} ({serial})";

                list.Add(new SdrDeviceInfo(
                    Key:    KeyPrefix + serial,
                    Label:  label,
                    Driver: "sdrplay"));
            }
        }

        private static IntPtr FindDeviceBySerial(IntPtr deviceArray, uint count, string serial)
        {
            for (uint i = 0; i < count; i++)
            {
                IntPtr ptr       = deviceArray + (int)(i * DeviceStructSize);
                string devSerial = Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
                if (string.Equals(devSerial, serial, StringComparison.OrdinalIgnoreCase))
                    return ptr;
            }
            return IntPtr.Zero;
        }

        private static string HwVerToModel(byte hwVer) => hwVer switch
        {
            1 => "RSP1",
            2 => "RSP2",
            3 => "RSP1A",
            4 => "RSPduo",
            5 => "RSPdx",
            6 => "RSP1B",
            7 => "RSPdx R2",
            _ => $"RSP (hwVer={hwVer})"
        };

        private static void WriteDouble(IntPtr ptr, int offset, double value)
        {
            Marshal.WriteInt64(ptr, offset, BitConverter.DoubleToInt64Bits(value));
        }

        private static void ThrowIfError(int err, string operation)
        {
            if (err != 0)
                throw new InvalidOperationException(
                    $"sdrplay_api: {operation} returned error code {err}");
        }
    }
}
