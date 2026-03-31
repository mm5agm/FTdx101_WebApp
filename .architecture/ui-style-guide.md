# UI Style Guide

This guide defines consistent UI rules for the FTdx101 WebApp to ensure clarity, performance, and architectural alignment.

## Principles
- Keep UI minimal and functional.
- Prioritise readability over decoration.
- Avoid unnecessary DOM complexity.
- Maintain consistent layout and spacing.
- Ensure accessibility and predictable behaviour.

## Layout Rules
- All meter UI lives in MeterPanel.
- Use flex or grid only for high-level layout.
- Avoid nested containers unless required.
- Keep DOM tree shallow for performance.

## Canvas Rules
- One canvas per meter.
- Static elements pre-rendered when possible.
- Dynamic elements updated via update-engine.
- Avoid full-canvas clears unless necessary.

## Typography
- Use a single font family across the app.
- Keep text sizes consistent for labels.
- Avoid inline styles; use classes or constants.

## Colours
- Use a small, consistent palette.
- Meter colours must match radio conventions.
- Avoid gradients or heavy effects.
- Ensure high contrast for readability.

## Spacing
- Use consistent padding/margins.
- Avoid pixel-perfect micro-adjustments.
- Prefer even spacing increments (4/8/12px).

## Interactions
- UI must remain responsive at 60fps.
- Avoid expensive event handlers.
- Debounce or throttle where appropriate.
- Keep hover/focus states subtle.

## DOM Rules
- DOM access only in UI subsystem.
- No DOM reads inside animation loops.
- Batch DOM writes when possible.
- Avoid layout thrashing.

## Components
- Keep components small and focused.
- No business logic in UI components.
- No calibration or formatting logic here.
- UI components should be predictable and stateless where possible.

## Accessibility
- Provide clear labels for meters.
- Ensure keyboard accessibility where relevant.
- Maintain sufficient colour contrast.

## Performance
- Minimise reflows and repaints.
- Avoid unnecessary DOM nodes.
- Use canvas for all meter rendering.
- Keep UI updates lightweight.

## Summary
UI must be simple, consistent, readable, and fast. DOM stays in UI subsystem, canvas handles rendering, and all logic remains in the appropriate subsystems.
