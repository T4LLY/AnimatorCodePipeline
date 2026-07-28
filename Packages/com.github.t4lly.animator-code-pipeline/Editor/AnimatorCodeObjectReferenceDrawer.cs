using System.Collections.Generic;
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
            var configured = property.FindPropertyRelative("configured");
            var avatarRelativePath = property.FindPropertyRelative("avatarRelativePath");
            var currentTarget = configured.boolValue
                ? FindUniqueTargetByPath(avatarRelativePath.stringValue)
                : null;

            var selected = EditorGUI.ObjectField(
                position,
                label,
                currentTarget,
                typeof(GameObject),
                true) as GameObject;

            if (selected == currentTarget)
                return;

            if (selected == null)
            {
                configured.boolValue = false;
                avatarRelativePath.stringValue = string.Empty;
                return;
            }

            var avatar = selected.GetComponentInParent<VRCAvatarDescriptor>(true);
            if (avatar == null)
            {
                configured.boolValue = false;
                avatarRelativePath.stringValue = string.Empty;
                return;
            }

            configured.boolValue = true;
            avatarRelativePath.stringValue = AnimatorCodeObjectReference.RelativePath(
                avatar.transform,
                selected.transform);
        }

        private static GameObject FindUniqueTargetByPath(string path)
        {
            var matches = new List<GameObject>();
            foreach (var avatar in Resources.FindObjectsOfTypeAll<VRCAvatarDescriptor>())
            {
                if (!avatar.gameObject.scene.IsValid())
                    continue;

                var target = string.IsNullOrEmpty(path)
                    ? avatar.gameObject
                    : avatar.transform.Find(path)?.gameObject;
                if (target != null)
                    matches.Add(target);
            }

            return matches.Count == 1 ? matches[0] : null;
        }
    }
}
