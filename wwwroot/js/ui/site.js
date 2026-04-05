// --- Fullscreen Toggle: 'f' or 'F' to enter, 'Esc' to exit ---
document.addEventListener('keydown', function (e) {
    // Ignore if typing in an input, textarea, or contenteditable
    const active = document.activeElement;
    if (active && (active.tagName === 'INPUT' || active.tagName === 'TEXTAREA' || active.isContentEditable)) return;
    if (e.key === 'f' || e.key === 'F') {
        // Enter fullscreen on the <body> element (entire app)
        const body = document.body;
        if (body && !document.fullscreenElement) {
            body.requestFullscreen && body.requestFullscreen();
            e.preventDefault();
        }
    } else if (e.key === 'Escape') {
        // Exit fullscreen if in fullscreen
        if (document.fullscreenElement) {
            document.exitFullscreen && document.exitFullscreen();
            e.preventDefault();
        }
    }
});

// Add/remove fullscreen-mode class on body when entering/exiting fullscreen
document.addEventListener('fullscreenchange', function () {
    if (document.fullscreenElement) {
        document.body.classList.add('fullscreen-mode');
    } else {
        document.body.classList.remove('fullscreen-mode');
    }
});

// Debugging: Log Save Button Presses and Page Content for Language Issues
// ========================================================================
// This block helps diagnose why the browser might think the page is in French.
// It logs all clicks on elements with "save" in their id, name, or class,
// and logs the text content of the page and any form data being submitted.
document.addEventListener('click', function (e) {
    let el = e.target;
    if (!el) return;
    // Check if the element is a button or input with "save" in id, name, or class
    let isSave = false;
    if (el.tagName === 'BUTTON' || el.tagName === 'INPUT') {
        let id = (el.id || '').toLowerCase();
        let name = (el.name || '').toLowerCase();
        let cls = (el.className || '').toLowerCase();
        if (id.includes('save') || name.includes('save') || cls.includes('save')) {
            isSave = true;
        }
    }
    // Also check parent elements (for icon buttons etc.)
    if (!isSave && el.closest) {
        let btn = el.closest('button, input');
        if (btn) {
            let id = (btn.id || '').toLowerCase();
            let name = (btn.name || '').toLowerCase();
            let cls = (btn.className || '').toLowerCase();
            if (id.includes('save') || name.includes('save') || cls.includes('save')) {
                isSave = true;
                el = btn; // Use the button/input as the element
            }
        }
    }
    // Removed debug logging and diagnostic alert for production cleanup
    // (No action needed on save button press)
});
// Style fix for Raw Power Out label
document.addEventListener('DOMContentLoaded', function () {
    var rawPowerLabel = document.getElementById('raw-powerout-label');
    if (rawPowerLabel) {
        rawPowerLabel.style.removeProperty('max-width');
        rawPowerLabel.style.minWidth = '120px';
        rawPowerLabel.style.removeProperty('width');
        rawPowerLabel.style.whiteSpace = 'nowrap';
        rawPowerLabel.style.textAlign = 'right';
        rawPowerLabel.style.fontFamily = 'monospace';
        rawPowerLabel.style.display = 'inline-block';
        rawPowerLabel.style.marginLeft = '12px';
    }

    // --- SignalR connection setup and disconnect on page unload ---
    if (window.signalRConnection === undefined) {
        window.signalRConnection = new signalR.HubConnectionBuilder().withUrl("/radioHub").build();
        window.signalRConnection.start().catch(function (err) { });
        // Heartbeat: send every 5 seconds
        window.signalRHeartbeatInterval = setInterval(function () {
            if (window.signalRConnection && window.signalRConnection.invoke) {
                window.signalRConnection.invoke("Heartbeat").catch(function (err) {
                    // Ignore errors if connection is closed
                });
            }
        }, 5000);
    }
    // Use 'unload', 'beforeunload', and 'visibilitychange' for best reliability
    window.addEventListener('unload', function (e) {
        if (window.signalRConnection && window.signalRConnection.stop) {
            window.signalRConnection.stop();
            if (window.signalRHeartbeatInterval) {
                clearInterval(window.signalRHeartbeatInterval);
            }
        }
    });
    window.addEventListener('beforeunload', function () {
        if (window.signalRConnection && window.signalRConnection.stop) {
            window.signalRConnection.stop();
            if (window.signalRHeartbeatInterval) {
                clearInterval(window.signalRHeartbeatInterval);
            }
        }
    });
    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'hidden') {
            if (window.signalRConnection && window.signalRConnection.stop) {
                window.signalRConnection.stop();
                if (window.signalRHeartbeatInterval) {
                    clearInterval(window.signalRHeartbeatInterval);
                }
            }
        }
    });
});
// FTdx101 Web App - site.js
// =============================================================================
// This file has two main sections:
//
//  1. A small block of globals (lines ~1-400) that were written early in the
//     project: the outer `state`, outer `fetchRadioStatus`, outer SignalR
//     handler, and the outer pollInitStatus / DOMContentLoaded wiring.
//
//  2. An IIFE (Immediately Invoked Function Expression) block that contains the
//     full, authoritative implementation: its own inner `state`, all the real
//     polling logic, highlightButtons, gauge init, etc.
//
// The outer globals are kept because the Razor pages call window.setBand,
// window.setMode, window.setAntenna, and window.radioControl directly via
// inline onchange="..." attributes, and the IIFE overwrites window.radioControl
// at the end with the real implementations.
//
// THE BUG THAT WAS FIXED:
// When the radio itself changed mode (e.g. the user turned the MODE knob on
// the front panel), the backend sent a SignalR "RadioStateUpdate" with
// property="ModeA" / "ModeB".  The handler only updated the modeDisplayA/B
// <span> element (the text label under the buttons), but never set .checked
// on the corresponding <input type="radio"> button.  So the text changed but
// the selected button did not move.
//
// Fix is in the first SignalR handler (~line 300) and the second one
// (~line 1017): both now call updateModeRadioButton() which sets .checked
// on the matching input[name="modeA/B"] element.
// =============================================================================

// ---------------------------------------------------------------------------
// OUTER GLOBALS
// These exist because the Razor page's inline onchange handlers fire before
// the IIFE runs, so window.setBand / setMode / setAntenna must be defined
// at global scope.  The IIFE later replaces window.radioControl with its
// own (better) versions.
// ---------------------------------------------------------------------------



// Frequency display renderer (outer version, used by outer updateFrequencyDisplay)
function updateFrequencyDisplay(receiver, freqHz) {
    const display = document.getElementById('freq' + receiver);
    if (!display) {
        return;
    }
    let selIdx = window.radioControl && window.radioControl._state ? window.radioControl._state.selectedIdx[receiver] : null;
    let editing = window.radioControl && window.radioControl._state ? window.radioControl._state.editing[receiver] : false;
    let localFreq = window.radioControl && window.radioControl._state ? window.radioControl._state.localFreq[receiver] : null;
    let lastBackendFreq = window.radioControl && window.radioControl._state ? window.radioControl._state.lastBackendFreq[receiver] : null;
    let freqToShow = (!editing || localFreq === null)
        ? lastBackendFreq
        : localFreq;
    display.innerHTML = renderFrequencyDigits(freqToShow, selIdx);
}

function renderFrequencyDigits(freq, selIdx) {
    // Show dashes if no valid frequency yet
    if (!freq || isNaN(freq) || freq < 100) {
        return '<span class="digit">-</span><span class="digit">-</span>.<span class="digit">-</span><span class="digit">-</span><span class="digit">-</span>.<span class="digit">-</span><span class="digit">-</span><span class="digit">-</span>';
    }
    let s = freq.toString().padStart(8, "0");
    let html = "";
    let digitIdx = 0;
    for (let i = 0; i < 8; i++) {
        if (i === 2 || i === 5) {
            html += '<span class="digit">.</span>';
        }
        let selected = (selIdx === digitIdx) ? " selected" : "";
        html += `<span class="digit${selected}" tabindex="0">${s[i]}</span>`;
        digitIdx++;
    }
    return html;
}

