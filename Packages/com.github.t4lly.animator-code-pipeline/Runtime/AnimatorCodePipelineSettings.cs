using UnityEngine;
using VRC.SDKBase;

namespace AnimatorCodePipeline
{
    /// <summary>
    /// Selects one Animator Code Pipeline module set for one generated feature group.
    /// This component is editor-only because it configures an NDMF build.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Animator Code Pipeline/Animator Code Pipeline Settings")]
    public sealed class AnimatorCodePipelineSettings : MonoBehaviour, IEditorOnly
    {
        [Tooltip("Animator Code Module Set asset evaluated for this settings component.")]
        public Object moduleSet;

        [Tooltip("Animator Controller used as the ACP generation template. Use the same controller in the Modular Avatar Merge Animator on this GameObject.")]
        [SerializeField]
        private RuntimeAnimatorController sourceController;

        [SerializeField, HideInInspector]
        private Animator targetAnimator;

        public RuntimeAnimatorController SourceController => sourceController;

        internal Animator LegacyTargetAnimator => targetAnimator;
    }
}
