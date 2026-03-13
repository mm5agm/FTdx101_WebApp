# How to Run a Drift Check (Quick Guide)

A drift check is a diagnostic pass that identifies naming issues, subsystem violations, duplicated logic, folder/namespace drift, outdated comments, and empirical mismatches. It does not fix anything — it produces a structured report that the next refactor session will act on.

---

# 1. Prepare the Session

Before starting, ensure the following files exist:

- `.github/copilot-instructions.md` (global rules)
- `scripts/drift-check.md` (drift engine)
- Architecture maps in `scripts/`:
  - architecture-overview.md
  - subsystem-boundaries.md
  - command-flow-map.md
  - helper-map.md
  - radio-state-map.md
  - empirical-rules.md

These define the intended architecture.

---

# 2. Choose Files to Analyse

Select the files you want to check for drift. Typical triggers:

- Naming inconsistencies  
- Logic in the wrong subsystem  
- Duplicated helpers or decoding rules  
- UI/state leakage  
- Outdated or misleading comments  
- Empirical behaviour mismatches  
- Folder/namespace misalignment  

You can analyse a single file or an entire subsystem.

---

# 3. Start the Drift Check

In Copilot Chat:

1. Paste the entire contents of `scripts/drift-check.md`
2. Immediately follow it with a list of files to analyse, for example:

FILES TO CHECK:

Serial/SerialService.cs

Decoding/MeterDecoder.cs

UI/State/RadioStateManager.cs

Copilot will begin the drift check automatically.

---

# 4. Copilot Analyses the Files

Copilot loads:

- Global rules  
- Architecture maps  
- Your file list  

It then checks for:

- Naming drift (PascalCase vs camelCase)  
- Subsystem boundary violations  
- Duplicated or divergent logic  
- Folder/namespace drift  
- Outdated comments or messaging  
- Empirical behaviour mismatches  

---

# 5. Copilot Produces a Drift Report

The report includes:

- A high‑level summary  
- Findings grouped by subsystem  
- Naming drift list  
- Duplicated logic list  
- Folder/namespace drift list  
- Recommended refactor chunks  

This report is the blueprint for the next refactor session.

---

# 6. Review the Findings

Look for:

- Patterns of drift  
- Subsystems that need attention  
- Helpers that should be consolidated  
- Comments that no longer match behaviour  
- Empirical rules that need reinforcement  

You do not apply changes during a drift check — you only review the report.

---

# 7. Run a Refactor Session Next

Take the drift report and run:

- `scripts/refactor-session.md`

This will:

- Fix naming  
- Repair boundaries  
- Consolidate helpers  
- Update comments  
- Realign empirical behaviour  
- Clean up structure  

The refactor session resolves everything the drift check found.

---

# 8. Update Architecture Maps (If Needed)

If the drift check reveals new architectural insights:

- Update helper-map.md  
- Update subsystem-boundaries.md  
- Update command-flow-map.md  
- Update empirical-rules.md  
- Update architecture-overview.md  

These maps must always reflect the real architecture.

---

# Summary

A drift check:

- Detects drift  
- Uses architecture maps  
- Produces a structured report  
- Prepares the next refactor session  
- Keeps the architecture aligned and empirical  

It is the diagnostic half of the maintenance loop.

