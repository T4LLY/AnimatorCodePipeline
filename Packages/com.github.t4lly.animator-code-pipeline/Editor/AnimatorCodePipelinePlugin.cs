using System;
using System.Collections.Generic;
using AnimatorAsCode.V1;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[assembly: ExportsPlugin(typeof(AnimatorCodePipeline.AnimatorCodePipelinePlugin))]

namespace AnimatorCodePipeline
{
    /// <summary>
    /// One central NDMF plugin. It evaluates each configured Module Set independently per avatar build.
    /// </summary>
    public sealed class AnimatorCodePipelinePlugin : Plugin<AnimatorCodePipelinePlugin>
    {
        public override string QualifiedName => "com.github.t4lly.animator-code-pipeline";
        public override string DisplayName => "Animator Code Pipeline";

        protected override void Configure()
        {
            // MA virtualizes Merge Animator controllers during Resolving. Create ours first so it is
            // included in that non-destructive controller pipeline.
            InPhase(BuildPhase.Resolving)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run($"Generate {DisplayName}", Generate);
        }

        private static void Generate(BuildContext ctx)
        {
            var settingsComponents = ctx.AvatarRootTransform
                .GetComponentsInChildren<AnimatorCodePipelineSettings>(true);

            ValidateSettings(settingsComponents);

            foreach (var settings in settingsComponents)
            {
                GenerateForSettings(ctx, settings);
            }

        }

        internal static void ValidateSettings(IEnumerable<AnimatorCodePipelineSettings> settingsComponents)
        {
            foreach (var settings in settingsComponents)
            {
                if (settings == null || !settings.enabled) continue;

                if (settings.moduleSet as AnimatorCodeModuleSet == null)
                {
                    throw new InvalidOperationException(
                        $"Animator Code Pipeline on '{settings.name}' requires an AnimatorCodeModuleSet asset.");
                }

                var mergeAnimator = settings.GetComponent<ModularAvatarMergeAnimator>();
                if (mergeAnimator == null)
                    throw new InvalidOperationException(
                        $"Animator Code Pipeline on '{settings.name}' requires an MA Merge Animator on the same GameObject.");
                if (mergeAnimator.animator == null)
                {
                    throw new InvalidOperationException(
                        $"MA Merge Animator on '{settings.name}' requires an Animator Controller.");
                }

                if (!(mergeAnimator.animator is AnimatorController))
                    throw new InvalidOperationException(
                        $"MA Merge Animator on '{settings.name}' must reference a regular AnimatorController, not an AnimatorOverrideController.");
            }
        }

        private static void GenerateForSettings(BuildContext ctx, AnimatorCodePipelineSettings settings)
        {
            if (settings == null || !settings.enabled) return;
            var moduleSet = settings.moduleSet as AnimatorCodeModuleSet;
            if (moduleSet == null)
            {
                throw new InvalidOperationException(
                    $"Animator Code Pipeline on '{settings.name}' requires a Module Set.");
            }

            if (settings.GetComponent<ModularAvatarMergeAnimator>() == null)
            {
                throw new InvalidOperationException(
                    $"Animator Code Pipeline on '{settings.name}' requires an MA Merge Animator on the same GameObject.");
            }

            var modules = moduleSet.CreateModules();
            if (modules.Count == 0) return;

            var applicableModules = GetApplicableModules(
                modules,
                ctx.AvatarRootObject,
                settings);
            if (applicableModules.Count == 0) return;

            var mergeAnimator = settings.GetComponent<ModularAvatarMergeAnimator>();
            var bindingRoot = GetBindingRoot(ctx, settings, mergeAnimator);
            var generatedController = CreateWorkingController(settings);
            ctx.AssetSaver.SaveAsset(generatedController);

            var aac = AacV1.Create(new AacConfiguration
            {
                SystemName = "AnimatorCodePipeline",
                AnimatorRoot = bindingRoot,
                DefaultValueRoot = ctx.AvatarRootTransform,
                AssetKey = GUID.Generate().ToString(),
                AssetContainer = ctx.AssetContainer,
                ContainerMode = AacConfiguration.Container.OnlyWhenPersistenceRequired,
                AssetContainerProvider = new NdmfAssetContainerProvider(ctx),
                DefaultsProvider = new AacDefaultsProvider(false)
            });

            var buildContext = new AnimatorCodeBuildContext(
                ctx, settings, aac, generatedController, bindingRoot);

            foreach (var module in applicableModules)
            {
                try
                {
                    module.Build(buildContext);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Animator Code Pipeline module '{module.Id}' failed during Build().", ex);
                }
            }

            if (buildContext.HasGeneratedLayers)
                buildContext.FinalizeMergeAnimators();
        }

        internal static IReadOnlyList<AnimatorCodeModule> GetApplicableModules(
            IReadOnlyList<AnimatorCodeModule> modules,
            GameObject avatarRoot,
            AnimatorCodePipelineSettings settings)
        {
            if (modules == null) throw new ArgumentNullException(nameof(modules));
            if (avatarRoot == null) throw new ArgumentNullException(nameof(avatarRoot));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var applicableModules = new List<AnimatorCodeModule>();
            foreach (var module in modules)
            {
                if (module == null)
                    throw new InvalidOperationException(
                        "Animator Code Pipeline encountered a null module instance.");

                bool applicable;
                try
                {
                    applicable = module.IsApplicable(avatarRoot, settings);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Animator Code Pipeline module '{module.Id}' failed during IsApplicable().", ex);
                }

                if (applicable)
                    applicableModules.Add(module);
            }

            return applicableModules;
        }

        internal static AnimatorController CreateWorkingController(AnimatorCodePipelineSettings settings)
        {
            var mergeAnimator = settings == null ? null : settings.GetComponent<ModularAvatarMergeAnimator>();
            if (mergeAnimator == null || mergeAnimator.animator == null)
                throw new InvalidOperationException(
                    "Animator Code Pipeline requires an MA Merge Animator with an Animator Controller.");

            if (!(mergeAnimator.animator is AnimatorController sourceController))
                throw new InvalidOperationException(
                    "Animator Code Pipeline MA Merge Animator must reference a regular AnimatorController, not an Animator Override Controller.");

            var workingController = UnityEngine.Object.Instantiate(sourceController);
            workingController.name = $"{sourceController.name} [AnimatorCodePipeline Build]";
            return workingController;
        }

        internal static Transform GetBindingRoot(
            BuildContext context,
            AnimatorCodePipelineSettings settings,
            ModularAvatarMergeAnimator mergeAnimator)
        {
            if (mergeAnimator.pathMode == MergeAnimatorPathMode.Absolute)
                return context.AvatarRootTransform;

            var root = mergeAnimator.relativePathRoot.Get(context.AvatarRootTransform);
            return root != null ? root.transform : settings.transform;
        }
    }
}
