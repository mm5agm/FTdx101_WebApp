# FTdx101 WebApp — Developer Onboarding Guide

Welcome to the FTdx101 WebApp project. This guide provides a concise, practical introduction for new contributors. It explains the architecture, subsystem boundaries, development workflow, and the rules that keep the project maintainable and predictable.

---

# 1. Project Purpose

The FTdx101 WebApp is a browser‑based control and monitoring interface for the Yaesu FTdx101 series transceivers. It provides:

- Real‑time meter visualisation (S‑meter, Power, SWR, ALC, etc.)
- CAT command integration
- Responsive UI
- Accurate calibration of radio values
- A clean, modular architecture designed for long‑term maintainability

---

# 2. High-Level Architecture

The system is composed of strict subsystems:

- **WebSocket subsystem** — receives raw CAT data
- **Calibration engine** — pure functions that convert raw values
- **Orchestrator (FTdx101Meters)** — wires subsystems together
- **Meter subsystem** — UI rendering, gauges, formatting
- **UI/State subsystem** — DOM access, layout, canvas rendering
- **CAT/Serial/Queue/Decoding** — backend communication layers

Each subsystem has strict boundaries defined in `subsystem-boundaries.md`.

---

# 3. The Strict Value Pipeline

All meter values must follow this exact flow:

WebSocket  
    ↓  
WsUpdatePipeline  
    ↓  
calibration-engine  
    ↓  
FTdx101Meters  
    ↓  
MeterPanel.update()  
    ↓  
gaugeFactory + update-engine  
    ↓  
Canvas rendering

This pipeline must never be bypassed or rearranged.

---

# 4. Folder Structure

The repository is organised into subsystem folders:

/websocket  
/calibration  
/ui  
/orchestrators  
/serial  
/queue  
/decoding  
.copilot  
.architecture  

Each folder contains logic for one subsystem only.

---

# 5. Development Rules

These rules are enforced by `.copilot/rules.md` and must be followed at all times:

- No global variables
- No magic strings
- No duplication of logic
- No DOM access outside UI subsystem
- No calibration logic outside calibration-engine
- No gauge creation outside gaugeFactory
- No formatting outside meter-formatters
- No WebSocket logic outside websocket subsystem
- No orchestrator logic outside FTdx101Meters
- Pure functions where possible
- ES modules everywhere

---

# 6. How to Add or Modify Features

## Adding a new meter
- Add calibration logic to calibration-engine
- Add UI rendering to MeterPanel
- Add gauge configuration to gaugeFactory
- Add formatting to meter-formatters
- Update orchestrator wiring if needed

## Adding a new WebSocket message
- Add routing in WsUpdatePipeline
- Add calibration if required
- Add UI handling in MeterPanel

## Adding new UI elements
- Modify only the UI subsystem
- Keep DOM access isolated

---

# 7. Refactoring Workflow

All refactors must follow the process defined in `refactor-session.md`:

1. Identify scope  
2. Analyse current state  
3. Propose refactor plan  
4. Apply changes in small chunks  
5. Validate architecture  
6. Summarise outcome  

Refactors must never introduce drift.

---

# 8. Drift Detection

Architectural drift is detected and corrected using:

- `drift-check.md`
- Folder structure
- Subsystem boundaries
- Copilot rules

If drift is found, it must be corrected immediately.

---

# 9. Tooling and Conventions

- ES modules only  
- No inline layout logic (except canvas size)  
- CSS grid/flexbox for layout  
- Class-based meter specialisation  
- Pure functions for calibration  
- No side effects in logic layers  

---

# 10. Getting Started

1. Clone the repository  
2. Review `.copilot/rules.md`  
3. Review `architecture-overview.md`  
4. Review `subsystem-boundaries.md`  
5. Start development inside the correct subsystem folder  
6. Use the strict value pipeline as your guide  

Welcome to the project — and thank you for helping maintain a clean, professional architecture.
