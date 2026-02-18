![FTdx101 WebApp Screenshot](pictures/WebApp.png)

# FTdx101 WebApp

## 📖 Why This Application Exists
I wrote this application because I can't see the FTdx101MP controls without using a magnifying glass. As a ham who uses WSJT-X, JTAlert, and Log4OM, there are many controls on the radio that I simply never touch. This web-based interface gives me a clean, large, easy-to-read control panel for the functions I actually use day-to-day.

I also use this application on my tablet, which provides a portable control panel in the shack. The large buttons and readable display work great on touchscreens, though the digit-by-digit frequency tuning feature (click digit + mouse wheel) hasn't been implemented for touch devices yet.

## Important - You need .NET10 to run this application. Download it from the official Microsoft website: https://dotnet.microsoft.com/en-us/download/dotnet/10.0 and install it before running.

## ⚠️ Limitations

- **Windows Only:** Serial port implementation currently requires Windows due to .NET 10 SerialPort limitations.
- **Touch Frequency Tuning:** Digit-by-digit frequency tuning not yet implemented for touch devices.
- **Single Radio:** Designed for controlling one radio at a time.
- **Linux:** Won't be implemented by me due to serial port implementation issues in .NET 10.
- **No Memory Management:** Per-band memory and memory channel management are not yet implemented.
- **No WSJT-X UDP Integration:** WSJT-X UDP control and status monitoring are on the roadmap but not yet available.
- **No Filter Controls:** Filter width and shift controls are not currently implemented.
- **No Antenna Update in the APP.** If the user changes the antenna selection on the radio, the app will not reflect this change.
## 🚀 Key Features
- **CAT Control:** Full control over frequency, mode, band, and antenna selection via the radio's CAT interface.
- **Radio changes reflected in real time:** Thanks to the use of Auto Information mode, apart from antenna changes, changes made on the radio (frequency, mode, band, etc.) are immediately reflected in the web app with minimal latency.
- **Band Selection:** Quick access to all amateur bands from 160m to 4m. Note Bands are UK-centric and may not reflect all international band plans.
- **Power Control:** Adjust the radio's power output directly from the web interface.
- **Real-Time S-Meter:** Live updates of signal strength with an analog-style meter.
- **Reactive State Management:** Real-time updates for all radio parameters with minimal latency.
- **Built-in CAT Multiplexer:** The web app is the only process that opens the radio's serial port, eliminating conflicts with other applications.
- **Real-Time Control:** Almost Instant updates for frequency, mode, band, and antenna changes.
- **Large, Accessible UI:** Clean, readable controls for frequency, band, mode, and antenna selection.
- **Dual Receiver Support:** Independent control and display for VFO A and VFO B.
- **Auto Information Mode (AI1;):** The app enables the radio's Auto Information mode, so the radio streams status updates (frequency, mode, S-meter, etc.) automatically to the web app for low-latency, real-time updates.
- **TCP Integration (rigctld-compatible):**  
  The web app exposes a TCP rigctld-compatible CAT server. Applications such as
  Log4OM, GridTracker, and other Hamlib-based tools can connect over TCP.  
  The web app is the only process that opens the radio's serial port.

- **WSJT-X Integration (UDP):**  ## On wish list.
  WSJT-X does not use rigctld over TCP. Instead, it communicates via UDP
  broadcasts and directed UDP commands. The web app will eventually listen for WSJT-X status
  messages and will respond to CAT control commands (frequency, mode, PTT, etc.)
  using the WSJT-X UDP protocol. This is my next to do.
- **No Virtual COM Ports Needed:** Eliminates the need for third-party serial port sharing utilities.
- **Tablet and Touch Friendly:** Optimized for use on tablets and touch devices.

---

## 🏗️ Architecture: Reactive State with Intelligent Polling

### 🆕 Auto Information Mode (AI1;)
This application now leverages the FTdx101's Auto Information mode by sending the AI1; CAT command. When enabled, the radio automatically streams status updates to the application, eliminating the need for constant polling and providing a more responsive user experience.

**How it works:**
- On connection, the app sends AI1; to the radio followed by a set of commands to request current data.
- The radio pushes real-time status messages, which are processed and reflected in the UI.
- This reduces latency and improves the experience for live frequency and S-meter updates.


