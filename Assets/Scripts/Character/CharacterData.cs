using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
#endif

public class CharacterData : ScriptableObject, IGameData
{
    public string characterName;
    public int id;
    public Gender gender;
#if UNITY_EDITOR
    [NonSerialized]
    [HideInInspector]
    private string iconName;
    [NonSerialized]
    [HideInInspector]
    private string objName;
    [NonSerialized]
    [HideInInspector]
    private string headName;
#endif

    public SpriteResourceRenference head;
    public CharacterRuntimeObj obj;
    public SpriteResourceRenference icon;
    public int profession;
    public int level;
    public string fightBehavior;
    public int packageId;
    public AttributeType attributeType;
   
   

    public string GetKey()
    {
        return id.ToString();
    }

    public override string ToString()
    {
        return GetKey();
    }

#if UNITY_EDITOR
    private static Dictionary<string, SpriteResourceRenference> headDatas = new Dictionary<string, SpriteResourceRenference>();
    private static Dictionary<string, SpriteResourceRenference> iconDatas = new Dictionary<string, SpriteResourceRenference>();

    public void SetReferenceData()
    {
        if (headDatas.Count == 0)
        {
            var sprites = Resources.LoadAll<SpriteResourceRenference>(headName.Split('/')[0]);
            for (int i = 0; i < sprites.Length; i++)
            {
                headDatas.Add(sprites[i].name, sprites[i]);
            }
        }
        if (iconDatas.Count == 0)
        {
            var sprites = Resources.LoadAll<SpriteResourceRenference>(iconName.Split('/')[0]);
            for (int i = 0; i < sprites.Length; i++)
            {
                iconDatas.Add(sprites[i].name, sprites[i]);
            }
        }
        if (!string.IsNullOrEmpty(headName))
        {
            headDatas.TryGetValue(headName.Split('/')[1], out head);
        }
        if (!string.IsNullOrEmpty(iconName))
        {
            iconDatas.TryGetValue(iconName.Split('/')[1], out icon);
        }

        obj = Resources.Load<CharacterRuntimeObj>(objName);
    }

#endif
}