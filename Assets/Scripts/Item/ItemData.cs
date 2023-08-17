using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[System.Serializable]
public enum ItemType
{
    种子 = 0,
    武器 = 1,
    防具 = 2,
    消耗物品 = 3,
    其他物品 = 4,
    食材 = 5,
    收集物 = 6,
    植物 = 7,
    食物 = 8,
    花 = 9,
    药剂 = 10,
    卷轴 = 11
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
    public string icon;
    public Sprite iconSprite;
    public string info;
    public ItemType Type;
    public int typeValue;
    public bool IsFresh;
    public int equipLevel;
    public List<int> dropEventId = new List<int>();
    public List<int> checkEventId = new List<int>();
    public List<int> useEventId = new List<int>();
    public List<int> equipEventId = new List<int>();
    public int groupCount;
    public ShopMoneyType shopMoneyType;
    public int ShopPrice, SellPrice;
    public string Text1, Text2;
    public Property property;

    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR 
    public void SetReferenceData()
    {
        string path = $"{EditorDataPath.itemIconPath}{icon}";
        iconSprite = Resources.Load<Sprite>(path);
    }
#endif
    public string GetKey()
    {
        return id.ToString();
    } 
}

