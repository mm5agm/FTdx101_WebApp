# Scripts Overview

This folder contains the full set of maintenance, auditing, and architectural tools used by the project.  
Each script is a command file: open it, scroll to the bottom, and paste file paths or file contents under the “Paste below this line” section.  
Copilot will then execute the script’s rules and generate the appropriate report.

All scripts follow the project-wide rules defined in `.github/copilot-instructions.md`.

---

## How to Use These Scripts

1. Open any script in this folder.
2. Scroll to the bottom where it says “Paste a list of files below this line”.
3. Paste:
   - File paths, or
   - Folder paths, or
   - Actual file contents
4. Copilot will read the script instructions and produce the structured output.

You do not need to type any additional commands.  
The script file itself *is* the instruction set.

---

## Script List

### 1. refactor-session.md
Runs a full refactor session using the project’s rules:
- Coherent refactor chunks
- Subsystem‑grouped rationales
- Naming fixes
- Helper extraction
- Folder/namespace reorganisation

Use when you want Copilot to actively refactor code.

---

### 2. check-for-drift.md
Scans for:
- Architectural drift
- Naming drift
- Duplicated logic
- Subsystem violations
- Outdated comments
- Incorrect folder/namespace placement

Use when you want a diagnostic report before refactoring.

---

### 3. naming-audit.md
Audits all identifiers for:
- camelCase compliance
- Descriptive naming
- Correct subsystem alignment
- Private field naming consistency

Use when naming clarity has degraded or after large refactors.

---

### 4. architecture-overview.md
Defines the intended architecture:
- Serial layer
- Async command queue
- Decoding layer
- UI/state layer
- Support messaging

Use as a reference for subsystem responsibilities.

---

### 5. helper-map.md
Maps all helper classes and methods:
- Responsibilities
- Subsystem alignment
- Naming issues
- Consolidation opportunities

Use when helpers start to multiply or drift.

---

### 6. subsystem-boundaries.md
Defines strict rules for what belongs in each subsystem:
- Allowed responsibilities
- Forbidden responsibilities
- Helper placement rules
- Folder/namespace alignment

Use when enforcing architectural discipline.

---

### 7. empirical-rules.md
Documents all empirical behaviours:
- CAT timing quirks
- Meter scaling curves
- Decoding patterns
- Undocumented response formats

Use when decoding or timing logic changes.

---

### 8. radio-state-map.md
Maps every piece of radio state:
- Where it originates
- How it flows through Serial → Queue → Decoding → UI/State
- Empirical rules applied
- Helpers involved

Use when state handling becomes inconsistent.

---

### 9. command-flow-map.md
Maps every CAT command:
- Purpose
- Queue behaviour
- Timing requirements
- Decoding path
- UI/state impact
- Empirical rules

Use when adding new commands or debugging command flow.

---

## Folder Structure

Your repository should contain:
/scripts
refactor-session.md
check-for-drift.md
naming-audit.md
architecture-overview.md
helper-map.md
subsystem-boundaries.md
empirical-rules.md
radio-state-map.md
command-flow-map.md


---

## Best Practices

- Use **refactor-session.md** after any drift report or naming audit.
- Run **check-for-drift.md** regularly to keep architecture clean.
- Keep **empirical-rules.md** updated whenever you discover new CAT behaviour.
- Use **command-flow-map.md** when adding or modifying CAT commands.
- Use **helper-map.md** when helpers start to grow or duplicate.

---

## Notes

These scripts are developer tools.  
They do not participate in builds and should remain outside project `.csproj` folders.


