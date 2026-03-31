// FTdx101 WebApp – Meter Panel
// Responsible only for creating and holding gauge instances.
// No calibration, no WebSocket, no DOM queries beyond canvas IDs.

import { createGauge } from './gaugeFactory.js';
import { updateGaugeValue } from './update-engine.js';

export class MeterPanel {
    constructor(config = {}) {
        // Expected config shape:
        // {
        //   smeter: { canvasId: 'smeterCanvas', min: 0, max: 60 },
        //   power:  { canvasId: 'powerCanvas',  min: 0, max: 200 },
        //   swr:    { canvasId: 'swrCanvas',    min: 1, max: 3 },
        //   alc:    { canvasId: 'alcCanvas',    min: 0, max: 100 }
        // }

        this.config = config;
        this.gauges = {};

        this._createGauges();
    }

    // Create all gauges defined in the config
    _createGauges() {
        for (const key of Object.keys(this.config)) {
            const { canvasId, ...options } = this.config[key];
            const gauge = createGauge(key, canvasId, options);

            if (gauge) {
                this.gauges[key] = gauge;
            } else {
                console.warn(`MeterPanel: Failed to create gauge for "${key}"`);
            }
        }
    }

    // Update a single gauge by key
    update(key, calibratedValue) {
        const gauge = this.gauges[key];
        const constraints = this.config[key] || {};

        if (gauge) {
            updateGaugeValue(gauge, calibratedValue, constraints);
        }
    }

    // Bulk update multiple gauges
    updateAll(valueMap) {
        for (const key of Object.keys(valueMap)) {
            this.update(key, valueMap[key]);
        }
    }
}
