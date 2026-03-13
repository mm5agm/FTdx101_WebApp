# Architectural Drift Check

This file defines how Copilot should scan for architectural drift, naming drift, and duplicated logic within the project.  
All actions must follow the project-wide rules defined in `.github/copilot-instructions.md`.

## Purpose
A drift check identifies areas where the codebase has diverged from the project’s architectural principles, naming conventions, or empirical behaviour.

## Instructions for Copilot
When this file is invoked with a list of files:

1. Scan the provided files for:
   - Logic appearing in the wrong subsystem.
   - UI/state leakage into logic layers.
   - Duplicated decoding or parsing logic.
   - Divergent implementations of similar behaviour.
   - Naming that no longer reflects actual behaviour.
   - Private fields or backing variables that violate naming rules.
   - Comments that no longer match the code.
   - Support messaging that no longer matches internal behaviour.
   - Folder/namespace names that no longer reflect conceptual boundaries.

2. Identify all instances of drift and list them clearly.

3. For each instance, describe:
   - What the drift is.
   - Why it matters.
   - Which subsystem it belongs to.
   - What kind of refactor would resolve it.

4. Do not perform the refactor here.  
   Instead, produce a clear, actionable drift report that can be used in a refactor session.

## Usage
Paste a list of files below this line to begin a drift check.

