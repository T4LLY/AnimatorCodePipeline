using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using nadena.dev.modular_avatar.core;

namespace AnimatorCodePipeline.Tests
{
    public sealed class AnimatorCodePipelineTests
    {
        [Test]
        public void Settings_OrdersEnabledDefinitionsAndSkipsDisabled()
        {
            var go = new GameObject("ACP Settings");
            try
            {
                var settings = go.AddComponent<AnimatorCodePipelineSettings>();
                var serialized = new SerializedObject(settings);
                var modules = serialized.FindProperty("modules");
                modules.arraySize = 3;
                AddDefinition(modules.GetArrayElementAtIndex(0), typeof(TestLastModule), true);
                AddDefinition(modules.GetArrayElementAtIndex(1), typeof(TestDisabledModule), false);
                AddDefinition(modules.GetArrayElementAtIndex(2), typeof(TestFirstModule), true);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                Assert.That(AnimatorCodeModuleCollection.CreateModuleInstances(settings).Select(module => module.Id),
                    Is.EqualTo(new[] { "test.first", "test.last" }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Settings_RequiresMergeAnimator()
        {
            var go = new GameObject("ACP Settings");
            try
            {
                var settings = go.AddComponent<AnimatorCodePipelineSettings>();
                Assert.That(go.GetComponent<ModularAvatarMergeAnimator>(), Is.Not.Null);
                Assert.That(settings.enabled, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void WorkingController_ClonesMergeAnimatorController()
        {
            var host = new GameObject("ACP Host");
            var path = $"Assets/AnimatorCodePipelineTest-{Guid.NewGuid():N}.controller";
            try
            {
                var source = AnimatorController.CreateAnimatorControllerAtPath(path);
                var settings = host.AddComponent<AnimatorCodePipelineSettings>();
                host.GetComponent<ModularAvatarMergeAnimator>().animator = source;

                var working = AnimatorCodePipelinePlugin.CreateWorkingController(settings);
                try
                {
                    Assert.That(working, Is.Not.SameAs(source));
                    working.AddLayer("Generated");
                    Assert.That(source.layers.Select(layer => layer.name), Does.Not.Contain("Generated"));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(working);
                }
            }
            finally
            {
                AssetDatabase.DeleteAsset(path);
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void GeneratedLayerCache_SharesExactSuffix()
        {
            var cache = new GeneratedLayerCache<object>();
            var first = cache.GetOrCreate("Face.Blink", _ => new object());
            var second = cache.GetOrCreate("Face.Blink", _ => new object());

            Assert.That(second, Is.SameAs(first));
            Assert.That(cache.Count, Is.EqualTo(1));
        }

        [Test]
        public void GeneratedLayerCache_DotAndUnderscoreSuffixesRemainDistinct()
        {
            var cache = new GeneratedLayerCache<object>();
            var dotted = cache.GetOrCreate("Face.Blink", _ => new object());
            var underscored = cache.GetOrCreate("Face_Blink", _ => new object());

            Assert.That(underscored, Is.Not.SameAs(dotted));
            Assert.That(cache.Count, Is.EqualTo(2));
        }

        [Test]
        public void ObjectReference_StoresAvatarRelativePathAndResolvesPerAvatar()
        {
            var avatarA = new GameObject("Avatar A");
            var avatarB = new GameObject("Avatar B");
            try
            {
                var targetA = new GameObject("Target");
                targetA.transform.SetParent(avatarA.transform, false);
                var targetB = new GameObject("Target");
                targetB.transform.SetParent(avatarB.transform, false);

                var reference = new AnimatorCodeObjectReference();
                reference.Set(avatarA, targetA);

                Assert.That(reference.AvatarRelativePath, Is.EqualTo("Target"));
                Assert.That(reference.Resolve(avatarA), Is.SameAs(targetA));
                Assert.That(reference.Resolve(avatarB), Is.SameAs(targetB));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(avatarA);
                UnityEngine.Object.DestroyImmediate(avatarB);
            }
        }

        [Test]
        public void ObjectReference_CanResolveAvatarRoot()
        {
            var avatar = new GameObject("Avatar");
            try
            {
                var reference = new AnimatorCodeObjectReference();
                reference.Set(avatar, avatar);

                Assert.That(reference.IsConfigured, Is.True);
                Assert.That(reference.AvatarRelativePath, Is.EqualTo(string.Empty));
                Assert.That(reference.Resolve(avatar), Is.SameAs(avatar));

                reference.Set(avatar, null);
                Assert.That(reference.IsConfigured, Is.False);
                Assert.That(reference.Resolve(avatar), Is.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(avatar);
            }
        }

        [Test]
        public void Settings_CreatesIndependentModuleInstancesPerCall()
        {
            var go = new GameObject("ACP Settings");
            try
            {
                var settings = go.AddComponent<AnimatorCodePipelineSettings>();
                var serialized = new SerializedObject(settings);
                var modules = serialized.FindProperty("modules");
                modules.arraySize = 1;
                modules.GetArrayElementAtIndex(0).managedReferenceValue = new TestConfiguredModule
                {
                    configuredValue = 42
                };
                serialized.ApplyModifiedPropertiesWithoutUndo();

                var first = (TestConfiguredModule)AnimatorCodeModuleCollection.CreateModuleInstances(settings).Single();
                var second = (TestConfiguredModule)AnimatorCodeModuleCollection.CreateModuleInstances(settings).Single();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(first.configuredValue, Is.EqualTo(42));
                Assert.That(second.configuredValue, Is.EqualTo(42));

                first.transientValue = 99;
                Assert.That(second.transientValue, Is.EqualTo(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static void AddDefinition(SerializedProperty property, Type type, bool enabled)
        {
            property.managedReferenceValue = System.Activator.CreateInstance(type);
            property.FindPropertyRelative("enabled").boolValue = enabled;
        }
    }

    [Serializable]
    public sealed class TestFirstModule : AnimatorCodeModule
    {
        public override string Id => "test.first";
        public override int Order => -1;
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    [Serializable]
    public sealed class TestLastModule : AnimatorCodeModule
    {
        public override string Id => "test.last";
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    [Serializable]
    public sealed class TestConfiguredModule : AnimatorCodeModule
    {
        public int configuredValue;
        [NonSerialized] public int transientValue;

        public override string Id => "test.configured";
        public override void Build(AnimatorCodeBuildContext context) { }
    }

    [Serializable]
    public sealed class TestDisabledModule : AnimatorCodeModule
    {
        public override string Id => "test.disabled";
        public override void Build(AnimatorCodeBuildContext context) { }
    }
}
