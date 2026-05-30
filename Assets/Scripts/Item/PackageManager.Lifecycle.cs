using System.Threading.Tasks;
using UnityEngine;

public partial class PackageManager
{
    private async Task CreatPackageAsync(CreatPackage creatPackage)
    {
        int instanceId = await CreatGamePackage(creatPackage.packageDataId, creatPackage.level, creatPackage.instanceId, creatPackage.saveId);
        if (instanceId == -1)
        {
            return;
        }
        if (creatPackage.playerPackage||GameManager.instance.GetPlayerBoxId() == instanceId)
        {
            AddPlayerPackage(instanceId);
        }

        if (creatPackage.setValue != null)
        {
            creatPackage.setValue(instanceId);
        }
        if (creatPackage.setResult != null)
        {
            creatPackage.setResult(true);
        }
    }

    private void RemovePackage(RemovePackage removePackage)
    {
        bool result = gamePackages.Remove(removePackage.packageDataId);
        removePackage.setResult(result);
    }

    public PackageData GetPackageData(int packageId)
    {
        if (gamePackages.TryGetValue(packageId, out var gamePackage))
        {
            return gamePackage.OutGamePackageData();
        }
        return default(PackageData);
    }

    public async Task<int> CreatGamePackage(int dataId, int level, int instanceId = 0, int saveId = 0)
    {
        if (dataId == 0) return -1;
        int packageSaveId = SaveRuntimeResolver.instance.EnsureSaveId(SaveEntityKind.Package, saveId);
        int packageInstanceId = instanceId != 0
            ? instanceId
            : SaveRuntimeResolver.instance.Resolve(SaveEntityKind.Package, packageSaveId);
        if (packageInstanceId == 0)
        {
            packageInstanceId = MyInstance.instance.Uid;
        }
        if (gamePackages.TryGetValue(packageInstanceId, out var _oldGamePackage))
        {
            RefreshPackageMapDisplay(_oldGamePackage.caseCount, _oldGamePackage.itemCount, _oldGamePackage.instanceId);
            return -1;
        }
        SaveRuntimeResolver.instance.Bind(SaveEntityKind.Package, packageSaveId, packageInstanceId);

        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(dataId);
        if (packageSetData == null)
        {
            Debug.LogError($"null packageSetData:{dataId}");
            return -1;
        }
        int nowCount = packageSetData.count + packageSetData.levelUpAddCount * level;
        GamePackage gamePackage = new GamePackage(nowCount, packageSetData.name, packageInstanceId, packageSetData, level, packageSaveId);
        gamePackages.Add(packageInstanceId, gamePackage);

        if (level <= 1)
        {
            for (int i = 0; i < packageSetData.initItems.Count; i++)
            {
              await  gamePackage.SetItemInPackage(
                    new Item(packageSetData.initItems[i].x, packageSetData.initItems[i].y));
            }

            RefreshShortcut refreshShortcut = new RefreshShortcut
            {
                packageId = packageInstanceId,
            };
            GameActionManager.instance.QueueAction(refreshShortcut);
        }
        SetItemAnimation setItemAnimation = new SetItemAnimation
        {
            id = gamePackage.instanceId,
            keyX = int.MinValue,
            keyY = level
        };
        GameActionManager.instance.QueueAction(setItemAnimation);
        return packageInstanceId;
    }

}
