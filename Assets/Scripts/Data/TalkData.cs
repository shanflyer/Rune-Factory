using System.Collections.Generic;
using UnityEngine;

public enum TalkerDir
{
    左, 右, 无
}

public enum TalkSource
{
    Player = 1, Fixed = 2, Dynamic = 3
}

[CreateAssetMenu(menuName = "Data/对话数据")]
public class TalkData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public TalkSource talkSource;
    public string talkerName;
    public string talkerIconPath;
    public SpriteResourceRenference talkerIcon;
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
    private static Dictionary<string, SpriteResourceRenference> sources = new Dictionary<string, SpriteResourceRenference>();

    public static void Clear()
    {
        sources.Clear();
    }

    public void SetReferenceData()
    {
        if (!sources.TryGetValue(talkerIconPath, out var talkerIconRenference))
        {
            var strs = talkerIconPath.Split("/");
            var sourcePath = talkerIconPath.Substring(0, talkerIconPath.Length - strs[strs.Length - 1].Length);
            var allSources = Resources.LoadAll<SpriteResourceRenference>(sourcePath);
            for (int i = 0; i < allSources.Length; i++)
            {
                sources[$"{sourcePath}{allSources[i].name}"] = allSources[i];
            }
        }
        sources.TryGetValue(talkerIconPath, out talkerIconRenference);
        if (talkerIconRenference != null)
        {
            talkerIcon = talkerIconRenference;
        }
    }

#endif
}