// FTdx101 WebApp – Meter UI Orchestrator
// Connects WebSocket → Update Pipeline → MeterPanel.
// No calibration logic, no formatting logic, no gauge logic.

import { MeterPanel } from '../guages/meter-panel.js';
import { WsConnection } from '../websocket/ws-connection.js';
import { WsUpdatePipeline } from '../websocket/ws-update-pipeline.js';

export class FTdx101Meters {
    constructor(wsUrl) {
        this.wsUrl = wsUrl;

        // ------------------------------------------------------------
        // 1. Create the meter panel (UI layer only)
        // ------------------------------------------------------------
        this.meterPanel = new MeterPanel({
            smeter: { canvasId: 'smeterCanvas', min: 0, max: 60 },
            power:  { canvasId: 'powerCanvas',  min: 0, max: 200 },
            swr:    { canvasId: 'swrCanvas',    min: 1, max: 3 },
            alc:    { canvasId: 'alcCanvas',    min: 0, max: 100 }
        });

        // ------------------------------------------------------------
        // 2. Create the update pipeline (calibration + routing)
        // ------------------------------------------------------------
        this.pipeline = new WsUpdatePipeline({
            onSMeter: (cal, raw) => this._update('smeter', cal, raw),
            onPower:  (cal, raw) => this._update('power',  cal, raw),
            onSWR:    (cal, raw) => this._update('swr',    cal, raw),
            onALC:    (cal, raw) => this._update('alc',    cal, raw),

            onUnknown: (payload) => {
                console.warn('Unknown WS payload:', payload);
            }
        });

        // ------------------------------------------------------------
        // 3. Create the WebSocket connection manager
        // ------------------------------------------------------------
        this.connection = new WsConnection(this.wsUrl, this.pipeline, {
            autoReconnect: true,
            reconnectDelay: 2000
        });
    }

    // ------------------------------------------------------------
    // Start the WebSocket connection
    // ------------------------------------------------------------
    start() {
        this.connection.connect();
    }

    // ------------------------------------------------------------
    // Stop the WebSocket connection
    // ------------------------------------------------------------
    stop() {
        this.connection.close();
    }

    // ------------------------------------------------------------
    // Internal: update a single gauge
    // ------------------------------------------------------------
    _update(key, calibratedValue, rawValue) {
        this.meterPanel.update(key, calibratedValue);
    }
}
