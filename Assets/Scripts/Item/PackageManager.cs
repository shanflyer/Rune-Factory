using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public partial class PackageManager : Singleton<PackageManager>
{
    public List<int> playerPackages = new List<int>();

    public void AddPlayerPackage(int id)
    {
        if (!playerPackages.Contains(id))
        {
            playerPackages.Add(id);
        }
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
            RefreshPackageChanged(gamePackage, true);
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
                RefreshPackageChanged(gamePackage);
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
            RefreshPackageChanged(gamePackage);
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

            RefreshPackageChanged(gamePackage);
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

            RefreshPackageChanged(gamePackage);
        }

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
            RefreshPackageChanged(gamePackage);
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
            RefreshPackageChanged(gamePackage, true);
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

    public Item GetItemFromInstanceId(int packageId, int itemInstanceId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.GetItemFromInstanceId(itemInstanceId);
        }
        return default(Item);
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
