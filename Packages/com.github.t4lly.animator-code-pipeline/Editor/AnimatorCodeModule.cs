using System;
using UnityEngine;

namespace AnimatorCodePipeline
{
    /// <summary>
    /// Base type for project-owned Animator As Code modules.
    /// Definitions are stored directly on AnimatorCodePipelineSettings and own their build logic and configuration.
    /// </summary>
    [Serializable]
    public abstract class AnimatorCodeModule : IAnimatorCodeModuleDefinition
    {
        /// <summary>Whether this definition participates in the current Settings configuration.</summary>
        public bool enabled = true;

        /// <summary>A stable identifier used for deterministic ordering and duplicate detection.</summary>
        public abstract string Id { get; }

        /// <summary>Lower values build first. Ties are resolved by Id and then full type name.</summary>
        public virtual int Order => 0;

        /// <summary>
        /// Return false when this module is not intended for the current avatar/settings.
        /// Keep this side-effect free: do not create controllers, clips, GameObjects, or MA components here.
        /// </summary>
        public virtual bool IsApplicable(GameObject avatarRoot, AnimatorCodePipelineSettings settings) => true;

        /// <summary>Generate Animator As Code content. Do not edit the avatar's existing controllers directly.</summary>
        public abstract void Build(AnimatorCodeBuildContext context);

        internal void ValidateDefinition()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new InvalidOperationException($"{GetType().FullName} returned an empty module Id.");
            }
        }
    }
}
