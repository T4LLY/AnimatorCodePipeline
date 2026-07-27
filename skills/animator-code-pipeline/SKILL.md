---
name: animator-code-pipeline
description: Build VRChat Animator features non-destructively as project-owned C# modules using Animator Code Pipeline, Animator As Code, NDMF, Modular Avatar, and Unity MCP.
---

# Animator Code Pipeline

Use this skill to create, modify, review, and debug ACP modules.

**Project-owned C# modules are the source of truth.**  
Generated Animator Controllers, Animation Clips, Layers, States, Transitions, and Blend Trees are build outputs.

## 1. Hard Rules

- Inspect the live Unity project before editing. Never guess hierarchy paths, component fields, enum values, serialized values, package versions, or APIs.
- **Edit the source avatar. Do not create a persistent working avatar clone.**
- **Do not use Modular Avatar Manual Bake for ACP implementation, inspection, or validation.**
- Temporary build clones created internally by NDMF / MA are expected, but never edit them as project targets.
- **Do not move, reparent, rename, or delete existing avatar GameObjects for ACP.**
- Only add ACP / MA feature-host GameObjects when needed.
- Do not directly modify the avatar's source Animator Controllers for ACP-managed behavior.
- Do not directly rewrite source `VRCExpressionsMenu` or `VRCAvatarDescriptor` assets when Modular Avatar can represent the feature non-destructively.
- Do not create another NDMF plugin for a normal feature. ACP owns NDMF integration.
- Use `context.Layer(...)` for generated layers.
- Use `RequireGameObject`, `RequireTransform`, or `RequireComponent<T>` for required targets.
- Do not hide missing required targets with `IsApplicable` or silent returns.
- Do not integrate third-party ACP modules or C# scripts without inspecting them first.
- A successful file write or MCP mutation is not completion. Compile and validate the NDMF result.

### Never infer component semantics from serialization

**Never infer Unity, Modular Avatar, VRChat SDK, or ACP behavior from raw serialized data.**

Do not guess the meaning of:

- enum integers;
- serialized field names;
- flags / bit values;
- default numeric values;
- undocumented relationships between serialized properties.

For component configuration:

1. inspect the installed component/API/type definition;
2. prefer named enum/API values over raw integers;
3. use Inspector state, installed package source, or Reflection when needed;
4. make the change;
5. read the live component back;
6. verify the resulting named state.

Raw serialized values may be used only after their meaning has been confirmed against the installed package version.

```text
Inspect Unity
  ↓
Verify API / component semantics
  ↓
Plan the smallest change
  ↓
Create or modify Module
  ↓
Configure ACP / MA
  ↓
Read back configuration
  ↓
Compile
  ↓
Validate NDMF result
  ↓
Review changes
```

## 2. Inspect Unity First

Use the installed Unity MCP implementation when available. Tool names vary by implementation; use capabilities rather than assuming exact tool names.

Inspect only what the feature requires:

- active Unity project / instance;
- avatar root and `VRCAvatarDescriptor`;
- current Unity selection;
- exact avatar-relative hierarchy paths for targets;
- relevant Components, Renderers, Materials, Blend Shapes, and Parameters;
- existing ACP Settings, ModuleSet, Modules, and local Animator;
- existing `ModularAvatarMergeAnimator`;
- existing MA Menu / Parameter components;
- Unity Console compile errors.

If multiple candidates exist, do not guess.

When the user has selected objects in Unity, prefer the live selection.

## 3. Feature Host Placement

Place ACP beside the feature targets when they belong to one clear hierarchy group.

### Single target

When creating a new host for one target, place the ACP host under the target's parent so the host is a sibling of the target.

```text
Parent
├── Target
└── ACP Feature Host
```

Keep the target where it is.

Reuse a suitable existing ACP host at that level when possible.

Do not put the ACP host inside the target merely to avoid creating a sibling host.

If the feature boundary makes sibling placement unclear, ask the user.

### Multiple targets

If multiple targets share the same parent and form one feature, create or reuse one ACP host under that common parent.

```text
Parent
├── Target A
├── Target B
├── Target C
└── ACP Feature Host
```

Do not create one ACP host per target when one common host represents the feature.

