using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimeLineManger : Singleton<TimeLineManger>
{
    struct RuntimePlayable
    {
        public List<Animator> animators;
        public PlayableDirector playableDirector;

        public RuntimePlayable(PlayableDirector playableDirector, MyTimeLineData myTimeLineData)
        {
            animators = new List<Animator>();
            this.playableDirector = playableDirector;


            
        }
        public void UpData()
        {
            playableDirector.time += Time.deltaTime;
            playableDirector.Evaluate();
        }
    }

    public async void PlayTimeLine(string name,PlayableDirector playableDirector,Animator animator)
    {
        MyTimeLineData myTimeLineData = await GameDataManager.instance.GetAsyncObjectData<MyTimeLineData>(name);
        playableDirector.playableAsset = myTimeLineData.asset;

        
    }
    public override void Init()
    {
        base.Init();
    }
    protected override void Clear()
    {
        base.Clear();
    }

    public void UpData()
    {

    }
}