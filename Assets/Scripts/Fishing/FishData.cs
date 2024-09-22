using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using UnityEngine;

public class FishData:ScriptableObject,IGameData,IReferenceData
{
    public int id;
    public int itemId;
    public string fishName;
    public string icon;
    public List<int> seasons=new List<int>();
    public List<int> places = new List<int>();
    public string info;
    public string showObjName;
    public GameObject showObj;
    public string behaviorName;
    public ExternalBehaviorTree externalBehavior;
    public Sprite iconSprite;
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
    public string GetName()
    {
        return fishName;
    }

#if UNITY_EDITOR
    static Dictionary<string, Sprite> allSprites = new Dictionary<string, Sprite>();
    static Dictionary<string, SpriteResourceRenference> iconDatas = new Dictionary<string, SpriteResourceRenference>();
    public void SetReferenceData()
    {
        string fishObjPath = "Prefabs/Fish/";
        showObj = Resources.Load<GameObject>($"{fishObjPath}{showObjName}");
        externalBehavior = Resources.Load<ExternalBehaviorTree>($"Behavior/NPC/{behaviorName}");


        if (allSprites.Count == 0)
        {
            var sprites = Resources.LoadAll<Sprite>(EditorDataPath.itemIconPath);
            for (int i = 0; i < sprites.Length; i++)
            {
                allSprites.Add(sprites[i].name, sprites[i]);
            }
        }

        if (!allSprites.TryGetValue(icon, out iconSprite))
        {
            if (iconDatas.Count == 0)
            {
                var sprites = Resources.LoadAll<SpriteResourceRenference>("Reference/");
                for (int i = 0; i < sprites.Length; i++)
                {
                    iconDatas.Add(sprites[i].name, sprites[i]);
                }
            }
            if (iconDatas.TryGetValue(icon, out SpriteResourceRenference spriteResourceRenference))
            {
                iconSprite = spriteResourceRenference.sprite;
            }
        }
    }
#endif 
}