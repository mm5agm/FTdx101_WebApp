# FTdx101 WebApp — Subsystem Boundaries (Authoritative Specification)

This document defines the strict subsystem boundaries for the FTdx101 WebApp. Each subsystem has a clear purpose, allowed behaviours, and forbidden behaviours. No subsystem may contain logic belonging to another.

---

# 1. Calibration Engine

## Purpose
Convert raw radio values into calibrated, meaningful meter values.

## Allowed
- Pure functions only
- Calibration tables
- Scaling logic
- Empirical corrections
- No side effects

## Forbidden
- DOM access
- UI logic
- WebSocket logic
- Gauge creation or updates
- Formatting logic
- Global state

---

# 2. WebSocket Subsystem

## Purpose
Receive raw CAT data from backend and route it into the update pipeline.

## Allowed
- Transport logic
- Message parsing
- Routing to update pipeline
- Connection lifecycle management

## Forbidden
- DOM access
- UI logic
- Calibration logic
- Gauge logic
- Formatting logic
- Global state

---

# 3. Meter Subsystem

## Purpose
Render calibrated values into UI meters.

## Allowed
- MeterPanel (UI owner)
- gaugeFactory (gauge creation)
- update-engine (gauge updates)
- meter-formatters (UI text formatting)
- DOM access (UI only)

## Forbidden
- Calibration logic
- WebSocket logic
- Decoding logic
- Serial/queue logic
- Orchestrator logic

---

# 4. Orchestrator Subsystem

## Purpose
Coordinate the flow between WebSocket → calibration → UI.

## Allowed
- Wiring subsystems together
- Passing calibrated values to MeterPanel
- Managing update flow

## Forbidden
- Calibration logic
- Gauge creation
- Formatting logic
- DOM access
- WebSocket transport logic

---

# 5. CAT / Serial / Queue / Decoding Subsystems

## Purpose
Handle radio communication, timing, decoding, and command flow.

## Allowed
- Serial timing
- Command queueing
- CAT decoding
- Radio state interpretation

## Forbidden
- UI logic
- DOM access
- Calibration logic
- WebSocket logic

---

# 6. UI/State Subsystem

## Purpose
Render the user interface and manage UI state.

## Allowed
- DOM access
- Canvas rendering
- Layout logic
- UI helpers

## Forbidden
- Calibration logic
- Decoding logic
- Serial/queue logic
- WebSocket logic

---

# 7. Enforcement

Subsystem boundaries are enforced by:
- `.copilot/rules.md`
- `drift-check.md`
- Folder structure
- Refactor workflow

All code must comply with these boundaries.
