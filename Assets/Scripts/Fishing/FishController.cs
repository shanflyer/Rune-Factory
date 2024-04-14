using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.TextCore.Text;
using Object = UnityEngine.Object;

public class FishController:Singleton<FishController>
{
    MyNativeData<FishRuntime> fishRuntimes = new MyNativeData<FishRuntime>();
    Dictionary<int, List<int>> fishPondFishes = new Dictionary<int, List<int>>(); 
    Dictionary<int, CharacterRuntimeObj> fishRuntimeObjs = new Dictionary<int, CharacterRuntimeObj>(); 
    Dictionary<int, BehaviorTree> behaviorTrees = new Dictionary<int, BehaviorTree>();
    MyInstance myInstance;

    Dictionary<int,List<int>> mapFishers = new Dictionary<int, List<int>>();
    Dictionary<int, Fisher> Fishers = new Dictionary<int, Fisher>();
    Dictionary<int, int> linkFishes = new Dictionary<int, int>();
    Dictionary<int, int> tempLinkFishes = new Dictionary<int, int>();
    public override bool NeedUpdata => true;
    private GameObject FishBehavior;
    private Transform fishTool;
    public override async void Init()
    {
        base.Init();
        fishRuntimes.Init(16);
        FishBehavior = GameObject.Find("FishBehaviorManager");
        if (FishBehavior == null)
        {
            FishBehavior = new GameObject("FishBehaviorManager");
        }
        myInstance = new MyInstance();
         
        GameActionManager.instance.AddListener<CreatFish>(CreatFish);
        GameActionManager.instance.AddListener<RemoveFish>(RemoveFish);
        GameActionManager.instance.AddListener<RefreshFishPondObj>(RefreshFishPondObj);
        GameActionManager.instance.AddListener<DestoryFishPond>(DestoryFishPond);
        GameActionManager.instance.AddListener<DisplayMap>(DisplayMap);
        GameActionManager.instance.AddListener<CreatFisher>(CreatFisher);
        GameActionManager.instance.AddListener<RecycleFisher>(RecycleFisher);
        GameActionManager.instance.AddListener<LinkFisher>(LinkFisher);
        GameActionManager.instance.AddListener<PlayFishWater>(PlayFishWater);
        GameActionManager.instance.AddListener<UnLinkFisher>(UnLinkFisher);
        GameActionManager.instance.AddListener<TrueLinkFisher>(TrueLinkFisher);
        GameActionManager.instance.AddListener<TryGetFish>(TryGetFish);

        fishTool = await GameSourceManager.instance.GetComponent<Transform>(DataPath.fishToolPrefab);
    }
    protected override void Clear()
    {
        base.Clear();
        myInstance.Clear();
    }

