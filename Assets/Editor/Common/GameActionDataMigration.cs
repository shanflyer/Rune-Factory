using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// GameActionData 批量重构与迁移工具
/// 支持：① 类型重命名 ② 参数批量修改 ③ 数据导出/导入
/// </summary>
public class GameActionDataMigration : EditorWindow
{
    [MenuItem("Tools/GameAction/迁移与重构工具")]
    private static void ShowWindow()
    {
        var win = GetWindow<GameActionDataMigration>();
        win.titleContent = new GUIContent("GameAction 迁移");
        win.minSize = new Vector2(500, 400);
        win.Show();
    }

    // ─── UI 状态 ──────────────────────────────────────────────

    private enum Tab { RenameType, BatchParam, ExportImport }
    private Tab _currentTab;

    // 重命名
    private string _oldTypeName = "";
    private string _newTypeName = "";
    private Vector2 _renamePreviewScroll;

    // 批量参数
    private string _batchTargetType = "";
    private int _batchParamIndex;
    private string _batchOldValue = "";
    private string _batchNewValue = "";
    private Vector2 _batchScroll;

    // 导出/导入
    private string _exportPath = "Assets/../GameActionData_Export.json";
    private Vector2 _exportScroll;

    // ─── GUI ──────────────────────────────────────────────────

    private void OnGUI()
    {
        EditorGUILayout.Space(4);
        _currentTab = (Tab)GUILayout.SelectionGrid((int)_currentTab,
            new[] { "类型重命名", "批量改参数", "导出/导入" }, 3);

        EditorGUILayout.Space(8);

        switch (_currentTab)
        {
            case Tab.RenameType: DrawRenameTab(); break;
            case Tab.BatchParam: DrawBatchParamTab(); break;
            case Tab.ExportImport: DrawExportImportTab(); break;
        }
    }

    // ─── Tab: 类型重命名 ──────────────────────────────────────

    private void DrawRenameTab()
    {
        EditorGUILayout.HelpBox(
            "当重命名或替换 GameAction struct 时，批量更新所有引用的 GameActionData",
            MessageType.Info);

        EditorGUILayout.LabelField("旧类型名（当前值）", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            _oldTypeName = EditorGUILayout.TextField(_oldTypeName);
            if (GUILayout.Button("▼ 选择", GUILayout.Width(60)))
                ShowTypeMenu((name) => _oldTypeName = name);
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("新类型名（替换为）", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            _newTypeName = EditorGUILayout.TextField(_newTypeName);
            if (GUILayout.Button("▼ 选择", GUILayout.Width(60)))
                ShowTypeMenu((name) => _newTypeName = name);
        }

        EditorGUILayout.Space(8);

        // 预览受影响资产
        if (!string.IsNullOrEmpty(_oldTypeName))
        {
            var affected = FindByTypeName(_oldTypeName);
            EditorGUILayout.LabelField($"受影响资产: {affected.Count} 个", EditorStyles.boldLabel);

            _renamePreviewScroll = EditorGUILayout.BeginScrollView(_renamePreviewScroll,
                GUILayout.Height(120));
            foreach (var a in affected)
                EditorGUILayout.SelectableLabel(a, EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();

            GUI.enabled = affected.Count > 0 && !string.IsNullOrEmpty(_newTypeName);
            if (GUILayout.Button($"执行重命名: {_oldTypeName} → {_newTypeName}",
                    GUILayout.Height(36)))
            {
                RenameType(_oldTypeName, _newTypeName, affected);
            }
            GUI.enabled = true;
        }
    }

    // ─── Tab: 批量参数 ─────────────────────────────────────────

    private void DrawBatchParamTab()
    {
        EditorGUILayout.HelpBox(
            "批量修改指定 Action 类型中某个参数的值（例如将所有 talkId=0 改为 talkId=1）",
            MessageType.Info);

        EditorGUILayout.LabelField("目标 Action 类型", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            _batchTargetType = EditorGUILayout.TextField(_batchTargetType);
            if (GUILayout.Button("▼ 选择", GUILayout.Width(60)))
                ShowTypeMenu((name) => _batchTargetType = name);
        }

        if (!string.IsNullOrEmpty(_batchTargetType))
        {
            var type = FindType(_batchTargetType);
            if (type != null)
            {
                var fields = type.GetFields(System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.Instance)
                    .Where(f => !IsExcluded(f))
                    .ToList();

                var fieldNames = fields.Select(f => $"[{fields.IndexOf(f)}] {f.Name} ({f.FieldType.Name})").ToArray();
                _batchParamIndex = EditorGUILayout.Popup("参数序号", _batchParamIndex, fieldNames);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("替换规则", EditorStyles.boldLabel);
            _batchOldValue = EditorGUILayout.TextField("旧值", _batchOldValue);
            _batchNewValue = EditorGUILayout.TextField("新值", _batchNewValue);

            EditorGUILayout.Space(8);

            var affected = FindByTypeName(_batchTargetType);
            EditorGUILayout.LabelField($"匹配资产: {affected.Count} 个");

            if (!string.IsNullOrEmpty(_batchOldValue) && affected.Count > 0)
            {
                // 预览
                var matched = new List<string>();
                foreach (var path in affected)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(path);
                    if (asset != null && asset._parameters != null
                                      && _batchParamIndex < asset._parameters.Count
                                      && asset._parameters[_batchParamIndex].value == _batchOldValue)
                        matched.Add(path);
                }

                EditorGUILayout.LabelField($"将修改: {matched.Count} 个参数");
                _batchScroll = EditorGUILayout.BeginScrollView(_batchScroll, GUILayout.Height(100));
                foreach (var m in matched)
                    EditorGUILayout.SelectableLabel(m, EditorStyles.miniLabel);
                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("执行批量替换", GUILayout.Height(32)))
                    BatchReplaceParam(affected, _batchParamIndex, _batchOldValue, _batchNewValue);
            }
        }
    }

    // ─── Tab: 导出/导入 ──────────────────────────────────────

    private void DrawExportImportTab()
    {
        EditorGUILayout.HelpBox(
            "将所有 GameActionData 导出为 JSON，可在外部编辑后重新导入",
            MessageType.Info);

        EditorGUILayout.LabelField("导出路径", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            _exportPath = EditorGUILayout.TextField(_exportPath);
            if (GUILayout.Button("…", GUILayout.Width(24)))
            {
                var chosen = EditorUtility.SaveFilePanel("导出 GameActionData",
                    Application.dataPath, "GameActionData_Export.json", "json");
                if (!string.IsNullOrEmpty(chosen))
                    _exportPath = chosen;
            }
        }

        EditorGUILayout.Space(6);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("📤 导出所有数据到 JSON"))
                ExportToJson(_exportPath);

            if (GUILayout.Button("📥 从 JSON 导入"))
                ImportFromJson(_exportPath);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("批量创建占位模板", EditorStyles.boldLabel);

        if (GUILayout.Button("为当前所有未使用的 Action 类型创建模板"))
        {
            CreateTemplateAssets();
        }
    }

