using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class EmoteRuntime
{
    public RuntimeObj runtimeObj;
    public Action waitAction;

    public void Recycle()
    {
        GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
        GameTimerController.instance.StopWaitDeleyAction(waitAction);
    }
}
public class EmoteManager : Singleton<EmoteManager>
{
    private Animator emoteAnimator;
    private static string emoteObjPath = "Prefabs/Other/emote";

    private Dictionary<int, EmoteRuntime> characterEmoteRuntimes = new Dictionary<int, EmoteRuntime>();
    private Dictionary<int, EmoteRuntime> itemEmoteRuntimes = new Dictionary<int, EmoteRuntime>();


    public override async void Init()
    {
        base.Init();
        var _emotePrefab = await GameSourceManager.instance.GetPrefab(emoteObjPath);
        emoteAnimator = _emotePrefab.GetComponent<Animator>();
        GameActionManager.instance.AddListener<ShowEmote>(ShowEmote);
    }
    
    public void TryRecycleItemEmote(int id)
    {
        if(itemEmoteRuntimes.TryGetValue(id,out var runtimeObj))
        {
            runtimeObj.Recycle();
            itemEmoteRuntimes.Remove(id);
        }
    }
    public void TryRecycleCharacterEmote(int id)
    {
        if (characterEmoteRuntimes.TryGetValue(id, out var runtimeObj))
        {
            runtimeObj.Recycle();
            characterEmoteRuntimes.Remove(id);
        }
    }

    public async Task<RuntimeObj> GetEmote(int id, Transform parent)
    {
        RuntimeObj runtimeObj =await GameRuntimeObjManager.instance.CreatRuntimeObj<Animator>(RuntimeObjType.EMOTE.ToString(),
            "emote", emoteAnimator, id, parent);
        return runtimeObj;
    }

    private async void ShowEmote(ShowEmote showEmote)
    {
        EmoteRuntime emoteRuntime=null;
        switch (showEmote.entityType)
        {
            case EntityType.角色:
                if (!characterEmoteRuntimes.TryGetValue(showEmote.id,out emoteRuntime))
                {
                   
                    if (CharacterManager.instance.GetRuntimeCharacterObj(showEmote.id, out var characterRuntimeObj))
                    {
                        emoteRuntime = new EmoteRuntime();
                        emoteRuntime.runtimeObj =await GetEmote(showEmote.emoteId, characterRuntimeObj.model);
                        characterEmoteRuntimes[showEmote.id] = emoteRuntime;
                    }
                } 
                break;

            default:

                if (!itemEmoteRuntimes.TryGetValue(showEmote.id, out emoteRuntime))
                { 
                    if (WorldMapObjManager.instance.GetRuntimeMapItemObj(showEmote.id, out var itemRuntimeObj))
                    {
                        emoteRuntime = new EmoteRuntime();
                        emoteRuntime.runtimeObj =await GetEmote(showEmote.emoteId, itemRuntimeObj.transform);
                        itemEmoteRuntimes[showEmote.id] = emoteRuntime;
                    }
                }
                break;
        }
        if (emoteRuntime == null||emoteRuntime.runtimeObj==null)
        {
            return;
        }
        var emoteData = await GameDataManager.instance.GetAsyncData<EmoteData>(showEmote.emoteId);
        Animator animator = emoteRuntime.runtimeObj.obj as Animator;
        animator.transform.localPosition = new Vector3(emoteData.X * 0.01f, emoteData.Y * 0.01f, -emoteData.Y * 0.01f);
        var playableGraph = PlayableGraph.Create();
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "emote", animator);
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, emoteData.animationClip);
        playableOutput.SetSourcePlayable(clipPlayable);
        playableGraph.Play();

        if (showEmote.showTime > 0)
        {
            Action action =WaitStopEmote;
            emoteRuntime.waitAction = action;
            GameTimerController.instance.DeleyActionMain(showEmote.showTime, action);

            void WaitStopEmote()
            {
                if (emoteRuntime.runtimeObj.obj != null)
                {
                    playableGraph.Stop();
                }

                emoteRuntime.Recycle();
                if (showEmote.entityType == EntityType.地图道具)
                {
                    itemEmoteRuntimes.Remove(showEmote.id);
                }
                else
                {
                    characterEmoteRuntimes.Remove(showEmote.id);
                }
            }
        }
        
    }
}