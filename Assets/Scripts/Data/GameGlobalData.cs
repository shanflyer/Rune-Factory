using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName ="Datas/全局游戏数据")]
public class GameGlobalData :ScriptableObject, IGameData
{
    public int endGuideIndex;
    public bool immediatelyAction;
    public bool Encrypt;
    public string GetKey()
    {
        return "GameGlobalData";
    }
    public override string ToString()
    {
        return "GameGlobalData";
    }
    public void SetReferenceData()
    { 
    }
}