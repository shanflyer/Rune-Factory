using UnityEngine;

/// <summary>
/// GameAction 强类型基类 —— 替代旧的 GameActionData（字符串 typeName + List&lt;Parameter&gt;）
/// 
/// 每个 GameAction struct 对应一个继承此类的 ScriptableObject 子类，
/// 字段直接暴露在 Inspector 中，运行时通过 CreateAction() 零反射创建 struct。
/// </summary>
public abstract class GameActionBaseData : ScriptableObject, IGameData
{
    public int id;

    /// <summary>
    /// 从配置字段创建运行时 GameAction struct，并投入 QueueAction
    /// </summary>
    /// <param name="source">通常表示角色 ID，由行为树/事件系统动态传入</param>
    /// <param name="target">通常表示目标 ID，由行为树/事件系统动态传入</param>
    /// <param name="value">通常表示数值，由行为树/事件系统动态传入</param>
    /// <param name="setResult">回调：通知行为树执行结果</param>
    /// <param name="setValue">回调：通知行为树执行数值</param>
    /// <param name="immediately">是否立即触发（跳过队列）</param>
    /// <returns>创建的 Action struct</returns>
    public abstract GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false);

    /// <inheritdoc/>
    public string GetKey() => id.ToString();

    /// <summary>
    /// 桥接方法：与旧 GameActionData.Action() 签名兼容，供行为树/事件等直接调用
    /// </summary>
    public void Action(int source = 0, int target = 0, int value = 0,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        CreateAction(source, target, value, setResult, setValue, immediately);
    }

#if UNITY_EDITOR
    /// <summary>Editor 用：设置 ScriptableObject 引用</summary>
    public virtual void SetReferenceData() { }
#endif
}
