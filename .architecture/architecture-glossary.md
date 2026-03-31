# FTdx101 WebApp — Architecture Glossary

This glossary defines all key terms, subsystem names, architectural concepts, and vocabulary used throughout the FTdx101 WebApp.  
It ensures consistent language across documentation, code, and refactors.

---

# A

### Architecture  
The complete set of rules, boundaries, workflows, and structures that define how the FTdx101 WebApp is built and maintained.

### Architecture Drift  
Any deviation from the defined architecture, including subsystem leakage, naming drift, folder drift, or pipeline violations.

### Architecture Pipeline  
See **Value Pipeline**.

---

# C

### Calibration Engine  
A pure‑function subsystem that converts raw radio values into calibrated, meaningful meter values.  
Contains no DOM, UI, WebSocket, or gauge logic.

### CAT (Computer Aided Transceiver)  
The protocol used to communicate with the FTdx101 radio via serial commands.

### Command Queue  
Backend subsystem that ensures CAT commands are sent in the correct order and timing.

### Copilot Compliance Test  
A prompt used to verify that Copilot is correctly loading and respecting the architecture.

---

# D

### Decoding Layer  
Backend subsystem that interprets raw CAT responses into structured values.

### DOM Access  
Any interaction with the browser’s Document Object Model.  
Allowed only in the UI subsystem.

### Drift  
See **Architecture Drift**.

---

# E

### ES Modules  
The import/export module system used throughout the project.  
Required for all files.

### Empirical Logic  
Measured, observed, or radio‑specific behaviour.  
Allowed only in calibration, decoding, or serial timing layers.

---

# F

### Formatting  
UI‑specific text formatting for meter values.  
Must live only in `meter-formatters.js`.

---

# G

### Gauge  
A visual meter rendered on canvas (e.g., S‑meter, Power, SWR).  
Created only in `gaugeFactory.js`.

### Gauge Factory  
Subsystem responsible for creating all gauges with consistent configuration.

### Gauge Update Engine  
Subsystem responsible for updating gauge values efficiently and consistently.

---

# M

### MeterPanel  
The UI owner for all meter rendering.  
Handles DOM access, canvas setup, and delegating gauge creation and updates.

### Meter Subsystem  
The collection of UI modules responsible for rendering meters:
- MeterPanel  
- gaugeFactory  
- update-engine  
- meter-formatters  

---

# O

### Orchestrator (FTdx101Meters)  
A coordination layer that wires subsystems together.  
Contains no logic, no DOM access, no formatting, no calibration, and no WebSocket code.

---

# P

### Pipeline  
See **Value Pipeline**.

### Pure Function  
A function with no side effects that always returns the same output for the same input.  
Required in calibration.

---

# S

### Serial Layer  
Backend subsystem that communicates with the radio via serial port.

### Subsystem  
A self‑contained architectural unit with a clear purpose and strict boundaries.

### Subsystem Boundaries  
The rules defining what each subsystem is allowed and forbidden to do.

---

# U

### UI Subsystem  
The only subsystem allowed to access the DOM.  
Handles layout, canvas rendering, overlays, and user interaction.

### Update Pipeline  
See **Value Pipeline**.

---

# V

### Value Pipeline  
The mandatory flow for all meter values:

WebSocket  
→ WsUpdatePipeline  
→ calibration-engine  
→ FTdx101Meters  
→ MeterPanel.update()  
→ gaugeFactory + update-engine  
→ canvas rendering

Any bypass is considered drift.

---

# W

### WebSocket Subsystem  
Receives raw CAT data from the backend and routes it into the update pipeline.  
Contains no UI, DOM, calibration, or gauge logic.

### Workflow  
The end‑to‑end process describing how data moves through the system.  
Defined in `workflow.md`.

---

# Z

### Zero Drift  
The architectural goal: no leakage, no duplication, no bypasses, no violations.

