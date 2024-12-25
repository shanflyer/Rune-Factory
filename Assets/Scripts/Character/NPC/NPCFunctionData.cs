using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPCFunctionData : ScriptableObject, IGameData,IReferenceData
{
    public int id;
    public string npcFunctionName;
    public string iconName;
    public Sprite icon;
    public bool closeTalk;
    public int checkAction;
    [Header("交互事件")]
    public int OperateAction;
    [Header("显示时Action")]
    public GameActionData GameActionData;
    public string GetKey()
    {
        return id.ToString();
    }
    public string GetName()
    {
        return npcFunctionName;
    }
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        string path = $"Reference/{iconName}";
       var spriteRenference= Resources.Load<SpriteResourceRenference>(path);
        icon = spriteRenference.sprite;
    }
#endif

}