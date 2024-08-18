
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
[System.Serializable]
public enum ItemType
{
    Default=-1,
    种子 = 0,
    武器 = 1,
    防具 = 2,
    食材 = 3,
    食物 = 4, 
    收集物 = 5,  
    工具=6,
    鞋子=7,
    帽子=8,
    家具 = 10, 
    动物=11
}
public enum ShopMoneyType
{
    金币 = 1,
    红晶 = 2
}
public class ItemData : ScriptableObject, IGameData
{
    public int id;
    public string itemName;
#if UNITY_EDITOR
    [NonSerialized]
    private string iconName;
#endif
    public AttributeType attributeType;
    public Sprite icon;
    public string info;
    public ItemType type;
    public int otherType;
    public int typeValue;
    public bool isFresh;
    public bool itemValue;
    public int equipLevel;
    public List<int> dropEventId = new List<int>();
    public List<int> checkEventId = new List<int>();
    public List<int> useEventId = new List<int>();
    public List<int> equipEventId = new List<int>();
    public int groupCount;
    public ShopMoneyType shopMoneyType;
    public int shopPrice, sellPrice;
#if UNITY_EDITOR
    [NonSerialized]
    public int HP, MP, Power, MaxHP, MaxMP, MaxPower, AT, DF, Lucky, Speed, Other;
#endif
    public CharacterProperty property;

    public override string ToString()
    {
        return id.ToString();
    }
    public string GetName()
    {
        return itemName;
    }
#if UNITY_EDITOR
    static Dictionary<string, Sprite> allSprites = new Dictionary<string, Sprite>();
    static Dictionary<string, SpriteResourceRenference> iconDatas = new Dictionary<string, SpriteResourceRenference>();
    public static void Clear()
    {
        allSprites.Clear();
        iconDatas.Clear();
    }
    public void SetReferenceData()
    {
        if (allSprites.Count == 0)
        {
            var sprites = Resources.LoadAll<Sprite>(EditorDataPath.itemIconPath);
            for(int i = 0; i < sprites.Length; i++)
            {
                allSprites.Add(sprites[i].name, sprites[i]);
            }
        }
      
        if (!allSprites.TryGetValue(iconName, out icon))
        {
            if (iconDatas.Count == 0)
            {
                var sprites = Resources.LoadAll<SpriteResourceRenference>("Reference/");
                for (int i = 0; i < sprites.Length; i++)
                {
                    iconDatas.Add(sprites[i].name, sprites[i]);
                } 
            }
            if(iconDatas.TryGetValue(iconName, out SpriteResourceRenference spriteResourceRenference))
            {
                icon = spriteResourceRenference.sprite;
            }
        }

        property.AT = AT;
        property.DF = DF;
        property.Power = Power;
        property.HP = HP;
        property.MP = MP;
        property.Speed = Speed;
        property.Other = Other;
        property.MaxPower = MaxPower;
        property.MaxMP = MaxMP;
        property.MaxHP = MaxHP;
        property.Lucky = Lucky;
    }
#endif
    public string GetKey()
    {
        return id.ToString();
    } 
}

