using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AnimatorCodePipeline
{
    [CustomEditor(typeof(AnimatorCodePipelineSettings))]
    internal sealed class AnimatorCodePipelineSettingsEditor : Editor
    {
        private SerializedProperty _moduleSet;
        private SerializedProperty _sourceController;
        private SerializedProperty _legacyTargetAnimator;

        private void OnEnable()
        {
            _moduleSet = serializedObject.FindProperty("moduleSet");
            _sourceController = serializedObject.FindProperty("sourceController");
            _legacyTargetAnimator = serializedObject.FindProperty("targetAnimator");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_moduleSet);

            var current = _sourceController.objectReferenceValue as AnimatorController;
            var next = (AnimatorController)EditorGUILayout.ObjectField(
                new GUIContent(
                    "Source Controller",
                    "ACP clones this controller before AAC generation. Use the same controller in the MA Merge Animator."),
                current,
                typeof(AnimatorController),
                false);
            _sourceController.objectReferenceValue = next;
            serializedObject.ApplyModifiedProperties();

            DrawValidation((AnimatorCodePipelineSettings)target);
        }

        private void DrawValidation(AnimatorCodePipelineSettings settings)
        {
            if (settings.SourceController == null)
            {
                EditorGUILayout.HelpBox("Source Controller is not assigned.", MessageType.Error);
                DrawMigration(settings);
                return;
            }

            if (!(settings.SourceController is AnimatorController))
            {
                EditorGUILayout.HelpBox("Source Controller must be a regular AnimatorController.", MessageType.Error);
                return;
            }

            var mergeAnimators = settings.GetComponents<ModularAvatarMergeAnimator>();
            if (mergeAnimators.Length == 0)
            {
                EditorGUILayout.HelpBox("Modular Avatar Merge Animator is missing on this GameObject.", MessageType.Error);
                return;
            }

            if (mergeAnimators.Length > 1)
            {
                EditorGUILayout.HelpBox("Multiple Merge Animators are present; ACP cannot choose one.", MessageType.Error);
                return;
            }

            if (mergeAnimators[0].animator == null)
            {
                EditorGUILayout.HelpBox("Merge Animator controller is not assigned.", MessageType.Error);
                return;
            }

            if (mergeAnimators[0].animator != settings.SourceController)
            {
                EditorGUILayout.HelpBox("Merge Animator references a different controller.", MessageType.Error);
                if (GUILayout.Button("Use Source Controller in Merge Animator"))
                {
                    Undo.RecordObject(mergeAnimators[0], "Assign ACP Source Controller to Merge Animator");
                    mergeAnimators[0].animator = settings.SourceController;
                    EditorUtility.SetDirty(mergeAnimators[0]);
                }
                return;
            }

            EditorGUILayout.HelpBox("Merge Animator uses the same Source Controller.", MessageType.Info);
        }

        private void DrawMigration(AnimatorCodePipelineSettings settings)
        {
            var legacy = _legacyTargetAnimator.objectReferenceValue as Animator;
            if (legacy == null || legacy.runtimeAnimatorController == null) return;

            var merge = settings.GetComponent<ModularAvatarMergeAnimator>();
            var legacyController = legacy.runtimeAnimatorController as AnimatorController;
            if (legacyController == null || merge == null || merge.animator != legacyController)
            {
                EditorGUILayout.HelpBox("Legacy Target Animator configuration is ambiguous. Assign Source Controller manually.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Migrate Target Animator to Source Controller"))
            {
                Undo.RecordObject(settings, "Migrate ACP Source Controller");
                _sourceController.objectReferenceValue = legacyController;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
