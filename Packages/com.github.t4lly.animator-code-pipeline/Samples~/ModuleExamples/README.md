# Animator Code Pipeline — Module Examples

These are learning examples, not additional required templates.

Copy or import the sample, replace every `REPLACE/...` value, then add only the definitions you want directly to the target `AnimatorCodePipelineSettings`.

## Host setup

These examples define generated Animator content only. Configure the avatar host separately:

- Add `AnimatorCodePipelineSettings` to the feature host.
- Add exactly one `ModularAvatarMergeAnimator` to the same host and assign its regular `AnimatorController`.
- Configure the Merge Animator playable layer, normally `FX`, and choose its Path Mode intentionally: `Absolute` uses the avatar root, while `Relative` uses the configured Relative Path Root (or the Merge Animator host when unset).
- ACP does not use a local Unity `Animator`; the Merge Animator owns the controller configuration.
- Register generated expression parameters with `ModularAvatarParameters` when the feature exposes parameters to the avatar.
- Configure menu components separately or on the same host. They do not need to be separate GameObjects.

ACP clones the Merge Animator controller during the NDMF build and does not modify the authored controller or the avatar root FX controller.

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
