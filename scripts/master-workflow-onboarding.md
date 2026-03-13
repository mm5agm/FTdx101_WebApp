# Welcome to the Architecture Workflow

This project uses a structured, self-correcting workflow to keep the codebase clean, consistent, and aligned with the real behaviour of the radio. This document explains the workflow in plain language so new contributors can understand how everything fits together.

---

# The Big Picture

The project is maintained using three key documents:

1. **copilot-instructions.md**  
   Defines the rules the codebase must follow.

2. **drift-check.md**  
   Finds places where the code has drifted away from those rules.

3. **refactor-session.md**  
   Fixes the drift and improves the architecture.

These three documents form a loop that keeps the system healthy.

---

# How the Loop Works

### 1. Global Rules
The file `.github/copilot-instructions.md` defines:
- Naming conventions  
- Subsystem boundaries  
- How helpers should be organised  
- How comments should describe intent  
- How empirical behaviour must be preserved  

These rules apply to the entire project.

### 2. Drift Check
When you run a drift check:
- Copilot scans the files you provide  
- It compares them against the architecture maps  
- It reports anything that looks wrong or inconsistent  

The drift check **does not fix anything** — it only reports issues.

### 3. Refactor Session
When you run a refactor session:
- Copilot uses the drift report  
- It applies the global rules  
- It fixes naming, boundaries, helpers, comments, and structure  
- It produces clear, coherent refactor chunks  

This updates the architecture.

### 4. New Ground Truth
After the refactor session:
- The architecture is clean again  
- The maps remain accurate  
- The system is ready for the next drift check  

This creates a stable, predictable development cycle.

---

# Architecture Maps

The `scripts` folder contains several documents that describe how the system works:

- **architecture-overview.md** — the big picture  
- **subsystem-boundaries.md** — what belongs where  
- **command-flow-map.md** — how commands move  
- **helper-map.md** — where helpers live  
- **radio-state-map.md** — how state flows  
- **empirical-rules.md** — real-world quirks  

These documents act as the “source of truth” for the architecture.

---

# Why This Workflow Exists

This project interacts with real hardware and has many moving parts.  
Without a structured workflow:

- Naming becomes inconsistent  
- Logic leaks between subsystems  
- Helpers multiply and drift  
- Comments become outdated  
- Empirical behaviour gets overwritten  
- Architecture becomes unclear  

The workflow prevents all of this.

---

# What Contributors Need to Know

- You don’t need to memorise every rule — the scripts enforce them.  
- When making changes, run a drift check to see what needs attention.  
- When cleaning up or improving code, run a refactor session.  
- The architecture maps help you understand how the system is meant to work.  
- The workflow ensures consistency across the entire project.

---

# Final Notes

This workflow is designed to:
- Keep the architecture clean  
- Preserve empirical behaviour  
- Make refactors predictable  
- Help contributors understand the system  
- Ensure long-term maintainability  

If you follow the loop, the architecture will always stay aligned.

