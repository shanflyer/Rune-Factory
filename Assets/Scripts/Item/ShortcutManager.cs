using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections; 

public class ShortcutManager : Singleton<ShortcutManager>
{
    Dictionary<int,ShortcutPackage> shortcutPackages = new Dictionary<int, ShortcutPackage>();
    public ShortcutPackage GetShortcutPackage(int characterId)
    {
        if(!shortcutPackages.TryGetValue(characterId,out var shortcutPackage))
        {
            shortcutPackage =new ShortcutPackage(characterId); 
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
                    int itemCount = PackageManager.instance.GetPackageItemCount(
                        CharacterManager.instance.controllerCharacter.characterPackage, item.dataId);
                    if (itemCount <= 0)
                    {
                        shortcutPackage.haveItems.Remove(shortcutPackage.items[i].instanceId);
                        shortcutPackage.items[i] = default(Item);
                    }
                    else
                    {
                        item.count = itemCount;
                        shortcutPackage.items[i] = item;
                        shortcutPackage.haveItems.Add(shortcutPackage.items[i].instanceId);
                    }
                }
            }
            shortcutPackages[shortcutPackage.Key]=(shortcutPackage);
            var shortcutPanel = await UIManager.instance.GetGamePanel<ShortcutPanel>();
            if (shortcutPanel != null)
            {
                shortcutPanel.InitReferenceData(shortcutPackage);
            }
            // UIManager.instance.ShowGamePanel<ShortcutPanel, ShortcutPackage>(shortcutPackage);

        }
    }
    async Task RefreshDisplayShortcutPackageAsync(ShortcutPackage shortcutPackage)
    {
        if (CharacterManager.instance.controllerCharacter.instanceId == shortcutPackage.characterId)
        {
            var shortcutPanel =await UIManager.instance.GetGamePanel<ShortcutPanel>();
            if (shortcutPanel != null)
            {
               shortcutPanel.InitReferenceData(shortcutPackage);
            }    
        }
    }
    async void RemoveShortcutItem(RemoveShortcutItem removeShortcutItem)
    {
        if(shortcutPackages.TryGetValue(removeShortcutItem.characterId,out var shortcutPackage))
        {
            shortcutPackage.RemoveItemIndex(removeShortcutItem.index);
            //shortcutPackages.SetData(shortcutPackage);
           await RefreshDisplayShortcutPackageAsync(shortcutPackage);
        }
    }
    async void SetShortcutItem(SetShortcutItem setShortcutItem)
    {
        if (shortcutPackages.TryGetValue(setShortcutItem.characterId, out var shortcutPackage))
        {
            if (shortcutPackage.SetItem(setShortcutItem.Item))
            {
                SetPackageSelectItem setPackageSelectItem = new SetPackageSelectItem
                {
                    packageId = shortcutPackage.packagerId,
                    selectItem = setShortcutItem.Item.instanceId
                };
                GameActionManager.instance.QueueAction(setPackageSelectItem);


                //shortcutPackages.SetData(shortcutPackage);
               await RefreshDisplayShortcutPackageAsync(shortcutPackage);
            } 
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
            haveItems.Add(items[index - 1].instanceId);
            items[index-1] = default(Item); 
        }
    }
    public bool SetItem(Item item)
    {
        if (haveItems.Contains(item.instanceId))
        {
            return false;
        }
        for(int i=0;i< items.Length; i++)
        {
            if (items[i].instanceId == 0)
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