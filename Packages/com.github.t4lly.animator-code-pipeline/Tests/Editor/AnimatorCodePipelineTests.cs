using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AnimatorCodePipeline.Tests
{
    public sealed class AnimatorCodePipelineTests
    {
        [Test]
        public void ModuleDiscovery_CreatesModulesInDeterministicOrder()
        {
            var modules = AnimatorCodeModuleDiscovery.CreateAll(new[]
            {
                typeof(LastModule),
                typeof(AlphabeticalModule),
                typeof(FirstModule)
            });

            Assert.That(modules.Select(module => module.Id), Is.EqualTo(new[]
            {
                "test.first",
                "test.alphabetical",
                "test.last"
            }));
        }

        [Test]
        public void ModuleDiscovery_RejectsDuplicateIds()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                AnimatorCodeModuleDiscovery.CreateAll(new[]
                {
                    typeof(DuplicateFirstModule),
                    typeof(DuplicateSecondModule)
                }));

            Assert.That(exception.Message, Does.Contain("duplicate module Id 'test.duplicate'"));
        }

        [Test]
        public void PipelineEnablement_UsesTheStandardComponentEnabledFlag()
        {
            var avatarRoot = new GameObject("Test Avatar");
            try
            {
                var settings = avatarRoot.AddComponent<AnimatorCodePipelineSettings>();
                Assert.That(settings.enabled, Is.True);

                settings.enabled = false;
                Assert.That(settings.enabled, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(avatarRoot);
            }
        }

        [Test]
        public void WorkingController_IsACopyOfTheConfiguredSourceController()
        {
            var avatarRoot = new GameObject("Test Avatar");
            var mergeObject = new GameObject("Merge Host");
            var controllerPath = $"Assets/AnimatorCodePipelineTest-{Guid.NewGuid():N}.controller";
            try
            {
                var sourceController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                var settings = avatarRoot.AddComponent<AnimatorCodePipelineSettings>();
                var mergeAnimator = mergeObject.AddComponent(
                    Type.GetType("nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator, nadena.dev.modular-avatar.core"));
                settings.transform.SetParent(mergeObject.transform);
                var serializedSettings = new SerializedObject(settings);
                serializedSettings.FindProperty("sourceController").objectReferenceValue = sourceController;
                serializedSettings.ApplyModifiedPropertiesWithoutUndo();
                var mergeSerialized = new SerializedObject(mergeAnimator);
                mergeSerialized.FindProperty("animator").objectReferenceValue = sourceController;
                mergeSerialized.ApplyModifiedPropertiesWithoutUndo();

                var workingController = AnimatorCodePipelinePlugin.CreateWorkingController(settings);
                Assert.That(workingController, Is.Not.SameAs(sourceController));

                workingController.AddLayer("Generated");
                Assert.That(sourceController.layers.Select(layer => layer.name), Does.Not.Contain("Generated"));

                UnityEngine.Object.DestroyImmediate(workingController);
            }
            finally
            {
                AssetDatabase.DeleteAsset(controllerPath);
                UnityEngine.Object.DestroyImmediate(mergeObject);
                UnityEngine.Object.DestroyImmediate(avatarRoot);
            }
        }

        [Test]
        public void Validation_RejectsMismatchedMergeAnimatorController()
        {
            var avatarRoot = new GameObject("Test Avatar");
            var host = new GameObject("ACP Host");
            var firstPath = $"Assets/AnimatorCodePipelineTest-{Guid.NewGuid():N}.controller";
            var secondPath = $"Assets/AnimatorCodePipelineTest-{Guid.NewGuid():N}.controller";
            try
            {
                var source = AnimatorController.CreateAnimatorControllerAtPath(firstPath);
                var other = AnimatorController.CreateAnimatorControllerAtPath(secondPath);
                var settings = host.AddComponent<AnimatorCodePipelineSettings>();
                settings.moduleSet = ScriptableObject.CreateInstance<AnimatorCodeModuleSet>();
                var merge = host.AddComponent(
                    Type.GetType("nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator, nadena.dev.modular-avatar.core"));
                var serializedSettings = new SerializedObject(settings);
                serializedSettings.FindProperty("sourceController").objectReferenceValue = source;
                serializedSettings.ApplyModifiedPropertiesWithoutUndo();
                var mergeSerialized = new SerializedObject(merge);
                mergeSerialized.FindProperty("animator").objectReferenceValue = other;
                mergeSerialized.ApplyModifiedPropertiesWithoutUndo();

                var exception = Assert.Throws<InvalidOperationException>(() =>
                    AnimatorCodePipelinePlugin.ValidateSettings(new[] { settings }));
                Assert.That(exception.Message, Does.Contain("same Animator Controller"));
            }
            finally
            {
                AssetDatabase.DeleteAsset(firstPath);
                AssetDatabase.DeleteAsset(secondPath);
                if (host != null)
                {
                    var settings = host.GetComponent<AnimatorCodePipelineSettings>();
                    if (settings != null && settings.moduleSet != null)
                        UnityEngine.Object.DestroyImmediate(settings.moduleSet);
                }
                UnityEngine.Object.DestroyImmediate(host);
                UnityEngine.Object.DestroyImmediate(avatarRoot);
            }
        }

        [Test]
        public void GeneratedLayerCache_ReturnsOneValueForTheSameSuffix()
        {
            var cache = new GeneratedLayerCache<object>();
            var createdCount = 0;
            object CreateValue(string _)
            {
                createdCount++;
                return new object();
            }

            var firstValue = cache.GetOrCreate("Shared.Feature", CreateValue);
            var secondValue = cache.GetOrCreate("Shared.Feature", CreateValue);

            Assert.That(secondValue, Is.SameAs(firstValue));
            Assert.That(createdCount, Is.EqualTo(1));
            Assert.That(cache.Count, Is.EqualTo(1));
        }

        [Test]
        public void GeneratedLayerCache_RejectsNormalizedSuffixCollision()
        {
            var cache = new GeneratedLayerCache<object>();
            cache.GetOrCreate("Face.Blink", _ => new object());

            var exception = Assert.Throws<InvalidOperationException>(() =>
                cache.GetOrCreate("Face_Blink", _ => new object()));

            Assert.That(exception.Message, Does.Contain("Face.Blink"));
            Assert.That(exception.Message, Does.Contain("Face_Blink"));
            Assert.That(exception.Message, Does.Contain("Face_Blink"));
        }

        [TestCase(typeof(AbstractModule), "abstract")]
        [TestCase(typeof(OpenGenericModule<>), "unbound generic")]
        [TestCase(typeof(NotAModule), "does not derive")]
        [TestCase(typeof(NoPublicConstructorModule), "public parameterless constructor")]
        public void ModuleDiscovery_RejectsInvalidModuleTypes(Type type, string expectedMessage)
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                AnimatorCodeModuleDiscovery.CreateAll(new[] { type }));

            Assert.That(exception.Message, Does.Contain(expectedMessage));
        }

        [Test]
        public void ModuleDiscovery_RejectsEmptyId()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                AnimatorCodeModuleDiscovery.CreateAll(new[] { typeof(EmptyIdModule) }));

            Assert.That(exception.Message, Does.Contain("empty module Id"));
        }

        [Test]
        public void Plugin_FiltersInapplicableModules()
        {
            var avatarRoot = new GameObject("Test Avatar");
            var settings = avatarRoot.AddComponent<AnimatorCodePipelineSettings>();
            try
            {
                var modules = AnimatorCodePipelinePlugin.GetApplicableModules(
                    new AnimatorCodeModule[] { new InapplicableModule(), new ApplicableModule() },
                    avatarRoot,
                    settings);

                Assert.That(modules.Select(module => module.Id), Is.EqualTo(new[] { "test.applicable" }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(avatarRoot);
            }
        }
    }

    public sealed class FirstModule : AnimatorCodeModule
    {
        public override string Id => "test.first";
        public override int Order => -10;
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    public sealed class AlphabeticalModule : AnimatorCodeModule
    {
        public override string Id => "test.alphabetical";
        public override int Order => 0;
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    public sealed class LastModule : AnimatorCodeModule
    {
        public override string Id => "test.last";
        public override int Order => 0;
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    public sealed class DuplicateFirstModule : AnimatorCodeModule
    {
        public override string Id => "test.duplicate";
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    public sealed class DuplicateSecondModule : AnimatorCodeModule
    {
        public override string Id => "test.duplicate";
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    public abstract class AbstractModule : AnimatorCodeModule
    {
        public override string Id => "test.abstract";
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    public class OpenGenericModule<T> : AnimatorCodeModule
    {
        public override string Id => "test.generic";
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    public sealed class NotAModule { }

    public sealed class NoPublicConstructorModule : AnimatorCodeModule
    {
        private NoPublicConstructorModule() { }
        public override string Id => "test.private-constructor";
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    public sealed class EmptyIdModule : AnimatorCodeModule
    {
        public override string Id => " ";
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    public sealed class InapplicableModule : AnimatorCodeModule
    {
        public override string Id => "test.inapplicable";
        public override bool IsApplicable(GameObject avatarRoot, AnimatorCodePipelineSettings settings) => false;
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    public sealed class ApplicableModule : AnimatorCodeModule
    {
        public override string Id => "test.applicable";
        public override void Build(AnimatorCodeBuildContext context) { }
    }
}
