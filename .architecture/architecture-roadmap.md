# Architecture Roadmap

This document outlines long‑term architectural goals and planned improvements for the FTdx101 WebApp.

## Purpose
Provide a clear, evolving plan for future enhancements while preserving subsystem boundaries and pipeline integrity.

## Short‑Term (0–3 months)
- Improve calibration tables for edge cases.
- Expand decoding coverage for additional CAT responses.
- Add more unit tests for pipeline routing.
- Optimise update-engine for reduced allocations.
- Refine UI layout for clarity and consistency.

## Mid‑Term (3–9 months)
- Introduce optional performance metrics overlay.
- Add subsystem‑level benchmarks.
- Improve WebSocket message coalescing.
- Expand developer-onboarding with examples.
- Add more diagrams to diagrams-overview.md.

## Long‑Term (9–18 months)
- Modularise backend CAT handling for future radios.
- Add optional plugin system for new meters.
- Introduce theme support in UI subsystem.
- Improve calibration-engine extensibility.
- Add automated drift-detection tooling.

## Architectural Guardrails
- No changes may violate subsystem-boundaries.md.
- Value pipeline must remain intact.
- Orchestrator must stay logic-free.
- UI must remain canvas-driven for meters.
- Calibration must remain pure.

## Evolution Rules
- All roadmap items must follow subsystem-evolution.md.
- Each change must include documentation updates.
- Each change must include tests.
- Each change must pass drift-check.md.

## Risks
- Feature creep causing boundary violations.
- UI complexity increasing DOM usage.
- Calibration contamination from UI or WebSocket layers.
- Pipeline bypasses introduced accidentally.

## Success Criteria
- Architecture remains stable and predictable.
- Subsystems remain clean and isolated.
- Performance remains high under load.
- Contributors can onboard quickly.
- Copilot remains aligned with architecture.

## Summary
The roadmap guides long‑term evolution while protecting architectural integrity, ensuring the system grows without drifting.
