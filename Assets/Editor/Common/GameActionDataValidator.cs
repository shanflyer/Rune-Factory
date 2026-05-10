using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// GameActionData 批量验证与引用分析工具
/// 提供：① 无效类型检查 ② 参数完整性检查 ③ 批量迁移辅助
/// </summary>
public static class GameActionDataValidator
{
    private static List<Type> _allActionTypes;
    private static HashSet<string> _allActionTypeNames;

    // ─── 菜单入口 ──────────────────────────────────────────────

    [MenuItem("Tools/GameAction/验证所有 GameActionData")]
    private static void ValidateAll()
    {
        RefreshTypeCache();
        var guids = AssetDatabase.FindAssets("t:GameActionData");
        var report = new Report();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(path);
            if (asset == null) continue;

            ValidateSingle(asset, path, report);
        }

        // 输出报告
        PrintReport(report, guids.Length);
    }

    [MenuItem("Tools/GameAction/统计各 Action 类型使用量")]
    private static void ShowTypeUsage()
    {
        RefreshTypeCache();
        var guids = AssetDatabase.FindAssets("t:GameActionData");
        var usageCount = new Dictionary<string, int>();
        var typeExamples = new Dictionary<string, string>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(path);
            if (asset == null || string.IsNullOrEmpty(asset.typeName)) continue;

            usageCount.TryGetValue(asset.typeName, out int count);
            usageCount[asset.typeName] = count + 1;

            if (!typeExamples.ContainsKey(asset.typeName))
                typeExamples[asset.typeName] = path;
        }

        Debug.Log("══════════════════════════════════════════");
        Debug.Log($"  Action 类型使用统计 (共 {guids.Length} 个 GameActionData)");
        Debug.Log("══════════════════════════════════════════");

        foreach (var kv in usageCount.OrderByDescending(x => x.Value))
        {
            var example = typeExamples[kv.Key];
            Debug.Log($"  {kv.Key,-40} ×{kv.Value,4}  例: {example}");
        }

        Debug.Log("══════════════════════════════════════════");

        // 同时显示未使用的类型
        var unused = _allActionTypes
            .Where(t => !usageCount.ContainsKey(t.Name))
            .OrderBy(t => t.Name)
            .ToList();

        if (unused.Count > 0)
        {
            Debug.Log($"\n未在任何 GameActionData 中使用的 Action 类型 ({unused.Count} 个):");
            foreach (var t in unused)
                Debug.Log($"  • {t.Name}");
        }
    }

    [MenuItem("Tools/GameAction/批量修复空参数")]
    private static void FixEmptyParameters()
    {
        RefreshTypeCache();
        var guids = AssetDatabase.FindAssets("t:GameActionData");
        int fixedCount = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(path);
            if (asset == null || string.IsNullOrEmpty(asset.typeName)) continue;

            var type = _allActionTypes.FirstOrDefault(t => t.Name == asset.typeName);
            if (type == null) continue;

            // 计算期望的参数数量
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !IsExcludedField(f))
                .ToList();

            if (asset._parameters == null)
                asset._parameters = new List<Parameter>();

            bool changed = false;

            // 补齐缺失参数
            while (asset._parameters.Count < fields.Count)
            {
                asset._parameters.Add(new Parameter { value = "0" });
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(asset);
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[批量修复] 已修复 {fixedCount} 个 GameActionData 的参数列表");
    }

    // ─── 核心逻辑 ──────────────────────────────────────────────

    private static void RefreshTypeCache()
    {
        if (_allActionTypes != null) return;

        var interfaceType = typeof(GameAction);
        _allActionTypes = new List<Type>();

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
                        _allActionTypes.Add(t);
                }
            }
            catch { /* 跳过 */ }
        }

        _allActionTypeNames = new HashSet<string>(_allActionTypes.Select(t => t.Name));
    }

    private static bool IsExcludedField(FieldInfo f)
    {
        var excludedNames = new[] { "setValue", "setResult", "endAction" };
        return excludedNames.Contains(f.Name)
               || f.FieldType == typeof(Action)
               || typeof(Delegate).IsAssignableFrom(f.FieldType);
    }

    private static void ValidateSingle(GameActionData asset, string path, Report report)
    {
        report.Total++;

        // 1. 验证 typeName
        if (string.IsNullOrEmpty(asset.typeName))
        {
            report.EmptyType.Add($"[空类型] {path}");
            return;
        }

        if (!_allActionTypeNames.Contains(asset.typeName))
        {
            report.InvalidType.Add($"[无效类型] {path} → typeName='{asset.typeName}' 未找到对应 struct");
            report.InvalidTypeAssets.Add(asset);
            return;
        }

        report.ValidTypes++;

        // 2. 验证参数
        var type = _allActionTypes.First(t => t.Name == asset.typeName);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => !IsExcludedField(f))
            .ToList();

        if ((asset._parameters == null || asset._parameters.Count == 0) && fields.Count > 0)
        {
            report.EmptyParams.Add($"[空参数] {path} ({asset.typeName}) 期望 {fields.Count} 个参数, 实际 0");
        }
        else if (asset._parameters != null && asset._parameters.Count != fields.Count)
        {
            report.MismatchParams.Add(
                $"[参数数不符] {path} ({asset.typeName}) 期望 {fields.Count} 个参数, 实际 {asset._parameters.Count}");
        }
    }

    private static void PrintReport(Report report, int totalAssets)
    {
        Debug.Log("══════════════════════════════════════════");
        Debug.Log($"  GameActionData 验证报告");
        Debug.Log($"  扫描资产总数: {totalAssets}");
        Debug.Log($"  已验证: {report.Total} | 有效类型: {report.ValidTypes}");
        Debug.Log("══════════════════════════════════════════");

        if (report.EmptyType.Count > 0)
        {
            Debug.LogWarning($"\n⚠ {report.EmptyType.Count} 个资产 typeName 为空:");
            foreach (var m in report.EmptyType) Debug.LogWarning($"  {m}");
        }

        if (report.InvalidType.Count > 0)
        {
            Debug.LogError($"\n✗ {report.InvalidType.Count} 个资产引用了不存在的类型:");
            foreach (var m in report.InvalidType) Debug.LogError($"  {m}");
        }

        if (report.EmptyParams.Count > 0)
        {
            Debug.LogWarning($"\n⚠ {report.EmptyParams.Count} 个资产参数为空:");
            foreach (var m in report.EmptyParams) Debug.LogWarning($"  {m}");
        }

        if (report.MismatchParams.Count > 0)
        {
            Debug.LogWarning($"\n⚠ {report.MismatchParams.Count} 个资产参数数量不匹配:");
            foreach (var m in report.MismatchParams) Debug.LogWarning($"  {m}");
        }

        if (report.EmptyType.Count == 0 && report.InvalidType.Count == 0
                                         && report.EmptyParams.Count == 0 && report.MismatchParams.Count == 0)
        {
            Debug.Log("✔ 所有 GameActionData 资产验证通过，无问题!");
        }

        Debug.Log("══════════════════════════════════════════");
    }

    // ─── 报告结构 ──────────────────────────────────────────────

    private class Report
    {
        public int Total;
        public int ValidTypes;
        public List<string> EmptyType = new List<string>();
        public List<string> InvalidType = new List<string>();
        public List<GameActionData> InvalidTypeAssets = new List<GameActionData>();
        public List<string> EmptyParams = new List<string>();
        public List<string> MismatchParams = new List<string>();
    }
}
