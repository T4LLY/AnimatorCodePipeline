using System.Linq;
using System.IO;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace AnimatorCodePipeline
{
    [CustomEditor(typeof(AnimatorCodePipelineSettings))]
    internal sealed class AnimatorCodePipelineSettingsEditor : Editor
    {
        private SerializedProperty _moduleSet;
        private AnimatorCodeModuleSet _sharedModuleSet;
        private Transform _sharedScope;
        private int _sharedCount;
        private bool _sharedCountDirty = true;

        private void OnEnable()
        {
            _moduleSet = serializedObject.FindProperty("moduleSet");
            EditorApplication.hierarchyChanged += InvalidateSharedCount;
            Undo.undoRedoPerformed += InvalidateSharedCount;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= InvalidateSharedCount;
            Undo.undoRedoPerformed -= InvalidateSharedCount;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var currentModuleSet = _moduleSet.objectReferenceValue as AnimatorCodeModuleSet;
            EditorGUI.BeginChangeCheck();
            var nextModuleSet = (AnimatorCodeModuleSet)EditorGUILayout.ObjectField(
                new GUIContent("Module Set", "Animator Code Module Set asset evaluated for this feature."),
                currentModuleSet,
                typeof(AnimatorCodeModuleSet),
                false);
            if (EditorGUI.EndChangeCheck())
                _moduleSet.objectReferenceValue = nextModuleSet;

            serializedObject.ApplyModifiedProperties();

            var settings = (AnimatorCodePipelineSettings)target;
            var moduleSet = settings.moduleSet as AnimatorCodeModuleSet;
            if (moduleSet == null)
            {
                EditorGUILayout.HelpBox("Assign an Animator Code Module Set.", MessageType.Error);
                if (GUILayout.Button("Create Module Set"))
                    CreateModuleSet();
            }
            else
            {
                DrawSharedCount(settings, moduleSet);
                var moduleSetObject = new SerializedObject(moduleSet);
                var modules = moduleSetObject.FindProperty("modules");
                AnimatorCodeModuleSetInspectorUtility.DrawModules(moduleSetObject, modules, moduleSet);
            }

            var merge = settings.GetComponent<ModularAvatarMergeAnimator>();
            if (merge == null)
            {
                EditorGUILayout.HelpBox("Modular Avatar Merge Animator is required on this GameObject.", MessageType.Error);
                return;
            }

            if (merge.animator == null)
            {
                EditorGUILayout.HelpBox("Assign an Animator Controller on the Merge Animator.", MessageType.Error);
            }
            else if (!(merge.animator is AnimatorController))
            {
                EditorGUILayout.HelpBox("Merge Animator must reference a regular AnimatorController.", MessageType.Error);
            }

        }

        private void CreateModuleSet()
        {
            var defaultName = SanitizeFileName(target.name) + " ModuleSet";
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Animator Code Module Set",
                defaultName,
                "asset",
                "Choose where to create the Animator Code Module Set.",
                "Assets/AnimatorCodePipeline");
            if (string.IsNullOrEmpty(path))
                return;

            var moduleSet = CreateInstance<AnimatorCodeModuleSet>();
            AssetDatabase.CreateAsset(moduleSet, path);
            AssetDatabase.SaveAssets();

            serializedObject.Update();
            _moduleSet.objectReferenceValue = moduleSet;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            EditorGUIUtility.PingObject(moduleSet);
        }

        private static string SanitizeFileName(string value)
        {
            foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
                value = value.Replace(invalidCharacter.ToString(), string.Empty);

            return string.IsNullOrWhiteSpace(value) ? "AnimatorCodePipeline" : value;
        }

        private void DrawSharedCount(AnimatorCodePipelineSettings settings, AnimatorCodeModuleSet moduleSet)
        {
            var avatar = settings.GetComponentInParent<VRCAvatarDescriptor>(true);
            var scope = avatar == null ? settings.transform.root : avatar.transform;
            if (_sharedCountDirty || _sharedModuleSet != moduleSet || _sharedScope != scope)
            {
                _sharedCount = scope
                    .GetComponentsInChildren<AnimatorCodePipelineSettings>(true)
                    .Count(candidate => candidate.moduleSet == moduleSet);
                _sharedModuleSet = moduleSet;
                _sharedScope = scope;
                _sharedCountDirty = false;
            }

            if (_sharedCount > 1)
            {
                EditorGUILayout.LabelField(
                    $"Shared in avatar hierarchy: {_sharedCount}",
                    EditorStyles.miniLabel);
            }
        }

        private void InvalidateSharedCount()
        {
            _sharedCountDirty = true;
        }
    }
}
