using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
[System.Serializable]
public struct Item:IReferenceData
{
    public int instanceId;
    public int dataId;
    public int count;
    public Item(int dataId,int count)
    {
        this.dataId = dataId;
        this.count = count;
        instanceId = 0;
    }
}

public class ItemManager
{
    public static ItemManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ItemManager();
            }
            return _instance;
        }
    }
    private static ItemManager _instance;

    private HashSet<int> IntanceIds = new HashSet<int>();
    public Item CreatItem(ItemData data, int count)
    {
        Item item = new Item
        {
            dataId = data.id,
            count = count,
            instanceId = CreatIntance()
        };
        return item;
    }
    public Item CreatItem(int dataId,int count)
    {
        Item item = new Item
        {
            dataId = dataId,
            count = count,
            instanceId = CreatIntance()
        };
        return item;
    }
    public int CreatIntance()
    {
        var guid = Guid.NewGuid(); 
        int intanceId = guid.GetHashCode();
        while (IntanceIds.Contains(intanceId))
        {
            guid = Guid.NewGuid();
            intanceId = guid.GetHashCode();
        }
        IntanceIds.Add(intanceId);
        return intanceId;
    }

    public void DeleteItem(int intanceId)
    {
        if (IntanceIds.Contains(intanceId))
        {
            IntanceIds.Remove(intanceId);
        }
    }

}