# UI Accessibility

This document defines accessibility rules for the FTdx101 WebApp to ensure clarity, usability, and inclusiveness.

## Principles
- UI must be readable, navigable, and understandable.
- Accessibility must not compromise performance.
- Canvas‑based meters must still provide accessible context.

## Text & Labels
- All meters require clear text labels.
- Use consistent terminology across UI.
- Avoid tiny text; maintain readable sizes.
- Ensure labels have sufficient contrast.

## Colour & Contrast
- Use high‑contrast colours for critical values.
- Avoid colour‑only communication; pair with labels.
- Ensure meter colours remain visible in low‑light conditions.

## Keyboard Navigation
- All interactive UI elements must be reachable via keyboard.
- Focus states must be visible.
- Avoid trapping focus inside components.

## Screen Readers
- Provide ARIA labels for meter containers.
- Expose meter values as text where possible.
- Avoid hiding essential information behind canvas‑only rendering.

## Motion & Animation
- Keep animations subtle and functional.
- Avoid rapid flashing or strobing.
- Allow users to disable non‑essential animations.

## Layout
- Maintain predictable structure.
- Avoid overlapping elements.
- Ensure UI scales cleanly on different resolutions.

## Error States
- Provide clear, readable error messages.
- Avoid ambiguous colours (e.g., red vs. orange).
- Ensure errors are announced to assistive tech.

## Performance & Accessibility
- Keep DOM minimal to avoid lag.
- Avoid heavy reflows that disrupt assistive tools.
- Maintain stable element positions during updates.

## Testing
- Validate contrast ratios.
- Test keyboard navigation paths.
- Confirm ARIA labels exist and are meaningful.
- Ensure meters expose readable values.

## Summary
Accessibility must remain simple, consistent, and predictable. Labels must be clear, colours must be readable, keyboard navigation must work, and canvas‑based meters must still provide accessible context.
