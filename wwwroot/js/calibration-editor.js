// calibration-editor.js
// Web Component for editing meter calibration

class CalibrationEditor extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.innerHTML = `
            <style>
                .cal-editor { display: flex; flex-direction: column; gap: 1rem; max-width: 400px; }
                .cal-row { display: flex; gap: 0.5rem; align-items: center; }
                .cal-row label { min-width: 90px; }
                .cal-row input, .cal-row select, .cal-row textarea { flex: 1; }
                .cal-row textarea { font-family: monospace; min-height: 60px; }
                .error { color: #b00; font-size: 0.95em; }
                button { margin-top: 0.5rem; }
            </style>
            <div class="cal-editor">
                <div class="cal-row">
                    <label for="gaugeType">Gauge Type</label>
                    <select id="gaugeType">
                        <option value="s-meter">S-Meter</option>
                        <option value="power">Power</option>
                        <option value="swr">SWR</option>
                        <option value="alc">ALC</option>
                    </select>
                </div>
                <div class="cal-row">
                    <label for="maxValue">Max Value</label>
                    <input type="number" id="maxValue" min="1" value="100">
                </div>
                <div class="cal-row">
                    <label for="majorTicks">Major Ticks</label>
                    <input type="text" id="majorTicks" placeholder="e.g. 0,20,40,60,80,100">
                </div>
                <div class="cal-row">
                    <label for="highlights">Highlights JSON</label>
                    <textarea id="highlights" placeholder='[{"from":0,"to":20,"color":"#f00"}]'></textarea>
                </div>
                <div class="cal-row">
                    <span id="errorMsg" class="error"></span>
                </div>
                <button id="applyBtn">Apply</button>
            </div>
        `;
    }
    connectedCallback() {
        this.shadowRoot.getElementById('applyBtn').addEventListener('click', () => this.applyConfig());
    }
    applyConfig() {
        const type = this.shadowRoot.getElementById('gaugeType').value;
        const maxValue = parseFloat(this.shadowRoot.getElementById('maxValue').value);
        const majorTicks = this.shadowRoot.getElementById('majorTicks').value.split(',').map(s => parseFloat(s.trim())).filter(n => !isNaN(n));
        let highlights;
        try {
            highlights = JSON.parse(this.shadowRoot.getElementById('highlights').value);
            this.shadowRoot.getElementById('errorMsg').textContent = '';
        } catch (e) {
            this.shadowRoot.getElementById('errorMsg').textContent = 'Invalid JSON in highlights.';
            return;
        }
        // Find the target meter-gauge
        const gauge = document.querySelector(`meter-gauge[type='${type}']`);
        if (gauge) {
            gauge.applyConfig({ maxValue, majorTicks, highlights });
        }
    }
}

customElements.define('calibration-editor', CalibrationEditor);
