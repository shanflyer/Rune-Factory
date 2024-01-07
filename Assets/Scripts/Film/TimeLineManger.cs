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
    public override bool NeedUpdata => true;
    struct RuntimePlayable
    {
        public List<Animator> animators;
        public List<List<AnimationParameter>> animationParameters;
        public PlayableDirector playableDirector;
        public Action StopEvent;
        public RuntimePlayable(PlayableDirector playableDirector, MyTimeLineData myTimeLineData, SkillEstimateData skillEstimateData, Action StopAction,int source= -1)
        {
            animators = new List<Animator>();
            animationParameters = new List<List<AnimationParameter>>();
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
                            case BindType.Character:
                                if (CharacterManager.instance.GetRuntimeCharacterObj(source, out var characterRuntimeObj))
                                {
                                    animator = characterRuntimeObj.animator;
                                }
                                break;
                            case BindType.Default:
                                break;
                            
                        }
                        if(animator!=null)
                        {
                            if (animator.runtimeAnimatorController!=null)
                            {
                                animator.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                            }
                            var parameters = animator.parameters;
                            List<AnimationParameter> AnimationParameters = new List<AnimationParameter>();
                            for (int j = 0; j < parameters.Length; j++)
                            {
                                AnimationParameter animationParameter = new AnimationParameter
                                {
                                    parameter = parameters[j].name,

                                };
                                switch (parameters[j].type)
                                {
                                    case AnimatorControllerParameterType.Float:
                                        animationParameter.parameterType = ParameterType.FLOAT;
                                        animationParameter.floatValue = animator.GetFloat(parameters[j].name);
                                        break;
                                    case AnimatorControllerParameterType.Int:
                                        animationParameter.parameterType = ParameterType.INT;
                                        animationParameter.intValue = animator.GetInteger(parameters[j].name);
                                        break;
                                    case AnimatorControllerParameterType.Bool:
                                        animationParameter.parameterType = ParameterType.BOOL;
                                        animationParameter.boolValue = animator.GetBool(parameters[j].name);
                                        break;
                                    case AnimatorControllerParameterType.Trigger:
                                        animationParameter.parameterType = ParameterType.TRIGGER;
                                        break;
                                }
                                AnimationParameters.Add(animationParameter);
                            }
                            animationParameters.Add(AnimationParameters);

                            animators.Add(animator);
                            playableDirector.SetGenericBinding(sourceObject, animator.gameObject);
                        } 
                       
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
                        var bindData = bindDatas.Find(g => g.outName == current.name);
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
                                        case BindType.Character:
                                            if(CharacterManager.instance.GetRuntimeCharacterObj(source,out var characterRuntimeObj))
                                            {
                                                childAnimator = characterRuntimeObj.animator;
                                            } 
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
                        //fightEventTrack.skillEstimateData = skillEstimateData;
                        fightEventTrack.SetSkillEstimateData(skillEstimateData);
                    }
                }
            }

            playableDirector.stopped += StopAction;
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
                    StopAction(this.playableDirector);
                }
            }
            
        }
        public void StopAction(PlayableDirector playableDirector)
        {
           // playableDirector.time = 0;
            //playableDirector.Evaluate();
           // playableDirector.Stop();

            for(int i = 0; i < animators.Count; i++)
            {
                var animator= animators[i];
                if (animator&&animator.runtimeAnimatorController!=null)
                {
                    
                   // animator.enabled = false;
                    //animator.enabled = true;
                     
                    animator.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
                    animator.Rebind();
                    animator.enabled = false;
                    animator.enabled = true;
                    var AnimationParameters = animationParameters[i];
                    for(int j = 0; j < AnimationParameters.Count; j++)
                    {
                        AnimationParameter animationParameter = AnimationParameters[j];
                        switch (animationParameter.parameterType)
                        {
                            case ParameterType.BOOL:
                                animator.SetBool(animationParameter.parameter, animationParameter.boolValue.Value);
                                break;
                            case ParameterType.INT:
                                animator.SetInteger(animationParameter.parameter, animationParameter.intValue.Value);
                                break;
                            case ParameterType.FLOAT:
                                animator.SetFloat(animationParameter.parameter, animationParameter.floatValue.Value);
                                break; 
                        }
                    }

                }
            }

             

            if (StopEvent != null)
            {
                StopEvent();
                StopEvent = null;
            }

            playableDirector.stopped -= StopAction;

        }
    } 
    Dictionary<PlayableDirector, RuntimePlayable> runtimePlayables = new Dictionary<PlayableDirector, RuntimePlayable>();

    private PlayableDirector defaultPlayableDirector;
     
    public void PlaySkillTimeline(int source,SkillEstimateData skillEstimateData, MyTimeLineData myTimeLineData,
        Action endAction)
    {
        var runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.PLAYABLEDIRECTOR.ToString(), "default", defaultPlayableDirector, 0);
        var playableDirector = runtimeObj.obj as PlayableDirector;

        if(!playableDirector.gameObject.TryGetComponent(out MyReciver myReciver))
        {
            myReciver = playableDirector.gameObject.AddComponent<MyReciver>();
        }

        if (runtimePlayables.TryGetValue(playableDirector, out RuntimePlayable RuntimePlayable))
        {
            RuntimePlayable.StopAction(playableDirector);
            runtimePlayables.Remove(playableDirector);
        } 
        playableDirector.playableAsset = myTimeLineData.asset;
        RuntimePlayable runtimePlayable = new RuntimePlayable(playableDirector, myTimeLineData, skillEstimateData, () =>
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
            if (endAction != null)
            {
                endAction();
            }
            runtimePlayables.Remove(playableDirector);
        },source);
        runtimePlayables[playableDirector] = runtimePlayable;
        playableDirector.Play();
    } 
    public void Stop(PlayableDirector playableDirector)
    {
        if(runtimePlayables.TryGetValue(playableDirector,out RuntimePlayable runtimePlayable))
        {
            runtimePlayable.StopAction(playableDirector);
        }
    }
    public override async void Init()
    {
        defaultPlayableDirector = new GameObject("defaultPlayableDirector").AddComponent<PlayableDirector>();
        defaultPlayableDirector.playOnAwake = false;
        defaultPlayableDirector.timeUpdateMode = DirectorUpdateMode.GameTime;

        GameActionManager.instance.AddListener<PlayCharacterTimeLine>(PlayCharacterTimeLine);
       
        base.Init();
    }
    
    async void PlayCharacterTimeLine(PlayCharacterTimeLine playCharacterTimeLine)
    {
        var myTimeLineData = await GameDataManager.instance.GetAsyncData<MyTimeLineData>(playCharacterTimeLine.playName);
        if (myTimeLineData)
        {
            PlaySkillTimeline(playCharacterTimeLine.characterId, default(SkillEstimateData), myTimeLineData, null);
        }
    }
    protected override void Clear()
    {
        base.Clear();
    }

    protected override void UpData()
    {
        /*
        using (var e = runtimePlayables.GetEnumerator())
        {
            while (e.MoveNext())
            {
                e.Current.Value.Evaluate();
            }
        }*/
    } 
}