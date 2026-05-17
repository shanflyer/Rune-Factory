using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class UGUICompatibilityMigrationWindow : EditorWindow
{
    string m_RootPath = "Assets";

    [MenuItem("Tools/UI/Migrate UGUI Compatibility Data...")]
    public static void Open()
    {
        GetWindow<UGUICompatibilityMigrationWindow>("UGUI Migration");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Prefab Root Path", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Only prefabs under this Assets path are scanned. Subfolders are included.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        m_RootPath = EditorGUILayout.TextField(m_RootPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select prefab root", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selected))
                m_RootPath = UGUICompatibilityMigration.NormalizeAssetFolderPath(selected);
        }
        EditorGUILayout.EndHorizontal();

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(m_RootPath)))
        {
            if (GUILayout.Button("Run Migration"))
                UGUICompatibilityMigration.MigratePrefabs(m_RootPath);
        }
    }
}

public static class UGUICompatibilityMigration
{
    static readonly System.Type GameButtonType = System.Type.GetType("GameButton, Assembly-CSharp");
    static readonly System.Type GameToggleType = System.Type.GetType("GameToggle, Assembly-CSharp");
    static readonly System.Type GameSliderType = System.Type.GetType("GameSlider, Assembly-CSharp");
    static readonly System.Type GameScrollbarType = System.Type.GetType("GameScrollbar, Assembly-CSharp");
    static readonly System.Type GameDropdownType = System.Type.GetType("GameDropdown, Assembly-CSharp");
    static readonly System.Type GameImageType = System.Type.GetType("GameImage, Assembly-CSharp");
    static readonly System.Type GameTextMeshProUGUIType = System.Type.GetType("GameTextMeshProUGUI, Assembly-CSharp");

    public static void MigratePrefabs(string rootPath)
    {
        rootPath = NormalizeAssetFolderPath(rootPath);
        if (!AssetDatabase.IsValidFolder(rootPath))
        {
            Debug.LogError($"UGUI migration path is not a valid Assets folder: {rootPath}");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { rootPath });
        int changedCount = 0;
        int failedCount = 0;

        AssetDatabase.StartAssetEditing();

        try
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = null;
                bool changed = false;

                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);

                    Selectable[] selectables = root.GetComponentsInChildren<Selectable>(true);
                    foreach (Selectable selectable in selectables)
                    {
                        if (selectable != null)
                            changed |= MigrateSelectable(selectable);
                    }

                    Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
                    foreach (Graphic graphic in graphics)
                    {
                        if (graphic != null)
                            changed |= MigrateGraphic(graphic);
                    }

                    TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
                    foreach (TMP_Text text in texts)
                    {
                        if (text != null)
                            changed |= MigrateText(text);
                    }

