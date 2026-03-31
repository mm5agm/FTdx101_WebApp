# FTdx101 WebApp — Architecture Compliance Checklist

This checklist provides a fast, high‑signal way to verify that any new code, refactor, or feature complies with the FTdx101 WebApp architecture.  
Use this checklist before committing code or merging PRs.

---

# 1. Subsystem Boundaries

## Calibration Engine
- [ ] Pure functions only  
- [ ] No DOM access  
- [ ] No UI logic  
- [ ] No WebSocket logic  
- [ ] No gauge creation or updates  
- [ ] No formatting logic  
- [ ] No global state  

## WebSocket Subsystem
- [ ] Transport logic only  
- [ ] No UI logic  
- [ ] No DOM access  
- [ ] No calibration logic  
- [ ] No gauge logic  
- [ ] No formatting logic  

## Meter Subsystem
- [ ] MeterPanel owns all DOM access  
- [ ] gaugeFactory creates all gauges  
- [ ] update-engine updates all gauges  
- [ ] meter-formatters handle all formatting  
- [ ] No calibration logic  
- [ ] No WebSocket logic  
- [ ] No decoding or serial logic  

## Orchestrator (FTdx101Meters)
- [ ] Wires subsystems together  
- [ ] No calibration logic  
- [ ] No DOM access  
- [ ] No formatting logic  
- [ ] No gauge creation  
- [ ] No WebSocket logic  

## UI/State Subsystem
- [ ] All DOM access is here  
- [ ] Canvas rendering only here  
- [ ] Layout logic only here  
- [ ] No calibration logic  
- [ ] No decoding logic  
- [ ] No serial/queue logic  
- [ ] No WebSocket logic  

## CAT / Serial / Queue / Decoding
- [ ] Serial timing only  
- [ ] Command queueing only  
- [ ] CAT decoding only  
- [ ] No UI logic  
- [ ] No DOM access  
- [ ] No calibration logic  
- [ ] No WebSocket logic  

---

# 2. Strict Value Pipeline

Verify the pipeline is intact:

- [ ] WebSocket  
- [ ] WsUpdatePipeline  
- [ ] calibration-engine  
- [ ] FTdx101Meters  
- [ ] MeterPanel.update()  
- [ ] gaugeFactory + update-engine  
- [ ] Canvas rendering  

No shortcuts. No bypasses. No reordering.

---

# 3. Enforcement Rules

- [ ] No global variables  
- [ ] No magic strings  
- [ ] No duplication of logic  
- [ ] ES modules everywhere  
- [ ] Class-based meter specialisation  
- [ ] Pure functions where possible  
- [ ] Folder structure respected  
- [ ] Naming matches subsystem conventions  

---

# 4. UI Rules

- [ ] DOM access only in UI subsystem  
- [ ] No inline layout logic except canvas sizing  
- [ ] No gauge creation outside gaugeFactory  
- [ ] No gauge updates outside update-engine  
- [ ] No formatting outside meter-formatters  

---

# 5. Calibration Rules

- [ ] Calibration is pure  
- [ ] No DOM access  
- [ ] No UI logic  
- [ ] No WebSocket logic  
- [ ] No formatting  
- [ ] No gauge logic  

---

# 6. WebSocket Rules

- [ ] Transport only  
- [ ] No UI logic  
- [ ] No DOM access  
- [ ] No calibration  
- [ ] No formatting  
- [ ] No gauge logic  

---

# 7. Orchestrator Rules

- [ ] No logic except wiring  
- [ ] No DOM access  
- [ ] No formatting  
- [ ] No calibration  
- [ ] No gauge creation  

---

# 8. Drift Check

Before merging code:

- [ ] No subsystem leakage  
- [ ] No naming drift  
- [ ] No folder drift  
- [ ] No empirical logic in wrong layer  
- [ ] No formatting drift  
- [ ] No pipeline violations  

---

# 9. Final Verification

- [ ] Code matches `.copilot/rules.md`  
- [ ] Code matches `architecture-overview.md`  
- [ ] Code matches `subsystem-boundaries.md`  
- [ ] Code matches `workflow.md`  
- [ ] Code passes drift-check.md  
- [ ] Code passes copilot-compliance-test.md  

If all boxes are checked, the change is architecturally safe.
