# FTdx101 WebApp — Architecture Index

This index provides a structured overview of all architectural documents in the `.architecture` folder.  
Use this as the starting point when exploring or modifying the system.

---

# 1. Core Architecture Documents

These documents define the system’s structure, rules, and design philosophy.

## **architecture-overview.md**
High‑level description of the entire FTdx101 WebApp architecture.  
Explains subsystems, value flow, design philosophy, and folder structure.

## **subsystem-boundaries.md**
Authoritative specification of what each subsystem is allowed and forbidden to do.  
Defines strict boundaries that prevent logic leakage.

## **workflow.md**
End‑to‑end workflow of the system, covering both frontend and backend flows.  
Explains how data moves through WebSocket → calibration → UI.

---

# 2. Enforcement & Quality Documents

These documents ensure the architecture remains clean and consistent.

## **drift-check.md**
Defines architectural drift, how to detect it, how to classify it, and how to correct it.  
Used during refactors and code reviews.

## **refactor-session.md**
Template and rules for performing safe, subsystem‑aligned refactors.  
Ensures changes are made in small, reversible, well‑documented chunks.

---

# 3. Contributor & Onboarding Documents

These documents help new developers understand and work within the architecture.

## **developer-onboarding.md**
A practical guide for new contributors.  
Explains how the system works, how to add features, and how to avoid drift.

---

# 4. Optional / Supporting Documents

These may be added as the project grows.

## **diagrams/**
Folder for architecture diagrams (PNG/SVG).  
Useful for visualising subsystem relationships and value flow.

## **future-guides/**
Space for additional documentation such as:
- Testing strategy  
- Performance notes  
- UI design guidelines  
- CAT protocol notes  

---

# 5. How to Use This Index

1. Start with **architecture-overview.md** to understand the big picture.  
2. Read **subsystem-boundaries.md** to learn the strict rules.  
3. Use **workflow.md** to understand how data flows through the system.  
4. When modifying code, consult **refactor-session.md**.  
5. When checking for violations, use **drift-check.md**.  
6. For new contributors, begin with **developer-onboarding.md**.

---

# 6. Document Ownership

All documents in this folder are part of the authoritative architecture.  
They must be kept up to date and aligned with:

- `.copilot/rules.md`
- The folder structure
- The strict value pipeline
- The subsystem boundaries

Changes to these documents must follow the refactor workflow.

---

# 7. Purpose of This Folder

The `.architecture` folder exists to:

- Provide a single source of truth for the system’s design  
- Prevent architectural drift  
- Guide contributors  
- Support Copilot’s understanding of the architecture  
- Maintain long‑term project health  

This folder is part of the living architecture and must be maintained with care.
