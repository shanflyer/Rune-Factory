using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 旧 GameActionData → 新强类型 XxxActionData 迁移工具
/// </summary>
public class GameActionDataUpgradeTool : EditorWindow
{
    [MenuItem("Tools/GameAction/迁移到强类型 (批量)")]
    private static void ShowWindow()
    {
        var win = GetWindow<GameActionDataUpgradeTool>();
        win.titleContent = new GUIContent("GameAction 升级");
        win.minSize = new Vector2(600, 500);
        win.Show();
    }

    private Vector2 _scroll;
    private List<UpgradeItem> _items;
    private bool _scanned;
    private string _targetFolder = "Assets/Resources/Data/GameActionData/Typed";

    private class UpgradeItem
    {
        public string oldPath;
        public GameActionData oldAsset;
        public string structName;
        public Type newType;
        public bool canUpgrade;
        public bool selected = true;
        public string error;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("旧 GameActionData → 强类型 ActionData 迁移", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox(
            "将旧的 GameActionData (字符串 typeName + List<Parameter>) " +
            "迁移为强类型 XxxActionData (typed fields)。\n" +
            "• 旧文件保留不动，新文件生成到指定目录\n" +
            "• 迁移后需手动更新行为树/事件中的引用", MessageType.Info);

        EditorGUILayout.Space(8);
        using (new EditorGUILayout.HorizontalScope())
        {
            _targetFolder = EditorGUILayout.TextField("输出目录", _targetFolder);
            if (GUILayout.Button("...", GUILayout.Width(24)))
            {
                var chosen = EditorUtility.OpenFolderPanel("选择输出目录", "Assets/Resources/Data/GameActionData", "");
                if (!string.IsNullOrEmpty(chosen))
                {
                    var idx = chosen.IndexOf("Assets/");
                    if (idx >= 0)
                        _targetFolder = chosen.Substring(idx).Replace("\\", "/");
                }
            }
        }

        EditorGUILayout.Space(8);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("🔍 扫描旧资产", GUILayout.Height(28)))
                ScanAssets();

            GUI.enabled = _scanned && _items.Any(i => i.canUpgrade && i.selected);
            if (GUILayout.Button("⬆ 批量升级选中", GUILayout.Height(28)))
                BatchUpgrade();
            GUI.enabled = true;

            if (GUILayout.Button("全选", GUILayout.Width(60)))
                SelectAll(true);
            if (GUILayout.Button("全不选", GUILayout.Width(60)))
                SelectAll(false);
        }

        if (!_scanned) return;

        EditorGUILayout.Space(4);
        var canUpgrade = _items.Where(i => i.canUpgrade).ToList();
        var cannotUpgrade = _items.Where(i => !i.canUpgrade).ToList();
        EditorGUILayout.LabelField(
            $"可升级 {canUpgrade.Count} / 共 {_items.Count} (选择 {_items.Count(i => i.selected)})",
            EditorStyles.miniLabel);

