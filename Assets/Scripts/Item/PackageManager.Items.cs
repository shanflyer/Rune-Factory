using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;

public partial class PackageManager
{
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
