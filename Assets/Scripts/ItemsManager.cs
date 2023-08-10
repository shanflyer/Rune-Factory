using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Text;
using LitJson;
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
    花=9,
    药剂=10,
    卷轴=11
}
[System.Serializable]
public struct Property
{
    public int MaxHP,HP,AT, DF,EXP, rewardEXP, NeedEXP,Power,MaxPower, Crit, Dodge;
    public Property(int zero)
    {
        MaxHP = 0;
        HP = 0;
        AT = 0;
        DF = 0;
        EXP = 0;
        rewardEXP = 0;
        NeedEXP = 0;
        Power = 0;
        MaxPower = 0;
        Crit = 0;
        Dodge = 0;
    }

    public Property(Property _property)
    {
        MaxHP = _property.MaxHP;
        HP = _property.HP;
        AT = _property.AT;
        DF = _property.DF;
        EXP = _property.EXP;
        NeedEXP = _property.NeedEXP;
        rewardEXP = _property.rewardEXP;
        Power = _property.Power;
        MaxPower = _property.MaxPower;
        Crit = _property.Crit;
        Dodge = _property.Dodge;
    }
    public  static Property operator+(Property property0,Property property1)
    {
        Property result = new Property
        {
            MaxHP =property0.MaxHP+property1.MaxHP,
            HP = property0.HP + property1.HP,
            AT = property0.AT + property1.AT,
            DF = property0.DF + property1.DF,
            EXP = property0.EXP+property1.EXP,
            NeedEXP = property0.NeedEXP+property1.NeedEXP,
            rewardEXP = property0.rewardEXP+property1.rewardEXP,
            Power = property0.Power+property1.Power,
            MaxPower = property0.MaxPower+property1.MaxPower,
            Crit=property0.Crit+property1.Crit,
            Dodge=property0.Dodge+property1.Dodge

            
        };
        if (result.HP > result.MaxHP)
        {
            result.HP = result.MaxHP;
        }
        if (result.Power > result.MaxPower)
        {
            result.Power = result.MaxPower;
        }
        return result;
    }
    public static Property operator-(Property property0, Property property1)
    {
        Property result = new Property
        {
            MaxHP = property0.MaxHP - property1.MaxHP,
            HP = property0.HP - property1.HP,
            AT = property0.AT - property1.AT,
            DF = property0.DF -property1.DF,
            EXP = property0.EXP-property1.EXP,
            NeedEXP = property0.NeedEXP-property1.NeedEXP,
            rewardEXP = property0.rewardEXP - property1.rewardEXP,
            Power = property0.Power-property1.Power,
            MaxPower = property0.MaxPower-property1.MaxPower,
            Crit = property0.Crit - property1.Crit,
            Dodge = property0.Dodge - property1.Dodge
        };
        return result;
    }
    public int AddExp(int value,int professionId,int _level)
    {
        int Level = _level;
        EXP += value;
        ProfessionData professionData =
            GameComponentData.gameData.charactorDataAction.professionDatas0.Find(p => p.id == professionId);
        while (EXP >= NeedEXP)
        {
            EXP -= NeedEXP;
            Level++;
            Property levelProperty = professionData.GetPropertyFromLevelup(Level);
            this += levelProperty;
        }

        return Level;
    }
}

[System.Serializable]
public enum ValueType{
    AT = 1,
    DF = 2,
    MaxHp = 3,
    Hp = 4,
    Exp = 5,
    Power = 6,
    MaxPower = 7,
    Crit=8,
    Dodge=9,
}

public enum ShopMoneyType
{
    金币=1,
    红晶=2
}
[System.Serializable]
public class ItemData
{
    public string Name;
    public int Id;
    public string Icon;
    public ItemType Type;
    public bool IsFresh;
    public int typeValue;
    public int equipLevel;
    public int groupNum;
    public string Text1,Text2;
    public string PropertyValue;
    public ShopMoneyType shopMoneyType;
    public int ShopPrice, SellPrice;
    private Property property;
    public ItemData()
    {
        
    }

