# UI Theming
This file defines how theming can be added without breaking architecture or performance.
## Principles
Themes must be optional, lightweight, UI‑only, and must not affect logic, pipeline, or subsystem boundaries.
## Allowed Changes
Colours, fonts, backgrounds, spacing, non‑meter UI elements.
## Forbidden Changes
Calibration, decoding, WebSocket logic, orchestrator behaviour, gauge logic, pipeline flow.
## Architecture Rules
Theme code lives only in UI subsystem; no global variables; meters remain canvas‑based; themes only adjust colours/fonts passed into gaugeFactory.
## Theme Object
{name:"dark",colors:{background,text,meterScale,meterNeedle},fonts:{base,labels},spacing:{padding,margin}}
## Loading Themes
Loaded at startup or via user settings; stored in UI state; never outside UI subsystem.
## Applying Themes
MeterPanel applies theme colours to gaugeFactory; UI components read from a theme provider; no inline styles.
## Performance Rules
Theme changes must not trigger full reflows; canvas meters redraw colours only; avoid recalculating layout.
## Accessibility
Each theme must meet contrast requirements; provide at least one high‑contrast theme.
## Testing
Validate theme structure, UI updates, meter redraws, and contrast compliance.
## Summary
Theming must stay simple, isolated to UI, and must never affect logic, performance, or subsystem boundaries.
