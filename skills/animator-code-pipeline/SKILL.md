---
name: animator-code-pipeline
description: Build VRChat Animator features non-destructively as project-owned C# modules using Animator Code Pipeline, Animator As Code, NDMF, Modular Avatar, and Unity MCP.
---

# Animator Code Pipeline

Use this skill to create, modify, review, and debug Animator Code Pipeline (ACP) modules.

**Project-owned C# modules and Module Set configuration are the source of truth.** Generated Animator Controllers, Animation Clips, Layers, States, Transitions, and Blend Trees are build outputs.

## 1. Hard Rules

- Inspect the live Unity project before editing. Never guess hierarchy paths, component fields, enum values, serialized values, package versions, or APIs.
- Edit the source avatar. Do not create a persistent working avatar clone.
- Do not use Modular Avatar Manual Bake for ACP implementation or validation.
- Temporary NDMF / MA build clones are expected, but never edit them as project targets.
- Do not move, reparent, rename, or delete existing avatar GameObjects merely to fit ACP.
- Do not directly modify authored Animator Controller assets for ACP-generated behavior.
- Do not directly rewrite authored `VRCExpressionsMenu`, Expression Parameters, or `VRCAvatarDescriptor` assets when Modular Avatar can represent the feature non-destructively.
- Do not create another NDMF plugin for a normal ACP feature. ACP owns the NDMF lifecycle.
- Use `context.Layer(...)` for ACP-generated Animator layers.
- Use `RequireGameObject`, `RequireTransform`, `RequireComponent<T>`, or `AnimatorCodeObjectReference` for required targets.
- Do not hide missing required targets with `IsApplicable` or silent returns.
- A successful write or MCP mutation is not completion. Compile and validate the NDMF result.

### Never infer component semantics from serialization

Never infer Unity, Modular Avatar, VRChat SDK, or ACP behavior from raw serialized values.

Do not guess:

- enum integers;
- flags / bit values;
- serialized field meaning;
- undocumented default numeric values;
- relationships between component fields.

For component configuration:

1. inspect the installed type / API / source or the named Inspector state;
2. use named enum/API values;
3. make the smallest required change;
4. read the live component back;
5. verify the resulting named state.

Installed package source and live Unity state are authoritative for version-specific behavior.

## 2. Inspect Unity First

Use the installed Unity MCP implementation when available. Tool names vary; use capabilities rather than assuming exact names.

Inspect only what the feature requires:

- active Unity project / instance;
- avatar root and `VRCAvatarDescriptor`;
- current Unity selection;
- exact avatar-relative paths for required targets;
- relevant Components, Renderers, Materials, Blend Shapes, and Parameters;
- existing `AnimatorCodePipelineSettings`;
- assigned `AnimatorCodeModuleSet` and existing module definitions;
- same-host `ModularAvatarMergeAnimator`;
- existing MA Parameters / Menu components;
- Unity Console compile errors.

If multiple candidates exist, do not guess. Prefer the live Unity selection when it resolves the ambiguity.

## 3. Feature Host Placement

ACP Settings and its MA Merge Animator belong on the same feature host.

For one target, a sibling host under the target's parent is usually clear:

```text
Parent
├── Target
└── ACP Feature Host
```

For several targets that form one feature and share a parent:

```text
Parent
├── Target A
├── Target B
├── Target C
└── ACP Feature Host
```

Reuse a suitable existing host when possible. Do not create one ACP host per target when one feature host is enough.

Host placement does not itself define the animation binding root. Configure MAMA Path Mode and Relative Path Root explicitly for the intended target scope.

If targets are in unrelated branches and no clear feature boundary exists, ask before choosing the host. Never reparent targets just to create a convenient ACP hierarchy.

## 4. Required Host Configuration

Current ACP uses this normal host shape:

```text
ACP Feature Host
├── AnimatorCodePipelineSettings
├── ModularAvatarMergeAnimator
├── ModularAvatarParameters      (when expression parameter registration is needed)
└── MA Menu components           (when menu controls are needed)
```

`AnimatorCodePipelineSettings` requires a same-host `ModularAvatarMergeAnimator` and disallows multiple ACP Settings components on one GameObject.

**ACP does not require a local Unity `Animator`.** There is no `targetAnimator` field and no ACP-specific blank-controller/local-Animator relationship.

### Merge Animator is the source of truth

Do not duplicate these settings in ACP:

- Animator Controller;
- VRChat playable layer;
- Path Mode;
- Relative Path Root;
- Layer Priority;
- other Merge Animator behavior that exists in the installed Modular Avatar version.

Do not assume optional/newer MA fields exist merely because they exist in recent MA releases. Inspect the installed package before using them.

The same-host `ModularAvatarMergeAnimator` owns them.

The controller must be a regular `AnimatorController` for the current ACP implementation. Do not use `AnimatorOverrideController` as ACP input.

