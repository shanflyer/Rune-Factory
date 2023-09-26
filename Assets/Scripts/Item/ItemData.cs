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
    public string iconName;
    public Sprite icon;
    public string info;
    public ItemType type;
    public int typeValue;
    public bool isFresh;
    public int equipLevel;
    public List<int> dropEventId = new List<int>();
    public List<int> checkEventId = new List<int>();
    public List<int> useEventId = new List<int>();
    public List<int> equipEventId = new List<int>();
    public int groupCount;
    public ShopMoneyType shopMoneyType;
    public int shopPrice, sellPrice;
    public string text1, text2;
    public CharacterProperty property;

    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR 
    static Dictionary<string, Sprite> allSprites = new Dictionary<string, Sprite>();
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
        allSprites.TryGetValue(iconName, out icon);
    }
#endif
    public string GetKey()
    {
        return id.ToString();
    } 
}

