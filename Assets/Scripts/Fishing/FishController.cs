using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FishController : Singleton<FishController>
{
    private MyInstance myInstance;
    Dictionary<int, FisherRuntime> Fishers = new Dictionary<int, FisherRuntime>();
    public override bool NeedUpdata => true;
    private Transform fishTool;

    public override async void Init()
    {
        base.Init();
        myInstance = new MyInstance();
        GameActionManager.instance.AddListener<PlayFishWater>(PlayFishWater);
        GameActionManager.instance.AddListener<CreatFisher>(CreatFisher);
        GameActionManager.instance.AddListener<RecycleFisher>(RecycleFisher);
        GameActionManager.instance.AddListener<TryGetFish>(TryGetFish);
        GameActionManager.instance.AddListener<NPCFishingResult>(NPCFishingResult);
        fishTool = await GameSourceManager.instance.GetComponent<Transform>(DataPath.fishToolPrefab);
    }

    int playerFishGetNum = 0;
    void NPCFishingResult(NPCFishingResult nPCFishingResult)
    {
        if(Fishers.TryGetValue(nPCFishingResult.characterId,out var fisherRuntime))
        {

            //垂钓结果表情
            if (nPCFishingResult.success)
            {

            }
            else
            {

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
        bool isSuccess = playerFishGetNum == 3;
        FishingIsSuccess fishingIsSuccess = new FishingIsSuccess
        {
            characterId = CharacterManager.instance.controllerCharacter.instanceId,
            isSuccess=isSuccess
        };
        GameActionManager.instance.QueueAction(fishingIsSuccess);
    }
    void RecycleFisher(RecycleFisher recycleFisher)
    {
        if(Fishers.TryGetValue(recycleFisher.characterInstance,out var fisherRuntime))
        {
            fisherRuntime.Clear();
            Fishers.Remove(recycleFisher.characterInstance);
        }
    }
    void CreatFisher(CreatFisher creatFisher)
    {
        if (!Fishers.ContainsKey(creatFisher.characterInstance))
        {
            Character character = CharacterManager.instance.GetCharacter(creatFisher.characterInstance);
            if (character != null)
            {
                FisherRuntime fisher = new FisherRuntime
                {
                    intanceId = character.instanceId,
                    roomId = character.mapInstance
                };
                if (CharacterManager.instance.GetRuntimeCharacterObj(creatFisher.characterInstance, out var characterRuntimeObj))
                {
                    RuntimeObj runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj<Transform>(RuntimeObjType.FISHTOOL.ToString(), "Fisher", fishTool, creatFisher.characterInstance);
                    fisher.runtimeObj = runtimeObj;
                    fisher.waterPs = (runtimeObj.obj as Transform).GetComponentInChildren<ParticleSystem>(true);
                    Vector3 pos = GameCommon.fishToolOffsets[character.direction];
                    pos += characterRuntimeObj.animator.transform.position;
                    fisher.SetToolPos(pos);

                }
                Fishers.Add(creatFisher.characterInstance, fisher);
                Fishers[creatFisher.characterInstance] = fisher; 
            }
        }
    }
    protected override void Clear()
    {
        base.Clear();
        myInstance.Clear();
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

public struct FisherRuntime
{
    public int intanceId;
    public ParticleSystem waterPs;
    public int roomId;

    public RuntimeObj runtimeObj;
    public void SetToolPos(Vector3 pos)
    {
        (runtimeObj.obj as Transform).position = pos;
    }


    public FisherRuntime(RuntimeObj runtimeObj, int characterId, int roomId)
    {
        intanceId = characterId;
        this.runtimeObj = runtimeObj;
        this.roomId = roomId;
        waterPs = (runtimeObj.obj as Transform).GetComponentInChildren<ParticleSystem>(true);
        runtimeObj = null;
    }
    public void Clear()
    {
        GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
        waterPs = null;
    }
    public void FishMove()
    {
        waterPs.Play();
    }
}