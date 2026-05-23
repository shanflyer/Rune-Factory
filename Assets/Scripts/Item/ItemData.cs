using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
#endif

[Serializable]
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
    动物=11,
    农作物=12
}
public enum ShopMoneyType
{
    金币 = 1,
    红晶 = 2
}
public enum SceneType
{
    全部,战斗, 城镇,
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
    [SerializeField]
    private string info;
    public string useInfo;
    public ItemType type;
    public int2 range;
    public int otherType;
    public int typeValue;
    public List<int> manufacture;
    public bool isFresh;
    public bool itemValue;
    public int equipLevel;
    public List<int> tag;
    public List<int> dropEventId = new List<int>();
    public List<int> checkEventId = new List<int>();
    public int useEventId;
    public int fightUseSkill;
    public List<int> equipEventId = new List<int>();
    public SceneType sceneType;
    public bool showProperty;
    public int groupCount;
    public ShopMoneyType shopMoneyType;
    public int shopPrice, sellPrice;
#if UNITY_EDITOR
    [NonSerialized]
    public int HP, MP, Power, MaxHP, MaxMP, MaxPower, AT, DF, Lucky, Speed, Other;
#endif
    [SerializeField]
    private CharacterProperty property;
    public CharacterProperty Property => property;
    public string GetInfo()
    {
        switch (type)
        {
            case ItemType.武器:
                if (attributeType != AttributeType.无)
                {
                    string str = LanguageManage.SwitchStr(info, "\n<color=red>", attributeType, " ", "属性攻击", "</color>");
                    return str;
                }
                else
                {
                    return info;
                } 
            case ItemType.防具:
                if (attributeType != AttributeType.无)
                {
                    var str = LanguageManage.SwitchStr(info, "\n<color=red>", attributeType, " ", "属性防御", "</color>");
                    return str;
                }
                else
                {
                    return info;
                }
            default:
                return info;
        }
       
    }
    public  string GetProperty()
    {
        switch (type)
        {
            case ItemType.种子:
                string outStr =LanguageManage.SwitchStr("生长时间:", growHour, "小时","  ","采摘次数:",pickTimes,  "\n","成熟时间:", fruitHour, "小时", "  ", "单次产量:", fruitCount);
                return outStr;
            case ItemType.动物:
                outStr = LanguageManage.SwitchStr("生长时间:", growHour, "天", "  ", "寿命:", animalDay, "天", "\n", "生产间隔:", fruitHour, "天", "  ", "单次产量:", fruitCount);
                return outStr;
            default:
                if (showProperty)
                {
                    return property.ToString();
                }
                else
                {

                    return property.GetItemProperty();
                }
               
        }
       
    }
    int growHour = 0;
    int fruitHour = 0;
    int fruitCount = 0;
    int pickTimes = 0;

    int animalDay = 0;
    
    public void Init()
    {
        // IGameData.Init 是同步接口，异步补充字段集中兜底，避免初始化异常丢失。
        GameActionAsyncRunner.Run(InitAsync(), nameof(ItemData));
    }

    private async Task InitAsync()
    {
        switch (type)
        {
            case ItemType.种子:
                PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(typeValue);
                if (plantData != null)
                {
                    pickTimes = plantData.pickTimes;
                    fruitCount = plantData.fruitCount;
                    for (int i = 0; i < 4; i++)
                    {
                        growHour += plantData.growthStages[i].growthHour;
                    }
                    fruitHour = plantData.growthStages[4].growthHour + plantData.growthStages[5].growthHour;
                }
                break;
            case ItemType.动物:
                AnimalData animalData = await GameDataManager.instance.GetAsyncData<AnimalData>(typeValue);
                if(animalData != null)
                {
                    growHour = animalData.growthStages[0].growthHour;
                    animalDay = animalData.growthStages[0].growthHour + animalData.growthStages[1].growthHour;
                    fruitHour = animalData.productCD;
                    fruitCount = animalData.productCount;
                }
                break;
        }
         
    }
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

