using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimeLineManger : Singleton<TimeLineManger>
{
   
    public override bool NeedUpdate => true;
    struct RuntimePlayable
    { 
        public List<Animator> animators;
        public List<List<AnimationParameter>> animationParameters;
        public PlayableDirector playableDirector;
        public Action StopEvent;

        private int source;

        private readonly Dictionary<RuntimeAnimatorController, Dictionary<AnimationClip, AnimationClip>>
            animatorOverrideClips;
        public RuntimePlayable(PlayableDirector playableDirector, MyTimeLineData myTimeLineData, SkillEstimateData skillEstimateData, Action StopAction,int source= -1)
        {
            animators = new List<Animator>();
            animationParameters = new List<List<AnimationParameter>>();
            this.playableDirector = playableDirector; 
            this.StopEvent = StopAction;
            this.source = source;
            animatorOverrideClips =
                new Dictionary<RuntimeAnimatorController, Dictionary<AnimationClip, AnimationClip>>();
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
                var targets = new List<Transform>();
                if (skillEstimateData != null && skillEstimateData.targets != null)
                    for (var index = 0; index < skillEstimateData.targets.Count; index++)
                    {
                        var target = FightController.instance.FindFightCharacter(skillEstimateData.targets[index]);
                        if (target != null) targets.Add(target.transform);
                    }

                int i = 0;
                while (playBindings.MoveNext())
                {
                    var sourceObject = playBindings.Current.sourceObject;
                    var bindData = i < bindDatas.Count ? bindDatas[i] : default;
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
                                    animator = characterRuntimeObj.Animator;
                                }
                                break;
                            case BindType.Default:
                                break;
                            
                        }
                        if(animator!=null)
                        { 
                            if (animator.runtimeAnimatorController!=null)
                            {
                                if (animator.runtimeAnimatorController is AnimatorOverrideController animatorController)
                                    if (!animatorOverrideClips.TryGetValue(animator.runtimeAnimatorController,
                                            out var overrideClips))
                                    {
                                        overrideClips = new Dictionary<AnimationClip, AnimationClip>();
                                        animatorOverrideClips.Add(animator.runtimeAnimatorController, overrideClips);
                                        var keyValuePairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                                        animatorController.GetOverrides(keyValuePairs);
                                        foreach (var pair in keyValuePairs) overrideClips.Add(pair.Key, pair.Value);
                                    }

                                animator.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

                                var parameters = animator.parameters;
                                var AnimationParameters = new List<AnimationParameter>();
                                for (var j = 0; j < parameters.Length; j++)
                                {
                                    var animationParameter = new AnimationParameter
                                    {
                                        parameter = parameters[j].name
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
                            }
                           
                            playableDirector.SetGenericBinding(sourceObject, animator.gameObject);
                        } 
                       
                    }

                    var type = sourceObject.GetType();
                    if (type == typeof(AnimationTrack))
                    {
                        if (type == typeof(AnimationTrack) && animator != null &&
                            animator.runtimeAnimatorController != null &&
                            animatorOverrideClips.TryGetValue(animator.runtimeAnimatorController, out var clipDic))
                            if (clipDic.Count > 0)
                            {
                                var animationTrack = (AnimationTrack)sourceObject;
                                var clips = animationTrack.GetClips().GetEnumerator();
                                while (clips.MoveNext())
                                {
                                    var clip = clips.Current;
                                    clipDic.TryGetValue(clip.assetClip, out var animationClip2);

                                    clip.overideClip = animationClip2;
                                }
                            }
                    }
                    else if (type == typeof(AudioTrack))
                    {
                        var track = (AudioTrack)sourceObject;
                        track.muted = false;
                        var matchDatas = track.matchDatas;
                        if (matchDatas.Count > 0)
                        {
                            var matchData = matchDatas.Find(m => m.key == "AttackType");
                            if (matchData.key == "AttackType")
                                if (matchData.value != attackType.ToString())
                                    track.muted = true;
                        }
                    }
                    else if (type == typeof(ControlTrack))
                    {
                        var controlTrack = (ControlTrack)sourceObject;
                        controlTrack.muted = false;
                        var matchDatas = controlTrack.matchDatas;
                        if (matchDatas.Count > 0)
                        {
                            var matchData = matchDatas.Find(m => m.key == "AttackType");
                            if (matchData.key == "AttackType")
                                if (matchData.value != attackType.ToString())
                                    controlTrack.muted = true;
                        }

                        var clips = controlTrack.GetClips().GetEnumerator();

                        var controllerIndex = 0;
                        while (clips.MoveNext())
                        {
                            var clipCurrent = clips.Current;
                            var asset = (ControlPlayableAsset)clipCurrent.asset;
                            asset.targets = targets;

                            if (bindData.bindChildren != null && bindData.bindChildren.Count > 0 &&
                                controllerIndex < bindData.bindChildren.Count)
                            {
                                GameObject childObj = null;

                                var myTrackAssetBind = bindData.bindChildren[controllerIndex];
                                switch (myTrackAssetBind.bindType)
                                {
                                    case BindType.FightSource:
                                        var childAnimator = FightController.instance.FindFightCharacter(source);
                                        if (childAnimator) childObj = childAnimator.gameObject;
                                        break;
                                    case BindType.FightTarget:
                                        if (skillEstimateData != null)
                                        {
                                            childAnimator =
                                                FightController.instance.FindFightCharacter(
                                                    skillEstimateData.targets[0]);
                                            if (childAnimator) childObj = childAnimator.gameObject;
                                        }

                                        break;
                                    case BindType.Character:
                                        if (CharacterManager.instance.GetRuntimeCharacterObj(source,
                                                out var characterRuntimeObj))
                                        {
                                            childAnimator = characterRuntimeObj.Animator;
                                            if (childAnimator) childObj = childAnimator.gameObject;
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

                            controllerIndex++;
                        }
                    }
                    else if (type == typeof(FightEventTrack))
                    {
                        var fightEventTrack = (FightEventTrack)sourceObject;
                        //fightEventTrack.skillEstimateData = skillEstimateData;
                        fightEventTrack.SetSkillEstimateData(skillEstimateData);
                    }

                    i++;
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
     
    public async void PlaySkillTimeline(int source,SkillEstimateData skillEstimateData, MyTimeLineData myTimeLineData,
        Action endAction)
    {
        var runtimeObj =await GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.PLAYABLEDIRECTOR.ToString(), "default", defaultPlayableDirector, 0);
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
    public override void Init()
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

    protected override void Update()
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