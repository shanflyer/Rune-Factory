using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PackageManager
{
    public async Task InitFromSaveData(List<PackageSaveData> packageSaveDatas)
    {
        for (int i = 0; i < packageSaveDatas.Count; i++)
        {
            packageSaveDatas[i].Unpack();
            if (gamePackages.TryGetValue(packageSaveDatas[i].id,out var gamePackage))
            {
                gamePackage.caseCount = packageSaveDatas[i].caseCount;
                gamePackage.level = packageSaveDatas[i].level;
                gamePackage.name = packageSaveDatas[i].packageName;

                var dataCount = packageSaveDatas[i].items.Count / 2;
                for (var j = 0; j < dataCount; j++)
                {
                    var item = new Item(packageSaveDatas[i].items[j * 2], packageSaveDatas[i].items[j * 2 + 1]);
                    await gamePackage.SetItemInPackage(item);
                }

                GameActionManager.instance.QueueAction(new RefreshShortcut
                {
                    packageId=gamePackage.instanceId
                });
            }
            else
            {
                var saveData = packageSaveDatas[i];
                var packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(saveData.dataId);

                 gamePackage = new GamePackage(saveData.caseCount,
                    saveData.packageName, saveData.id, packageSetData, saveData.level)
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
                gamePackages.Add(saveData.id, gamePackage);
                RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
            }

        }
    }

    public List<PackageSaveData> GetPackageSaveData()
    {
        List<PackageSaveData> packageSaveDatas = new List<PackageSaveData>();
        using (var e = gamePackages.GetEnumerator())
        {
            while (e.MoveNext())
            {
                GamePackage gamePackage = e.Current.Value;
                PackageSaveData packageSaveData = new PackageSaveData
                {
                    id = gamePackage.instanceId,
                    caseCount = gamePackage.caseCount,
                    dataId = gamePackage.packageSetData.id,
                    level = gamePackage.level,
                    packageType = gamePackage.packageType,
                    packageName = gamePackage.name,
                    itemPackage = gamePackage.itemPackage,
                    items = new List<ulong>()
                };
                var items = gamePackage.GetItems();
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
