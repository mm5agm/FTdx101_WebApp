# Subsystem Boundaries

This document defines the strict boundaries between subsystems in this project.  
Copilot must use these boundaries to maintain architectural clarity, prevent drift, and ensure that logic always lives in the correct layer.

All actions must follow the project-wide rules defined in `.github/copilot-instructions.md`.

## Purpose
Subsystem boundaries ensure:
- Logic stays in the correct layer.
- Responsibilities remain clear and non-overlapping.
- Helpers are placed in the right folders/namespaces.
- Refactors do not blur architectural intent.
- Empirical behaviour is preserved and applied consistently.

This file acts as a reference for Copilot during refactor sessions, drift checks, naming audits, and helper reorganisation.

## Serial Layer
Responsibilities:
- Low-level communication with the radio.
- Managing serial port configuration.
- Performing read/write operations.
- Handling timeouts, retries, and serial exceptions.
- Ensuring reliable I/O based on empirical timing.

Forbidden:
- Decoding CAT responses.
- Updating UI or application state.
- Managing command sequencing.
- Formatting values for display.

## Async Command Queue
Responsibilities:
- Sequencing all CAT commands.
- Ensuring strict ordering and controlled concurrency.
- Managing cancellation and retries.
- Acting as the single authority for command flow.

Forbidden:
- Performing serial I/O directly.
- Decoding or interpreting CAT responses.
- Updating UI/state.
- Storing radio state.

## Decoding Layer
Responsibilities:
- Parsing CAT responses into structured values.
- Applying empirical scaling rules (e.g., ALC, S-meter).
- Interpreting raw radio data into meaningful domain values.
- Maintaining decoding helpers and empirical notes.

Forbidden:
- Performing serial I/O.
- Managing command sequencing.
- Updating UI/state.
- Formatting values for display.

## UI/State Layer
Responsibilities:
- Representing the current radio state.
- Updating UI elements based on decoded values.
- Formatting values for display.
- Managing user interactions.

Forbidden:
- Performing decoding.
- Performing serial I/O.
- Managing command sequencing.
- Applying empirical scaling rules.

## Support Messaging
Responsibilities:
- Providing user-facing installation, troubleshooting, and runtime messages.
- Ensuring clarity, accuracy, and alignment with internal behaviour.
- Updating messaging when behaviour changes.

Forbidden:
- Containing logic, decoding, or serial behaviour.
- Managing state or sequencing.
- Storing empirical rules.

## Helper Classes
Rules:
- Helpers must belong to exactly one subsystem.
- Helpers must live in folders/namespaces that match their subsystem.
- Helpers must be renamed or reorganised when conceptual boundaries evolve.
- Helpers must use camelCase with long descriptive names.
- Helpers must encapsulate shared behaviour extracted from duplicated logic.

Forbidden:
- Cross-subsystem helpers.
- Helpers that mix responsibilities.
- Helpers that bypass subsystem boundaries.

## Folder and Namespace Alignment
Copilot must ensure:
- Folder structure mirrors subsystem boundaries.
- Namespaces reflect the same boundaries.
- Helpers are moved when boundaries evolve.
- Folders/namespaces are renamed when conceptual clarity improves.

## Empirical Behaviour
Rules:
- Empirical findings must live in the decoding layer or decoding helpers.
- Serial timing adjustments must live in the serial layer.
- Comments must document empirical behaviour clearly.
- UI/state must never contain empirical logic.

## Usage
This file is a reference for Copilot during:
- Refactor sessions
- Drift checks
- Naming audits
- Helper extraction
- Folder/namespace reorganisation

Subsystem boundaries must be enforced consistently across the entire project.
