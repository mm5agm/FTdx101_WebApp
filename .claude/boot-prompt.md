# Claude Boot Prompt for FTdx101 WebApp

Load and follow all architectural rules defined in `.claude/rules.md` and `.claude/project-overview.md` for this workspace.

## Acknowledgment Required
Acknowledge when the rules are loaded.

## Strict Architectural Compliance

For all code generation, refactoring, explanations, and suggestions, you must strictly follow the FTdx101 WebApp architecture:

### Subsystem Boundaries (Non-Negotiable)

- **Calibration engine**: pure functions only, no DOM, no UI, no WebSocket, no formatting.
- **WebSocket subsystem**: transport and routing only, no DOM, no UI, no calibration, no formatting.
- **Meter subsystem**: MeterPanel, gaugeFactory, update-engine, meter-formatters; DOM allowed only here.
- **Orchestrator**: FTdx101Meters wires subsystems together but contains no logic from them.
- **CAT/Serial/Queue/Decoding subsystems**: isolated, no UI, no DOM, no WebSocket, no calibration.
- **UI/state subsystem**: the ONLY place where DOM access is allowed.

### Required Practices

- Gauges must be created only through gaugeFactory.
- Formatting must live only in meter-formatters.js.
- All meter values must follow the strict pipeline:
  ```
  WebSocket → WsUpdatePipeline → calibration-engine → FTdx101Meters → 
  MeterPanel.update() → gaugeFactory/update-engine → canvas
  ```

### Forbidden Practices

- No global variables, no global orchestrators, no global gauges.
- Maintain folder boundaries exactly as defined.
- Never mix subsystem responsibilities.
- Never bypass the orchestrator.
- Never generate direct RadialGauge creation outside gaugeFactory.
- Never generate DOM access outside UI/state modules.

## Pre-Generation Verification

Before generating any code, verify that your output complies with these rules.

If a user request would violate the architecture, propose a compliant alternative instead.

## Permissions

You have my permission to read and modify all files in this project.

**Confirm when ready.**
