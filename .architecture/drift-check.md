# FTdx101 WebApp — Architectural Drift Detection Guide

This document defines how to detect, classify, and correct architectural drift in the FTdx101 WebApp. Drift is any deviation from the strict subsystem boundaries and architectural rules defined in `.copilot/rules.md`, `architecture-overview.md`, and `subsystem-boundaries.md`.

---

# 1. What Counts as Architectural Drift

Architectural drift occurs when code violates subsystem boundaries, the value pipeline, naming rules, folder structure, or purity constraints.

## 1.1 Subsystem Leakage
Any logic appearing in the wrong subsystem, including:
- DOM access outside UI subsystem
- Calibration logic outside calibration-engine
- WebSocket logic outside websocket subsystem
- Gauge creation outside gaugeFactory
- Formatting outside meter-formatters
- Decoding logic outside decoding subsystem
- Serial timing outside serial subsystem
- Orchestrator logic appearing anywhere else

## 1.2 Bypassing the Meter Pipeline
Any code that bypasses the required flow:

WebSocket → WsUpdatePipeline → calibration-engine → FTdx101Meters → MeterPanel.update() → gaugeFactory/update-engine → canvas

Examples:
- WebSocket updating gauges directly
- Calibration skipped or duplicated
- MeterPanel bypassed
- update-engine bypassed
- Direct DOM updates from logic layers

## 1.3 Direct DOM Manipulation in Forbidden Layers
Forbidden in:
- calibration-engine
- websocket subsystem
- decoding subsystem
- serial/queue subsystem
- orchestrator
- helpers

Allowed only in UI/state subsystem.

## 1.4 Direct Gauge Creation
Any instance of:
- `new RadialGauge(...)`
- Inline gauge configuration
- Gauge logic outside gaugeFactory

## 1.5 Formatting Drift
Formatting logic must live only in `meter-formatters.js`.

Drift includes:
- Inline formatting
- Formatting inside calibration
- Formatting inside WebSocket pipeline
- Formatting inside orchestrator
- Formatting inside gaugeFactory

## 1.6 Naming Drift
Examples:
- PascalCase used for flow-level identifiers
- camelCase used for architectural units
- Ambiguous names
- Names that no longer reflect behaviour

## 1.7 Empirical Logic in the Wrong Layer
Empirical behaviour must remain in:
- calibration tables
- decoding layer
- serial timing layer

Drift includes:
- Empirical scaling in UI
- Empirical quirks in WebSocket pipeline
- Empirical timing in orchestrator

## 1.8 Folder Structure Drift
Examples:
- Modules placed in the wrong folder
- Helpers not grouped with their subsystem
- New files created outside the architecture
- UI files placed in logic folders
- Logic files placed in UI folders

---

# 2. How to Respond to Drift

When drift is detected, the developer or Copilot must:

## 2.1 Produce a Drift Report
A short summary:
- What drift was found
- Why it violates the architecture
- Which subsystem owns the logic

## 2.2 Propose a Corrective Refactor
A minimal, safe, subsystem‑aligned fix:
- Move logic to the correct subsystem
- Extract pure functions
- Relocate DOM code to MeterPanel
- Move formatting to meter-formatters
- Move gauge creation to gaugeFactory
- Restore the correct value pipeline

## 2.3 Apply the Fix in a Clean Refactor Chunk
Each fix must:
- Be self‑contained
- Include a summary
- Include rationales
- Update comments
- Preserve behaviour
- Eliminate duplication
- Restore architectural purity

---

# 3. Drift Severity Levels

## 3.1 Critical Drift
Breaks subsystem boundaries or the meter pipeline.

Examples:
- DOM in calibration
- Gauge creation outside gaugeFactory
- WebSocket updating UI directly
- Calibration bypassed

Action: Immediate correction required.

## 3.2 Major Drift
Violates architectural rules but does not break runtime behaviour.

Examples:
- Formatting in orchestrator
- Empirical logic in UI
- Naming violations

Action: Correct in next refactor chunk.

## 3.3 Minor Drift
Cosmetic or structural issues.

Examples:
- Outdated comments
- Folder misplacement
- Inconsistent naming

Action: Fix during routine cleanup.

---

# 4. Drift Prevention Rules

To prevent drift:
- Check subsystem boundaries before generating code
- Verify the value pipeline is preserved
- Ensure DOM access stays in UI subsystem
- Ensure calibration stays pure
- Ensure gauge creation stays in gaugeFactory
- Ensure formatting stays in meter-formatters
- Ensure orchestrator remains logic‑free
- Ensure folder placement matches subsystem

If a request would cause drift, propose a compliant alternative.

---

# 5. Drift Check Workflow

For every refactor or code generation task:

1. Load `.copilot/rules.md`
2. Load `architecture-overview.md`
3. Identify the subsystem(s) involved
4. Check for drift using this document
5. Report any drift found
6. Propose corrections
7. Apply corrections in clean refactor chunks
8. Re‑verify subsystem boundaries
9. Update comments and documentation

---

# 6. Scope

This drift‑check applies to:
- All JavaScript modules
- All WebSocket logic
- All calibration logic
- All UI/state logic
- All decoding logic
- All serial/queue logic
- All orchestrator logic
- All refactors
- All new files
- All architectural decisions

All code must comply with this document.
