using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

        internal int ValidateDefinitions()
        {
            var selected = GetEnabledDefinitions();
            ValidateSelectedDefinitions(selected);
            return selected.Length;
        }

        internal IReadOnlyList<AnimatorCodeModule> CreateModuleInstances()
        {
            var selected = GetEnabledDefinitions();
            ValidateSelectedDefinitions(selected);

            return selected
                .OrderBy(module => module.Order)
                .ThenBy(module => module.Id, StringComparer.Ordinal)
                .ThenBy(module => module.GetType().FullName, StringComparer.Ordinal)
                .Select(CloneDefinition)
                .ToArray();
        }

        private AnimatorCodeModule[] GetEnabledDefinitions()
        {
            return modules
                .Where(module => module != null && module.enabled)
                .ToArray();
        }

        private static void ValidateSelectedDefinitions(IReadOnlyList<AnimatorCodeModule> selected)
        {
            foreach (var module in selected)
                module.ValidateDefinition();

            var duplicate = selected
                .GroupBy(module => module.Id, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException(
                    $"Animator Code Pipeline found duplicate module Id '{duplicate.Key}'.");
        }

        private static AnimatorCodeModule CloneDefinition(AnimatorCodeModule source)
        {
            AnimatorCodeModule clone;
            try
            {
                clone = (AnimatorCodeModule)Activator.CreateInstance(source.GetType());
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Animator Code Pipeline module '{source.GetType().FullName}' must have a public parameterless constructor.",
                    exception);
            }

            EditorUtility.CopySerializedManagedFieldsOnly(source, clone);
            return clone;
        }
    }
}