    void PlayFishWater(PlayFishWater playFishWater)
    {
        if(Fishers.TryGetValue(playFishWater.fisherId,out var fisher))
        {
            if (fisher.waterPs != null)
            {
                fisher.waterPs.Play();
            }
        }
    }
    void UnLinkFisher(UnLinkFisher unLinkFisher)
    {
        linkFishes.Remove(unLinkFisher.fisherId);
        tempLinkFishes.Remove(unLinkFisher.fisherId); 
    }
    void TrueLinkFisher(TrueLinkFisher linkFisher)
    {
        this.linkFishes[linkFisher.fisherId] = linkFisher.fishId; 
    }
    void LinkFisher(LinkFisher linkFisher)
    {
        if(fishRuntimes.GetData(linkFisher.fishId,out var fishRuntime))
        {
            if(mapFishers.TryGetValue(fishRuntime.room,out  var fishers))
            {
                List<int> _fishers = new List<int>();
                for(int i = 0; i < fishers.Count; i++)
                {
                    if (!tempLinkFishes.ContainsKey(fishers[i]))
                    {
                        _fishers.Add(fishers[i]);
                    }
                }
                if (_fishers.Count > 0)
                {
                    int index = GameRandom.RandomInt(0, _fishers.Count);
                    int fisherId = _fishers[index];
                    if (linkFisher.trueLink)
                    {
                        this.linkFishes[fisherId] = linkFisher.fishId;
                        this.tempLinkFishes[fisherId] = linkFisher.fishId; 
                    }
                    if (linkFisher.setValue != null)
                    {
                        linkFisher.setValue(fisherId);
                    }
                    if (linkFisher.setResult != null)
                    {
                        linkFisher.setResult(true);
                    }
                }
                else
                {
                    if (linkFisher.setResult != null)
                    {
                        linkFisher.setResult(false);
                    }
                }

              
            }
            else
            {
                if (linkFisher.setResult != null)
                {
                    linkFisher.setResult(false);
                }
            }
        }
    }
    void CreatFisher(CreatFisher creatFisher)
    {
        if (!Fishers.ContainsKey(creatFisher.characterInstance))
        {
            Character character = CharacterManager.instance.GetCharacter(creatFisher.characterInstance);
            if (character != null)
            {
                Fisher fisher = new Fisher
                {
                    intanceId = character.instanceId,
                    roomId = character.mapInstance
                };
                if (CharacterManager.instance.GetRuntimeCharacterObj(creatFisher.characterInstance, out var characterRuntimeObj))
                { 
                    RuntimeObj runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj<Transform>(RuntimeObjType.FISHTOOL.ToString(), "Fisher", fishTool, creatFisher.characterInstance);
                    fisher.runtimeObj = runtimeObj;
                    fisher.waterPs= (runtimeObj.obj as Transform).GetComponentInChildren<ParticleSystem>(true);
                    Vector3 pos = GameCommon.fishToolOffsets[character.direction];
                    pos += characterRuntimeObj.animator.transform.position;
                    fisher.SetToolPos(pos);
                   
                }
                Fishers.Add(creatFisher.characterInstance, fisher);
                Fishers[creatFisher.characterInstance] = fisher;

                if (!mapFishers.TryGetValue(character.mapInstance, out var ints))
                {
                    ints = new List<int>();
                    mapFishers.Add(character.mapInstance, ints);
                }
                ints.Add(character.instanceId);
            }
        }
    }

   public void RecycleFisherObj(int fisherId)
    {
        if (Fishers.TryGetValue(fisherId, out var fisher))
        { 
            fisher.Clear();
            Fishers[fisherId] = fisher;
        }
    }
    public void DisplayFisherObj(int fisherId)
    {
        if (Fishers.TryGetValue(fisherId, out var fisher))
        {
            if (CharacterManager.instance.GetRuntimeCharacterObj(fisherId, out var characterRuntimeObj))
            {
                Character character = CharacterManager.instance.GetCharacter(fisherId);
                RuntimeObj runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj<Transform>(RuntimeObjType.FISHTOOL.ToString(), "Fisher", fishTool,
                    fisherId);
                fisher.runtimeObj = runtimeObj;
                fisher.waterPs = (runtimeObj.obj as Transform).GetComponentInChildren<ParticleSystem>(true);
                Vector3 pos = GameCommon.fishToolOffsets[character.direction];
                pos += characterRuntimeObj.animator.transform.position;
                fisher.SetToolPos(pos);
                Fishers[fisherId] = fisher;
            } 
        }
    }


    void RecycleFisher(RecycleFisher recycleFisher)
    {
        if(Fishers.TryGetValue(recycleFisher.characterInstance,out var fisher))
        {
            fisher.Clear();
            Fishers.Remove(recycleFisher.characterInstance);

            if(mapFishers.TryGetValue(fisher.roomId,out var ints))
            {
                ints.Remove(fisher.intanceId);
                if (ints.Count == 0)
                {
                    mapFishers.Remove(fisher.roomId);
                }
            } 
        }
    }

