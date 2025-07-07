using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using Unity.Collections; 

public class ShortcutManager : Singleton<ShortcutManager>
{
    Dictionary<int,ShortcutPackage> shortcutPackages = new Dictionary<int, ShortcutPackage>();
    public ShortcutPackage playerShortcutPackage => GetShortcutPackage(CharacterManager.instance.controllerCharacter.instanceId);
    public ShortcutPackage GetShortcutPackage(int characterId)
    {
        if(!shortcutPackages.TryGetValue(characterId,out var shortcutPackage))
        {
            shortcutPackage =new ShortcutPackage(characterId); 
            shortcutPackages.Add(characterId,shortcutPackage );
        }
        return shortcutPackage;
    }
    public override void Init()
    {
        base.Init();
        shortcutPackages.Clear();
        GameActionManager.instance.AddListener<RemoveShortcutItem>(RemoveShortcutItem);
        GameActionManager.instance.AddListener<SetShortcutItem>(SetShortcutItem);
        GameActionManager.instance.AddListener<RefreshShortcut>(RefreshShortcutAsync);
        GameActionManager.instance.AddListener<SortShortcutItem>(SortShortcutItem);
        GameActionManager.instance.AddListener<RefreshItemValue>(RefreshItemValue);
    }
    protected override void Clear()
    {
        base.Clear();
    }
    void SortShortcutItem(SortShortcutItem sortShortcutItem)
    {
        if (ExploreManager.instance.isExplore)
        {
            return;
        }
        var shortcutPackage = GetShortcutPackage(CharacterManager.instance.controllerCharacter.instanceId);
        shortcutPackage.ChangeItem(sortShortcutItem.itemId, sortShortcutItem.index);
        RefreshShortcut refreshShortcut = new RefreshShortcut
        {
            packageId = CharacterManager.instance.controllerCharacter.characterPackage
        };
        GameActionManager.instance.QueueAction(refreshShortcut);
    }
    async void RefreshShortcutAsync(RefreshShortcut refreshShortcut)
    {
        if (ExploreManager.instance.isExplore)
        {
            return;
        }
        if (refreshShortcut.packageId == CharacterManager.instance.controllerCharacter.characterPackage)
        {
            var shortcutPackage = GetShortcutPackage(CharacterManager.instance.controllerCharacter.instanceId);
            for (int i = 0; i < shortcutPackage.items.Length; i++)
            {
                var item = shortcutPackage.items[i];
                if (item.dataId != 0)
                {
                    int itemCount =await item.IsSingleItem()?PackageManager.instance.GetPackageItemCountForInstance(CharacterManager.instance.controllerCharacter.characterPackage, item.instanceId):
                    PackageManager.instance.GetPackageItemCount(CharacterManager.instance.controllerCharacter.characterPackage, item.dataId);
                    if (itemCount <= 0)
                    {
                        shortcutPackage.haveItems.Remove(shortcutPackage.items[i].instanceId);
                        shortcutPackage.items[i] = default(Item);
                    }
                    else
                    { 
                        if (item.instanceId != 0)
                        {
                            item = await Item.SetValue(item, PackageManager.instance.GetPackageItemValue(CharacterManager.instance.controllerCharacter.characterPackage, item.instanceId));
                        }
                        else
                        {
                            item = await Item.SetValue(item, PackageManager.instance.GetPackageItemValue(CharacterManager.instance.controllerCharacter.characterPackage, item.dataId));
                        }
                        

                        item.count = itemCount;
                        shortcutPackage.items[i] = item;
                        shortcutPackage.haveItems.Add(shortcutPackage.items[i].instanceId);
                    }
                }
            }
            shortcutPackages[shortcutPackage.Key]=(shortcutPackage);
             UIManager.instance.GetGamePanel<ShortcutPanel>(SetPanel: shortcutPanel =>
            {
                if (shortcutPanel != null)
                {
                    shortcutPanel.InitReferenceData(shortcutPackage);
                }
            });
            
            // UIManager.instance.ShowGamePanel<ShortcutPanel, ShortcutPackage>(shortcutPackage);

        }
    }
    void RefreshDisplayShortcutPackageAsync(ShortcutPackage shortcutPackage)
    {
        if (CharacterManager.instance.controllerCharacter.instanceId == shortcutPackage.characterId)
        {
            UIManager.instance.GetGamePanel<ShortcutPanel>(SetPanel: shortcutPanel =>
            {
                if (shortcutPanel != null)
                {
                    shortcutPanel.InitReferenceData(shortcutPackage);
                }
            });
            
        }
    }
    void RefreshItemValue(RefreshItemValue refreshItemValue)
    {
        var shortcutPackage= GetShortcutPackage(refreshItemValue.characterId);
        shortcutPackage.SetItemValue(refreshItemValue.itemId, refreshItemValue.itemValue);

    }
    void RemoveShortcutItem(RemoveShortcutItem removeShortcutItem)
    {
        var shortcutPackage = GetShortcutPackage(removeShortcutItem.characterId);

        shortcutPackage.RemoveItemIndex(removeShortcutItem.index);
        //shortcutPackages.SetData(shortcutPackage);
        RefreshDisplayShortcutPackageAsync(shortcutPackage);
    }
   void SetShortcutItem(SetShortcutItem setShortcutItem)
    {
        var shortcutPackage = GetShortcutPackage(setShortcutItem.characterId);
        if (shortcutPackage.SetItem(setShortcutItem.Item))
        {
            SetPackageSelectItem setPackageSelectItem = new SetPackageSelectItem
            {
                packageId = shortcutPackage.packagerId,
                selectItem = setShortcutItem.Item.instanceId!=0?setShortcutItem.Item.instanceId: setShortcutItem.Item.dataId,
            };
            GameActionManager.instance.QueueAction(setPackageSelectItem);


            //shortcutPackages.SetData(shortcutPackage);
            RefreshDisplayShortcutPackageAsync(shortcutPackage);
        }
    }
   
}
public class ShortcutPackage : IReferenceData, INativeData
{
    public ShortcutPackage(int characterId)
    {
        this.characterId = characterId;
        items = new Item[GameCommon.shortcutItemCount]; 
        for (int i = 0; i < GameCommon.shortcutItemCount; i++)
        {
            items[i] = default(Item); 
        }
      
    }

