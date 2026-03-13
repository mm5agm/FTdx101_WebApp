# Command Flow Map

This document defines every CAT command used by the application, including its purpose, queue behaviour, decoding path, and state impact.  
It ensures that all commands follow the correct flow through the system:  
Serial → Async Command Queue → Decoding → UI/State.

All actions must follow the project-wide rules defined in `.github/copilot-instructions.md`.

## Purpose
The command flow map ensures:
- Each CAT command has a clear definition and responsibility.
- Commands always pass through the async command queue.
- Decoding logic is consistent and centralised.
- UI/state updates reflect decoded values, not raw responses.
- Helpers remain aligned with command behaviour.
- Architectural drift is detected early.

This file grows as new commands are added to the application.

## Instructions for Copilot
When this file is invoked with a list of files:

1. Identify all CAT commands, including:
   - Commands that query radio state.
   - Commands that change radio state.
   - Commands that return meter values.
   - Commands that require empirical decoding.
   - Commands that require special timing or sequencing.

2. For each command, record:
   - Command name (e.g., FA, IF, PC, RL, SM, AL).
   - Purpose of the command.
   - Whether it is a query, setter, or mixed.
   - How it flows through the async command queue.
   - Any timing or sequencing requirements.
   - How the response is decoded.
   - Which state values it updates.
   - Any empirical rules applied.
   - Helpers involved in sending or decoding it.

3. Detect and list:
   - Commands bypassing the queue.
   - Commands decoded in multiple places.
   - Commands with inconsistent naming.
   - Commands missing decoding helpers.
   - Commands missing comments or empirical notes.
   - Commands that should be grouped or consolidated.
   - Commands that need clearer naming or folder placement.

4. Produce a structured command flow map report.
5. Do not refactor here — only report.

## Output Format
Copilot must produce:

### Command List
For each command:
- Command name
- Purpose
- Query/set/mixed classification
- Queue behaviour
- Timing/ordering requirements
- Decoding logic
- UI/state impact
- Empirical rules
- Helpers involved
- Issues (if any)

### Consistency Issues
- Commands decoded in multiple places.
- Commands bypassing the queue.
- Commands with outdated or missing comments.
- Naming inconsistencies.

### Consolidation Opportunities
- Commands that should share helpers.
- Commands that should be grouped.
- Commands that need clearer decoding boundaries.

### Subsystem Alignment Issues
- Commands handled in the wrong subsystem.
- Commands decoded in UI/state.
- Commands performing serial I/O outside the serial layer.

## Usage
Paste a list of files below this line to generate a command flow map.

