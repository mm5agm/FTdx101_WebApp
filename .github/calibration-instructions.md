# Calibration Subsystem Instructions

## Purpose
This section defines the architectural boundaries, naming conventions, and workflow rules for all calibration-related code in this project. It ensures that calibration logic remains well-structured, empirical, and consistent with the project’s disciplined refactoring workflow.

## Subsystem Boundaries
- **Calibration logic** must reside in dedicated controllers (`CalibrationController`), services (`ICalibrationService`, `CalibrationService`), and models (`Models/Calibration/`).
- **UI/state flow** for calibration is handled in Razor pages (`Pages/Calibrations.cshtml`, `Pages/Calibration/MeterCalibration.cshtml`) and must not contain decoding or serial logic.
- **Empirical rules** for calibration (e.g., meter scaling, observed quirks) must be documented in `scripts/empirical-rules.md` and reflected in decoding logic only.
- **No calibration logic** should leak into unrelated subsystems (Serial, Queue, Messaging).

## Naming Conventions
- Use **PascalCase** for all calibration models, services, and controllers (e.g., `MeterCalibration`, `CalibrationFile`).
- Use **camelCase** for flow-level variables, method parameters, and temporary state within calibration logic.
- All naming must reflect true architectural boundaries and intent.

## Refactoring and Drift
- When refactoring calibration code, always:
  - Group changes by subsystem (Models, Controllers, UI, etc.).
  - Update comments and documentation to match new behaviour.
  - Check for duplicated calibration logic and consolidate as needed.
  - Ensure empirical rules are preserved and documented.
- Use the following scripts for guidance:
  - `scripts/architecture-overview.md`
  - `scripts/subsystem-boundaries.md`
  - `scripts/helper-map.md`
  - `scripts/empirical-rules.md`

## Example Prompts
- “Refactor meter calibration logic to remove duplication.”
- “Audit calibration naming for architectural drift.”
- “Document new empirical calibration rule.”
- “Move calibration UI logic out of decoding layer.”

## See Also
- `.github/copilot-instructions.md` (project-wide rules)
- `scripts/INDEX.md` (documentation map)
- `scripts/refactor-session.md` (refactor workflow)
- `scripts/drift-check.md` (drift detection)
