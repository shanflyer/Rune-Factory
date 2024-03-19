using System.Collections.Generic;
using UnityEngine;

public class ShopItemDisplayData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string itemName;
    public List<int> itemCounts = new List<int>();
    public List<string> sourceName=new List<string>();
    public List<Sprite> itemSprites = new List<Sprite>();

    public Sprite GetItemSprie(int count)
    {
        for(int i=itemCounts.Count;i>0;i--)
        {
            if (count >= itemCounts[i-1])
            {
                return itemSprites[i-1];
            }
        }

        return null;
    }
    public string GetKey()
    {
        return id.ToString();
    }

    public string GetName()
    {
        return itemName;
    }

    public override string ToString()
    {
        return id.ToString();
    }

#if UNITY_EDITOR
    static Dictionary<string, Sprite> allSprites = new Dictionary<string, Sprite>();
    public void SetReferenceData()
    {
        if (allSprites.Count == 0)
        {
            var sprites = Resources.LoadAll<Sprite>(EditorDataPath.itemIconPath);
            for (int i = 0; i < sprites.Length; i++)
            {
                allSprites.Add(sprites[i].name, sprites[i]);
            }
        }
        itemSprites.Clear();
        for(int i = 0; i < sourceName.Count; i++)
        {
            Sprite sprite = null;
            allSprites.TryGetValue(sourceName[i], out sprite);
            itemSprites.Add(sprite);
        } 
    }
#endif
}