If the targets are in different branches, different outfit / feature roots, or do not have one clear common feature parent, **ask the user before choosing the host location.**

Never move or reparent targets to manufacture a common parent.

### Host component grouping

ACP Settings, the local ACP Animator, MA Merge Animator, MA Parameters, and MA Menu components may live on the same feature host.

Do not split ACP and menu configuration into separate GameObjects without a concrete reason.

## 4. Required ACP Host Configuration

For the current ACP version, a normal feature host uses:

```text
ACP Feature Host
├── AnimatorCodePipelineSettings
├── Animator
├── ModularAvatarMergeAnimator
├── ModularAvatarParameters      (when avatar parameter registration is needed)
└── MA Menu components           (when menu controls are needed)
```

The host `Animator` is an ACP-local Animator. It is **not** the avatar root Animator.

### Blank Controller relationship

Create or use a dedicated project-owned blank regular `AnimatorController`.

Configure the references as follows:

```text
AnimatorCodePipelineSettings.targetAnimator
        ↓
ACP host Animator
        ↓
Animator.runtimeAnimatorController
        ┐
        ├── same blank AnimatorController
        │
ModularAvatarMergeAnimator.animator
        ┘
```

Required rules:

- `targetAnimator` points to the local Animator on the ACP feature host.
- The local Animator uses the dedicated blank Animator Controller.
- `ModularAvatarMergeAnimator.animator` references the **same blank Animator Controller**.
- **Never set `targetAnimator` to the avatar root Animator.**
- **Never use the avatar's existing FX / Gesture / Action controller as the ACP host's blank controller.**
- Do not leave MA Merge Animator referencing the avatar's controller after fixing only `targetAnimator`.

ACP clones the local blank controller during the NDMF build, generates AAC content into the working clone, and replaces the Merge Animator controller reference on the build clone with the generated controller.

Do not modify the authored blank controller directly from a module.

### Attached Animator removal

Because the current ACP host includes a local `Animator`, enable the Modular Avatar option whose installed-version meaning is **remove/delete the attached Animator after merge**.

Do not identify this option by serialized numeric value. Verify the actual Inspector/API meaning in the installed MA version and read it back after setting it.

## 5. ACP Contract

ACP owns one central NDMF plugin and runs in `BuildPhase.Resolving`.

Do not change the build phase for a normal feature.

An avatar may contain multiple `AnimatorCodePipelineSettings`. Each Settings is an independent generation boundary.

The current Settings boundary associates:

- `AnimatorCodeModuleSet`;
- local ACP host Animator through `targetAnimator`;
- blank source `AnimatorController`;
- same-host `ModularAvatarMergeAnimator`;
- temporary working controller created during build.

Use a regular `AnimatorController`. Do not use `AnimatorOverrideController`.

Do not reimplement ACP controller lifecycle logic from a module.

```text
Validate Modules
  ↓
Sort by Order → Id → Fully Qualified Type Name
  ↓
IsApplicable
  ↓
Clone blank Source Controller
  ↓
Persist as NDMF temporary asset
  ↓
Build Modules
  ↓
Pass generated working controller to MA Merge Animator
```

Never generate directly into the authored blank controller or the avatar's source controller.

## 6. Module Contract

Modules derive from `AnimatorCodeModule`.

```csharp
public sealed class ExampleModule : AnimatorCodeModule
{
    public override string Id => "project.example";
    public override int Order => 0;

    public override bool IsApplicable(
        GameObject avatarRoot,
        AnimatorCodePipelineSettings settings)
        => true;

    public override void Build(AnimatorCodeBuildContext context)
    {
        var target = context.RequireGameObject("Confirmed/Path");
        var layer = context.Layer("Example");

        // AAC generation
    }
}
```

### `Id`

- must not be empty;
- must be unique within the same Settings execution;
- should remain stable across ordinary refactors;
- is not a Layer name.

### `Order`

Use only when execution order matters. ModuleSet Inspector list order does not define execution order.

### `IsApplicable`

Use only when the entire module is genuinely optional, and keep it side-effect free.

Do not use it to hide a missing required target.

### Module validation

Treat these as errors rather than silently skipping them:

- null / unresolved script;
- type that is not an `AnimatorCodeModule`;
- abstract / unbound generic type;
- missing public parameterless constructor;
- empty `Id`;
- duplicate `Id`.

