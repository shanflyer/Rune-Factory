using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 可序列化的 Action 树节点（非 ScriptableObject，内嵌在父 SO 中）
/// 使用 [SerializeReference] 实现多态序列化，支持任意深度的递归嵌套
/// </summary>
[Serializable]
public abstract class ActionNode
{
    /// <summary>从此节点创建运行时 GameAction struct</summary>
    public abstract GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false);
}

/// <summary>
/// 容器节点的子项包装（用于 List 中的多态引用）
/// </summary>
[Serializable]
public class ActionEntry
{
    [SerializeReference]
    public ActionNode node;

    public GameAction CreateAction(int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        return node?.CreateAction(source, target, value, setResult, setValue, immediately);
    }
}

/// <summary>
/// 容器类型基类：包含子动作列表
/// </summary>
[Serializable]
public abstract class ContainerNode : ActionNode
{
    public List<ActionEntry> children = new List<ActionEntry>();

    protected void ExecuteChildren(int source, int target, int value,
        SetResult setResult, SetValue setValue, bool immediately)
    {
        if (children == null) return;
        foreach (var entry in children)
            entry?.CreateAction(source, target, value, setResult, setValue, immediately);
    }
}

/// <summary>
/// 统一的 GameAction 配置资产（替代旧的 GameActionData）
/// 内部使用 [SerializeReference] 树结构，支持递归嵌套
/// </summary>
[CreateAssetMenu(menuName = "GameAction/ActionAsset")]
public class GameActionAsset : GameActionBaseData
{
    [SerializeReference]
    public ActionNode root;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        return root?.CreateAction(source, target, value, setResult, setValue, immediately);
    }

    public GameAction CreateAction(
        List<Parameter> parameters,
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        if (root == null) return null;

        var runtimeRoot = CloneNode(root);
        ApplyParameters(runtimeRoot, parameters);
        return runtimeRoot.CreateAction(source, target, value, setResult, setValue, immediately);
    }

    private static ActionNode CloneNode(ActionNode node)
    {
        if (node == null) return null;

        var clone = (ActionNode)Activator.CreateInstance(node.GetType());
        foreach (var field in GetSerializableFields(node.GetType()))
        {
            var value = field.GetValue(node);
            if (value is List<ActionEntry> entries)
            {
                field.SetValue(clone, CloneEntries(entries));
            }
            else
            {
                field.SetValue(clone, value);
            }
        }

        return clone;
    }

    private static List<ActionEntry> CloneEntries(List<ActionEntry> entries)
    {
        if (entries == null) return null;

        var result = new List<ActionEntry>(entries.Count);
        foreach (var entry in entries)
            result.Add(new ActionEntry { node = CloneNode(entry?.node) });
        return result;
    }

    private static IEnumerable<FieldInfo> GetSerializableFields(Type type)
    {
        while (type != null && type != typeof(object))
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null || field.GetCustomAttribute<SerializeReference>() != null)
                    yield return field;
            }

            type = type.BaseType;
        }
    }

    private static void ApplyParameters(ActionNode node, List<Parameter> parameters)
    {
        if (node == null || parameters == null || parameters.Count == 0) return;

        var fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.DeclaringType != typeof(ActionNode)
                        && f.DeclaringType != typeof(ContainerNode)
                        && f.Name != nameof(ContainerNode.children))
            .ToList();

        for (int i = 0; i < fields.Count && i < parameters.Count; i++)
        {
            var converted = ConvertValue(parameters[i].value, fields[i].FieldType);
            if (converted != null || !fields[i].FieldType.IsValueType)
                fields[i].SetValue(node, converted);
        }
    }

    private static object ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value)) return DefaultFor(targetType);

        value = value.Trim();
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

        var parts = value.Trim('(', ')').Split(',');
        if (targetType == typeof(Vector2))
        {
            if (parts.Length >= 2 && float.TryParse(parts[0], out var x) && float.TryParse(parts[1], out var y))
                return new Vector2(x, y);
        }

        if (targetType == typeof(Vector3))
        {
            if (parts.Length >= 3 && float.TryParse(parts[0], out var x) && float.TryParse(parts[1], out var y) && float.TryParse(parts[2], out var z))
                return new Vector3(x, y, z);
        }

        if (targetType == typeof(Vector2Int))
        {
            if (parts.Length >= 2 && int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y))
                return new Vector2Int(x, y);
        }

        if (targetType == typeof(Vector3Int))
        {
            if (parts.Length >= 3 && int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y) && int.TryParse(parts[2], out var z))
                return new Vector3Int(x, y, z);
        }

        if (targetType.IsEnum)
        {
            int.TryParse(value, out var enumValue);
            return Enum.ToObject(targetType, enumValue);
        }

        return DefaultFor(targetType);
    }

    private static object DefaultFor(Type type)
    {
        if (type == typeof(string)) return "";
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
