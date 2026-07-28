# Animator Code Pipeline

Animator Code Pipeline is a thin, code-first bridge between:

- NDMF for non-destructive build execution
- Animator As Code V1 for controller/clip generation
- Modular Avatar As Code for merge registration and parameters
- Git for source-of-truth history
- Unity MCP / coding agents for project inspection and code editing

The generated AnimatorController is **not** the source of truth. Project-owned `AnimatorCodeModule` C# files are.

## Core rule

There is one central NDMF plugin. Do **not** create one NDMF plugin per generated controller or feature.

```text
Project modules (Git)
    -> one central plugin (Resolving, before Modular Avatar)
    -> each enabled Settings independently
    -> one temporary cloned working controller per applicable Settings
    -> generated layers shared by exact Layer(suffix)
    -> existing MA Merge Animator
    -> Modular Avatar normal merge
    -> final avatar controllers
```

## Install

Install dependencies through VCC/ALCOM, then install this package as a VPM package or embedded package.
Animator As Code's official documentation currently recommends VCC/ALCOM rather than direct UPM/tarball installation.

Required package ranges are declared in `package.json` under `vpmDependencies`.

## First use

1. Put the **Animator Code Pipeline** component on the avatar root.
2. Run **Tools > Animator Code Pipeline > Create Git-managed Module Folder**.
3. Add project modules under `Assets/AnimatorCodePipeline/Editor/`.
4. Each module derives from `AnimatorCodeModule`.
5. Enter Play Mode or build the avatar. NDMF invokes the pipeline.

See `Documentation~/architecture.md` and the bundled Skill.
