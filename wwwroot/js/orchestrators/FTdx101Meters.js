// FTdx101 WebApp – Meter Orchestrator
// Connects calibration-engine → MeterPanel.
// No DOM queries, no SignalR, no TX state logic, no smoothing.
// Receives a (property, rawValue) pair and routes it through calibration to the gauge.

/**
 * Maps SignalR property names to { calibrationKey, gaugeKey }.
 *
 * calibrationKey  — key passed to calibrationEngine.calibrateNumeric()
 *                   null means pass rawValue directly to the gauge (no calibration table)
 * gaugeKey        — key passed to meterPanel.update()
 */
const METER_MAP = {
    PowerMeter:       { calibrationKey: 'PWR',           gaugeKey: 'power'       },
    SWRMeter:         { calibrationKey: 'SWR',           gaugeKey: 'swr'         },
    CompressionMeter: { calibrationKey: null,            gaugeKey: 'compression' },
    ALCMeter:         { calibrationKey: 'ALC',           gaugeKey: 'alc'         },
    IDDMeter:         { calibrationKey: 'IDD',           gaugeKey: 'idd'         },
    VDDMeter:         { calibrationKey: 'VPA',           gaugeKey: 'vdd'         },
    Temperature:      { calibrationKey: 'TPA',           gaugeKey: 'temp'        },
    SMeterA:          { calibrationKey: 'SMETER_LABELS', gaugeKey: 'smeterA'     },
    SMeterB:          { calibrationKey: 'SMETER_LABELS', gaugeKey: 'smeterB'     },
};

export class FTdx101Meters {
    /**
     * @param {object} meterPanel        An initialised MeterPanel instance
     * @param {object} calibrationEngine An object exposing calibrateNumeric(key, raw)
     */
    constructor(meterPanel, calibrationEngine) {
        this._meterPanel = meterPanel;
        this._calibration = calibrationEngine;
    }

    /**
     * Route a single meter update from SignalR to the gauge.
     * TX-state checking, smoothing, and DOM label updates remain
     * the responsibility of the caller (site.js).
     *
     * @param {string} property   SignalR property name, e.g. 'PowerMeter'
     * @param {number} rawValue   Raw ADC value from the radio (0–255)
     * @returns {{ gaugeKey: string, calibrated: number } | null}
     *   Returns the gauge key and calibrated value so the caller can update DOM labels,
     *   or null if the property is not a known meter.
     */
    handleMeterUpdate(property, rawValue) {
        const mapping = METER_MAP[property];
        if (!mapping) return null;

        const calibrated = mapping.calibrationKey !== null
            ? this._calibration.calibrateNumeric(mapping.calibrationKey, rawValue)
            : rawValue;

        this._meterPanel.update(mapping.gaugeKey, calibrated);

        return { gaugeKey: mapping.gaugeKey, calibrated };
    }

    /**
     * Returns true if the given property name is a known meter property.
     * @param {string} property
     */
    isMeterProperty(property) {
        return Object.prototype.hasOwnProperty.call(METER_MAP, property);
    }
}
