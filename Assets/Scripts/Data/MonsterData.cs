using System.Collections;
using UnityEngine;

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
    public int dropId;
    public int behaviorId;
    public string GetKey()
    {
        return id.ToString() ;
    }
}