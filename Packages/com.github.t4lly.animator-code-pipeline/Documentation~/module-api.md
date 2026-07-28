# Module API Reference

This document describes the public module contract of Animator Code Pipeline.

It is intended for developers implementing project-owned `AnimatorCodeModule` classes, and for coding agents that need an exact description of how modules are selected, validated, ordered, and executed.

For a simpler introduction, see [guide.md](guide.md).

---

## 1. Overview

Animator Code Pipeline separates project Animator logic from the NDMF integration that executes it.

Project features are implemented as normal C# classes derived from `AnimatorCodeModule`.

Those modules are explicitly selected through an `AnimatorCodeModuleSet` and executed by the central `AnimatorCodePipelinePlugin`.

During execution, each applicable module receives an `AnimatorCodeBuildContext`.

```text
AnimatorCodeModule
        ↓
selected by
AnimatorCodeModuleSet
        ↓
used by
AnimatorCodePipelineSettings
        ↓
executed by
AnimatorCodePipelinePlugin
        ↓
receives
AnimatorCodeBuildContext
```

A module does not need to create its own NDMF plugin or manage the NDMF lifecycle directly.

---

## 2. `AnimatorCodeModule`

`AnimatorCodeModule` is the base class for project-owned Animator generation code.

A module may represent one feature, several related features, or any other project-defined unit of Animator logic.

A module does not need to correspond to one GameObject, one component, or one Animator layer.

Typical responsibilities include:

- inspecting the avatar hierarchy;
- locating required GameObjects and components;
- creating Animator layers, states, transitions, clips, and blend trees;
- creating or using Animator parameters;
- coordinating several avatar objects or features;
- generating Modular Avatar parameters or menu content.

### `Id`

Each module must provide a stable, non-empty identifier.

```csharp
public override string Id => "project.example-toggle";
```

`Id` is used for:

- duplicate detection;
- deterministic ordering;
- diagnostics.

The value should remain stable unless the logical identity of the module changes.

`Id` is not automatically used as an Animator layer name.

For example:

```csharp
public override string Id => "project.clothes";

public override void Build(AnimatorCodeBuildContext context)
{
    var layer = context.Layer("Clothes");
}
```

The module ID and generated layer suffix are separate concepts.

### `Order`

`Order` controls the primary execution order of modules.

Lower values execute first.

```csharp
public override int Order => 100;
```

Modules are ordered deterministically by:

1. `Order`;
2. `Id`;
3. fully qualified type name.

The order in which definitions appear inside `AnimatorCodeModuleSet` is not the execution order.

Use `Order` only when generation sequence matters. `Order` is a deterministic execution-order hint, not a module dependency graph.

### `IsApplicable(...)`

`IsApplicable(...)` determines whether the entire module should run for the current avatar and Settings configuration.

```csharp
public override bool IsApplicable(
    GameObject avatarRoot,
    AnimatorCodePipelineSettings settings)
{
    return true;
}
```

This method is intended for genuinely optional modules.

It must be side-effect free.

Do not create or modify build objects from `IsApplicable(...)`.

In particular, do not use it to:

- create Animator Controllers;
- create AnimationClips;
- create GameObjects;
- add Modular Avatar components;
- modify persistent assets.

Required avatar objects should normally be resolved in `Build(...)` through the `Require*` helpers.

Do not silently skip a module merely because a required path is missing.

Prefer:

```csharp
public override void Build(AnimatorCodeBuildContext context)
{
    var target = context.RequireGameObject("Clothes/Hat");
}
```

over using `Transform.Find(...)` inside `IsApplicable(...)` to hide a missing required dependency.

### `Build(...)`

`Build(...)` contains the actual generation logic.

```csharp
public override void Build(AnimatorCodeBuildContext context)
{
    // Generate Animator or Modular Avatar content here.
}
```

Only applicable modules are executed.

Modules processed within the same Settings configuration use the same temporary working Animator Controller.

