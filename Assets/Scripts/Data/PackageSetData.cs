using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;

public enum PackageType
{
    全部, 鲜活, 非鲜活,
}
public class PackageSetData :ScriptableObject,IGameData,IReferenceData
{
    public int id;
    public string packageName;
    public int count;
    public bool canLevelUp;
    public int levelUpAddCount;
    public int levelUpCost;
    public string iconName;
    public PackageType packageType;
    public SpriteResourceRenference icon;
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