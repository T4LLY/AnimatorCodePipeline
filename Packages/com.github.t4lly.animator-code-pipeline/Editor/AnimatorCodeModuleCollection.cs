using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AnimatorCodePipeline
{
    internal static class AnimatorCodeModuleCollection
    {
        internal static int ValidateDefinitions(AnimatorCodePipelineSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var selected = GetEnabledDefinitions(settings.Modules);
            ValidateSelectedDefinitions(selected);
            return selected.Length;
        }

        internal static IReadOnlyList<AnimatorCodeModule> CreateModuleInstances(AnimatorCodePipelineSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var selected = GetEnabledDefinitions(settings.Modules);
            ValidateSelectedDefinitions(selected);

            return selected
                .OrderBy(module => module.Order)
                .ThenBy(module => module.Id, StringComparer.Ordinal)
                .ThenBy(module => module.GetType().FullName, StringComparer.Ordinal)
                .Select(CloneDefinition)
                .ToArray();
        }

        private static AnimatorCodeModule[] GetEnabledDefinitions(
            IReadOnlyList<IAnimatorCodeModuleDefinition> definitions)
        {
            if (definitions == null)
                return Array.Empty<AnimatorCodeModule>();

            var modules = new List<AnimatorCodeModule>(definitions.Count);
            foreach (var definition in definitions)
            {
                if (definition == null)
                    continue;

                if (!(definition is AnimatorCodeModule module))
                {
                    throw new InvalidOperationException(
                        $"Animator Code Pipeline encountered unsupported module definition type '{definition.GetType().FullName}'.");
                }

                if (module.enabled)
                    modules.Add(module);
            }

            return modules.ToArray();
        }

        private static void ValidateSelectedDefinitions(IReadOnlyList<AnimatorCodeModule> selected)
        {
            foreach (var module in selected)
                module.ValidateDefinition();

            var duplicate = selected
                .GroupBy(module => module.Id, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
            {
                throw new InvalidOperationException(
                    $"Animator Code Pipeline found duplicate module Id '{duplicate.Key}' in one Settings component.");
            }
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
