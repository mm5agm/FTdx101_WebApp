# FTdx101 WebApp — Copilot Compliance Test Prompt

This document provides a single prompt used to verify that Copilot is correctly loading and obeying the FTdx101 WebApp architecture.

## 1. Purpose of This Test
This test ensures Copilot:
- Loads .copilot/rules.md
- Understands subsystem boundaries
- Enforces the strict value pipeline
- Prevents architectural drift
- Generates code only within the correct subsystem
- Avoids DOM access outside UI subsystem
- Avoids calibration logic outside calibration-engine
- Avoids gauge creation outside gaugeFactory
- Avoids formatting outside meter-formatters
- Keeps orchestrator logic pure
- Respects folder structure
- Uses ES modules
- Avoids global state and magic strings

## 2. The Compliance Test Prompt
Use this prompt exactly:

Copilot, run a full architecture compliance check.

1. Summarise the architectural rules defined in:
   - .copilot/rules.md
   - .architecture/architecture-overview.md
   - .architecture/subsystem-boundaries.md
   - .architecture/workflow.md

2. Confirm the strict value pipeline:
   WebSocket → WsUpdatePipeline → calibration-engine → FTdx101Meters → MeterPanel.update() → gaugeFactory/update-engine → canvas

3. Confirm subsystem boundaries:
   - Calibration engine is pure and contains no DOM, UI, WebSocket, or gauge logic.
   - WebSocket subsystem contains no UI, DOM, calibration, or gauge logic.
   - Meter subsystem contains all UI rendering, gauge creation, gauge updates, and formatting.
   - Orchestrator contains no logic except wiring.
   - UI subsystem is the only place where DOM access is allowed.
   - CAT/Serial/Queue/Decoding contain no UI, DOM, calibration, or WebSocket logic.

4. Confirm enforcement rules:
   - No global variables
   - No magic strings
   - No duplication of logic
   - ES modules everywhere
   - Class-based meter specialisation
   - Pure functions where possible

5. Confirm folder structure matches subsystem boundaries.

6. Report any drift or inconsistencies.

## 3. Expected Output
A correct Copilot response must:
- Accurately summarise all architecture documents
- Reproduce the strict value pipeline exactly
- Identify subsystem boundaries correctly
- Confirm enforcement rules
- Confirm folder structure alignment
- Report any drift
- Avoid hallucinating or inventing rules

If Copilot fails to mention:
- the strict value pipeline
- DOM isolation
- gaugeFactory
- update-engine
- meter-formatters
- orchestrator purity
- calibration purity
- subsystem boundaries  
…it is not compliant.

## 4. What to Do If Copilot Fails
If Copilot produces an incomplete or incorrect summary:
1. Open .copilot/rules.md
2. Open architecture-overview.md
3. Open subsystem-boundaries.md
4. Open workflow.md
5. Restart VS Code
6. Run the test again

If it still fails:
- Perform a drift check using drift-check.md
- Ensure no files were renamed or moved
- Ensure .copilot/rules.md is in the correct folder
- Ensure no syntax errors exist

## 5. When to Run This Test
Run this test:
- After restarting VS Code
- After updating Copilot
- After adding new files
- After major refactors
- Before new feature work
- When Copilot suggestions feel “off”
- When subsystem boundaries seem ignored

## 6. Purpose of This Document
This file ensures Copilot remains aligned with the architecture. It must stay consistent with:
- .copilot/rules.md
- architecture-overview.md
- subsystem-boundaries.md
- workflow.md
- drift-check.md
