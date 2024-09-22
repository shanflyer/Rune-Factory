using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu(menuName ="Data/演绎绑定")]
public class MyTimeLineData : ScriptableObject,IGameData
{
    public PlayableAsset asset; 
    public List<BindData> bindDatas;
#if UNITY_EDITOR
    public void SetReferenceData()
    {
    }
#endif
    public string GetKey()
    {
        return name;
    }
}
[System.Serializable]
public enum BindType
{
   Default,FightSource,FightTarget,Camera,Character,Source,Target,MapItem
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