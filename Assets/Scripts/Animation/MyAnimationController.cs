using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;

//using Unity.Collections;

public class MyAnimationController : Singleton<MyAnimationController>
{
    private Dictionary<int,AnimationStruct> animationStructData=new Dictionary<int, AnimationStruct>();

    public void AddItemAnimation(int id, Animator animator, string name)
    {
        AnimationStruct animationStruct = new AnimationStruct();
        animationStruct.InitAnimator(animator, id, name);
        animationStructData.Add(animationStruct.Key,animationStruct);
    }

    public void PlayAnimation(int id, AnimationClip clip)
    {
        if (animationStructData.TryGetValue(id, out AnimationStruct animationStruct))
        {
            animationStruct.Play(clip);
        }
    }

    public void StopAnimation(int id)
    {
        if (animationStructData.TryGetValue(id, out AnimationStruct animationStruct))
        {
            animationStruct.Stop();
        }
    }

    public void RemoveItemAnimation(int id)
    {
        animationStructData.Remove(id);
    }

    public override void Init()
    {
        base.Init();
        animationStructData.Clear();
    }

    public void ClearAnimation()
    {
        Clear();
    }

    protected override void Clear()
    {
        base.Clear();
        animationStructData.Clear();
    }

    public class AnimationStruct : INativeData
    {
        public int id;
        public PlayableGraph playableGraph;
        public AnimationPlayableOutput playableOutput;

        public void Dispose()
        {
            playableGraph.Destroy();
        }

        public int Key => id;

        public void InitAnimator(Animator animator, int id, string name)
        {
            this.id = id;
            playableGraph.Destroy();
            playableGraph = PlayableGraph.Create();
            playableOutput = AnimationPlayableOutput.Create(playableGraph, name, animator);
        }

        public void Play(AnimationClip animationClip = null)
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