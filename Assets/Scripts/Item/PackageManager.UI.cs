using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PackageManager
{
    public void ShowPlayerBagUse(bool close, ItemMatchData itemMatchData)
    {
        AsyncTaskRunner.Run(() => ShowPlayerBagUseAsync(close, itemMatchData), nameof(ShowPlayerBagUse));
    }

    private async Task ShowPlayerBagUseAsync(bool close, ItemMatchData itemMatchData)
    {
        Character character = CharacterManager.instance.controllerCharacter;
        PackageList packageList = new PackageList
        {
            packageDatas = new List<PackageData>(),
            itemMatchData = itemMatchData
        };
        if (gamePackages.TryGetValue(character.characterPackage, out GamePackage gamePackage))
        {
            packageList.packageDatas.Add(gamePackage.OutGamePackageData());
        }
        var warehousePanel = await UIManager.instance.ShowGamePanel<WarehousePanel, PackageList>(packageList);
        warehousePanel.SetSelectItemAction((Item item,int index,bool select) =>
        {
            UsingAction(item);
            if (close)
            {
                UIManager.instance.CloseGamePanel<WarehousePanel>();
            }
        }, "使用");
    }
    public void ShowFightPlayerBagUse(bool close, ItemMatchData itemMatchData)
    {
        AsyncTaskRunner.Run(() => ShowFightPlayerBagUseAsync(close, itemMatchData), nameof(ShowFightPlayerBagUse));
    }

    private async Task ShowFightPlayerBagUseAsync(bool close, ItemMatchData itemMatchData)
    {
        Character character = CharacterManager.instance.controllerCharacter;
        PackageList packageList = new PackageList
        {
            packageDatas = new List<PackageData>(),
            itemMatchData = itemMatchData
        };
        if (gamePackages.TryGetValue(character.characterPackage, out GamePackage gamePackage))
        {
            packageList.packageDatas.Add(gamePackage.OutGamePackageData());
        }
        var warehousePanel = await UIManager.instance.ShowGamePanel<WarehousePanel, PackageList>(packageList);
        warehousePanel.SetSelectItemAction((Item item, int index, bool select) =>
        {

            FightManager.instance.TryUseItem(item);
            // UsingAction(item);
            if (close)
            {
                UIManager.instance.CloseGamePanel<WarehousePanel>();
            }
        }, "使用");
    }
    private void UsingAction(Item item)
    {
        ItemUseAction itemUseAction = new ItemUseAction
        {
            itemId = item.dataId,
            itemCount = 1,
            packageId = item.packageId
        };
        GameActionManager.instance.QueueAction(itemUseAction, true);
    }

    public void ShowAllPlayerPackage(SelectAction<Item> selectItemAction, string actionName,
        int ManufactureId)
    {
        AsyncTaskRunner.Run(() => ShowAllPlayerPackageAsync(selectItemAction, actionName, ManufactureId), nameof(ShowAllPlayerPackage));
    }

    private async Task ShowAllPlayerPackageAsync(SelectAction<Item> selectItemAction, string actionName,
        int ManufactureId)
    {
        PackageList packageList = new PackageList
        {
            packageDatas = new List<PackageData>(),
            itemMatchData = new ItemMatchData
            {
                itemMatchType = ItemMatchType.Manufacture,
                matchValues = new HashSet<int>
                {
                    ManufactureId
                }
            }
        };
        for (int i = 0; i < playerPackages.Count; i++)
        {
            if (gamePackages.TryGetValue(playerPackages[i], out GamePackage gamePackage))
            {
                packageList.packageDatas.Add(gamePackage.OutGamePackageData());
            }
        }
        var warehousePanel = await UIManager.instance.ShowGamePanel<WarehousePanel, PackageList>(packageList);
        warehousePanel.SetSelectItemAction(selectItemAction, actionName);
    }

    private async Task ShowMultiPackagePanelAsync(ShowMultiPackagePanel showMultiPackagePanel)
    {
        PackageData packageData0 = GetPackageData(showMultiPackagePanel.packageId0);
        PackageData packageData1 = GetPackageData(showMultiPackagePanel.packageId1);

        PackageList packageList = new PackageList
        {
            packageDatas = new List<PackageData>
            {
                packageData0,packageData1
            }
        };
       await UIManager.instance.ShowGamePanel<MultiPackagePanel, PackageList>(packageList);
    }

    private async Task OpenPackageAsync(OpenPackage openPackage)
    {
        PackageList packageList = new PackageList
        {
            packageDatas = new List<PackageData>(),
            itemMatchData = openPackage.itemMatchData,
            canSetShortcut = openPackage.canSetShortcut
        };
        int packageId = openPackage.packageId;
        if (openPackage.packageId == -1)
        {
            packageId = CharacterManager.instance.controllerCharacter.characterPackage;
        }
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            PackageData packageData = gamePackage.OutGamePackageData();
            packageList.packageDatas.Add(packageData);

            GameActionAsset gameActionData = await GameDataManager.instance.GetAsyncData<GameActionAsset>(openPackage.selectActionId);
            if (gameActionData != null)
            {
                gameActionData.Action(packageId, target: openPackage.targetObj);
            }

            if (openPackage.isMiniShow)
            {
                var miniPackagePanel = await UIManager.instance.ShowGamePanel<MiniPackagePanel, PackageList>(packageList);

                if (gameActionData != null)
                {
                    miniPackagePanel.SetSelectItemAction(openPackage.selectAction, openPackage.selectActionName);
                }
                else
                {

                }
                    if (openPackage.selectActionId == 0 &&
                    openPackage.selectAction != null)
                {
                    miniPackagePanel.SetSelectItemAction(openPackage.selectAction, openPackage.selectActionName);
                }

                if (openPackage.setPanel != null)
                {
                    openPackage.setPanel(miniPackagePanel);
                }
            }
            else
            {
                var WarehousePanel = await UIManager.instance.ShowGamePanel<WarehousePanel, PackageList>(packageList);

                if (openPackage.selectActionId == 0 &&
                    openPackage.selectAction != null)
                {
                    WarehousePanel.SetSelectItemAction(openPackage.selectAction, openPackage.selectActionName);
                }

                if (openPackage.setPanel != null)
                {
                    openPackage.setPanel(WarehousePanel);
                }
            }

        }
    }
}
