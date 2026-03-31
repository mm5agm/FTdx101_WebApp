// gaugeFactory.js
// Centralized factory for all FTdx101MP WebApp gauge components
// Exports: createSMeterGauge, createPowerGauge, createSWRGauge, createALCGauge

import { Gauge, SMeterGauge, PowerGauge, SWRGauge, ALCGauge } from './gauge.js';

function createSMeterGauge(canvasId, options = {}) {
    return new SMeterGauge(canvasId, options);
}

function createPowerGauge(canvasId, options = {}) {
    return new PowerGauge(canvasId, options);
}

function createSWRGauge(canvasId, options = {}) {
    return new SWRGauge(canvasId, options);
}

function createALCGauge(canvasId, options = {}) {
    return new ALCGauge(canvasId, options);
}

export {
    createSMeterGauge,
    createPowerGauge,
    createSWRGauge,
    createALCGauge
};
