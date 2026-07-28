using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using UnityEngine;
using VRC.SDKBase;

namespace AnimatorCodePipeline
{
    /// <summary>
    /// Marker interface for serializable Animator Code Pipeline module definitions.
    /// Concrete project modules live in the Editor assembly and implement this through AnimatorCodeModule.
    /// </summary>
    public interface IAnimatorCodeModuleDefinition
    {
    }

    /// <summary>
    /// Defines one independent Animator Code Pipeline generation boundary.
    /// Module definitions and their user-adjustable configuration are stored directly on this component.
    /// This component is editor-only because it configures an NDMF build.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ModularAvatarMergeAnimator))]
    [AddComponentMenu("Animator Code Pipeline")]
    [Icon("Packages/com.github.t4lly.animator-code-pipeline/icon.png")]
    public sealed class AnimatorCodePipelineSettings : MonoBehaviour, IEditorOnly
    {
        [Tooltip("Modules evaluated for this feature. User-adjustable module configuration is stored directly on this component.")]
        [SerializeReference]
        private List<IAnimatorCodeModuleDefinition> modules = new List<IAnimatorCodeModuleDefinition>();

        public IReadOnlyList<IAnimatorCodeModuleDefinition> Modules => modules;
    }
}
