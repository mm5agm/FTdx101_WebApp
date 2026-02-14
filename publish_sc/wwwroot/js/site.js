// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// --- Frequency digit interaction logic ---}

// Global state objects
var selectedIdx = {};
var editing = {};
var localFreq = {};
var lastBackendFreq = {};

// Dummy helpers (replace with your real implementations)
function setFrequency(receiver, freq) {
    // Example: send the new frequency to the backend
    // You should implement this to match your backend API
}
function changeSelectedDigit(receiver, delta) {
    // Example: change the selected digit by delta (+1 or -1)
    // You should implement this to match your UI logic
}

// Improve touch detection for Samsung tablets and show a message on page load
function isTouchDevice() {
    // Use both maxTouchPoints and user agent for broader detection
    return navigator.maxTouchPoints > 0 ||
        /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Tablet|Samsung/i.test(navigator.userAgent);
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
            editing[receiver] = true;
            localFreq[receiver] = parseInt(digits.map(d => d.textContent).join(''));
            updateFrequencyDisplay(receiver, localFreq[receiver]); // <-- Ensure this is present
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

    // Add touch and click listeners for showing up/down buttons
    const digitElements = display.querySelectorAll('.digit');
    digitElements.forEach((digitElement, idx) => {
        digitElement.addEventListener('touchstart', showUpDownButtons);
        digitElement.addEventListener('click', showUpDownButtons);
    });
}

// --- Tablet/touch digit controls for frequency display ---


// 1. Render digits with + and - for touch devices
function renderFrequencyDigitsWithTouch(freq, selIdx, receiver) {
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
        if (selIdx === digitIdx && isTouchDevice()) {
            html += `
                <span class="digit-controls" style="display:inline-block;vertical-align:middle;text-align:center;">
                    <button type="button" class="digit-plus" data-idx="${digitIdx}" data-receiver="${receiver}" ontouchstart="startDigitChange('${receiver}',${digitIdx},1)" ontouchend="stopDigitChange()" onclick="startDigitChange('${receiver}',${digitIdx},1); stopDigitChange();" style="font-size:1.2em;">+</button>
                    <span class="digit selected" tabindex="0">${s[i]}</span>
                    <button type="button" class="digit-minus" data-idx="${digitIdx}" data-receiver="${receiver}" ontouchstart="startDigitChange('${receiver}',${digitIdx},-1)" ontouchend="stopDigitChange()" onclick="startDigitChange('${receiver}',${digitIdx},-1); stopDigitChange();" style="font-size:1.2em;">-</button>
                </span>
            `;
        } else {
            html += `<span class="digit${selIdx === digitIdx ? ' selected' : ''}" tabindex="0">${s[i]}</span>`;
        }
        digitIdx++;
    }
    return html;
}

// 2. Add touch/hold logic for increment/decrement
let digitChangeInterval = null;

function startDigitChange(receiver, idx, delta) {
    changeDigitValue(receiver, idx, delta);
    digitChangeInterval = setInterval(() => changeDigitValue(receiver, idx, delta), 500);
}

function stopDigitChange() {
    if (digitChangeInterval) clearInterval(digitChangeInterval);
    digitChangeInterval = null;
}

function changeDigitValue(receiver, idx, delta) {
    let freq = localFreq[receiver] || lastBackendFreq[receiver];
    let digits = freq.toString().padStart(8, "0").split("").map(Number);
    let newVal = digits[idx] + delta;
    if (newVal > 9) newVal = 0;
    if (newVal < 0) newVal = 9;
    digits[idx] = newVal;
    let newFreq = parseInt(digits.join(""));
    newFreq = Math.max(30000, Math.min(75000000, newFreq));
    localFreq[receiver] = newFreq;
    updateFrequencyDisplay(receiver, newFreq);
    clearTimeout(window['freqDebounce_' + receiver]);
    window['freqDebounce_' + receiver] = setTimeout(() => setFrequency(receiver, newFreq), 200);
}

// 3. Update updateFrequencyDisplay to use the new renderer for touch devices
// Replace your existing updateFrequencyDisplay function with this:
function updateFrequencyDisplay(receiver, freq) {
    const display = document.getElementById('freq' + receiver);
    if (!display) return;
    let selIdx = selectedIdx[receiver];
    // Use lastBackendFreq if freq is invalid
    if (!freq || isNaN(freq) || freq < 100) {
        freq = lastBackendFreq[receiver] || 30000000; // fallback to a valid frequency
    }
    if (isTouchDevice()) {
        display.innerHTML = renderFrequencyDigitsWithTouch(freq, selIdx, receiver);
    } else {
        display.innerHTML = renderFrequencyDigits(freq, selIdx);
    }
}

// Add this function if missing (for desktop rendering)
function renderFrequencyDigits(freq, selIdx) {
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
        html += `<span class="digit${selIdx === digitIdx ? ' selected' : ''}" tabindex="0">${s[i]}</span>`;
        digitIdx++;
    }
    return html;
}

// On SignalR or polling update:
function updateVfoA(data) {
    document.getElementById('powerA').textContent = data.power;
    document.getElementById('modeA').textContent = data.mode;
    document.getElementById('antennaA').textContent = data.antenna;
    // etc.
}

document.addEventListener('DOMContentLoaded', function() {
    initializeDigitInteraction('A');
    initializeDigitInteraction('B');
    // Try to show the message after DOM and frequency display are ready
    setTimeout(function() {
        if (isTouchDevice()) {
            showTabletMessage("Tablet mode active: Touch a digit to see + and - controls.");
        }
    }, 2000); // Increased delay to 2 seconds
});

// Permanently display a message overlay at the top of the screen for maximum visibility
(function() {
    var msgElem = document.createElement('div');
    msgElem.id = 'tabletMessage';
    msgElem.style.position = 'fixed';
    msgElem.style.top = '0';
    msgElem.style.left = '0';
    msgElem.style.width = '100vw';
    msgElem.style.background = '#fff';
    msgElem.style.color = '#d00';
    msgElem.style.fontSize = '2em';
    msgElem.style.fontWeight = 'bold';
    msgElem.style.textAlign = 'center';
    msgElem.style.zIndex = '99999';

    // Gather device info
    var screenW = window.screen.width;
    var screenH = window.screen.height;
    var innerW = window.innerWidth;
    var innerH = window.innerHeight;
    var dpr = window.devicePixelRatio;
    var touchPoints = navigator.maxTouchPoints;
    var msTouchPoints = navigator.msMaxTouchPoints || "n/a";
    var hasTouchStart = 'ontouchstart' in window;
    var pointerCoarse = window.matchMedia("(pointer: coarse)").matches;
    var userAgent = navigator.userAgent;
    var isTouch = typeof isTouchDevice === "function" ? isTouchDevice() : "n/a";

    msgElem.textContent =
        "Overlay Test: " +
        "Screen: " + screenW + "x" + screenH +
        " | Window: " + innerW + "x" + innerH +
        " | DPR: " + dpr +
        " | TouchPoints: " + touchPoints +
        " | msMaxTouchPoints: " + msTouchPoints +
        " | ontouchstart: " + hasTouchStart +
        " | pointer:coarse: " + pointerCoarse +
        " | TouchEnabled: " + isTouch +
        " | UA: " + userAgent;

    document.body.appendChild(msgElem);
})();