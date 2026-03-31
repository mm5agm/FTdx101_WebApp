// FTdx101 WebApp – Gauge Factory
// Creates gauge instances based on a type string.
// No layout logic, no DOM queries, no calibration logic.

import { MeterGauge } from './meter-gauge.js';
import { Gauge } from './gauge.js';

// Registry of gauge constructors.
// Add new meter types here as your UI grows.
const gaugeRegistry = {
    smeter: MeterGauge,
    power: MeterGauge,
    swr: MeterGauge,
    alc: MeterGauge
};

/**
 * Create a gauge instance.
 *
 * @param {string} type - The gauge type (e.g., "smeter", "power").
 * @param {string} canvasId - The ID of the canvas element.
 * @param {object} options - Gauge configuration overrides.
 * @returns {object|null} - A gauge instance or null if type is unknown.
 */
export function createGauge(type, canvasId, options = {}) {
    const Constructor = gaugeRegistry[type];

    if (!Constructor) {
        console.warn(`GaugeFactory: Unknown gauge type "${type}"`);
        return null;
    }

    return new Constructor(canvasId, options);
}

/**
 * Register a new gauge type at runtime.
 * Useful for plugins or future expansion.
 *
 * @param {string} type
 * @param {class} constructor
 */
export function registerGauge(type, constructor) {
    if (typeof constructor !== 'function') {
        console.error(`GaugeFactory: constructor for "${type}" must be a class/function`);
        return;
    }

    gaugeRegistry[type] = constructor;
}
