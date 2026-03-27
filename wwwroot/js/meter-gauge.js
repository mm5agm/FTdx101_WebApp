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
        const width = this.clientWidth || 200;
        const height = this.clientHeight || 120;
        this.canvas.width = width;
        this.canvas.height = height;
        // Placeholder: Replace with actual Gauge class logic
        const ctx = this.canvas.getContext('2d');
        ctx.clearRect(0, 0, width, height);
        ctx.fillStyle = '#222';
        ctx.fillRect(0, 0, width, height);
        ctx.fillStyle = '#fff';
        ctx.font = '16px sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText(this._type.toUpperCase(), width/2, 30);
        ctx.fillText(this._valueLabel, width/2, 60);
        ctx.fillText(this._bottomLabel, width/2, height-10);
        ctx.fillText(this._value, width/2, height/2);
        // TODO: Integrate with real Gauge/PowerGauge/etc. classes
    }
}

customElements.define('meter-gauge', MeterGauge);
