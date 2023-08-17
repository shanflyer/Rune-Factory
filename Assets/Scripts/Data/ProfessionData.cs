using System.Collections;
using UnityEngine;
public class ProfessionData : ScriptableObject,IGameData
{ 
    public int id;
    public int maxLevel;
    public CharacterProperty ZeroProperty; 
    public CharacterProperty FinalProperty;
    public int growModel;

    public string GetKey()
    {
       return id.ToString();
    }
}