ACP clones the Merge Animator controller during the NDMF build, generates into the temporary clone, and assigns that clone only to the build-clone Merge Animator when generated Animator layers exist. The authored controller is not modified.

## 5. ACP Build Contract

ACP owns one central NDMF plugin and runs in `BuildPhase.Resolving` before Modular Avatar.

An avatar may contain multiple `AnimatorCodePipelineSettings`. Each Settings is an independent generation boundary.

Each enabled Settings associates:

- one `AnimatorCodeModuleSet`;
- one same-host `ModularAvatarMergeAnimator`;
- independent build-time module instances copied from the Module Set definitions;
- one temporary cloned working controller when applicable modules run;
- one generated-layer cache for that Settings.

Conceptual lifecycle:

```text
Validate Module Set definitions
  ↓
Create independent build-time Module instances
  ↓
Sort by Order → Id → Fully Qualified Type Name
  ↓
IsApplicable(...)
  ↓
No applicable modules? → stop this Settings
  ↓
Read controller and path configuration from MAMA
  ↓
Clone MAMA Animator Controller
  ↓
Register temporary controller with NDMF
  ↓
Create AAC / AnimatorCodeBuildContext
  ↓
Build Modules
  ↓
Generated Animator layers?
  ├─ no  → no controller replacement required
  └─ yes → assign working controller to build-clone MAMA
  ↓
Modular Avatar / NDMF performs final integration
```

Do not recreate this lifecycle inside a module.

## 6. Module Contract

Modules derive from `AnimatorCodeModule` and are serialized as definitions inside `AnimatorCodeModuleSet`.

```csharp
[Serializable]
public sealed class ExampleModule : AnimatorCodeModule
{
    [SerializeField] private string parameterName = "ExampleEnabled";

    public override string Id => "project.example";
    public override int Order => 0;

    public override bool IsApplicable(
        GameObject avatarRoot,
        AnimatorCodePipelineSettings settings)
        => true;

    public override void Build(AnimatorCodeBuildContext context)
    {
        var target = context.RequireGameObject("Confirmed/Avatar/Path");
        var layer = context.Layer("Example");
        var parameter = layer.BoolParameter(parameterName);
        // AAC generation...
    }
}
```

Module fields may expose user-adjustable configuration in the Module Set Inspector. Prefer configuration fields over hard-coding values that users are expected to tune.

### `Id`

- must be non-empty;
- must be unique among enabled definitions in the same Module Set;
- should remain stable across ordinary refactors;
- is not an Animator layer name.

### `Order`

Lower values build first. Ties are resolved by `Id`, then fully qualified type name.

`Order` is deterministic execution ordering, not a dependency graph. Use it only when generation sequence actually matters.

### `IsApplicable`

Use only when the entire module is genuinely optional. Keep it side-effect free.

Do not create controllers, clips, GameObjects, or MA components in `IsApplicable`.

Do not use it to hide missing required targets.

### Definition construction and validation

Enabled definitions must:

- derive from `AnimatorCodeModule`;
- be concrete and non-generic;
- have a public parameterless constructor (an implicit default constructor is fine);
- return a non-empty `Id`;
- have a unique `Id` within the Module Set.

ACP creates independent build-time copies of serialized definitions before module execution.

Do not create a custom NDMF `Plugin<T>` or `[assembly: ExportsPlugin]` from a normal module.

## 7. Build Context

Important API:

```text
context.Settings
context.AvatarRoot
context.AvatarRootTransform
context.BindingRoot
context.Path(target)
context.Layer(...)
context.RequireGameObject(...)
context.RequireTransform(...)
context.RequireTransform(AnimatorCodeObjectReference)
context.RequireComponent<T>(...)
context.Aac
context.ModularAvatar
```

### Required target lookup

String `Require*` paths are relative to the avatar root:

```csharp
var sword = context.RequireGameObject("Armature/Hips/Spine/Chest/Sword");
var body = context.RequireComponent<SkinnedMeshRenderer>("Body");
```

For user-adjustable target fields, prefer `AnimatorCodeObjectReference`:

```csharp
[SerializeField]
private AnimatorCodeObjectReference target = new AnimatorCodeObjectReference();

public override void Build(AnimatorCodeBuildContext context)
{
    var targetTransform = context.RequireTransform(target);
}
```

Module Sets are project assets. `AnimatorCodeObjectReference` persists an avatar-relative path, not a Scene-object reference; the Inspector object picker is only a convenient way to choose that path.

### Animation binding root

`context.BindingRoot` follows the same-host Merge Animator:

```text
MAMA Path Mode = Absolute
→ Avatar root

MAMA Path Mode = Relative
→ configured Relative Path Root when it resolves
→ otherwise the MAMA host GameObject
```

Use `context.Path(target)` when raw binding-path text is needed. The target must be inside `BindingRoot`.

Most AAC helpers can animate the target object directly because ACP passes the same root to `AacConfiguration.AnimatorRoot`.

### Generated layers

```csharp
var layer = context.Layer("FeatureName");
```

