using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
 
public enum BuffActionType
{
    属性改变,伤害,回复
}
[CreateAssetMenu(menuName = "Data/Buff数据")]
public class BuffData : ScriptableObject, IGameData
{
    public int id;
    public int probability;
    public string buffName;
    public int lifeTime;
    public List<int> coverBuffs = new List<int>();
    public AttributeType attributeType;
    public BuffActionType buffactionType;
    public int2 addActionValue;
    public int2 mulActionValue;
    public Sprite icon;
    [NonSerialized] 
    private string iconName; 
    [NonSerialized] 
    private string buffObjName;
    public BuffActionBehavior buffObj;

    private string actionTimeLineDataName;
    public MyTimeLineData myTimeLineData;
    public override string ToString()
    {
        return id.ToString();
    }
    
    public string GetKey()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    static Dictionary<string, Sprite> allSprites = new Dictionary<string, Sprite>();
    public static void Clear()
    {
        allSprites.Clear();
    }

    public void SetReferenceData()
    {
        myTimeLineData = Resources.Load<MyTimeLineData>($"{DataPath.GetDataPath(typeof(MyTimeLineData))}/{actionTimeLineDataName}");
        buffObj = Resources.Load<BuffActionBehavior>($"Prefabs/Effect/{buffObjName}");
        if (allSprites.Count == 0)
        {
            var sprites = Resources.LoadAll<Sprite>($"Icon/{iconName}");
            for (int i = 0; i < sprites.Length; i++)
            {
                allSprites.Add(sprites[i].name, sprites[i]);
            }
        }

        if (!allSprites.TryGetValue(iconName, out icon))
        {

        }


    }
#endif

}