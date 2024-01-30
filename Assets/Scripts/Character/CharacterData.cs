using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
#endif

public class CharacterData : ScriptableObject, IGameData
{
    public string characterName;
    public int id;
    public Gender gender;
    public string iconName;
    public string objName;
    public string headName;
    public SpriteResourceRenference head;
    public Sprite icon;
    public GameObject obj;
    public int profession;
    public int level;
    public string behavior;
    public string fightBehavior;
    public int packageId;
    public AttributeType attributeType;
    public int playerOperateEventId;
    public int nextTalkEventId;
    public int tempTalkEventId;
    public string shopName;
    public List<int> functionIds;

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
    private static Dictionary<string, Sprite> iconDatas = new Dictionary<string, Sprite>();

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
            var sprites = Resources.LoadAll<Sprite>(iconName.Split('/')[0]);
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

        obj = Resources.Load<GameObject>(objName);
    }

#endif
}