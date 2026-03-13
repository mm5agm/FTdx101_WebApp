# Drift Check Runtime Flow

This diagram shows how a drift check runs end‑to‑end, using the rules in
`.github/copilot-instructions.md` and the architecture maps inside the `scripts` folder.

It illustrates the full lifecycle:
User input → Copilot analysis → Drift findings → Refactor session preparation.

---

# High-Level Flow

        ┌──────────────────────────────────────────────┐
        │ User pastes drift-check.md + file list       │
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
        │ and checks for:                               │
        │  - naming drift                               │
        │  - subsystem violations                       │
        │  - duplicated logic                           │
        │  - folder/namespace drift                     │
        │  - outdated comments                          │
        │  - empirical mismatches                       │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Copilot groups findings by subsystem          │
        │ (Serial, Queue, Decoding, UI/State, Messaging│
        │  + Comments/Docs + Naming + Structure)        │
        └───────────────────────────────┬──────────────┘
                                        │
                                        ▼
        ┌──────────────────────────────────────────────┐
        │ Copilot generates drift report sections:      │
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
        │ Drift report becomes input to next            │
        │ refactor-session.md run                       │
        └──────────────────────────────────────────────┘

---

# Detailed Step Breakdown

## 1. User initiates drift check
You paste:
- `drift-check.md`
- A list of files to analyse

This triggers the diagnostic workflow.

## 2. Copilot loads global rules
From `.github/copilot-instructions.md`, Copilot applies:
- Naming rules (PascalCase vs camelCase)
- Subsystem boundaries
- Drift detection rules
- Folder/namespace evolution rules
- Empirical behaviour preservation

These rules define what “drift” means.

## 3. Copilot loads architecture maps
From the `scripts` folder:
- architecture-overview.md
- subsystem-boundaries.md
- command-flow-map.md
- helper-map.md
- empirical-rules.md
- radio-state-map.md

These define the *intended* architecture.

## 4. Copilot analyses the file list
Copilot checks for:
- Naming inconsistencies
- Logic in the wrong subsystem
- Duplicated or divergent logic
- Folder/namespace misalignment
- Outdated or misleading comments
- Support messaging mismatches
- Empirical behaviour drift

## 5. Copilot groups findings by subsystem
Findings are organised into:
- Serial
- Queue
- Decoding
- UI/State
- Messaging
- Comments/Documentation
- Naming
- Structure (folders/namespaces)

## 6. Copilot generates the drift report
The report includes:
- A high‑level summary
- Detailed findings by subsystem
- Naming drift list
- Duplicated logic list
- Folder/namespace drift list
- Recommended refactor chunks

## 7. Output becomes input to refactor session
The next run of `refactor-session.md` uses the drift report to:
- Fix naming
- Repair boundaries
- Consolidate helpers
- Update comments
- Realign empirical behaviour
- Clean up structure

---

# Summary

This runtime flow ensures:
- Drift is detected early and systematically  
- Architecture maps remain the source of truth  
- Naming stays consistent  
- Subsystems remain cleanly separated  
- Helpers stay organised  
- Empirical behaviour is preserved  
- Refactor sessions always have a clear, actionable plan  

