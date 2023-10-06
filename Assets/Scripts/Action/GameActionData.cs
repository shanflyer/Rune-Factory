using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/GameActionData")]
 
[System.Serializable]
public class GameActionData : ScriptableObject,IGameData
{  
    public int id; 
    public string typeName; 
    public List<Parameter> _parameters;
   
    public void Action(int source=0,int target=0)
    {
        GameActionDataManager.instance.GameAction(typeName, _parameters, source, target);
    }
    
#if UNITY_EDITOR
    public void SetReferenceData()
    {
    }
#endif
    public string GetKey()
    {
        return id.ToString();
    }
}
[System.Serializable]
public struct Parameter
{
    public string value;
    public List<Parameter> parameters;
}