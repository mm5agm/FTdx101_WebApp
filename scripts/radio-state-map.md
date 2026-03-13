# Radio State Map

This document defines all pieces of radio state tracked by the application and describes how each state value flows through the system:  
Serial → Async Command Queue → Decoding → UI/State.

All actions must follow the project-wide rules defined in `.github/copilot-instructions.md`.

## Purpose
The radio state map ensures:
- Every piece of state has a clear definition and purpose.
- State flows through the correct subsystems in the correct order.
- Decoding logic remains consistent with UI/state representation.
- Helpers and naming remain aligned with actual behaviour.
- Architectural drift is detected early.
- Empirical behaviour is documented and preserved.

This file grows as new state values are added to the application.

## Instructions for Copilot
When this file is invoked with a list of files:

1. Identify all radio state values, including:
   - Power output
   - ALC level
   - S-meter level
   - Mode
   - Frequency
   - Split status
   - TX/RX status
   - Filter settings
   - Any additional decoded values

2. For each state value, record:
   - Name of the state value.
   - Subsystem where it originates (usually Serial).
   - How it flows through the Async Command Queue.
   - How it is decoded.
   - How it is represented in UI/state.
   - Any empirical rules applied to it.
   - Any helpers involved in its processing.

3. Detect and list:
   - State values implemented inconsistently across files.
   - State values decoded in multiple places.
   - State values updated directly in UI/state without decoding.
   - State values that need clearer naming.
   - Missing or outdated comments.
   - State values that should be extracted into helpers.
   - State values that belong in a different subsystem.

4. Produce a structured radio state map report.
5. Do not refactor here — only report.

## Output Format
Copilot must produce:

### State Value List
For each state value:
- Name
- Origin subsystem
- Queue flow
- Decoding logic
- UI/state representation
- Empirical rules
- Helpers involved
- Issues (if any)

### Consistency Issues
- State values decoded in multiple places.
- State values updated incorrectly.
- Missing or outdated comments.
- Naming inconsistencies.

### Consolidation Opportunities
- State values that should share helpers.
- State values that should be grouped.
- State values that need clearer decoding boundaries.

### Subsystem Alignment Issues
- State values handled in the wrong subsystem.
- State values bypassing the queue.
- State values decoded in UI/state.

## Usage
Paste a list of files below this line to generate a radio state map.

