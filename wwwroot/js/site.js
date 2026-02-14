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

window.setMode = async function (receiver, mode) {
    try {
        console.log(`Setting ${receiver} mode to ${mode}`);
        if (window.highlightButtons) highlightButtons(receiver, state.lastBand ? state.lastBand[receiver] : undefined, mode, state.lastAntenna ? state.lastAntenna[receiver] : undefined);
        if (state.lastMode) state.lastMode[receiver] = mode;
        const response = await fetch(`/api/cat/mode/${receiver.toLowerCase()}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ mode })
        });
        if (!response.ok) {
            console.error('Failed to set mode:', await response.text());
        } else {
            console.log(`Mode set successfully: ${receiver} -> ${mode}`);
        }
    } catch (error) {
        console.error('Error setting mode:', error);
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

connection.on("ShowSettingsPage", () => {
    window.location.href = "/Settings";
});

connection.on("RadioStateUpdate", function (update) {
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