        EditorGUILayout.Space(4);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        foreach (var item in _items)
        {
            EditorGUILayout.BeginHorizontal("box");

            item.selected = EditorGUILayout.Toggle(item.selected, GUILayout.Width(20));

            if (item.canUpgrade)
            {
                EditorGUILayout.LabelField($"✓ {item.structName}", EditorStyles.boldLabel,
                    GUILayout.Width(200));
                EditorGUILayout.LabelField(
                    System.IO.Path.GetFileNameWithoutExtension(item.oldPath),
                    EditorStyles.miniLabel);
            }
            else
            {
                var oldColor = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField($"✗ {item.error}", GUILayout.Width(250));
                GUI.color = oldColor;
                EditorGUILayout.LabelField(
                    System.IO.Path.GetFileNameWithoutExtension(item.oldPath),
                    EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void ScanAssets()
    {
        _items = new List<UpgradeItem>();
        var guids = AssetDatabase.FindAssets("t:GameActionData");

        var typedTypeCache = new Dictionary<string, Type>();
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in allAssemblies)
        {
            if (asm.GetName().Name.StartsWith("Unity") ||
                asm.GetName().Name.StartsWith("System") ||
                asm.GetName().Name == "mscorlib") continue;
            try
            {
                foreach (var t in asm.GetTypes())
                {
                    if (t.BaseType == typeof(GameActionBaseData))
                    {
                        // 类型名格式: SimpleTalkActionData → SimpleTalk
                        var match = System.Text.RegularExpressions.Regex.Match(
                            t.Name, @"^(.+)ActionData$");
                        if (match.Success)
                            typedTypeCache[match.Groups[1].Value] = t;
                    }
                }
            }
            catch { }
        }

        Debug.Log($"[升级工具] 找到 {typedTypeCache.Count} 个强类型");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(path);
            if (asset == null || string.IsNullOrEmpty(asset.typeName)) continue;

            var item = new UpgradeItem
            {
                oldPath = path,
                oldAsset = asset,
                structName = asset.typeName,
                selected = true
            };

            if (typedTypeCache.TryGetValue(asset.typeName, out var newType))
            {
                item.newType = newType;
                item.canUpgrade = true;
            }
            else
            {
                item.canUpgrade = false;
                item.error = $"未找到类型: {asset.typeName}";
            }

            _items.Add(item);
        }

        _items = _items.OrderBy(i => i.canUpgrade ? 0 : 1)
                       .ThenBy(i => i.structName)
                       .ToList();

        _scanned = true;
        Debug.Log($"[升级工具] 扫描完成: {_items.Count} 个资产");
    }

    private void SelectAll(bool select)
    {
        if (_items == null) return;
        foreach (var item in _items.Where(i => i.canUpgrade))
            item.selected = select;
    }

    private void BatchUpgrade()
    {
        var toUpgrade = _items.Where(i => i.canUpgrade && i.selected).ToList();
        if (toUpgrade.Count == 0) return;

        if (!EditorUtility.DisplayDialog("确认升级",
                $"将升级 {toUpgrade.Count} 个资产到强类型格式。\n" +
                $"旧文件会保留在原位。\n\n" +
                $"输出目录: {_targetFolder}", "确认", "取消"))
            return;

        if (!System.IO.Directory.Exists(_targetFolder))
            System.IO.Directory.CreateDirectory(_targetFolder);

        int success = 0, failed = 0;
        EditorUtility.DisplayProgressBar("升级中...", "", 0);

        for (int i = 0; i < toUpgrade.Count; i++)
        {
            var item = toUpgrade[i];
            EditorUtility.DisplayProgressBar("升级中...",
                $"{item.structName} ({i + 1}/{toUpgrade.Count})", (float)i / toUpgrade.Count);

            try
            {
                UpgradeSingle(item);
                success++;
            }
            catch (Exception e)
            {
                Debug.LogError($"升级失败: {item.structName} → {e.Message}");
                failed++;
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[升级工具] 完成: ✓{success} ✗{failed}");
        EditorUtility.DisplayDialog("升级完成",
            $"成功: {success}\n失败: {failed}", "确定");

        ScanAssets();
    }

    private void UpgradeSingle(UpgradeItem item)
    {
        // 创建新实例
        var newAsset = ScriptableObject.CreateInstance(item.newType) as GameActionBaseData;
        newAsset.id = item.oldAsset.id;
        newAsset.name = item.oldAsset.name;

        // 从旧参数复制到新字段
        if (item.oldAsset._parameters != null)
        {
            var fields = item.newType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.DeclaringType != typeof(GameActionBaseData)
                            && f.DeclaringType != typeof(ScriptableObject))
                .ToList();

            for (int i = 0; i < fields.Count && i < item.oldAsset._parameters.Count; i++)
            {
                var field = fields[i];
                var paramValue = item.oldAsset._parameters[i].value;

                object converted = ConvertTo(paramValue, field.FieldType);
                if (converted != null)
                    field.SetValue(newAsset, converted);
            }
        }

        // 保存
        var fileName = $"{item.structName}_{item.oldAsset.name}.asset";
        var newPath = System.IO.Path.Combine(_targetFolder, fileName);
        newPath = newPath.Replace("\\", "/");

        // 确保唯一性
        if (System.IO.File.Exists(newPath))
        {
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
        }

        AssetDatabase.CreateAsset(newAsset, newPath);
        EditorUtility.SetDirty(newAsset);
    }

    private static object ConvertTo(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value)) return null;

        if (targetType == typeof(int))
        {
            int.TryParse(value, out var v);
            return v;
        }
        if (targetType == typeof(float))
        {
            float.TryParse(value, out var v);
            return v;
        }
        if (targetType == typeof(bool))
        {
            return value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1";
        }
        if (targetType == typeof(string))
        {
            return value;
        }
        if (targetType == typeof(Vector2))
        {
            var parts = value.Split(',');
            if (parts.Length >= 2 &&
                float.TryParse(parts[0], out var x) &&
                float.TryParse(parts[1], out var y))
                return new Vector2(x, y);
        }
        if (targetType == typeof(Vector3))
        {
            var parts = value.Split(',');
            if (parts.Length >= 3 &&
                float.TryParse(parts[0], out var x) &&
                float.TryParse(parts[1], out var y) &&
                float.TryParse(parts[2], out var z))
                return new Vector3(x, y, z);
        }

        return value; // fallback: string
    }
}