// Outer digit interaction initializer (touch/pointer support)
function initializeDigitInteraction(receiver) {
    const display = document.getElementById('freq' + receiver);
    const controls = document.getElementById('freq' + receiver + '-controls');
    const upBtn = document.getElementById('freq' + receiver + '-up');
    const downBtn = document.getElementById('freq' + receiver + '-down');
    if (!display) return;
    if (display._initialized) return;
    display._initialized = true;

    display.addEventListener('pointerdown', function (e) {
        let digits = Array.from(display.querySelectorAll('.digit')).filter(d => d.textContent !== '.');
        let idx = -1;
        if (e.target.classList.contains('digit') && e.target.textContent !== '.') {
            idx = digits.indexOf(e.target);
        } else {
            let x = e.clientX;
            let minDist = Infinity;
            digits.forEach((d, i) => {
                let digitRect = d.getBoundingClientRect();
                let digitCenter = digitRect.left + digitRect.width / 2;
                let dist = Math.abs(x - digitCenter);
                if (dist < minDist) {
                    minDist = dist;
                    idx = i;
                }
            });
        }
        if (idx !== -1 && window.radioControl && window.radioControl._state) {
            digits.forEach(d => d.classList.remove('selected'));
            window.radioControl._state.selectedIdx[receiver] = idx;
            digits[idx].classList.add('selected');
            window.radioControl._state.editing[receiver] = true;
            window.radioControl._state.localFreq[receiver] = parseInt(digits.map(d => d.textContent).join(''));
            updateFrequencyDisplay(receiver, window.radioControl._state.localFreq[receiver]);
        }
        e.preventDefault();
    });

    display.addEventListener('wheel', function (e) {
        let digits = Array.from(display.querySelectorAll('.digit')).filter(d => d.textContent !== '.');
        let idx = state.selectedIdx[receiver];
        if (idx === null || !digits[idx]) return;
        let freqArr = digits.map(d => parseInt(d.textContent));
        let carry = e.deltaY < 0 ? 1 : -1;
        let i = idx;
        while (carry !== 0 && i >= 0 && i < freqArr.length) {
            let newVal = freqArr[i] + carry;
            if (newVal > 9) {
                freqArr[i] = 0;
                carry = 1;
                i--;
            } else if (newVal < 0) {
                freqArr[i] = 9;
                carry = -1;
                i--;
            } else {
                freqArr[i] = newVal;
                carry = 0;
            }
        }
        let newFreq = parseInt(freqArr.join(''));
        newFreq = Math.max(30000, Math.min(75000000, newFreq));
        state.localFreq[receiver] = newFreq;
        updateFrequencyDisplay(receiver, newFreq);
        clearTimeout(display._debounceTimer);
        display._debounceTimer = setTimeout(() => {
            setFrequency(receiver, newFreq);
            state.localFreq[receiver] = null;
            state.editing[receiver] = false;
            updateFrequencyDisplay(receiver, state.lastBackendFreq[receiver]);
        }, 200);
        e.preventDefault();
    }, { passive: false });

    if (isTouchDevice() && controls) {
        if (upBtn) {
            upBtn.onclick = function (e) {
                e.preventDefault();
                changeSelectedDigit(receiver, 1);
            };
        }
        if (downBtn) {
            downBtn.onclick = function (e) {
                e.preventDefault();
                changeSelectedDigit(receiver, -1);
            };
        }
    }

    display.addEventListener('mouseleave', function () {
        if (state.editing[receiver]) {
            state.selectedIdx[receiver] = null;
            state.editing[receiver] = false;
            state.localFreq[receiver] = null;
            updateFrequencyDisplay(receiver, state.lastBackendFreq[receiver]);
        }
    });

    document.addEventListener('pointerdown', function (e) {
        if (!display.contains(e.target) && (!controls || !controls.contains(e.target))) {
            if (state.editing[receiver]) {
                state.selectedIdx[receiver] = null;
                state.editing[receiver] = false;
                state.localFreq[receiver] = null;
                updateFrequencyDisplay(receiver, state.lastBackendFreq[receiver]);
            }
        }
    });
}

// Outer band setter - called from Razor inline onchange on band buttons
window.setBand = async function (receiver, band) {
    try {
        if (window.highlightButtons) highlightButtons(receiver, band, state.lastMode ? state.lastMode[receiver] : undefined, state.lastAntenna ? state.lastAntenna[receiver] : undefined);
        if (state.lastBand) state.lastBand[receiver] = band;
        const response = await fetch(`/api/cat/band/${receiver.toLowerCase()}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ band })
        });
        // No debug logging
    } catch (error) {
        // No debug logging
    }
};

// Outer mode setter - called from Razor inline onchange on mode select
window.setMode = async function (receiver, mode) {
    const modeToCatCode = {
        "LSB": "1", "USB": "2", "CW-U": "3", "FM": "4", "AM": "5", "RTTY-L": "6", "CW-L": "7", "DATA-L": "8", "RTTY-U": "9", "DATA-FM": "A", "FM-N": "B", "DATA-U": "C", "AM-N": "D", "PSK": "E", "DATA-FM-N": "F"
    };
    const catCode = modeToCatCode[mode];
    if (!catCode) {
        return;
    }
    const response = await fetch(`/api/cat/mode/${receiver.toLowerCase()}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mode: catCode })
    });
    // No debug logging
};

// Outer antenna setter - called from Razor inline onchange on antenna buttons
window.setAntenna = async function (receiver, antenna) {
    if (window.pausePolling) pausePolling();
    try {
        if (window.highlightButtons) highlightButtons(receiver, state.lastBand ? state.lastBand[receiver] : undefined, state.lastMode ? state.lastMode[receiver] : undefined, antenna);
        if (state.lastAntenna) state.lastAntenna[receiver] = antenna;
        const response = await fetch(`/api/cat/antenna/${receiver.toLowerCase()}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ antenna })
        });
        // No debug logging
    } catch (error) {
        // No debug logging
    }
};

// Outer power slider max updater
function updatePowerSliderMax(maxPower) {
    const slider = document.getElementById('powerSlider');
    const labelMax = document.getElementById('powerMaxLabel');

    // Always enforce correct max for FTdx101D and FTdx101MP
    let actualMax = 200;
    if (window.state && window.state.radioModel) {
        const model = window.state.radioModel.toLowerCase();
        if (model === "ftdx101d") {
            actualMax = 100;
        } else if (model === "ftdx101mp") {
            actualMax = 200;
        } else if (typeof maxPower === "number") {
            actualMax = maxPower;
        }
    } else if (typeof maxPower === "number") {
        actualMax = maxPower;
    }
    // Always enforce correct max for FTdx101D
    if (window.state && window.state.radioModel && window.state.radioModel.toLowerCase() === "ftdx101d") {
        actualMax = 100;
    }
    if (slider) slider.max = actualMax;
    if (labelMax) labelMax.textContent = window.MeterFormatters.powerLabel(actualMax);
}

// TX state updater - updates TX button and meters
function updateTxIndicators(isTransmitting) {
    // Debug logging


    // Update state for meter display logic (if IIFE state is accessible)
    if (window.radioControl && window.radioControl._state) {
        window.radioControl._state.isTransmitting = isTransmitting;

    } else {

    }

    if (!isTransmitting) {
        // Zero the meters when not transmitting
        if (typeof window.updatePowerMeter === 'function') {
            window.updatePowerMeter(0);
        }
        if (window.meterPanel) window.meterPanel.update('power', 0);
        if (typeof window.updateSWRMeter === 'function') {
            window.updateSWRMeter(0);
        }
    }
}

// Outer power setter (stub - real version is inside the IIFE)
async function setPower(receiver, watts) {
    const power = parseInt(watts);
    try {

        const response = await fetch(`/api/cat/power/${receiver.toLowerCase()}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Watts: power })
        });
        if (!response.ok) {

        } else {

        }
        updatePowerDisplay(receiver, power);
    } catch (error) {

    }
}

// Placeholder - replaced by the IIFE's real implementation once it runs
window.updatePowerDisplay = function(receiver, watts) {
    // Find the power value display element
    const powerValue = document.getElementById('powerValue');
    if (powerValue) {
        powerValue.innerText = window.MeterFormatters.powerLabel(watts);
    }
};

// ---------------------------------------------------------------------------
// Radio Power On/Off Toggle
// ---------------------------------------------------------------------------
let radioPowerOn = true; // Track radio power state

async function toggleRadioPower() {
    const btn = document.getElementById('radioPowerBtn');
    if (!btn) return;

    // Disable button during operation
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> POWER';

    try {
        const newPowerState = !radioPowerOn;


        const response = await fetch('/api/cat/radiopower', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ powerOn: newPowerState })
        });

        if (response.ok) {
            const data = await response.json();
            radioPowerOn = data.powerOn;
            updateRadioPowerButton();

        } else {

        }
    } catch (error) {

    } finally {
        btn.disabled = false;
        updateRadioPowerButton();
    }
}

function updateRadioPowerButton() {
    const btn = document.getElementById('radioPowerBtn');
    if (!btn) return;

    if (radioPowerOn) {
        btn.className = 'btn btn-success btn-sm';
        btn.innerHTML = '<i class="bi bi-power"></i> POWER';
        btn.title = 'Radio is ON - Click to turn OFF';
    } else {
        btn.className = 'btn btn-danger btn-sm';
        btn.innerHTML = '<i class="bi bi-power"></i> POWER';
        btn.title = 'Radio is OFF - Click to turn ON';
    }
}

// Check radio power status on page load
async function checkRadioPowerStatus() {
    try {
        const response = await fetch('/api/cat/radiopower');
        if (response.ok) {
            const data = await response.json();
            radioPowerOn = data.powerOn;
            updateRadioPowerButton();
        }
    } catch (error) {

    }
}