Do not create a custom NDMF `Plugin<T>` or `[assembly: ExportsPlugin]` from a module.

## 7. Build Context and Layers

Use the ACP context from normal modules:

```text
context.Layer(...)
context.RequireGameObject(...)
context.RequireTransform(...)
context.RequireComponent<T>(...)
context.Aac
context.ModularAvatar
```

Do not call `context.Aac.NewAnimatorController()` from a normal module.

Use only target paths confirmed in Unity.

```csharp
var sword = context.RequireGameObject("Armature/Hips/Spine/Chest/Sword");
var body = context.RequireComponent<SkinnedMeshRenderer>("Body");
```

Do not replace required targets with guessed `Transform.Find(...)` calls followed by silent returns.

### Layer

```csharp
var layer = context.Layer("FeatureName");
```

The same raw suffix within one Settings shares the same generated layer.

`.` is normalized to `_`.

If different raw suffixes normalize to the same name, for example:

```text
Face.Blink
Face_Blink
```

treat that as an explicit collision error.

`context.Layer(...)` does not select FX / Gesture / Action. The associated `ModularAvatarMergeAnimator` selects the destination playable layer.

## 8. Samples and External APIs

When available, inspect ACP examples first:

```text
Packages/com.github.t4lly.animator-code-pipeline/Samples~/ModuleExamples
```

Relative to the package root:

```text
Samples~/ModuleExamples
```

Samples are implementation references. Adapt hierarchy paths, parameters, and version-specific assumptions to the live project.

### Animator As Code (AAC)

Before creating or modifying AAC code, check the relevant AAC API in Context7 when Context7 is available:

```text
/hai-vr/documentation
```

Read only the relevant sections.

If Context7 is unavailable or unclear, inspect the installed AAC package / version / source.

Do not invent AAC APIs from memory.

### Modular Avatar (MA)

When MA API, component fields, enum values, or behavior needs verification, use Context7 as needed:

```text
/bdunderscore/modular-avatar
```

For version-specific component behavior, the installed MA package/source and live component state are authoritative.

### VRChat SDK / Creator Docs

When VRChat standard components or parameter behavior needs verification, use Context7 as needed:

```text
/websites/creators_vrchat
```

Do not guess behavior for Contacts, PhysBones, Avatar Parameter Driver, built-in parameters, or similar features.

If external documentation conflicts with the installed project, prefer the installed package and live Unity state.

## 9. Modular Avatar Merge Animator

Inspect the existing Merge Animator before changing it.

### Playable Layer

Do not infer the playable layer from a serialized enum integer.

For ordinary ACP-driven avatar feature behavior such as:

- toggles;
- Radial Puppet-controlled transforms;
- Blend Shapes;
- material / object state;
- feature animation driven by expression parameters;

the normal destination is **FX**.

Use Gesture, Action, Base, Additive, or another layer only when the requested behavior and existing avatar architecture explicitly require it.

After setting the layer, read the component again and verify the named playable layer.

### Path Mode

For normal ACP feature hosts, set `ModularAvatarMergeAnimator` Path Mode to **Absolute**.

ACP hosts are configuration hosts and are not automatically a valid relative binding root for the feature targets.

Do not use Relative merely because the host and targets exist somewhere inside the same avatar.

Use Relative only when:

- the animation bindings are intentionally relative to the Merge Animator path basis;
- all relevant targets fit that relative hierarchy;
- the relationship has been explicitly verified from the live hierarchy.

After configuration, read the component again and confirm the named Path Mode is **Absolute**.

Do not infer `Relative` / `Absolute` from serialized integer values.

## 10. Modular Avatar Parameters and Menu

Prefer Modular Avatar for non-destructive parameter and menu integration.

Inspect existing MA components before adding new ones and avoid duplicate parameters/menu entries.

### Avatar parameter registration

Creating an Animator parameter does **not** by itself register that parameter in the avatar's Expression Parameters.

When the feature needs an avatar expression parameter:

1. create or reuse `ModularAvatarParameters`;
2. register the parameter with the required type/default/sync/save configuration;
3. verify the resulting MA component state;
4. verify the parameter in the final NDMF-generated avatar configuration.

