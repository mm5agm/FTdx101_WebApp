// Loads accessible labels from /i18n/labels.json and applies them to
// any element that has a data-a11y-key attribute.
// Users can override labels by editing the file (or the AppData copy).

let _labels = null;

export async function loadLabels() {
    try {
        const resp = await fetch('/i18n/labels.json');
        if (resp.ok) _labels = await resp.json();
    } catch {
        // Non-fatal — built-in aria-labels remain as fallback.
    }
    applyAll();
}

function resolveKey(key) {
    if (!_labels) return null;
    const parts = key.split('.');
    let node = _labels;
    for (const part of parts) {
        if (node == null || typeof node !== 'object') return null;
        node = node[part];
    }
    return typeof node === 'string' ? node : null;
}

function applyAll() {
    document.querySelectorAll('[data-a11y-key]').forEach(el => {
        const label = resolveKey(el.dataset.a11yKey);
        if (label) el.setAttribute('aria-label', label);
    });
}
