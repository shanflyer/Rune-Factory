using System.Collections;
using UnityEngine;
public class ProfessionData : ScriptableObject,IGameData
{ 
    public int id; 
    public CharacterProperty ZeroProperty;

    public CharacterProperty LevelAddProperty;

    public string GetKey()
    {
       return id.ToString();
    }
}