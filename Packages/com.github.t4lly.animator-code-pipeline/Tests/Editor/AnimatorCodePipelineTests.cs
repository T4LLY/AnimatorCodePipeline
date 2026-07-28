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
        public void ModuleSet_OrdersEnabledDefinitionsAndSkipsDisabled()
        {
            var set = ScriptableObject.CreateInstance<AnimatorCodeModuleSet>();
            try
            {
                var serialized = new SerializedObject(set);
                var modules = serialized.FindProperty("modules");
                modules.arraySize = 3;
                AddDefinition(modules.GetArrayElementAtIndex(0), typeof(TestLastModule), true);
                AddDefinition(modules.GetArrayElementAtIndex(1), typeof(TestDisabledModule), false);
                AddDefinition(modules.GetArrayElementAtIndex(2), typeof(TestFirstModule), true);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                Assert.That(set.CreateModules().Select(module => module.Id),
                    Is.EqualTo(new[] { "test.first", "test.last" }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(set);
            }
        }

        [Test]
        public void Settings_DoesNotCreateMergeAnimatorDependency()
        {
            var go = new GameObject("ACP Settings");
            try
            {
                var settings = go.AddComponent<AnimatorCodePipelineSettings>();
                Assert.That(go.GetComponent<ModularAvatarMergeAnimator>(), Is.Null);
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
        public void GeneratedLayerCache_RejectsNormalizedSuffixCollision()
        {
            var cache = new GeneratedLayerCache<object>();
            cache.GetOrCreate("Face.Blink", _ => new object());
            var exception = Assert.Throws<InvalidOperationException>(() =>
                cache.GetOrCreate("Face_Blink", _ => new object()));
            Assert.That(exception.Message, Does.Contain("Face.Blink"));
            Assert.That(exception.Message, Does.Contain("Face_Blink"));
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
    public sealed class TestDisabledModule : AnimatorCodeModule
    {
        public override string Id => "test.disabled";
        public override void Build(AnimatorCodeBuildContext context) { }
    }
}
