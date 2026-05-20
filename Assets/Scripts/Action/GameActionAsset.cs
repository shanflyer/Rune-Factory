using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
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

        if (ApplySpecialParameters(node, parameters)) return;

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

    private static bool ApplySpecialParameters(ActionNode node, List<Parameter> parameters)
    {
        if (node is SetCharacterAnimatorNode animatorNode)
        {
            if (parameters.Count > 0) animatorNode.characterId = ToInt(parameters[0].value);
            if (parameters.Count > 1) animatorNode.parameter = parameters[1].value;
            if (parameters.Count > 2) animatorNode.parameterType = (ParameterType)ConvertValue(parameters[2].value, typeof(ParameterType));
            if (parameters.Count > 3)
            {
                switch (animatorNode.parameterType)
                {
                    case ParameterType.BOOL:
                        animatorNode.boolValue = ToBool(parameters[3].value);
                        break;
                    case ParameterType.INT:
                        animatorNode.intValue = ToInt(parameters[3].value);
                        break;
                    case ParameterType.FLOAT:
                        animatorNode.floatValue = ToFloat(parameters[3].value);
                        break;
                }
            }
            return true;
        }

        if (node is ChangeMapNode changeMapNode)
        {
            if (parameters.Count > 2)
                changeMapNode.mapValue = new int3(ToInt(parameters[0].value), ToInt(parameters[1].value), ToInt(parameters[2].value));
            else if (parameters.Count > 0)
                changeMapNode.mapValue = (int3)ConvertValue(parameters[0].value, typeof(int3));
            return true;
        }

        if (node is LerpScreenCycleValueNode cycleNode)
        {
            if (parameters.Count > 0) cycleNode.minCycleValue = ToFloat(parameters[0].value);
            if (parameters.Count > 1) cycleNode.maxCycleValue = ToFloat(parameters[1].value);
            if (parameters.Count > 2) cycleNode.lerpTime = ToFloat(parameters[2].value);
            if (parameters.Count > 4)
                cycleNode.cyclePos = new Vector2(ToFloat(parameters[3].value), ToFloat(parameters[4].value));
            else if (parameters.Count > 3)
                cycleNode.cyclePos = (Vector2)ConvertValue(parameters[3].value, typeof(Vector2));
            return true;
        }

        if (node is SetCharacterRandomCoordinateNode randomCoordinateNode)
        {
            if (parameters.Count > 0) randomCoordinateNode.characterId = ToInt(parameters[0].value);
            if (parameters.Count >= 4)
            {
                randomCoordinateNode.Coordinate = new int2(ToInt(parameters[1].value), ToInt(parameters[2].value));
                randomCoordinateNode.range = ToInt(parameters[3].value);
            }
            else if (parameters.Count >= 3)
            {
                randomCoordinateNode.Coordinate = (int2)ConvertValue(parameters[1].value, typeof(int2));
                randomCoordinateNode.range = ToInt(parameters[2].value);
            }
            return true;
        }

        if (node is SetCharacterTriggerItemNode triggerItemNode)
        {
            if (parameters.Count >= 3)
            {
                triggerItemNode.characterId = ToInt(parameters[0].value);
                triggerItemNode.mapItemEditId = new int2(ToInt(parameters[1].value), ToInt(parameters[2].value));
            }
            return true;
        }

        if (node is SetDirectionNode directionNode)
        {
            if (parameters.Count > 2)
            {
                directionNode.characterId = ToInt(parameters[0].value);
                directionNode.direction = new float2(ToFloat(parameters[1].value), ToFloat(parameters[2].value));
            }
            else if (parameters.Count > 1)
            {
                directionNode.characterId = ToInt(parameters[0].value);
                directionNode.directionEnum = (Direction)ConvertValue(parameters[1].value, typeof(Direction));
            }
            return true;
        }

        return false;
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

        var parts = SplitParts(value);
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

        if (targetType == typeof(int2))
        {
            if (parts.Length >= 2 && int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y))
                return new int2(x, y);
        }

        if (targetType == typeof(int3))
        {
            if (parts.Length >= 3 && int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y) && int.TryParse(parts[2], out var z))
                return new int3(x, y, z);
        }

        if (targetType == typeof(float2))
        {
            if (parts.Length >= 2 && float.TryParse(parts[0], out var x) && float.TryParse(parts[1], out var y))
                return new float2(x, y);
        }

        if (targetType == typeof(List<int>))
        {
            var result = new List<int>();
            foreach (var part in parts)
            {
                if (int.TryParse(part, out var item))
                    result.Add(item);
            }
            return result;
        }

        if (targetType.IsEnum)
        {
            if (int.TryParse(value, out var enumValue))
                return Enum.ToObject(targetType, enumValue);
            try
            {
                return Enum.Parse(targetType, value);
            }
            catch
            {
                return DefaultFor(targetType);
            }
        }

        return DefaultFor(targetType);
    }

    private static string[] SplitParts(string value)
    {
        value = value.Trim();
        var openIndex = value.IndexOf('(');
        var closeIndex = value.LastIndexOf(')');
        if (openIndex >= 0 && closeIndex > openIndex)
            value = value.Substring(openIndex + 1, closeIndex - openIndex - 1);
        else
            value = value.Trim('(', ')');

        return value.Split(',').Select(part => part.Trim()).Where(part => part.Length > 0).ToArray();
    }

    private static int ToInt(string value)
    {
        int.TryParse(value, out var result);
        return result;
    }

    private static float ToFloat(string value)
    {
        float.TryParse(value, out var result);
        return result;
    }

    private static bool ToBool(string value)
    {
        return value != null && (value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1");
    }

    private static object DefaultFor(Type type)
    {
        if (type == typeof(string)) return "";
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
