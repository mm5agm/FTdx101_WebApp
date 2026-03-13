# Scripts Folder Relationship Diagram

This diagram shows how each file in the `scripts` folder relates to the others and how they support the refactor + drift‑check workflow.

                           ┌──────────────────────────┐
                           │  .github/copilot-        │
                           │     instructions.md       │
                           │ (Global Behaviour Rules)  │
                           └──────────────┬───────────┘
                                          │
                                          ▼
                     ┌────────────────────────────────────────┐
                     │        Core Workflow Scripts            │
                     ├────────────────────────────────────────┤
                     │  refactor-session.md                    │
                     │  drift-check.md                         │
                     └──────────────┬─────────────────────────┘
                                    │
                                    ▼
        ┌────────────────────────────────────────────────────────────────┐
        │                     Architecture Maps (Inputs)                 │
        ├────────────────────────────────────────────────────────────────┤
        │ architecture-overview.md     → High-level conceptual map       │
        │ subsystem-boundaries.md      → Defines subsystem edges         │
        │ command-flow-map.md          → How commands move through system│
        │ helper-map.md                → Where helpers live & why        │
        │ radio-state-map.md           → How radio state flows           │
        │ empirical-rules.md           → CAT timing & empirical quirks   │
        └───────────────────────┬────────────────────────────────────────┘
                                │
                                ▼
                     ┌────────────────────────────────────────┐
                     │      Workflow Documentation             │
                     ├────────────────────────────────────────┤
                     │ workflow.md                             │
                     │ scriptsREADME.md                        │
                     │ Contributor_ReadMe.md                   │
                     └──────────────────────┬─────────────────┘
                                            │
                                            ▼
                     ┌────────────────────────────────────────┐
                     │      Setup & Legacy Files               │
                     ├────────────────────────────────────────┤
                     │ SetupApplications.md / .txt             │
                     │ txt radio_state.README.txt              │
                     │ refactor-session.txt (old copy)         │
                     └────────────────────────────────────────┘


# Relationship Summary

- **copilot-instructions.md** (in `.github/`)  
  Defines the rules that *everything else* must follow.

- **refactor-session.md**  
  Uses all architecture maps to perform structured refactors.

- **drift-check.md**  
  Uses all architecture maps to detect drift and produce a diagnostic report.

- **Architecture maps**  
  Provide the ground truth that both refactor and drift-check scripts rely on.

- **workflow.md**  
  Explains how the entire system loops together.

- **scriptsREADME.md**  
  Explains how to use the folder.

- **Contributor_ReadMe.md**  
  Helps new contributors understand the architecture and workflow.

- **Setup & legacy files**  
  Useful references but not part of the core workflow.

