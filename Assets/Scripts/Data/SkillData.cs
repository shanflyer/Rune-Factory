using System.Collections;
using UnityEngine;

public class SkillData : ScriptableObject, IGameData
{
    string IGameData.GetKey()
    {
        throw new System.NotImplementedException();
    }

    void IGameData.SetReferenceData()
    {
        throw new System.NotImplementedException();
    }
}