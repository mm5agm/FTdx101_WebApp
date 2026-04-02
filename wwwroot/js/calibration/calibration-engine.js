// FTdx101 WebApp – Calibration Engine
// Pure functions only. No DOM, no UI, no gauge logic.
// Single source of truth for all meter calibration data and functions.

// ------------------------------------------------------------
// Calibration tables
// Each entry maps a raw radio value to a calibrated display value.
// ------------------------------------------------------------

const smeterTable = [
    { raw: 0,   value: 0  },
    { raw: 40,  value: 1  },
    { raw: 80,  value: 3  },
    { raw: 120, value: 5  },
    { raw: 160, value: 7  },
    { raw: 200, value: 9  },
    { raw: 240, value: 20 },
    { raw: 255, value: 60 }
];

const powerTable = [
    { raw: 0,   value: 0   },
    { raw: 64,  value: 25  },
    { raw: 128, value: 50  },
    { raw: 192, value: 100 },
    { raw: 255, value: 200 }
];

const swrTable = [
    { raw: 0,   value: 1.0 },
    { raw: 128, value: 1.5 },
    { raw: 200, value: 2.0 },
    { raw: 255, value: 3.0 }
];

const alcTable = [
    { raw: 0,   value: 0   },
    { raw: 128, value: 50  },
    { raw: 255, value: 100 }
];

// ------------------------------------------------------------
// Interpolation helper (linear interpolation between table points)
// ------------------------------------------------------------

function interpolate(table, rawValue) {
    if (rawValue <= table[0].raw) return table[0].value;
    if (rawValue >= table[table.length - 1].raw) return table[table.length - 1].value;

    for (let i = 1; i < table.length; i++) {
        if (rawValue <= table[i].raw) {
            const prev = table[i - 1];
            const next = table[i];
            const t = (rawValue - prev.raw) / (next.raw - prev.raw);
            return prev.value + t * (next.value - prev.value);
        }
    }

    return table[table.length - 1].value;
}

// ------------------------------------------------------------
// Public calibration functions
// ------------------------------------------------------------

export function calibrateSMeter(raw) { return interpolate(smeterTable, raw); }
export function calibratePower(raw)  { return interpolate(powerTable,  raw); }
export function calibrateSWR(raw)    { return interpolate(swrTable,    raw); }
export function calibrateALC(raw)    { return interpolate(alcTable,    raw); }
