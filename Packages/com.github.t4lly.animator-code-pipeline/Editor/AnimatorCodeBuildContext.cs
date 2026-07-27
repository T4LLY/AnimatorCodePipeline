using System;
using AnimatorAsCode.V1;
using AnimatorAsCode.V1.ModularAvatar;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;
using UnityEditor.Animations;
using UnityEngine;

namespace AnimatorCodePipeline
{
    /// <summary>
    /// Shared build context for all modules in one avatar build.
    /// A build-only clone of the configured Animator Controller receives all generated layers.
    /// </summary>
    public sealed class AnimatorCodeBuildContext
    {
        private readonly GeneratedLayerCache<AacFlLayer> _layers =
            new GeneratedLayerCache<AacFlLayer>();
        private readonly AnimatorController _workingController;

        private GameObject _generatedRoot;
        private MaAc _modularAvatar;

        internal AnimatorCodeBuildContext(
            BuildContext ndmfContext,
            AnimatorCodePipelineSettings settings,
            AacFlBase aac,
            AnimatorController workingController)
        {
            NdmfContext = ndmfContext ?? throw new ArgumentNullException(nameof(ndmfContext));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Aac = aac ?? throw new ArgumentNullException(nameof(aac));
            _workingController = workingController ?? throw new ArgumentNullException(nameof(workingController));
        }

        public BuildContext NdmfContext { get; }
        public AnimatorCodePipelineSettings Settings { get; }
        public AacFlBase Aac { get; }
        public GameObject AvatarRoot => NdmfContext.AvatarRootObject;
        public Transform AvatarRootTransform => NdmfContext.AvatarRootTransform;

        /// <summary>
        /// Gets a generated layer in the build-only clone of the configured Animator Controller.
        /// The source controller asset is never changed.
        /// </summary>
        public AacFlLayer Layer(string suffix)
        {
            return _layers.GetOrCreate(
                suffix,
                normalizedSuffix => Aac.CreateSupportingArbitraryControllerLayer(
                    _workingController,
                    normalizedSuffix));
        }

        /// <summary>
        /// Shared MA-as-Code instance on a generated holder object. Use for Parameters/Menu Items when appropriate.
        /// </summary>
        public MaAc ModularAvatar
        {
            get
            {
                if (_modularAvatar != null) return _modularAvatar;
                _modularAvatar = MaAc.Create(GeneratedRoot);
                return _modularAvatar;
            }
        }

        /// <summary>Find a Transform by an exact avatar-relative path; throws with a useful message when missing.</summary>
        public Transform RequireTransform(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("A non-empty avatar-relative path is required.", nameof(relativePath));

            var result = AvatarRootTransform.Find(relativePath);
            if (result == null)
                throw new InvalidOperationException(
                    $"Animator Code Pipeline could not find avatar-relative path '{relativePath}'.");
            return result;
        }

        public GameObject RequireGameObject(string relativePath) => RequireTransform(relativePath).gameObject;

        public T RequireComponent<T>(string relativePath) where T : Component
        {
            var transform = RequireTransform(relativePath);
            var component = transform.GetComponent<T>();
            if (component == null)
                throw new InvalidOperationException(
                    $"Animator Code Pipeline found '{relativePath}', but it has no {typeof(T).Name} component.");
            return component;
        }

        internal bool HasGeneratedLayers => _layers.Count != 0;

        internal void FinalizeMergeAnimators()
        {
            if (_layers.Count == 0) return;

            // MCP configures the MA Merge Animator. On the build clone, replace only its
            // controller reference so the original scene component and controller remain untouched.
            var mergeAnimator = Settings.GetComponent<ModularAvatarMergeAnimator>();
            if (mergeAnimator == null)
                throw new InvalidOperationException(
                    $"Animator Code Pipeline Settings on '{Settings.name}' requires an MA Merge Animator on the same GameObject.");

            mergeAnimator.animator = _workingController;
        }

        private GameObject GeneratedRoot
        {
            get
            {
                if (_generatedRoot != null) return _generatedRoot;

                _generatedRoot = new GameObject($"{Settings.name} [Generated]");
                _generatedRoot.transform.SetParent(Settings.transform, false);
                return _generatedRoot;
            }
        }
    }
}
