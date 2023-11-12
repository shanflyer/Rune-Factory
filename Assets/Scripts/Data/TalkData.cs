using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum TalkerDir
{
    左,右,无
}
[CreateAssetMenu(menuName ="Data/对话数据")]
public class TalkData : ScriptableObject, IGameData,IReferenceData
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
    public bool DisplayClose;
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    static Dictionary<string, Sprite> sources = new Dictionary<string, Sprite>();
    public static void Clear()
    {
        sources.Clear();
    }
    public void SetReferenceData()
    {
        if(!sources.TryGetValue(talkerIconPath,out talkerIcon))
        {
            var strs = talkerIconPath.Split("/");
            var sourcePath = talkerIconPath.Substring(0, talkerIconPath.Length - strs[strs.Length - 1].Length);
            var allSources = Resources.LoadAll<Sprite>(sourcePath);
            for (int i = 0; i < allSources.Length; i++)
            {
                sources[$"{sourcePath}{allSources[i].name}"]= allSources[i];
            }
            sources.TryGetValue(talkerIconPath, out talkerIcon);
        } 
    }
#endif

}