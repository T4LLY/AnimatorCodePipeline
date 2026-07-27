# Architecture

## Goal

Use Animator As Code C# as the Git-managed source of truth for moderately complex VRChat Animator logic while preserving a non-destructive NDMF workflow.

Generated Animator Controllers are build-time outputs. Project-owned C# modules remain the authoritative source.

## Responsibilities

### Project module

A project module owns behavior such as clothes, gestures, props, face logic, or tracking logic.

It is normal C# and is expected to live in Git.

A module may:

* inspect the avatar hierarchy;
* access required avatar objects through `AnimatorCodeBuildContext`;
* create or share generated Animator layers using `context.Layer(...)`;
* create AAC states, transitions, clips, blend trees, and parameters;
* coordinate multiple avatar objects or features from one module;
* register expression parameters or menu items using `context.ModularAvatar`;
* use `IsApplicable(...)` when the entire module is genuinely optional for the current avatar or Settings configuration.

A module must not:

* edit the avatar's existing FX, Gesture, Action, or Additive Controller assets;
* create its own NDMF plugin;
* persist generated `.controller` or `.anim` files as the authoritative source;
* assume it is invoked once per feature component;
* use `IsApplicable(...)` to silently hide missing required objects that should instead fail through `RequireTransform`, `RequireGameObject`, or `RequireComponent<T>`.

### Central NDMF plugin

`AnimatorCodePipelinePlugin` is the only NDMF plugin provided by Animator Code Pipeline.

It runs once for the avatar build in `BuildPhase.Resolving`, before Modular Avatar.

For each enabled `AnimatorCodePipelineSettings` component, the plugin:

1. loads the modules explicitly selected by its `AnimatorCodeModuleSet`;
2. instantiates and validates those modules;
3. evaluates `IsApplicable(...)`;
4. skips controller generation when no modules are applicable;
5. clones the configured source Animator Controller into a temporary working controller;
6. persists that working controller as a temporary NDMF build asset;
7. executes the applicable modules against a shared `AnimatorCodeBuildContext`;
8. assigns the working controller to the Settings component's existing `ModularAvatarMergeAnimator` when generated Animator layers are present.

Each Settings component is processed independently.

### Generated layers

`AnimatorCodeBuildContext.Layer(suffix)` creates or retrieves a generated Animator layer inside the current Settings component's temporary working controller.

Multiple modules may intentionally share a generated layer by requesting the same suffix.

Layer suffixes are normalized before being passed to Animator As Code. Distinct suffixes that normalize to the same generated layer name are treated as an error rather than being merged implicitly.

`context.Layer(...)` does not select a VRChat playable layer.

The target playable layer is determined by the `ModularAvatarMergeAnimator` configured for that Settings component.

### Settings

`AnimatorCodePipelineSettings` defines one independent generation configuration.

An avatar may contain zero, one, or multiple enabled Settings components.

Each Settings component associates:

* a Module Set;
* an explicit project-owned regular `Source Controller` asset;
* exactly one same-host `ModularAvatarMergeAnimator` referencing that same controller;
* one temporary working controller created during the build.

The Source Controller is configured directly on `AnimatorCodePipelineSettings`. A local
`Animator` component is not required, and the avatar root Animator is not used as a
fallback. `AnimatorOverrideController` assets are rejected.

A Settings component is a configuration boundary, not a feature component and not a processor invocation unit.

Multiple project modules may execute within the same Settings component and cooperate through the same working controller and generated layers.

### Modular Avatar

Animator Code Pipeline does not directly merge generated Animator logic into the avatar's playable-layer Controllers.

After module generation, the temporary working controller is assigned to the existing `ModularAvatarMergeAnimator` on the Settings GameObject in the avatar build clone.

Modular Avatar then virtualizes and merges that controller through its normal NDMF pipeline.

The authored scene component and source Animator Controller asset are not modified.

## Execution model

The build flow is:

```
Project C# modules
    ↓
AnimatorCodeModuleSet
    ↓
AnimatorCodePipelineSettings
    ↓
AnimatorCodePipelinePlugin
    ↓
applicable modules
    ↓
temporary working Animator Controller
    ↓
Animator As Code generated layers
    ↓
ModularAvatarMergeAnimator
    ↓
Modular Avatar / NDMF
    ↓
final avatar
```

The C# modules are the source of truth.

The temporary Animator Controller is an intermediate build product.