Do not assume AAC Animator parameter creation replaces MA/VRChat Expression Parameter registration.

### Menu Control Type

Never infer Menu Item Control Type from a serialized integer.

Verify the named Control Type through the installed MA enum/API, Inspector, source, or Reflection.

After setting it, read the component again and verify the named state.

### Radial Puppet

For a Radial Puppet:

- set the Menu Item Control Type to **Radial Puppet**;
- place the parameter controlled by the radial input in the Radial Puppet's **control-specific SubParameter / radial parameter slot**;
- do not put the radial-control parameter into the ordinary Menu Item activation/open `parameter` field merely because it is named `parameter`.

Submenus and other Puppet controls can have separate activation/open parameters and control parameters.

Do not assume their parameter fields have the same meaning as Radial Puppet.

Prefer this verification order:

```text
ACP sample
  ↓
MA high-level API / Context7
  ↓
installed MA enum/type/source or Reflection
  ↓
live component read-back
  ↓
NDMF generated result
```

## 11. Validation

After making changes:

1. wait for Unity compilation;
2. inspect and fix Console errors;
3. verify ModuleSet registration;
4. verify ACP host placement;
5. verify `targetAnimator` points to the local ACP host Animator;
6. verify the local Animator and MA Merge Animator reference the same dedicated blank controller;
7. verify attached-Animator removal is enabled for the current host setup;
8. verify Merge Animator playable layer by **name**;
9. verify Path Mode is **Absolute** by **name** unless Relative was intentionally proven correct;
10. verify MA Parameters when the feature needs avatar expression parameters;
11. verify Menu Control Type and control-specific parameter fields;
12. read modified components back rather than trusting serialized writes;
13. run normal NDMF / avatar build validation;
14. inspect generated Animator / parameter / menu output when possible;
15. verify the requested behavior in Gesture Manager or another appropriate test environment.

**Do not use Manual Bake for validation.**

During compilation, domain reload, or MCP reconnection, re-check state before retrying mutations.

## 12. Final Review

Before completion, confirm that no changes outside the requested ACP / MA work were introduced.

Treat the following as unexpected:

- modifications to existing source `.controller` / `.anim` assets;
- unrelated Asset / Scene / Prefab changes;
- package core / plugin changes;
- a new NDMF plugin;
- a persistent working avatar clone;
- Manual Bake output.

A newly created dedicated blank ACP controller is expected when the feature requires one.

When Git is available, review the diff.

Do not Commit, Push, or Publish unless the user explicitly requests it.

## Completion Check

```text
[ ] Edited the source avatar
[ ] Did not create a persistent working avatar clone
[ ] Did not use Manual Bake
[ ] Did not move / reparent / rename / delete existing avatar objects
[ ] Verified Unity selection / targets
[ ] Did not guess hierarchy paths
[ ] Did not infer component semantics from serialized values
[ ] Single target host is a sibling under the target's parent when appropriate
[ ] Multiple same-parent targets share one sibling ACP host when appropriate
[ ] Asked before choosing a host for targets with no clear common feature parent
[ ] ACP / MA / Menu configuration was not unnecessarily split across GameObjects
[ ] targetAnimator points to the local ACP host Animator
[ ] Local Animator uses the dedicated blank Animator Controller
[ ] MA Merge Animator uses the same blank Animator Controller
[ ] Avatar root Animator / avatar FX controller is not used as ACP input
[ ] Attached-Animator removal setting was verified and enabled
[ ] Merge Animator playable layer was verified by name
[ ] Normal ACP feature uses FX unless another layer was explicitly required
[ ] Merge Animator Path Mode was verified by name
[ ] Path Mode is Absolute unless Relative was intentionally proven correct
[ ] Used context.Layer(...)
[ ] Used Require* for required targets
[ ] Verified relevant AAC API
[ ] MA Parameters are registered when avatar parameters are required
[ ] Menu Control Type was verified by name
[ ] Radial Puppet uses its control-specific SubParameter / radial parameter slot
[ ] Modified components were read back after setting
[ ] Unity compilation succeeded
[ ] NDMF validation succeeded
[ ] Requested behavior was verified
[ ] No unintended changes remain
```
