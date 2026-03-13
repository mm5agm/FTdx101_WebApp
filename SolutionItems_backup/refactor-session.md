# Refactor Session Script

This script defines how Copilot must run a structured refactor session for this repository.  
All behaviour in this file is mandatory.  
All naming, architectural, and subsystem rules from `copilot-instructions.md` apply here.

---

# Purpose of a Refactor Session
A refactor session is a structured, multi‑chunk improvement pass across one or more files.  
Each session must:

- Strengthen architectural boundaries  
- Remove drift  
- Consolidate duplicated logic  
- Improve naming and intent clarity  
- Update comments and support messaging  
- Preserve empirical behaviour  
- Maintain subsystem separation  

The output must read like a clear narrative of architectural intent.

---

# Session Structure
A refactor session consists of **refactor chunks**.  
Each chunk must:

1. Begin with a one‑ or two‑sentence summary describing the primary drivers of the change.  
2. Contain a coherent set of related improvements.  
3. Provide standalone rationales for each individual change.  
4. Group rationales by subsystem (Serial, Queue, Decoding, UI/State, Messaging).  
5. Order subsystem groups dynamically based on which areas were touched most.  
6. Use natural, human phrasing throughout.

Chunks must be stable, meaningful units of improvement — not scattered edits.

---

# Naming Rules (Integrated)
Copilot must apply the project‑wide naming contract during every refactor:

## Architectural units — PascalCase
Use PascalCase for:
- Classes, records, structs  
- Services and subsystems  
- Models and DTOs  
- Public helpers  
- Namespaces and folders representing architectural units  
- Any extracted helper that becomes part of the architectural surface  

These names represent conceptual boundaries and must remain stable and descriptive.

## Flow‑level identifiers — camelCase
Use camelCase for:
- Local variables  
- Method parameters  
- Private fields (unless `_camelCase` is used)  
- Internal helper lambdas  
- Temporary state  
- Data flowing through refactor sessions  

These names represent behaviour and data, not architectural units.

## Boundary rule
If the identifier represents a **thing** in the architecture → PascalCase.  
If it represents **behaviour or data** → camelCase.

## Drift detection
Naming inconsistencies must be treated as architectural drift and corrected immediately.

---

# Subsystem Vigilance
Every refactor chunk must evaluate subsystem boundaries:

- Serial service layer  
- Async command queue  
- Decoding logic  
- UI/state flow  
- Support messaging  
- Comments and intent documentation  

No subsystem is privileged.  
All must be checked for drift, duplication, or conceptual leakage.

---

# Architectural Drift Detection
During each chunk, Copilot must watch for:

- Logic in the wrong subsystem  
- UI/state leakage into logic layers  
- Duplicated decoding or parsing rules  
- Divergent implementations of similar behaviour  
- Naming that no longer reflects actual behaviour  
- Comments that no longer match the code  
- Support messaging that no longer matches runtime behaviour  
- Naming that violates the PascalCase/camelCase boundary rules  

When drift is found, the chunk must correct it.

---

# Duplicated Logic
When duplication is detected:

- Extract shared behaviour into a new helper (PascalCase).  
- Replace all duplicated instances with calls to the new helper.  
- Update naming and comments to reflect the new structure.  
- Ensure the helper is placed in the correct subsystem folder/namespace.

---

# Helper Extraction Rules
When extracting helpers:

- Use PascalCase for architectural helpers.  
- Use camelCase for internal flow helpers.  
- Place helpers in the correct subsystem folder.  
- Update all references.  
- Add or update comments to reflect intent.  
- Ensure the helper name expresses behaviour clearly.

---

# Folder and Namespace Alignment
During each chunk, Copilot must:

- Move helpers into correct subsystem folders.  
- Create new folders/namespaces when conceptual boundaries emerge.  
- Merge or rename folders/namespaces when boundaries shift.  
- Keep naming aligned with the architectural vocabulary.  

The folder structure must always reflect the current architecture.

---

# Comments and Documentation
Copilot must:

- Update comments immediately when behaviour changes.  
- Remove outdated or misleading comments.  
- Ensure comments describe intent, not mechanics.  
- Keep comments aligned with empirical findings (CAT timing, decoding quirks, meter scaling).  

---

# Support Messaging
Copilot must:

- Keep user‑facing messages aligned with internal behaviour.  
- Improve clarity, empathy, and accuracy.  
- Remove outdated installation or troubleshooting text.  
- Ensure messaging reflects the current runtime and architecture.

---

# Empirical Behaviour
When empirical logic is involved:

- Preserve empirical findings.  
- Update decoding logic when new behaviour is discovered.  
- Ensure all related helpers, comments, and UI/state flows remain consistent.  

Empirical behaviour must never be overwritten by assumptions.

---

# Output Format
A refactor session output must contain:

- A sequence of refactor chunks  
- Each chunk with a summary, subsystem‑grouped rationales, and clear intent  
- Natural, concise, descriptive language  
- No boilerplate  
- A narrative that explains why the architecture is improving  

---

# File List
At the end of this script, the user will provide a list of files to refactor.  
Copilot must:

- Read the file list  
- Analyse the architecture  
- Produce refactor chunks in the order that best reflects the actual work needed  
- Apply all rules in this document and in `copilot-instructions.md`

---

# Begin Refactor Session
When the user pastes this script and a file list into Copilot Chat, Copilot must begin the refactor session immediately and produce the first chunk.

