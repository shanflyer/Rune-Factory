using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

 
public class TimeLineManger : Singleton<TimeLineManger>
{
    struct RuntimePlayable
    {
        public List<Animator> animators;
        public PlayableDirector playableDirector;
        public Action StopEvent;
        public RuntimePlayable(PlayableDirector playableDirector, MyTimeLineData myTimeLineData, SkillEstimateData skillEstimateData, Action StopAction,int source= -1)
        {
            animators = new List<Animator>();
            this.playableDirector = playableDirector;
            this.StopEvent = StopAction;
            BindPlayable(playableDirector, myTimeLineData,skillEstimateData,source); 
        }
        void BindPlayable(PlayableDirector playableDirector, MyTimeLineData myTimeLineData, SkillEstimateData skillEstimateData,
            int source = -1)
        {
            TimelineAsset timelineAsset = (TimelineAsset)playableDirector.playableAsset;

            var bindDatas= myTimeLineData.bindDatas; 
            using(var playBindings = timelineAsset.outputs.GetEnumerator())
            {
                int i = 0;
                while (playBindings.MoveNext() && i < bindDatas.Count)
                {
                    Object sourceObject = playBindings.Current.sourceObject;
                    var bindData = bindDatas[i];
                    string streamName = playBindings.Current.streamName;
                    Animator animator=null; 
                    if (streamName == bindData.outName)
                    {
                        switch (bindData.bindType)
                        {
                            case BindType.FightSource:
                                animator = FightController.instance.FindFightCharacter(source);
                                break;
                            case BindType.FightTarget:
                                animator = FightController.instance.FindFightCharacter(skillEstimateData.target[0][0]);
                                break;
                            
                        }
                        if(animator!=null)
                        {
                            animator.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                            animators.Add(animator);
                        }
                        playableDirector.SetGenericBinding(sourceObject, animator.gameObject);
                    }
                     
                        i++;
                }
            }
             
            using(var tracks = timelineAsset.GetOutputTracks().GetEnumerator())
            {
                while (tracks.MoveNext())
                {
                    var current = tracks.Current;
                    Type type = current.GetType();
                    if (type == typeof(ControlTrack))
                    {
                        ControlTrack controlTrack = (ControlTrack)current; 
                        var bindData = bindDatas.Find(g => g.bindPath == current.name);
                        if (bindData.bindChildren != null && bindData.bindChildren.Count > 0)
                        {
                            var clips = current.GetClips().GetEnumerator();

                            int i = 0;
                            while (clips.MoveNext())
                            {
                                var clipCurrent = clips.Current;
                                if (i < bindData.bindChildren.Count)
                                {
                                    Animator childAnimator=null;
                                    var myTrackAssetBind = bindData.bindChildren[i];
                                    switch (myTrackAssetBind.bindType)
                                    {
                                        case BindType.FightSource:
                                            childAnimator = FightController.instance.FindFightCharacter(source);
                                            break;
                                        case BindType.FightTarget:
                                            childAnimator = FightController.instance.FindFightCharacter(skillEstimateData.target[0][0]);
                                            break;
                                    }
                                    if (childAnimator != null)
                                    {
                                        var asset = (ControlPlayableAsset)clipCurrent.asset;
                                        var parentObj = new ExposedReference<GameObject>();
                                        parentObj.defaultValue = childAnimator.gameObject;
                                        asset.sourceGameObject = parentObj;
                                    }  
                                }
                                i++;
                            }

                        }
                    }
                    else if(type==typeof(FightEventTrack))
                    {
                        FightEventTrack fightEventTrack = (FightEventTrack)current;
                        fightEventTrack.skillEstimateData = skillEstimateData;
                    }
                }
            }
       
        }
        public void Evaluate()
        {
            playableDirector.time += Time.deltaTime;
            playableDirector.Evaluate();
            if (playableDirector.time >= playableDirector.duration)
            {
                if (playableDirector.extrapolationMode == DirectorWrapMode.Loop)
                {
                    playableDirector.time = 0;
                }
                else
                {
                    StopAction();
                }
            }
            
        }
        public void StopAction()
        {
            playableDirector.time = 0;
            playableDirector.Evaluate();
            playableDirector.Stop();

            for(int i = 0; i < animators.Count; i++)
            {
                var animator= animators[i];
                if (animator)
                {
                    animator.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
                    animator.Rebind();
                }
            }

             

            if (StopEvent != null)
            {
                StopEvent();
                StopEvent = null;
            }



        }
    } 
    Dictionary<PlayableDirector, RuntimePlayable> runtimePlayables = new Dictionary<PlayableDirector, RuntimePlayable>();
   public void PlaySkillTimeline(int source,SkillEstimateData skillEstimateData, MyTimeLineData myTimeLineData,
       PlayableDirector playableDirector, Action endAction)
    {
        if (runtimePlayables.TryGetValue(playableDirector, out RuntimePlayable RuntimePlayable))
        {
            RuntimePlayable.StopAction();
            runtimePlayables.Remove(playableDirector);
        } 
        playableDirector.playableAsset = myTimeLineData.asset;
        RuntimePlayable runtimePlayable = new RuntimePlayable(playableDirector, myTimeLineData, skillEstimateData, () =>
        {
            if (endAction != null)
            {
                endAction();
            }
            runtimePlayables.Remove(playableDirector);
        });
        runtimePlayables[playableDirector] = runtimePlayable; 
    } 
    public void Stop(PlayableDirector playableDirector)
    {
        if(runtimePlayables.TryGetValue(playableDirector,out RuntimePlayable runtimePlayable))
        {
            runtimePlayable.StopAction();
        }
    }
    public override void Init()
    {
        base.Init();
    }
    protected override void Clear()
    {
        base.Clear();
    }

    protected override void UpData()
    {
        using (var e = runtimePlayables.GetEnumerator())
        {
            while (e.MoveNext())
            {
                e.Current.Value.Evaluate();
            }
        }
    } 
}