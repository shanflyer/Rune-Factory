using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Text;
using LitJson;



 /*
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
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(itemId);
        name = itemData.name;
        ItemId = itemId;
        count = _count;
        groupNum = itemData.groupCount;

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
        name = _itemDataJ.name;
        ItemId = _itemDataJ.id*1000;
        count = _count;
        groupNum = _itemDataJ.groupCount;
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
                Item item = ItemManager.instance.CreatItem(_item) {count = 1};
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
                            Item newItem = ItemManager.instance.CreatItem(_item) { count = spare };
                            spare = 0;
                            items.Add(newItem);
                            break;
                        }
                        else
                        {
                            Item newItem = ItemManager.instance.CreatItem(_item) { count = _item.groupNum };
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
                            Item newItem = ItemManager.instance.CreatItem(_item) { count = spare };
                            spare = 0;
                            items.Insert(0,newItem);
                            break;
                        }
                        else
                        {
                            Item newItem = ItemManager.instance.CreatItem(_item) { count = _item.groupNum };
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
        ItemData _itemData = await GameDataManager.instance.GetAsyncData<ItemData>(_itemid);

        Item _item = ItemManager.instance.CreatItem(_itemData, 1);

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
        ItemData _itemData = await GameDataManager.instance.GetAsyncData<ItemData>(_itemid);
        
        Item _item=ItemManager.instance.CreatItem(_itemData,count);

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
                        Item newItem = ItemManager.instance.CreatItem(_item) {count = spare};
                        spare = 0;
                        items.Add(newItem);
                        break;
                    }
                    else
                    {
                        Item newItem = ItemManager.instance.CreatItem(_item) {count = _item.groupNum};
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
            return ItemDataList.Find(i => i.id == itemId);
        }
        else
        {
            return ItemDataList.Find(i => i.id == itemId / 1000);
        }
        
    }
    public Sprite GetItemIcon(int itemId)
    {
        ItemData itemData = GetItemDataFromId(itemId);
        if (itemData != null)
        {
            return GetItemIcon(itemData.icon);
        }
        return null;
    }
    public Sprite GetItemIcon(string iconName)
    {
        return itemSprites.Find(i => i.name == iconName);
    }
    public Sprite GetItemIcon(Item item)
    {
        ItemData itemData = GetItemDataFromId(item.dataId);
        if (itemData != null)
        {
            return GetItemIcon(itemData.icon);
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
            itemData.name = LanguageManage.SwitchStr(itemData.name);
            itemData.text1 = LanguageManage.SwitchStr(itemData.text1);
            itemData.text2 = LanguageManage.SwitchStr(itemData.text2);
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
            
            
        }
        else
        {
            Debug.Log(file.name+"不存在");
        }
        
        
    }
    public ItemData GetItemDataJFromId(int id)
    {
        ItemData ItemData = ItemDataList.Find(i => i.id == id);
    
        return ItemData;
    }
 
    // Update is called once per frame
    void Update () {
		
	}
}
 */