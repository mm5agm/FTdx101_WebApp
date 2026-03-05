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

// Outer state - used only by the outer helpers below.
// The IIFE has its own richer state object that drives the real UI.
const state = {
    selectedIdx: { A: null, B: null },
    editing: { A: false, B: false },
    localFreq: { A: null, B: null },
    lastBackendFreq: { A: null, B: null }
};

// Frequency display renderer (outer version, used by outer updateFrequencyDisplay)
function updateFrequencyDisplay(receiver, freqHz) {
    const display = document.getElementById('freq' + receiver);
    if (!display) {
        console.warn(`Frequency display element not found: freq${receiver}`);
        return;
    }
    let selIdx = state.selectedIdx[receiver];
    let freqToShow = (!state.editing[receiver] || state.localFreq[receiver] === null)
        ? state.lastBackendFreq[receiver]
        : state.localFreq[receiver];
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
        if (idx !== -1) {
            digits.forEach(d => d.classList.remove('selected'));
            state.selectedIdx[receiver] = idx;
            digits[idx].classList.add('selected');
            state.editing[receiver] = true;
            state.localFreq[receiver] = parseInt(digits.map(d => d.textContent).join(''));
            updateFrequencyDisplay(receiver, state.localFreq[receiver]);
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
        console.log(`Setting ${receiver} band to ${band}`);
        if (window.highlightButtons) highlightButtons(receiver, band, state.lastMode ? state.lastMode[receiver] : undefined, state.lastAntenna ? state.lastAntenna[receiver] : undefined);
        if (state.lastBand) state.lastBand[receiver] = band;
        const response = await fetch(`/api/cat/band/${receiver.toLowerCase()}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ band })
        });
        if (!response.ok) {
            console.error('Failed to set band:', await response.text());
        } else {
            console.log(`Band set successfully: ${receiver} -> ${band}`);
        }
    } catch (error) {
        console.error('Error setting band:', error);
    }
};

// Outer mode setter - called from Razor inline onchange on mode select
window.setMode = async function (receiver, mode) {
    const modeToCatCode = {
        "LSB": "1", "USB": "2", "CW-U": "3", "FM": "4", "AM": "5", "RTTY-L": "6", "CW-L": "7", "DATA-L": "8", "RTTY-U": "9", "DATA-FM": "A", "FM-N": "B", "DATA-U": "C", "AM-N": "D", "PSK": "E", "DATA-FM-N": "F"
    };
    const catCode = modeToCatCode[mode];
    if (!catCode) {
        console.error("Unknown mode:", mode);
        return;
    }
    console.log(`Setting ${receiver} mode to ${mode} (CAT code ${catCode})`);
    const response = await fetch(`/api/cat/mode/${receiver.toLowerCase()}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mode: catCode })
    });
    if (!response.ok) {
        console.error('Failed to set mode:', await response.text());
    } else {
        console.log(`Mode set successfully: ${receiver} -> ${mode}`);
    }
};

// Outer antenna setter - called from Razor inline onchange on antenna buttons
window.setAntenna = async function (receiver, antenna) {
    if (window.pausePolling) pausePolling();
    try {
        console.log(`Setting ${receiver} antenna to ${antenna}`);
        if (window.highlightButtons) highlightButtons(receiver, state.lastBand ? state.lastBand[receiver] : undefined, state.lastMode ? state.lastMode[receiver] : undefined, antenna);
        if (state.lastAntenna) state.lastAntenna[receiver] = antenna;
        const response = await fetch(`/api/cat/antenna/${receiver.toLowerCase()}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ antenna })
        });
        if (!response.ok) {
            console.error('Failed to set antenna:', await response.text());
        } else {
            console.log(`Antenna set successfully: ${receiver} -> ${antenna}`);
        }
    } catch (error) {
        console.error('Error setting antenna:', error);
    }
};

// Outer power slider max updater
function updatePowerSliderMax(maxPower) {
    const sliderA = document.getElementById('powerSliderA');
    const sliderB = document.getElementById('powerSliderB');
    const labelMaxA = document.getElementById('powerMaxLabelA');
    const labelMaxB = document.getElementById('powerMaxLabelB');

    if (sliderA) sliderA.max = maxPower;
    if (sliderB) sliderB.max = maxPower;
    if (labelMaxA) labelMaxA.textContent = maxPower + 'W';
    if (labelMaxB) labelMaxB.textContent = maxPower + 'W';
}

// TX Indicator updater
function updateTxIndicators(isTransmitting) {
    const txIndicatorA = document.getElementById('txIndicatorA');
    const txIndicatorB = document.getElementById('txIndicatorB');

    // Debug logging
    console.log(`[TX Indicator] isTransmitting: ${isTransmitting}`);

    // Update state for meter display logic (if IIFE state is accessible)
    if (window.radioControl && window.radioControl._state) {
        window.radioControl._state.isTransmitting = isTransmitting;
        console.log(`[TX Indicator] State updated: ${window.radioControl._state.isTransmitting}`);
    } else {
        console.warn('[TX Indicator] radioControl._state not available yet');
    }

    if (isTransmitting) {
        // Show TX indicators with red background and make them blink
        if (txIndicatorA) {
            txIndicatorA.style.display = 'inline-block';
            txIndicatorA.className = 'badge bg-danger';
            txIndicatorA.classList.add('tx-blink');
        }
        if (txIndicatorB) {
            txIndicatorB.style.display = 'inline-block';
            txIndicatorB.className = 'badge bg-danger';
            txIndicatorB.classList.add('tx-blink');
        }
    } else {
        // Hide TX indicators and zero out power/SWR meters
        if (txIndicatorA) {
            txIndicatorA.style.display = 'none';
            txIndicatorA.classList.remove('tx-blink');
        }
        if (txIndicatorB) {
            txIndicatorB.style.display = 'none';
            txIndicatorB.classList.remove('tx-blink');
        }

        // Zero the meters when not transmitting
        if (typeof window.updatePowerMeter === 'function') {
            window.updatePowerMeter(0);
        }
        if (typeof window.updateSWRMeter === 'function') {
            window.updateSWRMeter(0);
        }
    }
}

// Outer power setter (stub - real version is inside the IIFE)
async function setPower(receiver, watts) {
    const maxPower = state.radioModel === 'FTdx101MP' ? 200 : 100;
    const power = Math.max(5, Math.min(parseInt(watts), maxPower));
    try {
        console.log(`Setting ${receiver} power to ${power}W`);
        const response = await fetch(`/api/cat/power/${receiver.toLowerCase()}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Watts: power })
        });
        if (!response.ok) {
            console.error('Failed to set power:', await response.text());
        } else {
            console.log(`Power set successfully: ${receiver} -> ${power}W`);
        }
        updatePowerDisplay(receiver, power);
    } catch (error) {
        console.error('Error setting power:', error);
    }
}

