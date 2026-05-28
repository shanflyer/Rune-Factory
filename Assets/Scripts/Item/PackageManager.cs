using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class PackageManager : Singleton<PackageManager>
{
    public List<int> playerPackages = new List<int>();

    public void AddPlayerPackage(int id)
    {
        if (!playerPackages.Contains(id))
        {
            playerPackages.Add(id);
        }
    }

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

    public int GetPlayerItemCount(int itemDataId)
    {
        int itemCount = 0;
        for (int i = 0; i < playerPackages.Count; i++)
        {
            if (gamePackages.TryGetValue(playerPackages[i], out GamePackage gamePackage))
            {
                itemCount += gamePackage.GetItemCount(itemDataId);
            }
        }
        return itemCount;
    }

    public bool RemovePlayerPackageItem(int itemDataId, int count)
    {
        for (int i = 0; i < playerPackages.Count; i++)
        {
            if (count <= 0)
            {
                return true;
            }
            int packageId = playerPackages[i];
            if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
            {
                count = gamePackage.TryGetItemOutPackage(itemDataId, count);
                //gamePackages[packageId] = gamePackage;
            }
        }
        return false;
    }

    public async Task<bool> SetPlayerPackageItem(int itemDataId, int count)
    {
        for (int i = 0; i < playerPackages.Count; i++)
        {
            int packageId = playerPackages[i];
            if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
            {
                Item item = new Item
                {
                    instanceId =MyInstance.instance.Uid,
                    dataId = itemDataId,
                    count = count
                };
                count = await gamePackage.SetItemInPackage(item);
                //gamePackages[packageId] = gamePackage;
            }

            if (count <= 0)
            {
                return true;
            }
        }

        return false;
    }

    public override void Init()
    {
        base.Init();
        LoadObjPackageAnimationData();
        GameActionManager.instance.AddAsyncListener<ItemUseAction>(UsetItemAsync, nameof(UsetItemAsync));
        GameActionManager.instance.AddAsyncListener<CreatRuntimePackage>(CreatRuntimePackageAsync, nameof(CreatRuntimePackage));
        GameActionManager.instance.AddListener<RemoveRuntimePackage>(RemoveRuntimePackage);
        GameActionManager.instance.AddAsyncListener<AddPackageItem>(AddPackageItemActionAsync, nameof(AddPackageItemActionAsync));
        GameActionManager.instance.AddAsyncListener<OpenPackage>(OpenPackageAsync, nameof(OpenPackage));
        GameActionManager.instance.AddAsyncListener<GiveGift>(GiveGiftAsync, nameof(GiveGift));
        GameActionManager.instance.AddListener<CheckItemValue>(CheckItemValue);
        GameActionManager.instance.AddAsyncListener<CreatPackage>(CreatPackageAsync, nameof(CreatPackage));
        GameActionManager.instance.AddListener<RemovePackage>(RemovePackage);
        GameActionManager.instance.AddListener<RemovePackageItem>(RemovePackageItemAction);
        GameActionManager.instance.AddAsyncListener<ShowMultiPackagePanel>(ShowMultiPackagePanelAsync, nameof(ShowMultiPackagePanel));
        GameActionManager.instance.AddListener<SetPackageSelectItem>(SetPackageSelectItem);
        GameActionManager.instance.AddListener<RemovePlayerPackageItem>(RemovePlayerPackageItem);
        GameActionManager.instance.AddAsyncListener<AddItemValue>(AddItemValueAsync, nameof(AddItemValue));
        GameActionManager.instance.AddAsyncListener<SetItemValue>(SetItemValueAsync, nameof(SetItemValue));
        GameActionManager.instance.AddListener<CheckCharacterItemValue>(CheckCharacterItemValue);
        GameActionManager.instance.AddListener<CheckCharacterPackageFull>(CheckCharacterPackageFull);
        GameActionManager.instance.AddListener<ChangePackageInnstance>(ChangePackageInnstance);
        GameActionManager.instance.AddListener<RefreshShortcut>(RefreshShortcut);
        GameActionManager.instance.AddListener<RemovePackageItemInstance>(RemovePackageItemInstance);
        GameActionManager.instance.AddAsyncListener<AddPackageItemList>(AddPackageItemListAsync, nameof(AddPackageItemList));
        GameActionManager.instance.AddListener<SortPackageItem>(SortPackageItem);
    }
    void SortPackageItem(SortPackageItem sortPackageItem)
    {

        if (gamePackages.TryGetValue(sortPackageItem.packageId, out var gamePackage))
        {
            gamePackage.SortItem(sortPackageItem.itemId, sortPackageItem.index);
        }
        else if (gamePackages.TryGetValue(CharacterManager.instance.controllerCharacter.characterPackage, out gamePackage))
         {
            gamePackage.SortItem(sortPackageItem.itemId, sortPackageItem.index);
        }
    }
    public bool GetPackageItemCounts(int packageId, out List<int2> items)
    {
        items = null;
        if (gamePackages.TryGetValue(packageId, out var gamePackage))
        {
            var itemDic = gamePackage.PackageItemCounts;
            items = new List<int2>();
            foreach (var item in itemDic)
            {
                items.Add(new int2(item.Key, item.Value));
            }
            return true;
        }
        return false;
    }

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
    private void RemovePackageItemInstance(RemovePackageItemInstance removePackageItemInstance)
    {
        if (gamePackages.TryGetValue(removePackageItemInstance.packageId, out var gamePackage))
        {
            gamePackage.GetItemOutPackage(removePackageItemInstance.itemInstanceId);
            //gamePackages[removePackageItemInstance.packageId] = gamePackage;
            RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);

            GameActionManager.instance.QueueAction(new RefreshShortcut
            {
                packageId = gamePackage.instanceId
            },true);
        }
    }
    private void ChangePackageInnstance(ChangePackageInnstance changePackageInnstance)
    {
        if (gamePackages.TryGetValue(changePackageInnstance.oldInstanceId, out var gamePackage))
        {
            gamePackage.instanceId = changePackageInnstance.newInstanceId;
            gamePackages.Remove(changePackageInnstance.oldInstanceId);
            gamePackages.Add(gamePackage.instanceId, gamePackage);

            RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
        }
    }

    public void CheckCharacterPackageFull(CheckCharacterPackageFull checkCharacterPackageFull)
    {
        Character character = CharacterManager.instance.GetCharacter(checkCharacterPackageFull.characterId);
        if (gamePackages.TryGetValue(character.characterPackage, out var gamePackage))
        {
            if (checkCharacterPackageFull.setResult != null)
            {
                checkCharacterPackageFull.setResult(gamePackage.itemCount < gamePackage.caseCount);
            }
        }
    }

    public Item GetPackageSelectItem(int packageId)
    {
        if (gamePackages.TryGetValue(packageId, out var gamePackage))
        {
            return GetItemFromInstanceId(packageId, gamePackage.SelectItem);
        }
        return default(Item);
    }

    private async Task AddItemValueAsync(AddItemValue addItemValue)
    {
        Character character = CharacterManager.instance.GetCharacter(addItemValue.characterId);
        if (character != null)
        {
            if (gamePackages.TryGetValue(character.characterPackage, out var gamePackage))
            {
                int value =await gamePackage.AddItemValue(addItemValue.selectItem, addItemValue.value);
                if (value >= 0)
                {
                    RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
                    GameActionManager.instance.QueueAction(new RefreshItemValue
                    {
                        characterId = addItemValue.characterId,
                        itemId = addItemValue.selectItem,
                        itemValue = value
                    });
                    if (addItemValue.setResult != null)
                    {
                        addItemValue.setResult(true);
                        return;
                    }
                }

            }
        }
        if (addItemValue.setResult != null)
        {
            addItemValue.setResult(false);
        }
    }

    private async Task SetItemValueAsync(SetItemValue setItemValue)
    {
        Character character = CharacterManager.instance.GetCharacter(setItemValue.characterId);
        if (character != null)
        {
            if (gamePackages.TryGetValue(character.characterPackage, out var gamePackage))
            {
                int value =await gamePackage.SetItemValue(setItemValue.selectItem, setItemValue.value);
                if (value >= 0)
                {
                    RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
                    GameActionManager.instance.QueueAction(new RefreshItemValue
                    {
                        characterId = setItemValue.characterId,
                        itemId = setItemValue.selectItem,
                        itemValue = value
                    });
                    if (setItemValue.setResult != null)
                    {
                        setItemValue.setResult(true);
                        return;
                    }
                }
            }
        }
        if (setItemValue.setResult != null)
        {
            setItemValue.setResult(false);
        }
    }

    private void SetPackageSelectItem(SetPackageSelectItem setPackageSelectItem)
    {
        if (gamePackages.TryGetValue(setPackageSelectItem.packageId, out var gamePackage))
        {
            gamePackage.SelectItem = setPackageSelectItem.selectItem;
            //amePackages[setPackageSelectItem.packageId] = gamePackage;
        }
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

    private async Task CreatPackageAsync(CreatPackage creatPackage)
    {
        int instanceId = await CreatGamePackage(creatPackage.packageDataId, creatPackage.level, creatPackage.instanceId);
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

    private void CheckCharacterItemValue(CheckCharacterItemValue CheckCharacterItemValue)
    {
        Character character = CharacterManager.instance.GetCharacter(CheckCharacterItemValue.characterId);
        if (character != null)
        {
            Item item = GetItemFromInstanceId(character.characterPackage, CheckCharacterItemValue.itemId);
            if (item.instanceId != 0)
            {
                if (item.value  >= CheckCharacterItemValue.itemValue)
                {
                    if (CheckCharacterItemValue.setResult != null)
                    {
                        CheckCharacterItemValue.setResult(true);
                        return;
                    }
                }
            }
        }
        if (CheckCharacterItemValue.setResult != null)
        {
            CheckCharacterItemValue.setResult(false);
        }
    }

    private void CheckItemValue(CheckItemValue checkItemValue)
    {
        if (gamePackages.TryGetValue(checkItemValue.packageId, out var gamePackage))
        {
            Item item = gamePackage.GetItemFromInstanceId(checkItemValue.itemDataId);
            if (item.instanceId == 0)
            {
                var items = gamePackage.GetItemFromDataId(checkItemValue.itemDataId);
                if (items != null)
                {
                    int totalValue = 0;
                    for (int i = 0; i < items.Count; i++)
                    {
                        totalValue += items[i].value;
                    }
                    if (totalValue >= checkItemValue.itemValue)
                    {
                        checkItemValue.setResult(true);
                    }
                    else
                    {
                        checkItemValue.setResult(false);
                    }
                    return;
                }
                if (checkItemValue.setResult != null)
                {
                    checkItemValue.setResult(false);
                }
            }
            else
            {
                if (item.value >= checkItemValue.itemValue)
                {
                    if (checkItemValue.setResult != null)
                    {
                        checkItemValue.setResult(true);
                    }
                }
                else
                {
                    if (checkItemValue.setResult != null)
                    {
                        checkItemValue.setResult(false);
                    }
                }
            }
        }
    }

    private async Task GiveGiftAsync(GiveGift giveGift)
    {
        Character receiveCharacter = CharacterManager.instance.GetCharacter(giveGift.receiveCharacter);
        if (receiveCharacter != null)
        {
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(giveGift.giftId);
            if (itemData.useEventId != 0)
            {
                List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>
                {
                   new EventReferenceData
                   {
                       name="CharacterId",
                       valueType=ReferenceValueType.Int,
                       value=giveGift.receiveCharacter
                   },
                   new EventReferenceData
                   {
                       name="SelectItem",
                       valueType=ReferenceValueType.Int,
                       value=giveGift.giftId
                   },
                };
                await GameEventManager.instance.AddGameEvent(itemData.useEventId, eventReferenceDatas);
            }
            else
            {
                await SetItemInPackage(new Item(giveGift.giftId, 1), receiveCharacter.characterPackage);
            }
        }
        Character giveCharacter = CharacterManager.instance.GetCharacter(giveGift.giveCharacter);
        if (giveCharacter != null)
        {
            GetOutItenFromPackage(giveCharacter.characterPackage, giveGift.giftId, 1);
        }
    }
    private Dictionary<int, ObjPackageAnimationData> objPackageAnimationDatas = new Dictionary<int, ObjPackageAnimationData>();
    private Dictionary<int, GamePackage> gamePackages = new Dictionary<int, GamePackage>();
    private Dictionary<Vector2Int, int> runtimePackageRuntimes = new Dictionary<Vector2Int, int>();


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

    public PackageData GetPackageData(int packageId)
    {
        if (gamePackages.TryGetValue(packageId, out var gamePackage))
        {
            return gamePackage.OutGamePackageData();
        }
        return default(PackageData);
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

    public int GetPackageLevelUpCost(int id)
    {
        if (gamePackages.TryGetValue(id, out GamePackage gamePackage))
        {
            if (gamePackage.packageSetData)
            {
                return (gamePackage.level + 1) * gamePackage.packageSetData.levelUpCost;
            }
        }
        return 0;
    }

    public void AddPackageUpLevel(int id)
    {
        if (gamePackages.TryGetValue(id, out GamePackage gamePackage))
        {
            if (gamePackage.packageSetData)
            {
                gamePackage.level += 1;
                gamePackage.caseCount += gamePackage.packageSetData.levelUpAddCount;
               // gamePackages[id] = gamePackage;
                GameActionManager.instance.QueueAction(default(RefreshPackage));

                SetItemAnimation setItemAnimation = new SetItemAnimation
                {
                    id = gamePackage.instanceId,
                    keyX = int.MinValue,
                    keyY = gamePackage.level
                };
                GameActionManager.instance.QueueAction(setItemAnimation);

                RefreshShortcut refreshShortcut = new RefreshShortcut
                {
                    packageId = gamePackage.instanceId
                };
                GameActionManager.instance.QueueAction(refreshShortcut);
            }
        }
    }

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

    public bool CheckPackageTryItemIn(int packageId, int itemDataId, int count)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.CheckPackageTryItemIn(itemDataId, count);
        }
        return false;
    }
    public int GetPackageItemValue(int packageId, int itemDataId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.GetItemValue(itemDataId);
        }
        return -1;
    }

    public int GetPackageItemCount(int packageId, int itemDataId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.GetItemCount(itemDataId);
        }
        return -1;
    }
    public int GetPackageItemCountForInstance(int packageId, int instanceId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.GetItemCountForInstance(instanceId);
        }
        return -1;
    }
    public void ClearPackageItem(int packageId, int itemDataId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
        }
    }

    public int AddPackageCaseCount(int packageId, int count)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            gamePackage.caseCount += count;
            //gamePackages[packageId] = gamePackage;
            return gamePackage.caseCount;
        }
        return 0;
    }

    public void SetPackageCaseCount(int packageId, int count)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            gamePackage.caseCount = count;
            //gamePackages[packageId] = gamePackage;
        }
    }

    private void RemovePlayerPackageItem(RemovePlayerPackageItem removePlayerPackageItem)
    {
        Character character = CharacterManager.instance.GetCharacter(removePlayerPackageItem.characterId);
        if (character != null)
        {
            if (gamePackages.TryGetValue(character.characterPackage, out GamePackage gamePackage))
            {
                bool result = gamePackage.GetItemOutPackage(removePlayerPackageItem.itemDataId, removePlayerPackageItem.itemCount);
                //gamePackages[character.characterPackage] = gamePackage;
                if (removePlayerPackageItem.setResult != null)
                {
                    removePlayerPackageItem.setResult(result);
                }
                RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
                GameActionManager.instance.QueueAction(new RefreshShortcut
                {
                    packageId = gamePackage.instanceId
                });
            }
        }
        else
        {
            if (removePlayerPackageItem.setResult != null)
            {
                removePlayerPackageItem.setResult(false);
            }
        }
    }

    private void RemovePackageItemAction(RemovePackageItem removePackageItem)
    {
        if (gamePackages.TryGetValue(removePackageItem.packageId, out GamePackage gamePackage))
        {
            bool result = gamePackage.GetItemOutPackage(removePackageItem.itemDataId, removePackageItem.itemCount);
            //gamePackages[removePackageItem.packageId] = gamePackage;
            if (removePackageItem.setResult != null)
            {
                removePackageItem.setResult(result);
            }
            RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
            GameActionManager.instance.QueueAction(new RefreshShortcut
            {
                packageId = gamePackage.instanceId
            });
        }
    }
    private async Task AddPackageItemListAsync(AddPackageItemList addPackageItem)
    {
        if (addPackageItem.packageId == 0)
        {
            addPackageItem.packageId = CharacterManager.instance.controllerCharacter.characterPackage;
        }

        if (gamePackages.TryGetValue(addPackageItem.packageId, out GamePackage gamePackage))
        {
            bool success = true;
            for(int i = 0; i < addPackageItem.items.Count; i++)
            {
                int intanceId = MyInstance.instance.Uid;
                int count = await gamePackage.SetItemInPackage(new Item
                {
                    instanceId = intanceId,
                    dataId = addPackageItem.items[i].x,
                    count = addPackageItem.items[i].y
                });
                if (count > 0)
                {
                    success = false;
                }
            }
            if (addPackageItem.setResult != null)
            {
                addPackageItem.setResult(success);
            }

            RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
            GameActionManager.instance.QueueAction(new RefreshShortcut
            {
                packageId = gamePackage.instanceId
            });
        }
    }

    private async Task AddPackageItemActionAsync(AddPackageItem addPackageItem)
    {
        if (addPackageItem.packageId == 0)
        {
            addPackageItem.packageId = CharacterManager.instance.controllerCharacter.characterPackage;
        }
        if (gamePackages.TryGetValue(addPackageItem.packageId, out GamePackage gamePackage))
        {

            int intanceId = MyInstance.instance.Uid;
            int count = await gamePackage.SetItemInPackage(new Item
            {
                instanceId = intanceId,
                dataId = addPackageItem.itemDataId,
                count = addPackageItem.itemCount
            });
            if (addPackageItem.setValue != null)
            {
                addPackageItem.setValue(count);
            }

            RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
            GameActionManager.instance.QueueAction(new RefreshShortcut
            {
                packageId = gamePackage.instanceId
            });
        }

    }

    private void RemoveRuntimePackage(RemoveRuntimePackage removeRuntimePackage)
    {
        if (runtimePackageRuntimes.TryGetValue(removeRuntimePackage.key, out int instanceId))
        {
            if (gamePackages.ContainsKey(instanceId))
            {
                gamePackages.Remove(instanceId);
            }
            runtimePackageRuntimes.Remove(removeRuntimePackage.key);
        }
    }

    private async Task CreatRuntimePackageAsync(CreatRuntimePackage creatRuntimePackage)
    {
        int instanceId = creatRuntimePackage.instanceId;
        if (instanceId < 0)
        {
            instanceId = MyInstance.instance.Uid;
        }
        GamePackage gamePackage = new GamePackage
        {
            instanceId = instanceId,
            name = creatRuntimePackage.name,
            caseCount = creatRuntimePackage.caseCount,
            itemPackage = creatRuntimePackage.itemPackage
        };
        for (int i = 0; i < creatRuntimePackage.Items.Count; i++)
        {
            await gamePackage.SetItemInPackage(creatRuntimePackage.Items[i]);
        }
        gamePackages.Add(creatRuntimePackage.instanceId, gamePackage);
        runtimePackageRuntimes.Add(creatRuntimePackage.key, creatRuntimePackage.instanceId);
    }

    public async Task<int> CreatGamePackage(int dataId, int level, int instanceId = 0)
    {
        if (dataId == 0) return -1;
        int packageInstanceId = instanceId == 0 ? MyInstance.instance.Uid : instanceId;
        if (gamePackages.TryGetValue(packageInstanceId, out var _oldGamePackage))
        {
            RefreshPackageMapDisplay(_oldGamePackage.caseCount, _oldGamePackage.itemCount, _oldGamePackage.instanceId);
            return -1;
        }

        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(dataId);
        if (packageSetData == null)
        {
            Debug.LogError($"null packageSetData:{dataId}");
            return -1;
        }
        int nowCount = packageSetData.count + packageSetData.levelUpAddCount * level;
        GamePackage gamePackage = new GamePackage(nowCount, packageSetData.name, packageInstanceId, packageSetData,level);
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
    public int GetPackageCaseCount(int packageId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.caseCount;
        }
        return 0;
    }

    public List<Item> GetPackageItems(int packageId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.GetItems();
        }
        return null;
    }

    public bool GetOutItenFromPackage(int packageId, int itemid, int count)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            bool result = gamePackage.GetItemOutPackage(itemid, count);
            //gamePackages[packageId] = gamePackage;
            RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
            GameActionManager.instance.QueueAction(new RefreshShortcut
            {
                packageId = gamePackage.instanceId
            });
            return result;
        }
        return false;
    }

    public async Task<int> SetItemInPackage(Item item, int packageId,bool display=false)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            int result = await gamePackage.SetItemInPackage(item);
            //gamePackages[packageId] = gamePackage;
            RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
            GameActionManager.instance.QueueAction(new RefreshShortcut
            {
                packageId = gamePackage.instanceId
            }, true);
            return result;
        }

        return -1;
    }

    public bool IsHaveItem(int packageId, int itemDataId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.IsHaveItem(itemDataId);
        }
        return false;
    }

    private async Task UsetItemAsync(ItemUseAction itemUseEvent)
    {
        if (gamePackages.TryGetValue(itemUseEvent.packageId, out GamePackage gamePackage))
        {
            if (await UsetItemAction(itemUseEvent.itemId,itemUseEvent.itemInstance,itemUseEvent.targetCharacter))
            {
                gamePackage.GetItemOutPackage(itemUseEvent.itemId, itemUseEvent.itemCount);
                //gamePackages[itemUseEvent.packageId] = gamePackage;
                RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
                GameActionManager.instance.QueueAction(new RefreshShortcut
                {
                    packageId = gamePackage.instanceId
                });
            }
            else
            {

            }
        }
    }

    private async Task<bool> UsetItemAction(int itemId,int itemInstance,int targetCharacter=0)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(itemId.ToString());
        if (itemData != null )
        {
            if (itemData.useEventId == 0)
            {
                InformationController.instance.AddInformation("这件物品不能自由使用", true, true);
                return false;
            }
            else
            {
                if (ExploreManager.instance.isExplore && itemData.sceneType == SceneType.城镇)
                {
                    InformationController.instance.AddInformation("什么也没发生", true, true);
                    return true;
                }
                if (!ExploreManager.instance.isExplore && itemData.sceneType == SceneType.战斗)
                {
                    InformationController.instance.AddInformation("什么也没发生", true, true);
                    return true;
                }
                List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>
                {
                   new EventReferenceData
                   {
                       name="CharacterId",
                       valueType=ReferenceValueType.Int,
                       value=targetCharacter==0?CharacterManager.instance.controllerCharacter.instanceId:targetCharacter
                   },
                   new EventReferenceData
                   {
                       name="SelectItem",
                       valueType=ReferenceValueType.Int,
                       value=itemId
                   },
                    new EventReferenceData
                   {
                       name="ItemInstance",
                       valueType=ReferenceValueType.Int,
                       value=itemInstance
                   },
                     new EventReferenceData
                   {
                       name="ItemTypeValue",
                       valueType=ReferenceValueType.Int,
                       value=itemData.typeValue
                   },
                };
                await GameEventManager.instance.AddGameEvent(itemData.useEventId, eventReferenceDatas);
                if (!string.IsNullOrEmpty(itemData.useInfo))
                {
                    InformationController.instance.AddInformation(itemData.useInfo, true, true);
                }

                return true;
            }

        }
        return false;
    }

    public Item GetItemFromInstanceId(int packageId, int itemInstanceId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.GetItemFromInstanceId(itemInstanceId);
        }
        return default(Item);
    }

    private class GamePackage
    {
        public string name;
        public int instanceId;
        public PackageSetData packageSetData;
        public int caseCount;
        public bool singleCase => packageSetData != null && packageSetData.singleCase;
        public int level;
        public bool itemPackage;
        private List<Item> items;
        public int itemCount => items.Count - nullItems.Count;
        public PackageType packageType => packageSetData != null ? packageSetData.packageType : default(PackageType);
        private Queue<int> nullItems;

        public Dictionary<int, int> PackageItemCounts => packageItemCounts;

        private Dictionary<int, int> packageItemCounts;
        private Dictionary<int, List<int>> packageItemIndexDatas;

        public int SelectItem;

        public void SortItem(int itemId,int index)
        {
            if(index >= 0 && index< items.Count && packageItemIndexDatas.TryGetValue(itemId,out var list))
            {
                if (list.Contains(index))
                {
                    return;
                }

                int oldIndex = list[list.Count - 1];
                Item item = items[oldIndex];
                if (nullItems.Contains(index))
                {
                    Queue<int> newNullItems = new Queue<int>();
                    foreach(var i in nullItems)
                    {
                        if (i != index)
                        {
                            newNullItems.Enqueue(i);
                        }
                    }
                    newNullItems.Enqueue(oldIndex);
                    nullItems = newNullItems;
                    items[oldIndex] = default(Item);
                }
                else
                {
                    Item item0 = items[index];
                    RemoveItemIndex(item0.dataId, index);
                    AddItemIndex(item0.dataId, oldIndex);
                    items[oldIndex] = item0;
                }
                items[index] = item;
                list[list.Count - 1] = index;
            }
        }
        public void InitSaveItemList(List<Item> items)
        {
            this.items = new List<Item>();
            packageItemCounts.Clear();
            packageItemIndexDatas.Clear();
            nullItems.Clear();
            for(int i = 0; i < items.Count; i++)
            {
                if (items[i].count == 0)
                {
                    continue;
                }

                int index = this.items.Count;
                ChangeItemCount(items[i].dataId, items[i].count);
                AddItemIndex(items[i].dataId, index);
                this.items.Add(items[i]);
            }
        }
        public Item GetItemFromInstanceId(int itemInstanceId)
        {
            int index= items.FindIndex(item => item.instanceId == itemInstanceId);
            if (index >= 0)
            {
                return items[index];
            }
            else
            {
                if (packageItemIndexDatas.TryGetValue(itemInstanceId, out var ints))
                {
                    if(ints.Count>0)
                    {
                       return items[ints[0]];
                    }

                }
            }
            return default(Item);
        }

        public PackageData OutGamePackageData()
        {
            PackageData packageData = new PackageData
            {
                caseCount = caseCount,
                instanceId = instanceId,
                dataId = packageSetData != null ? packageSetData.id : 0,
                name = name,
                level = level,
                packageType = packageType,
                items = GetItems(),
            };
            return packageData;
        }

        public GamePackage()
        {
            InitCollections();
        }

        public GamePackage(int caseCount, string name, int instanceId, PackageSetData packageSetData, int level = 0 )
        {
            InitCollections();
            this.instanceId = instanceId;
            this.packageSetData= packageSetData;
            this.level = level;
            this.name = name;
            this.caseCount = caseCount;
        }

        private void InitCollections()
        {
            items = new List<Item>();
            packageItemCounts = new Dictionary<int, int>();
            packageItemIndexDatas = new Dictionary<int, List<int>>();
            itemPackage = false;
            nullItems = new Queue<int>();
            SelectItem = 0;
        }

        private List<int> GetOrCreateItemIndexes(int itemDataId)
        {
            if (!packageItemIndexDatas.TryGetValue(itemDataId, out var indexDatas))
            {
                indexDatas = new List<int>();
                packageItemIndexDatas.Add(itemDataId, indexDatas);
            }

            return indexDatas;
        }

        private int TakeEmptySlot()
        {
            return nullItems.Count != 0 ? nullItems.Dequeue() : items.Count;
        }

        private void SetSlot(int index, Item item)
        {
            if (items.Count <= index)
            {
                items.Add(item);
            }
            else
            {
                items[index] = item;
            }
        }

        private void AddItemIndex(int itemDataId, int index)
        {
            var indexDatas = GetOrCreateItemIndexes(itemDataId);
            if (!indexDatas.Contains(index))
            {
                indexDatas.Add(index);
            }
        }

        private void RemoveItemIndex(int itemDataId, int index)
        {
            if (!packageItemIndexDatas.TryGetValue(itemDataId, out var indexDatas))
            {
                return;
            }

            indexDatas.Remove(index);
            if (indexDatas.Count == 0)
            {
                packageItemIndexDatas.Remove(itemDataId);
            }
        }

        private void ChangeItemCount(int itemDataId, int delta)
        {
            packageItemCounts.TryGetValue(itemDataId, out int count);
            count += delta;
            if (count > 0)
            {
                packageItemCounts[itemDataId] = count;
            }
            else
            {
                packageItemCounts.Remove(itemDataId);
            }
        }

        private void ReleaseSlot(int index)
        {
            if (index < 0 || index >= items.Count)
            {
                return;
            }

            if (!nullItems.Contains(index))
            {
                nullItems.Enqueue(index);
            }

            items[index] = default(Item);
        }

        private bool TryGetStackSlotWithSpace(int itemDataId, int groupCount, out int index)
        {
            index = -1;
            if (!packageItemIndexDatas.TryGetValue(itemDataId, out var indexDatas))
            {
                return false;
            }

            for (int i = indexDatas.Count - 1; i >= 0; i--)
            {
                int slot = indexDatas[i];
                if (slot >= 0 && slot < items.Count && items[slot].count < groupCount)
                {
                    index = slot;
                    return true;
                }
            }

            return false;
        }

        public async Task<int> SetItemValue(int instanceId, int value)
        {
            int index = items.FindIndex(item => item.instanceId == instanceId);
            if (index >= 0)
            {
                var item = items[index];
                item=await Item.SetValue(item,value);
                items[index] = item;
                return item.value;
            }
            else
            {
                return -1;
            }
        }

        public async Task<int> AddItemValue(int instanceId, int value)
        {
            int index = items.FindIndex(item => item.instanceId == instanceId);
            if (index >= 0)
            {
                var item = items[index];
                item=await Item.ChangeValue(item,value);

                items[index] = item;
                return item.value;
            }
            else
            {
                return -1;
            }
        }

        private void RefreshSelectItem()
        {
            if (SelectItem == 0)
            {
                return;
            }
            bool isHavelSelectItem = false;
            for (int i = 0; i < items.Count; i++)
            {
                if (!nullItems.Contains(i))
                {
                    if (items[i].instanceId == SelectItem)
                    {
                        isHavelSelectItem = true;
                        break;
                    }
                }
            }
            if (SelectItem != 0)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (!nullItems.Contains(i))
                    {
                        if (items[i].dataId == SelectItem)
                        {
                            isHavelSelectItem = true;
                            break;
                        }
                    }
                }
            }
            if (!isHavelSelectItem)
            {
                SelectItem = 0;
            }
        }

        public List<Item> GetItems()
        {
            List<Item> results = new List<Item>();
            for (int i = 0; i < items.Count; i++)
            {
                if (!nullItems.Contains(i))
                {
                    results.Add(items[i]);
                }
            }
            return results;
        }

        public void ClearItem(int itemDataId)
        {
            if (packageItemIndexDatas.TryGetValue(itemDataId, out var indexs))
            {
                for (int i = 0; i < indexs.Count; i++)
                {
                    ReleaseSlot(indexs[i]);
                }
                packageItemIndexDatas.Remove(itemDataId);
                packageItemCounts.Remove(itemDataId);
            }
            RefreshSelectItem();
        }

        public async Task<int> SetItemInPackage(Item item, bool display = false)
        {
            if (caseCount < itemCount)
            {
                return item.count;
            }
            if (item.instanceId == 0)
            {
                item.instanceId = MyInstance.instance.Uid;
            }

            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
            if (itemData!=null)
            {
                if(instanceId== CharacterManager.instance.controllerCharacter.characterPackage)
                {
                    if (itemData.type == ItemType.种子|| itemData.type ==ItemType.农作物)
                    {
                        GameDataSaveManager.instance.SetPlantFruitCount(itemData.typeValue, 0);
                    }
                }

                if (packageType == PackageType.鲜活 && !itemData.isFresh)
                {
                    return item.count;
                }
                if (packageType == PackageType.非鲜活 && itemData.isFresh)
                {
                    return item.count;
                }
                int groupCount = singleCase ? 1 : itemData.groupCount;

                if (groupCount > 1)//物体堆叠数量
                {
                    int index ;//空物体位置
                    if (!TryGetStackSlotWithSpace(itemData.id, itemData.groupCount, out index))
                    {
                        if (caseCount <= itemCount)
                        {
                            return item.count;
                        }

                        index = TakeEmptySlot();//空物体位置
                        Item newItem = new Item
                        {
                            instanceId = MyInstance.instance.Uid,
                            dataId = itemData.id,
                            packageId = instanceId,
                            isFresh = itemData.isFresh,
                            itemType = itemData.type,
                            count = 0
                        };
                        newItem=await Item.SetValue(newItem,item.value);
                        SetSlot(index, newItem);
                        AddItemIndex(itemData.id, index);
                        if (!packageItemCounts.ContainsKey(itemData.id))
                        {
                            packageItemCounts.Add(itemData.id, 0);
                        }
                    }

                    int inCount = item.count;//要放入的数量
                    while (inCount > 0)
                    {
                        Item setItem = items[index];
                        int setCount = itemData.groupCount - setItem.count;//填充一个消耗数量

                        if (inCount - setCount > 0)//剩余的数量大于0
                        {
                            setItem.count = itemData.groupCount;
                            items[index] = setItem;

                            ChangeItemCount(itemData.id, setCount);
                        }
                        else
                        {
                            setItem.count += inCount;
                            items[index] = setItem;

                            ChangeItemCount(itemData.id, inCount);
                            break;
                        }

                        inCount -= setCount;//当前剩余数量
                        if (caseCount <= itemCount)//背包格子是否还有空白
                        {
                            //oldCount += setCount - inCount;//????
                            // packageItemCounts[itemData.id] = oldCount;

                            if (display)
                            {
                                InformationController.instance.AddInformation($"{LanguageManage.SwitchStr("获得")}[{LanguageManage.SwitchStr(itemData.name)}] {item.count - inCount}{LanguageManage.SwitchStr("个")}");
                            }
                            return inCount;
                        }
                        index = TakeEmptySlot();
                        //index = itemData.groupCount - setItem.count;
                        Item item1 = new Item
                        {
                            instanceId = MyInstance.instance.Uid,
                            dataId = itemData.id,
                            packageId = instanceId,
                            isFresh = itemData.isFresh,
                            itemType = itemData.type,
                            count = 0
                        };
                        item1=await Item.SetValue(item1,item.value);

                        SetSlot(index, item1);
                        AddItemIndex(itemData.id, index);
                    }
                }
                else
                {
                    int addCount = 0;
                    for (int i = 0; i < item.count; i++)
                    {
                        if (caseCount <= itemCount)
                        {
                            break;
                        }

                        Item item1 = new Item
                        {
                            instanceId = MyInstance.instance.Uid,
                            dataId = itemData.id,
                            packageId = instanceId,
                            isFresh = itemData.isFresh,
                            itemType = itemData.type,
                            count = 1
                        };
                        item1 = await Item.SetValue(item1, item.value);
                        int index = TakeEmptySlot();
                        SetSlot(index, item1);
                        addCount++;
                        AddItemIndex(itemData.id, index);
                    }
                    // packageItemIndexDatas.Add(itemData.id, indexDatas);
                    if (addCount > 0)
                    {
                        ChangeItemCount(itemData.id, addCount);
                    }

                    int outCount= item.count - addCount;
                    if (display)
                    {
                        InformationController.instance.AddInformation($"{LanguageManage.SwitchStr("获得")}[{LanguageManage.SwitchStr(itemData.name)}] {outCount}{LanguageManage.SwitchStr("个")}");
                    }
                    return outCount;
                }
            }
            if (display)
            {
                InformationController.instance.AddInformation($"{LanguageManage.SwitchStr("获得")}[{LanguageManage.SwitchStr(itemData.name)}] {item.count}{LanguageManage.SwitchStr("个")}");
            }
            return 0;
        }

        public List<Item> GetItemFromDataId(int itemDataId)
        {
            if (packageItemIndexDatas.TryGetValue(itemDataId, out var ints))
            {
                List<Item> results = new List<Item>();
                for (int i = 0; i < ints.Count; i++)
                {
                    results.Add(items[ints[i]]);
                }
                return results;
            }
            return null;
        }

        public int TryGetItemOutPackage(int itemDataId, int count)
        {
            if (packageItemCounts.TryGetValue(itemDataId, out int itemCount))
            {
                int nowCount = 0;
                if (itemCount < count)
                {
                    nowCount = count - itemCount;
                    count = itemCount;
                }

                ChangeItemCount(itemDataId, -count);

                List<int> indexDatas = packageItemIndexDatas[itemDataId];
                int index = indexDatas.Count - 1;

                while (count > 0)
                {
                    int slot = indexDatas[index];
                    Item nowItem = items[slot];
                    if (nowItem.count > count)
                    {
                        nowItem.count -= count;
                        items[slot] = nowItem;
                        count = 0;
                    }
                    else
                    {
                        count -= nowItem.count;
                        ReleaseSlot(slot);
                        RemoveItemIndex(itemDataId, slot);
                        index--;
                    }
                }
                RefreshSelectItem();
                return nowCount;
            }
            RefreshSelectItem();
            return count;
        }

        public bool GetItemOutPackage(int itemDataId, int count)
        {
            if (packageItemCounts.TryGetValue(itemDataId, out int itemCount))
            {
                if (itemCount >= count)
                {
                    int nowCount = itemCount - count;
                    ChangeItemCount(itemDataId, -count);

                    List<int> indexDatas = packageItemIndexDatas[itemDataId];
                    int index = indexDatas.Count - 1;

                    while (count > 0)
                    {
                        int slot = indexDatas[index];
                        Item nowItem = items[slot];
                        if (nowItem.count > count)
                        {
                            nowItem.count -= count;
                            items[slot] = nowItem;
                            count = 0;
                        }
                        else
                        {
                            count -= nowItem.count;
                            ReleaseSlot(slot);
                            RemoveItemIndex(itemDataId, slot);
                            index--;
                        }
                    }

                    RefreshSelectItem();
                    return true;
                }
            }

            return false;
        }

        public void GetItemOutPackage(int itemInstanceId)
        {
            int index= items.FindIndex(item => item.instanceId == itemInstanceId);
            if (index < 0)
            {
                return;
            }

            Item item = items[index];
            if (packageItemCounts.ContainsKey(item.dataId))
            {
                ChangeItemCount(item.dataId, -item.count);
                RemoveItemIndex(item.dataId, index);
                ReleaseSlot(index);
                RefreshSelectItem();
            }
        }

        public bool IsHaveItem(int itemDataId)
        {
            return packageItemCounts.ContainsKey(itemDataId);
        }

        public bool CheckPackageTryItemIn(int itemDataId, int count)
        {
            if (caseCount < itemCount)
            {
                return false;
            }

            var itemData = GameDataManager.instance.GetData<ItemData>(itemDataId.ToString());
            if (!string.IsNullOrEmpty(itemData.name))
            {
                if (itemData.groupCount > 1)
                {
                    int totalNull = 0;
                    if (packageItemIndexDatas.TryGetValue(itemDataId, out List<int> indexDatas))
                    {
                        for (int i = 0; i < indexDatas.Count; i++)
                        {
                            int index = indexDatas[i];
                            totalNull += itemData.groupCount - items[index].count;
                        }
                    }
                    int nullCase = caseCount - itemCount;
                    return totalNull + nullCase * itemData.groupCount >= count;
                }
                else
                {
                    return caseCount - itemCount >= count;
                }
            }
            return false;
        }
        public int GetItemValue(int itemDataId)
        {
            Item item = GetItemFromInstanceId(itemDataId);
            if (item.instanceId == itemDataId)
            {
                return item.value;
            }
            var items = GetItemFromDataId(itemDataId);
            if (items!=null&&items.Count > 0)
            {
                return items[0].value;
            }
            return 0;
        }
        public int GetItemCount(int itemDataId)
        {
            if (packageItemCounts.TryGetValue(itemDataId, out int itemCount))
            {
                return itemCount;
            }
            return 0;
        }
        public int GetItemCountForInstance(int itemInstanceId)
        {
            Item item = GetItemFromInstanceId(itemInstanceId);
            return item.count;
        }
    }
}

