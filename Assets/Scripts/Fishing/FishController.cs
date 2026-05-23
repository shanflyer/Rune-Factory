using System.Collections.Generic;
using Unity.Mathematics; 
using UnityEngine;

public class FishController : Singleton<FishController>
{
    Dictionary<int, FisherRuntime> Fishers = new Dictionary<int, FisherRuntime>();
    public override bool NeedUpdate => true;
    private FishTool fishTool;
    private System.Threading.Tasks.Task initializationTask = System.Threading.Tasks.Task.CompletedTask;
    public override System.Threading.Tasks.Task InitializationTask => initializationTask;

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async System.Threading.Tasks.Task InitAsync()
    {
        GameActionManager.instance.AddListener<PlayFishWater>(PlayFishWater);
        GameActionManager.instance.AddListener<CreatFisher>(CreatFisher);
        GameActionManager.instance.AddListener<RecycleFisher>(RecycleFisher);
        GameActionManager.instance.AddListener<TryGetFish>(TryGetFish);
        GameActionManager.instance.AddListener<NPCFishingResult>(NPCFishingResult);
        GameActionManager.instance.AddListener<StartFishingGame>(StartFishingGame);
        fishTool = await GameSourceManager.instance.GetComponent<FishTool>(DataPath.fishToolPrefab);
        if (fishTool == null)
        {
            Debug.LogError($"FishController init failed: missing fish tool prefab '{DataPath.fishToolPrefab}'.");
        }
    }

    void StartFishingGame(StartFishingGame startFishingGame)
    {
        if(Fishers.TryGetValue(startFishingGame.characterId,out var fisherRuntime))
        {
          
            if (fisherRuntime != null && fisherRuntime.fishTool != null)
            {
                fisherRuntime.fishTool.StartFishing();
            }

        }
    }
     
    void NPCFishingResult(NPCFishingResult nPCFishingResult)
    {
        if(Fishers.TryGetValue(nPCFishingResult.characterId,out var fisherRuntime))
        {

            //垂钓结果表情
            if (nPCFishingResult.success)
            {
                ShowEmote showEmote = new ShowEmote
                {
                    emoteId = GameCommon.fishSuccessEmote,
                    showTime = 1000,
                    entityType = EntityType.角色,
                    id = nPCFishingResult.characterId
                };
                GameActionManager.instance.QueueAction(showEmote);
            }
            else
            {
                ShowEmote showEmote = new ShowEmote
                {
                    emoteId = GameCommon.fishFailedmote,
                    showTime = 1000,
                    entityType = EntityType.角色,
                    id = nPCFishingResult.characterId
                };
                GameActionManager.instance.QueueAction(showEmote);
            }
            SetCharacterAnimator setCharacterAnimator = new SetCharacterAnimator
            {
                characterId = nPCFishingResult.characterId,
                parameterType = ParameterType.BOOL,
                parameter = "Fish",
                boolValue = false
            };
            GameActionManager.instance.QueueAction(setCharacterAnimator);
            fisherRuntime.Clear();
            Fishers.Remove(nPCFishingResult.characterId);
        }
    }
    void TryGetFish(TryGetFish tryGetFish)
    {
        if (Fishers.TryGetValue(tryGetFish.characterInstance,out var fisherRuntime))
        {
            fisherRuntime.fishTool.StopFishing();
            FishingIsSuccess fishingIsSuccess = new FishingIsSuccess
            {
                characterId = tryGetFish.characterInstance,
                isSuccess = fisherRuntime.fishTool.isGetFish,
                fishValue=fisherRuntime.fishTool.FishValue,
                pondData=fisherRuntime.fishPondData
            };
            GameActionManager.instance.QueueAction(fishingIsSuccess); 
        }
       
    }
    void RecycleFisher(RecycleFisher recycleFisher)
    {
        if(Fishers.TryGetValue(recycleFisher.characterInstance,out var fisherRuntime))
        {
            fisherRuntime.Clear();
            Fishers.Remove(recycleFisher.characterInstance);
        }
    }
    async void CreatFisher(CreatFisher creatFisher)
    {
        if (!Fishers.ContainsKey(creatFisher.characterInstance))
        {
            Character character = CharacterManager.instance.GetCharacter(creatFisher.characterInstance);
            if (character != null)
            {
                if (CharacterManager.instance.GetRuntimeCharacterObj(creatFisher.characterInstance, out var characterRuntimeObj))
                {
                    RuntimeObj runtimeObj =await GameRuntimeObjManager.instance.CreatRuntimeObj<FishTool>(RuntimeObjType.FISHTOOL.ToString(), "Fisher", fishTool, creatFisher.characterInstance);
                  
                    Vector3 pos = GameCommon.fishToolOffsets[character.direction];
                    pos += characterRuntimeObj.transform.position;



                    FisherRuntime fisher = new FisherRuntime(runtimeObj, creatFisher.characterInstance, character.mapInstance,creatFisher.pondData); 
                    fisher.SetToolPos(pos);

                    Fishers.Add(creatFisher.characterInstance, fisher);
                    Fishers[creatFisher.characterInstance] = fisher;
                }
             
            }
        }
    }
    protected override void Clear()
    {
        initializationTask = System.Threading.Tasks.Task.CompletedTask;
        base.Clear();
    }

    private void PlayFishWater(PlayFishWater playFishWater)
    {
    }

    public void RecycleFisherObj(int fisherId)
    {
    }

    public bool GetFisherToolCoordinate(int fisherid, out int3 coordinate)
    {
        coordinate = int3.zero;

        return false;
    }

    public Transform GetFishTransform(int id, out Animator animator)
    {
        animator = null;

        return null;
    }
}

public class FisherRuntime
{
    public int instanceId; 
    public int roomId;

    public RuntimeObj runtimeObj;
    public FishTool fishTool;
    public FishPondData fishPondData;
    public void SetToolPos(Vector3 pos)
    {
        (runtimeObj.obj as FishTool).transform.position = pos;
    }

    private TryUpDataCharacterEmote characterEmote;
    void FishGetChangeAction()
    {
        AudioController.instance.PlayAudio(SE.Fishing_FishingThrowingTrap);
        characterEmote.id = instanceId;
        characterEmote.emote = GameCommon.GetFishEmote;
        characterEmote.showTime = 2;
        GameActionManager.instance.QueueAction(characterEmote, true);
    }
    void FishNotGetChangeAction()
    {
        TryRecycleCharacterEmote tryRecycleCharacterEmote = new TryRecycleCharacterEmote { id = instanceId}; 
        GameActionManager.instance.QueueAction(tryRecycleCharacterEmote, true);
    }
    public FisherRuntime(RuntimeObj runtimeObj, int characterId, int roomId, FishPondData fishPondData)
    {
        instanceId = characterId;
        this.runtimeObj = runtimeObj;
        this.roomId = roomId;
        if (runtimeObj == null || runtimeObj.obj == null)
        {
            fishTool = null;
        }
        else
        {
            fishTool = runtimeObj.obj as FishTool;
            fishTool.GetChangeAction = FishGetChangeAction;
            fishTool.NotGetChangeAction = FishNotGetChangeAction;
        }
        this.fishPondData = fishPondData;
    }
    public void Clear()
    {
        fishPondData = null;
        fishTool = null;
        GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj); 
    }
   
}
