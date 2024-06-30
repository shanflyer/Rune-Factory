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
            shortcutPackage = ShortcutPackage.CreatShortCutPackage(characterId); 
        }
        return shortcutPackage;
    }
    public override void Init()
    {
        base.Init();
        shortcutPackages.Clear();
        GameActionManager.instance.AddListener<RemoveShortcutItem>(RemoveShortcutItem);
        GameActionManager.instance.AddListener<SetShortcutItem>(SetShortcutItem);
        GameActionManager.instance.AddListener<ChangeShortcutItemIndex>(ChangeShortcutItemIndex);
        GameActionManager.instance.AddListener<RefreshShortcut>(RefreshShortcut);
    }
    protected override void Clear()
    {
        base.Clear();
    }

    void RefreshShortcut(RefreshShortcut refreshShortcut)
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
                        shortcutPackage.items[i] = default(Item);
                    }
                    else
                    {
                        item.count = itemCount;
                        shortcutPackage.items[i] = item;
                    }
                }
            }
            shortcutPackages[shortcutPackage.Key]=(shortcutPackage);
            UIManager.instance.ShowGamePanel<ShortcutPanel, ShortcutPackage>(shortcutPackage);

        }
    }
    void RefreshDisplayShortcutPackage(ShortcutPackage shortcutPackage)
    {
        if (CharacterManager.instance.controllerCharacter.instanceId == shortcutPackage.characterId)
        {
            UIManager.instance.ShowGamePanel<ShortcutPanel, ShortcutPackage>(shortcutPackage);
        }
    }
    void RemoveShortcutItem(RemoveShortcutItem removeShortcutItem)
    {
        if(shortcutPackages.TryGetValue(removeShortcutItem.characterId,out var shortcutPackage))
        {
            shortcutPackage.RemoveItemIndex(removeShortcutItem.index);
            //shortcutPackages.SetData(shortcutPackage);
            RefreshDisplayShortcutPackage(shortcutPackage);
        }
    }
    void SetShortcutItem(SetShortcutItem setShortcutItem)
    {
        if (shortcutPackages.TryGetValue(setShortcutItem.characterId, out var shortcutPackage))
        {
            SetPackageSelectItem setPackageSelectItem = new SetPackageSelectItem
            {
                packageId = shortcutPackage.packagerId,
                selectItem = setShortcutItem.Item.instanceId
            };
            GameActionManager.instance.QueueAction(setPackageSelectItem);

            shortcutPackage.SetItemIndex(setShortcutItem.index,setShortcutItem.Item);
            //shortcutPackages.SetData(shortcutPackage);
            RefreshDisplayShortcutPackage(shortcutPackage);
        }
    }
    void ChangeShortcutItemIndex(ChangeShortcutItemIndex changeShortcutItemIndex)
    {
        if(shortcutPackages.TryGetValue(changeShortcutItemIndex.characterId, out var shortcutPackage))
        {
            shortcutPackage.ChangeItemIndex(changeShortcutItemIndex.sourceIndex, changeShortcutItemIndex.targetIndex);
           // shortcutPackages.SetData(shortcutPackage);
            RefreshDisplayShortcutPackage(shortcutPackage);
        }
    }
}
public class ShortcutPackage : IReferenceData, INativeData
{
    public static ShortcutPackage CreatShortCutPackage(int characterId)
    {
        ShortcutPackage shortcutPackage = new ShortcutPackage
        {
            characterId = characterId,
            items = new Item[GameCommon.shortcutItemCount]
        };
        for (int i = 0; i < GameCommon.shortcutItemCount; i++)
        {
            shortcutPackage.items[i] = default(Item);
        }

        return shortcutPackage;
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

    public int Key => characterId;

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
            items[index-1] = default(Item);
        }
    }
    public void SetItemIndex(int index,Item item)
    {
        if (index <= items.Length)
        {
            if (index <= 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].instanceId == 0)
                    {
                        items[i] = item;
                        break;
                    }
                }
            }
            else
            {
                items[index - 1] = item;
                for (int i = 0; i < items.Length; i++)
                {
                    if (i != index - 1)
                    {
                        if (items[i].dataId == item.dataId)
                        {
                            items[i] = default(Item);
                        }
                    }
                }
            } 
        }
    }
    public void ChangeItemIndex(int sourceIndex,int targetIndex)
    {
        Item sourceItem = items[sourceIndex-1];
        items[sourceIndex-1] = items[targetIndex-1];
        items[targetIndex-1] = sourceItem;
    }
    public void Dispose()
    { 
    }
}