// Initialize radio power button state on page load
document.addEventListener('DOMContentLoaded', function() {
    checkRadioPowerStatus();
    checkTxStatus();
    // Fetch radio status and update slider max
    fetch('/api/cat/status')
        .then(response => response.json())
        .then(data => {
            if (data && data.radioModel && window.state) {
                window.state.radioModel = data.radioModel;
                updatePowerSliderMax();
            }
        });

    // Update powerValue label live as slider moves (outer/global version)
    const slider = document.getElementById('powerSlider');
    const display = document.getElementById('powerValue');
    if (slider && display) {
        // Initialize label to slider value on page load
        display.textContent = window.MeterFormatters.powerLabel(slider.value);
        slider.addEventListener('input', function () {
            display.textContent = window.MeterFormatters.powerLabel(slider.value);
        });
    }
});

// ---------------------------------------------------------------------------
// TX Button Toggle
// ---------------------------------------------------------------------------
let isTransmitting = false;
let txVfo = 0; // 0 = VFO A, 1 = VFO B

async function toggleTx() {
    const newTxState = !isTransmitting;


    try {
        const response = await fetch('/api/cat/tx', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ transmit: newTxState })
        });

        if (response.ok) {
            const data = await response.json();
            isTransmitting = data.transmitting;
            updateTxButton();

        } else {

        }
    } catch (error) {

    }
}

function updateTxButton() {
    const btnA = document.getElementById('txButtonA');
    const btnB = document.getElementById('txButtonB');

    // Show only on TX VFO
    if (btnA) btnA.style.display = (txVfo === 0) ? 'inline-block' : 'none';
    if (btnB) btnB.style.display = (txVfo === 1) ? 'inline-block' : 'none';

    // Update button state
    const activeBtn = (txVfo === 0) ? btnA : btnB;
    if (activeBtn) {
        if (isTransmitting) {
            activeBtn.className = 'btn btn-danger btn-sm';
            activeBtn.innerHTML = '<i class="bi bi-broadcast"></i> TX ON';
            activeBtn.title = 'Click to stop transmitting';
        } else {
            activeBtn.className = 'btn btn-warning btn-sm';
            activeBtn.innerHTML = '<i class="bi bi-broadcast"></i> TX';
            activeBtn.title = 'Click to transmit';
        }
    }
}

async function checkTxStatus() {
    try {
        const response = await fetch('/api/cat/tx');
        if (response.ok) {
            const data = await response.json();
            isTransmitting = data.transmitting;
            txVfo = data.txVfo;
            updateTxButton();
        }
    } catch (error) {

    }
}

// ---------------------------------------------------------------------------
// SignalR connection - shared by both the outer handler below and the
// second handler at the bottom of the file (after the IIFE).
// ---------------------------------------------------------------------------
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/radioHub")
    .build();

// Redirect to Settings page if the backend signals an init failure
connection.on("ShowSettingsPage", function () {
    window.location.href = "/Settings";
});




function sMeterLabel(val) {
    return window.calibrationEngine.calibrateSMeterLabel(val);
}


// ---------------------------------------------------------------------------
// BUG FIX: updateModeSelect
// ---------------------------------------------------------------------------
// Updates the mode dropdown select when the mode changes from the radio
// (e.g., via SignalR update or front panel knob change).
// ---------------------------------------------------------------------------
function updateModeSelect(receiver, mode) {
    const select = document.getElementById(`modeSelect${receiver}`);
    if (select) {
        select.value = mode;
    } else {

    }
}

// ---------------------------------------------------------------------------
// updateMicGainLabel
// ---------------------------------------------------------------------------
// Updates the MIC Gain label based on the current mode.
// In DATA modes (DATA-U, DATA-L, PSK, DATA-FM, etc.), this controls Data Out level.
// In voice modes (SSB, AM, FM, etc.), this controls MIC Gain.
// ---------------------------------------------------------------------------
function updateMicGainLabel(mode) {
    const label = document.getElementById('micGainLabel');
    if (!label) return;

    // Data modes where "MIC Gain" actually controls Data Out level
    const dataModes = ['DATA-U', 'DATA-L', 'PSK', 'DATA-FM', 'DATA-FM-N', 'RTTY-U', 'RTTY-L'];

    if (dataModes.includes(mode)) {
        label.textContent = 'Data Out Gain';
    } else {
        label.textContent = 'MIC Gain';
    }
}

// First SignalR RadioStateUpdate handler (outer scope).
// Handles ModeA/B, FrequencyA/B, PowerA/B updates pushed from the backend.
connection.on("RadioStateUpdate", function (update) {
    // ...removed debug logging...

    // --- MODE CHANGE (THE BUG FIX) ---
    if (update.property === "ModeA") {
        updateModeSelect('A', update.value);
        updateMicGainLabel(update.value);
    }
    if (update.property === "ModeB") {
        updateModeSelect('B', update.value);
    }

    // --- FREQUENCY CHANGE ---
    if (update.property === "FrequencyA") {
        state.lastBackendFreq.A = update.value;
        updateFrequencyDisplay('A', update.value);
    }
    if (update.property === "FrequencyB") {
        state.lastBackendFreq.B = update.value;
        updateFrequencyDisplay('B', update.value);
    }

    // --- BAND CHANGE ---
    if (update.property === "BandA") {
        // ...removed debug logging...
        updateBandButton('A', update.value);
    }
    if (update.property === "BandB") {
        // ...removed debug logging...
        updateBandButton('B', update.value);
    }

    // --- POWER CHANGE ---
    // Only handle generic Power (no A/B distinction)
    if (update.property === "PowerA") {
        if (typeof window.updatePowerDisplay === 'function') window.updatePowerDisplay("A", update.value);
        const sliderA = document.getElementById('powerSliderA');
        if (sliderA) sliderA.value = update.value;
    }
    if (update.property === "PowerB") {
        if (typeof window.updatePowerDisplay === 'function') window.updatePowerDisplay("B", update.value);
        const sliderB = document.getElementById('powerSliderB');
        if (sliderB) sliderB.value = update.value;
    }
    if (update.property === "Power") {
        if (typeof window.updatePowerDisplay === 'function') window.updatePowerDisplay("A", update.value);
        const sliderA = document.getElementById('powerSliderA');
        if (sliderA) sliderA.value = update.value;
    }

    // --- RADIO POWER STATE ---
    if (update.property === "RadioPowerOn") {
        radioPowerOn = update.value;
        updateRadioPowerButton();
    }

    // --- TX STATE ---
    if (update.property === "IsTransmitting") {
        isTransmitting = update.value;
        // Always update the IIFE's state for correct gauge behavior
        if (window.radioControl && window.radioControl._state) {
            window.radioControl._state.isTransmitting = update.value;
            // ...removed debug logging...
        } else {
            // ...removed debug logging...
        }
        updateTxButton();
        updateTxIndicators(update.value);
    }
    if (update.property === "TxVfo") {
        txVfo = update.value;
        updateTxButton();
    }

    // --- METER UPDATES ---
    if (update.property === "PowerMeter" && typeof window.updatePowerMeter === 'function') {
        // Support new format: update.value = { value, isTransmitting }
        let powerValue = update.value;
        let txState = isTransmitting;
        if (typeof update.value === 'object' && update.value !== null && 'value' in update.value && 'isTransmitting' in update.value) {
            powerValue = update.value.value;
            txState = update.value.isTransmitting;
        }
        // Debug: log incoming PowerMeter update
        // ...removed debug logging...
        // Always sync the IIFE's state and global state
        if (window.radioControl && window.radioControl._state) {
            window.radioControl._state.isTransmitting = txState;
        }
        if (typeof state !== 'undefined') {
            state.isTransmitting = txState;
        }
        // Zero the meter if not transmitting, as in old logic
        if (!txState) {
            window.updatePowerMeter(0);
        } else {
            window.updatePowerMeter(powerValue);
        }
    }
    if (update.property === "SWRMeter" && typeof window.updateSWRMeter === 'function') {
        window.updateSWRMeter(update.value);
    }
    if (update.property === "CompressionMeter" && typeof window.updateCompressionMeter === 'function') {
        window.updateCompressionMeter(update.value);
    }
    if (update.property === "ALCMeter" && typeof window.updateALCMeter === 'function') {
        window.updateALCMeter(update.value);
    }
    if (update.property === "IDDMeter" && typeof window.updateIDDMeter === 'function') {
        window.updateIDDMeter(update.value);
    }
    if (update.property === "VDDMeter" && typeof window.updatePAVoltage === 'function') {
        window.updatePAVoltage(update.value);
    }
    if (update.property === "Temperature") {
        // ...removed debug logging...
        if (typeof window.updatePATemperature === 'function') {
            window.updatePATemperature(update.value);
        }
    }

    // --- ROOFING FILTER ---
    if (update.property === "RoofingFilterA") {
        const selectEl = document.getElementById('roofingFilterSelectA');
        if (selectEl) selectEl.value = update.value;
    }
    if (update.property === "RoofingFilterB") {
        const selectEl = document.getElementById('roofingFilterSelectB');
        if (selectEl) selectEl.value = update.value;
    }

    // --- AF GAIN ---
    if (update.property === "AfGainA" || update.property === "AfGainB") {
        const receiver = update.property === "AfGainA" ? 'A' : 'B';
        const slider = document.getElementById(`afGainSlider${receiver}`);
        if (slider) {
            afGainLastConfirmed[receiver] = update.value;
            if (
                afGainPendingValue[receiver] !== null &&
                Math.abs(Number(update.value) - Number(afGainPendingValue[receiver])) <= 2
            ) {
                slider.value = update.value;
                slider.classList.remove('pending');
                afGainPendingValue[receiver] = null;
                if (afGainPendingTimer[receiver]) clearTimeout(afGainPendingTimer[receiver]);
            } else if (afGainPendingValue[receiver] === null && !afGainDragging[receiver]) {
                slider.value = update.value;
            }
        }
    }
});

