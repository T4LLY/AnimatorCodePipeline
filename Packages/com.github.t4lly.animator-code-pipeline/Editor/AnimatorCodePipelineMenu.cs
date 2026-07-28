using System.IO;
using UnityEditor;
using UnityEngine;

namespace AnimatorCodePipeline
{
    internal static class AnimatorCodePipelineMenu
    {
        private const string ModuleFolder = "Assets/AnimatorCodePipeline/Editor";
        private const string AsmRefPath = ModuleFolder + "/AnimatorCodePipeline.Editor.asmref";

        [MenuItem("Tools/Animator Code Pipeline/Add Settings to Selected GameObject")]
        private static void AddSettings()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogError("Select a GameObject first.");
                return;
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

        [MenuItem("Tools/Animator Code Pipeline/Log Configured Modules")]
        private static void LogModules()
        {
            var settings = Selection.activeObject as AnimatorCodePipelineSettings;
            if (settings == null && Selection.activeGameObject != null)
                settings = Selection.activeGameObject.GetComponent<AnimatorCodePipelineSettings>();

            if (settings == null)
            {
                Debug.LogWarning("Select a GameObject or component with Animator Code Pipeline Settings first.");
                return;
            }

            var modules = AnimatorCodeModuleCollection.CreateModuleInstances(settings);
            if (modules.Count == 0)
            {
                Debug.Log("Animator Code Pipeline: the selected Settings component contains no enabled modules.");
                return;
            }

            foreach (var module in modules)
            {
                Debug.Log($"Animator Code Pipeline module: order={module.Order}, id={module.Id}, type={module.GetType().FullName}");
            }
        }
    }
}
