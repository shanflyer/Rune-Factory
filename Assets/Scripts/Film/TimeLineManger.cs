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

        private int source;
        public RuntimePlayable(PlayableDirector playableDirector, MyTimeLineData myTimeLineData, SkillEstimateData skillEstimateData, Action StopAction,int source= -1)
        {
            animators = new List<Animator>();
            animationParameters = new List<List<AnimationParameter>>();
            this.playableDirector = playableDirector; 
            this.StopEvent = StopAction;
            this.source = source;
            BindPlayable(playableDirector, myTimeLineData,skillEstimateData,source);
          
        }
        void BindPlayable(PlayableDirector playableDirector, MyTimeLineData myTimeLineData, SkillEstimateData skillEstimateData,
           int source = -1)
        {
            TimelineAsset timelineAsset = (TimelineAsset)playableDirector.playableAsset;
            int attackType = FightManager.instance.GetAttackType(source);
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
                                animator = FightController.instance.FindFightCharacter(skillEstimateData.targets[0]);
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

                List<Transform> targets = new List<Transform>();
                if (skillEstimateData!=null&&skillEstimateData.targets != null)
                {
                    for (int i = 0; i < skillEstimateData.targets.Count; i++)
                    {
                        var target = FightController.instance.FindFightCharacter(skillEstimateData.targets[i]);
                        if (target != null)
                        {
                            targets.Add(target.transform);
                        }
                    }
                }
               

                while (tracks.MoveNext())
                {
                    var current = tracks.Current;
                    Type type = current.GetType();
                    if (type == typeof(AudioTrack))
                    {
                        AudioTrack track = (AudioTrack)current;
                        current.muted = false;
                        var matchDatas = track.matchDatas;
                        if (matchDatas.Count > 0)
                        {
                            var matchData = matchDatas.Find(m => m.key == "AttackType");
                            if (matchData.key == "AttackType")
                            {
                                if (matchData.value != attackType.ToString())
                                {
                                    current.muted = true;
                                }
                            }

                        }
                    }
                    else
                    if (type == typeof(ControlTrack))
                    {
                        ControlTrack controlTrack = (ControlTrack)current;
                        current.muted = false;
                        var matchDatas = controlTrack.matchDatas;
                        if (matchDatas.Count > 0)
                        {
                            var matchData = matchDatas.Find(m => m.key == "AttackType");
                            if (matchData.key == "AttackType")
                            {
                                if (matchData.value != attackType.ToString())
                                {
                                    current.muted = true;
                                }
                            }
                           
                        }

                        var clips = current.GetClips().GetEnumerator();

                        var bindData = bindDatas.Find(g => g.outName == current.name);
                        int i = 0;
                        while (clips.MoveNext())
                        {
                            var clipCurrent = clips.Current;
                            var asset = (ControlPlayableAsset)clipCurrent.asset;
                            asset.targets = targets;

                            if (bindData.bindChildren != null && bindData.bindChildren.Count > 0&& i < bindData.bindChildren.Count)
                            {
                                GameObject childObj = null;

                                var myTrackAssetBind = bindData.bindChildren[i];
                                switch (myTrackAssetBind.bindType)
                                {
                                    case BindType.FightSource:
                                        var childAnimator = FightController.instance.FindFightCharacter(source);
                                        if (childAnimator)
                                        {
                                            childObj = childAnimator.gameObject;
                                        }
                                        break;
                                    case BindType.FightTarget:
                                        if (skillEstimateData != null)
                                        {
                                            childAnimator = FightController.instance.FindFightCharacter(skillEstimateData.targets[0]);
                                            if (childAnimator)
                                            {
                                                childObj = childAnimator.gameObject;
                                            }
                                        }
                                       
                                        break;
                                    case BindType.Character:
                                        if (CharacterManager.instance.GetRuntimeCharacterObj(source, out var characterRuntimeObj))
                                        {
                                            childAnimator = characterRuntimeObj.animator;
                                            if (childAnimator)
                                            {
                                                childObj = childAnimator.gameObject;
                                            }
                                        }
                                        break;
                                    case BindType.Target:
                                        childObj = FightController.instance.GetParentObj(source, false);
                                        break;
                                    case BindType.Source:
                                        childObj = FightController.instance.GetParentObj(source, true);
                                        break;
                                }
                                if (childObj != null)
                                {
                                    var parentObj = new ExposedReference<GameObject>();
                                    parentObj.defaultValue = childObj;
                                    asset.sourceGameObject = parentObj;
                                }
                            }
                            else
                            {
                                var parentObj = new ExposedReference<GameObject>();
                                parentObj.defaultValue = skillEstimateData.target.gameObject;
                                asset.sourceGameObject = parentObj;
                            }
                            i++;
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
            PlaySkillTimeline(playCharacterTimeLine.characterId, null, myTimeLineData, null);
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