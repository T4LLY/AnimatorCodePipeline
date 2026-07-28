# Changelog

## [Unreleased]

- Store module definitions and user-adjustable module configuration directly on `AnimatorCodePipelineSettings`; the required `AnimatorCodeModuleSet` asset layer has been removed.
- Let the ACP Settings Inspector add, remove, configure, and generate modules in place.
- Preserve exact `Layer(suffix)` values; ACP no longer rewrites `.` to `_`.
- Create independent build-time module instances from serialized Settings definitions for each Settings component.
- Keep `AnimatorCodeObjectReference` targets as avatar-relative paths so build-time module copies resolve against the active avatar.
- Bound generated-module registration retries and report compile/assembly-definition problems instead of polling indefinitely.
- Synchronize public documentation with the MAMA source-of-truth, feature-host, path-root, and direct Settings module model.

## 0.2.0 - 2026-07-28

### Changed

- ACP Settings now stores only a Module Set; Modular Avatar Merge Animator is the source of truth for controller and merge settings.
- Module Sets now store enabled, serializable module definitions and user configuration directly.
- Generated bindings use the Merge Animator path mode and relative path root.

## Historical internal pre-release notes

The 0.1.x entries below describe superseded private-beta designs and are retained only as development history. They are not part of the current API contract and require no migration support.

## 0.1.2 - 2026-07-27

- Replaced the required Target Animator configuration with an explicit Source Controller asset.
- Require a regular Source Controller and exactly one same-host Modular Avatar Merge Animator referencing the same controller.
- Added editor validation and explicit migration support for legacy Target Animator configurations.
- Prevented silent fallback to the avatar root Animator and rejected AnimatorOverrideController source assets.
- Updated architecture, setup, AI workflow, and module sample documentation for the Source Controller workflow.
- Added EditMode coverage for source-controller cloning and Merge Animator reference validation.

## 0.1.1 - 2026-07-27

- Fixed layer suffix collisions after AAC normalizes `.` to `_`.
- Invalid selected modules now fail explicitly instead of being skipped.
- Evaluated `IsApplicable` before creating AAC/controller state and persisted working controllers before AAC generation.
- Merge Animator replacement now occurs only when a module generated a layer.
- Renamed the internal shared layer cache to `GeneratedLayerCache`.
- Added EditMode coverage for module validation, layer caching, and applicability filtering.

## 0.1.0 - 2026-07-26

- Initial package.
- One central NDMF Generating pass.
- Explicit `AnimatorCodeModuleSet` selection with deterministic ordering.
- One temporary working controller per enabled Settings generation boundary.
- NDMF `IAssetSaver` bridge compatible with AAC 1.2.0+ integration guidance.
- Modular Avatar As Code merge finalization.
- Settings components define independent generation boundaries.
- Git-managed module folder setup via `.asmref`.
- OpenCode/agent Skill bundle included in distribution ZIP.
