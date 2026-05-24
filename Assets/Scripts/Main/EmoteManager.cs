using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class EmoteRuntime
{
    public RuntimeObj runtimeObj
    {
        get=>_runtimeObj;
        set
        {
            if (!isRecycle)
            {
                _runtimeObj = value;
            }
        }
    }
    private RuntimeObj _runtimeObj;
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
        if (runtimeObj == null || runtimeObj.obj == null)
        {
            return;
        }
        Animator animator = runtimeObj.obj as Animator;
        animator.transform.localPosition = new Vector3(emote.X * 0.01f, emote.Y * 0.01f, -emote.Y * 0.01f);
        if (!playableGraph.IsValid())
        {
            playableGraph = PlayableGraph.Create();
        }
     
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "emote", animator);
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, emote.animationClip);
        playableOutput.SetSourcePlayable(clipPlayable);
        playableGraph.Play(); 
    }
    bool isRecycle = false;
    public void Recycle()
    {
        if (runtimeObj!= null){
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
        }
        if (waitAction != null)
            GameTimerController.instance.RemoveWaiter(waitAction); 
        if (playableGraph.IsValid())
        {
            playableGraph.Stop();
            playableGraph.Destroy();
        }
        runtimeObj = null;
        waitAction = null;
        isRecycle = true;

    }
    public void Dispose()
    {
        if (playableGraph.IsValid())
        { 
            playableGraph.Destroy();
        }
    }
}
public class EmoteManager : Singleton<EmoteManager>
{
    private Animator emoteAnimator;
    private static string emoteObjPath = "Prefabs/Other/emote";

    private Dictionary<int, EmoteRuntime> characterEmoteRuntimes = new Dictionary<int, EmoteRuntime>();
    private Dictionary<int, EmoteRuntime> itemEmoteRuntimes = new Dictionary<int, EmoteRuntime>();
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;

    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
        base.Clear();
        foreach(var e in characterEmoteRuntimes.Values)
        {
            e.Dispose();
        }
        foreach(var e in itemEmoteRuntimes.Values)
        {
            e.Dispose();
        }
    }
    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        var _emotePrefab = await GameSourceManager.instance.GetPrefab(emoteObjPath);
        if (_emotePrefab == null)
        {
            Debug.LogError($"EmoteManager init failed: missing prefab at {emoteObjPath}");
            return;
        }

        emoteAnimator = _emotePrefab.GetComponent<Animator>();
        if (emoteAnimator == null)
        {
            Debug.LogError($"EmoteManager init failed: prefab has no Animator at {emoteObjPath}");
            return;
        }

        GameActionManager.instance.AddAsyncListener<ShowEmote>(ShowEmoteAsync, nameof(ShowEmote));
        GameActionManager.instance.AddListener<ShowRandomEmote>(ShowRandomEmote);
        GameActionManager.instance.AddListener<TryRecycleCharacterEmote>(TryRecycleCharacterEmote);
        GameActionManager.instance.AddListener<TryRecycleItemEmote>(TryRecycleItemEmote);
        GameActionManager.instance.AddListener<TryUpDataCharacterEmote>(TryUpDataCharacterEmote);
        GameActionManager.instance.AddListener<TryUpDataItemEmote>(TryUpDataItemEmote);
    }

    void TryUpDataItemEmote(TryUpDataItemEmote TryUpDataItemEmote)
    {
        if (itemEmoteRuntimes.TryGetValue(TryUpDataItemEmote.id, out var runtimeObj))
        {
            runtimeObj.Recycle();
            itemEmoteRuntimes.Remove(TryUpDataItemEmote.id);
        }
        ShowEmote(TryUpDataItemEmote.emote, EntityType.地图道具, TryUpDataItemEmote.id, TryUpDataItemEmote.showTime);
    }
    private void TryRecycleItemEmote(TryRecycleItemEmote TryRecycleItemEmote)
    {
        if(itemEmoteRuntimes.TryGetValue(TryRecycleItemEmote.id, out var runtimeObj))
        {
            runtimeObj.Recycle();
            itemEmoteRuntimes.Remove(TryRecycleItemEmote.id);
        } 
    }
    private void TryUpDataCharacterEmote(TryUpDataCharacterEmote tryUpDataCharacterEmote)
    {
        if (characterEmoteRuntimes.TryGetValue(tryUpDataCharacterEmote.id, out var runtimeObj))
        {
            runtimeObj.Recycle();
            characterEmoteRuntimes.Remove(tryUpDataCharacterEmote.id);
        }
        ShowEmote(tryUpDataCharacterEmote.emote, EntityType.角色, tryUpDataCharacterEmote.id, tryUpDataCharacterEmote.showTime);
    }
    private void TryRecycleCharacterEmote(TryRecycleCharacterEmote TryRecycleCharacterEmote)
    {
        if (characterEmoteRuntimes.TryGetValue(TryRecycleCharacterEmote.id, out var runtimeObj))
        {
            runtimeObj.Recycle();
            characterEmoteRuntimes.Remove(TryRecycleCharacterEmote.id);
        }
    }

    private async Task<RuntimeObj> GetEmote(int id, Transform parent)
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
    void ShowEmote(int emoteId,EntityType entityType,int entityId,int showTime)
    {
        AsyncTaskRunner.Run(() => ShowEmoteAsync(emoteId, entityType, entityId, showTime), nameof(ShowEmote));
    }

    async System.Threading.Tasks.Task ShowEmoteAsync(ShowEmote showEmote)
    {
        await ShowEmoteAsync(showEmote.emoteId, showEmote.entityType, showEmote.id, showEmote.showTime);
    }

    async System.Threading.Tasks.Task ShowEmoteAsync(int emoteId,EntityType entityType,int entityId,int showTime)
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
                        characterEmoteRuntimes[entityId] = emoteRuntime;
                        emoteRuntime.runtimeObj = await GetEmote(emoteId, characterRuntimeObj.transform); 
                    }
                }
                break;

            default:

                if (!itemEmoteRuntimes.TryGetValue(entityId, out emoteRuntime))
                {
                    if (WorldMapObjManager.instance.GetRuntimeMapItemObj(entityId, out var itemRuntimeObj))
                    {
                        emoteRuntime = new EmoteRuntime();
                        itemEmoteRuntimes[entityId] = emoteRuntime;
                        emoteRuntime.runtimeObj = await GetEmote(emoteId, itemRuntimeObj.transform); 
                    }
                }
                break;
        }
        if (emoteRuntime == null || emoteRuntime.runtimeObj == null)
        {
            if (entityType == EntityType.地图道具)
            {
                if (itemEmoteRuntimes.TryGetValue(entityId, out var runtimeObj))
                {
                    runtimeObj.Recycle();
                    itemEmoteRuntimes.Remove(entityId);
                } 
            }
            else
            {
                if (characterEmoteRuntimes.TryGetValue(entityId, out var runtimeObj))
                {
                    runtimeObj.Recycle();
                    characterEmoteRuntimes.Remove(entityId);
                }
                    
            }
            emoteRuntime = null;
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
                    
                    if (itemEmoteRuntimes.TryGetValue(entityId, out var runtimeObj))
                    { 
                        itemEmoteRuntimes.Remove(entityId);
                    } 
                }
                else
                {
                    if(characterEmoteRuntimes.TryGetValue(entityId, out var runtimeObj))
                    { 
                        characterEmoteRuntimes.Remove(entityId);
                    }
                   
                } 
            }
        }

    }
    private void ShowEmote(ShowEmote showEmote)
    {
        ShowEmote(showEmote.emoteId, showEmote.entityType, showEmote.id, showEmote.showTime); 
    }
}
