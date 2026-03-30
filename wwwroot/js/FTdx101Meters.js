// FTdx101Meters.js
// Shared mapping + formatting module for FTdx101D / FTdx101MP

const FTdx101Meters = {

    // ------------------------------------------------------------
    // S-METER (0–255 raw → S0 ... +60)
    // ------------------------------------------------------------
    formatS(v) {
        if (v < 4) return "S0";
        if (v < 30) return "S1";
        if (v < 65) return "S3";
        if (v < 95) return "S5";
        if (v < 130) return "S7";
        if (v < 171) return "S9";
        if (v < 212) return "S9+20";
        if (v < 255) return "+40";
        return "+60";
    },

    // ------------------------------------------------------------
    // POWER (0–200 raw → watts)
    // ------------------------------------------------------------
    formatPower(v) {
        return `${Math.round(v)} W`;
    },

    // ------------------------------------------------------------
    // SWR (0–255 raw → SWR scale)
    // ------------------------------------------------------------
    formatSWR(v) {
        const labels = ["1.0","1.3","1.5","1.7","2.0","2.3","2.5","2.7","3.0"];
        const idx = Math.round((v / 255) * (labels.length - 1));
        return labels[idx];
    },

    // ------------------------------------------------------------
    // ALC (0–255 raw → ALC scale)
    // ------------------------------------------------------------
    formatALC(v) {
        const labels = ["0","6","12","19","25","31","37","44","50"];
        const idx = Math.round((v / 255) * (labels.length - 1));
        return labels[idx];
    },

    // ------------------------------------------------------------
    // TEMP (0–255 raw → °C)
    // FTdx101D/MP typically reports PA temp 0–100°C mapped to 0–255
    // ------------------------------------------------------------
    formatTemp(v) {
        const tempC = Math.round((v / 255) * 100);
        return `${tempC}°C`;
    },

    // ------------------------------------------------------------
    // IDD (0–255 raw → Amps)
    // FTdx101D/MP typically reports 0–40A mapped to 0–255
    // ------------------------------------------------------------
    formatIDD(v) {
        const amps = ((v / 255) * 40).toFixed(1);
        return `${amps} A`;
    },

    // ------------------------------------------------------------
    // TEMPLATE FOR NEW METERS
    // ------------------------------------------------------------
    createLinearFormatter(maxRaw, maxDisplay, unit = "") {
        return function(v) {
            const scaled = (v / maxRaw) * maxDisplay;
            return unit ? `${scaled.toFixed(1)} ${unit}` : scaled.toFixed(1);
        };
    }
};

// Export globally
window.FTdx101Meters = FTdx101Meters;
