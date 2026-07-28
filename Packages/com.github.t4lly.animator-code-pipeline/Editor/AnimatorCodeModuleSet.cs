using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnimatorCodePipeline
{
    [CreateAssetMenu(
        fileName = "AnimatorCodeModuleSet",
        menuName = "Animator Code Pipeline/Module Set")]
    public sealed class AnimatorCodeModuleSet : ScriptableObject
    {
        [Tooltip("Module definitions and their user-adjustable configuration. Disabled definitions are skipped.")]
        [SerializeReference]
        private List<AnimatorCodeModule> modules = new List<AnimatorCodeModule>();

        internal IReadOnlyList<AnimatorCodeModule> CreateModules()
        {
            var selected = modules.Where(module => module != null && module.enabled).ToArray();
            foreach (var module in selected)
                module.ValidateDefinition();

            var duplicate = selected
                .GroupBy(module => module.Id, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException(
                    $"Animator Code Pipeline found duplicate module Id '{duplicate.Key}'.");

            return selected
                .OrderBy(module => module.Order)
                .ThenBy(module => module.Id, StringComparer.Ordinal)
                .ThenBy(module => module.GetType().FullName, StringComparer.Ordinal)
                .ToArray();
        }
    }
}
