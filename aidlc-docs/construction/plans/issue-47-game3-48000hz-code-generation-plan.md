# Issue #47 Game3 48,000 Hz Fix — Code Generation Plan

## Scope

- Modify only `Assets/Scripts/Game3/` and Game3-specific EditMode tests.
- Do not modify `Geidai.Common`, `Geidai.Services`, Game1, Game2, or agreed `Game3Config` cents values.
- Preserve catalog `AudioClip` native 48,000 Hz playback in Game3.

## Execution Steps

- [x] Add a Game3-local pitch player that plays the original catalog `AudioClip` without converting it to the 44,100 Hz `AudioBuffer` format.
- [x] Route Game3 catalog playback, previews, hints, and stop actions through the Game3-local player while retaining the existing service only for user-recording fallback.
- [x] Add a Game3-specific regression test proving a 48,000 Hz clip remains 48,000 Hz when selected for playback.
- [x] Run available compile/tests and inspect the final diff to confirm no Common, Services, Game1, Game2, or cents configuration changes.
- [x] Record implementation and verification results in the audit log.

## Validation

- Markdown structure checked; no diagrams or embedded structured data are present.
- Application code remains at the workspace root; this plan remains under `aidlc-docs/`.
