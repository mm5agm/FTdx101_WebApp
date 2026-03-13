# Empirical Rules

This document records all empirical behaviours observed in the radio, CAT interface, timing characteristics, decoding patterns, and meter scaling.  
Copilot must use these rules to ensure decoding, serial timing, and UI/state logic remain consistent with real-world behaviour.

All actions must follow the project-wide rules defined in `.github/copilot-instructions.md`.

## Purpose
Empirical rules ensure:
- Decoding logic reflects actual radio behaviour.
- Serial timing is tuned to real-world performance.
- Meter scaling remains accurate and consistent.
- UI/state updates match observed values.
- Helpers and comments stay aligned with tested behaviour.
- Architectural drift does not introduce incorrect assumptions.

This file grows as new empirical findings are discovered.

## Instructions for Copilot
When this file is invoked with a list of files:

1. Identify all code that implements empirical behaviour, including:
   - CAT response patterns.
   - Timing adjustments.
   - Meter scaling rules.
   - Parsing quirks.
   - Behaviour that differs from documentation.
   - Observed edge cases.

2. For each empirical rule found, record:
   - The subsystem (Serial, Queue, Decoding, UI/State).
   - The file and location.
   - A short description of the empirical behaviour.
   - The observed pattern or rule.
   - Any known constraints or caveats.
   - Whether the implementation matches the rule.

3. Detect and list:
   - Empirical rules implemented inconsistently across files.
   - Rules duplicated in multiple places.
   - Rules that should be extracted into helpers.
   - Rules that need clearer comments.
   - Rules that belong in a different subsystem.
   - Rules that require updated naming.

4. Produce a structured empirical rules report.
5. Do not refactor here — only report.

## Output Format
Copilot must produce:

### Empirical Rule List
For each rule:
- Subsystem
- File and location
- Description of behaviour
- Observed pattern
- Implementation notes
- Consistency issues (if any)

### Consolidation Opportunities
- Rules implemented in multiple places.
- Rules that should be centralised in helpers.
- Rules that need clearer naming or comments.

### Subsystem Alignment Issues
- Rules implemented in the wrong subsystem.
- Rules that should move to decoding or serial layers.
- Rules that should not appear in UI/state.

### Documentation Improvements
- Rules missing comments.
- Rules with outdated or incorrect comments.
- Rules requiring clearer empirical notes.

## Usage
Paste a list of files below this line to generate an empirical rules report.

