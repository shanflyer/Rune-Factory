using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore.Text;

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

    public async void ShowPlayerBagUse(bool close, ItemMatchData itemMatchData)
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
        warehousePanel.SetSelectItemAction((Item item,bool select) =>
        {
            UsingAction(item);
            if (close)
            {
                UIManager.instance.CloseGamePanel<WarehousePanel>();
            }
        }, "使用");
    }
    public async void ShowFightPlayerBagUse(bool close, ItemMatchData itemMatchData)
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
        warehousePanel.SetSelectItemAction((Item item, bool select) =>
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

    public async void ShowAllPlayerPackage(SelectAction<Item> selectItemAction, string actionName)
    {
        PackageList packageList = new PackageList
        {
            packageDatas = new List<PackageData>()
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
                    instanceId =MyInstance.instance.uid,
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
        GameActionManager.instance.AddListener<ItemUseAction>(UsetItem);
        GameActionManager.instance.AddListener<CreatRuntimePackage>(CreatRuntimePackage);
        GameActionManager.instance.AddListener<RemoveRuntimePackage>(RemoveRuntimePackage);
        GameActionManager.instance.AddListener<AddPackageItem>(AddPackageItemAction);
        GameActionManager.instance.AddListener<OpenPackage>(OpenPackage);
        GameActionManager.instance.AddListener<GiveGift>(GiveGift);
        GameActionManager.instance.AddListener<CheckItemValue>(CheckItemValue);
        GameActionManager.instance.AddListener<CreatPackage>(CreatPackage);
        GameActionManager.instance.AddListener<RemovePackage>(RemovePackage);
        GameActionManager.instance.AddListener<RemovePackageItem>(RemovePackageItemAction);
        GameActionManager.instance.AddListener<ShowMultiPackagePanel>(ShowMultiPackagePanel);
        GameActionManager.instance.AddListener<SetPackageSelectItem>(SetPackageSelectItem);
        GameActionManager.instance.AddListener<RemovePlayerPackageItem>(RemovePlayerPackageItem);
        GameActionManager.instance.AddListener<AddItemValue>(AddItemValue);
        GameActionManager.instance.AddListener<SetItemValue>(SetItemValue);
        GameActionManager.instance.AddListener<CheckCharacterItemValue>(CheckCharacterItemValue);
        GameActionManager.instance.AddListener<CheckCharacterPackageFull>(CheckCharacterPackageFull);
        GameActionManager.instance.AddListener<ChangePackageInnstance>(ChangePackageInnstance);
        GameActionManager.instance.AddListener<RefreshShortcut>(RefreshShortcut);
        GameActionManager.instance.AddListener<RemovePackageItemInstance>(RemovePackageItemInstance);
        GameActionManager.instance.AddListener<AddPackageItemList>(AddPackageItemList);
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

    private void AddItemValue(AddItemValue addItemValue)
    {
        Character character = CharacterManager.instance.GetCharacter(addItemValue.characterId);
        if (character != null)
        {
            if (gamePackages.TryGetValue(character.characterPackage, out var gamePackage))
            {
                float value = gamePackage.AddItemValue(addItemValue.selectItem, addItemValue.value * 0.01f);
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

    private void SetItemValue(SetItemValue setItemValue)
    {
        Character character = CharacterManager.instance.GetCharacter(setItemValue.characterId);
        if (character != null)
        {
            if (gamePackages.TryGetValue(character.characterPackage, out var gamePackage))
            {
                float value = gamePackage.SetItemValue(setItemValue.selectItem, setItemValue.value * 0.01f);
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

    private async void ShowMultiPackagePanel(ShowMultiPackagePanel showMultiPackagePanel)
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

    private async void CreatPackage(CreatPackage creatPackage)
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
                if (item.value * 100 >= CheckCharacterItemValue.itemValue)
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
                        totalValue += (int)items[i].value * 100;
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
                if (item.value >= checkItemValue.itemValue * 0.01f)
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

    private async void GiveGift(GiveGift giveGift)
    {
        Character receiveCharacter = CharacterManager.instance.GetCharacter(giveGift.receiveCharacter);
        if (receiveCharacter != null)
        {
           await SetItemInPackage(new Item(giveGift.giftId, 1), receiveCharacter.characterPackage);
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
    async void LoadObjPackageAnimationData()
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

    private async void OpenPackage(OpenPackage openPackage)
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
        
            GameActionData gameActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(openPackage.selectActionId);
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

    public async void InitFromSaveData(List<PackageSaveData> packageSaveDatas)
    {
        for (int i = 0; i < packageSaveDatas.Count; i++)
        {
            var saveData = packageSaveDatas[i];
            var packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(saveData.dataId);

            GamePackage gamePackage = new GamePackage(saveData.caseCount,
                saveData.packageName, saveData.id, packageSetData, saveData.level)
            {
                itemPackage = saveData.itemPackage,
                
            };
            gamePackage.InitSaveItemList(saveData.items);
            MyInstance.instance.AddInstance(saveData.id);
            gamePackages.Add(saveData.id, gamePackage);
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
                    items = gamePackage.GetItems()
                };
                packageSaveDatas.Add(packageSaveData);
            }
        }
        return packageSaveDatas;
    }

    public async Task<bool> CheckPackageTryItemIn(int packageId, int itemDataId, int count)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return await gamePackage.CheckPackageTryItemIn(itemDataId, count);
        }
        return false;
    }

    public int GetPackageItemCount(int packageId, int itemDataId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.GetItemCount(itemDataId);
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
    private async void AddPackageItemList(AddPackageItemList addPackageItem)
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
                int intanceId = MyInstance.instance.uid;
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
   
    private async void AddPackageItemAction(AddPackageItem addPackageItem)
    {
        if (addPackageItem.packageId == 0)
        {
            addPackageItem.packageId = CharacterManager.instance.controllerCharacter.characterPackage;
        } 
        if (gamePackages.TryGetValue(addPackageItem.packageId, out GamePackage gamePackage))
        {

            int intanceId = MyInstance.instance.uid;
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

    private async void CreatRuntimePackage(CreatRuntimePackage creatRuntimePackage)
    {
        int instanceId = creatRuntimePackage.instanceId;
        if (instanceId < 0)
        {
            instanceId = MyInstance.instance.uid;
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
        int packageInstaceId = instanceId == 0 ? MyInstance.instance.uid : instanceId;
        if(gamePackages.ContainsKey(packageInstaceId))
        {
            return -1;
        }


        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(dataId); 
        int nowCount = packageSetData.count + packageSetData.levelUpAddCount * level;
        GamePackage gamePackage = new GamePackage(nowCount, packageSetData.name, packageInstaceId, packageSetData,level);
        gamePackages.Add(packageInstaceId, gamePackage);

        if (level <= 1)
        {
            for (int i = 0; i < packageSetData.initItems.Count; i++)
            {
              await  gamePackage.SetItemInPackage(
                    new Item(packageSetData.initItems[i].x, packageSetData.initItems[i].y));
            }

            RefreshShortcut refreshShortcut = new RefreshShortcut
            {
                packageId = packageInstaceId, 
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
        return packageInstaceId;
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

    private async void UsetItem(ItemUseAction itemUseEvent)
    {
        if (gamePackages.TryGetValue(itemUseEvent.packageId, out GamePackage gamePackage))
        {
            if (await UsetItemAction(itemUseEvent.itemId,itemUseEvent.itemInstance))
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

    private async Task<bool> UsetItemAction(int itemId,int itemInstance)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(itemId.ToString());
        if (itemData != null )
        {
            if (ExploreManager.instance.isExplore&& itemData.sceneType==SceneType.城镇)
            {
                InformationController.instance.AddInformation("什么也没发生", true, true);
                return true;
            }
            if (!ExploreManager.instance.isExplore && itemData.sceneType == SceneType.战斗)
            {
                InformationController.instance.AddInformation("什么也没发生", true, true);
                return true;
            }
            if (itemData.useEventId!=0)
            {
                List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>
                {
                   new EventReferenceData
                   {
                       name="CharacterId",
                       valueType=ReferenceValueType.Int,
                       value=CharacterManager.instance.controllerCharacter.instanceId
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
                };
                GameEventManager.instance.AddGameEvent(itemData.useEventId, eventReferenceDatas); 
            }
           
            return true;
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
        public bool singleCase=>packageSetData.singleCase;
        public int level;
        public bool itemPackage; 
        private List<Item> items;
        public int itemCount => items.Count - nullItems.Count;
        public PackageType packageType=>packageSetData.packageType;
        private Queue<int> nullItems;

        public Dictionary<int, int> PackageItemCounts => packageItemCounts;

        private Dictionary<int, int> packageItemCounts;
        private Dictionary<int, List<int>> packageItemIndexDatas;

        public int SelectItem;

        public void SortItem(int itemId,int index)
        {
            if(index< items.Count && packageItemIndexDatas.TryGetValue(itemId,out var list))
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
                            nullItems.Enqueue(i);
                        }
                    }
                    newNullItems.Enqueue(oldIndex);
                    nullItems = newNullItems; 
                }
                else
                {
                    Item item0 = items[index];
                    var list0 = packageItemIndexDatas[item0.dataId];
                    list0.Remove(index);
                    list0.Add(oldIndex);
                    items[oldIndex] = item0;
                }
                items[index] = item;
                list[list.Count - 1] = index;
            }
        }
        public void InitSaveItemList(List<Item> items)
        {
            this.items = new List<Item>();
            for(int i = 0; i < items.Count; i++)
            {
                if (packageItemCounts.TryGetValue(items[i].dataId,out var count))
                {
                    count += items[i].count;
                }
                count = items[i].count;
                packageItemCounts[items[i].dataId] = count;
                if (packageItemIndexDatas.TryGetValue(items[i].dataId,out var indexs))
                {
                    indexs.Add(i);
                }
                else
                {
                    indexs = new List<int>();
                    indexs.Add(i);
                    packageItemIndexDatas.Add(items[i].dataId, indexs);
                }
                this.items.Add(items[i]);
            }
        }
        public Item GetItemFromInstanceId(int itemInstanceId)
        {
            return items.Find(item => item.instanceId == itemInstanceId);
        }

        public PackageData OutGamePackageData()
        {
            PackageData packageData = new PackageData
            {
                caseCount = caseCount,
                instanceId = instanceId,
                dataId = packageSetData.id,
                name = name,
                level = level,
                items = GetItems(),
            };
            return packageData;
        }

        public GamePackage() { }
       
        public GamePackage(int caseCount, string name, int instanceId, PackageSetData packageSetData, int level = 0 )
        {
            this.instanceId = instanceId;
            this.packageSetData= packageSetData; 
            this.level = level; 
            this.name = name;
            this.caseCount = caseCount;
            items = new List<Item>();
            packageItemCounts = new Dictionary<int, int>();
            packageItemIndexDatas = new Dictionary<int, List<int>>();
            itemPackage = false;
            nullItems = new Queue<int>();
            SelectItem = 0;
        }

        public float SetItemValue(int instanceId, float value)
        {
            int index = items.FindIndex(item => item.instanceId == instanceId);
            if (index >= 0)
            {
                var item = items[index];
                item.value = value;
                items[index] = item;
                return item.value;
            }
            else
            {
                return -1;
            }
        }

        public float AddItemValue(int instanceId, float value)
        {
            int index = items.FindIndex(item => item.instanceId == instanceId);
            if (index >= 0)
            {
                var item = items[index];
                item.value += value;
                if (item.value < 0)
                {
                    item.value = 0;
                }
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
                    nullItems.Enqueue(indexs[i]);
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
                item.instanceId = MyInstance.instance.uid;
            }

            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
            if (!string.IsNullOrEmpty(itemData.name))
            {
                if (packageType == PackageType.鲜活 && !itemData.isFresh)
                {
                    return item.count;
                }
                if (packageType == PackageType.非鲜活 && itemData.isFresh)
                {
                    return item.count;
                }
                int groupCount = singleCase ? 1 : itemData.groupCount;

                List<int> indexDatas = new List<int>();
                if (!packageItemIndexDatas.TryGetValue(item.dataId, out indexDatas))
                {
                    indexDatas = new List<int>();
                    packageItemIndexDatas.Add(item.dataId, indexDatas);
                }// 如果背包内没有同类物体，则新建索引

                if (groupCount > 1)//物体堆叠数量
                {
                    int index ;//空物体位置
                    if (indexDatas.Count > 0)
                    {
                        index = indexDatas[indexDatas.Count - 1];//旧物体最后一个未填满的位置
                    }
                    else
                    {
                        index = nullItems.Count != 0 ? nullItems.Dequeue() : items.Count;//空物体位置
                        Item newItem = new Item
                        {
                            instanceId = MyInstance.instance.uid,
                            dataId = itemData.id,
                            packageId = instanceId,
                            isFresh = itemData.isFresh,
                            itemType = itemData.type,
                            value = item.value,
                            count = 0
                        };
                        if (items.Count <= index)
                        {
                            items.Add(newItem);
                        }
                        else
                        {
                            items[index] = newItem;
                        }
                        indexDatas.Add(index);
                        packageItemCounts.Add(itemData.id, 0);
                    }

                    int inCount = item.count;//要放入的数量
                    int oldCount = 0;
                    packageItemCounts.TryGetValue(itemData.id, out oldCount);//旧有数量
                    while (inCount > 0)
                    {
                        Item setItem = items[index];
                        int setCount = itemData.groupCount - setItem.count;//填充一个消耗数量

                        if (inCount - setCount > 0)//剩余的数量大于0
                        {
                            setItem.count = itemData.groupCount;
                            items[index] = setItem;

                            oldCount += setCount;
                            packageItemCounts[itemData.id] = oldCount;
                        }
                        else
                        {
                            setItem.count += inCount;
                            items[index] = setItem;

                            oldCount += inCount;
                            packageItemCounts[itemData.id] = oldCount;
                            break;
                        }

                        inCount -= setCount;//当前剩余数量
                        if (caseCount <= itemCount)//背包格子是否还有空白
                        {
                            //oldCount += setCount - inCount;//????
                            // packageItemCounts[itemData.id] = oldCount;

                            if (display)
                            {
                                InformationController.instance.AddInformation($"获得[{itemData.name}] {item.count - inCount}个");
                            }
                            return inCount;
                        }
                        index = nullItems.Count != 0 ? nullItems.Dequeue() : items.Count;
                        //index = itemData.groupCount - setItem.count;
                        Item item1 = new Item
                        {
                            instanceId = MyInstance.instance.uid,
                            dataId = itemData.id,
                            packageId = instanceId,
                            isFresh = itemData.isFresh,
                            itemType = itemData.type,
                            value = item.value,
                            count = 0
                        };

                        if (items.Count <= index)
                        {
                            items.Add(item1);
                        }
                        else
                        {
                            items[index] = item1;
                        }
                        indexDatas.Add(index);
                    }
                    packageItemIndexDatas[itemData.id] = indexDatas;
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
                            instanceId = MyInstance.instance.uid,
                            dataId = itemData.id,
                            packageId = instanceId,
                            isFresh = itemData.isFresh,
                            itemType = itemData.type,
                            value = item.value,
                            count = 1
                        };
                        int index = items.Count;
                        if (nullItems.Count > 0)
                        {
                            index = nullItems.Dequeue();
                            items[index] = item1;
                        }
                        else
                        {
                            items.Add(item1);
                        }
                        addCount++;
                        indexDatas.Add(index);
                    }
                    // packageItemIndexDatas.Add(itemData.id, indexDatas);
                    if (addCount > 0)
                    {
                        packageItemCounts.TryGetValue(itemData.id, out int count);
                        count += addCount;
                        packageItemCounts[itemData.id] = count;
                    }

                    int outCount= item.count - addCount;
                    if (display)
                    {
                        InformationController.instance.AddInformation($"获得[{itemData.name}] {outCount}个");
                    }
                    return outCount;
                }
            }
            if (display)
            {
                InformationController.instance.AddInformation($"获得[{itemData.name}] {item.count}个");
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

                packageItemCounts[itemDataId] = itemCount - count;
                List<int> indexDatas = packageItemIndexDatas[itemDataId];
                int index = indexDatas.Count - 1;

                while (count > 0)
                {
                    Item nowItem = items[indexDatas[index]];
                    if (nowItem.count > count)
                    {
                        nowItem.count -= count;
                        items[indexDatas[index]] = nowItem;
                        count = 0;
                    }
                    else
                    {
                        count -= nowItem.count;
                        MyInstance.instance.RemoveInstance(nowItem.instanceId);
                        nullItems.Enqueue(indexDatas[index]);
                        itemCount--;
                        indexDatas.RemoveAt(index);
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
                    if (nowCount > 0)
                    {
                        packageItemCounts[itemDataId] = nowCount;
                    }
                    else
                    {
                        packageItemCounts.Remove(itemDataId);
                    }

                    List<int> indexDatas = packageItemIndexDatas[itemDataId];
                    int index = indexDatas.Count - 1;

                    while (count > 0)
                    {
                        Item nowItem = items[indexDatas[index]];
                        if (nowItem.count > count)
                        {
                            nowItem.count -= count;
                            items[indexDatas[index]] = nowItem;
                            count = 0;
                        }
                        else
                        {
                            count -= nowItem.count;
                            MyInstance.instance.RemoveInstance(nowItem.instanceId);
                            nullItems.Enqueue(indexDatas[index]);
                            itemCount -= nowItem.count;
                            items[indexDatas[index]] = default(Item);
                            indexDatas.RemoveAt(index);
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
            if (packageItemCounts.TryGetValue(items[index].dataId, out int itemCount))
            {
                int nowCount = itemCount - items[index].count;
                if (nowCount > 0)
                {
                    packageItemCounts[items[index].dataId] = nowCount;
                }
                else
                {
                    packageItemCounts.Remove(items[index].dataId);
                }
                RefreshSelectItem();
                items[index] = default(Item); 
            }
        }

        public bool IsHaveItem(int itemDataId)
        {
            return packageItemCounts.ContainsKey(itemDataId);
        }

        public async Task<bool> CheckPackageTryItemIn(int itemDataId, int count)
        {
            if (caseCount < itemCount)
            {
                return false;
            }

            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(itemDataId.ToString());
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

        public int GetItemCount(int itemDataId)
        {
            if (packageItemCounts.TryGetValue(itemDataId, out int itemCount))
            {
                return itemCount;
            }
            return 0;
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
    Null = 0, ItemType = 1, IsFresh = 2
}

public struct ItemMatchData
{
    public ItemMatchType itemMatchType;
    public HashSet<int> matchValues;

    public bool MatchAction(Item item)
    {
        if (matchValues == null || matchValues.Count == 0)
        {
            return true;
        }
        switch (itemMatchType)
        {
            case ItemMatchType.ItemType:
                return matchValues.Contains((int)item.itemType);

            case ItemMatchType.IsFresh:
                return matchValues.Contains(item.isFresh ? 1 : 0);
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