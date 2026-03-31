// FTdx101 WebApp – WebSocket Update Pipeline
// No DOM, no UI, no gauge logic. Pure data flow only.

import {
    calibrateSMeter,
    calibratePower,
    calibrateSWR,
    calibrateALC
} from '../calibration/calibration-engine.js';

// The pipeline is event-driven: it receives raw CAT data,
// runs calibration, and emits calibrated values via callbacks.

export class WsUpdatePipeline {
    constructor(options = {}) {
        this.onSMeter = options.onSMeter || null;
        this.onPower = options.onPower || null;
        this.onSWR = options.onSWR || null;
        this.onALC = options.onALC || null;
        this.onUnknown = options.onUnknown || null;
    }

    // Entry point for raw WebSocket messages (string or object)
    handleMessage(message) {
        const payload = this._normaliseMessage(message);
        if (!payload || typeof payload !== 'object') {
            return;
        }

        // Example expected payload shape (adjust to your backend):
        // {
        //   type: 'meter',
        //   smeter: <raw>,
        //   power: <raw>,
        //   swr: <raw>,
        //   alc: <raw>
        // }

        if (payload.type === 'meter') {
            this._handleMeterPayload(payload);
        } else {
            this._emitUnknown(payload);
        }
    }

    // Normalise incoming message into a JS object
    _normaliseMessage(message) {
        if (typeof message === 'string') {
            try {
                return JSON.parse(message);
            } catch {
                return null;
            }
        }

        if (typeof message === 'object' && message !== null) {
            return message;
        }

        return null;
    }

    _handleMeterPayload(payload) {
        if (typeof payload.smeter === 'number' && this.onSMeter) {
            const calibrated = calibrateSMeter(payload.smeter);
            this.onSMeter(calibrated, payload.smeter);
        }

        if (typeof payload.power === 'number' && this.onPower) {
            const calibrated = calibratePower(payload.power);
            this.onPower(calibrated, payload.power);
        }

        if (typeof payload.swr === 'number' && this.onSWR) {
            const calibrated = calibrateSWR(payload.swr);
            this.onSWR(calibrated, payload.swr);
        }

        if (typeof payload.alc === 'number' && this.onALC) {
            const calibrated = calibrateALC(payload.alc);
            this.onALC(calibrated, payload.alc);
        }
    }

    _emitUnknown(payload) {
        if (this.onUnknown) {
            this.onUnknown(payload);
        }
    }
}
