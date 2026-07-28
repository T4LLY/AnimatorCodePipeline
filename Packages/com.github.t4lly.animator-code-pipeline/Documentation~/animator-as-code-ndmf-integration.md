# Animator As Code and NDMF integration

Animator Code Pipeline builds on the established combination of **Animator As Code (AAC)** and **NDMF** for non-destructive Animator generation.

Animator As Code previously provided a separate integration package named **Animator As Code V1 - NDMF Processor** (`dev.hai-vr.animator-as-code.v1.ndmf-processor`).

That package provided a convenient component-oriented way to connect AAC generation with the NDMF build process.

The current Animator As Code documentation instead recommends integrating AAC from within a tool's own NDMF plugin. Animator Code Pipeline follows this current integration pattern.

## Different abstraction, same foundation

Animator Code Pipeline does not attempt to reproduce the NDMF Processor abstraction.

This is not a replacement for Animator As Code or a different approach to AAC + NDMF integration. AAC remains responsible for expressing and generating Animator logic, while NDMF provides the non-destructive build environment.

Animator Code Pipeline adds a project-level module system on top of those tools.

The distinction is mainly about the unit of organization.

The NDMF Processor model was organized around processing components of a particular type.

Animator Code Pipeline is designed around explicitly selected C# modules and Settings configurations instead. This makes it possible for a project to organize Animator generation independently from the number or type of feature components present on the avatar.

For example, a project module may:

* inspect the avatar as a whole;
* coordinate several avatar objects or features;
* inspect multiple component types;
* generate several related Animator behaviors together;
* share generated layers with other modules;
* participate in a deterministic project-defined module set.

These requirements lead naturally to a project-level module abstraction rather than a component-oriented processor abstraction.

## Animator Code Pipeline model

Animator Code Pipeline uses one central `AnimatorCodePipelinePlugin` for the avatar build.

Project Animator logic is implemented as normal C# `AnimatorCodeModule` classes and explicitly selected through `AnimatorCodeModuleSet`.

An avatar may contain multiple enabled `AnimatorCodePipelineSettings` components. Each Settings component defines an independent generation configuration.

Within one Settings configuration:

1. the selected modules are loaded and validated;
2. applicable modules are evaluated;
3. the same-host Merge Animator controller is cloned into a temporary working Animator Controller for the build;
4. modules generate Animator logic through Animator As Code;
5. modules may share generated layers through `context.Layer(...)`;
6. the Settings component's existing `ModularAvatarMergeAnimator` determines the target VRChat playable layer;
7. Modular Avatar performs the final non-destructive integration through NDMF.

A module is therefore a unit of project code, not an NDMF plugin and not a one-to-one counterpart of a component on the avatar.

## Why this model is useful for Animator Code Pipeline

Animator Code Pipeline is intended to keep project Animator logic in normal, Git-managed C# source.

A module may represent a feature such as clothing, gestures, props, facial behavior, or tracking logic, but it may also coordinate several of these concerns when that better matches the project architecture.

This allows the project to choose appropriate code boundaries without requiring each feature to define its own NDMF integration.

Adding a feature normally means adding or editing an `AnimatorCodeModule` and selecting it in a Module Set.

The central pipeline continues to handle the NDMF lifecycle and the shared build environment.

## Relationship to current AAC guidance

Animator Code Pipeline follows the current Animator As Code guidance for non-destructive NDMF integration:

* AAC is initialized from a tool-owned NDMF plugin;
* generated Animator assets are treated as build-time assets;
* project-owned Animator Controller assets are not used as destinations for generated content;
* Modular Avatar and NDMF perform the final non-destructive integration.

Animator Code Pipeline primarily adds an organizational layer for project code on top of this integration model.

## References

* Animator As Code — NDMF Processor documentation
* Animator As Code — Getting started
* Animator As Code NDMF Processor repository
