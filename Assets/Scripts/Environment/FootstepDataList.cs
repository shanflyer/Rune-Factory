
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[CreateAssetMenu(menuName = "Datas/脚步声")]
public class FootstepDataList : ScriptableObject, IGameData
{
    public FootstepData[] footstepDatas;

    Dictionary<int3, FootstepSource> audioDic = new Dictionary<int3, FootstepSource>(); 

    public void Init()
    {
        audioDic.Clear(); 
        for(int i = 0; i < footstepDatas.Length; i++)
        {
            audioDic.Add(new int3(footstepDatas[i].isOutSide ? 1 : 0, footstepDatas[i].index, 0), footstepDatas[i].dryClip);
            audioDic.Add(new int3(footstepDatas[i].isOutSide ? 1 : 0, footstepDatas[i].index, 1), footstepDatas[i].wetClip);
            audioDic.Add(new int3(footstepDatas[i].isOutSide ? 1 : 0, footstepDatas[i].index, 2), footstepDatas[i].snowClip); 
        }
    }
    public FootstepSource GetSource(int3 key)
    {
       if(!audioDic.TryGetValue(key, out var audioClip))
        {
            Debug.Log($"未找到脚步{key}");
        }
        return audioClip;
    }
    public string GetKey()
    {
        return "";
    }

    public void SetReferenceData()
    { 
    }
}
[Serializable]
public struct FootstepSource
{
    public Color footStepColor;
    public List<AudioClip> clips;
}

[Serializable]
public struct FootstepData
{
    public string name;
    public bool isOutSide;
    public int index;
    public FootstepSource dryClip;
    public FootstepSource wetClip;
    public FootstepSource snowClip;
}