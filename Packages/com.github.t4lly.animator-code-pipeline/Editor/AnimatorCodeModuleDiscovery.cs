using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AnimatorCodePipeline
{
    internal static class AnimatorCodeModuleDiscovery
    {
        internal static IReadOnlyList<AnimatorCodeModule> CreateAll(IEnumerable<MonoScript> scripts)
        {
            if (scripts == null) throw new ArgumentNullException(nameof(scripts));

            var types = new List<Type>();
            foreach (var script in scripts)
            {
                if (script == null)
                    throw new InvalidOperationException(
                        "Animator Code Pipeline Module Set contains a missing script reference.");

                var type = script.GetClass();
                if (type == null || !typeof(AnimatorCodeModule).IsAssignableFrom(type))
                {
                    throw new InvalidOperationException(
                        $"Animator Code Pipeline Module Set script '{script.name}' does not define an AnimatorCodeModule.");
                }

                types.Add(type);
            }

            return CreateAll(types);
        }

        internal static IReadOnlyList<AnimatorCodeModule> CreateAll(IEnumerable<Type> types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));

            var modules = new List<AnimatorCodeModule>();

            foreach (var type in types)
            {
                ValidateModuleType(type);

                AnimatorCodeModule module;
                try
                {
                    module = (AnimatorCodeModule)Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Animator Code Pipeline could not instantiate module '{type.FullName}'. " +
                        "Modules must have a public parameterless constructor.", ex);
                }

                module.ValidateDefinition();
                modules.Add(module);
            }

            var duplicate = modules
                .GroupBy(m => m.Id, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
            {
                throw new InvalidOperationException(
                    $"Animator Code Pipeline found duplicate module Id '{duplicate.Key}': " +
                    string.Join(", ", duplicate.Select(m => m.GetType().FullName)));
            }

            return modules
                .OrderBy(m => m.Order)
                .ThenBy(m => m.Id, StringComparer.Ordinal)
                .ThenBy(m => m.GetType().FullName, StringComparer.Ordinal)
                .ToArray();
        }

        private static void ValidateModuleType(Type type)
        {
            if (type == null)
                throw new InvalidOperationException(
                    "Animator Code Pipeline Module Set contains a module type that could not be resolved.");

            if (!typeof(AnimatorCodeModule).IsAssignableFrom(type))
                throw new InvalidOperationException(
                    $"Animator Code Pipeline type '{type.FullName}' does not derive from AnimatorCodeModule.");

            if (type.IsAbstract)
                throw new InvalidOperationException(
                    $"Animator Code Pipeline module '{type.FullName}' is abstract and cannot be instantiated.");

            if (type.ContainsGenericParameters)
                throw new InvalidOperationException(
                    $"Animator Code Pipeline module '{type.FullName}' contains unbound generic parameters and cannot be instantiated.");
        }
    }
}
