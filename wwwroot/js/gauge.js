// gauge.js - Base and derived classes for all meter gauges
// Requires: canvas-gauge library loaded globally (RadialGauge)

class Gauge {
    static defaultWidth = 220;
    static defaultHeight = 135;

    constructor(canvasId, config) {
        this.canvasId = canvasId;
        // Use defaults if not provided
        this.config = Object.assign({
            width: Gauge.defaultWidth,
            height: Gauge.defaultHeight
        }, config);
        this.gauge = null;
    }

    render() {
        if (!window.RadialGauge) {
            return;
        }

        // Set canvas size to match config
        const canvas = document.getElementById(this.canvasId);
        if (canvas) {
            canvas.width = this.config.width;
            canvas.height = this.config.height;
            canvas.style.width = this.config.width + 'px';
            canvas.style.height = this.config.height + 'px';
        }

        // Create gauge
        this.gauge = new RadialGauge(this.config);
        this.gauge.draw();

        // Create overlay labels
        this.createLabels();
    }

    createLabels() {
        const canvas = document.getElementById(this.canvasId);
        if (!canvas) return;

        const wrapper = canvas.parentNode;
        wrapper.style.position = "relative";
        wrapper.style.width = this.config.width + 'px';
        wrapper.style.height = this.config.height + 'px';

        // Remove any existing overlay (prevents stale labels)
        const existing = wrapper.querySelector('.gauge-labels-overlay');
        if (existing) existing.remove();

        // Overlay container
        const labelsDiv = document.createElement('div');
        labelsDiv.className = 'gauge-labels-overlay';
        labelsDiv.style.position = 'absolute';
        labelsDiv.style.left = '0';
        labelsDiv.style.top = '0';
        labelsDiv.style.width = this.config.width + 'px';
        labelsDiv.style.height = this.config.height + 'px';
        labelsDiv.style.pointerEvents = 'none';

        // Use ONLY your own labels
        const labels = this.config.labels || [];

        // Place labels in an arc above the gauge
        const centerX = this.config.width / 2;
        const centerY = this.config.height / 2 + 10; // slightly above center
        const radius = this.config.width * 0.38; // arc radius

        const angleStep = 180 / (labels.length - 1);

        labels.forEach((label, index) => {
            const angle = 180 - (angleStep * index);
            const radians = angle * Math.PI / 180;

            const x = centerX + radius * Math.cos(radians);
            const y = centerY - radius * Math.sin(radians);

            const span = document.createElement('span');
            span.className = 'gauge-label';
            span.textContent = label;
            span.style.position = 'absolute';
            span.style.left = `${x}px`;
            span.style.top = `${y}px`;
            span.style.transform = 'translate(-50%, -50%)';
            span.style.fontSize = '0.8rem';
            span.style.fontWeight = 'bold';
            span.style.color = '#333';
            labelsDiv.appendChild(span);
        });

        wrapper.appendChild(labelsDiv);
    }
}

// ------------------------------------------------------------
// S-METER
// ------------------------------------------------------------

class SMeterGauge extends Gauge {
    constructor(canvasId, options = {}) {
        const config = Object.assign({
            renderTo: canvasId,
            minValue: 0,
            maxValue: 255,
            majorTicks: ["0", "4", "30", "65", "95", "130", "171", "212", "255"],
            highlights: [
                { from: 0, to: 130, color: "rgba(0,255,0,.25)" },
                { from: 130, to: 255, color: "rgba(255,0,0,.25)" }
            ],
            labels: ["S0", "S1", "S3", "S5", "S7", "S9", "+20", "+40", "+60"],
            startAngle: 90,
            ticksAngle: 180,
            valueBox: false,
            minorTicks: 0,
            strokeTicks: false,
            tickSide: "out",
            needleSide: "center",
            colorPlate: "#ffffff",
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
        }, options);

        super(canvasId, config);
    }
}

// ------------------------------------------------------------
// POWER METER
// ------------------------------------------------------------

class PowerGauge extends Gauge {
    constructor(canvasId, options = {}) {
        console.log('[PowerGauge] constructor called for', canvasId);
        const config = Object.assign({
            renderTo: canvasId,
            minValue: 0,
            maxValue: 200,
            majorTicks: ["0", "25", "50", "75", "100", "125", "150", "175", "200"],
            highlights: [
                { from: 0, to: 150, color: "rgba(0,255,0,.25)" },
                { from: 150, to: 175, color: "rgba(255,255,0,.25)" },
                { from: 175, to: 200, color: "rgba(255,0,0,.25)" }
            ],
            labels: ["0", "25", "50", "75", "100", "125", "150", "175", "200"],
            startAngle: 90,
            ticksAngle: 180,
            valueBox: false,
            minorTicks: 0,
            strokeTicks: false,
            tickSide: "out",
            needleSide: "center",
            colorPlate: "#ffffff",
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
        }, options);

        super(canvasId, config);
    }

    render() {
        console.log('[PowerGauge] render called for', this.canvasId);
        super.render();
    }
}

// ------------------------------------------------------------
// SWR METER
// ------------------------------------------------------------

class SWRGauge extends Gauge {
    constructor(canvasId, options = {}) {
        const config = Object.assign({
            renderTo: canvasId,
            minValue: 0,
            maxValue: 255,
            majorTicks: ["0", "32", "64", "96", "128", "160", "192", "224", "255"],
            highlights: [
                { from: 0, to: 85, color: "rgba(0,255,0,.25)" },
                { from: 85, to: 128, color: "rgba(255,255,0,.25)" },
                { from: 128, to: 255, color: "rgba(255,0,0,.25)" }
            ],
            labels: ["1.0", "1.2", "1.5", "1.7", "2.0", "2.3", "2.5", "2.7", "3.0"],
            startAngle: 90,
            ticksAngle: 180,
            valueBox: false,
            minorTicks: 0,
            strokeTicks: false,
            tickSide: "out",
            needleSide: "center",
            colorPlate: "#ffffff",
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
        }, options);

        super(canvasId, config);
    }
}

// ------------------------------------------------------------
// ALC METER
// ------------------------------------------------------------

class ALCGauge extends Gauge {
    constructor(canvasId, options = {}) {
        const config = Object.assign({
            renderTo: canvasId,
            minValue: 0,
            maxValue: 255,
            majorTicks: ["0", "32", "64", "96", "128", "160", "192", "224", "255"],
            highlights: [
                { from: 0, to: 178, color: "rgba(0,255,0,.25)" },
                { from: 178, to: 230, color: "rgba(255,255,0,.25)" },
                { from: 230, to: 255, color: "rgba(255,0,0,.25)" }
            ],
            labels: ["0", "6", "12", "19", "25", "31", "37", "44", "50"],
            startAngle: 90,
            ticksAngle: 180,
            valueBox: false,
            minorTicks: 0,
            strokeTicks: false,
            tickSide: "out",
            needleSide: "center",
            colorPlate: "#ffffff",
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
        }, options);

        super(canvasId, config);
    }
}

// Export classes as ES module
export { Gauge, SMeterGauge, PowerGauge, SWRGauge, ALCGauge };
