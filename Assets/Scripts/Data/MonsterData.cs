using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/怪物数据")]
public class MonsterData : ScriptableObject, IGameData
{
    public int id;
    public string monsterName;
    public int HP, AT, DF, Lucky;
    public AttributeType attributeType;
    public string monsterDescription;
    public string SpriteName; 
    public Sprite monsterSprite;
   
    public List<int> skills = new List<int>();
    public int dropId;
    public int exp;
    public int behaviorId;
#if UNITY_EDITOR
    static Dictionary<string, Sprite> monsterSpriteDic = new Dictionary<string, Sprite>(); 
    public void SetReferenceData()
    {
        if (monsterSpriteDic.Count == 0)
        {
            var sources = AssetDatabase.LoadAllAssetsAtPath("Assets/Texture/Monster/Monster-0.png");
            foreach (var source in sources)
            {
                if (source is Sprite sprite)
                {
                    monsterSpriteDic[sprite.name] = sprite;
                }
            }
        }
        if(monsterSpriteDic.TryGetValue(SpriteName,out monsterSprite))
        {

        } 
    }
#endif
    public string GetKey()
    {
        return id.ToString() ;
    }
    public override string ToString()
    {
        return id.ToString();
    }
}