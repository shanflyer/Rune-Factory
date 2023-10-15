using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class NPCFunctionData : ScriptableObject, IGameData,IReferenceData
{
    public int id;
    public string npcFunctionName;
    public string iconName;
    public Sprite icon;
    public int checkAction;
    public int OperateAction;
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
    public void SetReferenceData()
    {
        throw new NotImplementedException();
    }
}