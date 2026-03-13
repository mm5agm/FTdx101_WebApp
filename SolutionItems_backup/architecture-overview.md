# Architecture Overview

This document provides a high-level description of the architectural structure used in this project.  
It exists to help Copilot maintain architectural clarity, detect drift, and keep subsystem boundaries consistent with the project’s design principles.

All actions must follow the project-wide rules defined in `.github/copilot-instructions.md`.

## Purpose
This overview defines the intended responsibilities, boundaries, and interactions between the major subsystems in the application.  
Copilot uses this document to:
- Detect architectural drift.
- Ensure new code fits the correct subsystem.
- Keep helper classes organised.
- Maintain consistent naming and folder structures.
- Preserve the project’s empirical behaviour model.

## Subsystems

### Serial Layer
Responsibilities:
- Low-level communication with the radio.
- Managing read/write operations.
- Handling timeouts, retries, and serial exceptions.
- Ensuring reliable I/O behaviour based on empirical timing.

Boundaries:
- Must not contain decoding logic.
- Must not contain UI/state logic.
- Must not bypass the async command queue.

### Async Command Queue
Responsibilities:
- Sequencing all CAT commands.
- Ensuring commands execute in strict order.
- Managing cancellation, retries, and concurrency.
- Acting as the single authority for command flow.

Boundaries:
- Must not contain serial I/O logic.
- Must not contain decoding logic.
- Must not contain UI/state logic.

### Decoding Layer
Responsibilities:
- Parsing CAT responses.
- Applying empirical scaling rules (e.g., ALC, S-meter).
- Interpreting raw radio data into meaningful values.
- Maintaining decoding helpers and empirical notes.

Boundaries:
- Must not perform serial I/O.
- Must not update UI/state directly.
- Must not contain queue sequencing logic.

### UI/State Layer
Responsibilities:
- Representing the current radio state.
- Updating UI elements based on decoded values.
- Formatting values for display.
- Maintaining clean separation from logic layers.

Boundaries:
- Must not perform decoding.
- Must not perform serial I/O.
- Must not manage command sequencing.

### Support Messaging
Responsibilities:
- Providing user-facing installation, troubleshooting, and runtime messages.
- Ensuring clarity, accuracy, and alignment with internal behaviour.
- Updating messaging when behaviour changes.

Boundaries:
- Must not contain logic or decoding.
- Must not contain serial or queue behaviour.

## Helper Classes
Helper classes must:
- Encapsulate shared behaviour extracted from duplicated logic.
- Live in folders/namespaces that match their subsystem.
- Be renamed or reorganised when conceptual boundaries evolve.
- Use camelCase with long descriptive names.

## Folder and Namespace Structure
The folder/namespace layout must reflect the subsystem boundaries above.  
Copilot must:
- Create new folders/namespaces when helpers grow.
- Merge or rename folders/namespaces when boundaries shift.
- Keep naming consistent with the project’s architectural vocabulary.

## Empirical Behaviour
This project includes empirical logic derived from real-world testing.  
Copilot must:
- Preserve empirical findings.
- Update decoding and timing logic when new behaviour is discovered.
- Keep comments and helpers aligned with empirical rules.

## Usage
This file is a reference for Copilot during:
- Refactor sessions
- Drift checks
- Naming audits
- Folder/namespace reorganisation
- Helper extraction

It defines the intended architecture so the project remains clean, modular, and aligned with real behaviour.
