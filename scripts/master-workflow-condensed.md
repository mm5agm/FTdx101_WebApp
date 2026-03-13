# Master Architectural Workflow — Condensed Edition

This document provides a compact, print‑friendly summary of the entire Copilot‑driven architecture workflow. It is designed for quick reference or PDF export.

---

# Closed Loop Overview

        copilot-instructions.md
                │
                ▼
        drift-check.md
                │
                ▼
        refactor-session.md
                │
                ▼
        Updated Architecture
                │
                ▼
        copilot-instructions.md

This loop keeps the system clean, empirical, and aligned.

---

# Architecture Maps (Ground Truth)

- architecture-overview.md — conceptual model  
- subsystem-boundaries.md — subsystem edges  
- command-flow-map.md — command movement  
- helper-map.md — helper organisation  
- radio-state-map.md — UI/state behaviour  
- empirical-rules.md — CAT timing & quirks  

These define the intended architecture.

---

# Drift Check Flow (Summary)

1. User pastes drift-check.md + file list  
2. Load global rules  
3. Load architecture maps  
4. Analyse files for drift  
5. Group findings by subsystem  
6. Produce drift report  
7. Output becomes input to refactor session  

---

# Refactor Session Flow (Summary)

1. User pastes refactor-session.md + file list  
2. Load global rules  
3. Load architecture maps  
4. Analyse files  
5. Generate refactor chunks  
6. Continue until aligned  
7. Updated architecture becomes new ground truth  

---

# Unified System Diagram (Condensed)

- **copilot-instructions.md** defines rules  
- **drift-check.md** detects violations  
- **refactor-session.md** repairs them  
- **Architecture maps** define intended structure  
- **Updated architecture** becomes new ground truth  

The cycle repeats indefinitely.

---

# Purpose of This Document

This condensed version provides a single-page reference for:
- Printing  
- PDF export  
- Quick onboarding  
- High-level architectural review  

