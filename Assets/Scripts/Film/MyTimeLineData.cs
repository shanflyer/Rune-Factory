using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MyTimeLineData : ScriptableObject,IGameData
{
    public PlayableAsset asset;
    public List<BindData> bindDatas;

    public string GetKey()
    {
        return name;
    }
}
[System.Serializable]
public enum BindType
{
   Default,FightPlayer,FightMonster,Camera
}
[System.Serializable]
public struct BindData
{
    public string outName;
    public BindType bindType;
    public string bindPath;
    public List<BindChild> bindChildren;
}
[System.Serializable]
public struct BindChild
{
    public BindType bindType;
    public string bindPath;
}