They may therefore intentionally coordinate through shared Animator parameters or shared generated layers.

---

## 3. Module construction and validation

Modules are selected explicitly through `AnimatorCodeModuleSet`.

Each configured module definition must satisfy the module contract.

The build fails when an enabled definition is invalid.

A valid module definition must:

- derive from `AnimatorCodeModule`;
- be a concrete type;
- contain no unbound generic parameters;
- be constructible through a public parameterless constructor;
- return a non-empty, non-whitespace `Id`;
- have an `Id` that is unique within the processed module set.

Invalid enabled module definitions are treated as configuration errors.

They are not silently skipped.

This fail-fast behavior is intentional: a Module Set should describe the code that will actually participate in the build.

---

## 4. `AnimatorCodeModuleSet`

`AnimatorCodeModuleSet` is an Editor-only ScriptableObject used to select serializable project module definitions.

It is an execution configuration, not an automatic discovery mechanism.

A Module Set contains `[SerializeReference]` `AnimatorCodeModule` definitions, including user-adjustable configuration.

```text
AnimatorCodeModuleSet
├── ClothesModuleDefinition
├── PropsModuleDefinition
└── FaceModuleDefinition
```

The same Module Set may be referenced by multiple Settings components.

Each Settings configuration receives independent module instances copied from the serialized definitions, and its working controller is also independent.

The Module Set list order does not define execution order.

Execution order is determined by the module contract:

```text
Order
  ↓
Id
  ↓
fully qualified type name
```

---

## 5. `AnimatorCodeBuildContext`

`AnimatorCodeBuildContext` provides the build environment available to a module.

The important members are:

- `Settings`
- `AvatarRoot`
- `AvatarRootTransform`
- `BindingRoot`
- `Path(Transform target)`
- `Aac`
- `ModularAvatar`
- `Layer(string suffix)`
- `RequireTransform(string path)`
- `RequireTransform(AnimatorCodeObjectReference reference)`
- `RequireGameObject(string path)`
- `RequireComponent<T>(string path)`

A module should normally interact with the build through this context rather than recreating pipeline behavior itself.

---

## 6. `Layer(string suffix)`

`Layer(...)` creates or retrieves a generated Animator layer inside the current Settings working controller.

```csharp
var layer = context.Layer("Clothes");
```

Layer creation is lazy.

Requesting the same suffix again within the same Settings configuration returns the same generated layer.

This makes intentional cooperation between modules possible.

For example:

```csharp
// Module A
var layer = context.Layer("Clothes");

// Module B
var layer = context.Layer("Clothes");
```

Both modules work with the same generated layer.

### Layer suffix identity

ACP passes the suffix to Animator As Code unchanged.

The exact suffix string is the sharing key inside one Settings configuration.

For example, these are distinct layer suffixes:

```text
Face.Blink
Face_Blink
```

Request the exact same suffix when modules are intended to share a generated layer.

### `Layer(...)` does not select a VRChat playable layer

`context.Layer(...)` creates a layer in the current temporary working Animator Controller.

It does not select FX, Gesture, Action, Additive, or Base.

The target VRChat playable layer is selected by the `ModularAvatarMergeAnimator` associated with the current Settings configuration.

---

## 7. Required avatar objects

Animator Code Pipeline provides helper methods for required avatar-relative objects.

Use these helpers when the module cannot function correctly without the requested target.

### `RequireTransform(string path)`

Returns a required `Transform`.

```csharp
var target = context.RequireTransform("Clothes/Hat");
```

### `RequireGameObject(string path)`

Returns a required `GameObject`.

```csharp
var target = context.RequireGameObject("Clothes/Hat");
```

### `RequireComponent<T>(string path)`

Returns a required component from the object at the given avatar-relative path.

```csharp
var renderer =
    context.RequireComponent<SkinnedMeshRenderer>("Body");
```

If the required object or component cannot be resolved, the build fails with an error that includes the relevant avatar-relative path.