    public bool GetFisherToolCoordinate(int fisherid,out int3 coordinate)
    {
        coordinate = int3.zero;
        if(Fishers.TryGetValue(fisherid,out var fisher))
        {
            if (fisher.waterPs != null)
            {
                coordinate =new int3(GameCommon.GetMapCoordinateInt(fisher.waterPs.transform.position),fisher.roomId);
                return true;
            }
        }
        return false;
    }
    void DisplayMap(DisplayMap displayMap)
    {
        foreach(FishRuntime fishRuntime in fishRuntimes)
        {
            RefreshFish(fishRuntime.intanceId, displayMap.displayMap,  fishRuntime.pondId);
        }
    }
    void CreatFish(CreatFish creatFish)
    {
        FishPond fishPond;
        if (!FishingManager.instance.GetFishPond(creatFish.pondId, out fishPond))
        {
            return;
        }
        try
        {
            var cells = MapCellController.instance.GetItemTriggerCells(fishPond.instanceId, fishPond.room);
            int index = GameRandom.RandomInt(0, cells.Length);

            FishRuntime fishRuntime = new FishRuntime
            {
                intanceId = myInstance.CreatInstanceId(),
                dataId = creatFish.dataId,
                pondId = creatFish.pondId,
                room = creatFish.room,
                value = creatFish.fishValue,
                cell = cells[index]
            };
            fishRuntimes.SetData(fishRuntime);
            creatFish.setValue(fishRuntime.Key);
            if (!fishPondFishes.TryGetValue(creatFish.pondId, out var ints))
            {
                ints = new List<int>();
                fishPondFishes.Add(creatFish.pondId, ints);
            }
            ints.Add(fishRuntime.intanceId);
            RefreshFish(fishRuntime.intanceId, fishRuntime.room, fishRuntime.room);
        }
        catch
        {
            Debug.LogError($"CreatFish Error:{fishPond.room}");
        }
       
    } 
    void RemoveFish(RemoveFish RemoveFish)
    {
        if(fishRuntimes.GetData(RemoveFish.instanceId,out var fishRuntime))
        {
            myInstance.RemoveInstance(RemoveFish.instanceId);
            RecycleFishObj(fishRuntime.intanceId);
            fishRuntimes.RemoveData(RemoveFish.instanceId);

            if(fishPondFishes.TryGetValue(fishRuntime.pondId,out var ints))
            {
                ints.Remove(RemoveFish.instanceId);
            }

            FishingManager.instance.TryRemoveFish(fishRuntime.pondId, fishRuntime.intanceId);
        }
    }

    void RefreshFish(int instanceId,int room,int pondId)
    {
        if (room == WorldMapObjManager.instance.displayMap)
        {
            if (fishRuntimes.GetData(instanceId, out var fishRuntime))
            {
                if (!fishRuntimeObjs.ContainsKey(instanceId))
                {
                    CreatFishObjAsync(fishRuntime.dataId,fishRuntime.cell, instanceId, pondId);
                }
            }
        }
        else
        {
            RecycleFishObj(instanceId);
        }
    }

    void RefreshFishPondObj(RefreshFishPondObj RefreshFishPondObj)
    {
        if (RefreshFishPondObj.room == WorldMapObjManager.instance.displayMap)
        {
            if (fishPondFishes.TryGetValue(RefreshFishPondObj.pondId, out var ints))
            {
                for (int i = ints.Count - 1; i >= 0; i--)
                {
                    int id = ints[i];
                    if (fishRuntimes.GetData(id, out var fishRuntime))
                    {
                        if (!fishRuntimeObjs.ContainsKey(id))
                        {
                            CreatFishObjAsync(fishRuntime.dataId, fishRuntime.cell, id, RefreshFishPondObj.pondId);
                        }
                    }
                    else
                    {
                        ints.RemoveAt(i);
                    }
                }
            }
            else
            {
                List<int> fishIds = new List<int>();
                fishPondFishes[RefreshFishPondObj.pondId] = fishIds;
            }
        }
        else
        {
            if (fishPondFishes.TryGetValue(RefreshFishPondObj.pondId, out var ints))
            {
                for (int i = ints.Count - 1; i >= 0; i--)
                {
                    int id = ints[i];
                    RecycleFishObj(id);
                }
            }
            else
            {
                List<int> fishIds = new List<int>();
                fishPondFishes[RefreshFishPondObj.pondId] = fishIds;
            }
        }
    }
    async void CreatFishObjAsync(int dataId,int2 nowCell,int instanceId,int pondId)
    {
        FishPond fishPond;
        if (!FishingManager.instance.GetFishPond(pondId, out fishPond))
        {
            return;
        } 
        FishData fishData = await GameDataManager.instance.GetAsyncData<FishData>(dataId);
        var runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.CHARACTER.ToString(), fishData.name,
         fishData.showObj.transform, instanceId);
        CharacterRuntimeObj fishRuntimeObj = new CharacterRuntimeObj
        {
            runtimeObj = runtimeObj,
            model = runtimeObj.obj as Transform
        };
        fishRuntimeObj.animator = fishRuntimeObj.model.GetComponentInChildren<Animator>();
        fishRuntimeObjs[instanceId] = fishRuntimeObj;
        Vector2 pos = GameCommon.GetMapPos(nowCell);
        fishRuntimeObj.animator.transform.localPosition = pos;

