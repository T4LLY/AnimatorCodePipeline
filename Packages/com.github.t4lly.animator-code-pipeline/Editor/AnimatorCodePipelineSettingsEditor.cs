using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEditor.Animations;

namespace AnimatorCodePipeline
{
    [CustomEditor(typeof(AnimatorCodePipelineSettings))]
    internal sealed class AnimatorCodePipelineSettingsEditor : Editor
    {
        private SerializedProperty _modules;

        private void OnEnable()
        {
            _modules = serializedObject.FindProperty("modules");
        }

        public override void OnInspectorGUI()
        {
            var settings = (AnimatorCodePipelineSettings)target;
            AnimatorCodeModuleInspectorUtility.DrawModules(serializedObject, _modules, settings);

            var merge = settings.GetComponent<ModularAvatarMergeAnimator>();
            if (merge == null)
            {
                EditorGUILayout.HelpBox(
                    "Modular Avatar Merge Animator is required on this GameObject.",
                    MessageType.Error);
                return;
            }

            if (merge.animator == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign an Animator Controller on the Merge Animator.",
                    MessageType.Error);
            }
            else if (!(merge.animator is AnimatorController))
            {
                EditorGUILayout.HelpBox(
                    "Merge Animator must reference a regular AnimatorController.",
                    MessageType.Error);
            }
        }
    }
}
