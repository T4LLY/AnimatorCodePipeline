using System;
using UnityEngine;

namespace AnimatorCodePipeline
{
    /// <summary>
    /// Stores an editor-friendly object shortcut and an avatar-relative fallback path.
    /// The path remains valid when the Module Set asset is used in another build context.
    /// </summary>
    [Serializable]
    public sealed class AnimatorCodeObjectReference
    {
        [SerializeField]
        private GameObject targetObject;

        [SerializeField, HideInInspector]
        private string avatarRelativePath;

        public GameObject Resolve(GameObject avatarRoot)
        {
            if (avatarRoot == null) throw new ArgumentNullException(nameof(avatarRoot));
            if (targetObject != null && targetObject.transform.IsChildOf(avatarRoot.transform))
                return targetObject;
            if (string.IsNullOrWhiteSpace(avatarRelativePath)) return null;
            return avatarRoot.transform.Find(avatarRelativePath)?.gameObject;
        }

        public void Set(GameObject avatarRoot, GameObject target)
        {
            if (avatarRoot == null) throw new ArgumentNullException(nameof(avatarRoot));
            if (target == null || !target.transform.IsChildOf(avatarRoot.transform))
                throw new ArgumentException("The target must be inside the avatar root.", nameof(target));
            targetObject = target;
            avatarRelativePath = RelativePath(avatarRoot.transform, target.transform);
        }

        internal static string RelativePath(Transform root, Transform target)
        {
            var parts = new System.Collections.Generic.Stack<string>();
            for (var current = target; current != root; current = current.parent)
            {
                if (current == null) throw new InvalidOperationException("The target is outside the avatar root.");
                parts.Push(current.name);
            }
            return string.Join("/", parts.ToArray());
        }
    }
}
