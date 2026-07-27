# Compatibility

## Compatibility baseline

Animator Code Pipeline targets the following dependency range:

* Unity 2022.3
* Animator As Code V1 >= 1.2.0 and < 2.0.0-a
* Animator As Code - Modular Avatar >= 1.1.1 and < 2.0.0-a
* NDMF >= 1.6.0 and < 2.0.0-a
* Modular Avatar >= 1.9.9 and < 2.0.0-a
* VRChat Avatars SDK >= 3.5.2 and < 999.0.0-a

These ranges define the compatibility baseline of the package. Newer compatible releases within these ranges may be used without changing Animator Code Pipeline.

## Animator As Code and NDMF

Animator As Code 1.2.0 introduced `IAacAssetContainerProvider`, allowing generated AAC assets to be managed by an external build system.

NDMF 1.6.0 introduced `IAssetSaver`, and the AAC NDMF integration pattern uses an NDMF-backed asset container provider so generated animations, blend trees, and related assets remain part of the non-destructive build process.

Animator Code Pipeline follows this model rather than persisting generated Animator assets into the project.

## Animator As Code 1.3

Animator Code Pipeline does not depend on the Animator As Code 1.3 Modification API.

The Modification API is intended for editing or replacing the contents of existing Animator assets and requires AAC 1.3.0 or later. Animator Code Pipeline instead builds against the AAC 1.2-compatible generation APIs and operates on temporary build-time assets.

This keeps AAC 1.2.0 as the minimum supported version and avoids making destructive Animator modification APIs part of the pipeline contract.
