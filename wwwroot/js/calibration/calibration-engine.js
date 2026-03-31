// FTdx101 WebApp – Calibration Engine
// Pure functions only. No DOM, no UI, no gauge logic.
// FTdx101 WebApp – Calibration Tables
// Single source of truth for all meter calibration data.
// Values below are placeholders and should be replaced with real measurements.

export const smeterTable = [
    { raw: 0, value: 0 },
    { raw: 40, value: 1 },
    { raw: 80, value: 3 },
    { raw: 120, value: 5 },
    { raw: 160, value: 7 },
    { raw: 200, value: 9 },
    { raw: 240, value: 20 },
    { raw: 255, value: 60 }
];

export const powerTable = [
    { raw: 0, value: 0 },
    { raw: 64, value: 25 },
    { raw: 128, value: 50 },
    { raw: 192, value: 100 },
    { raw: 255, value: 200 }
];

export const swrTable = [
    { raw: 0, value: 1.0 },
    { raw: 128, value: 1.5 },
    { raw: 200, value: 2.0 },
    { raw: 255, value: 3.0 }
];

export const alcTable = [
    { raw: 0, value: 0 },
    { raw: 128, value: 50 },
    { raw: 255, value: 100 }
];
