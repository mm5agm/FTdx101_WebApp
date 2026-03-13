# Master Architectural Workflow Diagram

This diagram shows how the entire Copilot-driven architecture maintenance system works as a closed loop.  
It unifies the global rules, drift detection, refactor sessions, and all architecture maps.

---

# Top-Level Loop

                ┌──────────────────────────────────────┐
                │ .github/copilot-instructions.md      │
                │ (Global Behaviour & Architecture Rules) 
                └───────────────────────┬──────────────┘
                                        │
                                        ▼
                ┌──────────────────────────────────────┐
                │ scripts/drift-check.md               │
                │ (Detect Drift & Misalignment)        │
                └───────────────────────┬──────────────┘
                                        │
                                        ▼
                ┌──────────────────────────────────────┐
                │ scripts/refactor-session.md          │
                │ (Repair, Consolidate, Realign)       │
                └───────────────────────┬──────────────┘
                                        │
                                        ▼
                ┌──────────────────────────────────────┐
                │ Updated Architecture                 │
                │ (New Ground Truth)                   │
                └───────────────────────┬──────────────┘
                                        │
                                        ▼
                ┌──────────────────────────────────────┐
                │ .github/copilot-instructions.md      │
                │ (Rules Remain Constant)              │
                └──────────────────────────────────────┘

This is the closed loop that keeps the system clean, empirical, and aligned.

---

# How Architecture Maps Feed the Loop

        ┌──────────────────────────────────────────────────────────────┐
        │ Architecture Maps (Ground Truth)                             │
        ├──────────────────────────────────────────────────────────────┤
        │ architecture-overview.md     → High-level conceptual model   │
        │ subsystem-boundaries.md      → Defines subsystem edges       │
        │ command-flow-map.md          → Command movement through app  │
        │ helper-map.md                → Helper class organisation     │
        │ radio-state-map.md           → UI/state & radio behaviour    │
        │ empirical-rules.md           → CAT timing & empirical quirks │
        └───────────────────────┬──────────────────────────────────────┘
                                │
                                ▼
        ┌──────────────────────────────────────────────────────────────┐
        │ Used by BOTH drift-check.md and refactor-session.md          │
        │ to detect drift and realign architecture.                    │
        └──────────────────────────────────────────────────────────────┘

---

# Full System Diagram (All Components)

                         ┌──────────────────────────────┐
                         │ .github/copilot-instructions │
                         │  (Global Rules & Contracts)  │
                         └───────────────┬──────────────┘
                                         │
                                         ▼
        ┌──────────────────────────────────────────────────────────────┐
        │ scripts/drift-check.md                                       │
        │  - Detects naming drift                                      │
        │  - Detects subsystem violations                              │
        │  - Detects duplicated logic                                  │
        │  - Detects folder/namespace drift                            │
        │  - Detects empirical mismatches                              │
        │  - Produces recommended refactor chunks                      │
        └───────────────┬──────────────────────────────────────────────┘
                        │
                        ▼
        ┌──────────────────────────────────────────────────────────────┐
        │ scripts/refactor-session.md                                  │
        │  - Applies naming rules                                       │
        │  - Fixes subsystem boundaries                                 │
        │  - Consolidates helpers                                       │
        │  - Updates comments & messaging                               │
        │  - Preserves empirical behaviour                              │
        │  - Produces coherent refactor chunks                          │
        └───────────────┬──────────────────────────────────────────────┘
                        │
                        ▼
        ┌──────────────────────────────────────────────────────────────┐
        │ Updated Architecture                                          │
        │  - Clean boundaries                                           │
        │  - Correct naming                                             │
        │  - No duplication                                             │
        │  - Accurate comments                                          │
        │  - Preserved empirical rules                                  │
        └───────────────┬──────────────────────────────────────────────┘
                        │
                        ▼
        ┌──────────────────────────────────────────────────────────────┐
        │ Architecture Maps (Ground Truth)                              │
        │  - architecture-overview.md                                   │
        │  - subsystem-boundaries.md                                    │
        │  - command-flow-map.md                                        │
        │  - helper-map.md                                              │
        │  - radio-state-map.md                                         │
        │  - empirical-rules.md                                         │
        └───────────────┬──────────────────────────────────────────────┘
                        │
                        ▼
        ┌──────────────────────────────────────────────────────────────┐
        │ Next Drift Check                                              │
        └──────────────────────────────────────────────────────────────┘

---

# Summary

This master diagram shows:

- **copilot-instructions.md** defines the rules.
- **drift-check.md** detects violations of those rules.
- **refactor-session.md** repairs the violations.
- **Architecture maps** define the intended structure.
- **The updated architecture becomes the new ground truth.**
- The loop repeats, keeping the system clean, empirical, and aligned.

