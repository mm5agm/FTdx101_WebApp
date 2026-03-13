# Helper Map

This document tracks all helper classes and helper methods in the project, along with their intended responsibilities and subsystem alignment.  
It exists to help Copilot maintain architectural clarity, avoid duplication, and ensure helpers remain correctly organised as the codebase evolves.

All actions must follow the project-wide rules defined in `.github/copilot-instructions.md`.

## Purpose
The helper map provides:
- A central reference for all helper classes and extracted shared logic.
- A way to detect duplicated behaviour before it spreads.
- A guide for where new helpers should live.
- A record of conceptual boundaries as they evolve.
- A tool for Copilot to reorganise helpers into correct folders/namespaces.

This file grows over time as new helpers are created through aggressive consolidation.

## Instructions for Copilot
When this file is invoked with a list of files:

1. Identify all helper classes and helper methods in the provided files.
2. For each helper, record:
   - Name of the helper class or method.
   - Subsystem it belongs to (Serial, Queue, Decoding, UI/State, Messaging).
   - Folder/namespace where it currently lives.
   - A short description of its responsibility.
   - Whether its name matches the project’s naming conventions.
   - Whether its folder/namespace matches the subsystem boundaries.
3. Detect and list:
   - Helpers that duplicate behaviour.
   - Helpers that belong in a different subsystem.
   - Helpers that should be merged.
   - Helpers that should be split into smaller units.
   - Helpers that need renaming for clarity.
   - Helpers that need to be moved to a new or renamed folder/namespace.
4. Produce a structured helper map report.
5. Do not perform refactors here — only report.

## Output Format
Copilot must produce a report containing:

### Helper List
For each helper:
- Helper name
- Subsystem
- Folder/namespace
- Responsibility summary
- Naming issues (if any)
- Location issues (if any)

### Consolidation Opportunities
- List helpers with overlapping or duplicated behaviour.
- Describe how they relate.
- Suggest which helper should become the canonical implementation.

### Reorganisation Opportunities
- Helpers that should move to a different folder/namespace.
- Helpers that should be grouped together.
- Helpers that should be split or merged.

### Naming Improvements
- Helpers with unclear or non-descriptive names.
- Recommended camelCase descriptive names.

## Usage
Paste a list of files below this line to generate a helper map.

