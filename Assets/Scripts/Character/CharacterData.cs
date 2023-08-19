using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sex
{
    Male=1,Female=2
}
public class CharacterData : ScriptableObject, IGameData
{ 
    public string characterName;
    public int id;
    public Gender gender;
    public string icon;
    public string obj;
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
}
