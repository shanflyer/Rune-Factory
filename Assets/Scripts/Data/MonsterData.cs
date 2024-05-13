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
    public SpriteResourceRenference monsterSprite;
    public float scale;
    public List<int> skills = new List<int>();
    public int dropId;
    public int exp;
    public int behaviorId;
#if UNITY_EDITOR 
    static Dictionary<string, SpriteResourceRenference> monsterSpriteResourceRenferenceDic = new Dictionary<string, SpriteResourceRenference>();
    public void SetReferenceData()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            if (monsterSpriteResourceRenferenceDic.Count == 0)
            {
                monsterSpriteResourceRenferenceDic.Clear();
                var sources = AssetDatabase.LoadAllAssetsAtPath("Assets/Texture/Monster/Monster-0.png");
                foreach (var source in sources)
                {
                    if (source is Sprite sprite)
                    {
                        SpriteResourceRenference spriteResourceRenference = new SpriteResourceRenference
                        {
                            sprite = sprite,

                        };
                        AssetDatabase.CreateAsset(spriteResourceRenference, $"Assets/Resources/MonsterReference/{sprite.name}.asset");
                        monsterSpriteResourceRenferenceDic[sprite.name] = spriteResourceRenference;
                    }
                }
            }
            if (monsterSpriteResourceRenferenceDic.TryGetValue(SpriteName, out monsterSprite))
            {
                monsterSprite.scaleValue = scale;
                EditorUtility.SetDirty(monsterSprite);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
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