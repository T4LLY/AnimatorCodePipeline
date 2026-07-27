# Git workflow

Animator Code Pipeline is designed to keep Animator generation logic in normal, Git-managed C# source.

Project-owned `AnimatorCodeModule` files define the generated Animator behavior, while Module Sets, Settings, Modular Avatar components, and authored source assets provide the configuration required to reproduce the build.

Generated Animator assets are build-time outputs and are not the source of truth.

## Recommended project layout

A typical project layout is:

```text
Assets/
  AnimatorCodePipeline/
    Editor/
      AnimatorCodePipeline.Editor.asmref
      ClothesAnimatorModule.cs
      PropsAnimatorModule.cs
      FaceAnimatorModule.cs
```

The Assembly Definition Reference connects project modules to the package's `AnimatorCodePipeline.Editor` assembly.

Project modules may be divided into additional folders as the project grows. The folder structure does not define module execution order; execution order is determined by the module contract.

## What to commit

Commit the project-owned inputs required to reproduce the build, including:

* `AnimatorCodeModule` source files;
* `AnimatorCodeModuleSet` assets;
* scenes or prefabs containing `AnimatorCodePipelineSettings`;
* the associated Modular Avatar configuration;
* project-authored source Animator Controller assets used by the Settings configuration;
* AnimationClips and other authored assets referenced by modules;
* Unity `.meta` files associated with these assets.

As with other Unity projects, `.meta` files should remain under version control so asset GUID references remain stable across clones and branches.

Do not treat build-time generated Animator Controllers, AnimationClips, BlendTrees, or other temporary AAC assets as project source.

Generated Animator content is an intermediate result of the NDMF build and should be reproducible from the committed C# modules and project configuration.

## Typical workflow

A normal change can follow this sequence:

```text
request
  -> inspect the avatar and existing module code
  -> edit the relevant modules or configuration
  -> Unity compile
  -> NDMF / Play Mode validation
  -> review git diff
  -> commit
```

Inspection may be performed manually in Unity or with development tools that can inspect the project.

Animator Code Pipeline does not require a particular editor, automation system, or AI tool.

## AI-assisted workflow

When using a coding agent with access to Unity project information, a typical workflow may be:

```text
request
  -> inspect the avatar and project through Unity MCP
  -> inspect the relevant modules
  -> edit project-owned C# or configuration
  -> Unity compile
  -> NDMF / Play Mode validation
  -> review git diff
  -> commit
```

Unity MCP is one possible inspection and validation tool. It is not a runtime or build dependency of Animator Code Pipeline.

The same module source can be edited manually or by other development tools.

## Commit granularity

Prefer commits that describe one logical Animator behavior change.

Examples:

* `Add clothes mode state machine`
* `Disable hat in hoodie state`
* `Add combat condition to sword layer`

Because generated Animator behavior is primarily expressed as C# source, behavior changes can usually be reviewed as normal code diffs instead of large serialized Animator Controller diffs.

This makes changes easier to review, revert, branch, merge, and reproduce.
