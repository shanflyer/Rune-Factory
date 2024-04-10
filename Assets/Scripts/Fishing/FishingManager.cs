using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.InputSystem;

public class FishingManager:Singleton<FishingManager>
{
    MyNativeData<FishPond> fishPonds = new MyNativeData<FishPond>(); 
    Dictionary<int2, FishPondData> fishPondDatas = new Dictionary<int2, FishPondData>();
    MyInstance myInstance;
    public override async void Init()
    {
        base.Init();
        myInstance = new MyInstance();
        fishPonds.Init(16);

        fishPondDatas.Clear();
        var allData=await GameDataManager.instance.GetAllAsyncData<FishPondData>();
        for(int i = 0; i < allData.Count; i++)
        {
            var data = allData[i];
            fishPondDatas[data.linkMapItem] = data;
        }

        GameActionManager.instance.AddListener<TryCreatFishPond>(TryCreatFishPond);
        GameActionManager.instance.AddListener<TryDeleteFishPond>(TryDeleteFishPond);
    }
    protected override void Clear()
    {
        base.Clear();
        myInstance.Clear();
        fishPonds.Dispose();
    }

    void CreatFish(int pondId)
    {
        if(fishPonds.GetData(pondId,out var fishPond))
        {
            FishPondData fishPondData = fishPondDatas[new int2(fishPond.room, fishPond.itemInstanceId)];
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
            if (tryDeleteFishPond.setResult != null)
                tryDeleteFishPond.setResult(result);
             
        }
        else 
        {
            if (tryDeleteFishPond.setResult != null)
                tryDeleteFishPond.setResult(false);
        } 
    }
    void TryCreatFishPond(TryCreatFishPond tryCreatFishPond)
    {
        int2 key=new int2(tryCreatFishPond.room,tryCreatFishPond.itemId);
        if(fishPondDatas.TryGetValue(key,out var fishPondData))
        { 
            int instanceId = tryCreatFishPond.instanceId;
            
            FishPond fishPond = new FishPond
            {
                dataId = fishPondData.id,
                instanceId = instanceId, 
                room = tryCreatFishPond.room,
                itemInstanceId=tryCreatFishPond.itemId,
                fishs = new NativeHashSet<int>(8, Allocator.TempJob)
            };
            fishPonds.SetData(fishPond);
            if (tryCreatFishPond.setValue != null)
            {
                tryCreatFishPond.setValue(instanceId);
            }
            CreatFish(instanceId);


            GameActionManager.instance.QueueAction(new RefreshFishPondObj
            {
                pondId = instanceId,
                room = tryCreatFishPond.room,
            });
        } 
        if(tryCreatFishPond.setResult!=null)
        {
            tryCreatFishPond.setResult(false);
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
    public int room;
    public int itemInstanceId;
    public int dataId;
     
    public NativeHashSet<int> fishs;
    public int Key => instanceId;

    public void Dispose()
    {
        fishs.Dispose();
    }
}