The exact suffix string is the cache identity. The same exact suffix inside one Settings returns the same generated layer.

ACP does **not** normalize `.` to `_`.

```text
Face.Blink
Face_Blink
```

are distinct suffixes.

`context.Layer(...)` does not select FX / Gesture / Action. The same-host Merge Animator selects the destination playable layer.

Do not call `context.Aac.NewAnimatorController()` for a normal ACP module; use the working controller provided by ACP through `context.Layer(...)`.

## 8. Animator As Code / MA API Verification

Inspect bundled ACP examples first when relevant:

```text
Packages/com.github.t4lly.animator-code-pipeline/Samples~/ModuleExamples
```

For AAC or MA APIs not demonstrated by ACP:

1. inspect the installed package version/source or reliable version-matched documentation;
2. verify exact method/type/enum names;
3. implement only against APIs that actually exist in the project.

Do not invent APIs from memory and do not infer enum meaning from serialized integers.

## 9. Modular Avatar Merge Animator

Inspect the existing MAMA before changing it.

### Playable Layer

For ordinary expression-driven avatar features, FX is common, but do not force FX when the requested behavior or existing architecture requires Gesture, Action, Base, Additive, or another layer.

Always verify the named `layerType` in the installed API / Inspector.

### Path Mode

Path Mode is a MAMA setting, not an ACP setting.

Choose it based on binding scope:

- **Absolute**: bindings are based at the avatar root; appropriate when targets may be anywhere under the avatar.
- **Relative**: bindings are based at the configured Relative Path Root; when that root is unset, MA uses the Merge Animator host as its path basis.

Relative is useful for self-contained reusable feature hierarchies. Do not choose it merely because the feature host is nearby; all animated targets must fit the selected binding root.

After editing MAMA, read the component back and verify the named Path Mode and Relative Path Root.

### Attached Animator removal

ACP has no local-Animator requirement. Do not add an `Animator` merely for ACP, and do not treat MAMA's attached-Animator removal option as an ACP requirement.

If the host already has an Animator for some independent reason, evaluate that MAMA option according to the installed MA behavior and the user's architecture.

## 10. Parameters and Menu

Creating an Animator parameter does not by itself register it in VRChat Expression Parameters.

When the feature exposes an avatar expression parameter, use Modular Avatar to register the required type/default/sync/save behavior.

Inspect existing parameter and menu components first; do not create duplicate parameters or menu controls unnecessarily.

For Radial Puppet and other control types, verify the installed MA API / Inspector fields by name. Do not infer control type or parameter-slot meaning from serialized integers.

## 11. Validation

After making changes:

1. wait for Unity compilation and fix Console errors;
2. verify the ACP Settings has the intended Module Set;
3. verify the same-host MAMA exists;
4. verify MAMA references a regular Animator Controller;
5. verify playable layer by name;
6. verify Path Mode and Relative Path Root by name;
7. verify Module definitions/configuration and duplicate IDs;
8. verify required target paths or `AnimatorCodeObjectReference` selections;
9. verify MA Parameters / Menu configuration when used;
10. read modified components back instead of trusting writes;
11. run normal NDMF / avatar build validation;
12. inspect generated Animator / parameter / menu output when possible;
13. verify the requested behavior in Gesture Manager or another appropriate test environment.

Do not use Manual Bake for ACP validation.

## 12. Final Review

Treat these as unexpected unless the user explicitly requested them:

- changes to authored `.controller` or `.anim` assets;
- unrelated Scene / Prefab / Asset changes;
- package-core edits when implementing only a project module;
- a new per-feature NDMF plugin;
- a persistent working avatar clone;
- Manual Bake output;
- a local Animator added only to make ACP work.

Review the Git diff when Git is available. Do not Commit, Push, or Publish unless the user explicitly requests it.

## Completion Check

```text
[ ] Edited the source avatar
[ ] Did not create a persistent working avatar clone
[ ] Did not use Manual Bake
[ ] Did not move/reparent/rename/delete existing targets merely for ACP
[ ] Verified Unity selection and exact targets
[ ] Did not infer component semantics from serialized values
[ ] ACP and MAMA are on the same feature host
[ ] No local Animator / targetAnimator / blank-controller workaround was introduced
[ ] Module Set is assigned and definitions are configured
[ ] MAMA Animator Controller was verified
[ ] MAMA playable layer was verified by name
[ ] MAMA Path Mode / Relative Path Root were verified by name
[ ] Used context.Layer(...) for generated layers
[ ] Treated Layer suffixes as exact strings; no dot-to-underscore assumption
[ ] Used Require* or AnimatorCodeObjectReference for required targets
[ ] Verified relevant installed AAC / MA APIs
[ ] MA Parameters are registered when expression parameters are required
[ ] Modified component state was read back
[ ] Unity compilation succeeded
[ ] NDMF validation succeeded
[ ] Requested behavior was verified
[ ] No unintended changes remain
```