// Placeholder - replaced by the IIFE's real implementation once it runs
window.updatePowerDisplay = function () {
    console.warn("updatePowerDisplay not implemented");
};


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
        console.warn(`updateModeSelect: select element not found for modeSelect${receiver}`);
    }
}

// First SignalR RadioStateUpdate handler (outer scope).
// Handles ModeA/B, FrequencyA/B, PowerA/B updates pushed from the backend.
connection.on("RadioStateUpdate", function (update) {
    console.log('[SignalR] Received RadioStateUpdate:', JSON.stringify(update));

    // --- MODE CHANGE (THE BUG FIX) ---
    // Update the dropdown select when mode changes from the radio.
    if (update.property === "ModeA") {
        updateModeSelect('A', update.value);
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
        console.log(`[SignalR] Received BandA update: ${update.value}`);
        updateBandButton('A', update.value);
    }
    if (update.property === "BandB") {
        console.log(`[SignalR] Received BandB update: ${update.value}`);
        updateBandButton('B', update.value);
    }

    // --- POWER CHANGE ---
    if (update.property === "PowerA") {
        updatePowerDisplay("A", update.value);
        const sliderA = document.getElementById('powerSliderA');
        if (sliderA) sliderA.value = update.value;
    }

    // --- METER UPDATES ---
    if (update.property === "PowerMeter" && typeof window.updatePowerMeter === 'function') {
        window.updatePowerMeter(update.value);
    }
    if (update.property === "SWRMeter" && typeof window.updateSWRMeter === 'function') {
        window.updateSWRMeter(update.value);
    }
    if (update.property === "IDDMeter" && typeof window.updateIDDMeter === 'function') {
        window.updateIDDMeter(update.value);
    }
    if (update.property === "VDDMeter" && typeof window.updatePAVoltage === 'function') {
        window.updatePAVoltage(update.value);
    }

    // --- TX INDICATOR ---
    if (update.property === "IsTransmitting") {
        updateTxIndicators(update.value);
    }

    if (update.property === "PowerB") {
        updatePowerDisplay("B", update.value);
        const sliderB = document.getElementById('powerSliderB');
        if (sliderB) sliderB.value = update.value;
    }
    // Generic "Power" fallback (maps to receiver A)
    if (update.property === "Power") {
        updatePowerDisplay("A", update.value);
        const sliderA = document.getElementById('powerSliderA');
        if (sliderA) sliderA.value = update.value;
    }
});

// SignalR connection is started once below (after the IIFE) with a .catch() error handler.

// ---------------------------------------------------------------------------
// Initialization overlay polling
// Polls /api/status/init every second until status is "complete" or "error".
// On error, redirects to /Settings ONLY if user hasn't dismissed the overlay.
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
        } else if (data.status === "error") {
            statusText.innerHTML = "Radio initialization failed. <a href='/Settings' class='text-white'>Go to Settings</a> or <button onclick='dismissInitOverlay()' class='btn btn-sm btn-warning ms-2'>Continue Anyway</button>";
            overlay.style.display = "block";
            // Don't auto-redirect - let user choose
        } else {
            overlay.style.display = "block";
        }

        if (data.status !== "complete" && !initPollingStopped) {
            setTimeout(pollInitStatus, 1000);
        }
    } catch (error) {
        console.error('Error polling init status:', error);
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
        console.error('Error updating band buttons:', error);
    }
}

