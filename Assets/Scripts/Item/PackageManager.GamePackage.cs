using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class PackageManager
{
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
        public int itemCount => items.Count - emptySlots.Count;
        public PackageType packageType => packageSetData != null ? packageSetData.packageType : default(PackageType);
        private Queue<int> emptySlotQueue;
        private HashSet<int> emptySlots;

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
                if (emptySlots.Contains(index))
                {
                    emptySlots.Remove(index);
                    ReleaseSlot(oldIndex);
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
                ValidateIndexes();
            }
        }
        public void InitSaveItemList(List<Item> items)
        {
            this.items = new List<Item>();
            packageItemCounts.Clear();
            packageItemIndexDatas.Clear();
            emptySlotQueue.Clear();
            emptySlots.Clear();
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
            ValidateIndexes();
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
            emptySlotQueue = new Queue<int>();
            emptySlots = new HashSet<int>();
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
            while (emptySlotQueue.Count != 0)
            {
                int slot = emptySlotQueue.Dequeue();
                if (emptySlots.Remove(slot))
                {
                    return slot;
                }
            }

            return items.Count;
        }

        private void SetSlot(int index, Item item)
        {
            emptySlots.Remove(index);
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

            if (emptySlots.Add(index))
            {
                emptySlotQueue.Enqueue(index);
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

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void ValidateIndexes()
        {
            Dictionary<int, int> actualCounts = new Dictionary<int, int>();
            Dictionary<int, HashSet<int>> actualIndexes = new Dictionary<int, HashSet<int>>();

            foreach (int slot in emptySlots)
            {
                if (slot < 0 || slot >= items.Count)
                {
                    Debug.LogError($"Package {instanceId} has invalid empty slot index {slot}.");
                }
                else if (items[slot].count != 0 || items[slot].dataId != 0)
                {
                    Debug.LogError($"Package {instanceId} empty slot {slot} still contains item {items[slot].dataId}.");
                }
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (emptySlots.Contains(i))
                {
                    continue;
                }

                Item item = items[i];
                if (item.count <= 0)
                {
                    Debug.LogError($"Package {instanceId} slot {i} has non-positive item count.");
                    continue;
                }

                actualCounts.TryGetValue(item.dataId, out int count);
                actualCounts[item.dataId] = count + item.count;

                if (!actualIndexes.TryGetValue(item.dataId, out var indexes))
                {
                    indexes = new HashSet<int>();
                    actualIndexes.Add(item.dataId, indexes);
                }

                indexes.Add(i);
            }

            foreach (var count in actualCounts)
            {
                if (!packageItemCounts.TryGetValue(count.Key, out int indexedCount) || indexedCount != count.Value)
                {
                    Debug.LogError($"Package {instanceId} item {count.Key} count index mismatch. actual={count.Value}, indexed={indexedCount}");
                }
            }

            foreach (var count in packageItemCounts)
            {
                if (!actualCounts.ContainsKey(count.Key))
                {
                    Debug.LogError($"Package {instanceId} item {count.Key} count index exists without slots.");
                }
            }

            foreach (var indexData in actualIndexes)
            {
                if (!packageItemIndexDatas.TryGetValue(indexData.Key, out var indexedSlots))
                {
                    Debug.LogError($"Package {instanceId} item {indexData.Key} missing slot index list.");
                    continue;
                }

                if (indexedSlots.Count != indexData.Value.Count)
                {
                    Debug.LogError($"Package {instanceId} item {indexData.Key} slot index count mismatch.");
                }

                for (int i = 0; i < indexedSlots.Count; i++)
                {
                    if (!indexData.Value.Contains(indexedSlots[i]))
                    {
                        Debug.LogError($"Package {instanceId} item {indexData.Key} has stale slot index {indexedSlots[i]}.");
                    }
                }
            }
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
                if (!emptySlots.Contains(i))
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
                    if (!emptySlots.Contains(i))
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
                if (!emptySlots.Contains(i))
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
            ValidateIndexes();
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
                ValidateIndexes();
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
                    ValidateIndexes();
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
                ValidateIndexes();
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