These helpers are preferred over manual `Transform.Find(...)` calls for required dependencies because they make module assumptions explicit and produce more useful diagnostics.

### `AnimatorCodeObjectReference`

`AnimatorCodeObjectReference` is intended for user-adjustable target fields in Module definitions.

A Module Set is a project asset, so ACP persists only the avatar-relative path. The Inspector may use a live Scene object as a picker convenience, but the Scene object itself is not serialized into the Module Set asset.

```csharp
public AnimatorCodeObjectReference target = new AnimatorCodeObjectReference();

public override void Build(AnimatorCodeBuildContext context)
{
    var targetTransform = context.RequireTransform(target);
}
```

---

## 8. Avatar-relative paths

Paths passed to `RequireTransform`, `RequireGameObject`, and `RequireComponent<T>` are relative to the avatar root.

Example:

```csharp
var target = context.RequireGameObject("Clothes/Hoodie");
```

Conceptually:

```text
Avatar
└── Clothes
    └── Hoodie
```

The object lookup path and the final AnimationClip binding path are related but separate concerns.

`BindingRoot` is the root ACP passes to `AacConfiguration.AnimatorRoot`. It follows the same-host Merge Animator configuration:

```text
Path Mode = Absolute
→ Avatar root

Path Mode = Relative
→ Relative Path Root when it resolves
→ otherwise the Merge Animator host GameObject
```

Use `context.Path(target)` when code needs the binding path explicitly. It returns a path relative to `BindingRoot` and fails when the target is outside that root.

Most high-level AAC clip helpers can work from the target object directly because AAC already receives the same `BindingRoot` as its Animator root.

---

## 9. `Aac`

`context.Aac` exposes the configured Animator As Code instance for the current Settings build.

Use it to access Animator As Code generation APIs such as:

- AnimationClip generation;
- BlendTree generation;
- Animator parameter helpers;
- state and transition construction through generated layers;
- other AAC-supported Animator operations.

Example:

```csharp
var clip = context.Aac.NewClip("Hat On")
    .Toggling(target, true);
```

Animator Code Pipeline does not redefine the Animator As Code API.

When using AAC functionality beyond the examples provided by this package, consult the API available in the installed Animator As Code version or the official Animator As Code documentation.

Do not assume that an API exists only because a similar method appears in another AAC version.

---

## 10. `ModularAvatar`

`context.ModularAvatar` exposes the Modular Avatar / AAC integration associated with the current build context.

It may be used for operations such as:

- expression parameter registration;
- menu generation;
- other supported MA-as-Code operations.

Example:

```csharp
context.ModularAvatar
    .NewParameter(parameter)
    .WithDefaultValue(false);
```

A module should prefer non-destructive Modular Avatar integration instead of directly modifying authored VRChat assets such as:

- `VRCAvatarDescriptor`;
- `VRCExpressionsMenu`;
- Expression Parameters assets.

The exact available MA-as-Code methods depend on the installed integration package version.

---

## 11. Settings and build boundaries

`AnimatorCodePipelineSettings` defines one independent generation configuration.

An avatar may contain zero, one, or multiple enabled Settings components.

Conceptually:

```text
Avatar
├── Settings A
│   ├── ModuleSet A
│   └── Working Controller A
│
└── Settings B
    ├── ModuleSet B
    └── Working Controller B
```

Within one Settings configuration:

- enabled module definitions are copied into independent build-time module instances;
- applicable modules share one temporary working Animator Controller;
- generated layers may be shared by suffix;
- the associated `ModularAvatarMergeAnimator` determines the target playable layer.

Across different Settings configurations:

- working controllers are separate;
- generated layer caches are separate;
- module instances are separate.

A Settings component is therefore a generation boundary, not a feature component and not a processor invocation unit.

---

## 12. Build lifecycle

The central `AnimatorCodePipelinePlugin` owns the NDMF lifecycle.

