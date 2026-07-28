using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace AnimatorCodePipeline
{
    [CustomPropertyDrawer(typeof(AnimatorCodeObjectReference), true)]
    public sealed class AnimatorCodeObjectReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var targetObject = property.FindPropertyRelative("targetObject");
            var avatarRelativePath = property.FindPropertyRelative("avatarRelativePath");
            var currentTarget = targetObject.objectReferenceValue as GameObject;
            if (currentTarget == null && !string.IsNullOrWhiteSpace(avatarRelativePath.stringValue))
                currentTarget = FindTargetByPath(avatarRelativePath.stringValue);

            var selected = EditorGUI.ObjectField(
                position,
                label,
                currentTarget,
                typeof(GameObject),
                true) as GameObject;

            if (selected == currentTarget)
                return;

            targetObject.objectReferenceValue = selected;
            var avatar = selected == null
                ? null
                : selected.GetComponentInParent<VRCAvatarDescriptor>(true);
            if (avatar == null)
            {
                avatarRelativePath.stringValue = string.Empty;
                return;
            }

            avatarRelativePath.stringValue = AnimatorCodeObjectReference.RelativePath(
                avatar.transform,
                selected.transform);
        }

        private static GameObject FindTargetByPath(string path)
        {
            foreach (var avatar in Resources.FindObjectsOfTypeAll<VRCAvatarDescriptor>())
            {
                if (!avatar.gameObject.scene.IsValid())
                    continue;

                var target = avatar.transform.Find(path);
                if (target != null)
                    return target.gameObject;
            }

            return null;
        }
    }
}
