using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

public class FishController:Singleton<FishController>
{
    MyNativeData<FishRuntime> fishRuntimes = new MyNativeData<FishRuntime>();
    Dictionary<int, List<int>> fishPondFishes = new Dictionary<int, List<int>>(); 
    Dictionary<int, CharacterRuntimeObj> fishRuntimeObjs = new Dictionary<int, CharacterRuntimeObj>(); 
    Dictionary<int, BehaviorTree> behaviorTrees = new Dictionary<int, BehaviorTree>();
    MyInstance myInstance;
    public override bool NeedUpdata => true;
    private GameObject FishBehavior;
    public override void Init()
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
    }
    protected override void Clear()
    {
        base.Clear();
        myInstance.Clear();
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
        var cells = MapCellController.instance.GetItemTriggerCells(fishPond.itemInstanceId, fishPond.room);
        int index = GameRandom.RandomInt(0, cells.Length);

        FishRuntime fishRuntime = new FishRuntime
        {
            intanceId=myInstance.CreatInstanceId(),
            dataId = creatFish.dataId,
            pondId=creatFish.pondId,
            room=creatFish.room,
            value=creatFish.fishValue,
            cell = cells[index]
        };
        fishRuntimes.SetData(fishRuntime);
        creatFish.setValue(fishRuntime.Key);
        if(!fishPondFishes.TryGetValue(creatFish.pondId,out var ints))
        {
            ints = new List<int>();
            fishPondFishes.Add(creatFish.pondId,ints);
        }
        ints.Add(fishRuntime.intanceId);
        RefreshFish(fishRuntime.intanceId, fishRuntime.room, fishRuntime.room);
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
        fishRuntimeObjs[runtimeObj.linkId] = fishRuntimeObj;

        var cells = MapCellController.instance.GetItemTriggerCells(fishPond.itemInstanceId, fishPond.room);
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
        }
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