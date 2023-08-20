using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public enum Sex
{
    Male=1,Female=2
}
public class CharacterData : ScriptableObject, IGameData
{ 
    public string characterName;
    public int id;
    public Gender gender;
    public string iconName;
    public string objName;
    public string headName;
    public Sprite head;
    public Sprite icon;
    public GameObject obj;
    public int profession;
    public AttributeType attributeType;

    public string GetKey()
    {
        return id.ToString();
    }

    public override string ToString()
    {
        return characterName;
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        head = Resources.Load<Sprite>(headName);
        icon = Resources.Load<Sprite>(iconName);
        obj = Resources.Load<GameObject>(objName);
    }
#endif
}