    public int packagerId
    {
        get
        {
            Character character = CharacterManager.instance.GetCharacter(characterId);
            if (character != null)
            {
                return character.characterPackage;
            }
            return 0;
        }
    }
    public int characterId;
    public Item[] items;
    public HashSet<int> haveItems = new HashSet<int>();
    public int Key => characterId;
    public void ChangeItem(int itemId, int index)
    {
        int oldIndex = -1;
        for(int i = 0; i < items.Length; i++)
        {
            if (items[i].dataId == itemId)
            {
                oldIndex = i;
                break;
            }
        }
        if (oldIndex != -1 && oldIndex != index)
        {
            Item oldItem = items[oldIndex];
            items[oldIndex] = items[index];
            items[index] = oldItem;
        } 
    }
    public bool CheckItem(int itemId,out int itemInstance)
    {
        itemInstance = 0;
        for(int i = 0; i < items.Length; i++)
        {
            if (items[i].dataId == itemId)
            {
                itemInstance = items[i].instanceId;
                return true;
            }
        }
        return false;
    }
    public bool CheckItem(int itemId, out List<int> itemInstances)
    {
        itemInstances = new List<int>();
        bool result = false;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].dataId == itemId)
            {
                itemInstances.Add(items[i].instanceId);
                result = true;
            }
        }
        return result;
    }
    public async void SetItemValue(int instanceId,int itemValue)
    {
        for(int i = 0; i < items.Length; i++)
        {
            if (items[i].instanceId == instanceId)
            {
                items[i]=await Item.SetValue(items[i],itemValue);
            }
        }
    }

    public bool TryChangeItemValue(List<int> itemIds,int value)
    {
        for(int i=0;i<items.Length; i++)
        {
            if (itemIds.Contains(items[i].dataId))
            {
                AddItemValue addItemValue = new AddItemValue
                {
                    characterId = Key,
                    selectItem = items[i].instanceId,
                    value = value
                };
                GameActionManager.instance.QueueAction(addItemValue);
                return true;
            }
        }
        return false;
    }
    public bool TrySetItemValue(int itemId, int value)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].dataId == itemId && items[i].value >= value)
            {
                SetItemValue addItemValue = new SetItemValue
                {
                    characterId = Key,
                    selectItem = items[i].instanceId,
                    value = value
                };
                GameActionManager.instance.QueueAction(addItemValue); 
            }
        }
        return true;
    }
    public bool TrySetItemValue(List<int> itemIds, int value)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (itemIds.Contains(items[i].dataId))
            {
                SetItemValue addItemValue = new SetItemValue
                {
                    characterId = Key,
                    selectItem = items[i].instanceId,
                    value = value
                };
                GameActionManager.instance.QueueAction(addItemValue);
             
            }
        }
        return true;
    }
    public List<ShortcutItem> GetShortcutItems()
    {
        List<ShortcutItem> shortcutItems = new List<ShortcutItem>();
        for (int i = 0; i < items.Length; i++)
        {
            shortcutItems.Add(new ShortcutItem
            {
                index = i+1,
                Item = items[i]
            });
        }
        return shortcutItems;
    }

    public void RemoveItemIndex(int index)
    {
        if (index <= items.Length)
        {
            haveItems.Remove(items[index - 1].instanceId);
            items[index-1] = default(Item); 
        }
    }
    public bool SetItem(Item item)
    {
        if (item.instanceId!=0&&haveItems.Contains(item.instanceId))
        {
            return false;
        }
        for(int i=0;i< items.Length; i++)
        {
            if (items[i].instanceId == 0&& items[i].count<=0)
            {
                items[i] = item;
                haveItems.Add(item.instanceId);
                return true; 
            }
        }
        return false;
    }
   
    public void Dispose()
    { 
    }
}