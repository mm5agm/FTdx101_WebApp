// FTdx101 WebApp – On-screen Frequency Keyboard
// Floating, draggable, resizable <dialog> for entering VFO frequencies.
// Position and size persist to localStorage across sessions.

const STORAGE_KEY  = 'freqKeyboard.layout';
const MIN_FREQ_HZ  = 30_000;
const MAX_FREQ_HZ  = 75_000_000;

// ── State ────────────────────────────────────────────────────────────────────

let _receiver = 'A';
let _digits   = Array(8).fill('0');  // [d0..d7] → display as d0d1.d2d3d4d5d6d7 MHz
let _cursor   = 0;                   // 0–7, current digit position

// ── DOM refs ─────────────────────────────────────────────────────────────────

let _dialog, _title, _display, _error;

// ── Frequency helpers ─────────────────────────────────────────────────────────

function hzToDigits(hz) {
    hz = Math.max(MIN_FREQ_HZ, Math.min(MAX_FREQ_HZ, Math.round(hz)));
    return String(hz).padStart(8, '0').split('');
}

function digitsToHz() {
    return parseInt(_digits.join(''), 10);
}

// ── Display ───────────────────────────────────────────────────────────────────

function renderDisplay() {
    let html = '';
    for (let i = 0; i < 8; i++) {
        if (i === 2) html += '<span class="fkb-decimal">.</span>';
        const cls = i === _cursor ? 'fkb-digit fkb-digit-cursor' : 'fkb-digit';
        html += `<span class="${cls}">${_digits[i]}</span>`;
    }
    _display.innerHTML = html;

    const mhz = `${_digits[0]}${_digits[1]}.${_digits[2]}${_digits[3]}${_digits[4]}${_digits[5]}${_digits[6]}${_digits[7]}`;
    _display.setAttribute('aria-label', `Entered frequency ${mhz} megahertz`);
}

function clearError()    { _error.textContent = ''; }
function showError(msg)  { _error.textContent = msg; }

// ── Key handlers ──────────────────────────────────────────────────────────────

function pressDigit(d) {
    clearError();
    _digits[_cursor] = String(d);
    if (_cursor < 7) _cursor++;
    renderDisplay();
}

function moveCursor(delta) {
    _cursor = Math.max(0, Math.min(7, _cursor + delta));
    renderDisplay();
}

function pressClear() {
    clearError();
    _digits = Array(8).fill('0');
    _cursor = 0;
    renderDisplay();
}

function pressBackspace() {
    clearError();
    _digits[_cursor] = '0';
    if (_cursor > 0) _cursor--;
    renderDisplay();
}

async function pressEnter() {
    clearError();
    const hz = digitsToHz();
    if (hz < MIN_FREQ_HZ || hz > MAX_FREQ_HZ) {
        showError(`Must be between 0.030 and 75.000 MHz`);
        return;
    }
    if (window.radioControl?.setFrequency) {
        await window.radioControl.setFrequency(_receiver, hz);
    }
    closeKeyboard();
}

// ── Open / close ──────────────────────────────────────────────────────────────

export function openKeyboard(receiver) {
    _receiver = receiver;
    _title.textContent = `VFO ${receiver} Frequency`;
    clearError();

    const hz = window.radioControl?._state?.lastBackendFreq?.[receiver];
    _digits = hz ? hzToDigits(hz) : Array(8).fill('0');
    _cursor = 0;
    renderDisplay();

    applySavedLayout();
    _dialog.showModal();
    _dialog.querySelector('.fkb-num')?.focus();
}

function closeKeyboard() {
    _dialog.close();
}

// ── Drag ──────────────────────────────────────────────────────────────────────

function initDrag(header) {
    header.addEventListener('mousedown', e => {
        if (e.target.closest('.fkb-close')) return;

        const rect  = _dialog.getBoundingClientRect();
        const origX = e.clientX;
        const origY = e.clientY;
        let   left  = rect.left;
        let   top   = rect.top;

        _dialog.style.margin = '0';
        _dialog.style.left   = `${left}px`;
        _dialog.style.top    = `${top}px`;

        function onMove(ev) {
            left = rect.left + (ev.clientX - origX);
            top  = rect.top  + (ev.clientY - origY);
            // Keep at least 40 px of the header on-screen
            left = Math.max(-rect.width  + 40, Math.min(window.innerWidth  - 40, left));
            top  = Math.max(0,                 Math.min(window.innerHeight - 40, top));
            _dialog.style.left = `${left}px`;
            _dialog.style.top  = `${top}px`;
        }

        function onUp() {
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup',   onUp);
            saveLayout();
        }

        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup',   onUp);
        e.preventDefault();
    });
}

// ── Resize persistence ────────────────────────────────────────────────────────

function saveLayout() {
    const s = _dialog.style;
    localStorage.setItem(STORAGE_KEY, JSON.stringify({
        left:   s.left   || '',
        top:    s.top    || '',
        width:  s.width  || '',
        height: s.height || ''
    }));
}

function applySavedLayout() {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return;
        const { left, top, width, height } = JSON.parse(raw);

        // Clamp restored position to viewport so it can't appear off-screen
        if (left || top) {
            const w = width  ? parseInt(width,  10) : 300;
            const h = height ? parseInt(height, 10) : 380;
            const l = left   ? parseInt(left,   10) : 0;
            const t = top    ? parseInt(top,    10) : 0;
            _dialog.style.left   = `${Math.max(0, Math.min(window.innerWidth  - w, l))}px`;
            _dialog.style.top    = `${Math.max(0, Math.min(window.innerHeight - h, t))}px`;
            _dialog.style.margin = '0';
        }
        if (width)  _dialog.style.width  = width;
        if (height) _dialog.style.height = height;
    } catch { /* ignore corrupt data */ }
}

function initResizeObserver() {
    if (!window.ResizeObserver) return;
    new ResizeObserver(() => {
        // Only save if we have an explicit position (i.e. user has moved/resized it)
        if (_dialog.style.left) saveLayout();
    }).observe(_dialog);
}

// ── Init ──────────────────────────────────────────────────────────────────────

export function initFreqKeyboard() {
    _dialog  = document.getElementById('freqKeyboardDialog');
    _title   = document.getElementById('freqKeyboardTitle');
    _display = document.getElementById('freqKeyboardDisplay');
    _error   = document.getElementById('freqKeyboardError');

    if (!_dialog) return;

    initDrag(document.getElementById('freqKeyboardHeader'));
    initResizeObserver();

    document.getElementById('freqKeyboardClose').addEventListener('click', closeKeyboard);

    // Open buttons (VFO A and VFO B)
    document.querySelectorAll('[data-freq-keyboard]').forEach(btn => {
        btn.addEventListener('click', () => openKeyboard(btn.dataset.freqKeyboard));
    });

    // Number keys
    _dialog.querySelectorAll('.fkb-num').forEach(btn => {
        btn.addEventListener('click', () => pressDigit(btn.dataset.digit));
    });

    document.getElementById('freqKbLeft').addEventListener('click',      () => moveCursor(-1));
    document.getElementById('freqKbRight').addEventListener('click',     () => moveCursor(1));
    document.getElementById('freqKbBackspace').addEventListener('click', pressBackspace);
    document.getElementById('freqKbClear').addEventListener('click',     pressClear);
    document.getElementById('freqKbEnter').addEventListener('click',     pressEnter);

    // Close on backdrop click
    _dialog.addEventListener('click', e => { if (e.target === _dialog) closeKeyboard(); });
}
