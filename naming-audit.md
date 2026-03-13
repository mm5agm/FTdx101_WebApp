# Naming Audit

This file defines how Copilot should perform a naming audit within this project.
All actions must follow the project-wide rules defined in `.github/copilot-instructions.md`.

## Purpose
A naming audit ensures that all identifiers across the codebase follow the project's naming conventions:
- camelCase for all identifiers
- Longer, descriptive names that clearly express intent
- The same naming style applied to private fields and backing variables
- Names that reflect actual behaviour, not abbreviations or shorthand

The audit identifies inconsistencies and produces a clear report of what should be renamed.

## Instructions for Copilot
When this file is invoked with a list of files:

1. Scan the provided files for:
   - Identifiers that do not use camelCase.
   - Names that are too short, vague, or unclear.
   - Private fields or backing variables that do not follow the naming rules.
   - Names that no longer reflect the behaviour of the code.
   - Method names that do not describe their intent clearly.
   - Helper classes or methods whose names do not match their responsibilities.
   - Folder or namespace names that no longer align with the project’s conceptual boundaries.

2. For each naming issue found, list:
   - The identifier.
   - The file and location.
   - Why the name is problematic.
   - A recommended clearer, descriptive camelCase name.

3. Do not perform renaming here.
   Produce a structured naming audit report that can be used in a refactor session.

## Output Format
Copilot must produce:
- A list of naming issues.
- Each issue must include the identifier, location, explanation, and recommended name.
- Explanations must be concise and natural.
- Recommendations must follow the project’s naming conventions.

## Usage
Paste a list of files below this line to begin a naming audit.

