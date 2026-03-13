# Scripts Folder Index

This folder contains all architectural maps, workflow scripts, and supporting documents used to maintain the structure, behaviour, and evolution of the project. Each file plays a specific role in the refactor and drift‑check workflow.

---

# Core Workflow Scripts

## refactor-session.md
Defines how Copilot performs a structured refactor session.  
Includes chunk rules, naming rules, subsystem grouping, architectural boundaries, and output format.

## drift-check.md
Defines how Copilot performs an architectural drift check.  
Identifies naming drift, subsystem violations, duplicated logic, folder/namespace issues, and empirical mismatches.

---

# Architecture Maps

## architecture-overview.md
High‑level conceptual map of the entire system.  
Describes the major subsystems, their responsibilities, and how they interact.

## subsystem-boundaries.md
Defines the boundaries between Serial, Queue, Decoding, UI/State, Messaging, and other subsystems.  
Used to detect architectural drift and ensure logic stays in the correct layer.

## command-flow-map.md
Describes how commands move through the system from UI to Serial and back.  
Useful for debugging, refactors, and drift detection.

## helper-map.md
Lists all helper classes and their responsibilities.  
Ensures helpers remain organised, non‑duplicated, and placed in the correct subsystem.

## radio-state-map.md
Documents how radio state is represented, updated, and consumed across the system.  
Ensures UI/state flow remains consistent and free of leakage.

## empirical-rules.md
Documents all empirical behaviour discovered through testing (CAT timing, decoding quirks, meter scaling, etc.).  
Ensures empirical logic is preserved and not overwritten by assumptions.

---

# Workflow Documentation

## workflow.md
Explains the closed‑loop workflow connecting:
- copilot-instructions.md  
- drift-check.md  
- refactor-session.md  

Shows how the architecture evolves through repeated cycles.

## scriptsREADME.md
Local documentation for the scripts folder.  
Explains how to run scripts, how they interact, and how the folder is organised.

## Contributor_ReadMe.md
Guidance for contributors working with the architecture, scripts, and workflow.  
Ensures consistent onboarding and shared understanding.

---

# Setup and Legacy Files

## SetupApplications.md / SetupApplications.txt
Setup notes for external tools or supporting applications.  
Not part of the architectural workflow but kept for reference.

## txt radio_state.README.txt
Legacy documentation related to radio state.  
Kept for historical context; may be merged into radio-state-map.md in future.

## refactor-session.txt
Older or backup version of the refactor session script.  
Safe to delete if no longer needed.

---

# Summary

This folder contains:
- The operational scripts that drive refactor and drift‑check workflows  
- The architectural maps that define subsystem boundaries and behaviour  
- The empirical rules that preserve real‑world behaviour  
- Supporting documentation for contributors and maintainers  

Together, these files form the architectural toolbox that keeps the project clean, empirical, and aligned with its intended design.
