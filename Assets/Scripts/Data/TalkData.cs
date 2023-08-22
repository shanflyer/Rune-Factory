using System.Collections;
using UnityEngine;

public enum TalkerDir
{
    左,右,无
}
[CreateAssetMenu(menuName ="Data/对话数据")]
public class TalkData : ScriptableObject, IGameData
{
    public int id;
    public bool myTalk;
    public string talkerName;
    public string talkerIconPath;
    public Sprite talkerIcon;
    public TalkerDir talkerDir;
    public bool clearTalkIcon;
    public string text;
    public int nexTalkId;
    public int actionId;
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    {
        talkerIcon = Resources.Load<Sprite>(talkerIconPath);
    }
}