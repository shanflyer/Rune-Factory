using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName ="Datas/全局游戏数据")]
public class GameGlobalData :ScriptableObject, IGameData
{
    [Header("最终指引")]
    public int endGuideIndex;
    [Header("是否立即执行Action")]
    public bool immediatelyAction;
    [Header("存档是否加密")]
    public bool Encrypt;
    [Header("调试")]
    public bool debug;
    public string GetKey()
    {
        return "";
    }
    public override string ToString()
    {
        return "GameGlobalData";
    }
    public void SetReferenceData()
    { 
    }
}