public struct PackageList : IReferenceData
{
    public List<PackageData> packageDatas;
    public ItemMatchData itemMatchData;
    public bool canSetShortcut;
}

public enum ItemMatchType
{
    Null = 0,
    ItemType = 1,
    IsFresh = 2,
    Manufacture = 3
}

public struct ItemMatchData
{
    public ItemMatchType itemMatchType;
    public HashSet<int> matchValues;

    public async Task<bool> MatchAction(Item item)
    {
        if (matchValues == null || matchValues.Count == 0 || item.dataId == 0)
        {
            return true;
        }
        switch (itemMatchType)
        {
            case ItemMatchType.ItemType:
                return matchValues.Contains((int)item.itemType);

            case ItemMatchType.IsFresh:
                return matchValues.Contains(item.isFresh ? 1 : 0);
            case ItemMatchType.Manufacture:
                var match = false;
                var itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
                foreach (var matchValue in matchValues)
                    if (itemData.manufacture.Contains(matchValue))
                    {
                        match = true;
                        break;
                    }

                return match;
        }
        return true;
    }

    public bool MatchAction(ItemData itemData)
    {
        switch (itemMatchType)
        {
            case ItemMatchType.ItemType:
                return matchValues.Contains((int)itemData.type);

            case ItemMatchType.IsFresh:
                return matchValues.Contains(itemData.isFresh ? 1 : 0);
        }
        return true;
    }
}

public struct PackageData : IReferenceData
{
    public string name;
    public int instanceId;
    public int dataId;
    public int caseCount;
    public int level;

    public PackageType packageType;
    public List<Item> items;
}
