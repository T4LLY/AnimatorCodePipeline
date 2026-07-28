using nadena.dev.modular_avatar.core;
using UnityEngine;
using VRC.SDKBase;

namespace AnimatorCodePipeline
{
    /// <summary>
    /// Selects one Animator Code Pipeline module set for one generated feature group.
    /// This component is editor-only because it configures an NDMF build.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ModularAvatarMergeAnimator))]
    [AddComponentMenu("Animator Code Pipeline")]
    [Icon("Packages/com.github.t4lly.animator-code-pipeline/icon.png")]
    public sealed class AnimatorCodePipelineSettings : MonoBehaviour, IEditorOnly
    {
        [Tooltip("Animator Code Module Set asset evaluated for this feature.")]
        public Object moduleSet;
    }
}
