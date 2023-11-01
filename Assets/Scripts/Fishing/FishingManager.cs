using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                iteminstanceId = tryCreatFishPond.itemId
            };
            fishPonds.SetData(fishPond);
            if (tryCreatFishPond.setValue != null)
            {
                tryCreatFishPond.setValue(instanceId);
            }
        }
    }
}
public struct FishPond:INativeData
{
    public int instanceId;
    public int iteminstanceId;
    public int dataId;
    public int Key => instanceId;

    public void Dispose()
    { 
    }
}