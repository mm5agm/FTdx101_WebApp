// FTdx101 WebApp – Spectrum Panel
// UI module — DOM access is intentional and correct here.
// Owns a single <canvas> element that is divided into two rendering zones:
//   Top 45%  — spectrum trace (line graph of dBFS vs frequency)
//   Bottom 55% — waterfall  (scrolling time–frequency heatmap)
//
// Frequency axis labels are computed from the VFO frequency reported by
// SdrSpectrumPipeline so the display is always centred on the current band.

export class SpectrumPanel {

    /**
     * @param {string} canvasId       ID of the <canvas> element to render into.
     * @param {string} containerId    ID of the wrapper element to show/hide.
     * @param {number} initialVfoHz   Starting VFO A frequency in Hz.
     */
    constructor(canvasId, containerId, initialVfoHz = 14_074_000) {
        this._canvasId    = canvasId;
        this._containerId = containerId;
        this._vfoHz       = initialVfoHz;
        this._status      = 'unconfigured';

        // Waterfall state: ImageData that is scrolled down one row per frame.
        this._waterfallData = null;
        this._waterfallRows = 0;
        this._waterfallCols = 0;

        this._errorDetail = null;

        // Last received spectrum data; held so the canvas can be redrawn on resize.
        this._lastBins    = null;
        this._lastCentreHz = 0;
        this._lastSpanHz   = 0;

        this._resizeObserver = null;
        this._init();
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /** Update the spectrum/waterfall with a new frame of FFT data. */
    update({ bins, centreHz, spanHz }) {
        this._lastBins    = bins;
        this._lastCentreHz = centreHz;
        this._lastSpanHz   = spanHz;
        this._render();
    }

    /** Store the latest error detail string for display alongside status overlays. */
    setError(detail) {
        this._errorDetail = detail;
    }

    /** Receive the current VFO frequency so the axis labels stay accurate. */
    setVfoFrequency(hz) {
        this._vfoHz = hz;
        if (this._lastBins) this._render();
    }

    /**
     * Respond to SDR lifecycle state changes.
     * @param {string} status  One of: "unconfigured" | "connecting" | "streaming"
     *                                 | "disconnected" | "nodll"
     */
    setStatus(status) {
        this._status = status;

        const container = document.getElementById(this._containerId);
        if (!container) return;

        if (status === 'unconfigured') {
            container.style.display = 'none';
            return;
        }

        container.style.display = '';
        this._drawStatusOverlay(status);
    }

    // ── Initialisation ───────────────────────────────────────────────────────

    _init() {
        const container = document.getElementById(this._containerId);
        if (container) container.style.display = 'none';   // hidden until streaming

        const canvas = document.getElementById(this._canvasId);
        if (!canvas) return;

        // Size the canvas to match its CSS layout width.
        this._sizeCanvas(canvas);

        // Rebuild waterfall buffer whenever the canvas is resized.
        this._resizeObserver = new ResizeObserver(() => {
            this._sizeCanvas(canvas);
            this._waterfallData = null;  // force rebuild on next frame
            if (this._lastBins) this._render();
        });
        this._resizeObserver.observe(canvas.parentElement ?? canvas);
    }

    _sizeCanvas(canvas) {
        const w = canvas.parentElement
            ? canvas.parentElement.clientWidth || 800
            : 800;
        canvas.width  = w;
        canvas.height = 280;   // 126px spectrum + 154px waterfall
    }

    // ── Rendering ────────────────────────────────────────────────────────────

    _render() {
        const canvas = document.getElementById(this._canvasId);
        if (!canvas || !this._lastBins) return;

        const ctx         = canvas.getContext('2d');
        const W           = canvas.width;
        const H           = canvas.height;
        const specH       = Math.floor(H * 0.45);
        const wfH         = H - specH;
        const bins        = this._lastBins;
        const centreHz    = this._lastCentreHz;
        const spanHz      = this._lastSpanHz;

        this._drawSpectrum(ctx, bins, W, specH);
        this._drawFrequencyAxis(ctx, bins, W, specH, centreHz, spanHz);
        this._scrollWaterfall(ctx, bins, W, specH, wfH);
    }

    // ── Spectrum trace ───────────────────────────────────────────────────────

    _drawSpectrum(ctx, bins, W, H) {
        const N      = bins.length;
        const dbMin  = -120;
        const dbMax  = 0;
        const range  = dbMax - dbMin;

        // Background
        ctx.fillStyle = '#0a0a14';
        ctx.fillRect(0, 0, W, H);

        // Grid lines at every 20 dB
        ctx.strokeStyle = '#1e2030';
        ctx.lineWidth   = 1;
        for (let db = dbMin; db <= dbMax; db += 20) {
            const y = H - ((db - dbMin) / range) * H;
            ctx.beginPath();
            ctx.moveTo(0, y);
            ctx.lineTo(W, y);
            ctx.stroke();
        }

        // Build the trace path
        ctx.beginPath();
        for (let i = 0; i < N; i++) {
            const x = (i / N) * W;
            const y = H - Math.max(0, Math.min(1, (bins[i] - dbMin) / range)) * H;
            if (i === 0) ctx.moveTo(x, y);
            else         ctx.lineTo(x, y);
        }

        // Close path to the bottom to fill
        ctx.lineTo(W, H);
        ctx.lineTo(0, H);
        ctx.closePath();

        ctx.fillStyle = 'rgba(0, 140, 255, 0.18)';
        ctx.fill();

        // Redraw the outline on top of the fill
        ctx.beginPath();
        for (let i = 0; i < N; i++) {
            const x = (i / N) * W;
            const y = H - Math.max(0, Math.min(1, (bins[i] - dbMin) / range)) * H;
            if (i === 0) ctx.moveTo(x, y);
            else         ctx.lineTo(x, y);
        }
        ctx.strokeStyle = '#00aaff';
        ctx.lineWidth   = 1.5;
        ctx.stroke();

        // dB scale labels (right-aligned)
        ctx.fillStyle  = '#667799';
        ctx.font       = '10px monospace';
        ctx.textAlign  = 'right';
        for (let db = dbMin; db <= dbMax; db += 20) {
            const y = H - ((db - dbMin) / range) * H;
            ctx.fillText(`${db} dB`, W - 4, y - 2);
        }
    }

    // ── Frequency axis ───────────────────────────────────────────────────────

    _drawFrequencyAxis(ctx, bins, W, specH, centreHz, spanHz) {
        const axisH  = 20;
        const tickY0 = specH - axisH;       // top of axis strip
        const labelY = specH - 4;           // baseline for text

        ctx.fillStyle = '#111118';
        ctx.fillRect(0, tickY0, W, axisH);

        // VFO centre marker line (drawn first, behind labels)
        ctx.strokeStyle = 'rgba(0, 170, 255, 0.4)';
        ctx.lineWidth   = 1;
        ctx.beginPath();
        ctx.moveTo(W / 2, 0);
        ctx.lineTo(W / 2, tickY0);
        ctx.stroke();

        // Sanity-check: _vfoHz below 500 kHz means the persisted state hasn't been
        // overwritten by a live FrequencyA SignalR update yet — skip labels.
        if (this._vfoHz < 100_000) {
            ctx.fillStyle = '#667799';
            ctx.font = '10px monospace';
            ctx.textAlign = 'center';
            ctx.fillText('Waiting for VFO frequency…', W / 2, labelY);
            return;
        }

        // Choose a "nice" tick interval that gives roughly 6–12 ticks across the span.
        // Candidate steps in Hz: 50k, 100k, 200k, 250k, 500k, 1M, 2M, 5M, 10M
        const steps = [50e3, 100e3, 200e3, 250e3, 500e3, 1e6, 2e6, 5e6, 10e6];
        const targetTicks = 8;
        const stepHz = steps.find(s => spanHz / s <= targetTicks) ?? steps[steps.length - 1];

        // First tick at the next multiple of stepHz above the left edge
        const leftHz  = this._vfoHz - spanHz / 2;
        const firstHz = Math.ceil(leftHz / stepHz) * stepHz;

        ctx.font      = '10px monospace';
        ctx.textAlign = 'center';

        for (let tickHz = firstHz; tickHz <= leftHz + spanHz; tickHz += stepHz) {
            const x = ((tickHz - leftHz) / spanHz) * W;

            // Tick line
            const isVfo = Math.abs(tickHz - this._vfoHz) < stepHz * 0.01;
            ctx.strokeStyle = isVfo ? 'rgba(0,170,255,0.8)' : '#334466';
            ctx.lineWidth   = 1;
            ctx.beginPath();
            ctx.moveTo(x, tickY0);
            ctx.lineTo(x, tickY0 + 4);
            ctx.stroke();

            // Label — skip if too close to edge or below 0 Hz
            if (x < 24 || x > W - 24 || tickHz <= 0) continue;

            const mhz    = tickHz / 1e6;
            // Decimal places: enough to show the step resolution clearly
            const dp     = stepHz < 1e6 ? (stepHz < 100e3 ? 3 : 2) : 1;
            const label  = mhz.toFixed(dp);

            ctx.fillStyle = isVfo ? '#44aaff' : '#8899bb';
            ctx.fillText(label, x, labelY);
        }
    }

    // ── Waterfall ────────────────────────────────────────────────────────────

    _scrollWaterfall(ctx, bins, W, specH, wfH) {
        // Lazily allocate or reallocate when the canvas size changes.
        if (!this._waterfallData || this._waterfallCols !== W || this._waterfallRows !== wfH) {
            this._waterfallCols = W;
            this._waterfallRows = wfH;
            this._waterfallData = ctx.createImageData(W, wfH);
            // Initialise to black.
            this._waterfallData.data.fill(0);
            for (let p = 3; p < this._waterfallData.data.length; p += 4)
                this._waterfallData.data[p] = 255;   // alpha = 255
        }

        const data = this._waterfallData.data;
        const N    = bins.length;

        // Shift all existing rows down by one pixel (4 bytes per pixel, W pixels per row).
        const rowBytes = W * 4;
        data.copyWithin(rowBytes, 0, data.length - rowBytes);

        // Draw new row at the top.
        for (let x = 0; x < W; x++) {
            const binIdx = Math.floor((x / W) * N);
            const [r, g, b] = SpectrumPanel._dbToColor(bins[binIdx]);
            const p = x * 4;
            data[p + 0] = r;
            data[p + 1] = g;
            data[p + 2] = b;
            data[p + 3] = 255;
        }

        ctx.putImageData(this._waterfallData, 0, specH);
    }

    // ── Status overlay ───────────────────────────────────────────────────────

    _drawStatusOverlay(status) {
        const canvas = document.getElementById(this._canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        const W   = canvas.width;
        const H   = canvas.height;

        if (status === 'streaming') {
            // Clear any previous overlay; the next data frame will paint correctly.
            return;
        }

        const messages = {
            connecting:   'Connecting to SDR device…',
            disconnected: 'SDR device unavailable — retrying every 5 s',
            nodll:        'SoapySDR.dll not found — install SoapySDR + device driver',
        };

        const line1 = messages[status] ?? `SDR status: ${status}`;
        const line2 = status === 'disconnected' && this._errorDetail
            ? this._errorDetail
            : null;

        ctx.fillStyle = '#0a0a14';
        ctx.fillRect(0, 0, W, H);

        ctx.fillStyle = '#8899bb';
        ctx.textAlign = 'center';

        ctx.font = '14px sans-serif';
        ctx.fillText(line1, W / 2, H / 2 - (line2 ? 10 : 0));

        if (line2) {
            ctx.font      = '11px sans-serif';
            ctx.fillStyle = '#cc6655';
            ctx.fillText(line2, W / 2, H / 2 + 12);
        }
    }

    // ── Color mapping ────────────────────────────────────────────────────────

    /**
     * Maps a dBFS value (−120 … 0) to an RGB thermal colour.
     * Black → blue → cyan → green → yellow → red.
     */
    static _dbToColor(db) {
        const t = Math.max(0, Math.min(1, (db + 120) / 120));
        if (t < 0.2)  return [0,                   0,                   Math.round(t * 5 * 180)];
        if (t < 0.4)  return [0,                   Math.round((t - 0.2) * 5 * 200), 180];
        if (t < 0.6)  return [0,                   200,                 Math.round(180 - (t - 0.4) * 5 * 180)];
        if (t < 0.8)  return [Math.round((t - 0.6) * 5 * 255), 200,    0];
        return               [255,                 Math.round(200 - (t - 0.8) * 5 * 200), 0];
    }
}
