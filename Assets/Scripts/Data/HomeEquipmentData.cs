using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public enum HomeEquipType
{
    生产设施, 生活设施
}
public enum HomeEquipFunc
{
    装饰=0,床=1, 箱子=2,柜台=3,生产=4
}
public class HomeEquipmentData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string equipmentName;
    public int mapItemDataId;
#if UNITY_EDITOR
    public string iconName;
#endif

    public Sprite icon;
    public HomeEquipType homeEquipType;
    public HomeEquipFunc homeEquipFunc;
    public int homeEquipFuncValue;
    public bool hide;
    public List<int> canSetMaps = new List<int>();
    public string info;

    public string GetName()
    {
        return equipmentName;
    }

    public string GetKey()
    {
        return id.ToString();
    }

    public override string ToString()
    {
        return id.ToString();
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
            var sprites = ExtensionsResources.LoadAllResource<Sprite>(EditorDataPath.itemIconPath);
            for (int i = 0; i < sprites.Length; i++)
            {
                allSprites.Add(sprites[i].name, sprites[i]);
            }
        }

        if (!allSprites.TryGetValue(iconName, out icon))
        {
            if (iconDatas.Count == 0)
            {
                var sprites = ExtensionsResources.LoadAllResource<SpriteResourceRenference>("Reference/");
                for (int i = 0; i < sprites.Length; i++)
                {
                    iconDatas.Add(sprites[i].name, sprites[i]);
                }
            }
            if (iconDatas.TryGetValue(iconName, out SpriteResourceRenference spriteResourceRenference))
            {
                icon = spriteResourceRenference.sprite;
            }
        }
    }
#endif

}