    public ItemData(ItemData _itemData)
    {
        Name = _itemData.Name;
        Id = _itemData.Id;
        Icon = _itemData.Icon;
        Type = _itemData.Type;
        IsFresh = _itemData.IsFresh;
        typeValue = _itemData.typeValue;
        equipLevel = _itemData.equipLevel;
        groupNum = _itemData.groupNum;
        Text1 = _itemData.Text1;
        Text2 = _itemData.Text2;
        PropertyValue = _itemData.PropertyValue;
        ShopPrice = _itemData.ShopPrice;
        SellPrice = _itemData.SellPrice;
        property = _itemData.property;
        shopMoneyType = _itemData.shopMoneyType;
    }
    public Property GetProperty()
    {
        return property;
    }
    public void InitProperty()
    {
        property=new Property(0);
        if (PropertyValue != "")
        {
            var x = PropertyValue.Split(';');
            List<List<string>> valueList = new List<List<string>>();
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != "")
                {
                    List<string> y = x[i].Split(',').ToList();
                    valueList.Add(y);
                }
                
            }
            foreach (var value in valueList)
            {
                ValueType valueType = (ValueType)(int.Parse(value[0]));
                switch (valueType)
                {
                    case ValueType.AT:
                        property.AT += int.Parse(value[1]);
                        break;
                    case ValueType.DF:
                        property.DF += int.Parse(value[1]);
                        break;
                    case ValueType.Power:
                        property.Power += int.Parse(value[1]);
                        break;
                    case ValueType.MaxPower:
                        property.MaxPower *= int.Parse(value[1]);
                        break;
                    case ValueType.Exp:
                        property.EXP += int.Parse(value[1]);
                        break;
                    case ValueType.Hp:
                        property.HP += int.Parse(value[1]);
                        break;
                    case ValueType.MaxHp:
                        property.MaxHP += int.Parse(value[1]);
                        break;
                    case ValueType.Crit:
                        property.Crit += int.Parse(value[1]);
                        break;
                    case ValueType.Dodge:
                        property.Dodge += int.Parse(value[1]);
                        break;
                    default:
                        break;
                }
            }
        }
       
    }
}
[System.Serializable]
public class Item : IComparable<Item>
{
    public string name;
    public int ItemId;
    public int count;
    public int groupNum;
    public Item() { }

