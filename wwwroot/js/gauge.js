// --- Dynamic config builder for PowerGauge (and optionally others) ---
function makePowerGaugeConfig(options = {}) {
    // Detect radio model if available
    let model = (window.radioControl && window.radioControl._state && window.radioControl._state.radioModel)
        ? window.radioControl._state.radioModel.toLowerCase() : "ftdx101mp";
    let maxValue = model === "ftdx101d" ? 100 : 200;
    let majorTicks = model === "ftdx101d"
        ? ["0", "13", "25", "38", "50", "63", "75", "88", "100"]
        : ["0", "25", "50", "75", "100", "125", "150", "175", "200"];
    let highlights = model === "ftdx101d"
        ? [
            { from: 0, to: 75, color: "rgba(0,255,0,.25)" },
            { from: 75, to: 88, color: "rgba(255,255,0,.25)" },
            { from: 88, to: 100, color: "rgba(255,0,0,.25)" }
        ]
        : [
            { from: 0, to: 150, color: "rgba(0,255,0,.25)" },
            { from: 150, to: 175, color: "rgba(255,255,0,.25)" },
            { from: 175, to: 200, color: "rgba(255,0,0,.25)" }
        ];
    let labels = majorTicks;
    return Object.assign({
        minValue: 0,
        maxValue,
        majorTicks,
        highlights,
        labels,
        _labels: labels,
        colorPlate: "#ffffff"
    }, options);
}

// gauge.js - Base and derived classes for all meter gauges
// Requires: canvas-gauge library loaded globally (RadialGauge)

class Gauge {
    constructor(canvasId, config) {
        // Only set defaults for properties not provided in config
        const defaultConfig = {
            renderTo: canvasId,
            width: 420,
            height: 135,
            startAngle: 90,
            ticksAngle: 180,
            valueBox: false,
            minorTicks: 0,
            strokeTicks: false,
            tickSide: "out",
            needleSide: "center",
            colorPlate: "transparent",
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
        };
        // Do NOT override minValue, maxValue, majorTicks, labels, highlights if provided
        this.canvasId = canvasId;
        this.config = Object.assign({}, defaultConfig, config);
        this.gauge = null;
    }

    render() {
        if (!window.RadialGauge) {
            console.error('RadialGauge library not loaded.');
            return;
        }
        // Always enforce half-arc (180°) for all meters
        this.config.startAngle = 90;
        this.config.ticksAngle = 180;
        this.gauge = new RadialGauge(this.config);
        this.gauge.draw();
        this.createLabels();
    }

    createLabels() {
        // Suppress overlay labels if requested (used by MeterGauge to avoid double overlays)
        if (this.config.suppressOverlayLabels) return;
        // Overlay readable labels on top of the canvas gauge
        const canvas = document.getElementById(this.canvasId);
        if (!canvas || canvas.nextElementSibling?.classList.contains('gauge-labels-overlay')) {
            return;
        }
        const wrapper = document.createElement('div');
        wrapper.className = 'gauge-wrapper';
        wrapper.style.cssText = `position:relative;display:block;width:${this.config.width}px;height:${this.config.height}px;margin-left:0`;
        const labelsDiv = document.createElement('div');
        labelsDiv.className = 'gauge-labels-overlay';
        const centerX = this.config.width / 2;
        const centerY = this.config.height - 64;
        const radius = this.config.width * 0.17;
        const labels = this.config._labels || [];
        const angleStep = 180 / (labels.length - 1);
        labels.forEach((label, index) => {
            const angle = 180 - (angleStep * index);
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
}

class SMeterGauge extends Gauge {
    constructor(canvasId, options = {}) {
        super(canvasId, Object.assign({
            minValue: 0,
            maxValue: 255,
            majorTicks: ["0", "4", "30", "65", "95", "130", "171", "212", "255"],
            highlights: [
                { from: 0, to: 130, color: "rgba(0,255,0,.25)" },
                { from: 130, to: 255, color: "rgba(255,0,0,.25)" }
            ],
            labels: ["0", "S1", "S3", "S5", "S7", "S9", "+20", "+40", "+60"],
            _labels: ["0", "S1", "S3", "S5", "S7", "S9", "+20", "+40", "+60"]
        }, options));
    }
}

class PowerGauge extends Gauge {
    constructor(canvasId, options = {}) {
        super(canvasId, makePowerGaugeConfig(options));
    }
}

class SWRGauge extends Gauge {
    constructor(canvasId, options = {}) {
        super(canvasId, Object.assign({
            colorMajorTicks: "transparent",
            majorTicks: [],
            colorNumbers: "transparent",
            fontNumbersSize: 0,
            colorPlate: "transparent",
            colorTitle: "transparent",
            highlights: [
                { from: 0, to: 85, color: "rgba(0,255,0,.25)" },
                { from: 85, to: 128, color: "rgba(255,255,0,.25)" },
                { from: 128, to: 255, color: "rgba(255,0,0,.25)" }
            ],
            _labels: ["1.0", "1.3", "1.5", "1.7", "2.0", "2.3", "2.5", "2.7", "3.0"]
        }, options));
    }
}

class ALCGauge extends Gauge {
    constructor(canvasId, options = {}) {
        super(canvasId, Object.assign({
            colorMajorTicks: "transparent",
            majorTicks: [],
            colorNumbers: "transparent",
            fontNumbersSize: 0,
            colorPlate: "transparent",
            colorTitle: "transparent",
            highlights: [
                { from: 0, to: 178, color: "rgba(0,255,0,.25)" },
                { from: 178, to: 230, color: "rgba(255,255,0,.25)" },
                { from: 230, to: 255, color: "rgba(255,0,0,.25)" }
            ],
            _labels: ["0", "6", "12", "19", "25", "31", "37", "44", "50"]
        }, options));
    }
}

// Export classes to window for use in inline scripts
window.Gauge = Gauge;
window.SMeterGauge = SMeterGauge;
window.PowerGauge = PowerGauge;
window.SWRGauge = SWRGauge;
window.ALCGauge = ALCGauge;