    // ─── 内部逻辑 ──────────────────────────────────────────────

    private static List<string> FindByTypeName(string typeName)
    {
        var guids = AssetDatabase.FindAssets("t:GameActionData");
        var result = new List<string>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(path);
            if (asset != null && asset.typeName == typeName)
                result.Add(path);
        }

        return result;
    }

    private static Type FindType(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name.StartsWith("Unity") ||
                asm.GetName().Name.StartsWith("System") ||
                asm.GetName().Name == "mscorlib") continue;
            try
            {
                foreach (var t in asm.GetTypes())
                {
                    if (t.Name == typeName) return t;
                }
            }
            catch { }
        }
        return null;
    }

    private static bool IsExcluded(System.Reflection.FieldInfo f)
    {
        var excluded = new[] { "setValue", "setResult", "endAction" };
        return excluded.Contains(f.Name)
               || f.FieldType == typeof(Action)
               || typeof(Delegate).IsAssignableFrom(f.FieldType);
    }

    private void ShowTypeMenu(Action<string> onSelect)
    {
        var menu = new GenericMenu();
        var interfaceType = typeof(GameAction);
        var types = new List<Type>();

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name.StartsWith("Unity") ||
                asm.GetName().Name.StartsWith("System") ||
                asm.GetName().Name == "mscorlib") continue;
            try
            {
                foreach (var t in asm.GetTypes())
                {
                    if (t.IsValueType && !t.IsAbstract && interfaceType.IsAssignableFrom(t))
                        types.Add(t);
                }
            }
            catch { }
        }

        types = types.OrderBy(t => t.Name).ToList();
        foreach (var t in types)
        {
            var source = t.DeclaringType != null ? t.DeclaringType.Name : "global";
            menu.AddItem(new GUIContent($"{t.Name} ({source})"), false,
                () => onSelect(t.Name));
        }

        menu.ShowAsContext();
    }

    // ─── 操作执行 ──────────────────────────────────────────────

    private void RenameType(string oldName, string newName, List<string> affected)
    {
        if (!EditorUtility.DisplayDialog("确认重命名",
                $"将 {affected.Count} 个 GameActionData 的 typeName\n" +
                $"从 \"{oldName}\" 改为 \"{newName}\" ?\n\n" +
                "此操作无法直接撤销!", "确认执行", "取消"))
            return;

        int count = 0;
        foreach (var path in affected)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(path);
            if (asset != null)
            {
                asset.typeName = newName;
                EditorUtility.SetDirty(asset);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[迁移] 已完成 {count} 个资产的类型重命名: {oldName} → {newName}");
    }

    private void BatchReplaceParam(List<string> assets, int paramIndex,
        string oldValue, string newValue)
    {
        int count = 0;
        foreach (var path in assets)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(path);
            if (asset == null || asset._parameters == null) continue;
            if (paramIndex < asset._parameters.Count &&
                asset._parameters[paramIndex].value == oldValue)
            {
                asset._parameters[paramIndex].value = newValue;
                EditorUtility.SetDirty(asset);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[批量替换] 已修改 {count} 个参数值: [{paramIndex}] \"{oldValue}\" → \"{newValue}\"");
    }

    // ─── 导出/导入数据模型 ────────────────────────────────────
    [Serializable]
    private class ExportData
    {
        public List<ExportItem> items = new List<ExportItem>();
    }

    [Serializable]
    private struct ExportItem
    {
        public int id;
        public string name;
        public string typeName;
        public List<string> parameters;
    }

    private void ExportToJson(string path)
    {
        var guids = AssetDatabase.FindAssets("t:GameActionData");
        var exportData = new ExportData();

        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(assetPath);
            if (asset == null) continue;

            exportData.items.Add(new ExportItem
            {
                id = asset.id,
                name = asset.name,
                typeName = asset.typeName,
                parameters = asset._parameters?.Select(p => p.value).ToList() ?? new List<string>()
            });
        }

        var json = JsonUtility.ToJson(exportData, true);
        File.WriteAllText(path, json);

        Debug.Log($"[导出] 已导出 {exportData.items.Count} 个 GameActionData 到 {path}");
    }

    private void ImportFromJson(string path)
    {
        if (!File.Exists(path))
        {
            EditorUtility.DisplayDialog("导入错误", $"文件不存在: {path}", "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("确认导入",
                "从 JSON 导入将覆盖匹配的 GameActionData 数据。\n" +
                "此操作不可撤销!", "确认导入", "取消"))
            return;

        var json = File.ReadAllText(path);
        var exportData = JsonUtility.FromJson<ExportData>(json);
        if (exportData == null || exportData.items == null)
        {
            Debug.LogError("[导入] JSON 格式无效");
            return;
        }

        int updated = 0, created = 0;
        foreach (var item in exportData.items)
        {
            var guids = AssetDatabase.FindAssets($"t:GameActionData {item.name}");
            GameActionData asset = null;

            foreach (var guid in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                var a = AssetDatabase.LoadAssetAtPath<GameActionData>(p);
                if (a != null && a.name == item.name) { asset = a; break; }
            }

            if (asset == null)
            {
                // 创建新资产
                asset = ScriptableObject.CreateInstance<GameActionData>();
                asset.name = item.name;
                var savePath = $"Assets/Resources/Data/GameActionData/{item.name}.asset";
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                AssetDatabase.CreateAsset(asset, savePath);
                created++;
            }

            asset.id = item.id;
            asset.typeName = item.typeName;
            asset._parameters = item.parameters?
                .Select(v => new Parameter { value = v })
                .ToList() ?? new List<Parameter>();

            EditorUtility.SetDirty(asset);
            updated++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[导入] 完成: 更新 {updated} 个, 新建 {created} 个");
    }

    private void CreateTemplateAssets()
    {
        var interfaceType = typeof(GameAction);
        var allTypes = new List<Type>();

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name.StartsWith("Unity") ||
                asm.GetName().Name.StartsWith("System") ||
                asm.GetName().Name == "mscorlib") continue;
            try
            {
                foreach (var t in asm.GetTypes())
                {
                    if (t.IsValueType && !t.IsAbstract && interfaceType.IsAssignableFrom(t))
                        allTypes.Add(t);
                }
            }
            catch { }
        }

        // 获取已有 typeName
        var existingNames = new HashSet<string>();
        var guids = AssetDatabase.FindAssets("t:GameActionData");
        foreach (var guid in guids)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(
                AssetDatabase.GUIDToAssetPath(guid));
            if (asset != null && !string.IsNullOrEmpty(asset.typeName))
                existingNames.Add(asset.typeName);
        }

        var missingTypes = allTypes.Where(t => !existingNames.Contains(t.Name)).ToList();
        if (missingTypes.Count == 0)
        {
            Debug.Log("所有 Action 类型已有对应的 GameActionData 模板，无需创建");
            return;
        }

        var basePath = "Assets/Resources/Data/GameActionData/Templates";
        Directory.CreateDirectory(basePath);

        int created = 0;
        foreach (var t in missingTypes)
        {
            var asset = ScriptableObject.CreateInstance<GameActionData>();
            asset.id = -1; // 标记为模板
            asset.typeName = t.Name;
            asset.name = $"_{t.Name}_Template";

            // 创建默认参数
            var fields = t.GetFields(System.Reflection.BindingFlags.Public |
                                     System.Reflection.BindingFlags.Instance)
                .Where(f => !IsExcluded(f)).ToList();
            asset._parameters = fields.Select(f => new Parameter { value = "0" }).ToList();

            var savePath = $"{basePath}/{asset.name}.asset";
            AssetDatabase.CreateAsset(asset, savePath);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[模板] 已创建 {created} 个缺失类型的模板到 {basePath}");
    }
}
