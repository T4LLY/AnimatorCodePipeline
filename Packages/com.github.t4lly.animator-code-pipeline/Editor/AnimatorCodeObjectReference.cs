using System;
using UnityEngine;

namespace AnimatorCodePipeline
{
    /// <summary>
    /// Stores an avatar-relative path for a user-configurable target.
    /// Scene object references are intentionally not persisted because Module Sets are project assets.
    /// </summary>
    [Serializable]
    public sealed class AnimatorCodeObjectReference
    {
        [SerializeField]
        private bool configured;

        [SerializeField]
        private string avatarRelativePath;

        public bool IsConfigured => configured;
        public string AvatarRelativePath => configured ? avatarRelativePath : null;

        public GameObject Resolve(GameObject avatarRoot)
        {
            if (avatarRoot == null) throw new ArgumentNullException(nameof(avatarRoot));
            if (!configured) return null;
            if (string.IsNullOrEmpty(avatarRelativePath)) return avatarRoot;
            return avatarRoot.transform.Find(avatarRelativePath)?.gameObject;
        }

        public void Set(GameObject avatarRoot, GameObject target)
        {
            if (avatarRoot == null) throw new ArgumentNullException(nameof(avatarRoot));
            if (target == null)
            {
                configured = false;
                avatarRelativePath = string.Empty;
                return;
            }
            if (!target.transform.IsChildOf(avatarRoot.transform))
                throw new ArgumentException("The target must be inside the avatar root.", nameof(target));

            configured = true;
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
