using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/怪物数据")]
public class MonsterData : ScriptableObject, IGameData
{
    public int id;
    public string monsterName;
    public int HP, AT, DF, Crit, Dodge;
    public string monsterDescription;
    public string monsterSource;
    public Sprite monsterSprite;
    public string objName;
    public GameObject obj;
    public List<int> skills = new List<int>();
    public int dropId;
    public int exp;
    public int behaviorId;
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        obj = Resources.Load<GameObject>($"{DataPath.monsterPrefabPath}{objName}");
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