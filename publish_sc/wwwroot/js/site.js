// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// --- Frequency digit interaction logic ---

// Global state objects
var selectedIdx = {};
var editing = {};
var localFreq = {};
var lastBackendFreq = {};

// Dummy helpers (replace with your real implementations)
function updateFrequencyDisplay(receiver, freq) {
    // Example: update the frequency display in the DOM
    // You should implement this to match your UI
    // document.getElementById('freq' + receiver).textContent = freq;
}
function setFrequency(receiver, freq) {
    // Example: send the new frequency to the backend
    // You should implement this to match your backend API
}
function changeSelectedDigit(receiver, delta) {
    // Example: change the selected digit by delta (+1 or -1)
    // You should implement this to match your UI logic
}
function isTouchDevice() {
    return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
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

    // Digit selection: pointerdown for both mouse and touch, prevent text selection
    display.addEventListener('pointerdown', function (e) {
        let digits = Array.from(display.querySelectorAll('.digit')).filter(d => d.textContent !== '.');
        let idx = -1;
        if (e.target.classList.contains('digit') && e.target.textContent !== '.') {
            idx = digits.indexOf(e.target);
        } else {
            // If not clicking a digit, select the nearest digit to the pointer
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
            selectedIdx[receiver] = idx;
            digits[idx].classList.add('selected');
            editing[receiver] = true;
            localFreq[receiver] = parseInt(digits.map(d => d.textContent).join(''));
            updateFrequencyDisplay(receiver, localFreq[receiver]);
        }
        e.preventDefault(); // Prevent text selection and long-press menu
    });

    // Mouse wheel to change digit (desktop)
    display.addEventListener('wheel', function (e) {
        let digits = Array.from(display.querySelectorAll('.digit')).filter(d => d.textContent !== '.');
        let idx = selectedIdx[receiver];
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
        localFreq[receiver] = newFreq;
        updateFrequencyDisplay(receiver, newFreq);
        clearTimeout(display._debounceTimer);
        display._debounceTimer = setTimeout(() => {
            setFrequency(receiver, newFreq);
        }, 200);
        e.preventDefault();
    }, { passive: false });

    // Tablet up/down controls
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

    // Mouseleave: exit editing mode and update from backend
    display.addEventListener('mouseleave', function () {
        if (editing[receiver]) {
            selectedIdx[receiver] = null;
            editing[receiver] = false;
            localFreq[receiver] = null;
            updateFrequencyDisplay(receiver, lastBackendFreq[receiver]);
        }
    });

    // Touch: exit editing on click-away
    document.addEventListener('pointerdown', function (e) {
        if (!display.contains(e.target) && (!controls || !controls.contains(e.target))) {
            if (editing[receiver]) {
                selectedIdx[receiver] = null;
                editing[receiver] = false;
                localFreq[receiver] = null;
                updateFrequencyDisplay(receiver, lastBackendFreq[receiver]);
            }
        }
    });
}

// On SignalR or polling update:
function updateVfoA(data) {
    document.getElementById('powerA').textContent = data.power;
    document.getElementById('modeA').textContent = data.mode;
    document.getElementById('antennaA').textContent = data.antenna;
    // etc.
}