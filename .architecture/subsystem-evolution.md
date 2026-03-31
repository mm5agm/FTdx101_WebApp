# Subsystem Evolution

This document defines how subsystems may evolve over time without causing drift or breaking architecture.

## Principles
- Evolution must preserve boundaries.
- Changes must be incremental, documented, and validated.
- Subsystems may grow, split, or be replaced, but never blur responsibilities.

## When Evolution Is Allowed
- New radio features require new logic.
- Performance improvements demand restructuring.
- UI enhancements require new components.
- Calibration refinements require new tables or functions.

## When Evolution Is NOT Allowed
- Mixing responsibilities across subsystems.
- Adding logic to orchestrator.
- Adding DOM access outside UI subsystem.
- Adding gauge logic outside gaugeFactory/update-engine.
- Adding formatting outside meter-formatters.

## Evolution Workflow
1. Identify need for change.
2. Review subsystem-boundaries.md.
3. Draft proposed evolution.
4. Validate with refactor-session.md.
5. Update documentation.
6. Apply changes in small steps.
7. Run drift-check.
8. Update tests.
9. Merge only after full validation.

## Adding a New Subsystem
- Must have a clear, single responsibility.
- Must not duplicate existing logic.
- Must integrate cleanly into the value pipeline.
- Must include documentation and tests.

## Splitting a Subsystem
- Allowed when responsibilities diverge.
- Each new subsystem must have a clear boundary.
- Update imports, pipeline, and documentation.

## Replacing a Subsystem
- Allowed only when benefits outweigh risk.
- Replacement must be drop‑in compatible or migration must be documented.
- Old subsystem removed cleanly.

## Deprecation
- Mark subsystem or module as deprecated.
- Provide migration path.
- Remove only after stable replacement exists.

## Documentation Requirements
- Update architecture-overview.md.
- Update subsystem-boundaries.md.
- Update diagrams-overview.md.
- Update INDEX.md.

## Testing Requirements
- Ensure new subsystem has full unit tests.
- Validate pipeline integrity.
- Confirm no cross‑layer leakage.

## Summary
Subsystems may evolve, but boundaries must remain strict, documentation must stay current, and the value pipeline must remain intact.
