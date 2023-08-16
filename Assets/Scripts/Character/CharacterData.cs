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
    public Sex sex;
    public string icon;

    public string GetKey()
    {
        return characterName;
    }

    public override string ToString()
    {
        return characterName;
    }
}
