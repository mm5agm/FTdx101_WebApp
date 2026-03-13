# Copilot Architectural Workflow

This project uses a closed-loop workflow consisting of three coordinated files:

- `copilot-instructions.md`
- `drift-check.md`
- `refactor-session.md`

Each file has a distinct role, and together they maintain a stable, empirical, well‑mapped architecture.

---

# Workflow Overview

                ┌──────────────────────────┐
                │  copilot-instructions.md │
                │  (Global Rules & Intent) │
                └──────────────┬───────────┘
                               │
                               ▼
                ┌──────────────────────────┐
                │      drift-check.md      │
                │ (Detect Drift & Issues)  │
                └──────────────┬───────────┘
                               │
                               ▼
                ┌──────────────────────────┐
                │   refactor-session.md    │
                │ (Repair & Improve Code)  │
                └──────────────┬───────────┘
                               │
                               ▼
                ┌──────────────────────────┐
                │  Updated Architecture     │
                │ (New Ground Truth)        │
                └──────────────┬───────────┘
                               │
                               ▼
                ┌──────────────────────────┐
                │  copilot-instructions.md │
                │ (Rules Remain Stable)    │
                └──────────────────────────┘

---

# File Roles

## copilot-instructions.md
Defines the permanent architectural contract:
- Naming rules (PascalCase for units, camelCase for flow)
- Subsystem boundaries
- Drift detection rules
- Folder/namespace evolution
- Refactor chunk structure
- Empirical behaviour preservation

This file is the **source of truth** for how the architecture should behave.

---

## drift-check.md
Runs a diagnostic pass to detect:
- Naming drift
- Subsystem violations
- Duplicated logic
- Divergent implementations
- Folder/namespace misalignment
- Outdated comments or messaging
- Empirical behaviour mismatches

It produces a **report**, not code changes.

---

## refactor-session.md
Consumes the drift-check report and performs:
- Coherent refactor chunks
- Boundary corrections
- Naming fixes
- Helper extraction
- Folder/namespace reorganisation
- Comment and messaging updates
- Empirical behaviour alignment

It produces **architectural improvements**.

---

# Closed-Loop Behaviour

1. **copilot-instructions.md** defines the rules.  
2. **drift-check.md** finds where the code has drifted from those rules.  
3. **refactor-session.md** repairs the drift and evolves the architecture.  
4. The improved architecture becomes the new ground truth.  
5. The cycle repeats, keeping the system clean, empirical, and aligned.

---

# Summary

This workflow ensures:
- Naming stays consistent  
- Subsystems remain cleanly separated  
- Architecture evolves intentionally  
- Drift is detected early  
- Refactors are structured and meaningful  
- Empirical behaviour is preserved  
- The codebase remains a living, mapped system  

