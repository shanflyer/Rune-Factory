using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum PackageType
{
    全部, 鲜活, 非鲜活,
}

public enum  MoveItemType
{
    Default=0,OnlyPut=1,OnlyGet=2
}
public class PackageSetData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string packageName;
    public int count;
    public bool canLevelUp;
    public int levelUpAddCount;
    public int levelUpCost;
    public string iconName;
    public PackageType packageType;
    public MoveItemType moveItemType; 
    public bool singleCase;
    public SpriteResourceRenference icon;
    public List<int> limitItems=new List<int> ();
    public List<int2> initItems = new List<int2>();

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
        icon = Resources.Load<SpriteResourceRenference>($"Reference/{iconName}");
    }
}