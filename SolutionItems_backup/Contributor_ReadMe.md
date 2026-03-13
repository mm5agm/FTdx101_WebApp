# FTDX101MP Web Controller

This project is a modular, empirically‑driven web application for controlling and monitoring the Yaesu FTDX101MP radio.  
It uses a clean subsystem architecture, strict naming conventions, and a Copilot‑assisted maintenance workflow to ensure long‑term clarity, reliability, and predictable evolution.

The project is designed around real‑world CAT behaviour, empirical timing, and accurate decoding of radio state.

---

## Architecture

The application is built around five core subsystems.  
Each subsystem has strict responsibilities and boundaries.

### Serial Layer
Handles low‑level communication with the radio:
- Serial port configuration
- Read/write operations
- Timeouts and retries
- Empirical timing behaviour

Forbidden:
- Decoding
- UI/state updates
- Command sequencing

### Async Command Queue
Ensures all CAT commands execute in strict order:
- Sequencing
- Concurrency control
- Cancellation and retries
- Timing enforcement

Forbidden:
- Serial I/O
- Decoding
- UI/state updates

### Decoding Layer
Interprets CAT responses:
- Parsing raw responses
- Applying empirical scaling (ALC, S‑meter, power)
- Multi‑field parsing (IF, status blocks)
- Decoding helpers

Forbidden:
- Serial I/O
- UI/state updates
- Command sequencing

### UI/State Layer
Represents the current radio state:
- Updating UI elements
- Formatting values for display
- Managing user interactions

Forbidden:
- Serial I/O
- Decoding
- Queue logic

### Support Messaging
Provides user‑facing runtime and troubleshooting messages:
- Clear, accurate, empathetic text
- Aligned with internal behaviour
- Updated when architecture changes

Forbidden:
- Logic or decoding
- Serial or queue behaviour

---

## Empirical Behaviour

The FTDX101MP exhibits several behaviours that differ from documentation.  
This project preserves and documents these empirical findings, including:

- Meter scaling curves (S‑meter, ALC)
- Timing quirks (TX/RX transitions, IF polling)
- Response formats that differ from manuals
- Latency differences between commands

Empirical rules are centralised in the decoding and serial layers.

---

## Folder Structure

A typical layout looks like:

/src
/Serial
/Queue
/Decoding
/UI
/Messaging
/scripts
(maintenance and audit tools)
/.github
copilot-instructions.md
README.md


The folder structure mirrors subsystem boundaries.

---

## Development Workflow

This project uses a Copilot‑assisted workflow with strict architectural rules.

### 1. Architectural Rules
Defined in:
