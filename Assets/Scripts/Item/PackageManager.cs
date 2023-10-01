using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
public class PackageManager : Singleton<PackageManager>
{
    private List<int> playerPackages = new List<int>();
    public async void ShowAllPlayerPackage(SelectAction<Item> selectItemAction, string actionName)
    {
        PackageList packageList = new PackageList
        {
            packageDatas = new List<PackageData>()
        };
        for(int i = 0; i < playerPackages.Count; i++)
        {
            if (gamePackages.TryGetValue(playerPackages[i],out GamePackage gamePackage))
            {
                packageList.packageDatas.Add(gamePackage.OutGamePackageData());
            }
        }
        var warehousePanel=await  UIManager.instance.ShowGamePanel<WarehousePanel, PackageList>(packageList);
        warehousePanel.SetSelectItemAction(selectItemAction,actionName);
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



    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<ItemUseAction>(UsetItem);
        GameActionManager.instance.AddListener<CreatRuntimePackage>(CreatRuntimePackage);
        GameActionManager.instance.AddListener<RemoveRuntimePackage>(RemoveRuntimePackage);
        GameActionManager.instance.AddListener<AddPackageItem>(AddPackageItemAction);
    }

    private Dictionary<int, GamePackage> gamePackages = new Dictionary<int, GamePackage>();
    private Dictionary<Vector2Int, int> runtimePackageRuntimes = new Dictionary<Vector2Int, int>();
    private int nowPackageId;

    public void InitFromSaveData(List<PackageSaveData> packageSaveDatas)
    {
        for(int i = 0; i < packageSaveDatas.Count; i++)
        {
            var saveData = packageSaveDatas[i];
            GamePackage gamePackage = new GamePackage(saveData.caseCount, saveData.packageName, saveData.id) 
            { 
                itemPackage = saveData.itemPackage
            };
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
                    packageName = gamePackage.name,
                    itemPackage=gamePackage.itemPackage,
                    items = gamePackage.GetItems()
                };
                packageSaveDatas.Add(packageSaveData);
            }
        }
        return packageSaveDatas;
    }

    public async Task<bool> CheckPackageTryItemIn(int packageId,int itemDataId, int count)
    {
        if(gamePackages.TryGetValue(packageId,out GamePackage gamePackage))
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
    public int AddPackageCaseCount(int packageId,int count)
    {
        if(gamePackages.TryGetValue(packageId,out GamePackage gamePackage))
        {
            gamePackage.caseCount += count;
            return gamePackage.caseCount;
        }
        return 0;
    }
    public void SetPackageCaseCount(int packageId, int count)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            gamePackage.caseCount = count;
        }
    }
    private void RemovePackageItemAction(RemovePackageItem removePackageItem)
    {
        if (gamePackages.TryGetValue(removePackageItem.packageId, out GamePackage gamePackage))
        {
            gamePackage.GetItemOutPackage(removePackageItem.itemDataId, removePackageItem.itemCount);
        }
        if (nowPackageId == removePackageItem.packageId)
        {

        }
    }
    private async void AddPackageItemAction(AddPackageItem addPackageItem)
    {
        if (gamePackages.TryGetValue(addPackageItem.packageId, out GamePackage gamePackage))
        {
            int intanceId = ItemManager.instance.CreatIntance();
            await gamePackage.SetItemInPackage(new Item
            {
                instanceId = intanceId,
                dataId = addPackageItem.itemDataId,
                count = addPackageItem.itemCount
            });

            gamePackages[addPackageItem.packageId] = gamePackage;
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
        int instanceId= creatRuntimePackage.instanceId;
        if (instanceId < 0)
        {
            instanceId = ItemManager.instance.CreatIntance();
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

    public int CreatGamePackage(int caseCount, string name = null, int packageInstaceId = 0)
    {
        nowPackageId++;
        packageInstaceId = nowPackageId;
        GamePackage gamePackage = new GamePackage(caseCount, name, packageInstaceId);
        gamePackages.Add(packageInstaceId, gamePackage);

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

    public async Task<int> SetItemInPackage(Item item, int packageId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return await gamePackage.SetItemInPackage(item);
        }

        return -1;
    }
    public bool IsHaveItem(int packageId,int itemDataId)
    {
        if(gamePackages.TryGetValue(packageId,out GamePackage gamePackage))
        {
            return gamePackage.IsHaveItem(itemDataId);
        }
        return false;
    }
    private void UsetItem(ItemUseAction itemUseEvent)
    {
        if (gamePackages.TryGetValue(itemUseEvent.packageId, out GamePackage gamePackage))
        {
            UsetItemAction(itemUseEvent.itemId);
            gamePackage.GetItemOutPackage(itemUseEvent.itemId, itemUseEvent.itemCount);
        }
    }
    async void UsetItemAction(int itemId)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(itemId.ToString());

        for (int i = 0; i < itemData.useEventId.Count; i++)
        {
            GameActionData gameActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(itemData.useEventId[i].ToString());
            gameActionData.Action();
        }

    }

    struct GamePackage
    {
        public string name;
        public int instanceId;
        public int caseCount;
        public bool itemPackage;
        private List<Item> items; 
        public int itemCount;

        private Queue<int> nullItems;
        private Dictionary<int, int> packageItemCounts;
        private Dictionary<int, List<int>> packageItemIndexDatas;

        public PackageData OutGamePackageData()
        {
            PackageData packageData = new PackageData
            {
                caseCount = caseCount,
                instanceId = instanceId,
                name = name,
                items = items,
            };
            return packageData;
        }

        public GamePackage(int caseCount, string name, int id)
        {
            if (id < 0)
            {
                instanceId = ItemManager.instance.CreatIntance();
            }
            else
            {
                instanceId = id;
            }
            
            itemCount = 0;
            this.name = name;
            this.caseCount = caseCount;
            items = new List<Item>();
            packageItemCounts = new Dictionary<int, int>();
            packageItemIndexDatas = new Dictionary<int, List<int>>();
            itemPackage = false;
            nullItems = new Queue<int>();
        }

        public List<Item> GetItems()
        {
            List<Item> results=new List<Item>();
            for(int i=0;i<items.Count;i++)
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
            if(packageItemIndexDatas.TryGetValue(itemDataId,out var indexs))
            {
                for(int i = 0; i < indexs.Count; i++)
                {
                    nullItems.Enqueue(indexs[i]);
                }
                packageItemIndexDatas.Remove(itemDataId);
                packageItemCounts.Remove(itemDataId);
            }
        }
        public async Task<int> SetItemInPackage(Item item)
        {
            if (caseCount < itemCount)
            {
                return item.count;
            }

            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
            if (!string.IsNullOrEmpty(itemData.name))
            {
                if (itemData.groupCount > 1)
                {
                    List<int> indexDatas = new List<int>();
                    if (!packageItemIndexDatas.TryGetValue(item.dataId, out indexDatas))
                    {
                        indexDatas = new List<int>();
                        packageItemIndexDatas.Add(item.dataId, indexDatas);
                    }


                    int index =nullItems.Count!=0?nullItems.Dequeue(): items.Count;
                    if (indexDatas.Count > 0)
                    {
                        index = indexDatas[indexDatas.Count - 1];
                    }
                    else
                    {
                        Item newItem = new Item
                        {
                            instanceId = ItemManager.instance.CreatIntance(),
                            dataId = itemData.id,
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
                        itemCount++;
                        indexDatas.Add(index);
                        packageItemCounts.Add(itemData.id, 0);
                    }


                    int inCount = item.count;
                    int oldCount = 0;
                    while (inCount > 0)
                    {
                        Item setItem = items[index];
                        int setCount = itemData.groupCount - setItem.count;

                        if (inCount - setCount > 0)
                        {
                            setItem.count = itemData.groupCount;
                            items[index] = setItem;

                            oldCount += setCount;
                            packageItemCounts[itemData.id] = oldCount;
                        }
                        else
                        {
                            setItem.count = inCount;
                            items[index] = setItem;

                            oldCount += inCount;
                            packageItemCounts[itemData.id] = oldCount;
                            break;
                        }

                        inCount -= setCount;
                        if (caseCount <= items.Count)
                        {
                            oldCount += setCount - inCount;
                            packageItemCounts[itemData.id] = oldCount;
                            return inCount;
                        }

                        index = itemData.groupCount - setItem.count;
                        Item item1 = new Item
                        {
                            instanceId = ItemManager.instance.CreatIntance(),
                            dataId = itemData.id,
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
                        itemCount++; 
                        indexDatas.Add(index);
                    }
                    packageItemIndexDatas[itemData.id] = indexDatas;
                }
                else
                {
                    if (nullItems.Count > 0)
                    {
                        items[nullItems.Dequeue()] = item;
                    }
                    else
                    {
                        items.Add(item);
                    }
                    itemCount++;
                }
            }

            return 0;
        }
        public bool GetItemOutPackage(int itemDataId, int count)
        {
            if (packageItemCounts.TryGetValue(itemDataId, out int itemCount))
            {
                if (itemCount >= count)
                {
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
                            ItemManager.instance.DeleteItem(nowItem.instanceId);
                            nullItems.Enqueue(indexDatas[index]);
                            itemCount--;
                            indexDatas.RemoveAt(index);
                            index--;
                        }
                    }

                    return true;
                }
            }

            return false;
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
                        for(int i = 0; i < indexDatas.Count; i++)
                        {
                            int index = indexDatas[i];
                            totalNull += itemData.groupCount-items[index].count;
                        }
                    }
                    int nullCase = caseCount - itemCount;
                    return totalNull + nullCase * itemData.groupCount >= count;
                }
                else
                {
                    return caseCount-itemCount>= count;
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
}
public struct PackageData
{
    public string name;
    public int instanceId;
    public int caseCount;
    public List<Item> items;
     
}