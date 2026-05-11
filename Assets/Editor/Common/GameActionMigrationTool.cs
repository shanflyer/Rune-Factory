using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class GameActionMigrationTool
{
    private static Dictionary<string, Type> _nodeTypeCache;

    [MenuItem("Tools/GameAction/\u8fc1\u79fb\u6240\u6709\u65e7\u8d44\u4ea7\u5230\u65b0\u683c\u5f0f")]
    public static void MigrateAll()
    {
        BuildNodeCache();
        var oldDir = "Assets/Resources/Data/GameActionData";
        var newDir = "Assets/Resources/Data/GameActionAssets";
        Directory.CreateDirectory(newDir);

        try
        {
            AssetDatabase.StartAssetEditing();   
            
            var guids = AssetDatabase.FindAssets("t:GameActionData", new[] { oldDir });
            int success = 0, fail = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith(oldDir)) continue;
                var old = AssetDatabase.LoadAssetAtPath<GameActionData>(path);
                if (old == null || string.IsNullOrEmpty(old.typeName)) continue;
                try
                {
                    MigrateOne(old, newDir);
                    success++;
                }
                catch (Exception e)
                {
                    Debug.LogError("Migrate failed: " + old.name + " - " + e.Message);
                    fail++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Migration] OK=" + success + " FAIL=" + fail + " - saved forcefully");
        }finally
        {
            AssetDatabase.StopAssetEditing(); 
        }

       
    }

    private static void MigrateOne(GameActionData old, string newDir)
    {
        var tn = old.typeName;
        if (!_nodeTypeCache.TryGetValue(tn, out var nt))
            _nodeTypeCache.TryGetValue(tn + "Node", out nt);

        if (nt == null)
            throw new Exception("No node for " + tn);

        var node = (ActionNode)Activator.CreateInstance(nt);
        PopulateNodeFields(node, old._parameters, nt);

        var np = Path.Combine(newDir, old.name + ".asset").Replace("\\", "/");

        // 先尝试按正确类型加载
        var existing = AssetDatabase.LoadAssetAtPath<GameActionAsset>(np);

        if (existing != null)
        {
            // 正常资产：原地覆盖，保留 GUID
            existing.id = old.id;
            existing.name = old.name;
            existing.root = node;

            EditorUtility.SetDirty(existing);
            return;
        }

        // 到这里有两种情况：
        // 1. 文件根本不存在
        // 2. 文件存在，但脚本丢失 / 类型不对 / YAML 损坏，导致无法按 GameActionAsset 加载

        if (File.Exists(np))
        {
            Debug.LogWarning("[Migration] Broken asset exists, delete file before recreate: " + np);

            // 关键：不要只依赖 AssetDatabase.DeleteAsset，因为 Missing Script 的坏资产有时 Load 不出来
            FileUtil.DeleteFileOrDirectory(np);

            // 如果你不需要保留旧 GUID，可以顺手删 meta
            // FileUtil.DeleteFileOrDirectory(np + ".meta");

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        var asset = ScriptableObject.CreateInstance<GameActionAsset>();
        asset.id = old.id;
        asset.name = old.name;
        asset.root = node;

        AssetDatabase.CreateAsset(asset, np);
        EditorUtility.SetDirty(asset);

        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(np, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

        var created = AssetDatabase.LoadAssetAtPath<GameActionAsset>(np);
        if (created == null)
        {
            throw new Exception("CreateAsset failed or asset still cannot load as GameActionAsset: " + np);
        }

        var script = MonoScript.FromScriptableObject(created);
        if (script == null)
        {
            throw new Exception(
                "Created asset has no valid m_Script. Check GameActionAsset class location/definition: " + np);
        }
    }

    private static void PopulateNodeFields(ActionNode node, List<Parameter> parameters, Type nodeType)
    {
        if (parameters == null || parameters.Count == 0) return;

        if (node is ContainerNode container)
        {
            if (node is WaitActionNode waitNode && parameters.Count > 0)
            {
                var dp = parameters[0];
                int.TryParse(dp.value, out var d);
                waitNode.delay = d;
                MigrateChildren(container, dp.parameters);
            }
            else
            {
                foreach (var p in parameters)
                    MigrateChildFromParameter(container, p);
            }
        }
        else
        {
            var fields = nodeType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.DeclaringType != typeof(ActionNode) && f.DeclaringType != typeof(ContainerNode))
                .ToList();
            for (int i = 0; i < fields.Count && i < parameters.Count; i++)
            {
                var cv = ConvertValue(parameters[i].value, fields[i].FieldType);
                if (cv != null) fields[i].SetValue(node, cv);
            }
        }
    }

    private static void MigrateChildFromParameter(ContainerNode parent, Parameter p)
    {
        Type st = null;
        _nodeTypeCache.TryGetValue(p.value, out st);
        if (st == null) _nodeTypeCache.TryGetValue(p.value + "Node", out st);
        if (st == null) return;

        var sn = (ActionNode)Activator.CreateInstance(st);
        PopulateNodeFields(sn, p.parameters, st);
        if (parent.children == null) parent.children = new List<ActionEntry>();
        parent.children.Add(new ActionEntry { node = sn });
    }

    private static void MigrateChildren(ContainerNode parent, List<Parameter> children)
    {
        if (children == null) return;
        foreach (var p in children)
            MigrateChildFromParameter(parent, p);
    }

    private static object ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value)) return DefaultFor(targetType);
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

        if (targetType == typeof(string)) return value;
        if (targetType == typeof(Vector2))
        {
            var p = value.Split(',');
            if (p.Length >= 2 && float.TryParse(p[0], out var x) && float.TryParse(p[1], out var y))
                return new Vector2(x, y);
        }

        if (targetType == typeof(Vector3))
        {
            var p = value.Split(',');
            if (p.Length >= 3 && float.TryParse(p[0], out var x) && float.TryParse(p[1], out var y) &&
                float.TryParse(p[2], out var z)) return new Vector3(x, y, z);
        }

        if (targetType == typeof(Vector2Int))
        {
            var p = value.Split(',');
            if (p.Length >= 2 && int.TryParse(p[0], out var x) && int.TryParse(p[1], out var y))
                return new Vector2Int(x, y);
        }

        if (targetType == typeof(Vector3Int))
        {
            var p = value.Split(',');
            if (p.Length >= 3 && int.TryParse(p[0], out var x) && int.TryParse(p[1], out var y) &&
                int.TryParse(p[2], out var z)) return new Vector3Int(x, y, z);
        }

        if (targetType.IsEnum)
        {
            int.TryParse(value, out var ei);
            return Enum.ToObject(targetType, ei);
        }

        return DefaultFor(targetType);
    }

    private static object DefaultFor(Type t)
    {
        if (t.IsValueType) return Activator.CreateInstance(t);
        if (t == typeof(string)) return "";
        return null;
    }

    private static void BuildNodeCache()
    {
        if (_nodeTypeCache != null) return;
        _nodeTypeCache = new Dictionary<string, Type>();
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name.StartsWith("Unity") || asm.GetName().Name.StartsWith("System")) continue;
            try
            {
                foreach (var t in asm.GetTypes())
                {
                    if (t.IsSubclassOf(typeof(ActionNode)) && !t.IsAbstract)
                    {
                        _nodeTypeCache[t.Name.Replace("Node", "")] = t;
                        _nodeTypeCache[t.Name] = t;
                    }
                }
            }
            catch
            {
            }
        }

        Debug.Log("[Migration] Cache: " + _nodeTypeCache.Count + " types");
    }
}