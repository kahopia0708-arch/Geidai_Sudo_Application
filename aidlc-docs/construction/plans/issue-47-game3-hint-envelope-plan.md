# Issue #47 Game3 Wizard Hint Envelope — Code Generation Plan

## Scope

- Modify only Game3 playback/controller code and its Game3-specific EditMode test.
- Preserve native 48,000 Hz catalog playback and agreed cents values.
- Do not modify Common, Services, Game1, Game2, or `Game3Config`.

## Execution Steps

- [x] Expose Game3-local playback duration and volume controls without altering the source clip.
- [x] Replace the wizard hint's fixed hard stop with a capped duration, short release envelope, and configurable silent gap.
- [x] Prevent pitch-item previews from interrupting the wizard hint.
- [x] Extend the Game3 regression test for pitch-adjusted duration calculation.
- [x] Build Game3 and test assemblies, inspect scope, and record results.

## Validation

- Markdown syntax checked; no diagrams or embedded structured data are present.
- Application code remains outside `aidlc-docs/`.