        var cells = MapCellController.instance.GetItemTriggerCells(fishPond.instanceId, fishPond.room);
        AddBehavior(instanceId, fishPond.room,nowCell, cells, fishData.externalBehavior);
    }
    void RecycleFishObj(int instanceId)
    {
        if (fishRuntimeObjs.TryGetValue(instanceId, out var characterRuntimeObj))
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
            fishRuntimeObjs.Remove(instanceId);
        }
        if (behaviorTrees.TryGetValue(instanceId, out BehaviorTree behaviorTree))
        {
            Object.Destroy(behaviorTree);
        } 
    } 
    void AddBehavior(int fishId,int mapInstance,int2 nowCell, int2[] cells, ExternalBehaviorTree externalBehavior)
    {
        if (!behaviorTrees.TryGetValue(fishId, out BehaviorTree behaviorTree))
        {
            behaviorTree = FishBehavior.AddComponent<BehaviorTree>();
            behaviorTrees[fishId] = behaviorTree;
        }
        behaviorTree.StopAllTaskCoroutines();
        behaviorTree.ExternalBehavior = externalBehavior;
        behaviorTree.SetVariable("FishId", new SharedInt { Value = fishId });
        behaviorTree.SetVariable("NowCell", new SharedInt2 { Value = nowCell });
        behaviorTree.SetVariable("MapInstance", new SharedInt { Value = mapInstance });
        behaviorTree.SetVariable("Cells", new SharedInt2List { Value = cells });
        behaviorTree.RestartWhenComplete = true;
        behaviorTree.Start();
    }
    
    void DestoryFishPond(DestoryFishPond destoryFishPond)
    {
        if(fishPondFishes.TryGetValue(destoryFishPond.PondId, out var fishs))
        {
            for(int i = 0; i < fishs.Count; i++)
            {
                RecyleFish(fishs[i]);
            }
        }
    }
    void RecyleFish(int id=0)
    {
        if (fishRuntimes.RemoveData(id))
        {
            RecycleFishObj(id);
        }
    }

    async void TryGetFish(TryGetFish tryGetFish)
    {
        if(linkFishes.TryGetValue(tryGetFish.characterId,out int fishId))
        {
            if (fishRuntimes.GetData(fishId, out var fishRuntime))
            {
                FishData fishData = await GameDataManager.instance.GetAsyncData<FishData>(fishRuntime.dataId);
                Item item = new Item
                {
                    dataId = fishData.itemId,
                    count = 1,
                    value = fishRuntime.value
                };
                Character character = CharacterManager.instance.GetCharacter(tryGetFish.characterId);
                int count = await PackageManager.instance.SetItemInPackage(item, character.characterPackage);
                if (count <= 0)
                {
                    if (tryGetFish.setResult != null)
                        tryGetFish.setResult(true);
                    ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(fishData.itemId);

                    bool newRecord = false;
                    if (character == CharacterManager.instance.controllerCharacter)
                    {
                        newRecord=GameDataSaveManager.instance.SetFishSaveData(fishData.id, fishRuntime.value, character.mapInstance);
                    }


                    ItemResultInfo itemResultInfo = new ItemResultInfo
                    {
                        icon = itemData.icon,
                        info0 = $"获得了一条  <color=green>{item.value}</color>cm<color=#02B8E3> {itemData.itemName} </color>!",
                        info1 = newRecord ? $"<color=red> 新记录！ </color>": ""
                    };
                UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);

                    RemoveFish(new global::RemoveFish { instanceId = fishId });
                    if (tryGetFish.setResult != null)
                        tryGetFish.setResult(true);
                }
                else
                {
                    ItemResultInfo itemResultInfo = new ItemResultInfo
                    {
                        icon = null,
                        info0 = "",
                        info1 = "$背包空间不足，鱼已放生"
                    }; 
                    UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);

                    if (behaviorTrees.TryGetValue(fishId, out var behaviorTree))
                    {
                        behaviorTree.StopAllTaskCoroutines();
                        behaviorTree.Start();
                    }
                    if (tryGetFish.setResult != null)
                        tryGetFish.setResult(false);
                }

            }
            else
            {
                if (tryGetFish.setResult != null)
                    tryGetFish.setResult(false);
            }
           
        }
        else
        {
            ItemResultInfo itemResultInfo = new ItemResultInfo
            {
                icon = null,
                info0 = "",
                info1 = "本次垂钓一无所获"
            };
            UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);

            if(tempLinkFishes.TryGetValue(tryGetFish.characterId,out fishId))
            {
                if(behaviorTrees.TryGetValue(fishId,out var behaviorTree))
                { 
                    behaviorTree.StopAllTaskCoroutines();
                    behaviorTree.Start();
                }
            }

        }
        linkFishes.Remove(tryGetFish.characterId);
        tempLinkFishes.Remove(tryGetFish.characterId);

        /*
        if(fishRuntimes.GetData(tryGetFish.fishId,out var fishRuntime))
        {
            FishData fishData = await GameDataManager.instance.GetAsyncData<FishData>(fishRuntime.dataId);
            Item item = new Item
            {
                dataId = fishData.itemId,
                count = 1,
                value=fishRuntime.value
            };
            int count=  await PackageManager.instance.SetItemInPackage(item, CharacterManager.instance.controllerCharacter.characterPackage);
            if (count > 0) { 
                tryGetFish.setResult(true);
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(fishData.itemId);
                Something something = new Something
                {
                    sprite = itemData.icon,
                    info = $"获得了一条{item.value}cm的{itemData.itemName}!",
                };
                UIManager.instance.ShowGamePanel<SomethingGetPanel, Something>(something);
            }
            else
            {
                tryGetFish.setResult(false);
            }
               
        }
        else
        {
            tryGetFish.setResult(false);
        }*/
    }

    public Transform GetFishTransform(int id,out Animator animator)
    {
        animator = null;
        if(fishRuntimeObjs.TryGetValue(id,out var fishRuntimeObj))
        {
            animator = fishRuntimeObj.animator;
            return fishRuntimeObj.model;
        }
        return null;
    }
}
 
public struct FishRuntime:INativeData
{ 
    public int intanceId;
    public int dataId;
    public int value;
    public int pondId;
    public int room;
    public int2 cell;
    public int Key => intanceId;

    public void Dispose()
    { 
    }
}
public struct Fisher 
{
    public int intanceId; 
    public ParticleSystem waterPs;
    public int roomId;

    public RuntimeObj runtimeObj;
    public void SetToolPos(Vector3 pos)
    {
        (runtimeObj.obj as Transform).position = pos;
    }
    
   
    public Fisher(RuntimeObj runtimeObj,int characterId,int roomId)
    {
        intanceId = characterId;
        this.runtimeObj = runtimeObj;
        this.roomId = roomId;
        waterPs = (runtimeObj.obj as Transform).GetComponentInChildren<ParticleSystem>(true);
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