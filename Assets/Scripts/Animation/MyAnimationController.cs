using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Mathematics;
//using Unity.Collections;

public class MyAnimationController :Singleton<MyAnimationController>
{
    private MyNativeData<AnimationStruct> animationStructData;


    public void AddItemAnimation(int id,Animator animator,string name)
    {
        AnimationStruct animationStruct = new AnimationStruct();
        animationStruct.InitAnimator(animator, id, name);
        animationStructData.AddData(animationStruct);
    }
    public void PlayAnimation(int id,AnimationClip clip)
    {
        if(animationStructData.GetData(id,out AnimationStruct animationStruct))
        {
            animationStruct.Play(clip);
        }
    }
    public void StopAnimation(int id)
    {
        if (animationStructData.GetData(id, out AnimationStruct animationStruct))
        {
            animationStruct.Stop();
        }
    }
    public void RemoveItemAnimation(int id)
    {
        animationStructData.RemoveData(id);

         
    }
    public override void Init()
    {
        base.Init();
        animationStructData.Init(32);
    }
    public void ClearAnimation()
    {
        Clear();
    }
    protected override void Clear()
    {
        base.Clear();
        animationStructData.Dispose(); 
    }

    public struct AnimationStruct : INativeData
    {
        public int id;
        public PlayableGraph playableGraph;
        public AnimationPlayableOutput playableOutput;

        public int Key => id;

        public void InitAnimator(Animator animator,int id,string name)
        {
            this.id = id;
            playableGraph = PlayableGraph.Create();
            playableOutput = AnimationPlayableOutput.Create(playableGraph, name, animator);
        }
        public void Play(AnimationClip animationClip=null)
        {
            if (animationClip != null)
            {
                var clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);
                playableOutput.SetSourcePlayable(clipPlayable);
            } 
            playableGraph.Play();
        }
        public void Stop()
        {
            playableGraph.Stop();
        }
        
    }
     
}