                    if (changed)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        changedCount++;
                    }
                }
                catch (System.Exception exception)
                {
                    failedCount++;
                    Debug.LogError($"Failed to migrate prefab '{path}': {exception}");
                }
                finally
                {
                    if (root != null)
                        PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Migrated UGUI compatibility data under '{rootPath}'. Changed prefabs: {changedCount}. Failed prefabs: {failedCount}. Scanned prefabs: {guids.Length}.");
    }

    public static string NormalizeAssetFolderPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        path = path.Trim().Replace('\\', '/');
        string dataPath = Application.dataPath.Replace('\\', '/');
        if (path.StartsWith(dataPath, System.StringComparison.OrdinalIgnoreCase))
        {
            string relative = path.Substring(dataPath.Length).TrimStart('/');
            return string.IsNullOrEmpty(relative) ? "Assets" : $"Assets/{relative}";
        }

        return path;
    }

    static bool MigrateSelectable(Selectable selectable)
    {
        System.Type sourceType = selectable.GetType();
        System.Type targetType = null;
        var source = new SerializedObject(selectable);
        int setUid = source.FindProperty("m_setUid")?.intValue ?? 0;
        int guid = source.FindProperty("m_guid")?.intValue ?? 0;
        bool hideSelected = source.FindProperty("HideSelected")?.boolValue ?? false;

        if (sourceType == typeof(Button))
            targetType = GameButtonType;
        else if (sourceType == typeof(Toggle))
            targetType = GameToggleType;
        else if (sourceType == typeof(Slider))
            targetType = GameSliderType;
        else if (sourceType == typeof(Scrollbar))
            targetType = GameScrollbarType;
        else if (sourceType == typeof(Dropdown))
            targetType = GameDropdownType;

        GameObject gameObject = selectable.gameObject;
        if (targetType != null && !ReplaceScript(selectable, targetType))
            return false;

        if (targetType == null && !HasProperty(source, "m_GameSetUid"))
            return false;

        Component migratedComponent = targetType != null ? gameObject.GetComponent(targetType) : selectable;
        if (migratedComponent == null)
        {
            Debug.LogError($"Failed to reacquire migrated selectable on {gameObject.name}.");
            return false;
        }

        var target = new SerializedObject(migratedComponent);
        SetInt(target, "m_GameSetUid", setUid);
        SetInt(target, "m_GameGuid", guid);
        SetBool(target, "m_GameHideSelected", hideSelected);
        target.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    static bool MigrateGraphic(Graphic graphic)
    {
        if (graphic.GetType() != typeof(Image))
            return false;

        var source = new SerializedObject(graphic);
        SerializedProperty nullClearProperty = source.FindProperty("m_NullClear");
        SerializedProperty colorGradientProperty = source.FindProperty("m_ColorGradient");
        Color colorTL = source.FindProperty("m_ColorTL")?.colorValue ?? Color.white;
        Color colorTR = source.FindProperty("m_ColorTR")?.colorValue ?? Color.white;
        Color colorBL = source.FindProperty("m_ColorBL")?.colorValue ?? Color.white;
        Color colorBR = source.FindProperty("m_ColorBR")?.colorValue ?? Color.white;

        bool nullClear = nullClearProperty?.boolValue ?? false;
        bool colorGradient = colorGradientProperty?.boolValue ?? false;
        if (!nullClear && !colorGradient)
            return false;

        GameObject gameObject = graphic.gameObject;
        if (!ReplaceScript(graphic, GameImageType))
            return false;

        Component migratedComponent = gameObject.GetComponent(GameImageType);
        if (migratedComponent == null)
        {
            Debug.LogError($"Failed to reacquire migrated image on {gameObject.name}.");
            return false;
        }

        var target = new SerializedObject(migratedComponent);
        SetBool(target, "m_GameNullClear", nullClear);
        SetBool(target, "m_GameColorGradient", colorGradient);
        SetColor(target, "m_GameColorTL", colorTL);
        SetColor(target, "m_GameColorTR", colorTR);
        SetColor(target, "m_GameColorBL", colorBL);
        SetColor(target, "m_GameColorBR", colorBR);
        target.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    static bool MigrateText(TMP_Text text)
    {
        System.Type sourceType = text.GetType();
        System.Type targetType = null;

        if (sourceType == typeof(TextMeshProUGUI))
            targetType = GameTextMeshProUGUIType;
        else if (sourceType == typeof(TextMeshPro))
            return false;

        if (targetType == null)
            return false;

        var source = new SerializedObject(text);
        bool swLanguage = source.FindProperty("isSwLanguage")?.boolValue ?? true;
        GameObject gameObject = text.gameObject;
        if (!ReplaceScript(text, targetType))
            return false;

        Component migratedComponent = gameObject.GetComponent(targetType);
        if (migratedComponent == null)
        {
            Debug.LogError($"Failed to reacquire migrated text on {gameObject.name}.");
            return false;
        }

        var target = new SerializedObject(migratedComponent);
        SetBool(target, "m_GameSwLanguage", swLanguage);
        target.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    static bool ReplaceScript(Component component, System.Type targetType)
    {
        if (targetType == null)
        {
            Debug.LogError($"Target component type for {component.GetType().Name} was not found. Let Unity compile runtime scripts before running migration.");
            return false;
        }

        MonoScript targetScript = FindScript(targetType);
        if (targetScript == null)
        {
            Debug.LogError($"MonoScript for {targetType.FullName} was not found.");
            return false;
        }

        var serializedObject = new SerializedObject(component);
        SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
        if (scriptProperty == null || scriptProperty.objectReferenceValue == targetScript)
            return false;

        scriptProperty.objectReferenceValue = targetScript;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    static void SetInt(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.intValue = value;
    }

    static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.boolValue = value;
    }

    static void SetColor(SerializedObject serializedObject, string propertyName, Color value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.colorValue = value;
    }

    static bool HasProperty(SerializedObject serializedObject, string propertyName)
    {
        return serializedObject.FindProperty(propertyName) != null;
    }

    static MonoScript FindScript(System.Type type)
    {
        foreach (MonoScript script in MonoImporter.GetAllRuntimeMonoScripts())
        {
            if (script.GetClass() == type)
                return script;
        }

        return null;
    }
}
