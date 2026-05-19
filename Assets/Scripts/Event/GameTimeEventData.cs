using UnityEngine;

#if UNITY_EDITOR
#endif

public enum TimeEventType
{
    时间序列, 苏醒序列
}

[CreateAssetMenu(menuName = "Datas/游戏时间事件数据")]
public class GameTimeEventData : ScriptableObject, IGameData
{
    public int id;
    public string text;
    public TimeEventType timeEventType;
    public bool startEnable;
    public int triggerValue;
    public int actionValue;
    public int actionCount;

    public string GetName()
    {
        return text;
    }

    public override string ToString()
    {
        return id.ToString();
    }

    public string GetKey()
    {
        return id.ToString();
    }

#if UNITY_EDITOR

    public void SetReferenceData()
    {
        string actionPath = $"Assets/Resources/Data/GameActionAssets/{name}.asset";
    }

#endif
}