// Update band button selection for a specific receiver (called via SignalR)
function updateBandButton(receiver, band) {
    console.log(`[Band] updateBandButton called: receiver=${receiver}, band=${band}`);
    if (!band) {
        console.warn('[Band] updateBandButton: band is null/undefined');
        return;
    }
    const bandLower = band.toLowerCase();
    const inputs = document.querySelectorAll(`input[name="band-${receiver}"]`);
    console.log(`[Band] Found ${inputs.length} band buttons for receiver ${receiver}`);

    let foundMatch = false;
    inputs.forEach(radio => {
        const matches = (radio.value.toLowerCase() === bandLower);
        if (matches) {
            foundMatch = true;
            console.log(`[Band] Setting checked=true for ${radio.value}`);
        }
        radio.checked = matches;
    });

    if (!foundMatch) {
        console.warn(`[Band] No matching band button found for band="${band}" (looked for "${bandLower}")`);
    }
    console.log(`[Band] Updated ${receiver} band button to: ${band}`);
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
function sendAfGain(receiver, value) {
    fetch(`/api/cat/afgain/${receiver.toLowerCase()}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(value)
    }).then(r => {
        if (!r.ok) {
            r.text().then(t => console.error('Failed to set AF Gain:', t));
        }
    }).catch(e => console.error('Error setting AF Gain:', e));
}

(function () {
    'use strict';

    console.log('=== FTdx101 Control Interface Starting ===');

    // Gauge instances (canvas-gauge RadialGauge)
    let gaugeA, gaugeB;

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
            console.warn(`Frequency display element not found: freq${receiver}`);
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

    function initializeDigitInteraction(receiver) {
        const display = document.getElementById('freq' + receiver);
        if (!display) {
            console.warn(`Cannot initialize digit interaction: freq${receiver} not found`);
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
            console.log(`Setting ${receiver} frequency to ${freqHz} Hz`);
            const response = await fetch(`/api/cat/frequency/${receiver.toLowerCase()}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ frequencyHz: freqHz })
            });
            if (!response.ok) {
                console.error('Failed to set frequency:', await response.text());
            } else {
                console.log(`Frequency set successfully: ${receiver} -> ${freqHz} Hz`);
            }
            updateFrequencyDisplay(receiver, freqHz);
        } catch (error) {
            console.error('Error setting frequency:', error);
        }
    }

    async function setBand(receiver, band) {
        const didPause = pausePolling();
        try {
            console.log(`Setting ${receiver} band to ${band}`);
            highlightButtons(receiver, band, state.lastMode[receiver], state.lastAntenna[receiver]);
            state.lastBand[receiver] = band;
            const response = await fetch(`/api/cat/band/${receiver.toLowerCase()}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ band })
            });
            if (!response.ok) {
                console.error('Failed to set band:', await response.text());
            } else {
                console.log(`Band set successfully: ${receiver} -> ${band}`);
            }
        } catch (error) {
            console.error('Error setting band:', error);
        } finally {
            if (didPause) {
                resumePolling();
            }
        }
    }

    async function setMode(receiver, mode) {
        const catCode = modeToCatCode[mode];
        if (!catCode) {
            console.error("Unknown mode:", mode);
            return;
        }
        console.log(`Setting ${receiver} mode to ${mode} (CAT code ${catCode})`);
        const response = await fetch(`/api/cat/mode/${receiver.toLowerCase()}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ mode: catCode })
        });
        if (!response.ok) {
            console.error('Failed to set mode:', await response.text());
        } else {
            console.log(`Mode set successfully: ${receiver} -> ${mode}`);
        }
    }

    async function setAntenna(receiver, antenna) {
        const didPause = pausePolling();
        try {
            console.log(`Setting ${receiver} antenna to ${antenna}`);
            highlightButtons(receiver, state.lastBand[receiver], state.lastMode[receiver], antenna);
            state.lastAntenna[receiver] = antenna;
            const response = await fetch(`/api/cat/antenna/${receiver.toLowerCase()}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ antenna })
            });
            if (!response.ok) {
                console.error('Failed to set antenna:', await response.text());
            } else {
                console.log(`Antenna set successfully: ${receiver} -> ${antenna}`);
            }
        } catch (error) {
            console.error('Error setting antenna:', error);
        } finally {
            if (didPause) {
                resumePolling();
            }
        }
    }

    function pausePolling() {
        if (state.pollingInterval && !state.operationInProgress) {
            state.operationInProgress = true;
            console.log('Polling paused for operation');
            return true;
        }
        console.log('Could not pause - operation already in progress');
        return false;
    }

    function resumePolling() {
        if (state.operationInProgress) {
            state.operationInProgress = false;
            setTimeout(fetchRadioStatus, 500);
            console.log('Polling resumed');
        }
    }

    // Full status poll - updates frequencies, S-meter, band/mode/antenna buttons, and power
    async function fetchRadioStatus() {
        if (state.operationInProgress) {
            console.log('Skipping poll - operation in progress');
            return;
        }
        try {
            const response = await fetch('/api/cat/status');
            if (!response.ok) {
                console.error('Failed to fetch status:', response.status);
                return;
            }
            const data = await response.json();

            if (data.maxPower !== undefined) {
                state.maxPower = data.maxPower;
                updatePowerSliderMax(data.maxPower);
            }
            if (data.radioModel !== undefined) {
                state.radioModel = data.radioModel;
            }

            state.lastBackendFreq.A = data.vfoA.frequency;
            state.lastBackendFreq.B = data.vfoB.frequency;
            state.lastMode.A = data.vfoA.mode;
            state.lastMode.B = data.vfoB.mode;
            state.lastAntenna.A = data.vfoA.antenna;
            state.lastAntenna.B = data.vfoB.antenna;

            if (data.vfoA.power !== undefined) {
                updatePowerSlider('A', data.vfoA.power);
            }
            if (data.vfoB.power !== undefined) {
                updatePowerSlider('B', data.vfoB.power);
            }

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

            // Update band buttons from polling (fixes WSJT-X and radio band changes)
            // Always log band data for debugging
            console.log(`[Poll] Band data: A=${data.vfoA.band} (last=${state.lastBand.A}), B=${data.vfoB.band} (last=${state.lastBand.B})`);

            // FORCE update band buttons every poll to test if DOM updates work
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
            console.error('Error fetching radio status:', error);
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
        const display = document.getElementById('powerValue' + receiver);
        if (display) {
            display.textContent = watts + 'W';
        }
        const slider = document.getElementById('powerSlider' + receiver);
        if (slider) {
            slider.value = watts;
            updateSliderFill(slider);
        }
        if (state.lastPower) state.lastPower[receiver] = watts;
    }

    async function setPower(receiver, watts) {
        try {
            console.log(`Setting ${receiver} power to ${watts}W`);
            state.lastPower[receiver] = parseInt(watts);
            const response = await fetch(`/api/cat/power/${receiver.toLowerCase()}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ watts: parseInt(watts) })
            });
            if (!response.ok) {
                console.error('Failed to set power:', await response.text());
            } else {
                console.log(`Power set successfully: ${receiver} -> ${watts}W`);
            }
            updatePowerDisplay(receiver, watts);
        } catch (error) {
            console.error('Error setting power:', error);
        }
    }

    function updatePowerSlider(receiver, watts) {
        if (state.editingPower && state.editingPower[receiver]) {
            return; // Don't update while user is dragging the slider
        }
        updatePowerDisplay(receiver, watts);
    }

    function updatePowerSliderMax(maxPower) {
        const sliderA = document.getElementById('powerSliderA');
        const sliderB = document.getElementById('powerSliderB');
        const labelMaxA = document.getElementById('powerMaxLabelA');
        const labelMaxB = document.getElementById('powerMaxLabelB');

        if (sliderA) { sliderA.max = maxPower; updateSliderFill(sliderA); }
        if (sliderB) { sliderB.max = maxPower; updateSliderFill(sliderB); }
        if (labelMaxA) labelMaxA.textContent = maxPower + 'W';
        if (labelMaxB) labelMaxB.textContent = maxPower + 'W';
    }

    // S-Meter label interpolation
    function sMeterLabel(val) {
        const points = [
            { label: "S0", value: 0 },
            { label: "S1", value: 4 },
            { label: "S3", value: 30 },
            { label: "S5", value: 65 },
            { label: "S7", value: 95 },
            { label: "S9", value: 130 },
            { label: "S9+20", value: 171 },
            { label: "S9+40", value: 212 },
            { label: "S9+60", value: 255 }
        ];
        if (val <= points[0].value) return points[0].label;
        for (let i = 1; i < points.length; i++) {
            if (val <= points[i].value) {
                const prev = points[i - 1];
                const next = points[i];
                const frac = (val - prev.value) / (next.value - prev.value);
                if (val === next.value) return next.label;
                if (next.label.startsWith("S9+")) {
                    const plus = parseInt(next.label.replace("S9+", ""));
                    const prevPlus = prev.label.startsWith("S9+") ? parseInt(prev.label.replace("S9+", "")) : 0;
                    const interp = Math.round(prevPlus + frac * (plus - prevPlus));
                    return "S9+" + interp;
                } else {
                    const prevNum = parseInt(prev.label.replace("S", ""));
                    const nextNum = parseInt(next.label.replace("S", ""));
                    const interp = Math.round(prevNum + frac * (nextNum - prevNum));
                    return "S" + interp;
                }
            }
        }
        return "S9+60";
    }

    function updateSMeter(receiver, value) {
        // Update only the analog gauge (text badges removed from UI)
        if (receiver === 'A' && gaugeA) {
            gaugeA.value = value;
            gaugeA.draw();
        } else if (receiver === 'B' && gaugeB) {
            gaugeB.value = value;
            gaugeB.draw();
        }
    }

    // Smoothing buffers for meters (reduce jumpiness)
    let powerHistory = [];
    let swrHistory = [];
    const historyLength = 7; // Average last 7 readings for smoother display

    function updatePowerMeter(value) {
        // Debug: log TX state and value
        console.log(`[PowerMeter] isTransmitting: ${state.isTransmitting}, value: ${value}`);

        // Clear immediately when not transmitting OR when we get a zero while transmitting
        // (zero during TX means tail-end of transmission, should clear the meter)
        if (!state.isTransmitting || (state.isTransmitting && value === 0)) {
            powerHistory = []; // Clear history
            const valueSpan = document.getElementById('powerMeterValue');
            if (valueSpan) valueSpan.textContent = '0W';
            if (window.gaugePower) {
                window.gaugePower.value = 0;
                window.gaugePower.draw();
            }
            console.log('[PowerMeter] Cleared (zero or not TX)');
            return;
        }

        // Add to history buffer
        powerHistory.push(value);
        if (powerHistory.length > historyLength) {
            powerHistory.shift(); // Remove oldest
        }

        // Calculate average
        const avgValue = powerHistory.reduce((sum, v) => sum + v, 0) / powerHistory.length;

        // FT-dx101 power meter calibration: User reports 20W actual shows ~70 raw
        // So: 20W / 70 = 0.286 watts per raw unit
        // This scale is the SAME for both FTdx101D and MP (same meter, different max power)
        const scale = 0.286;
        const watts = Math.round(avgValue * scale);

        console.log(`[PowerMeter] Raw: ${value}, Avg: ${avgValue.toFixed(1)}, Watts: ${watts}`);

        const valueSpan = document.getElementById('powerMeterValue');
        if (valueSpan) valueSpan.textContent = `${watts}W`;

        if (window.gaugePower) {
            // The gauge needle position must match the watt labels, not the raw 0-255 scale
            // So we set gauge.value = watts (which the gauge will display on its 0-255 internal scale)
            // But the gauge maxValue is 255, so we need to use watts directly
            window.gaugePower.value = watts;
            window.gaugePower.draw();
        }
    }

    function updateSWRMeter(value) {
        // Debug: log TX state and value ALWAYS
        console.log(`[SWRMeter] isTransmitting: ${state.isTransmitting}, value: ${value}`);

        // Clear immediately when not transmitting OR when we get a zero while transmitting
        // (zero during TX means tail-end of transmission, should clear the meter)
        if (!state.isTransmitting || (state.isTransmitting && value === 0)) {
            swrHistory = []; // Clear history
            const valueSpan = document.getElementById('swrMeterValue');
            if (valueSpan) valueSpan.textContent = '1.0:1';
            if (window.gaugeSWR) {
                window.gaugeSWR.value = 0;
                window.gaugeSWR.draw();
            }
            console.log('[SWRMeter] Cleared (zero or not TX)');
            return;
        }

        // Add to history buffer
        swrHistory.push(value);
        if (swrHistory.length > historyLength) {
            swrHistory.shift(); // Remove oldest
        }

        // Calculate average to reduce jumpiness
        const avgValue = swrHistory.reduce((sum, v) => sum + v, 0) / swrHistory.length;

        // FT-dx101 SWR meter calibration
        // User reports: SWR 5:1 actual shows raw values 68-255 (avg peaks ~244)
        // So: (5 - 1) / 244 = 0.0164, or SWR = 1.0 + (value / 61)
        const swr = 1.0 + (avgValue / 61.0);
        const swrClamped = Math.min(swr, 10.0); // Clamp to 10.0 max

        console.log(`[SWRMeter] Raw: ${value}, Avg: ${avgValue.toFixed(1)}, SWR: ${swrClamped.toFixed(1)}:1`);

        const valueSpan = document.getElementById('swrMeterValue');
        if (valueSpan) valueSpan.textContent = `${swrClamped.toFixed(1)}:1`;

        if (window.gaugeSWR) {
            // The gauge labels range from 1.0 to 3.0, and the gauge internal arc is 0-255
            // So we need to map the SWR ratio to the gauge position:
            // SWR 1.0 → position 0, SWR 3.0 → position 255
            // Position = (swr - 1.0) / (3.0 - 1.0) * 255 = (swr - 1.0) * 127.5
            const gaugePosition = (swrClamped - 1.0) * 127.5;
            window.gaugeSWR.value = gaugePosition;
            window.gaugeSWR.draw();
        }
    }

    // Update ALC bar meter (0-255 raw value) and ALC gauge
    function updateALCMeter(value) {
        // Convert 0-255 raw value to 0-50V scale for display
        const alcVolts = (value / 255) * 50;
        const percentage = Math.round((value / 255) * 100);
        const valueSpan = document.getElementById('alcValue');
        const progressBar = document.getElementById('alcBar');

        if (valueSpan) valueSpan.textContent = `${percentage}%`;
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
        if (alcMeterValue) alcMeterValue.textContent = `${alcVolts.toFixed(0)}V`;

        if (window.gaugeALC) {
            window.gaugeALC.value = value;  // Raw 0-255 value
            window.gaugeALC.draw();
        }
    }

    // Update IDD display (0-255 raw value, display as amps)
    function updateIDDMeter(value) {
        // Assuming 255 = ~25A max for FTdx101MP (adjust based on actual specs)
        const amps = (value / 255) * 25.0;
        const iddDisplay = document.getElementById('iddDisplayValue');

        if (iddDisplay) {
            iddDisplay.textContent = `${amps.toFixed(1)}A`;
            // Color coding based on current draw
            iddDisplay.classList.remove('bg-primary', 'bg-warning', 'bg-danger', 'bg-success');
            if (amps < 10) {
                iddDisplay.classList.add('bg-success');
            } else if (amps < 20) {
                iddDisplay.classList.add('bg-primary');
            } else {
                iddDisplay.classList.add('bg-danger');
            }
        }
    }

    // Update PA Voltage display (0-255 raw value, display as volts)
    // Filter out noisy readings - PA voltage should be stable around 48V
    let lastValidVDD = 204; // Default to ~48V
    function updatePAVoltage(value) {
        // Filter out obviously wrong values (PA voltage should be 40-55V range, which is ~170-235 raw)
        // Only accept values in reasonable range
        const minRaw = 170;  // ~40V
        const maxRaw = 235;  // ~55V

        if (value >= minRaw && value <= maxRaw) {
            lastValidVDD = value;
        } else {
            return; // Ignore noisy reading
        }

        // Assuming 255 = ~60V max for PA voltage
        const volts = (lastValidVDD / 255) * 60.0;
        const voltageDisplay = document.getElementById('paVoltageValue');

        if (voltageDisplay) {
            voltageDisplay.textContent = `${volts.toFixed(1)}V`;
            // Color coding based on voltage (nominal ~50V for FTdx101)
            voltageDisplay.classList.remove('bg-secondary', 'bg-success', 'bg-warning', 'bg-danger');
            if (volts < 45) {
                voltageDisplay.classList.add('bg-warning'); // Low voltage warning
            } else if (volts <= 55) {
                voltageDisplay.classList.add('bg-success'); // Normal range
            } else {
                voltageDisplay.classList.add('bg-danger'); // High voltage
            }
        }
    }

    // Update MIC bar meter (0-255 raw value)
    function updateMICMeter(value) {
        const percentage = Math.round((value / 255) * 100);
        const valueSpan = document.getElementById('micValue');
        const progressBar = document.getElementById('micBar');

        if (valueSpan) valueSpan.textContent = `${percentage}%`;
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

    // Unified gauge configuration - supports S-Meter, Power, and SWR
    function makeGaugeConfig(canvasId, type = 'smeter', options = {}) {
        const configs = {
            smeter: {
                minValue: 0,
                maxValue: 255,
                majorTicks: ["0", "4", "30", "65", "95", "130", "171", "212", "255"],
                highlights: [
                    { from: 0, to: 130, color: "rgba(0,255,0,.25)" },
                    { from: 130, to: 255, color: "rgba(255,0,0,.25)" }
                ],
                labels: ["0", "S1", "S3", "S5", "S7", "S9", "+20", "+40", "+60"]
            },
            power: {
                minValue: 0,
                maxValue: 255,  // 0-255 scale from RM1 command
                majorTicks: ["0", "32", "64", "96", "128", "160", "192", "224", "255"],
                highlights: [
                    { from: 0, to: 192, color: "rgba(0,255,0,.25)" },    // Green: 0-75%
                    { from: 192, to: 224, color: "rgba(255,255,0,.25)" }, // Yellow: 75-88%
                    { from: 224, to: 255, color: "rgba(255,0,0,.25)" }    // Red: 88-100%
                ],
                labels: ["0", "25", "50", "75", "100", "125", "150", "175", "200"]
            },
            swr: {
                minValue: 0,
                maxValue: 255,  // 0-255 scale from RM2 command
                majorTicks: ["0", "32", "64", "96", "128", "160", "192", "224", "255"],
                highlights: [
                    { from: 0, to: 85, color: "rgba(0,255,0,.25)" },     // Green: 1.0-1.5
                    { from: 85, to: 128, color: "rgba(255,255,0,.25)" },  // Yellow: 1.5-2.0
                    { from: 128, to: 255, color: "rgba(255,0,0,.25)" }    // Red: 2.0-3.0+
                ],
                labels: ["1.0", "1.3", "1.5", "1.7", "2.0", "2.3", "2.5", "2.7", "3.0"]
            },
            alc: {
                minValue: 0,
                maxValue: 255,  // 0-255 scale (same as other gauges)
                majorTicks: ["0", "32", "64", "96", "128", "160", "192", "224", "255"],
                highlights: [
                    { from: 0, to: 178, color: "rgba(0,255,0,.25)" },      // Green: 0-70% (normal)
                    { from: 178, to: 230, color: "rgba(255,255,0,.25)" },  // Yellow: 70-90% (caution)
                    { from: 230, to: 255, color: "rgba(255,0,0,.25)" }     // Red: 90-100% (high)
                ],
                labels: ["0", "6", "12", "19", "25", "31", "37", "44", "50"]
            }
        };

        const config = configs[type] || configs.smeter;

        return {
            renderTo: canvasId,
            width: options.width || 420,    // Reduced from 560 (25% smaller)
            height: options.height || 135,  // Reduced from 180 (25% smaller)
            units: "",
            minValue: config.minValue,
            maxValue: config.maxValue,
            startAngle: 90,
            ticksAngle: 180,
            valueBox: false,
            majorTicks: config.majorTicks,
            minorTicks: 0,
            strokeTicks: false,
            tickSide: "out",
            needleSide: "center",
            highlights: config.highlights,
            colorPlate: "#ffffff",
            borderShadowWidth: 0,
            borders: false,
            needleShadow: false,
            colorMajorTicks: "#555555",
            colorMinorTicks: "transparent",
            colorNumbers: "transparent",
            fontNumbersSize: 0,
            colorBarProgress: "#198754",
            colorBarProgressEnd: "#198754",
            colorBar: "#dddddd",
            barShadow: 0,
            barWidth: 10,
            barStrokeWidth: 0,
            needleType: "arrow",
            needleWidth: 3,
            needleCircleSize: 7,
            needleCircleOuter: false,
            needleCircleInner: true,
            colorNeedleCircleInner: "#dc3545",
            colorNeedleCircleInnerEnd: "#dc3545",
            animationDuration: 400,
            animationRule: "linear",
            value: 0,
            _labels: config.labels  // Store labels for later use
        };
    }

    function initializeGauges() {
        const gaugeWidth = 420;   // Updated from 560 (25% smaller)
        const gaugeHeight = 135;  // Updated from 180 (25% smaller)

        // Overlay readable labels on top of the canvas gauge
        function createGaugeLabels(canvasId, labels) {
            const canvas = document.getElementById(canvasId);
            if (!canvas || canvas.nextElementSibling?.classList.contains('gauge-labels-overlay')) {
                return;
            }

            const wrapper = document.createElement('div');
            wrapper.className = 'gauge-wrapper';
            // Apply left transform only to S-meter canvases
            const translateX = (canvasId === 'sMeterCanvasA' || canvasId === 'sMeterCanvasB') ? ';transform:translateX(-120px)' : '';
            wrapper.style.cssText = `position:relative;display:block;width:${gaugeWidth}px;height:${gaugeHeight}px;margin-left:0${translateX}`;

            const labelsDiv = document.createElement('div');
            labelsDiv.className = 'gauge-labels-overlay';

            const centerX = gaugeWidth / 2;
            const centerY = gaugeHeight - 64;  // Adjusted from 85 (75% of original)
            const radius = gaugeWidth * 0.17;
            const angleStep = 180 / (labels.length - 1);

            labels.forEach((label, index) => {
                const angle = 180 - (angleStep * index);
                const radians = (angle * Math.PI) / 180;
                const x = centerX + radius * Math.cos(radians);
                const y = centerY - radius * Math.sin(radians);

                const span = document.createElement('span');
                span.className = 'gauge-label';
                span.textContent = label;
                span.style.left = x + 'px';
                span.style.top = y + 'px';
                labelsDiv.appendChild(span);
            });

            canvas.parentNode.insertBefore(wrapper, canvas);
            wrapper.appendChild(canvas);
            wrapper.appendChild(labelsDiv);

            // Add "S-Meter" label at the bottom for S-Meter gauges only
            if (canvasId === 'sMeterCanvasA' || canvasId === 'sMeterCanvasB') {
                const meterLabel = document.createElement('div');
                // Positioned further right and higher up
                meterLabel.style.cssText = 'position:absolute;bottom:40px;left:188px;font-size:14px;font-weight:normal;';
                meterLabel.textContent = 'S-Meter';
                wrapper.appendChild(meterLabel);
            }
        }

        // Initialize S-Meters
        const sMeterConfigA = makeGaugeConfig('sMeterCanvasA', 'smeter');
        gaugeA = new RadialGauge(sMeterConfigA);
        gaugeA.draw();
        createGaugeLabels('sMeterCanvasA', sMeterConfigA._labels);

        const sMeterConfigB = makeGaugeConfig('sMeterCanvasB', 'smeter');
        gaugeB = new RadialGauge(sMeterConfigB);
        gaugeB.draw();
        createGaugeLabels('sMeterCanvasB', sMeterConfigB._labels);

        // Initialize Power Meter (if element exists)
        if (document.getElementById('powerMeterCanvas')) {
            const powerConfig = makeGaugeConfig('powerMeterCanvas', 'power');
            window.gaugePower = new RadialGauge(powerConfig);
            window.gaugePower.draw();
            createGaugeLabels('powerMeterCanvas', powerConfig._labels);
        }

        // Initialize SWR Meter (if element exists)
        if (document.getElementById('swrMeterCanvas')) {
            const swrConfig = makeGaugeConfig('swrMeterCanvas', 'swr');
            window.gaugeSWR = new RadialGauge(swrConfig);
            window.gaugeSWR.draw();
            createGaugeLabels('swrMeterCanvas', swrConfig._labels);
        }

        // Initialize ALC Meter (if element exists)
        if (document.getElementById('alcMeterCanvas')) {
            console.log('[Gauge] Initializing ALC meter...');
            const alcConfig = makeGaugeConfig('alcMeterCanvas', 'alc');
            console.log('[Gauge] ALC config:', alcConfig);
            window.gaugeALC = new RadialGauge(alcConfig);
            window.gaugeALC.draw();
            createGaugeLabels('alcMeterCanvas', alcConfig._labels);
            console.log('[Gauge] ALC meter initialized');
        } else {
            console.warn('[Gauge] alcMeterCanvas element not found!');
        }
    }



    // Attach AF Gain slider event listeners only once, when data is available
    function attachAfGainSliderListeners() {
        const sliderA = document.getElementById('afGainSliderA');
        const sliderB = document.getElementById('afGainSliderB');
        const labelA = document.getElementById('afGainValueA');
        const labelB = document.getElementById('afGainValueB');
        if (sliderA && !sliderA._afGainListenerAttached && labelA) {
            sliderA.addEventListener('input', function () {
                labelA.innerText = sliderA.value;
                updateAfGainFill('afGainSliderA');
                sendAfGain('A', parseInt(sliderA.value));
            });
            sliderA._afGainListenerAttached = true;
        }
        if (sliderB && !sliderB._afGainListenerAttached && labelB) {
            sliderB.addEventListener('input', function () {
                labelB.innerText = sliderB.value;
                updateAfGainFill('afGainSliderB');
                sendAfGain('B', parseInt(sliderB.value));
            });
            sliderB._afGainListenerAttached = true;
        }
    }

    // Kick everything off
    initializeGauges();
    initializeDigitInteraction('A');
    initializeDigitInteraction('B');
    ['A', 'B'].forEach(r => { const s = document.getElementById('powerSlider' + r); if (s) updateSliderFill(s); });
    fetchRadioStatus();
    state.pollingInterval = setInterval(fetchRadioStatus, 500);

    // Overwrite the interim window.radioControl with the real implementations
    window.radioControl = {
        setFrequency,
        setBand,
        setMode,
        setAntenna,
        _state: state,  // Expose state for TX indicator updates
        // Wrap updatePowerDisplay so we flag editingPower while the user is dragging
        updatePowerDisplay: (receiver, value) => {
            state.editingPower[receiver] = true;
            updatePowerDisplay(receiver, value);
        },
        setPower: async (receiver, value) => {
            state.editingPower[receiver] = true;
            await setPower(receiver, value);
            clearTimeout(state.editingPowerTimeout);
            state.editingPowerTimeout = setTimeout(() => {
                state.editingPower[receiver] = false;
                updatePowerDisplay(receiver, state.lastPower[receiver]);
            }, 1000);
        }
    };

    // Expose meter update functions globally for SignalR
    window.updatePowerMeter = updatePowerMeter;
    window.updateSWRMeter = updateSWRMeter;
    window.updateALCMeter = updateALCMeter;
    window.updateIDDMeter = updateIDDMeter;
    window.updatePAVoltage = updatePAVoltage;
    window.updateMICMeter = updateMICMeter;

})();

// ===========================================================================
// Second SignalR RadioStateUpdate handler (post-IIFE)
// ===========================================================================
connection.on("RadioStateUpdate", function (update) {
    if (update.property && update.value !== undefined) {
        if (update.property === "FrequencyA") {
            updateFrequencyDisplay('A', update.value);
        }
        if (update.property === "FrequencyB") {
            updateFrequencyDisplay('B', update.value);
        }
        // Mode dropdown updates
        if (update.property === "ModeA") {
            updateModeSelect('A', update.value);
        }
        if (update.property === "ModeB") {
            updateModeSelect('B', update.value);
        }
        // Band button updates
        if (update.property === "BandA") {
            updateBandButton('A', update.value);
        }
        if (update.property === "BandB") {
            updateBandButton('B', update.value);
        }
    }
});

connection.start().catch(function (err) {
    return console.error(err.toString());
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
