using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[CreateAssetMenu(menuName ="Data/物体动画数据")]
public class ItemAnimationData : ScriptableObject,IGameData
{ 
    public List<AnimationStateData> animationStateDatas = new List<AnimationStateData>();
    public ItemAnimationDictionary animationStateDataDic=new ItemAnimationDictionary();
   // private Dictionary<int2, AnimationStateData> animationStateDataDic = new Dictionary<int2, AnimationStateData>();
    
    public void InitDic()
    {
        animationStateDataDic.Clear();
        for (int i = 0; i < animationStateDatas.Count; i++)
        {
            animationStateDataDic[animationStateDatas[i].key] = animationStateDatas[i];
        }
        /*
        if (animationStateDataDic.Count == 0)
        {
            animationStateDataDic = new Dictionary<int2, AnimationStateData>();
            for (int i = 0; i < animationStateDatas.Count; i++)
            {
                animationStateDataDic[animationStateDatas[i].key] = animationStateDatas[i];
            }
        }*/

    }
    public AnimationClip GetAnimationClip(int2 key, out int clipCount)
    {
        clipCount = 0;
        if (animationStateDataDic.TryGetValue(key, out AnimationStateData animationStateData))
        {
            List<AnimationClip> clips = animationStateData.clips;

            if (clips != null && clips.Count > 0)
            {
                clipCount = clips.Count;

                Random random = new Random((uint)GameCommon.CreateRandSeed());
                int index = random.NextInt(0, clips.Count); 
                return clips[index];
            }
        }
        return null;
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        animationStateDataDic.Clear();
        for (int i = 0; i < animationStateDatas.Count; i++)
        {
            animationStateDataDic[animationStateDatas[i].key] = animationStateDatas[i];
        }
    }
#endif
    public string GetKey()
    {
       return name;
    }
}
[System.Serializable]
public struct AnimationStateData
{
    public string stateName;
    public int2 key;
    public List<AnimationClip> clips;
}
