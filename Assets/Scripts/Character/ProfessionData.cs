using System.Collections;
using UnityEngine;

public class ProfessionData : ScriptableObject,IGameData
{ 
    public int id; 
    public Property ZeroProperty;

    public Property LevelAddProperty;

    public string GetKey()
    {
       return id.ToString();
    }
}