**The Problem:** Early versions polled the radio constantly for all parameters, creating serial port congestion and slow UI updates.

**The Solution:**
- **Reactive State Service:** Radio changes stream in real-time via Auto-Information (AI) mode
- **Smart Polling:** Only S-meters are polled (3x per second) since AI mode doesn't provide them
- **Instant Updates:** Frequency, mode, and antenna changes propagate immediately
- **No Queue Backup:** Minimal serial commands = fast, responsive UI

This architecture change in transformed the app from sluggish polling to buttery-smooth real-time control!

## 🎯 Current Capabilities

- **Frequency Control:** Large, readable frequency display with interactive tuning
- **Band Selection:** Quick access to all amateur bands (160m - 4m)
- **Mode Selection:** All modes available but this makes the screen cluttered, so I may implement a mode filter in the future.
- **Antenna Switching:** Select between ANT 1, 2, or 3
- **Power Control:** Adjustable power output (0-200W for FT-dx101MP)
- **S-Meter Display:** Real-time analogue meter showing signal strength
- **Dual Receiver Support:** Independent control of both VFO A and VFO B
- **Built-in CAT Multiplexer:** 
- **Reactive Architecture:** Instant response to radio changes
- **Tablet Compatible:** Works on tablets and touch devices

---

## 🔧 What's Next

If there's interest in this program, I'm open to suggestions for additional controls to add, such as:

- Touch-friendly frequency tuning for tablets
- Per-band memory (frequency, mode, antenna, power)
- Filter selection (width, shift)
- Noise blanker controls
- AGC settings
- Clarifier/RIT controls
- Split operation
- Memory management


My next focus will be on integrating WSJT-X UDP control and status monitoring. 

Suggestions are welcome!  
Open an issue or discussion with your ideas.

---

## 🏗️ Technology Stack

- **Backend:** ASP.NET Core Razor Pages (.NET 10)
- **Frontend:** Bootstrap 5, JavaScript, HTML5 Canvas
- **CAT Control:** Serial Port communication via FT-dx101 CAT protocol
- **TCP Server:** rigctld-compatible interface for external apps like WSJT-X, Log4OM, JTAlert
- **Gauges:** Canvas-Gauges library for analog S-Meter display
- **Architecture:** Reactive state with Auto-Information (AI) mode streaming
- **Platform:** Windows only (serial port implementation requires Windows)

---

## 🖥️ Usage

- Use the web interface to control frequency, band, mode, and antenna.
- All radio state changes are reflected in real time thanks to Auto Information mode.

---

## ⚙️ Configuration Files

The application uses two JSON files to persist settings between sessions.

### `radio_state.json` — Radio State
Stores your last-used frequency, band, mode, antenna, and power settings.

**Location:**

e.g. `C:\Users\<you>\AppData\Roaming\MM5AGM\FTdx101 WebApp\radio_state.json`

The file is created automatically on first run. You can edit it manually while the application is **not running**.

**Example contents:**
```json
{
  "LastFrequency": "144300000",
  "LastMode": "FM",
  "LastBand": "2m",
  "LastPower": 50,
  "LastAntenna": "ANT 1"
}

```

## ⚠️ .NET 10 x86 (32-bit) Runtime Required

This application is built as a **32-bit (x86)** application so it can integrate with 32-bit ham radio programs such as Log4OM, JTAlert, and WSJT-X.

You **must** install the **.NET 10 x86 Runtime** before running — the 64-bit runtime will not work.

### How to install the correct runtime:
1. Go to [https://dotnet.microsoft.com/en-us/download/dotnet/10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
2. Scroll down to **".NET Runtime 10.x.x"**
3. Under the **Windows** column, click **x86** — not x64
4. Run the downloaded installer
5. Then run the FTdx101 WebApp installer

> **Why x86?** Many popular ham radio applications (Log4OM, WSJT-X, JTAlert, older logging software) are 32-bit programs. A 32-bit web app can share process space and communicate more reliably with these tools. The app runs perfectly on both 32-bit and 64-bit versions of Windows 10/11.
