# Meter Calibration

This document describes how each meter is read from the FTdx101MP via CAT, how raw values are converted to display values, and how the calibration tables are structured.

---

## How Meter Reading Works

The FTdx101MP CAT interface exposes meters through two commands:

- **`MSxy;`** — Selects which meter is shown on the left (`x`) and right (`y`) display slots on the radio's front panel. This must be sent before reading with `RM0`.
- **`RM0;`** — Reads the currently selected meters. Returns `RM0LLLRRR;` where `LLL` is the left meter value (0–255) and `RRR` is the right meter value (0–255).

Some meters have dedicated read commands (`RM1`–`RM9`) but on the FTdx101MP these return unreliable or stale values. All meter reads in this app use the `MS` + `RM0` pattern.

### MS Slot Values

| Code | Left meter | Right meter |
|------|-----------|-------------|
| `0`  | Power output | — |
| `1`  | Compression | — |
| `2`  | ALC | — |
| `3`  | — | SWR |

---

## Poll Cycle (MeterPollingService.cs)

Each 500 ms the service sends the following CAT commands in sequence:

| Command | Parses | Notes |
|---------|--------|-------|
| `TX;` | TX state | Debounced: 2 consecutive TX=false needed to clear TX state |
| `SM0;` | S-Meter VFO-A | |
| `SM1;` | S-Meter VFO-B | |
| `RM5;` | Power output | |
| `MS13;` + `RM0;` | Compression (left) + SWR (right) | Single round-trip for both |
| `RM7;` | IDD (drain current) | |
| `RM4;` | ALC | |
| `RM8;` | VDD (PA supply voltage) | |
| `RM9;` | PA temperature | |

---

## Calibration Pipeline

Raw values (0–255) from the radio are converted to display values by the calibration engine before being shown on gauges or labels.

```
CAT raw value (0–255)
  → MeterPollingService  (C# backend)
  → SignalR RadioStateUpdate { property, value }
  → FTdx101Meters.js  (orchestrator — smoothing, TX gating)
  → calibration-engine.js  (pure interpolation)
  → MeterPanel / gauge canvas
```

The calibration engine (`wwwroot/js/calibration/calibration-engine.js`) performs linear interpolation between points in the calibration table for the named meter. If no table exists for a meter name the raw value is passed through unchanged.

Default tables are defined in `calibration-tables.js`. At startup `loadFromBackend()` replaces them with any user-saved data from `calibration.default.json` (development) or `calibration.user.json` (installed).

---

## Per-Meter Calibration Tables


### S-Meter (`SM0` / `SM1`)

Raw 0–255 → S-unit label (S0–S9, +10, +20, +40).

Uses label snapping (nearest lower-or-equal point), not interpolation.

### Power (`RM5;`)

Raw 0–255 → watts. Calibrated against known power levels on a dummy load.

### SWR (`MS13;` + `RM0;` right meter)

Scale derived from friend's lookup table:

| Raw | SWR |
|-----|-----|
| 0   | 1.0 |
| 51  | 1.5 |
| 77  | 2.0 |
| 128 | 3.0 |
| 173 | 5.0 |
| 242 | 9.9 |

The gauge needle uses `(SWR − 1.0) × 127.5` to map the 1.0–3.0 range across the full arc. SWR above 3.0 pegs the needle; the numeric overlay still shows the actual value.

A 7-reading rolling average is applied in the orchestrator before display.

**Note:** `RM6;` (the dedicated SWR command) returns a constant stale value on this radio and is not used.

### Compression (`MS13;` + `RM0;` left meter)

Scale derived from friend's table (dB → pointer %):

| Raw | dB |
|-----|----|
| 0   | 0  |
| 56  | 5  |
| 102 | 10 |
| 140 | 15 |
| 204 | 20 |

**Note:** `RM3;` (the dedicated compression command) returns unreliable values and is not used.

### ALC (`RM4;`)

Raw 0–255 → volts. 

### IDD — Drain Current (`RM7;`)

Scale derived from reading rig values:

| Raw | Amps |
|-----|------|
| 0   | 0    |
| 51  | 5    |
| 102 | 10   |
| 153 | 15   |
| 204 | 20   |
| 242 | 25   |

### VDD — PA Supply Voltage (`RM8;`)

This table was calibrated directly against the radio's own voltage display.

Valid operating range is approximately raw 170–235 (40–55 V). Readings outside this range are discarded as noise.

### PA Temperature (`RM9;`)

Scale derived from formula: `temp°C = (raw / 2.3) − 6`

| Raw | °C  |
|-----|-----|
| 14  | 0   |
| 60  | 20  |
| 106 | 40  |
| 152 | 60  |
| 198 | 80  |
| 244 | 100 |

---

## Calibration Storage

| Mode | File |
|------|------|
| Development (dotnet run) | `wwwroot/calibration.default.json` |
| Installed app | `%APPDATA%\MM5AGM\FTdx101 WebApp\calibration.user.json` |

The Meter Calibration page (`/MeterCalibration`) reads and writes the active file via `POST /api/calibration/file`. Changes take effect immediately without a restart.

---

## Cache Busting

`calibration-engine.js` and `calibration-tables.js` are loaded as ES modules with a `?v=N` version query string. Bump `N` in both `calibration-engine.js` (its own import of `calibration-tables.js`) and `Pages/Shared/_Layout.cshtml` (its import of `calibration-engine.js`) whenever either file changes, to force browsers to reload the updated module.

Current version: **v=10**
