# How to Run a Refactor Session (Quick Guide)

This guide explains the fastest, safest way to run a refactor session using the project’s architecture workflow.  
A refactor session applies the global rules, fixes drift, and realigns the codebase.

---

# 1. Prepare the Session

Before starting, make sure you have:

- `.github/copilot-instructions.md` (global rules)
- `scripts/refactor-session.md` (refactor engine)
- Architecture maps in `scripts/` (boundaries, helpers, command flow, empirical rules, etc.)

These files define the architecture Copilot must follow.

---

# 2. Decide What to Refactor

Choose the files you want Copilot to work on.  
Typical triggers:

- Naming inconsistencies  
- Logic in the wrong subsystem  
- Duplicated helpers or decoding rules  
- UI/state leakage  
- Outdated comments  
- Empirical behaviour mismatches  
- Folder/namespace drift  

You can refactor a single file or a whole subsystem.

---

# 3. Start the Session

In Copilot Chat:

1. Paste the entire contents of `scripts/refactor-session.md`
2. Immediately follow it with a list of files to refactor, for example:

FILES TO REFACTOR:

Serial/SerialService.cs

Decoding/MeterDecoder.cs

UI/State/RadioStateManager.cs


Copilot will begin the session automatically.

---

# 4. Copilot Generates Refactor Chunks

Each chunk includes:

- A short summary of the drivers  
- Subsystem‑grouped rationales  
- Coherent improvements  
- Naming fixes (PascalCase vs camelCase)  
- Boundary corrections  
- Helper extraction  
- Comment and messaging updates  
- Folder/namespace alignment  

Chunks continue until the architecture is clean.

---

# 5. Apply the Changes

For each chunk:

- Review the proposed changes  
- Apply them to your codebase  
- Commit them as a single atomic change  
- Move to the next chunk  

This keeps your history clean and auditable.

---

# 6. Finish the Session

When Copilot says the session is complete:

- All drift should be resolved  
- Naming should be consistent  
- Helpers should be consolidated  
- Comments should reflect intent  
- Empirical behaviour should be preserved  
- Subsystems should be cleanly separated  

Your architecture is now aligned with the maps.

---

# 7. Update Architecture Maps (If Needed)

If the refactor revealed new insights:

- Update helper-map.md  
- Update subsystem-boundaries.md  
- Update command-flow-map.md  
- Update empirical-rules.md  
- Update architecture-overview.md  

These maps must always reflect the real architecture.

---

# 8. Run a Drift Check Later

After a few development cycles, run:

- `scripts/drift-check.md`

This will detect new drift and produce the next set of refactor chunks.

---

# Summary

A refactor session:

- Uses global rules  
- Uses architecture maps  
- Fixes drift  
- Produces coherent chunks  
- Realigns the architecture  
- Preserves empirical behaviour  
- Keeps the system clean and predictable  

This is the core maintenance loop of the project.

