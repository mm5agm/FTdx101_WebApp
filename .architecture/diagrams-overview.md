# FTdx101 WebApp — Architecture Diagrams Overview

This document describes the recommended diagrams for the FTdx101 WebApp architecture.  
These diagrams provide visual clarity for contributors and help maintain architectural consistency.  
All diagrams referenced here should be stored in the `.architecture/diagrams/` folder.

---

# 1. Subsystem Map Diagram

## Purpose
Shows all major subsystems and how they relate to each other.

## Contents
- WebSocket subsystem
- Calibration engine
- Orchestrator (FTdx101Meters)
- Meter subsystem (MeterPanel, gaugeFactory, update-engine, meter-formatters)
- UI/State subsystem
- CAT/Serial/Queue/Decoding subsystems

## Notes
This diagram should emphasise strict boundaries and one‑directional flow.

---

# 2. Frontend Value Pipeline Diagram

## Purpose
Visualises the strict meter update pipeline.

## Pipeline
WebSocket  
    ↓  
WsUpdatePipeline  
    ↓  
calibration-engine  
    ↓  
FTdx101Meters  
    ↓  
MeterPanel.update()  
    ↓  
gaugeFactory + update-engine  
    ↓  
Canvas rendering

## Notes
This is the most important diagram in the system.  
It must match the pipeline defined in `architecture-overview.md`.

---

# 3. Backend CAT Workflow Diagram

## Purpose
Shows how CAT commands and responses move through backend layers.

## Contents
- Serial communication
- Command queue
- CAT decoding
- Radio state interpretation
- WebSocket push to frontend

## Notes
This diagram helps contributors understand how raw radio data reaches the browser.

---

# 4. Meter Subsystem Diagram

## Purpose
Shows how the meter UI components interact.

## Components
- MeterPanel (UI owner)
- gaugeFactory (creates gauges)
- update-engine (updates gauges)
- meter-formatters (text formatting)
- Canvas rendering

## Notes
This diagram should highlight:
- DOM access only in MeterPanel
- Gauge creation only in gaugeFactory
- Gauge updates only in update-engine
- Formatting only in meter-formatters

---

# 5. UI/State Diagram

## Purpose
Shows how UI elements are structured and updated.

## Contents
- DOM ownership
- Canvas rendering
- Layout system
- Overlay components
- User interaction flow

## Notes
This diagram must show that UI is isolated from logic layers.

---

# 6. Orchestrator Diagram

## Purpose
Shows the role of FTdx101Meters as a coordinator.

## Contents
- Inputs from calibration-engine
- Outputs to MeterPanel.update()
- No internal logic
- No DOM access
- No calibration or formatting logic

## Notes
This diagram reinforces the orchestrator’s purity.

---

# 7. Diagram Storage Rules

All diagrams must be stored in:

`.architecture/diagrams/`

File naming convention:
- `subsystem-map.png`
- `value-pipeline.png`
- `backend-cat-workflow.png`
- `meter-subsystem.png`
- `ui-state.png`
- `orchestrator.png`

Formats allowed:
- PNG
- SVG

---

# 8. Diagram Update Policy

Diagrams must be updated when:
- Subsystems change
- Value pipeline changes
- Folder structure changes
- UI architecture changes
- Calibration or WebSocket flow changes

Diagrams must always match:
- `.copilot/rules.md`
- `architecture-overview.md`
- `workflow.md`
- `subsystem-boundaries.md`

---

# 9. Purpose of This Document

This document ensures:
- Diagrams remain consistent with architecture
- Contributors know which diagrams exist
- Visual documentation stays aligned with rules
- The architecture remains easy to understand

This file is part of the authoritative architecture documentation.
