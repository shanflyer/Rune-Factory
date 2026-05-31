using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PackageManager
{
    public async Task InitFromSaveData(List<PackageSaveData> packageSaveDatas)
    {
        for (int i = 0; i < packageSaveDatas.Count; i++)
        {
            var saveData = packageSaveDatas[i];
            saveData.Unpack();
            int packageRuntimeId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.Package, saveData.saveId);
            if (packageRuntimeId != 0 && gamePackages.TryGetValue(packageRuntimeId, out var gamePackage))
            {
                gamePackage.saveId = saveData.saveId;
                SaveRuntimeResolver.instance.Bind(SaveEntityKind.Package, saveData.saveId, gamePackage.instanceId);
                gamePackage.caseCount = saveData.caseCount;
                gamePackage.level = saveData.level;
                gamePackage.name = saveData.packageName;

                var dataCount = saveData.items.Count / 2;
                for (var j = 0; j < dataCount; j++)
                {
                    var item = new Item(saveData.items[j * 2], saveData.items[j * 2 + 1]);
                    await gamePackage.SetItemInPackage(item);
                }

                GameActionManager.instance.QueueAction(new RefreshShortcut
                {
                    packageId=gamePackage.instanceId
                });
            }
            else
            {
                var packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(saveData.dataId);
                packageRuntimeId = packageRuntimeId != 0 ? packageRuntimeId : MyInstance.instance.Uid;
                SaveRuntimeResolver.instance.Bind(SaveEntityKind.Package, saveData.saveId, packageRuntimeId);

                 gamePackage = new GamePackage(saveData.caseCount,
                    saveData.packageName, packageRuntimeId, packageSetData, saveData.level, saveData.saveId)
                {
                    itemPackage = saveData.itemPackage,

                };

                var dataCount = saveData.items.Count / 2;
                var items = new List<Item>();
                for (var j = 0; j < dataCount; j++)
                {
                    var item = new Item(saveData.items[j * 2], saveData.items[j * 2 + 1]);
                    items.Add(item);
                }

                gamePackage.InitSaveItemList(items);
                gamePackages.Add(packageRuntimeId, gamePackage);
                RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
            }

        }
    }

    public void LoadPlayerPackagesFromSaveIds(List<int> packageSaveIds)
    {
        playerPackages.Clear();
        if (packageSaveIds == null)
        {
            return;
        }

        for (int i = 0; i < packageSaveIds.Count; i++)
        {
            int runtimeId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.Package, packageSaveIds[i]);
            if (runtimeId != 0)
            {
                AddPlayerPackage(runtimeId);
            }
        }
    }

    public List<int> GetPlayerPackageSaveIds()
    {
        List<int> packageSaveIds = new List<int>();
        for (int i = 0; i < playerPackages.Count; i++)
        {
            int saveId = SaveRuntimeResolver.instance.GetSaveId(SaveEntityKind.Package, playerPackages[i]);
            if (saveId != 0)
            {
                packageSaveIds.Add(saveId);
            }
        }

        return packageSaveIds;
    }

    public List<PackageSaveData> GetPackageSaveData()
    {
        List<PackageSaveData> packageSaveDatas = new List<PackageSaveData>();
        using (var e = gamePackages.GetEnumerator())
        {
            while (e.MoveNext())
            {
                GamePackage gamePackage = e.Current.Value;
                gamePackage.saveId = SaveRuntimeResolver.instance.EnsureSaveId(SaveEntityKind.Package, gamePackage.saveId);
                SaveRuntimeResolver.instance.Bind(SaveEntityKind.Package, gamePackage.saveId, gamePackage.instanceId);
                PackageSaveData packageSaveData = new PackageSaveData
                {
                    saveId = gamePackage.saveId,
                    caseCount = gamePackage.caseCount,
                    dataId = gamePackage.packageSetData.id,
                    level = gamePackage.level,
                    packageType = gamePackage.packageType,
                    packageName = gamePackage.name,
                    itemPackage = gamePackage.itemPackage,
                    items = new List<ulong>()
                };
                var items = gamePackage.GetItemsForSave();
                for (var i = 0; i < items.Count; i++)
                {
                    var packed = items[i].Pack();
                    packageSaveData.items.Add(packed.Item1);
                    packageSaveData.items.Add(packed.Item2);
                }

                packageSaveData.Pack();
                packageSaveDatas.Add(packageSaveData);
            }
        }
        return packageSaveDatas;
    }
}
