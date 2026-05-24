using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[CreateAssetMenu(menuName ="Data/物体动画数据")]
public class ItemAnimationData : ScriptableObject,IGameData
{
    public bool nativeAnimator;
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
    public void PlayAnimator(Animator animator,int2 key)
    {
        if(animationStateDataDic.TryGetValue(key,out var animationStateData))
        {
            animator.SetFloat(animationStateData.parameterName, animationStateData.transitionDuration);
        }
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
    public string parameterName;
    public float transitionDuration;
    public List<AnimationClip> clips;
}
