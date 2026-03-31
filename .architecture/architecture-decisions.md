# FTdx101 WebApp — Architecture Decisions (ADR Log)

This document records the major architectural decisions that define the FTdx101 WebApp.  
Each decision explains *why* the architecture is structured the way it is, ensuring long‑term clarity and consistency.

---

# 1. Decision: Strict Subsystem Boundaries

**Status:** Accepted  
**Date:** Initial architecture  
**Reasoning:**  
The system handles real‑time data, UI rendering, calibration, and hardware communication.  
Mixing these concerns leads to:
- unpredictable behaviour  
- performance issues  
- duplicated logic  
- untestable code  
- long‑term architectural decay  

**Outcome:**  
Each subsystem has a clearly defined purpose and strict allowed/forbidden behaviours.

---

# 2. Decision: Mandatory Value Pipeline

**Status:** Accepted  
**Pipeline:**  
WebSocket → WsUpdatePipeline → calibration-engine → FTdx101Meters → MeterPanel.update() → gaugeFactory/update-engine → canvas

**Reasoning:**  
A predictable, linear pipeline ensures:
- consistent behaviour  
- easy debugging  
- pure calibration  
- clean UI updates  
- no cross‑layer leakage  

**Outcome:**  
All meter values must follow this pipeline with no bypasses.

---

# 3. Decision: Pure Calibration Engine

**Status:** Accepted  
**Reasoning:**  
Calibration must be:
- deterministic  
- testable  
- side‑effect free  
- independent of UI and WebSocket layers  

**Outcome:**  
Calibration engine contains only pure functions and calibration tables.

---

# 4. Decision: DOM Access Only in UI Subsystem

**Status:** Accepted  
**Reasoning:**  
DOM access is slow and side‑effectful.  
Allowing it outside the UI subsystem causes:
- performance regressions  
- unpredictable rendering  
- logic/UI coupling  

**Outcome:**  
Only MeterPanel and UI helpers may touch the DOM.

---

# 5. Decision: Gauge Creation Centralised in gaugeFactory

**Status:** Accepted  
**Reasoning:**  
Gauges must be:
- created consistently  
- configured uniformly  
- easy to replace or upgrade  

**Outcome:**  
All gauge creation happens in `gaugeFactory.js`.

---

# 6. Decision: Gauge Updates Centralised in update-engine

**Status:** Accepted  
**Reasoning:**  
Updating gauges in multiple places leads to:
- drift  
- inconsistent behaviour  
- duplicated logic  

**Outcome:**  
All gauge updates happen in `update-engine.js`.

---

# 7. Decision: Formatting Isolated to meter-formatters

**Status:** Accepted  
**Reasoning:**  
Formatting is UI‑specific.  
If formatting leaks into calibration or WebSocket layers, the system becomes brittle.

**Outcome:**  
All text formatting lives in `meter-formatters.js`.

---

# 8. Decision: Orchestrator Contains No Logic

**Status:** Accepted  
**Reasoning:**  
The orchestrator must remain a pure coordinator.  
If it accumulates logic, it becomes a “god object.”

**Outcome:**  
FTdx101Meters only wires subsystems together.

---

# 9. Decision: ES Modules Everywhere

**Status:** Accepted  
**Reasoning:**  
ES modules provide:
- predictable imports  
- no global namespace pollution  
- tree‑shaking  
- clarity of dependencies  

**Outcome:**  
All files use ES module syntax.

---

# 10. Decision: No Global State

**Status:** Accepted  
**Reasoning:**  
Global state leads to:
- hidden dependencies  
- unpredictable behaviour  
- testability issues  

**Outcome:**  
State is localised to the subsystem that owns it.

---

# 11. Decision: Folder Structure Mirrors Architecture

**Status:** Accepted  
**Reasoning:**  
Folder structure *is* architecture.  
It ensures:
- contributors know where code belongs  
- Copilot knows where to generate code  
- drift is easy to detect  

**Outcome:**  
Each subsystem has its own folder.

---

# 12. Decision: Drift Detection Is Mandatory

**Status:** Accepted  
**Reasoning:**  
Architectural drift is the silent killer of maintainability.

**Outcome:**  
`drift-check.md` defines how to detect and correct drift.

---

# 13. Decision: Copilot Compliance Test Required

**Status:** Accepted  
**Reasoning:**  
Copilot must remain aligned with the architecture.  
The compliance test ensures it loads and respects all rules.

**Outcome:**  
`copilot-compliance-test.md` is part of the architecture.

---

# 14. Decision: Refactors Must Follow a Formal Workflow

**Status:** Accepted  
**Reasoning:**  
Refactors can easily introduce drift.  
A structured workflow ensures safety and clarity.

**Outcome:**  
`refactor-session.md` defines the required process.

---

# 15. Decision: Documentation Is Part of the Architecture

**Status:** Accepted  
**Reasoning:**  
Architecture must persist beyond individual contributors.  
Documentation ensures continuity.

**Outcome:**  
The `.architecture` folder is authoritative and must be maintained.

