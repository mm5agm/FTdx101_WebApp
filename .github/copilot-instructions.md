# Project‑wide Behaviour
This project uses a structured, disciplined refactoring and maintenance workflow. Copilot must follow all rules in this document whenever generating, rewriting, or refactoring code, comments, documentation, or support messaging within this repository.

The goal is to maintain a clean, empirical, well‑structured architecture with clear intent, consistent naming, and predictable evolution.

# Refactor Structure
When performing any refactor, Copilot must:
1. Produce coherent refactor chunks — each chunk represents a stable, meaningful unit of improvement.
2. Begin each chunk with a one‑ or two‑sentence summary describing the primary drivers of the refactor.
3. Provide standalone rationales for each individual change within the chunk.
4. Group those rationalales by subsystem (e.g., Serial, Queue, Decoding, UI/State, Messaging).
5. Order subsystem groups dynamically based on which areas were touched most significantly in that refactor.
6. Use natural, human phrasing for all summaries and rationales.

# Subsystem Vigilance
Copilot must treat all subsystems as equally important:
- Serial service layer
- Async command queue
- Decoding logic
- UI/state flow
- Support messaging
- Comments and intent documentation

No subsystem receives special priority; all must be monitored for drift, duplication, or architectural issues.

# Architectural Drift Detection
Copilot must continuously watch for and correct:
- Logic appearing in the wrong subsystem
- UI/state leakage into logic layers
- Duplicated decoding or parsing rules
- Divergent implementations of similar behaviour
- Naming that no longer reflects actual behaviour
- Comments that no longer match the code
- Support messaging that no longer matches internal behaviour
- Naming that violates architectural boundary rules (PascalCase for units, camelCase for flow)

When drift is detected, Copilot must correct it as part of the next coherent refactor chunk.

# Duplicated Logic
Copilot must aggressively consolidate duplicated logic. When duplication is found:
- Extract shared behaviour into new helper methods or classes.
- Replace all duplicated instances with calls to the new abstraction.
- Ensure naming and comments reflect the new structure.

# Helper Classes and Organisation
As helper classes grow in number:
- Copilot must automatically reorganise them into appropriate folders and namespaces.
- Copilot must rename folders and namespaces when conceptual boundaries evolve.
- Copilot must ensure all references update cleanly.

The project structure must always reflect the current architecture, not legacy assumptions.

# Naming Conventions
Copilot must treat naming as part of the project’s architectural map. Names must reflect whether an identifier represents a conceptual unit in the architecture or flow‑level behaviour/data inside that architecture.

## Architectural units — PascalCase
Use PascalCase for anything that defines a stable conceptual boundary:
- Classes, records, structs
- Services and subsystems
- Models and DTOs
- Public helpers
- Namespaces and folders representing architectural units
- Any extracted helper that becomes part of the architectural surface

These names must remain stable and descriptive, reflecting the true conceptual role of the unit.

## Flow‑level identifiers — camelCase
Use camelCase for anything representing behaviour, data, or flow:
- Local variables
- Method parameters
- Private fields (unless the project uses _camelCase)
- Internal helper lambdas
- Temporary state
- Data flowing through refactor sessions

These names must be descriptive, intent‑revealing, and free of abbreviations.

## Boundary rule
If the identifier represents a thing in the architecture → PascalCase.  
If it represents behaviour or data → camelCase.

## Drift detection
Naming inconsistencies are treated as architectural drift:
- PascalCase used for flow‑level identifiers
- camelCase used for conceptual units
- Ambiguous or mixed‑case names
- Names that no longer reflect actual behaviour

Copilot must correct naming drift as part of the next coherent refactor chunk.

# Comments and Documentation
Copilot must:
- Immediately update comments when behaviour changes.
- Ensure comments describe intent, not just mechanics.
- Remove outdated or misleading comments.
- Keep comments aligned with empirical findings (e.g., CAT behaviour, timing quirks, decoding rules).

# Support Messaging
Copilot must:
- Keep user‑facing messages aligned with internal behaviour.
- Improve clarity, empathy, and accuracy when rewriting support text.
- Remove outdated installation or troubleshooting instructions.
- Ensure messaging reflects the current runtime, architecture, and workflow.

# Empirical Behaviour
This project includes empirical logic (e.g., CAT timing, decoding, meter scaling). Copilot must:
- Preserve empirical findings.
- Update decoding logic when new empirical behaviour is discovered.
- Ensure all related comments, helpers, and UI/state flows remain consistent.

# Folder and Namespace Evolution
Copilot must:
- Maintain a clean, modular folder structure.
- Create new folders/namespaces when helpers or logic clusters grow.
- Merge or rename folders/namespaces when conceptual boundaries shift.
- Keep naming consistent with the project’s architectural vocabulary.

# Output Style
When generating refactor chunks, Copilot must:
- Use natural, concise, descriptive language.
- Avoid boilerplate or generic phrasing.
- Keep rationales short but meaningful.
- Ensure summaries and rationales read like a clear narrative of architectural intent.

# Scope of Application
These rules apply to:
- Code
- Comments
- Documentation
- Support messaging
- Folder structure
- Namespaces
- Helper classes
- Refactor operations
- Architectural improvements

Copilot must follow these rules for all work performed within this repository.
