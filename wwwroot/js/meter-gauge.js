// <meter-gauge> Web Component
// Wraps Gauge, PowerGauge, SWRGauge, ALCGauge, SMeterGauge
// Auto-scales, creates canvas, supports type/value-label/bottom-label, exposes .value and applyConfig(), re-renders on resize

class MeterGauge extends HTMLElement {
    static get observedAttributes() {
        return ['type', 'value-label', 'bottom-label'];
    }

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.canvas = document.createElement('canvas');
        this.shadowRoot.appendChild(this.canvas);
        this.gauge = null;
        this._type = this.getAttribute('type') || 'generic';
        this._valueLabel = this.getAttribute('value-label') || '';
        this._bottomLabel = this.getAttribute('bottom-label') || '';
        this._value = 0;
        this._config = {};
        this.resizeObserver = new ResizeObserver(() => this.renderGauge());
    }

    connectedCallback() {
        this.resizeObserver.observe(this);
        this.renderGauge();
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

        // Choose the correct gauge class
        let GaugeClass = window.Gauge;
        switch ((this._type || '').toLowerCase()) {
            case 's-meter':
                GaugeClass = window.SMeterGauge;
                break;
            case 'power':
                GaugeClass = window.PowerGauge;
                break;
            case 'swr':
                GaugeClass = window.SWRGauge;
                break;
            case 'alc':
                GaugeClass = window.ALCGauge;
                break;
        }

        // Compose config
        const config = Object.assign({
            width,
            height,
            value: this._value,
            valueLabel: this._valueLabel,
            bottomLabel: this._bottomLabel
        }, this._config);

        // Create and render the gauge
        this.gauge = new GaugeClass(this.canvas, config);
        if (typeof this.gauge.render === 'function') {
            this.gauge.render();
        }
    }
}

customElements.define('meter-gauge', MeterGauge);
