# Drift Check Script

This script defines how Copilot must perform an architectural drift check across one or more files.  
A drift check is a focused, diagnostic pass that identifies inconsistencies, boundary violations, naming drift, duplicated logic, and conceptual misalignments.  
All rules from `copilot-instructions.md` and `refactor-session.md` apply here.

---

# Purpose of a Drift Check
A drift check ensures the codebase still reflects the true architecture.  
It must detect and report:

- Subsystem boundary violations  
- Naming inconsistencies  
- Duplicated or divergent logic  
- Outdated comments or intent documentation  
- UI/state leakage into logic layers  
- Empirical behaviour mismatches  
- Folder/namespace misalignment  
- Any conceptual drift from the mapped architecture  

A drift check does **not** perform refactoring.  
It produces a structured diagnostic report that the next refactor session will act on.

---

# Output Structure
A drift check output must contain:

1. A short, high‑level summary of the overall architectural health.  
2. A list of drift findings grouped by subsystem.  
3. Each finding must include:  
   - A clear description of the drift  
   - Why it matters  
   - The architectural boundary it violates  
4. A final section listing recommended refactor chunks to address the drift.

The output must be concise, natural, and narrative — not mechanical.

---

# Naming Drift Rules
Copilot must apply the project‑wide naming contract during drift checks.

## Architectural units — PascalCase
PascalCase must be used for:
- Classes, records, structs  
- Services and subsystems  
- Models and DTOs  
- Public helpers  
- Namespaces and folders representing architectural units  

Any deviation is drift.

## Flow‑level identifiers — camelCase
camelCase must be used for:
- Local variables  
- Method parameters  
- Private fields (unless `_camelCase` is used)  
- Internal helper lambdas  
- Temporary state  
- Data flowing through refactor sessions  

Any deviation is drift.

## Boundary rule
If the identifier represents a **thing** in the architecture → PascalCase.  
If it represents **behaviour or data** → camelCase.

## Naming drift examples
- PascalCase used for a flow variable  
- camelCase used for a conceptual unit  
- Mixed‑case or ambiguous names  
- Names that no longer match actual behaviour  

All must be reported.

---

# Subsystem Drift Rules
Copilot must check each subsystem independently:

- Serial service layer  
- Async command queue  
- Decoding logic  
- UI/state flow  
- Support messaging  
- Comments and intent documentation  

For each subsystem, Copilot must detect:

- Logic placed in the wrong subsystem  
- Divergent implementations of similar behaviour  
- Duplicated decoding or parsing rules  
- UI/state leakage into logic layers  
- Missing or outdated comments  
- Support messaging that no longer matches runtime behaviour  

Each issue must be reported with clear architectural reasoning.

---

# Duplicated Logic Detection
Copilot must identify:

- Repeated decoding rules  
- Repeated parsing logic  
- Repeated UI/state transitions  
- Repeated Serial or Queue behaviours  
- Helpers that should be consolidated  

Duplicated logic is always drift and must be listed.

---

# Empirical Behaviour Drift
Copilot must detect:

- Logic that contradicts known empirical behaviour  
- Missing comments explaining empirical quirks  
- Divergent implementations of empirical rules  
- UI/state flows that no longer match observed behaviour  

Empirical drift must be reported explicitly.

---

# Folder and Namespace Drift
Copilot must detect:

- Helpers in the wrong folder  
- Namespaces that no longer match conceptual boundaries  
- Missing subsystem folders  
- Folders that should be merged or renamed  
- Architectural units placed in utility folders  

All folder/namespace drift must be listed.

---

# Comment and Documentation Drift
Copilot must detect:

- Comments that no longer match behaviour  
- Comments describing mechanics instead of intent  
- Outdated or misleading comments  
- Missing comments for empirical behaviour  
- Support messaging that contradicts runtime behaviour  

Each issue must be reported with context.

---

# Output Format
A drift check must produce:

## 1. Summary
A short narrative describing the overall architectural health.

## 2. Findings by Subsystem
For each subsystem:
- A list of drift findings  
- Each finding with a clear explanation of the architectural boundary violated  

## 3. Naming Drift
A list of all naming inconsistencies, grouped by type.

## 4. Duplicated Logic
A list of all duplicated or divergent logic patterns.

## 5. Folder/Namespace Drift
A list of structural misalignments.

## 6. Recommended Refactor Chunks
A list of coherent refactor chunks that would resolve the drift.

---

# File List
At the end of this script, the user will provide a list of files to check.  
Copilot must:

- Read the file list  
- Analyse the architecture  
- Produce a drift‑check report in the format above  
- Defer all fixes to the next refactor session  

---

# Begin Drift Check
When the user pastes this script and a file list into Copilot Chat, Copilot must begin the drift check immediately and produce the diagnostic report.