Modules do not need to implement NDMF integration themselves.

For each enabled Settings configuration, the build proceeds conceptually as follows:

```text
Settings discovered
        ↓
Enabled Module definitions selected
        ↓
Definitions validated
        ↓
Per-Settings Module instances created
        ↓
Deterministic ordering
        ↓
IsApplicable(...)
        ↓
No applicable modules?
        ├── yes → stop processing this Settings
        └── no
              ↓
Merge Animator Controller cloned
              ↓
Working Controller registered as a temporary NDMF build asset
              ↓
AnimatorCodeBuildContext created
              ↓
Build(...) executed for applicable modules
              ↓
Generated Animator layers exist?
        ├── no  → no controller merge is required
        └── yes
              ↓
Working Controller assigned to ModularAvatarMergeAnimator
              ↓
Modular Avatar continues its normal NDMF processing
```

The working controller is a build-time object.

It is not intended to become a project-owned generated `.controller` asset.

---

## 13. Multiple modules and composition

Modules are project-defined units of code.

They are not constrained to one component or one avatar object.

For example:

```text
ClothesModule
FaceModule
PropsModule
        ↓
same Settings
        ↓
same working controller
```

One module may inspect:

- several GameObjects;
- several component types;
- several feature groups.

One module may also generate:

- several states;
- several parameters;
- several generated layers.

Conversely, several modules may intentionally contribute to the same generated layer.

Use module boundaries that match the project architecture rather than forcing a one-component-per-module structure.

---

## 14. Non-destructive contract

Animator Code Pipeline is designed around build-time generation.

### Project modules should not

- edit the Animator Controller asset referenced by the same-host Merge Animator;
- edit existing project AnimationClip assets as generated output;
- save generated `.controller` or `.anim` files as the authoritative source;
- create a separate NDMF plugin for each feature;
- directly modify authored VRChat descriptor, menu, or parameter assets when a non-destructive Modular Avatar approach is available.

### The pipeline

- clones the Animator Controller referenced by the same-host Merge Animator;
- performs generation against the temporary working controller;
- manages generated assets within the NDMF build;
- passes generated Animator content to Modular Avatar for final integration.

The project-owned C# modules and configuration remain the reproducible source for generated Animator behavior.

---

## 15. Minimal example

```csharp
using AnimatorAsCode.V1;

namespace ProjectAnimatorCode
{
    public sealed class ExampleToggleModule
        : AnimatorCodePipeline.AnimatorCodeModule
    {
        public override string Id => "project.example-toggle";

        public override void Build(
            AnimatorCodePipeline.AnimatorCodeBuildContext context)
        {
            var target =
                context.RequireGameObject("Clothes/Hat");

            var layer =
                context.Layer("ExampleToggle");

            var enabled =
                layer.BoolParameter("ExampleToggle");

            var off = layer.NewState("Off")
                .WithAnimation(
                    context.Aac.NewClip("Off")
                        .Toggling(target, false));

            var on = layer.NewState("On")
                .WithAnimation(
                    context.Aac.NewClip("On")
                        .Toggling(target, true));

            off.TransitionsTo(on)
                .When(enabled.IsTrue());

            on.TransitionsTo(off)
                .When(enabled.IsFalse());

            // Optional:
            // context.ModularAvatar
            //     .NewParameter(enabled)
            //     .WithDefaultValue(false);
        }
    }
}
```

This example demonstrates the normal module pattern:

```text
resolve required avatar objects
        ↓
request a generated layer
        ↓
define parameters and Animator behavior
        ↓
optionally register Modular Avatar data
```

For larger examples, see the package samples.

---

## 16. Related documentation

- [Architecture](architecture.md)
- [Compatibility](compatibility.md)
- [Git workflow](git-workflow.md)
- [Animator As Code and NDMF integration](animator-as-code-ndmf-integration.md)

For practical code patterns, also see the examples included under `Samples~/ModuleExamples`.
