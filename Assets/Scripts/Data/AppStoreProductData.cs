 
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public enum StoreType
{
    GOOGLEPLAY,APPALE
}
public class AppStoreProductData : ScriptableObject, IGameData,IReferenceData
{
    public int id;
   
    private string iconName;
    public Sprite icon;
    public string ProductName;
    public int getDiamond;
    public string showName;
    public StoreType storeType;
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
#if UNITY_EDITOR
       var iconData = Resources.Load<SpriteResourceRenference>($"Reference/{iconName}");
        if (iconData != null)
        {
            icon = iconData.sprite;
        }
#endif
    }
}