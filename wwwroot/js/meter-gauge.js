// <meter-gauge> Web Component
// Wraps Gauge, PowerGauge, SWRGauge, ALCGauge, SMeterGauge
// Auto-scales, creates canvas, supports type/value-label/bottom-label, exposes .value and applyConfig(), re-renders on resize

class MeterGauge extends HTMLElement {
    static get observedAttributes() {
        return ['type', 'value-label', 'bottom-label'];
    }

    constructor() {
        super();
        // Create a wrapper for relative positioning
        this.wrapper = document.createElement('div');
        this.wrapper.style.position = 'relative';
        this.wrapper.style.width = '100%';
        this.wrapper.style.height = '100%';
        this.canvas = document.createElement('canvas');
        // Assign a unique ID to the canvas for label overlay math
        this.canvas.id = 'meter-gauge-canvas-' + Math.random().toString(36).substr(2, 9);
        this.wrapper.appendChild(this.canvas);
        this.gauge = null;
        this._valueLabel = this.getAttribute('value-label') || '';
        this._bottomLabel = this.getAttribute('bottom-label') || '';
        this._value = 0;
        this._config = {};
        this.resizeObserver = new ResizeObserver(() => this.renderGauge());
    }

    connectedCallback() {
        this.resizeObserver.observe(this);
        // Only append the wrapper to the light DOM
        if (!this.contains(this.wrapper)) {
            this.appendChild(this.wrapper);
        }
    }

    disconnectedCallback() {
        this.resizeObserver.disconnect();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue !== newValue) {
            if (name === 'type') this._type = newValue;
            if (name === 'value-label') this._valueLabel = newValue;
            if (name === 'bottom-label') this._bottomLabel = newValue;
            this.renderGauge();
        }
    }

    set value(val) {
        this._value = val;
        if (this.gauge && typeof this.gauge.setValue === 'function') {
            this.gauge.setValue(val);
        } else {
            this.renderGauge();
        }
    }
    get value() { return this._value; }

    applyConfig(config) {
        this._config = config;
        this.renderGauge();
    }

    renderGauge() {
        // Set default size if not specified
        const width = this.clientWidth || 420;
        const height = this.clientHeight || 135;
        this.canvas.width = width;
        this.canvas.height = height;

        // Remove any previous gauge instance
        if (this.gauge && this.gauge.gauge && typeof this.gauge.gauge.destroy === 'function') {
            this.gauge.gauge.destroy();
        }

        // Remove any previous label overlay
        const oldOverlay = this.wrapper.querySelector('.gauge-labels-overlay');
        if (oldOverlay) oldOverlay.remove();

        // Choose the correct gauge class
        let GaugeClass = window.Gauge;
        let labels = [];
        switch ((this._type || '').toLowerCase()) {
            case 's-meter':
                GaugeClass = window.SMeterGauge;
                labels = ["0", "S1", "S3", "S5", "S7", "S9", "+20", "+40", "+60"];
                break;
            case 'power':
                GaugeClass = window.PowerGauge;
                labels = ["0", "25", "50", "75", "100", "125", "150", "175", "200"];
                break;
            case 'swr':
                GaugeClass = window.SWRGauge;
                labels = ["1.0", "1.3", "1.5", "1.7", "2.0", "2.3", "2.5", "2.7", "3.0"];
                break;
            case 'alc':
                GaugeClass = window.ALCGauge;
                labels = ["0", "6", "12", "19", "25", "31", "37", "44", "50"];
                break;
        }

        // Compose config
        const config = Object.assign({
            width,
            height,
            value: this._value,
            valueLabel: this._valueLabel,
            bottomLabel: this._bottomLabel,
            _labels: labels,
            suppressOverlayLabels: true // Prevent double overlay from gauge.js
        }, this._config);

        // Create and render the gauge
        this.gauge = new GaugeClass(this.canvas.id, config);
        if (typeof this.gauge.render === 'function') {
            this.gauge.render();
        }

        // Render gauge labels overlay in shadow DOM (math matches gauge.js)
        if (labels.length > 1) {
            const labelsDiv = document.createElement('div');
            labelsDiv.className = 'gauge-labels-overlay';
            labelsDiv.style.position = 'absolute';
            labelsDiv.style.top = '0';
            labelsDiv.style.left = '0';
            labelsDiv.style.width = '100%';
            labelsDiv.style.height = '100%';
            labelsDiv.style.pointerEvents = 'none';
            const centerX = width / 2;
            const centerY = height - 64;
            // Use a larger radius for non-S-Meter gauges
            let radius = width * 0.17;
            if ((this._type || '').toLowerCase() !== 's-meter') {
                radius = width * 0.23;
            }
            const angleStep = 180 / (labels.length - 1);
            labels.forEach((label, index) => {
                const angle = 180 - (angleStep * index);
                const radians = (angle * Math.PI) / 180;
                const x = centerX + radius * Math.cos(radians);
                const y = centerY - radius * Math.sin(radians);
                const span = document.createElement('span');
                span.className = 'gauge-label';
                span.textContent = label;
                span.style.position = 'absolute';
                span.style.fontSize = '12px';
                span.style.fontWeight = '600';
                span.style.color = '#333';
                span.style.transform = 'translate(-50%, -50%)';
                span.style.whiteSpace = 'nowrap';
                span.style.left = x + 'px';
                span.style.top = y + 'px';
                labelsDiv.appendChild(span);
            });
            this.wrapper.appendChild(labelsDiv);
        }
    }
}

customElements.define('meter-gauge', MeterGauge);
