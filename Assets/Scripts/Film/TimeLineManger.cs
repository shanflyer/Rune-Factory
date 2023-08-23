using System;
using System.Collections;
using System.Collections.Generic;
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
        public RuntimePlayable(PlayableDirector playableDirector, MyTimeLineData myTimeLineData, Action StopAction)
        {
            animators = new List<Animator>();
            this.playableDirector = playableDirector;
            this.StopEvent = StopAction;
            BindPlayable(playableDirector, myTimeLineData); 
        }
        void BindPlayable(PlayableDirector playableDirector, MyTimeLineData myTimeLineData)
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
                    Animator animator;
                    GameObject bindObj=null;
                    if (streamName == bindData.outName)
                    {
                        switch (bindData.bindType)
                        {
                            case BindType.FightPlayer:
                                bindObj = FightController.instance.FindFightPlayer(bindData.bindPath);
                                break;
                            case BindType.FightMonster:
                                bindObj = FightController.instance.FindFingMoster(bindData.bindPath);
                                break;
                            
                        }
                        if(bindObj!=null&&bindObj.TryGetComponent(out animator))
                        {
                            animator.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                            animators.Add(animator);
                        }
                        playableDirector.SetGenericBinding(sourceObject, bindObj);
                    }
                     
                        i++;
                }
            }
            
            /*
            using(var tracks = timelineAsset.GetOutputTracks().GetEnumerator())
            {
                while (tracks.MoveNext())
                {
                    var current = tracks.Current;
                    if (current.GetType() == typeof(ControlTrack))
                    {

                        ControlTrack controlTrack = (ControlTrack)current;
                        controlTrack.SetRotAngle(angle);
                        controlTrack.SetParentObj(model ? model.gameObject : sourceObj);


                        var bindData = data.pathBindDatas.Find(g => g.clipName == current.name);
                        if (bindData.myTrackAssetBinds != null && bindData.myTrackAssetBinds.Count > 0)
                        {
                            var clips = current.GetClips().GetEnumerator();

                            int i = 0;
                            while (clips.MoveNext())
                            {
                                var clipCurrent = clips.Current;
                                if (i < bindData.myTrackAssetBinds.Count)
                                {
                                    MyTrackAssetBind myTrackAssetBind = bindData.myTrackAssetBinds[i];
                                    if (!string.IsNullOrEmpty(myTrackAssetBind.path))
                                    {
                                        Transform child = controller.transform.Find(myTrackAssetBind.path);

                                        if (child != null)
                                        {
                                            var asset = (ControlPlayableAsset)clipCurrent.asset;
                                            var parentObj = new ExposedReference<GameObject>();
                                            parentObj.defaultValue = child.gameObject;
                                            asset.sourceGameObject = parentObj;

                                        }
                                    }
                                    else
                                    {
                                        var asset = (ControlPlayableAsset)clipCurrent.asset;
                                        var parentObj = new ExposedReference<GameObject>();
                                        parentObj.defaultValue = model.gameObject;
                                        asset.sourceGameObject = parentObj;
                                    }

                                }
                                i++;
                            }

                        }


                    }
                }
            }
            */
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
    public async void PlayTimeLine(string name,PlayableDirector playableDirector,Action endAction)
    {
        if (runtimePlayables.TryGetValue(playableDirector,out RuntimePlayable RuntimePlayable))
        {
            RuntimePlayable.StopAction();
            runtimePlayables.Remove(playableDirector);
        }

        MyTimeLineData myTimeLineData = await GameDataManager.instance.GetAsyncData<MyTimeLineData>(name);
        playableDirector.playableAsset = myTimeLineData.asset;
        RuntimePlayable runtimePlayable = new RuntimePlayable(playableDirector, myTimeLineData, () => 
        {
            if (endAction!=null)
            {
                endAction();
            }
            runtimePlayables.Remove(playableDirector);
        });
        runtimePlayables[playableDirector]=runtimePlayable;

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

    public void UpData()
    {
        using(var e = runtimePlayables.GetEnumerator())
        {
            while (e.MoveNext())
            {
                e.Current.Value.Evaluate();
            }
        } 
    }
}