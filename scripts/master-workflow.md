# Master Architectural Workflow (Single‑Page Overview)

This document unifies the entire Copilot‑driven architecture workflow into a single reference page.  
It shows how global rules, drift detection, refactor sessions, and architecture maps form a closed, self‑correcting loop.

---

# 1. Top-Level Closed Loop

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

This loop keeps the system clean, empirical, and aligned.

---

# 2. Architecture Maps (Ground Truth)

        ┌──────────────────────────────────────────────────────────────┐
        │ Architecture Maps                                             │
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

These files define the intended architecture and serve as the reference model.

---

# 3. Refactor Session Runtime Flow

        ┌──────────────────────────────────────────────┐
        │ User pastes refactor-session.md + file list  │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Load global rules from copilot-instructions  │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Load architecture maps (boundaries, helpers, │
        │ command flow, empirical rules, radio state)  │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Analyse files for drift, naming issues,      │
        │ duplication, boundary violations, etc.       │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Generate refactor chunks:                    │
        │  - summary                                   │
        │  - subsystem-grouped rationales              │
        │  - coherent improvements                     │
        │  - naming fixes                              │
        │  - helper extraction                         │
        │  - boundary corrections                      │
        │  - comment updates                           │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Continue until architecture is aligned        │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Updated architecture becomes new ground truth │
        └──────────────────────────────────────────────┘

---

# 4. Drift Check Runtime Flow

        ┌──────────────────────────────────────────────┐
        │ User pastes drift-check.md + file list       │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Load global rules from copilot-instructions  │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Load architecture maps                       │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Analyse files for:                           │
        │  - naming drift                              │
        │  - subsystem violations                       │
        │  - duplicated logic                           │
        │  - folder/namespace drift                     │
        │  - outdated comments                          │
        │  - empirical mismatches                       │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Group findings by subsystem                   │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Produce drift report:                         │
        │  1. Summary                                   │
        │  2. Findings by subsystem                     │
        │  3. Naming drift                              │
        │  4. Duplicated logic                          │
        │  5. Folder/namespace drift                    │
        │  6. Recommended refactor chunks               │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Output becomes input to next refactor session │
        └──────────────────────────────────────────────┘

---

# 5. Unified System Diagram (Everything Together)

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

This single-page document shows how:

- **copilot-instructions.md** defines the rules  
- **drift-check.md** detects violations  
- **refactor-session.md** repairs them  
- **Architecture maps** define the intended structure  
- **The updated architecture becomes the new ground truth**  
- The loop repeats indefinitely  

This creates a stable, empirical, self-correcting architecture.

