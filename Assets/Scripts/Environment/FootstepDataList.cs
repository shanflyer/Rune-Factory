
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[CreateAssetMenu(menuName = "Datas/脚步声")]
public class FootstepDataList : ScriptableObject, IGameData
{
    public FootstepData[] footstepDatas;

    Dictionary<int3, List<AudioClip>> audioDic = new Dictionary<int3, List<AudioClip>>();

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
    public AudioClip GetAudioClip(int3 key)
    {
        audioDic.TryGetValue(key, out var audioClip);
        return audioClip[GameRandom.RandomInt(0,audioClip.Count)];
    }
    public string GetKey()
    {
        return "FootstepData";
    }

    public void SetReferenceData()
    { 
    }
}
[Serializable]
public struct FootstepData
{
    public string name;
    public bool isOutSide;
    public int index;
    public List<AudioClip> dryClip;
    public List<AudioClip> wetClip;
    public List<AudioClip> snowClip;
}