// SignalR connection is started once below (after the IIFE) with a .catch() error handler.

// ---------------------------------------------------------------------------
// Initialization overlay polling
// Polls /api/status/init every second until status is "complete", "radio_off", or "error".
// On error, redirects to /Settings ONLY if user hasn't dismissed the overlay.
// On radio_off, stays on Index page so user can turn radio on via power button.
// ---------------------------------------------------------------------------
let initPollingStopped = false; // Allow user to dismiss and continue

async function pollInitStatus() {
    if (initPollingStopped) return; // User dismissed, stop polling

    try {
        const response = await fetch('/api/status/init');
        if (!response.ok) return;
        const data = await response.json();
        const overlay = document.getElementById('initOverlay');
        const statusText = document.getElementById('initStatusText');
        if (!overlay || !statusText) return;

        statusText.innerText = data.status;

        if (data.status === "complete") {
            overlay.style.display = "none";
            initPollingStopped = true; // Stop polling
            radioPowerOn = true;
            updateRadioPowerButton();
        } else if (data.status === "radio_off") {
            // Radio is off - hide overlay and let user turn it on via power button
            overlay.style.display = "none";
            initPollingStopped = true;
            radioPowerOn = false;
            updateRadioPowerButton();
            // ...removed debug logging...
        } else if (data.status === "error") {
            statusText.innerHTML = "COM port error. <a href='/Settings' class='text-white'>Go to Settings</a> to configure the serial port.";
            overlay.style.display = "block";
            // Don't auto-redirect - let user choose
        } else {
            overlay.style.display = "block";
        }

        if (data.status !== "complete" && data.status !== "radio_off" && !initPollingStopped) {
            setTimeout(pollInitStatus, 1000);
        }
    } catch (error) {
        // ...removed debug logging...
        if (!initPollingStopped) {
            setTimeout(pollInitStatus, 2000);
        }
    }
}

function dismissInitOverlay() {
    initPollingStopped = true;
    const overlay = document.getElementById('initOverlay');
    if (overlay) overlay.style.display = "none";
}

// Touch device detection helper
function isTouchDevice() {
    return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
}

// Interim radioControl - overwritten by the Iife below once it executes
window.radioControl = {
    setBand: window.setBand,
    setMode: window.setMode,
    setAntenna: window.setAntenna,
    setPower: window.setPower,
    updatePowerDisplay: window.updatePowerDisplay
};

// Fetch and apply band button state from the backend on page load
async function updateBandButtonsFromBackend() {
    try {
        const response = await fetch('/api/cat/status');
        if (!response.ok) return;
        const data = await response.json();
        // Update global radioModel if present
        if (data.radioModel) {
            state.radioModel = data.radioModel;
            // Always call updatePowerSliderMax to use latest radioModel
            updatePowerSliderMax();
        }
        if (data.vfoA && data.vfoA.band) {
            document.querySelectorAll('input[name="band-A"]').forEach(radio => {
                radio.checked = (radio.value.toLowerCase() === data.vfoA.band.toLowerCase());
            });
        }
        if (data.vfoB && data.vfoB.band) {
            document.querySelectorAll('input[name="band-B"]').forEach(radio => {
                radio.checked = (radio.value.toLowerCase() === data.vfoB.band.toLowerCase());
            });
        }
    } catch (error) {
        // ...removed debug logging...
    }
}


// Update band button selection for a specific receiver (called via SignalR)
function updateBandButton(receiver, band) {
    // ...removed debug logging...
    if (!band) {
        // ...removed debug logging...
        return;
    }
    const bandLower = band.toLowerCase();
    const inputs = document.querySelectorAll(`input[name="band-${receiver}"]`);
    // ...removed debug logging...

    let foundMatch = false;
    inputs.forEach(radio => {
        const matches = (radio.value.toLowerCase() === bandLower);
        if (matches) {
            foundMatch = true;
            // ...removed debug logging...
        }
        radio.checked = matches;
    });

    if (!foundMatch) {
        // ...removed debug logging...
    }
    // ...removed debug logging...
}

// Outer DOMContentLoaded - initial UI wiring
window.addEventListener('DOMContentLoaded', () => {
    pollInitStatus();
        updateBandButtonsFromBackend();

    // Event delegation for band button changes
    document.addEventListener('change', function(e) {
        if (e.target.type === 'radio' && e.target.name && e.target.name.startsWith('band-')) {
            const receiver = e.target.getAttribute('data-receiver');
            const band = e.target.value;
            if (receiver && band && window.radioControl && window.radioControl.setBand) {
                window.radioControl.setBand(receiver, band);
            }
        }
    });
});

// Touch up/down button handler for mobile frequency editing
function changeSelectedDigit(receiver, delta) {
    const display = document.getElementById('freq' + receiver);
    let digits = Array.from(display.querySelectorAll('.digit')).filter(d => d.textContent !== '.');
    let idx = state.selectedIdx[receiver];
    if (idx === null || !digits[idx]) return;
    let freqArr = digits.map(d => parseInt(d.textContent));
    let newVal = freqArr[idx] + delta;
    if (newVal > 9) newVal = 0;
    if (newVal < 0) newVal = 9;
    freqArr[idx] = newVal;
    let newFreq = parseInt(freqArr.join(''));
    newFreq = Math.max(30000, Math.min(75000000, newFreq));
    state.localFreq[receiver] = newFreq;
    updateFrequencyDisplay(receiver, newFreq);
    const displayElem = document.getElementById('freq' + receiver);
    clearTimeout(displayElem._debounceTimer);
    displayElem._debounceTimer = setTimeout(() => {
        setFrequency(receiver, newFreq);
    }, 200);
}

// ===========================================================================
// IIFE - Full authoritative implementation
// ===========================================================================
// Everything inside this block is the "real" app logic. It defines its own
// inner state, all the polling/display/gauge functions, and at the end
// overwrites window.radioControl so Razor inline handlers call these
// better-implemented versions.
// ===========================================================================
// --- AF Gain slider change handler ---
// Professional AF Gain handler: sets pending state, updates only on backend confirmation
// --- AF Gain slider change handler with smooth UX ---
// Track user interaction state
const afGainDragging = { A: false, B: false };
const afGainPendingValue = { A: null, B: null };
const afGainPendingTimer = { A: null, B: null };
const afGainLastConfirmed = { A: null, B: null };

