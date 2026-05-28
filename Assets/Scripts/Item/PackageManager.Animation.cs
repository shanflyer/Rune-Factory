using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PackageManager
{
    private void RefreshPackageMapDisplay(int packageCount, int itemCount, int packageId)
    {
        int index = 0;
        int perCount = packageCount / 4;
        if (itemCount >= perCount * 3)
        {
            index = 3;
        }
        else if (itemCount >= perCount * 2)
        {
            index = 2;
        }
        else if (itemCount >= perCount)
        {
            index = 1;
        }

        SetItemAnimation setItemAnimation = new SetItemAnimation
        {
            id = packageId,
            keyX = index,
        };
        GameActionManager.instance.QueueAction(setItemAnimation);

        RefreshMapPackageItemRender RefreshMapPackageItemRender = new RefreshMapPackageItemRender
        {
            linkInstanceId = packageId,
        };
        GameActionManager.instance.QueueAction(RefreshMapPackageItemRender);
    }

    private void RefreshShortcut(RefreshShortcut refreshShortcut)
    {
        if(gamePackages.TryGetValue(refreshShortcut.packageId,out var gamePackage))
        {
            var packageSetData= gamePackage.packageSetData;
            if (packageSetData.objPackageAnimationDataId != 0)
            {
                var PackageItemCounts = gamePackage.PackageItemCounts;
                foreach (var item in PackageItemCounts)
                {
                    if (objPackageAnimationDatas.TryGetValue(packageSetData.objPackageAnimationDataId, out var objPackageAnimationData))
                    {
                        var key = objPackageAnimationData.GetAnimationKey(item.Key, item.Value);
                        SetItemAnimation setItemAnimation = new SetItemAnimation
                        {
                            id = gamePackage.instanceId,
                            keyX = key.x,
                            keyY = key.y
                        };
                        GameActionManager.instance.QueueAction(setItemAnimation);
                    }
                }
            }
        }
    }
    void LoadObjPackageAnimationData()
    {
        // 包裹动画数据加载入口保持同步，加载异常统一进入异步日志。
        AsyncTaskRunner.Run(LoadObjPackageAnimationDataAsync(), nameof(LoadObjPackageAnimationData));
    }

    async System.Threading.Tasks.Task LoadObjPackageAnimationDataAsync()
    {
        var datas =await GameDataManager.instance.GetAllAsyncData<ObjPackageAnimationData>();
        objPackageAnimationDatas.Clear();
        for(int i = 0; i < datas.Count; i++)
        {
            objPackageAnimationDatas[datas[i].id] = datas[i];
        }
    }

}
