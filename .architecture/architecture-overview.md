# FTdx101 WebApp — Architecture Overview

The FTdx101 WebApp is a modular, subsystem‑driven architecture designed for clarity, maintainability, and real‑time performance. This document provides a high‑level overview of the system and how its major components interact.

---

# 1. Architectural Goals

The architecture is designed to be:

- Predictable and maintainable
- Strictly modular
- Easy to reason about
- Free of cross‑layer leakage
- Real‑time capable (20–60 Hz updates)
- Pure where possible
- UI‑safe and DOM‑isolated
- Extensible without drift

The system should feel like it was built by a disciplined engineering team.

---

# 2. High-Level Subsystems

The application is composed of the following major subsystems:

## 1. WebSocket Subsystem
Receives raw CAT data from the backend and routes it into the update pipeline.

## 2. Calibration Engine
Converts raw radio values into calibrated, meaningful meter values using pure functions.

## 3. Orchestrator (FTdx101Meters)
Coordinates the flow between WebSocket → calibration → UI.

## 4. Meter Subsystem
Renders calibrated values into UI meters using:
- MeterPanel
- gaugeFactory
- update-engine
- meter-formatters

## 5. UI/State Subsystem
Owns all DOM access, layout, and canvas rendering.

## 6. CAT / Serial / Queue / Decoding Subsystems
Backend-facing layers that handle:
- CAT command timing
- Serial communication
- Command queueing
- CAT decoding
- Radio state interpretation

---

# 3. Frontend Value Flow (Strict)

All meter values must follow this exact pipeline:

WebSocket
    ↓
WsUpdatePipeline (routing only)
    ↓
calibration-engine (pure functions)
    ↓
FTdx101Meters (orchestrator)
    ↓
MeterPanel.update()
    ↓
gaugeFactory + update-engine
    ↓
Canvas rendering

This flow must never be bypassed or rearranged.

---

# 4. Subsystem Responsibilities (High-Level)

## WebSocket
- Transport only
- No UI, no calibration

## Calibration Engine
- Pure logic
- Single source of truth for scaling

## Orchestrator
- Wires subsystems together
- Contains no logic of its own

## Meter Subsystem
- UI rendering
- Gauge creation and updates
- Formatting

## UI/State
- DOM access
- Layout
- Canvas rendering

## CAT/Serial/Queue/Decoding
- Backend communication
- Radio state interpretation

---

# 5. Folder Structure

The repository is organised into subsystem folders:

/websocket
/calibration
/ui
/orchestrators
/serial
/queue
/decoding
.copilot
.architecture

Each folder corresponds to a subsystem and must contain only logic belonging to that subsystem.

---

# 6. Design Philosophy

The architecture follows these principles:

- Single responsibility
- No duplication
- No magic strings
- No global variables
- ES modules everywhere
- Class-based meter specialisation
- Pure functions where possible
- DOM access isolated to UI subsystem
- Predictable, maintainable code

---

# 7. Enforcement

This architecture is enforced through:

- .copilot/rules.md (strict rules for Copilot)
- drift-check.md (drift detection and correction)
- Folder structure
- Refactor workflow
- Boot prompt

All code must comply with this architecture.