function sendAfGain(receiver, value) {
    const slider = document.getElementById(`afGainSlider${receiver}`);
    if (!slider) return;
    // Optimistic UI: move instantly, show pending
    slider.value = value;
    slider.classList.add('pending');
    afGainPendingValue[receiver] = value;
    // Start/clear timeout for backend confirmation
    if (afGainPendingTimer[receiver]) {
        clearTimeout(afGainPendingTimer[receiver]);
    }
    afGainPendingTimer[receiver] = setTimeout(() => {
        // Timeout: revert to last confirmed value and show error
        if (afGainPendingValue[receiver] !== null) {
            slider.value = afGainLastConfirmed[receiver] !== null ? afGainLastConfirmed[receiver] : slider.value;
            slider.classList.remove('pending');
            afGainPendingValue[receiver] = null;
            alert('AF Gain not confirmed by radio. Reverted.');
        }
    }, 3500); // 3.5 seconds
    fetch(`/api/cat/afgain/${receiver.toLowerCase()}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(value)
    }).then(r => {
        if (!r.ok) {
            // ...removed debug logging...
            slider.classList.remove('pending');
            afGainPendingValue[receiver] = null;
            if (afGainPendingTimer[receiver]) clearTimeout(afGainPendingTimer[receiver]);
        }
        // On success, wait for backend confirmation via SignalR
    }).catch(e => {
        // ...removed debug logging...
        slider.classList.remove('pending');
        afGainPendingValue[receiver] = null;
        if (afGainPendingTimer[receiver]) clearTimeout(afGainPendingTimer[receiver]);
    });
}

// Attach event listeners to track dragging
function setupAfGainSlider(receiver) {
    const slider = document.getElementById(`afGainSlider${receiver}`);
    if (!slider) return;
    slider.addEventListener('mousedown', () => { afGainDragging[receiver] = true; });
    slider.addEventListener('touchstart', () => { afGainDragging[receiver] = true; });
    // Only send value on release
    slider.addEventListener('mouseup', () => {
        afGainDragging[receiver] = false;
        sendAfGain(receiver, slider.value);
    });
    slider.addEventListener('touchend', () => {
        afGainDragging[receiver] = false;
        sendAfGain(receiver, slider.value);
    });
    slider.addEventListener('mouseleave', () => { afGainDragging[receiver] = false; });
    // Prevent sending on every input/change
    slider.addEventListener('input', () => {
        // Do nothing here; only send on release
    });
}

// Call setupAfGainSlider for both receivers on DOMContentLoaded
document.addEventListener('DOMContentLoaded', function() {
    setupAfGainSlider('A');
    setupAfGainSlider('B');
});


(function () {
    'use strict';

    // ...removed debug logging...


    // Full inner state object - this is the authoritative state for the app
    const state = {
        editing: { A: false, B: false },
        editingPower: { A: false, B: false },
        localFreq: { A: null, B: null },
        selectedIdx: { A: null, B: null },
        lastSentFreq: { A: null, B: null },
        lastBackendFreq: { A: null, B: null },
        lastBand: { A: null, B: null },
        lastMode: { A: null, B: null },
        lastAntenna: { A: null, B: null },
        lastPower: { A: 100, B: 100 },
        maxPower: 200,
        radioModel: 'FTdx101MP',
        pollingInterval: null,
        operationInProgress: false,
        isTransmitting: false  // Track TX state for meter display
    };

    function renderFrequencyDigits(freq, selIdx) {
        if (!freq || freq < 1000) {
            return '<span class="digit">-</span><span class="digit">-</span>.<span class="digit">-</span><span class="digit">-</span><span class="digit">-</span>.<span class="digit">-</span><span class="digit">-</span><span class="digit">-</span>';
        }
        let s = freq.toString().padStart(8, "0");
        let html = "";
        let digitIdx = 0;
        for (let i = 0; i < 8; i++) {
            if (i === 2 || i === 5) {
                html += '<span class="digit">.</span>';
            }
            let selected = (selIdx === digitIdx) ? " selected" : "";
            html += `<span class="digit${selected}" tabindex="0">${s[i]}</span>`;
            digitIdx++;
        }
        return html;
    }

    function updateFrequencyDisplay(receiver, freqHz) {
        const display = document.getElementById('freq' + receiver);
        if (!display) {
            // ...removed debug logging...
            return;
        }
        let selIdx = state.selectedIdx[receiver];
        let freqToShow = (state.editing[receiver] && state.localFreq[receiver] !== null)
            ? state.localFreq[receiver]
            : freqHz;
        display.innerHTML = renderFrequencyDigits(freqToShow, selIdx);
    }

    // Update band, mode, and antenna radio/toggle buttons to reflect current state.
    // NOTE: The Razor page renders mode buttons as <input type="radio" name="modeA" value="USB">
    // and band/antenna buttons similarly.  We update .checked directly.
    function highlightButtons(receiver, band, mode, antenna) {
        // Band buttons (rendered by _BandButtonsPartial as input[name="band-A/B"])
        document.querySelectorAll(`input[name="band-${receiver}"]`).forEach(btn => {
            btn.checked = (btn.value === band);
        });

        // Mode dropdown - update the selected value
        const modeSelect = document.getElementById(`modeSelect${receiver}`);
        if (modeSelect && mode) {
            modeSelect.value = mode;
        }

        // Antenna buttons
        document.querySelectorAll(`input[name="antenna${receiver}"]`).forEach(btn => {
            btn.checked = (btn.value === antenna);
        });
    }

    // Update ONLY mode and antenna buttons (not bands) - used by polling to avoid overwriting user's band selection
    function updateModeAndAntennaButtons(receiver, mode, antenna) {
        // Mode dropdown
        const modeSelect = document.getElementById(`modeSelect${receiver}`);
        if (modeSelect && mode) {
            modeSelect.value = mode;
        }

        // Antenna buttons
        document.querySelectorAll(`input[name="antenna${receiver}"]`).forEach(btn => {
            btn.checked = (btn.value === antenna);
        });
    }

    // Update roofing filter dropdown
    function updateRoofingFilterSelect(receiver, filterCode) {
        const selectEl = document.getElementById(`roofingFilterSelect${receiver}`);
        if (selectEl && filterCode) {
            selectEl.value = filterCode;
        }
    }

    function initializeDigitInteraction(receiver) {
        const display = document.getElementById('freq' + receiver);
        if (!display) {
            // ...removed debug logging...
            return;
        }
        if (display._initialized) return;
        display._initialized = true;

        display.addEventListener('click', function (e) {
            if (!e.target.classList.contains('digit') || e.target.textContent === '.') return;
            let digits = Array.from(display.querySelectorAll('.digit')).filter(d => d.textContent !== '.');
            digits.forEach(d => d.classList.remove('selected'));
            state.selectedIdx[receiver] = digits.indexOf(e.target);
            if (state.selectedIdx[receiver] !== -1) {
                digits[state.selectedIdx[receiver]].classList.add('selected');
                state.editing[receiver] = true;
                state.localFreq[receiver] = parseInt(digits.map(d => d.textContent).join(''));
            }
        });

        display.addEventListener('wheel', function (e) {
            let digits = Array.from(display.querySelectorAll('.digit')).filter(d => d.textContent !== '.');
            let idx = state.selectedIdx[receiver];
            if (idx === null || !digits[idx]) return;
            let freqArr = digits.map(d => parseInt(d.textContent));
            let carry = e.deltaY < 0 ? 1 : -1;
            let i = idx;
            while (carry !== 0 && i >= 0 && i < freqArr.length) {
                let newVal = freqArr[i] + carry;
                if (newVal > 9) {
                    freqArr[i] = 0;
                    carry = 1;
                    i--;
                } else if (newVal < 0) {
                    freqArr[i] = 9;
                    carry = -1;
                    i--;
                } else {
                    freqArr[i] = newVal;
                    carry = 0;
                }
            }
            let newFreq = parseInt(freqArr.join(''));
            newFreq = Math.max(30000, Math.min(70000000, newFreq));
            state.localFreq[receiver] = newFreq;
            updateFrequencyDisplay(receiver, newFreq);
            let newDigits = Array.from(display.querySelectorAll('.digit')).filter(d => d.textContent !== '.');
            newDigits.forEach(d => d.classList.remove('selected'));
            if (state.selectedIdx[receiver] !== null && newDigits[state.selectedIdx[receiver]]) {
                newDigits[state.selectedIdx[receiver]].classList.add('selected');
            }
            clearTimeout(display._debounceTimer);
            display._debounceTimer = setTimeout(() => {
                setFrequency(receiver, newFreq);
                state.lastSentFreq[receiver] = newFreq;
                state.localFreq[receiver] = null;
            }, 600);
            e.preventDefault();
        }, { passive: false });

        document.addEventListener('click', function (e) {
            if (!display.contains(e.target)) {
                state.selectedIdx[receiver] = null;
                state.editing[receiver] = false;
                state.localFreq[receiver] = null;
                updateFrequencyDisplay(receiver, state.lastBackendFreq[receiver]);
            }
        });
    }

    async function setFrequency(receiver, freqHz) {
        try {
            const response = await fetch(`/api/cat/frequency/${receiver.toLowerCase()}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ frequencyHz: freqHz })
            });
            updateFrequencyDisplay(receiver, freqHz);
        } catch (error) {
        }
    }

    async function setBand(receiver, band) {
        const didPause = pausePolling();
        try {
            highlightButtons(receiver, band, state.lastMode[receiver], state.lastAntenna[receiver]);
            state.lastBand[receiver] = band;
            const response = await fetch(`/api/cat/band/${receiver.toLowerCase()}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ band })
            });
        } catch (error) {
        } finally {
            if (didPause) {
                resumePolling();
            }
        }
    }

    async function setMode(receiver, mode) {
        const catCode = modeToCatCode[mode];
        if (!catCode) {
            return;
        }
        const response = await fetch(`/api/cat/mode/${receiver.toLowerCase()}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ mode: catCode })
        });
    }

    async function setAntenna(receiver, antenna) {
        const didPause = pausePolling();
        try {
            highlightButtons(receiver, state.lastBand[receiver], state.lastMode[receiver], antenna);
            state.lastAntenna[receiver] = antenna;
            const response = await fetch(`/api/cat/antenna/${receiver.toLowerCase()}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ antenna })
            });
        } catch (error) {
        } finally {
            if (didPause) {
                resumePolling();
            }
        }
    }

    // Show Windows-style message box (auto-dismisses after 3 seconds)
    function showMessageBox(message, title = 'Warning') {
        const modalEl = document.getElementById('messageBoxModal');
        const titleEl = document.getElementById('messageBoxTitle');
        const textEl = document.getElementById('messageBoxText');

        if (modalEl && titleEl && textEl) {
            titleEl.innerHTML = `<i class="bi bi-exclamation-triangle-fill me-2"></i>${title}`;
            textEl.textContent = message;
            const modal = new bootstrap.Modal(modalEl);
            modal.show();

            // Auto-dismiss after 3 seconds
            setTimeout(() => {
                modal.hide();
            }, 3000);
        } else {
            // Fallback to alert if modal not found
            alert(message);
        }
    }

    async function setRoofingFilter(receiver, filter) {
        const didPause = pausePolling();

        try {
            const response = await fetch(`/api/cat/roofingfilter/${receiver.toLowerCase()}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ filter })
            });

            const data = await response.json();

            if (!response.ok) {
                showMessageBox(`Failed to set roofing filter: ${data.error}`, 'Error');
                return;
            }

            // Check if there's a warning (filter not installed)
            if (data.warning) {
                showMessageBox(data.message, 'Roofing Filter');
                // Update dropdown to show actual filter
                const selectEl = document.getElementById(`roofingFilterSelect${receiver}`);
                if (selectEl && data.filter) {
                    selectEl.value = data.filter;
                }
            }
        } catch (error) {
            showMessageBox('Error setting roofing filter. Check console for details.', 'Error');
        } finally {
            if (didPause) {
                resumePolling();
            }
        }
    }

    function pausePolling() {
        if (state.pollingInterval && !state.operationInProgress) {
            state.operationInProgress = true;
            return true;
        }
        return false;
    }

    function resumePolling() {
        if (state.operationInProgress) {
            state.operationInProgress = false;
            setTimeout(fetchRadioStatus, 500);
        }
    }

    // Full status poll - updates frequencies, S-meter, band/mode/antenna buttons, and power
    async function fetchRadioStatus() {
        if (state.operationInProgress) {
            return;
        }
        try {
            const response = await fetch('/api/cat/status');
            if (!response.ok) {
                return;
            }
            const data = await response.json();

            if (data.radioModel !== undefined) {
                state.radioModel = data.radioModel;
            }
            // Always call updatePowerSliderMax to use latest radioModel
            if (state.radioModel) {
                const model = state.radioModel.toLowerCase();
                if (model === "ftdx101d") {
                    state.maxPower = 100;
                } else if (model === "ftdx101mp") {
                    state.maxPower = 200;
                } else {
                    const maxPower = (data.maxPower !== undefined) ? data.maxPower : 200;
                    state.maxPower = maxPower;
                }
            } else {
                const maxPower = (data.maxPower !== undefined) ? data.maxPower : 200;
                state.maxPower = maxPower;
            }
            updatePowerSliderMax();

            state.lastBackendFreq.A = data.vfoA.frequency;
            state.lastBackendFreq.B = data.vfoB.frequency;
            state.lastMode.A = data.vfoA.mode;
            state.lastMode.B = data.vfoB.mode;
            state.lastAntenna.A = data.vfoA.antenna;
            state.lastAntenna.B = data.vfoB.antenna;

            // Show set power value (not meter reading) when not transmitting
            let powerValue = 100;
            if (data.vfoA && data.vfoA.power !== undefined) {
                powerValue = data.vfoA.power;
                state.lastPower.A = data.vfoA.power;
            } else if (state.lastPower && typeof state.lastPower === 'object' && state.lastPower.A !== undefined) {
                powerValue = state.lastPower.A;
            }
            updatePowerSlider(null, powerValue);
            // TX meter (updatePowerMeter) will use RM5 during transmit only

            // Stop showing local frequency once backend confirms our sent value
            if (state.editing.A && state.lastSentFreq.A !== null && state.localFreq.A === null && data.vfoA.frequency === state.lastSentFreq.A) {
                state.editing.A = false;
                state.selectedIdx.A = null;
            }
            if (state.editing.B && state.lastSentFreq.B !== null && state.localFreq.B === null && data.vfoB.frequency === state.lastSentFreq.B) {
                state.editing.B = false;
                state.selectedIdx.B = null;
            }

            if (!state.editing.A) updateFrequencyDisplay('A', data.vfoA.frequency);
            else updateFrequencyDisplay('A', state.localFreq.A);

            if (!state.editing.B) updateFrequencyDisplay('B', data.vfoB.frequency);
            else updateFrequencyDisplay('B', state.localFreq.B);

            updateSMeter('A', data.vfoA.sMeter);
            updateSMeter('B', data.vfoB.sMeter);

            if (typeof window.updatePowerMeter === 'function' && data.powerMeter !== undefined) {
                window.updatePowerMeter(data.powerMeter);
            }
            if (typeof window.updateSWRMeter === 'function' && data.swrMeter !== undefined) {
                window.updateSWRMeter(data.swrMeter);
            }
            if (typeof window.updateCompressionMeter === 'function' && data.compressionMeter !== undefined) {
                window.updateCompressionMeter(data.compressionMeter);
            }
            if (typeof window.updateALCMeter === 'function' && data.alcMeter !== undefined) {
                window.updateALCMeter(data.alcMeter);
            }
            if (typeof window.updateIDDMeter === 'function' && data.iddMeter !== undefined) {
                window.updateIDDMeter(data.iddMeter);
            }
            if (typeof window.updatePAVoltage === 'function' && data.vddMeter !== undefined) {
                window.updatePAVoltage(data.vddMeter);
            }
            if (typeof window.updatePATemperature === 'function' && data.temperature !== undefined) {
                window.updatePATemperature(data.temperature);
            }

            // Update band buttons from polling (fixes WSJT-X and radio band changes)
            if (data.vfoA.band) {
                updateBandButton('A', data.vfoA.band);
                state.lastBand.A = data.vfoA.band;
            }
            if (data.vfoB.band) {
                updateBandButton('B', data.vfoB.band);
                state.lastBand.B = data.vfoB.band;
            }

            // Update mode and antenna buttons from polling
            updateModeAndAntennaButtons('A', data.vfoA.mode, data.vfoA.antenna);
            updateModeAndAntennaButtons('B', data.vfoB.mode, data.vfoB.antenna);

            // Update roofing filter dropdowns
            if (data.vfoA.roofingFilter) {
                updateRoofingFilterSelect('A', data.vfoA.roofingFilter);
            }
            if (data.vfoB.roofingFilter) {
                updateRoofingFilterSelect('B', data.vfoB.roofingFilter);
            }

            // Update MIC Gain / Data Out Gain label based on current mode (VFO A is main)
            updateMicGainLabel(data.vfoA.mode);

            // --- Robust AF Gain slider enable/attach ---
            const sliderA = document.getElementById('afGainSliderA');
            const labelA = document.getElementById('afGainValueA');
            const sliderB = document.getElementById('afGainSliderB');
            const labelB = document.getElementById('afGainValueB');
            // Use backend value or fallback to 0 (handle null, undefined, NaN, or missing)
            let afGainA = 0;
            let afGainB = 0;
            if (data.vfoA && typeof data.vfoA.afGain === 'number' && !isNaN(data.vfoA.afGain)) {
                afGainA = data.vfoA.afGain;
            }
            if (data.vfoB && typeof data.vfoB.afGain === 'number' && !isNaN(data.vfoB.afGain)) {
                afGainB = data.vfoB.afGain;
            }
            if (sliderA && labelA) {
                sliderA.value = afGainA;
                labelA.innerText = afGainA;
                updateAfGainFill('afGainSliderA');
                sliderA.disabled = false;
            }
            if (sliderB && labelB) {
                sliderB.value = afGainB;
                labelB.innerText = afGainB;
                updateAfGainFill('afGainSliderB');
                sliderB.disabled = false;
            }
            attachAfGainSliderListeners();

        } catch (error) {
        }
    }

    // Power display helpers
    function updateSliderFill(slider) {
        const min = parseFloat(slider.min) || 0;
        const max = parseFloat(slider.max) || 100;
        const val = parseFloat(slider.value) || 0;
        const pct = ((val - min) / (max - min)) * 100;
        slider.style.setProperty('--fill-pct', pct + '%');
    }

    function updatePowerDisplay(receiver, watts) {
        // Only one power control supported
        // Only update the label from the slider value, never from backend
        const display = document.getElementById('powerValue');
        const slider = document.getElementById('powerSlider');
        if (display && slider) {
            display.textContent = window.MeterFormatters.powerLabel(slider.value);
        }
    }

    async function setPower(receiver, watts) {
        try {
            // Ensure state.lastPower is an object
            if (typeof state.lastPower !== 'object' || state.lastPower === null) {
                state.lastPower = {};
            }
            state.lastPower[receiver] = parseInt(watts);
            const response = await fetch(`/api/cat/power/${receiver.toLowerCase()}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Watts: parseInt(watts) })
            });
            updatePowerDisplay(receiver, watts);
        } catch (error) {
        }
    }

    function updatePowerSlider(receiver, watts) {
        // No-op: backend never updates the slider. User only.
    }

    function updatePowerSliderMax(maxPower) {
        // Enforce correct min/max for FTdx101D and FTdx101MP
        const slider = document.getElementById('powerSlider');
        const labelMax = document.getElementById('powerMaxLabel');
        let actualMax = 200;
        let actualMin = 5;
        if (state.radioModel) {
            const model = state.radioModel.toLowerCase();
            if (model === "ftdx101d") {
                actualMax = 100;
            } else if (model === "ftdx101mp") {
                actualMax = 200;
            } else if (typeof maxPower === "number") {
                actualMax = maxPower;
            }
        } else if (typeof maxPower === "number") {
            actualMax = maxPower;
        }
        if (slider) {
            slider.max = actualMax;
            slider.min = actualMin;
            updateSliderFill(slider);
        }
        if (labelMax) labelMax.textContent = window.MeterFormatters.powerLabel(actualMax);
    }

    function updateSMeter(receiver, value) {
        if (receiver === 'A') {
            if (window.meterPanel) window.meterPanel.update('smeterA', value);
            updateRawSMeterValueA(value);
        } else if (receiver === 'B') {
            if (window.meterPanel) window.meterPanel.update('smeterB', value);
        }
    }

    // Smoothing buffers for meters (reduce jumpiness)
    let powerHistory = [];
let swrHistory = [];
const historyLength = 7; // Average last 7 readings for smoother display
let wasTransmittingPower = false;
let wasTransmittingSWR = false;

    function updatePowerMeter(value) {
        // Store last raw value globally for calibration reloads
        window.lastPowerMeterRawValue = value;
        // Robust TX state detection: check both global and IIFE state
        let tx = false;
        if (typeof state !== 'undefined' && typeof state.isTransmitting !== 'undefined') {
            tx = state.isTransmitting;
        }
        if (window.radioControl && window.radioControl._state && typeof window.radioControl._state.isTransmitting !== 'undefined') {
            tx = tx || window.radioControl._state.isTransmitting;
        }
        // Always enforce: if not transmitting, meter is zero and does not animate, regardless of incoming value
        if (!tx) {
            value = 0;
        }
        if (!tx) {
            powerHistory = [];
            wasTransmittingPower = false;
            const valueSpan = document.getElementById('powerMeterValue');
            if (valueSpan) valueSpan.textContent = '0';
            // Update raw Power Out label to 0 with descriptive label
            var rawPowerLabel = document.getElementById('raw-powerout-label');
            if (rawPowerLabel) rawPowerLabel.textContent = 'Raw Power Out: 0';
            if (window.meterPanel) window.meterPanel.update('power', 0);
            return;
        }
        // Only update on TX
        if (!wasTransmittingPower) {
            powerHistory = [];
        }
        wasTransmittingPower = true;
        powerHistory.push(value);
        if (powerHistory.length > historyLength) {
            powerHistory.shift();
        }
        const avgValue = powerHistory.reduce((sum, v) => sum + v, 0) / powerHistory.length;
        let watts = window.calibrationEngine.calibrateNumeric("PWR", avgValue);
        let clampedWatts = Math.round(Math.max(0, Math.min(watts, 200)));
        const valueSpan = document.getElementById('powerMeterValue');
        if (valueSpan) valueSpan.textContent = window.MeterFormatters.powerOverlay(clampedWatts);
        // Update raw Power Out label with descriptive label
        var rawPowerLabel = document.getElementById('raw-powerout-label');
        if (rawPowerLabel) rawPowerLabel.textContent = 'Raw Power Out: ' + Math.round(avgValue);
        if (window.meterPanel) window.meterPanel.update('power', clampedWatts);
    }

    function updateSWRMeter(value) {
        window.lastSWRMeterRawValue = value;
        // Always enforce: if not transmitting, meter is zero and does not animate, regardless of incoming value
        if (!state.isTransmitting) {
            swrHistory = [];
            wasTransmittingSWR = false;
            const valueSpan = document.getElementById('swrMeterValue');
            if (valueSpan) valueSpan.textContent = '1.0:1';
            if (window.meterPanel) window.meterPanel.update('swr', 0);
            return;
        }
        if (!wasTransmittingSWR) {
            swrHistory = [];
        }
        wasTransmittingSWR = true;
        swrHistory.push(value);
        if (swrHistory.length > historyLength) {
            swrHistory.shift();
        }
        const avgValue = swrHistory.reduce((sum, v) => sum + v, 0) / swrHistory.length;
      const swr = window.calibrationEngine.calibrateNumeric("SWR", avgValue);

        const swrClamped = Math.min(swr, 10.0);
        const valueSpan = document.getElementById('swrMeterValue');
        if (valueSpan) valueSpan.textContent = window.MeterFormatters.swr(swrClamped);
        if (window.meterPanel) window.meterPanel.update('swr', (swrClamped - 1.0) * 127.5);
    }

    function updateCompressionMeter(value) {
        const percent = state.isTransmitting ? Math.max(0, Math.min(100, Math.round((value / 255) * 100))) : 0;
        const valueSpan = document.getElementById('compressionMeterValue');
        if (valueSpan) valueSpan.textContent = window.MeterFormatters.compressionOverlay(percent);
        if (window.meterPanel) window.meterPanel.update('compression', percent);
    }

    // ALC gauge (0-255 raw value)
    function updateALCMeter(value) {
        // When not transmitting, always show the bottom scale value (0%)
        if (!state.isTransmitting) {
            const valueSpan = document.getElementById('alcValue');
            const progressBar = document.getElementById('alcBar');
            if (valueSpan) valueSpan.textContent = window.MeterFormatters.percent(0);
            if (progressBar) {
                progressBar.style.width = '0%';
                progressBar.setAttribute('aria-valuenow', 0);
                progressBar.className = 'progress-bar bg-success';
            }
            const alcMeterValue = document.getElementById('alcMeterValue');
            if (alcMeterValue) alcMeterValue.textContent = window.MeterFormatters.alcVolts(0);
            if (window.meterPanel) window.meterPanel.update('alc', 0);
            return;
        }

        const alcVolts = window.calibrationEngine.calibrateNumeric("ALC", value);
        const percentage = Math.round((value / 255) * 100);
        const valueSpan = document.getElementById('alcValue');
        const progressBar = document.getElementById('alcBar');

        if (valueSpan) valueSpan.textContent = window.MeterFormatters.percent(percentage);
        if (progressBar) {
            progressBar.style.width = `${percentage}%`;
            progressBar.setAttribute('aria-valuenow', percentage);

            // Color coding: green < 70%, yellow < 90%, red >= 90%
            progressBar.className = 'progress-bar';
            if (percentage < 70) {
                progressBar.classList.add('bg-success');
            } else if (percentage < 90) {
                progressBar.classList.add('bg-warning');
            } else {
                progressBar.classList.add('bg-danger');
            }
        }

        // Update ALC gauge (pass raw 0-255 value directly, gauge uses 0-255 scale)
        const alcMeterValue = document.getElementById('alcMeterValue');
        if (alcMeterValue) alcMeterValue.textContent = window.MeterFormatters.alcVolts(alcVolts);

        if (window.meterPanel) window.meterPanel.update('alc', value);
    }

    // Update IDD display (0-255 raw value, display as amps)
    function updateIDDMeter(value) {
        // Smoothing: ignore sudden jumps >5A, ignore 0 unless persists, clamp to 0-25A
        if (!window._iddLast) window._iddLast = 0;
       const amps = window.calibrationEngine.calibrateNumeric("IDD", value);
        // Ignore 0 unless it persists for 2+ updates
        if (amps === 0) {
            window._iddZeroCount = (window._iddZeroCount || 0) + 1;
            if (window._iddZeroCount < 2) return;
        } else {
            window._iddZeroCount = 0;
        }
        // Ignore sudden jumps >5A
        if (Math.abs(amps - window._iddLast) > 5 && window._iddLast !== 0) {
            return;
        }
        window._iddLast = amps;
        const iddMeterValue = document.getElementById('iddMeterValue');
        if (iddMeterValue) {
            iddMeterValue.textContent = window.MeterFormatters.iddOverlay(amps);
        }
        if (window.meterPanel) window.meterPanel.update('idd', Math.max(0, Math.min(amps, 25)));
    }

    // Update PA Voltage display (0-255 raw value, display as volts)
    // Filter out noisy readings - PA voltage should be stable around 48V
    let lastValidVDD = 204; // Default to ~48V
    function updatePAVoltage(value) {
        // minRaw=175 (~41.2V) not 170 (~40V): keeps a margin above the gauge
        // minimum so a threshold reading can never pin the needle to the bottom.
        const minRaw = 175;  // ~41.2V
        const maxRaw = 235;  // ~55V
        if (!window._vddLast) window._vddLast = 48;
        if (value >= minRaw && value <= maxRaw) {
            lastValidVDD = value;
        } else {
            return;
        }
        const volts = window.calibrationEngine.calibrateNumeric("VPA", lastValidVDD);
        // Ignore sudden jumps >3V
        if (Math.abs(volts - window._vddLast) > 3 && window._vddLast !== 0) {
            return;
        }
        window._vddLast = volts;
        const vddMeterValue = document.getElementById('vddMeterValue');
        if (vddMeterValue) {
            vddMeterValue.textContent = window.MeterFormatters.vddOverlay(volts);
        }
        if (window.meterPanel) window.meterPanel.update('vdd', Math.max(40, Math.min(volts, 55)));
    }

    // Update PA Temperature display (value is directly in °C from IF command)
    function updatePATemperature(tempC) {
        // Smoothing: ignore sudden jumps >10°C, ignore 0 unless persists
        if (!window._paTempLast) window._paTempLast = 0;
        if (!window._paTempZeroCount) window._paTempZeroCount = 0;
        if (tempC === 0) {
            window._paTempZeroCount++;
            if (window._paTempZeroCount < 2) return;
        } else {
            window._paTempZeroCount = 0;
        }
        if (Math.abs(tempC - window._paTempLast) > 10 && window._paTempLast !== 0) {
            return;
        }
       window._paTempLast = window.calibrationEngine.calibrateNumeric("TPA", tempC);
        if (window.meterPanel) window.meterPanel.update('temp', tempC);
        // Update gauge overlay value span
        const tempDisplay = document.getElementById('paTemperatureValue');
        if (tempDisplay) {
            tempDisplay.textContent = window.MeterFormatters.tempOverlay(tempC);
        }
    }

    // Update MIC bar meter (0-255 raw value)
    function updateMICMeter(value) {
        const percentage = Math.round((value / 255) * 100);
        const valueSpan = document.getElementById('micValue');
        const progressBar = document.getElementById('micBar');

        if (valueSpan) valueSpan.textContent = window.MeterFormatters.percent(percentage);
        if (progressBar) {
            progressBar.style.width = `${percentage}%`;
            progressBar.setAttribute('aria-valuenow', percentage);

            // Color coding: green < 80%, warning >= 80%
            progressBar.className = 'progress-bar';
            if (percentage < 80) {
                progressBar.classList.add('bg-success');
            } else {
                progressBar.classList.add('bg-warning');
            }
        }
    }

    // Attach AF Gain slider event listeners only once, when data is available
    function attachAfGainSliderListeners() {
        const sliderA = document.getElementById('afGainSliderA');
        const sliderB = document.getElementById('afGainSliderB');
        const labelA = document.getElementById('afGainValueA');
        const labelB = document.getElementById('afGainValueB');
        // Debounce logic for AF Gain sliders
        function debounce(fn, delay) {
            let timer = null;
            return function(...args) {
                clearTimeout(timer);
                timer = setTimeout(() => fn.apply(this, args), delay);
            };
        }
        if (sliderA && !sliderA._afGainListenerAttached && labelA) {
            const debouncedSendA = debounce(function () {
                sendAfGain('A', parseInt(sliderA.value));
            }, 100);
            sliderA.addEventListener('input', function () {
                labelA.innerText = sliderA.value;
                updateAfGainFill('afGainSliderA');
                debouncedSendA();
            });
            sliderA._afGainListenerAttached = true;
        }
        if (sliderB && !sliderB._afGainListenerAttached && labelB) {
            const debouncedSendB = debounce(function () {
                sendAfGain('B', parseInt(sliderB.value));
            }, 100);
            sliderB.addEventListener('input', function () {
                labelB.innerText = sliderB.value;
                updateAfGainFill('afGainSliderB');
                debouncedSendB();
            });
            sliderB._afGainListenerAttached = true;
        }
    }

    // Kick everything off
    initializeDigitInteraction('A');
    initializeDigitInteraction('B');
    const s = document.getElementById('powerSlider'); if (s) updateSliderFill(s);
    fetchRadioStatus();
    state.pollingInterval = setInterval(fetchRadioStatus, 500);

    // Robustly track editing state for the power slider to prevent backend/UI jumps
    const powerSlider = document.getElementById('powerSlider');
    const powerDisplay = document.getElementById('powerValue');
    if (powerSlider && powerDisplay) {
        window.editingPower = false;
        // Set editingPower true on any user interaction
        powerSlider.addEventListener('input', function () {
            window.editingPower = true;
            powerDisplay.textContent = window.MeterFormatters.powerLabel(powerSlider.value);
        });
        powerSlider.addEventListener('mousedown', function () {
            window.editingPower = true;
        });
        powerSlider.addEventListener('touchstart', function () {
            window.editingPower = true;
        });
        powerSlider.addEventListener('focus', function () {
            window.editingPower = true;
        });
        // Reset editingPower on all possible end events
        powerSlider.addEventListener('change', function () {
            window.editingPower = false;
        });
        powerSlider.addEventListener('mouseup', function () {
            window.editingPower = false;
        });
        powerSlider.addEventListener('touchend', function () {
            window.editingPower = false;
        });
        powerSlider.addEventListener('mouseleave', function () {
            window.editingPower = false;
        });
        powerSlider.addEventListener('blur', function () {
            window.editingPower = false;
        });
        // Defensive: clear editingPower if window loses focus
        window.addEventListener('blur', function () {
            window.editingPower = false;
        });
    }

    // Overwrite the interim window.radioControl with the real implementations
    window.radioControl = {
        setFrequency,
        setBand,
        setMode,
        setAntenna,
        setRoofingFilter,
        _state: state,  // Expose state for TX indicator updates
        updatePowerDisplay: updatePowerDisplay,
        setPower: setPower
    };

    // Expose meter update functions globally for SignalR
    window.updatePowerMeter = updatePowerMeter;
    window.updateSWRMeter = updateSWRMeter;
    window.updateCompressionMeter = updateCompressionMeter;
    window.updateALCMeter = updateALCMeter;
    window.updateIDDMeter = updateIDDMeter;
    window.updatePAVoltage = updatePAVoltage;
    window.updatePATemperature = updatePATemperature;
    window.updateMICMeter = updateMICMeter;

    // --- Raw Meter Label Visibility State (S-Meter and Power Out) ---
    // Use localStorage to sync across tabs/pages
    function getShowRawMeterLabels() {
        return localStorage.getItem('showRawMeterLabels') === 'true';
    }
    function setShowRawMeterLabels(val) {
        localStorage.setItem('showRawMeterLabels', val ? 'true' : 'false');
        window.showRawMeterLabels = val;
        updateRawMeterLabelVisibility();
    }
    function updateRawMeterLabelVisibility() {
        var show = window.showRawMeterLabels;
        var elS = document.getElementById('raw-s-meter-label-a');
        if (elS) elS.style.display = show ? '' : 'none';
        var elP = document.getElementById('raw-powerout-label');
        if (elP) elP.style.display = show ? '' : 'none';
    }
    // Listen for localStorage changes (cross-tab)
    window.addEventListener('storage', function (e) {
        if (e.key === 'showRawMeterLabels') {
            window.showRawMeterLabels = getShowRawMeterLabels();
            updateRawMeterLabelVisibility();
        }
    });
    // Expose for other scripts
    window.getShowRawMeterLabels = getShowRawMeterLabels;
    window.setShowRawMeterLabels = setShowRawMeterLabels;
    window.updateRawMeterLabelVisibility = updateRawMeterLabelVisibility;
    // Init on page load
    window.showRawMeterLabels = getShowRawMeterLabels();
    document.addEventListener('DOMContentLoaded', updateRawMeterLabelVisibility);

    // --- Raw S-Meter Value Update ---
    // Store last raw S-Meter value for VFO A
    window.lastRawSMeterA = 0;
    function updateRawSMeterValueA(val) {
        window.lastRawSMeterA = val;
        var el = document.getElementById('rawSMeterValueA');
        if (el) el.textContent = val;
    }

    // Calibration page: Toggle button logic for raw meter labels
    // (runs on both pages, harmless if button not present)
    document.addEventListener('DOMContentLoaded', function () {
        var btn = document.getElementById('toggleRawMeterLabelsBtn');
        if (btn) {
            function updateBtnText() {
                btn.textContent = window.getShowRawMeterLabels() ? 'Hide Raw Meter Readings' : 'Show Raw Meter Readings';
            }
            btn.addEventListener('click', function () {
                var newVal = !window.getShowRawMeterLabels();
                window.setShowRawMeterLabels(newVal);
                updateBtnText();
            });
            updateBtnText();
        }
    });
})();


connection.start().catch(function (err) {
    return;
});

// Show touch frequency controls on mobile
document.addEventListener('DOMContentLoaded', function () {
    if ('ontouchstart' in window || navigator.maxTouchPoints > 0) {
        var aControls = document.getElementById('freqA-controls');
        var bControls = document.getElementById('freqB-controls');
        if (aControls) aControls.style.display = '';
        if (bControls) bControls.style.display = '';
    }
});

pollInitStatus();
