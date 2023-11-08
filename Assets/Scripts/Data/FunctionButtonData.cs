using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Data/其他功能按钮数据")]
public class FunctionButtonData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string buttonName;
    public Sprite icon;
    public GameActionData gameActionData;
    public bool alwaysClosePanel;
    public override string ToString()
    {
        return id.ToString();
    }
    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    { 
    }
}