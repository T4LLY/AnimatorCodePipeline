using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace AnimatorCodePipeline
{
    [CustomEditor(typeof(AnimatorCodeModuleSet))]
    internal sealed class AnimatorCodeModuleSetEditor : Editor
    {
        private SerializedProperty _modules;

        private void OnEnable()
        {
            _modules = serializedObject.FindProperty("modules");
        }

        public override void OnInspectorGUI()
        {
            AnimatorCodeModuleSetInspectorUtility.DrawModules(
                serializedObject,
                _modules,
                (AnimatorCodeModuleSet)target);
        }
    }

    internal static class AnimatorCodeModuleSetInspectorUtility
    {
        internal static void DrawModules(SerializedObject serializedObject, SerializedProperty modules, AnimatorCodeModuleSet moduleSet)
        {
            serializedObject.Update();
            for (var index = 0; index < modules.arraySize; index++)
            {
                var element = modules.GetArrayElementAtIndex(index);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                var previousIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = previousIndentLevel + 1;
                element.isExpanded = EditorGUILayout.Foldout(
                    element.isExpanded,
                    element.managedReferenceValue == null
                        ? "Missing Module Definition"
                        : element.managedReferenceValue.GetType().Name,
                    true);
                EditorGUI.indentLevel = previousIndentLevel;
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    modules.DeleteArrayElementAtIndex(index);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                if (element.isExpanded)
                    DrawModuleProperties(element);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New Module..."))
                AnimatorCodeModuleGenerator.Create(serializedObject, modules, moduleSet);
            if (GUILayout.Button("Add Module Definition"))
            {
                var menu = new GenericMenu();
                var types = TypeCache.GetTypesDerivedFrom<AnimatorCodeModule>()
                    .Where(type => !type.IsAbstract && !type.ContainsGenericParameters)
                    .OrderBy(type => type.FullName, StringComparer.Ordinal)
                    .ToArray();
                foreach (var type in types)
                {
                    var capturedType = type;
                    menu.AddItem(new GUIContent(type.FullName), false, () => Add(capturedType, serializedObject, modules, moduleSet));
                }
                if (types.Length == 0)
                    menu.AddDisabledItem(new GUIContent("No module definitions found"));
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
            try
            {
                var count = moduleSet.CreateModules().Count;
                if (count == 0)
                    EditorGUILayout.HelpBox("This Module Set is empty. ACP will perform no work.", MessageType.Warning);
            }
            catch (Exception exception)
            {
                EditorGUILayout.HelpBox(exception.Message, MessageType.Error);
            }
        }

        private static void Add(
            Type type,
            SerializedObject serializedObject,
            SerializedProperty modules,
            AnimatorCodeModuleSet moduleSet)
        {
            serializedObject.Update();
            modules.arraySize++;
            modules.GetArrayElementAtIndex(modules.arraySize - 1).managedReferenceValue =
                Activator.CreateInstance(type);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(moduleSet);
        }

        private static void DrawModuleProperties(SerializedProperty element)
        {
            var property = element.Copy();
            var end = property.GetEndProperty();
            if (!property.NextVisible(true))
                return;

            EditorGUI.indentLevel++;
            do
            {
                if (SerializedProperty.EqualContents(property, end))
                    break;
                EditorGUILayout.PropertyField(property, true);
            }
            while (property.NextVisible(false));
            EditorGUI.indentLevel--;
        }
    }

    [InitializeOnLoad]
    internal static class AnimatorCodeModuleGenerator
    {
        private const string PendingModuleSetKey = "AnimatorCodePipeline.NewModule.ModuleSet";
        private const string PendingScriptKey = "AnimatorCodePipeline.NewModule.Script";
        private const string FolderKeyPrefix = "AnimatorCodePipeline.NewModule.Folder.";

        static AnimatorCodeModuleGenerator()
        {
            EditorApplication.delayCall += TryRegisterPendingModule;
        }

        internal static void Create(
            SerializedObject serializedObject,
            SerializedProperty modules,
            AnimatorCodeModuleSet moduleSet)
        {
            var folderKey = FolderKeyPrefix + Application.dataPath;
            var rememberedFolder = EditorPrefs.GetString(folderKey, "Assets");
            if (!AssetDatabase.IsValidFolder(rememberedFolder))
                rememberedFolder = "Assets";

            var path = EditorUtility.SaveFilePanelInProject(
                "Create Animator Code Module",
                "NewAnimatorCodeModule",
                "cs",
                "Choose where to create the new Animator Code module.",
                rememberedFolder);
            if (string.IsNullOrEmpty(path))
                return;

            var className = MakeClassName(Path.GetFileNameWithoutExtension(path));
            var folder = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "Assets";
            var scriptPath = folder + "/" + className + ".cs";
            if (File.Exists(ToAbsolutePath(scriptPath)) &&
                !EditorUtility.DisplayDialog(
                    "File Already Exists",
                    $"A module named '{className}' already exists. Overwrite it?",
                    "Overwrite",
                    "Cancel"))
                return;

            EnsureEditorAssemblyReference(folder);
            File.WriteAllText(ToAbsolutePath(scriptPath), LoadTemplate(className), new UTF8Encoding(false));
            EditorPrefs.SetString(folderKey, folder);
            SessionState.SetString(PendingModuleSetKey, AssetDatabase.GetAssetPath(moduleSet));
            SessionState.SetString(PendingScriptKey, scriptPath);
            AssetDatabase.Refresh();
            EditorApplication.delayCall += TryRegisterPendingModule;
        }

        private static void TryRegisterPendingModule()
        {
            var moduleSetPath = SessionState.GetString(PendingModuleSetKey, string.Empty);
            var scriptPath = SessionState.GetString(PendingScriptKey, string.Empty);
            if (string.IsNullOrEmpty(moduleSetPath) || string.IsNullOrEmpty(scriptPath))
                return;

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            var type = script == null ? null : script.GetClass();
            if (type == null)
            {
                EditorApplication.delayCall += TryRegisterPendingModule;
                return;
            }

            if (!typeof(AnimatorCodeModule).IsAssignableFrom(type) || type.IsAbstract)
            {
                ClearPending();
                return;
            }

            var moduleSet = AssetDatabase.LoadAssetAtPath<AnimatorCodeModuleSet>(moduleSetPath);
            if (moduleSet == null)
            {
                ClearPending();
                return;
            }

            var serializedObject = new SerializedObject(moduleSet);
            var modules = serializedObject.FindProperty("modules");
            for (var index = 0; index < modules.arraySize; index++)
            {
                if (modules.GetArrayElementAtIndex(index).managedReferenceValue?.GetType() == type)
                {
                    ClearPending();
                    return;
                }
            }

            modules.arraySize++;
            modules.GetArrayElementAtIndex(modules.arraySize - 1).managedReferenceValue =
                Activator.CreateInstance(type);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(moduleSet);
            AssetDatabase.SaveAssets();
            ClearPending();
            Selection.activeObject = moduleSet;
        }

        private static void ClearPending()
        {
            SessionState.EraseString(PendingModuleSetKey);
            SessionState.EraseString(PendingScriptKey);
        }

        private static string ToAbsolutePath(string projectPath)
        {
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, projectPath);
        }

        private static void EnsureEditorAssemblyReference(string folder)
        {
            var directory = new DirectoryInfo(ToAbsolutePath(folder));
            var assetsDirectory = new DirectoryInfo(Application.dataPath);
            while (directory != null && directory.FullName.StartsWith(assetsDirectory.FullName, StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(Path.Combine(directory.FullName, "AnimatorCodePipeline.Editor.asmref")))
                    return;
                if (directory.GetFiles("*.asmdef").Length != 0)
                    return;
                if (directory.FullName.Equals(assetsDirectory.FullName, StringComparison.OrdinalIgnoreCase))
                    break;
                directory = directory.Parent;
            }

            var asmrefPath = folder + "/AnimatorCodePipeline.Editor.asmref";
            if (!File.Exists(ToAbsolutePath(asmrefPath)))
            {
                File.Copy(GetTemplatePath("AnimatorCodePipeline.Editor.asmref"), ToAbsolutePath(asmrefPath));
            }
        }

        private static string MakeClassName(string value)
        {
            var builder = new StringBuilder();
            foreach (var character in value)
            {
                if (char.IsLetterOrDigit(character) || character == '_')
                    builder.Append(character);
                else
                    builder.Append('_');
            }

            if (builder.Length == 0)
                builder.Append("NewAnimatorCodeModule");
            if (char.IsDigit(builder[0]))
                builder.Insert(0, '_');
            return builder.ToString();
        }

        private static string LoadTemplate(string className)
        {
            var template = File.ReadAllText(GetTemplatePath("NewModule.cs.txt"));
            return template
                .Replace("{{CLASS_NAME}}", className)
                .Replace("{{MODULE_ID}}", "project." + ToKebabCase(className))
                .Replace("{{LAYER_NAME}}", className);
        }

        private static string GetTemplatePath(string fileName)
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(
                "Packages/com.github.t4lly.animator-code-pipeline/Editor/AnimatorCodeModuleSetEditor.cs");
            if (package == null || string.IsNullOrEmpty(package.resolvedPath))
                throw new InvalidOperationException("Animator Code Pipeline package path could not be resolved.");

            return Path.Combine(package.resolvedPath, "Templates~", "ProjectModules", fileName);
        }

        private static string ToKebabCase(string value)
        {
            return string.Concat(value.Select((character, index) =>
                index > 0 && char.IsUpper(character) ? "-" + char.ToLowerInvariant(character) : character.ToString()))
                .ToLowerInvariant();
        }
    }
}
