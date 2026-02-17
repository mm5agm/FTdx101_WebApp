// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// --- Frequency digit interaction logic ---

// Unified global state object
const state = {
    selectedIdx: { A: null, B: null },
    editing: { A: false, B: false },
    localFreq: { A: null, B: null },
    lastBackendFreq: { A: null, B: null }
};

// Frequency display renderer
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
    // Show dashes for frequencies less than 100 Hz
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

// Main interaction initializer
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

const modeToCatCode = {
    "LSB": "1",
    "USB": "2",
    "CW-U": "3",
    "FM": "4",
    "AM": "5",
    "RTTY-L": "6",
    "CW-L": "7",
    "DATA-L": "8",
    "RTTY-U": "9",
    "DATA-FM": "A",
    "FM-N": "B",
    "DATA-U": "C",
    "AM-N": "D",
    "PSK": "E",
    "DATA-FM-N": "F"
};

window.setMode = async function (receiver, mode) {
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

async function setPower(receiver, watts) {
    const maxPower = state.radioModel === 'FTdx101MP' ? 200 : 100;
    const power = Math.max(5, Math.min(parseInt(watts), maxPower));
    try {
        console.log(`Setting ${receiver} power to ${power}W`);
        const response = await fetch(`/api/cat/power/${receiver.toLowerCase()}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Watts: power }) // Only Watts, not Power
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
};

window.updatePowerDisplay = function () {
    console.warn("updatePowerDisplay not implemented");
};

async function fetchRadioStatus() {
    try {
        const response = await fetch('/api/cat/status');
        if (!response.ok) return;
        const data = await response.json();

        state.lastBackendFreq.A = data.vfoA.frequency;
        state.lastBackendFreq.B = data.vfoB.frequency;

        console.log('Fetched frequencies:', state.lastBackendFreq.A, state.lastBackendFreq.B);

        updateFrequencyDisplay('A', state.lastBackendFreq.A);
        updateFrequencyDisplay('B', state.lastBackendFreq.B);
    } catch (error) {
        console.error('Error fetching radio status:', error);
    }
}

// SignalR connection for backend events
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/radioHub")
    .build();

connection.on("ShowSettingsPage", function () {
         window.location.href = "/Settings";
     });

connection.on("RadioStateUpdate", function (update) {
    if (update.property === "ModeA") {
        // Update the mode display for VFO A
        document.getElementById("modeDisplayA").innerText = update.value;
    }
    if (update.property === "ModeB") {
        // Update the mode display for VFO B
        document.getElementById("modeDisplayB").innerText = update.value;
    }
    if (update.property === "FrequencyA") {
        state.lastBackendFreq.A = update.value;
        updateFrequencyDisplay('A', update.value);
    }
    if (update.property === "FrequencyB") {
        state.lastBackendFreq.B = update.value;
        updateFrequencyDisplay('B', update.value);
    }
});

connection.start();

// Initialization status polling and overlay logic
async function pollInitStatus() {
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
        } else if (data.status === "error") {
            statusText.innerText = "Radio initialization failed. Please check connection and restart.";
            overlay.style.display = "block";
            console.log("Redirecting to /Settings due to error status");
            window.location.href = "/Settings";
        } else {
            overlay.style.display = "block";
        }

        if (data.status !== "complete") {
            setTimeout(pollInitStatus, 1000); // poll again in 1s
        }
    } catch (error) {
        console.error('Error polling init status:', error);
        setTimeout(pollInitStatus, 2000);
    }
}

// Touch device detection
function isTouchDevice() {
    return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
}

window.radioControl = {
    setBand: window.setBand,
    setMode: window.setMode,
    setAntenna: window.setAntenna,
    setPower: window.setPower,
    updatePowerDisplay: window.updatePowerDisplay
};

async function updateBandButtonsFromBackend() {
    try {
        const response = await fetch('/api/cat/status');
        if (!response.ok) return;
        const data = await response.json();
        // Update band A
        if (data.vfoA && data.vfoA.band) {
            document.querySelectorAll('input[name="band-A"]').forEach(radio => {
                radio.checked = (radio.value.toLowerCase() === data.vfoA.band.toLowerCase());
            });
        }
        // Update band B
        if (data.vfoB && data.vfoB.band) {
            document.querySelectorAll('input[name="band-B"]').forEach(radio => {
                radio.checked = (radio.value.toLowerCase() === data.vfoB.band.toLowerCase());
            });
        }
    } catch (error) {
        console.error('Error updating band buttons:', error);
    }
}

// Start polling and UI initialization on page load
window.addEventListener('DOMContentLoaded', () => {
    pollInitStatus();
    fetchRadioStatus().then(() => {
        updateFrequencyDisplay('A', state.lastBackendFreq.A);
        updateFrequencyDisplay('B', state.lastBackendFreq.B);
        initializeDigitInteraction('A');
        initializeDigitInteraction('B');
        updateBandButtonsFromBackend();
    });
});

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
    // Optionally send the new frequency to the backend
    const displayElem = document.getElementById('freq' + receiver);
    clearTimeout(displayElem._debounceTimer);
    displayElem._debounceTimer = setTimeout(() => {
        setFrequency(receiver, newFreq);
    }, 200);
}
(function () {
    'use strict';

    console.log('=== FTdx101 Control Interface Starting ===');

    // Gauge instances
    let gaugeA, gaugeB;

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
        operationInProgress: false
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
        let freqToShow = (state.editing[receiver] && state.localFreq[receiver] !== null) ? state.localFreq[receiver] : freqHz;
        display.innerHTML = renderFrequencyDigits(freqToShow, selIdx);
    }

    function highlightButtons(receiver, band, mode, antenna) {
        document.querySelectorAll(`.band-btn[data-receiver="${receiver}"]`).forEach(btn => {
            btn.classList.toggle('active', btn.getAttribute('data-value') === band);
        });
        document.querySelectorAll(`.mode-btn[data-receiver="${receiver}"]`).forEach(btn => {
            btn.classList.toggle('active', btn.getAttribute('data-value') === mode);
        });
        document.querySelectorAll(`.antenna-btn[data-receiver="${receiver}"]`).forEach(btn => {
            btn.classList.toggle('active', btn.getAttribute('data-value') === antenna);
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
        pausePolling();
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
            resumePolling();
        }
    }

    function pausePolling() {
        if (state.pollingInterval && !state.operationInProgress) {
            state.operationInProgress = true;
            console.log('Polling paused for operation');
        }
    }

    function resumePolling() {
        if (state.operationInProgress) {
            state.operationInProgress = false;
            setTimeout(fetchRadioStatus, 500);
            console.log('Polling resumed');
        }
    }

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
            state.lastBand.A = data.vfoA.band;
            state.lastBand.B = data.vfoB.band;
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
            highlightButtons('A', data.vfoA.band, data.vfoA.mode, data.vfoA.antenna);
            highlightButtons('B', data.vfoB.band, data.vfoB.mode, data.vfoB.antenna);
        } catch (error) {
            console.error('Error fetching radio status:', error);
        }
    }

    // Power control functions
    function updatePowerDisplay(receiver, watts) {
        const display = document.getElementById('powerValue' + receiver);
        if (display) {
            display.textContent = watts + 'W';
        }
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
            return; // Don't update while user is editing
        }
        const slider = document.getElementById('powerSlider' + receiver);
        if (slider) {
            slider.value = watts;
        }
        updatePowerDisplay(receiver, watts);
    }

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

    // S-Meter rendering (simplified for brevity)
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
        const valueSpan = document.getElementById('sMeterValue' + receiver);
        const rawSpan = document.getElementById('sMeterRaw' + receiver);

        if (valueSpan) valueSpan.textContent = sMeterLabel(value);
        if (rawSpan) rawSpan.textContent = `[${value}]`;

        if (receiver === 'A' && gaugeA) {
            gaugeA.value = value;
            gaugeA.draw();
        } else if (receiver === 'B' && gaugeB) {
            gaugeB.value = value;
            gaugeB.draw();
        }
    }

    // S-Meter Gauge Initialization
    function initializeGauges() {
        const gaugeWidth = 560;
        const gaugeHeight = 180;

        function createGaugeLabels(canvasId, labels) {
            const canvas = document.getElementById(canvasId);
            if (!canvas || canvas.nextElementSibling?.classList.contains('gauge-labels-overlay')) {
                return;
            }

            const wrapper = document.createElement('div');
            wrapper.className = 'gauge-wrapper';
            wrapper.style.position = 'relative';
            wrapper.style.display = 'inline-block';
            wrapper.style.width = gaugeWidth + 'px';
            wrapper.style.height = gaugeHeight + 'px';

            const labelsDiv = document.createElement('div');
            labelsDiv.className = 'gauge-labels-overlay';

            const centerX = gaugeWidth / 2;
            const centerY = gaugeHeight - 85;
            const radius = gaugeWidth * 0.17;
            const startAngle = 180;
            const endAngle = 0;
            const angleStep = (startAngle - endAngle) / (labels.length - 1);

            labels.forEach((label, index) => {
                const angle = startAngle - (angleStep * index);
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
        }

        gaugeA = new RadialGauge({
            renderTo: 'sMeterCanvasA',
            width: gaugeWidth,
            height: gaugeHeight,
            units: "",
            minValue: 0,
            maxValue: 255,
            startAngle: 90,
            ticksAngle: 180,
            valueBox: false,
            majorTicks: ["0", "4", "30", "65", "95", "130", "171", "212", "255"],
            minorTicks: 0,
            strokeTicks: false,
            tickSide: "out",
            needleSide: "center",
            highlights: [
                { from: 0, to: 130, color: "rgba(0,255,0,.25)" },
                { from: 130, to: 255, color: "rgba(255,0,0,.25)" }
            ],
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
            value: 0
        });
        gaugeA.draw();

        const labels = ["0", "S1", "S3", "S5", "S7", "S9", "+20", "+40", "+60"];
        createGaugeLabels('sMeterCanvasA', labels);

        gaugeB = new RadialGauge({
            renderTo: 'sMeterCanvasB',
            width: gaugeWidth,
            height: gaugeHeight,
            units: "",
            minValue: 0,
            maxValue: 255,
            startAngle: 90,
            ticksAngle: 180,
            valueBox: false,
            majorTicks: ["0", "4", "30", "65", "95", "130", "171", "212", "255"],
            minorTicks: 0,
            strokeTicks: false,
            tickSide: "out",
            needleSide: "center",
            highlights: [
                { from: 0, to: 130, color: "rgba(0,255,0,.25)" },
                { from: 130, to: 255, color: "rgba(255,0,0,.25)" }
            ],
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
            value: 0
        });
        gaugeB.draw();

        createGaugeLabels('sMeterCanvasB', labels);
    }

    // Call initialization
    initializeGauges();
    initializeDigitInteraction('A');
    initializeDigitInteraction('B');
    fetchRadioStatus();
    state.pollingInterval = setInterval(fetchRadioStatus, 500);

    window.radioControl = {
        setFrequency,
        setBand,
        setMode,
        setAntenna,
        updatePowerDisplay: (receiver, value) => {
            state.editingPower[receiver] = true;
            updatePowerDisplay(receiver, value);
        },
        setPower: async (receiver, value) => {
            state.editingPower[receiver] = true;
            setPower(receiver, value);
            clearTimeout(state.editingPowerTimeout);
            state.editingPowerTimeout = setTimeout(() => {
                state.editingPower[receiver] = false;
                updatePowerDisplay(receiver, state.lastPower[receiver]);
            }, 1000);
        }
    };
})();

// SignalR integration for live updates


connection.on("RadioStateUpdate", function (update) {
    if (update.property && update.value !== undefined) {
        if (update.property === "FrequencyA") {
            updateFrequencyDisplay('A', update.value);
        }
        if (update.property === "FrequencyB") {
            updateFrequencyDisplay('B', update.value);
        }
        // Add more property handlers as needed
    }
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

async function pollInitStatus() {
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
        } else if (data.status === "error") {
            statusText.innerText = "Radio initialization failed. Please check connection and restart.";
            overlay.style.display = "block";
        } else {
            overlay.style.display = "block";
        }

        if (data.status !== "complete") {
            setTimeout(pollInitStatus, 1000); // poll again in 1s
        }
    } catch (error) {
        console.error('Error polling init status:', error);
        setTimeout(pollInitStatus, 2000);
    }
}
document.addEventListener('DOMContentLoaded', function () {
    if ('ontouchstart' in window || navigator.maxTouchPoints > 0) {
        var aControls = document.getElementById('freqA-controls');
        var bControls = document.getElementById('freqB-controls');
        if (aControls) aControls.style.display = '';
        if (bControls) bControls.style.display = '';
    }
});
pollInitStatus();