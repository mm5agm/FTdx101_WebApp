# FTdx101 WebApp — Refactor Session Template

This document defines the structure and rules that must be followed during any refactor session in the FTdx101 WebApp. All refactors must preserve subsystem boundaries, maintain architectural purity, and avoid introducing drift.

---

# 1. Refactor Session Goals

Every refactor must aim to:

- Preserve subsystem boundaries
- Eliminate duplication
- Correct architectural drift
- Improve clarity and maintainability
- Maintain existing behaviour and performance
- Strengthen alignment with `.copilot/rules.md`

Refactors must never weaken or dilute the architecture.

---

# 2. Refactor Session Structure

Each refactor session must follow the steps below.

## Step 1 — Identify Scope
- Determine which subsystem(s) are involved.
- Identify the files that will be affected.
- Describe the problem or drift being addressed.

## Step 2 — Analyse Current State
- Summarise existing behaviour.
- Identify violations of `.copilot/rules.md`.
- Identify naming drift, folder drift, or logic leakage.
- Confirm whether the value pipeline is intact.

## Step 3 — Propose Refactor Plan
- Break the refactor into small, safe, reversible chunks.
- Explain the rationale for each chunk.
- Ensure each change respects subsystem boundaries.
- Ensure no UI, DOM, calibration, or WebSocket logic leaks across layers.

## Step 4 — Apply Changes
- Implement changes one chunk at a time.
- Keep each chunk self‑contained.
- Update comments and documentation immediately.
- Maintain readability and consistency.

## Step 5 — Validate Architecture
- Confirm no drift remains.
- Confirm the strict value flow is preserved:
  WebSocket → pipeline → calibration → orchestrator → UI → gauges.
- Confirm DOM access is isolated to UI subsystem.
- Confirm calibration remains pure.
- Confirm gauge creation is only in gaugeFactory.
- Confirm formatting is only in meter-formatters.

## Step 6 — Summarise Outcome
- Describe what was improved.
- List drift that was corrected.
- Note any follow‑up tasks.
- Confirm the architecture is now clean.

---

# 3. Rules for Refactoring

All refactors must follow these rules:

- Never mix concerns across subsystems.
- Never introduce global state.
- Never bypass the orchestrator.
- Never introduce DOM access outside UI subsystem.
- Never duplicate calibration or formatting logic.
- Never create gauges outside gaugeFactory.
- Never update gauges outside update-engine.
- Always update comments and documentation.
- Always maintain folder boundaries.

---

# 4. Output Format for Refactor Sessions

Each refactor session must produce:

1. **Summary**  
   A short description of the purpose of