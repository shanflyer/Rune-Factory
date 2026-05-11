using System;
using System.Collections.Generic;
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
}