    public Item(int itemId, int _count)
    {
        ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(itemId);
        name = itemData.Name;
        ItemId = itemId;
        count = _count;
        groupNum = itemData.groupNum;

    }
    public Item(Item _item)
    {
        name = _item.name;
        ItemId = _item.ItemId;
        count = _item.count;
        groupNum = _item.groupNum;
    }
    public Item(ItemData _itemDataJ,int _count)
    {
        name = _itemDataJ.Name;
        ItemId = _itemDataJ.Id*1000;
        count = _count;
        groupNum = _itemDataJ.groupNum;
    }
    public int CompareTo(Item other)
    {
        int result = other.ItemId
            .CompareTo(ItemId);
        return result;
    }
}
[System.Serializable]
public class Package
{
    public string name;
    public int CaseCount;
    public List<Item> items;
    public Package()
    {
        CaseCount = 100;
        items = new List<Item>();
    }
    public Package(int _CaseCount)
    {
        CaseCount = _CaseCount;
        items = new List<Item>();
    }
    public Package(Package _package)
    {
        if (_package != null)
        {
            CaseCount = _package.CaseCount;
            items = new List<Item>();
            foreach (var pi in _package.items)
            {
                items.Add(pi);
            }
        }
        
    }
    public void SetItemInShopPackage(int _itemId, int itemCount, bool isDispersed)
    {

        if (!isDispersed)
        {
            
            Item item = items.Find(i => i.ItemId / 1000 == _itemId / 1000);
            if (item!=null&&item.ItemId != 0)
            {
                item.count += itemCount;
            }
            else if (items.Count < CaseCount)
            {
                Item addItem = new Item
                {
                    ItemId = _itemId,
                    count = itemCount,
                };
                items.Add(addItem);

                if (this == GameComponentData.gameData.gameManager.gamePlayer.package)
                {
                    GameComponentData.gameData.charactorTitleAction.AddGetItems(addItem.ItemId/1000);
                }
            }
            else
            {
                Debug.Log("背包空间不足");
            }
        }
        else
        {
            Item item = items.Find(i => i.ItemId / 1000 == _itemId / 1000);
           // int newItemId = _itemId;
            if (items.Count < CaseCount)
            {
                if (item != null && item.ItemId != 0)
                {
                  //  newItemId=item.ItemId++;
                }
 
                Item addItem = new Item
                {
                    ItemId = _itemId,
                    count = itemCount,
                };

                items.Add(addItem);
            }
            else
            {
                Debug.Log("背包空间不足");
            }


        }

    }
    //道具进背包
    public bool IsHaveItem(int itemID)
    {
        if (itemID / 1000000 == 0)
        {
            if (items.Count > 0)
            {
                return items.Exists(i => i.ItemId / 1000 == itemID);
            }
            
        }
        else
        {
            if (items.Count > 0)
            {
                return items.Exists(i => i.ItemId / 1000 == itemID / 1000);
            }
        }
       
        return false;
    }
    public int IsHaveItem(Item _item)
    {
        if (_item.ItemId / 1000000 == 0)
        {
            if (items.Count > 0)
            {
                var x = items.FindAll(i => i.ItemId/1000 == _item.ItemId);
                if (x.Count > 0)
                {
                    int total = 0;
                    foreach (var item in x)
                    {
                        total += item.count;
                    }
                    if (total >= _item.count)
                    {
                        return _item.count;
                    }
                    else
                    {
                        return total;
                    }
                }
                else
                {
                    return 0;
                }
            }

        }
        else
        {
            if (items.Count > 0)
            {
                var x = items.FindAll(i => i.ItemId/1000 == _item.ItemId/1000);
                if (x.Count > 0)
                {
                    int total = 0;
                    foreach (var item in x)
                    {
                        total += item.count;
                    }
                    if (total >= _item.count)
                    {
                        return _item.count;
                    }
                    else
                    {
                        return total;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
        return 0;
    }
    public bool IsPackageFill(Item _item)
    {
        List<Item> _items = items.FindAll(i => i.ItemId / 1000 == _item.ItemId / 1000 && i.count < i.groupNum);
        int laveCount = _item.count;
        foreach (var item in _items)
        {
            laveCount -= (item.groupNum - item.count);
        }


        int needCaseCount = laveCount / _item.groupNum;
        int count = laveCount % _item.groupNum;
        if (count > 0)
        {
            needCaseCount++;
        }
        if ((CaseCount - items.Count) >= needCaseCount)
        {
            return true;
        }
        
        return false;
    }
    public void SetItemInParturePackage(Item _item)
    {
        for (int i = 0; i < _item.count; i++)
        {
            if (CaseCount > items.Count)
            {
                Item item = new Item(_item) {count = 1};
                items.Add(item);
            }
            else
            {
                break;
            }
        }
    }
    public int SetItemInPackage(Item _item)
    {
        if (_item != null)
        {
            if (_item.ItemId / 100000 == 0)
            {
                _item.ItemId *=1000;
            }
            if (_item.ItemId != 0)
            {
                List<Item> _items = items.FindAll(i => i.ItemId / 1000 == _item.ItemId / 1000 && i.count < i.groupNum);
                int spare = _item.count;
                foreach (var item in _items)
                {
                    if (spare > item.groupNum - item.count)
                    {
                        spare -= (item.groupNum - item.count);
                        item.count = item.groupNum;
                    }
                    else
                    {
                        item.count += spare;
                        spare = 0;
                        break;

                    }
                }
                if (spare > 0 && CaseCount > items.Count)
                {
                    int nullCaseCount = CaseCount - items.Count;
                    for (int i = 1; i <= nullCaseCount; i++)
                    {
                        if (spare <= _item.groupNum)
                        {
                            Item newItem = new Item(_item) { count = spare };
                            spare = 0;
                            items.Add(newItem);
                            break;
                        }
                        else
                        {
                            Item newItem = new Item(_item) { count = _item.groupNum };
                            spare -= _item.groupNum;
                            items.Add(newItem);
                        }
                    }
                    if (spare > 0)
                    {

                        Debug.Log("货物太多");

                    }
                }
                return spare;
            }
        }
        

        return 0;
    }
    public int SetItemInPackageFormShop(Item _item)
    {
        if (_item != null)
        {
            if (_item.ItemId / 100000 == 0)
            {
                _item.ItemId *= 1000;
            }
            if (_item.ItemId != 0)
            {
                List<Item> _items = items.FindAll(i => i.ItemId / 1000 == _item.ItemId / 1000 && i.count < i.groupNum);
                int spare = _item.count;
                foreach (var item in _items)
                {
                    if (spare > item.groupNum - item.count)
                    {
                        spare -= (item.groupNum - item.count);
                        item.count = item.groupNum;
                    }
                    else
                    {
                        item.count += spare;
                        spare = 0;
                        break;

                    }
                }
                if (spare > 0 && CaseCount > items.Count)
                {
                    int nullCaseCount = CaseCount - items.Count;
                    for (int i = 1; i <= nullCaseCount; i++)
                    {
                        if (spare <= _item.groupNum)
                        {
                            Item newItem = new Item(_item) { count = spare };
                            spare = 0;
                            items.Insert(0,newItem);
                            break;
                        }
                        else
                        {
                            Item newItem = new Item(_item) { count = _item.groupNum };
                            spare -= _item.groupNum;
                            items.Insert(0, newItem);
                        }
                    }
                    if (spare > 0)
                    {

                        Debug.Log("货物太多");

                    }
                }
                return spare;
            }
        }


        return 0;
    }
    public void ClearPackage()
    {
        items.Clear();
    }
    public void  SetItemXInPackage(int _itemid)
    {
        ItemData _itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(_itemid);

        Item _item = new Item(_itemData, 1);

        if (_item != null)
        {
            List<Item> _items = items.FindAll(i => i.ItemId / 1000 == _item.ItemId / 1000 && i.count < i.groupNum);
            if (_items.Count < 0)
            {
                items.Add(_item);
            }
        }
        
    }

    public int SetItemInPackage(int _itemid,int count)
    {
        ItemData _itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(_itemid);
        
        Item _item=new Item(_itemData,count);

        if (_item != null)
        {
            List<Item> _items = items.FindAll(i => i.ItemId / 1000 == _item.ItemId / 1000 && i.count < i.groupNum);
            int spare = _item.count;
            foreach (var item in _items)
            {
                if (spare > item.groupNum - item.count)
                {
                    spare -= (item.groupNum - item.count);
                    item.count = item.groupNum;
                }
                else
                {
                    item.count += spare;
                    spare = 0;
                    break;

                }
            }
            if (spare > 0 && CaseCount > items.Count)
            {
                int nullCaseCount = CaseCount - items.Count;
                for (int i = 1; i <= nullCaseCount; i++)
                {
                    if (spare <= _item.groupNum)
                    {
                        Item newItem = new Item(_item) {count = spare};
                        spare = 0;
                        items.Add(newItem);
                        break;
                    }
                    else
                    {
                        Item newItem = new Item(_item) {count = _item.groupNum};
                        spare -= _item.groupNum;
                        items.Add(newItem);
                    }
                }
                if (spare > 0)
                {

                    Debug.Log("货物太多");

                }
            }
            return spare;
        }

        return 0;
    }
    //道具出背包
    public void GetItemOutPackage(int _itemId, int count)
    {
        int itemId = _itemId;
        if (itemId / 100000 != 0)
        {
            itemId = itemId / 1000;
        }
        

        List<Item> _items = items.FindAll(i => i.ItemId / 1000 == itemId);
        int itemCount = 0;
        foreach (var item in _items)
        {
            itemCount += item.count;
        }
        if (itemCount >= count)
        {
            foreach (var item in _items)
            {
                if (item.count > count)
                {
                    item.count -= count;
                    break;
                }
                else
                {
                    count -= item.count;
                    item.count = 0;
                }
            }
            var zeroItems = items.FindAll(i => i.count == 0);
            for (int i = 0; i < zeroItems.Count; i++)
            {
                items.Remove(zeroItems[i]);
            }

        }
        else
        {
            Debug.Log("物品数量不足");
        }

    }
}

public class ItemsManager : MonoBehaviour {
    
    public List<ItemData> ItemDataList;
    private GameComponent GameComponentData;
    private List<Sprite> itemSprites;
    // Use this for initialization
    void Start ()
    {
       
    }
    public ItemData GetItemDataFromId(int itemId)
    {
        if (itemId / 100000 == 0)
        {
            return ItemDataList.Find(i => i.Id == itemId);
        }
        else
        {
            return ItemDataList.Find(i => i.Id == itemId / 1000);
        }
        
    }
    public Sprite GetItemIcon(int itemId)
    {
        ItemData itemData = GetItemDataFromId(itemId);
        if (itemData != null)
        {
            return GetItemIcon(itemData.Icon);
        }
        return null;
    }
    public Sprite GetItemIcon(string iconName)
    {
        return itemSprites.Find(i => i.name == iconName);
    }
    public Sprite GetItemIcon(Item item)
    {
        ItemData itemData = GetItemDataFromId(item.ItemId);
        if (itemData != null)
        {
            return GetItemIcon(itemData.Icon);
        }
        return null;
    }
    public void InitData()
    {
        itemSprites = Resources.LoadAll<Sprite>("item/").ToList();
        GameComponentData = GetComponent<GameComponent>();
        //gameManager = GameComponentData.gameManager;
        JsonToData();
        foreach (var itemData in ItemDataList)
        {
            itemData.Name = LanguageManage.SwitchStr(itemData.Name);
            itemData.Text1 = LanguageManage.SwitchStr(itemData.Text1);
            itemData.Text2 = LanguageManage.SwitchStr(itemData.Text2);
        }

    }
    public void DataToJson()
    {
        string filePath = Application.dataPath + @"/Resources/Datas/"  + "item.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        string jsonStr = JsonMapper.ToJson(ItemDataList);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset file = Resources.Load<TextAsset>("Datas/item");
        if (file != null)
        {
            ItemDataList = JsonMapper.ToObject<List<ItemData>>(file.text);
            
            foreach (var ItemData in ItemDataList)
            {
                ItemData.InitProperty();
            }
        }
        else
        {
            Debug.Log(file.name+"不存在");
        }
        
        
    }
    public ItemData GetItemDataJFromId(int id)
    {
        ItemData ItemData = ItemDataList.Find(i => i.Id == id);
    
        return ItemData;
    }
 
    // Update is called once per frame
    void Update () {
		
	}
}
