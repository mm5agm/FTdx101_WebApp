# Refactor Session Runtime Flow

This diagram shows how a refactor session runs end‑to‑end, using the rules in
`.github/copilot-instructions.md` and the maps inside the `scripts` folder.

It illustrates the full lifecycle:
User input → Copilot analysis → Chunk generation → Architecture update.

---

# High-Level Flow

        ┌──────────────────────────────────────────────┐
        │ User pastes refactor-session.md + file list  │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Copilot loads global rules from               │
        │ .github/copilot-instructions.md               │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Copilot loads architecture maps from scripts/ │
        │ (boundaries, helpers, command flow, empirical │
        │ rules, radio state, architecture overview)    │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Copilot analyses the provided file list       │
        │ and identifies:                               │
        │  - drift                                      │
        │  - naming issues                              │
        │  - subsystem violations                       │
        │  - duplicated logic                           │
        │  - folder/namespace misalignment              │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Copilot generates the FIRST refactor chunk    │
        │  - summary of drivers                         │
        │  - subsystem-grouped rationales               │
        │  - coherent set of improvements               │
        │  - naming fixes (PascalCase/camelCase)        │
        │  - boundary corrections                       │
        │  - helper extraction                          │
        │  - comment updates                            │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Copilot continues generating chunks until     │
        │ all drift and inconsistencies are resolved    │
        │ according to the architecture maps and rules  │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Updated codebase now reflects:                │
        │  - correct subsystem boundaries               │
        │  - correct naming conventions                 │
        │  - consolidated helpers                       │
        │  - aligned comments and messaging             │
        │  - preserved empirical behaviour              │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Updated architecture becomes new ground truth │
        │ for next drift-check and next refactor        │
        └──────────────────────────────────────────────┘

---

# Detailed Step Breakdown

## 1. User initiates session
You paste:
- `refactor-session.md`
- A list of files to refactor

This triggers the workflow.

## 2. Copilot loads global rules
From `.github/copilot-instructions.md`, Copilot applies:
- Naming rules (PascalCase vs camelCase)
- Subsystem boundaries
- Drift detection rules
- Folder/namespace evolution rules
- Refactor chunk structure
- Empirical behaviour preservation

These rules govern the entire session.

## 3. Copilot loads architecture maps
From the `scripts` folder:
- architecture-overview.md
- subsystem-boundaries.md
- command-flow-map.md
- helper-map.md
- empirical-rules.md
- radio-state-map.md

These define the *actual* architecture Copilot must align to.

## 4. Copilot analyses the file list
Copilot identifies:
- Drift
- Naming inconsistencies
- Duplicated logic
- Boundary violations
- Folder/namespace issues
- Comment mismatches
- Empirical behaviour mismatches

## 5. Copilot generates refactor chunks
Each chunk includes:
- A summary of the drivers
- Subsystem-grouped rationales
- Coherent improvements
- Naming corrections
- Helper extraction
- Boundary fixes
- Comment updates

Chunks are produced until the architecture is clean.

## 6. Architecture is updated
The codebase now reflects:
- Correct subsystem boundaries
- Correct naming conventions
- Updated helpers
- Updated comments
- Preserved empirical behaviour

## 7. New ground truth
The updated architecture becomes the basis for:
- The next drift-check
- The next refactor session
- Future architectural evolution

---

# Summary

This runtime flow ensures:
- Refactors are structured and intentional  
- Drift is systematically removed  
- Architecture remains mapped and empirical  
- Naming stays consistent  
- Subsystems remain cleanly separated  
- Helpers stay organised  
- Comments and messaging stay aligned  
- The system evolves predictably  

