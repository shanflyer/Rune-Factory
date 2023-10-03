using System.Collections;
using UnityEngine;

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
    public PackageType packageType;

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

     
}