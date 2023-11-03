using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine.InputSystem;

public class FishingManager:Singleton<FishingManager>
{
    MyNativeData<FishPond> fishPonds = new MyNativeData<FishPond>();
    Dictionary<int, int> itemFishPonds = new Dictionary<int, int>();

    MyInstance myInstance;
    public override void Init()
    {
        base.Init();
        myInstance = new MyInstance();
        fishPonds.Init(16);
    }
    protected override void Clear()
    {
        base.Clear();
        myInstance.Clear();
        fishPonds.Dispose();
    }

    async void CreatFish(int pondId)
    {
        if(fishPonds.GetData(pondId,out var fishPond))
        {
            FishPondData fishPondData = await GameDataManager.instance.GetAsyncData<FishPondData>(fishPond.Key);
            if (fishPond.fishs.Count < fishPondData.maxFishCount)
            {
                Season season = GameTimeManager.instance.Season;
                if (!fishPondData.seasonRandomValue.TryGetValue(season, out var randomId))
                {
                    fishPondData.seasonRandomValue.TryGetValue(Season.Default, out randomId);
                }
                var randomResults = GameRandom.instance.GetRandomValue(randomId);
                if (randomResults.Count > 0)
                {
                    var randomResult = randomResults[0];
                    CreatFish CreatFish = new CreatFish
                    {
                        dataId = int.Parse(randomResult.result),
                        fishValue = randomResult.count,
                        pondId = pondId,
                        room = fishPond.room,
                        setValue=SetValue
                    };

                    void SetValue(int fishId)
                    {
                        fishPond.fishs.Add(fishId);
                        fishPonds.SetData(fishPond);
                    }

                    GameActionManager.instance.QueueAction(CreatFish,true);
                }
            }
            GameTimerController.instance.DelayAction(fishPondData.produceCD, () =>
            {
                CreatFish(pondId);
            });
        }
    }

    public bool GetFishPond(int instanceId,out FishPond fishPond)
    {
        return fishPonds.GetData(instanceId, out fishPond);
    }

    void TryDeleteFishPond(TryDeleteFishPond tryDeleteFishPond)
    {
        if (tryDeleteFishPond.instanceId != 0)
        {
            bool result = fishPonds.RemoveData(tryDeleteFishPond.instanceId);
            tryDeleteFishPond.setResult(result);
             
        }
        else
        {
            if (itemFishPonds.TryGetValue(tryDeleteFishPond.itemId, out int instanceId))
            {
                bool result = fishPonds.RemoveData(instanceId);
                tryDeleteFishPond.setResult(result);
            }
            else
            {
                tryDeleteFishPond.setResult(false);
            } 
        } 
    }
    async void TryCreatFishPond(TryCreatFishPond tryCreatFishPond)
    {
        FishPondData fishPondData = await GameDataManager.instance.GetAsyncData<FishPondData>(tryCreatFishPond.dataId);
        if (fishPondData)
        {
            int instanceId = myInstance.CreatInstanceId();
            if (tryCreatFishPond.itemId != 0)
            {
                itemFishPonds[tryCreatFishPond.itemId] = instanceId;
            }
            FishPond fishPond = new FishPond
            {
                dataId = tryCreatFishPond.dataId,
                instanceId = instanceId,
                itemInstanceId = tryCreatFishPond.itemId,
                room = tryCreatFishPond.room,
                fishs = new NativeHashSet<int>(8,Allocator.TempJob)
            };
            fishPonds.SetData(fishPond);
            if (tryCreatFishPond.setValue != null)
            {
                tryCreatFishPond.setValue(instanceId);
            }
            GameActionManager.instance.QueueAction(new RefreshFishPondObj
            {
                pondId=instanceId,
                room=tryCreatFishPond.room,
            });
        }
    }

    public void TryRemoveFish(int pondId,int fishId)
    {
        if(fishPonds.GetData(pondId,out var fishPond))
        {
            fishPond.fishs.Remove(fishId);
        }
    }
   
}
public struct FishPond:INativeData
{
    public int instanceId;
    public int itemInstanceId;
    public int room;
    public int dataId;
     
    public NativeHashSet<int> fishs;
    public int Key => instanceId;

    public void Dispose()
    {
        fishs.Dispose();
    }
}