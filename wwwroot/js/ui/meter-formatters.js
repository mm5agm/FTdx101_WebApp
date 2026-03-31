// FTdx101 WebApp – Meter Formatters
// UI-only formatting helpers for calibrated values.
// No calibration, no gauge logic, no WebSocket logic.

export const MeterFormatters = {

    // S-METER (calibrated 0–60)
    formatS(value) {
        if (value < 1) return "S0";
        if (value < 3) return "S1";
        if (value < 5) return "S3";
        if (value < 7) return "S5";
        if (value < 9) return "S7";
        if (value < 20) return "S9";
        if (value < 40) return "S9+20";
        if (value < 60) return "S9+40";
        return "+60";
    },

    // POWER (calibrated watts)
    formatPower(value) {
        return `${Math.round(value)} W`;
    },

    // SWR (calibrated 1.0–3.0)
    formatSWR(value) {
        return value.toFixed(1);
    },

    // ALC (calibrated 0–100)
    formatALC(value) {
        return `${Math.round(value)}%`;
    },

    // TEMP (raw 0–255 → °C)
    formatTemp(raw) {
        const tempC = Math.round((raw / 255) * 100);
        return `${tempC}°C`;
    },

    // IDD (raw 0–255 → 0–40A)
    formatIDD(raw) {
        const amps = ((raw / 255) * 40).toFixed(1);
        return `${amps} A`;
    },

    // Generic linear formatter
    createLinearFormatter(maxRaw, maxDisplay, unit = "") {
        return function(raw) {
            const scaled = (raw / maxRaw) * maxDisplay;
            return unit ? `${scaled.toFixed(1)} ${unit}` : scaled.toFixed(1);
        };
    }
};
