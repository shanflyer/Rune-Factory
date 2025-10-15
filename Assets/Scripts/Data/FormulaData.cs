using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum FormulaType
{
    铸造 = 0,缝纫 = 1,书写=2,劈砍=3,纺线=4,织布=5,制革=6, 粉磨 = 10, 徒手 = 11,蒸煮 = 12,烘烤 = 13,酿造 = 14,搅拌=15
}

public class FormulaData : ScriptableObject, IGameData, IReferenceData
{
    public string formulaName;
    public int id;
    public FormulaType formulaType;
    [SerializeField]
    private List<int> Stuffs;
    [SerializeField]
    private int Product;
    public int PowerCost;
    public int produceTime;
    private ItemData _ProductItem;
    private List<ItemData> _StuffItems;
    public ItemData ProductItem => GameDataManager.instance.GetData<ItemData>(Product.ToString());

    public List<ItemData> StuffItems
    {
        get
        {
            var _StuffItems = new List<ItemData>();
            for (var i = 0; i < Stuffs.Count; i++)
                _StuffItems.Add(GameDataManager.instance.GetData<ItemData>(Stuffs[i].ToString()));
            return _StuffItems;
        }
    }
    public string GetKey()
    {
        return id.ToString();
    }
    
    public override string ToString()
    {
        return id.ToString();
    }
   
    public void SetReferenceData()
    {
    }

    public bool Check(List<int> items)
    {
        items.RemoveAll(item => item == 0);
        if (Stuffs.Count == items.Count)
        {
            List<int> stuffs = Stuffs.GetRange(0, Stuffs.Count);
            for (int i = 0; i < items.Count; i++)
            {
                int index = stuffs.FindIndex(s => s == items[i]);
                if (index < 0)
                {
                    return false;
                }
                stuffs.RemoveAt(index);
            }
            return true;
        }
        return false;
    }
}