using System.IO;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace AnimatorCodePipeline
{
    internal static class AnimatorCodePipelineMenu
    {
        private const string ModuleFolder = "Assets/AnimatorCodePipeline/Editor";
        private const string AsmRefPath = ModuleFolder + "/AnimatorCodePipeline.Editor.asmref";

        [MenuItem("Tools/Animator Code Pipeline/Add Settings to Selected Avatar")]
        private static void AddSettings()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogError("Select the avatar root GameObject first.");
                return;
            }

            if (selected.GetComponent<VRCAvatarDescriptor>() == null)
            {
                Debug.LogWarning("The selected GameObject does not have a VRCAvatarDescriptor. " +
                                 "Animator Code Pipeline should normally be placed on the avatar root.");
            }

            var existing = selected.GetComponent<AnimatorCodePipelineSettings>();
            if (existing != null)
            {
                Selection.activeObject = existing;
                return;
            }

            var settings = Undo.AddComponent<AnimatorCodePipelineSettings>(selected);
            Selection.activeObject = settings;
        }

        [MenuItem("Tools/Animator Code Pipeline/Create Git-managed Module Folder")]
        private static void CreateModuleFolder()
        {
            Directory.CreateDirectory(ModuleFolder);
            if (!File.Exists(AsmRefPath))
            {
                File.WriteAllText(AsmRefPath, "{\n  \"reference\": \"AnimatorCodePipeline.Editor\"\n}\n");
            }

            AssetDatabase.Refresh();
            var asset = AssetDatabase.LoadAssetAtPath<Object>(AsmRefPath);
            if (asset != null) Selection.activeObject = asset;
            Debug.Log($"Animator Code Pipeline module folder is ready: {ModuleFolder}");
        }

        [MenuItem("Tools/Animator Code Pipeline/Log Discovered Modules")]
        private static void LogModules()
        {
            var moduleSet = Selection.activeObject as AnimatorCodeModuleSet;
            if (moduleSet == null)
            {
                Debug.LogWarning("Select an Animator Code Module Set asset first.");
                return;
            }

            var modules = moduleSet.CreateModules();
            if (modules.Count == 0)
            {
                Debug.Log("Animator Code Pipeline: the selected Module Set contains no modules.");
                return;
            }

            foreach (var module in modules)
            {
                Debug.Log($"Animator Code Pipeline module: order={module.Order}, id={module.Id}, type={module.GetType().FullName}");
            }
        }
    }
}
