# FTdx101 WebApp — Architecture Risks & Mitigation

This document identifies the key architectural risks in the FTdx101 WebApp and provides mitigation strategies to ensure long‑term stability, maintainability, and performance.

---

# 1. Risk: Subsystem Leakage

## Description
Logic from one subsystem appears in another (e.g., DOM access in calibration, formatting in WebSocket, gauge creation in UI helpers).

## Impact
- Architectural drift  
- Hard‑to‑debug behaviour  
- Long‑term decay  
- Loss of subsystem clarity  

## Mitigation
- Enforce subsystem-boundaries.md  
- Use architecture-checklist.md before merging  
- Run copilot-compliance-test.md regularly  
- Perform drift checks during refactors  

---

# 2. Risk: Value Pipeline Bypass

## Description
A developer or tool bypasses the mandatory pipeline:

WebSocket → WsUpdatePipeline → calibration-engine → FTdx101Meters → MeterPanel.update() → gaugeFactory/update-engine → canvas

## Impact
- Inconsistent meter behaviour  
- Incorrect calibration  
- UI desynchronisation  
- Debugging complexity  

## Mitigation
- Validate pipeline integrity during code review  
- Reject any code that updates gauges directly  
- Keep orchestrator logic pure  

---

# 3. Risk: Calibration Contamination

## Description
Calibration engine receives UI, DOM, WebSocket, or formatting logic.

## Impact
- Loss of purity  
- Hard‑to‑test calibration  
- Incorrect meter values  
- Hidden side effects  

## Mitigation
- Keep calibration pure and functional  
- Use architecture-checklist.md  
- Review calibration changes carefully  

---

# 4. Risk: DOM Access Outside UI Subsystem

## Description
DOM manipulation appears in logic layers.

## Impact
- Performance regressions  
- Rendering inconsistencies  
- Tight coupling between layers  

## Mitigation
- Restrict DOM access to MeterPanel and UI helpers  
- Reject DOM usage in logic layers  

---

# 5. Risk: Gauge Logic Drift

## Description
Gauge creation or updates appear outside gaugeFactory or update-engine.

## Impact
- Inconsistent gauge behaviour  
- Duplicated configuration  
- Hard‑to‑maintain UI  

## Mitigation
- Centralise gauge creation in gaugeFactory  
- Centralise updates in update-engine  
- Use meter-formatters for all text output  

---

# 6. Risk: Formatting Drift

## Description
Formatting logic appears outside meter-formatters.

## Impact
- Inconsistent UI text  
- Hard‑to‑localise or adjust formatting  
- Duplication  

## Mitigation
- Enforce formatting isolation  
- Review UI code for inline formatting  

---

# 7. Risk: Folder Structure Drift

## Description
Files placed in incorrect folders or new folders created without architectural alignment.

## Impact
- Architecture becomes unclear  
- Copilot suggestions degrade  
- Subsystem boundaries weaken  

## Mitigation
- Follow folder structure defined in architecture-overview.md  
- Reject PRs that introduce new folders without justification  

---

# 8. Risk: Orchestrator Becoming a “God Object”

## Description
FTdx101Meters accumulates logic instead of remaining a pure coordinator.

## Impact
- Centralised complexity  
- Hard‑to‑test behaviour  
- Circular dependencies  

## Mitigation
- Keep orchestrator logic-free  
- Move logic to appropriate subsystems  
- Review orchestrator changes carefully  

---

# 9. Risk: Copilot Misalignment

## Description
Copilot stops respecting architecture due to updates, context loss, or drift.

## Impact
- Incorrect code suggestions  
- Subsystem leakage  
- Architecture violations  

## Mitigation
- Run copilot-compliance-test.md regularly  
- Keep .copilot/rules.md up to date  
- Restart VS Code when behaviour seems “off”  

---

# 10. Risk: Performance Regression in UI Rendering

## Description
UI code becomes inefficient, especially with high‑frequency meter updates.

## Impact
- Jank  
- Frame drops  
- Slow gauge updates  

## Mitigation
- Keep rendering in canvas  
- Avoid unnecessary DOM operations  
- Use update-engine for efficient updates  

---

# 11. Risk: Empirical Logic in Wrong Layer

## Description
Radio quirks or empirical corrections appear in UI or WebSocket layers.

## Impact
- Inconsistent behaviour  
- Hard‑to‑trace bugs  
- Calibration drift  

## Mitigation
- Keep empirical logic in calibration or decoding layers  
- Document empirical behaviour clearly  

---

# 12. Risk: Documentation Drift

## Description
Architecture documents fall out of sync with the codebase.

## Impact
- Confusion for contributors  
- Copilot misalignment  
- Incorrect assumptions  

## Mitigation
- Update documentation during refactors  
- Treat documentation as part of the architecture  
- Use INDEX.md to track completeness  

---

# 13. Risk: Unreviewed Refactors

## Description
Refactors performed without following the refactor-session workflow.

## Impact
- Hidden drift  
- Broken boundaries  
- Subtle regressions  

## Mitigation
- Enforce refactor-session.md  
- Require architectural validation before merging  

---

# 14. Risk: Over‑Coupling Between Subsystems

## Description
Subsystems begin depending on each other in unintended ways.

## Impact
- Reduced modularity  
- Harder testing  
- Increased fragility  

## Mitigation
- Maintain strict one‑directional flow  
- Avoid circular imports  
- Keep responsibilities isolated  

---

# 15. Risk: Loss of Architectural Intent Over Time

## Description
New contributors misunderstand the architecture or bypass rules.

## Impact
- Long‑term decay  
- Increased maintenance cost  
- Loss of clarity  

## Mitigation
- Use developer-onboarding.md  
- Keep glossary and FAQ updated  
- Maintain architecture-checklist.md  

