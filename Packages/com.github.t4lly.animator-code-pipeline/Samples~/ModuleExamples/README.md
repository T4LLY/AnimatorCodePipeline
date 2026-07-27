# Animator Code Pipeline — Module Examples

These are learning examples, not additional required templates.

Copy or import the sample, replace every `REPLACE/...` value, then add only the modules you want to an `AnimatorCodeModuleSet`.

## Host setup

These examples define generated Animator content only. Configure the avatar host separately:

- Add `AnimatorCodePipelineSettings` to the feature host.
- Assign a project-owned regular `AnimatorController` to its `Source Controller` field.
- Add exactly one `ModularAvatarMergeAnimator` to the same host and assign the same controller.
- Configure the Merge Animator playable layer, normally `FX`, and use `Absolute` path mode unless a verified relative path root is required.
- A local Unity `Animator` component is not required. Do not use the avatar root Animator as the Source Controller.
- Register generated expression parameters with `ModularAvatarParameters` when the feature exposes parameters to the avatar.
- Configure menu components separately or on the same host. They do not need to be separate GameObjects.

The Source Controller is only a project-owned template. ACP clones it during the NDMF build and does not modify the authored controller or the avatar root FX controller.

## Examples

### ExampleBlendShapeRadialModule

Shows:

- `RequireComponent<SkinnedMeshRenderer>()`
- Float Animator parameter
- AAC Simple 1D Blend Tree
- blend shape animation
- Modular Avatar expression parameter
- radial menu item

The `.Radial(weight)` call configures the generated menu item for a Radial Puppet. The parameter used by the radial control is the same float parameter passed to `Radial`; it is not the open/close parameter used by Sub Menu or other Puppet controls.

Replace:

- `REPLACE/WITH/BODY/PATH`
- `REPLACE_BLENDSHAPE`

### ExampleIntSelectorModule

Shows:

- Int Animator parameter
- three mutually exclusive states
- multiple required GameObjects
- Modular Avatar Int parameter registration

Replace:

- `REPLACE/WITH/OPTION_A`
- `REPLACE/WITH/OPTION_B`
- `REPLACE/WITH/OPTION_C`

### ExampleMultiLayerSharedParameterModule

Shows:

- one module controlling multiple generated layers
- one Animator parameter name reused across those layers
- one module coordinating several avatar features

Replace:

- `REPLACE/WITH/PRIMARY_TARGET`
- `REPLACE/WITH/SECONDARY_TARGET`

The same parameter must be requested on every AAC layer that uses it.

### ExampleTwoConditionModule

Shows:

- `BoolParameters(...)`
- transition when all conditions are true
- transition back when any condition is false
- Modular Avatar parameter group registration

Replace:

- `REPLACE/WITH/REAL/PATH`

### ExampleTransformScaleModule

Shows:

- `RequireTransform(...)`
- transform animation with AAC
- using the authored local scale as the enabled value

Replace:

- `REPLACE/WITH/REAL/PATH`

## Design rules demonstrated here

Required hierarchy objects are resolved in `Build()` through:

- `RequireTransform`
- `RequireGameObject`
- `RequireComponent<T>`

Do not use `IsApplicable()` merely to hide a missing required path.

`IsApplicable()` is for genuinely optional modules and must remain side-effect free.

`context.Layer(...)` creates or retrieves a generated layer in the Settings working controller. It does not select a VRChat playable layer; the Settings-side Modular Avatar Merge Animator determines that.

Generated Animator content remains build-time output. Project-owned C# modules are the source of truth.
