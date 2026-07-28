using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AnimatorCodePipeline
{
    internal static class AnimatorCodeModuleInspectorUtility
    {
        internal static void DrawModules(
            SerializedObject serializedObject,
            SerializedProperty modules,
            AnimatorCodePipelineSettings settings)
        {
            if (serializedObject == null) throw new ArgumentNullException(nameof(serializedObject));
            if (modules == null) throw new ArgumentNullException(nameof(modules));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            serializedObject.Update();

            EditorGUILayout.LabelField("Modules", EditorStyles.boldLabel);
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
                AnimatorCodeModuleGenerator.Create(settings);
            if (GUILayout.Button("Add Module"))
                ShowAddMenu(serializedObject, modules, settings);
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
            try
            {
                var count = AnimatorCodeModuleCollection.ValidateDefinitions(settings);
                if (count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "This Settings component has no enabled modules. ACP will perform no work.",
                        MessageType.Warning);
                }
            }
            catch (Exception exception)
            {
                EditorGUILayout.HelpBox(exception.Message, MessageType.Error);
            }
        }

        private static void ShowAddMenu(
            SerializedObject serializedObject,
            SerializedProperty modules,
            AnimatorCodePipelineSettings settings)
        {
            var menu = new GenericMenu();
            var types = TypeCache.GetTypesDerivedFrom<AnimatorCodeModule>()
                .Where(type => !type.IsAbstract &&
                               !type.ContainsGenericParameters &&
                               type.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToArray();

            foreach (var type in types)
            {
                var capturedType = type;
                menu.AddItem(
                    new GUIContent(type.FullName),
                    false,
                    () => Add(capturedType, serializedObject, modules, settings));
            }

            if (types.Length == 0)
                menu.AddDisabledItem(new GUIContent("No module definitions found"));

            menu.ShowAsContext();
        }

        private static void Add(
            Type type,
            SerializedObject serializedObject,
            SerializedProperty modules,
            AnimatorCodePipelineSettings settings)
        {
            serializedObject.Update();
            modules.arraySize++;
            modules.GetArrayElementAtIndex(modules.arraySize - 1).managedReferenceValue =
                Activator.CreateInstance(type);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
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
        private const string PendingSettingsKey = "AnimatorCodePipeline.NewModule.Settings";
        private const string PendingScriptKey = "AnimatorCodePipeline.NewModule.Script";
        private const string PendingAttemptKey = "AnimatorCodePipeline.NewModule.Attempt";
        private const string FolderKeyPrefix = "AnimatorCodePipeline.NewModule.Folder.";
        private const int MaxRegistrationAttempts = 10;

        static AnimatorCodeModuleGenerator()
        {
            EditorApplication.delayCall += TryRegisterPendingModule;
        }

        internal static void Create(AnimatorCodePipelineSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

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

            var settingsId = GlobalObjectId.GetGlobalObjectIdSlow(settings);
            if (settingsId.identifierType == 0)
            {
                Debug.LogError(
                    "Animator Code Pipeline could not identify the Settings component for generated-module registration. " +
                    "Save the scene or prefab and try again.");
                return;
            }

            EnsureEditorAssemblyReference(folder);
            File.WriteAllText(ToAbsolutePath(scriptPath), LoadTemplate(className), new UTF8Encoding(false));
            EditorPrefs.SetString(folderKey, folder);
            SessionState.SetString(PendingSettingsKey, settingsId.ToString());
            SessionState.SetString(PendingScriptKey, scriptPath);
            SessionState.SetInt(PendingAttemptKey, 0);
            AssetDatabase.Refresh();
            EditorApplication.delayCall += TryRegisterPendingModule;
        }

        private static void TryRegisterPendingModule()
        {
            var settingsIdText = SessionState.GetString(PendingSettingsKey, string.Empty);
            var scriptPath = SessionState.GetString(PendingScriptKey, string.Empty);
            if (string.IsNullOrEmpty(settingsIdText) || string.IsNullOrEmpty(scriptPath))
                return;

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            var type = script == null ? null : script.GetClass();
            if (type == null)
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    EditorApplication.delayCall += TryRegisterPendingModule;
                    return;
                }

                var attempt = SessionState.GetInt(PendingAttemptKey, 0) + 1;
                if (!EditorUtility.scriptCompilationFailed && attempt < MaxRegistrationAttempts)
                {
                    SessionState.SetInt(PendingAttemptKey, attempt);
                    EditorApplication.delayCall += TryRegisterPendingModule;
                    return;
                }

                Debug.LogWarning(
                    $"Animator Code Pipeline could not register the generated module '{scriptPath}'. " +
                    "Fix any compile errors, then add the module from the Animator Code Pipeline Settings Inspector.");
                ClearPending();
                return;
            }

            if (!typeof(AnimatorCodeModule).IsAssignableFrom(type) || type.IsAbstract ||
                type.ContainsGenericParameters || type.GetConstructor(Type.EmptyTypes) == null)
            {
                Debug.LogError(
                    $"Animator Code Pipeline generated type '{type.FullName}' is not a concrete module with a public parameterless constructor.");
                ClearPending();
                return;
            }

            if (!GlobalObjectId.TryParse(settingsIdText, out var settingsId))
            {
                Debug.LogError(
                    "Animator Code Pipeline could not parse the pending Settings object identifier. " +
                    "Add the generated module manually from the Settings Inspector.");
                ClearPending();
                return;
            }

            var settings = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(settingsId) as AnimatorCodePipelineSettings;
            if (settings == null)
            {
                Debug.LogWarning(
                    "Animator Code Pipeline could not find the Settings component that requested the generated module. " +
                    "Add the generated module manually from the Settings Inspector.");
                ClearPending();
                return;
            }

            var serializedObject = new SerializedObject(settings);
            var modules = serializedObject.FindProperty("modules");
            if (modules == null)
            {
                Debug.LogError("Animator Code Pipeline Settings no longer exposes its serialized module list.");
                ClearPending();
                return;
            }

            for (var index = 0; index < modules.arraySize; index++)
            {
                if (modules.GetArrayElementAtIndex(index).managedReferenceValue?.GetType() == type)
                {
                    ClearPending();
                    Selection.activeObject = settings;
                    return;
                }
            }

            modules.arraySize++;
            modules.GetArrayElementAtIndex(modules.arraySize - 1).managedReferenceValue =
                Activator.CreateInstance(type);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);

            if (settings.gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(settings.gameObject.scene);
            else
                AssetDatabase.SaveAssets();

            ClearPending();
            Selection.activeObject = settings;
        }

        private static void ClearPending()
        {
            SessionState.EraseString(PendingSettingsKey);
            SessionState.EraseString(PendingScriptKey);
            SessionState.EraseInt(PendingAttemptKey);
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
                var assemblyReferences = directory.GetFiles("*.asmref");
                if (assemblyReferences.Any(ReferencesAnimatorCodePipelineEditor))
                    return;
                if (assemblyReferences.Length != 0)
                {
                    Debug.LogWarning(
                        $"Animator Code Pipeline module folder is already controlled by '{assemblyReferences[0].Name}'. " +
                        "Ensure that referenced assembly depends on AnimatorCodePipeline.Editor before compiling the generated module.");
                    return;
                }

                var assemblyDefinitions = directory.GetFiles("*.asmdef");
                if (assemblyDefinitions.Length != 0)
                {
                    Debug.LogWarning(
                        $"Animator Code Pipeline module folder is already controlled by '{assemblyDefinitions[0].Name}'. " +
                        "Ensure that assembly references AnimatorCodePipeline.Editor before compiling the generated module.");
                    return;
                }
                if (directory.FullName.Equals(assetsDirectory.FullName, StringComparison.OrdinalIgnoreCase))
                    break;
                directory = directory.Parent;
            }

            var asmrefPath = folder + "/AnimatorCodePipeline.Editor.asmref";
            File.Copy(GetTemplatePath("AnimatorCodePipeline.Editor.asmref"), ToAbsolutePath(asmrefPath));
        }

        private static bool ReferencesAnimatorCodePipelineEditor(FileInfo asmref)
        {
            try
            {
                var data = JsonUtility.FromJson<AssemblyReferenceData>(File.ReadAllText(asmref.FullName));
                if (data == null || string.IsNullOrWhiteSpace(data.reference))
                    return false;

                if (data.reference == "AnimatorCodePipeline.Editor")
                    return true;

                var editorAssemblyGuid = AssetDatabase.AssetPathToGUID(
                    "Packages/com.github.t4lly.animator-code-pipeline/Editor/AnimatorCodePipeline.Editor.asmdef");
                return !string.IsNullOrEmpty(editorAssemblyGuid) && data.reference == "GUID:" + editorAssemblyGuid;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Serializable]
        private sealed class AssemblyReferenceData
        {
            public string reference;
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
                "Packages/com.github.t4lly.animator-code-pipeline/Editor/AnimatorCodeModuleInspectorUtility.cs");
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
