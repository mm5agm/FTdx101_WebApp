# FTdx101 WebApp — Architecture FAQ

This FAQ provides quick, high‑clarity answers to common questions about the FTdx101 WebApp architecture.  
It complements the detailed documents in this folder by explaining the reasoning behind key decisions.

---

# 1. Why is the architecture so strict?

Because the FTdx101 WebApp is a real‑time system with:
- high update frequency (20–60 Hz)
- multiple interacting subsystems
- strict timing requirements
- complex UI rendering
- calibration logic that must remain pure

Strict boundaries prevent:
- performance regressions  
- UI jank  
- calibration drift  
- logic duplication  
- long‑term architectural decay  

---

# 2. Why is the value pipeline mandatory?

The pipeline ensures:
- predictable data flow  
- pure calibration  
- clean UI updates  
- no cross‑layer leakage  
- easy debugging  
- consistent behaviour  

The pipeline is:

WebSocket  
→ WsUpdatePipeline  
→ calibration-engine  
→ FTdx101Meters  
→ MeterPanel.update()  
→ gaugeFactory + update-engine  
→ canvas rendering

Any bypass introduces drift and breaks maintainability.

---

# 3. Why must calibration be pure?

Pure calibration ensures:
- reproducible results  
- testability  
- no side effects  
- no UI or DOM dependencies  
- no WebSocket dependencies  

Calibration is the mathematical heart of the system.  
Purity keeps it correct and stable.

---

# 4. Why is DOM access restricted to the UI subsystem?

DOM access is:
- slow  
- side‑effectful  
- tightly coupled to rendering  

Keeping it isolated:
- prevents logic layers from becoming UI‑aware  
- keeps rendering predictable  
- avoids accidental performance issues  
- makes the system testable  

---

# 5. Why do gaugeFactory and update-engine exist?

Because gauges must be:
- created in one place  
- updated in one place  
- configured consistently  
- easy to replace or upgrade  

Without these modules, gauge logic would scatter across the codebase.

---

# 6. Why does the orchestrator contain no logic?

The orchestrator (FTdx101Meters) is intentionally “dumb”:

- It wires subsystems together  
- It performs no calculations  
- It performs no formatting  
- It performs no DOM access  
- It performs no WebSocket work  

This keeps the architecture clean and prevents circular dependencies.

---

# 7. Why is formatting isolated to meter-formatters?

Formatting is UI‑specific.  
If formatting leaks into calibration or WebSocket layers, the system becomes brittle.

meter-formatters ensures:
- consistent text output  
- easy localisation  
- clean separation of concerns  

---

# 8. Why is the folder structure so rigid?

Because folder structure *is* architecture.

It ensures:
- contributors know where code belongs  
- Copilot knows where to generate code  
- drift is easy to detect  
- subsystems remain isolated  

---

# 9. Why do we use ES modules everywhere?

ES modules provide:
- predictable imports  
- no global namespace pollution  
- tree‑shaking  
- clarity of dependencies  
- compatibility with modern tooling  

---

# 10. Why is drift detection so important?

Drift is the silent killer of long‑term maintainability.

Drift leads to:
- duplicated logic  
- unpredictable behaviour  
- broken boundaries  
- performance regressions  
- architectural collapse  

`drift-check.md` exists to prevent this.

---

# 11. Why do we have a Copilot compliance test?

Because Copilot is a powerful assistant — but only when aligned with the architecture.

The compliance test ensures:
- Copilot loads the rules  
- Copilot respects subsystem boundaries  
- Copilot generates correct code  
- Copilot avoids drift  

It is essential for maintaining architectural integrity.

---

# 12. Why is everything documented so thoroughly?

Because:
- contributors change  
- Copilot updates  
- memory fades  
- architecture must persist  

Documentation is the backbone of a stable, long‑lived system.

---

# 13. What should I do if I’m unsure where something belongs?

Use this rule:

**If it touches the DOM → UI subsystem**  
**If it draws a gauge → gaugeFactory/update-engine**  
**If it formats text → meter-formatters**  
**If it calibrates → calibration-engine**  
**If it routes WebSocket messages → WsUpdatePipeline**  
**If it wires subsystems → FTdx101Meters**  
**If it talks to the radio → serial/queue/decoding**  

If still unsure, check:
- subsystem-boundaries.md  
- architecture-overview.md  
- workflow.md  

---

# 14. Who owns the architecture?

The architecture is owned collectively by:
- the maintainers  
- the contributors  
- the documentation  
- the rules  
- the refactor workflow  

No single file defines it — the entire `.architecture` folder does.

---

# 15. What happens if a subsystem needs to evolve?

Follow the refactor workflow:
1. Identify scope  
2. Analyse current state  
3. Propose a plan  
4. Apply changes in small chunks  
5. Validate architecture  
6. Update documentation  

Architecture evolves — but never drifts.

