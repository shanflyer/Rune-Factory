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
    public EmoteData emote;
    public void InitData(EmoteData emoteData)
    {
        emote = emoteData;
        Show();
    }
    PlayableGraph playableGraph;
    void Show()
    {
        Animator animator = runtimeObj.obj as Animator;
        animator.transform.localPosition = new Vector3(emote.X * 0.01f, emote.Y * 0.01f, -emote.Y * 0.01f);
        playableGraph = PlayableGraph.Create();
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "emote", animator);
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, emote.animationClip);
        playableOutput.SetSourcePlayable(clipPlayable);
        playableGraph.Play(); 
    }
    public void Recycle()
    {
        if (playableGraph.IsDone())
        {
            playableGraph.Stop();
            playableGraph.Destroy();
        }
       
        GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
        GameTimerController.instance.RemoveWaiter(waitAction);
        
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
        GameActionManager.instance.AddListener<ShowRandomEmote>(ShowRandomEmote);
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
    void ShowRandomEmote(ShowRandomEmote showRandomEmote)
    {
        var result = GameRandom.instance.GetRandomValue(showRandomEmote.randomId);
        var emoteId = result[0].x;
        ShowEmote(emoteId, showRandomEmote.entityType, showRandomEmote.id, showRandomEmote.showTime);
    }
    async void ShowEmote(int emoteId,EntityType entityType,int entityId,int showTime)
    {
        EmoteRuntime emoteRuntime = null;
        switch (entityType)
        {
            case EntityType.角色:
                if (!characterEmoteRuntimes.TryGetValue(entityId, out emoteRuntime))
                {

                    if (CharacterManager.instance.GetRuntimeCharacterObj(entityId, out var characterRuntimeObj))
                    {
                        emoteRuntime = new EmoteRuntime();
                        emoteRuntime.runtimeObj = await GetEmote(emoteId, characterRuntimeObj.model);
                        characterEmoteRuntimes[entityId] = emoteRuntime;
                    }
                }
                break;

            default:

                if (!itemEmoteRuntimes.TryGetValue(entityId, out emoteRuntime))
                {
                    if (WorldMapObjManager.instance.GetRuntimeMapItemObj(entityId, out var itemRuntimeObj))
                    {
                        emoteRuntime = new EmoteRuntime();
                        emoteRuntime.runtimeObj = await GetEmote(emoteId, itemRuntimeObj.transform);
                        itemEmoteRuntimes[entityId] = emoteRuntime;
                    }
                }
                break;
        }
        if (emoteRuntime == null || emoteRuntime.runtimeObj == null)
        {
            return;
        }
        var emoteData = await GameDataManager.instance.GetAsyncData<EmoteData>(emoteId);
        if (emoteData == null)
        {
            Debug.LogError($"缺少表情数据{emoteId}");
            return;
        }
        emoteRuntime.InitData(emoteData);

        if (showTime > 0)
        {
            Action action = WaitStopEmote;
            emoteRuntime.waitAction = action;
            GameTimerController.instance.DelayAction(showTime, action);

            void WaitStopEmote()
            {
                
                emoteRuntime.Recycle();
                if (entityType == EntityType.地图道具)
                {
                    itemEmoteRuntimes.Remove(entityId);
                }
                else
                {
                    characterEmoteRuntimes.Remove(entityId);
                }
            }
        }

    }
    private void ShowEmote(ShowEmote showEmote)
    {
        ShowEmote(showEmote.emoteId, showEmote.entityType, showEmote.id, showEmote.showTime); 
    }
}