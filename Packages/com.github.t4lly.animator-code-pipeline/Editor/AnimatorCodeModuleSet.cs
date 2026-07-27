using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AnimatorCodePipeline
{
    [CreateAssetMenu(
        fileName = "AnimatorCodeModuleSet",
        menuName = "Animator Code Pipeline/Module Set")]
    public sealed class AnimatorCodeModuleSet : ScriptableObject
    {
        [Tooltip("Compiled AnimatorCodeModule scripts. Execution order is determined by Order, then Id, then full type name.")]
        [SerializeField]
        private List<MonoScript> modules = new List<MonoScript>();

        internal IReadOnlyList<MonoScript> Modules => modules;

        internal IReadOnlyList<AnimatorCodeModule> CreateModules()
        {
            return AnimatorCodeModuleDiscovery.CreateAll(modules);
        }
    }
}
