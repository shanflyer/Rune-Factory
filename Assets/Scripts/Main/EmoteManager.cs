using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class EmoteManager : Singleton<EmoteManager>
{
    private Animator emoteAnimator;
    private static string emoteObjPath = "Prefabs/Other/emote";

    private Dictionary<int, RuntimeObj> characterEmoteRuntimes = new Dictionary<int, RuntimeObj>();
    private Dictionary<int, RuntimeObj> itemEmoteRuntimes = new Dictionary<int, RuntimeObj>();
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
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
            itemEmoteRuntimes.Remove(id);
        }
    }
    public void TryRecycleCharacterEmote(int id)
    {
        if (characterEmoteRuntimes.TryGetValue(id, out var runtimeObj))
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
            characterEmoteRuntimes.Remove(id);
        }
    }

    public RuntimeObj GetEmote(int id, Transform parent)
    {
        RuntimeObj runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj<Animator>(RuntimeObjType.EMOTE.ToString(),
            "emote", emoteAnimator, id, parent);
        return runtimeObj;
    }

    private async void ShowEmote(ShowEmote showEmote)
    {
        RuntimeObj runtimeObj = null;
        switch (showEmote.entityType)
        {
            case EntityType.角色:
                if (!characterEmoteRuntimes.TryGetValue(showEmote.id,out runtimeObj))
                {
                    if (CharacterManager.instance.GetRuntimeCharacterObj(showEmote.id, out var characterRuntimeObj))
                    {
                        runtimeObj = GetEmote(showEmote.emoteId, characterRuntimeObj.model);
                        characterEmoteRuntimes[showEmote.id] = runtimeObj;
                    }
                } 
                break;

            default:

                if (!itemEmoteRuntimes.TryGetValue(showEmote.id, out runtimeObj))
                {
                    if (WorldMapObjManager.instance.GetRuntimeMapItemObj(showEmote.id, out var itemRuntimeObj))
                    {
                        runtimeObj = GetEmote(showEmote.emoteId, itemRuntimeObj.transform);
                        itemEmoteRuntimes[showEmote.id] = runtimeObj;
                    }
                }
                break;
        }
        if (runtimeObj== null)
        {
            return;
        }
        var emoteData = await GameDataManager.instance.GetAsyncData<EmoteData>(showEmote.emoteId);
        Animator animator = runtimeObj.obj as Animator;
        animator.transform.localPosition = new Vector3(emoteData.X * 0.01f, emoteData.Y * 0.01f, -emoteData.Y * 0.01f);
        var playableGraph = PlayableGraph.Create();
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "emote", animator);
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, emoteData.animationClip);
        playableOutput.SetSourcePlayable(clipPlayable);
        playableGraph.Play();

        if (showEmote.showTime > 0)
        {
            GameTimerController.instance.DeleyActionMain(showEmote.showTime, () =>
            {
                if (runtimeObj.obj != null)
                {
                    playableGraph.Stop();
                }

                GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
                if (showEmote.entityType == EntityType.地图道具)
                {
                    itemEmoteRuntimes.Remove(showEmote.id);
                }
                else
                {
                    characterEmoteRuntimes.Remove(showEmote.id);
                }
            });
        }
        
    }
}