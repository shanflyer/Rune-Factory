using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public struct ShortcutItem: IReferenceData
{
    public Item Item;
    public int index;
}
public class ShortcutPanel : GamePanel<ShortcutPackage>
{
    DisplayList<ShortcutItemReference, ShortcutItem> itemList;
    [SerializeField]
    Transform itemParent;
    [SerializeField]
    Button bagButton;
    [SerializeField]
    ToggleGroup toggleGroup;
    [SerializeField]
    ShortcutItemReference ShortcutItemReference;

    protected override void Awake()
    {
        itemList = new DisplayList<ShortcutItemReference, ShortcutItem>(ShortcutItemReference, itemParent);
        bagButton.onClick.AddListener(async () =>
        {
            if (UIManager.instance.GamePanelIsShow<WarehousePanel>())
            {
                UIManager.instance.CloseGamePanel<WarehousePanel>();
            }
            else
            {
                OpenPackage openPackage = new OpenPackage
                {
                    packageId = CharacterManager.instance.controllerCharacter.characterPackage,
                    selectItemTypes = new List<ItemType> { ItemType.Default },
                    targetObj = CharacterManager.instance.controllerCharacter.instanceId,
                    selectActionName = "สนำร",
                    selectAction = TryUsedItem,
                    setPanel=(BaseReference reference) =>
                    {
                        if(reference is WarehousePanel warehousePanel)
                        {
                            warehousePanel.SetOtherSelectAction(SelectPackageItemAction);
                        }
                    }
                };
                GameActionManager.instance.QueueAction(openPackage,true);  
            }
           
        });
    }
   
    Item selectPackageItem;
    void SelectPackageItemAction(Item item, int packageId)
    {
        if (packageId == CharacterManager.instance.controllerCharacter.characterPackage)
        {
            selectPackageItem = item;
            if (selectShortIndex != 0&&selectPackageItem.dataId!=0)
            {
                Item newItem = new Item
                {
                    instanceId = selectPackageItem.instanceId,
                    dataId = selectPackageItem.dataId,
                    count = PackageManager.instance.GetPackageItemCount(shortcutPackage.packagerId, selectPackageItem.dataId)
                };
                SetShortcutItem setShortcutItem = new SetShortcutItem
                {
                    characterId = shortcutPackage.characterId,
                    index = selectShortIndex,
                    Item = newItem
                };
                GameActionManager.instance.QueueAction(setShortcutItem);
            }
        } 
    }

    void TryUsedItem(Item item, int packageId)
    {
        ItemUseAction itemUseAction = new ItemUseAction
        {
            itemId = item.instanceId,
            itemCount = 1,
            packageId = packageId
        };
        GameActionManager.instance.QueueAction(itemUseAction, true);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        itemParent = FindChildGameObject("ItemList");
        bagButton = FindChildGameObject<Button>("BagButtpn");
        toggleGroup = FindChildGameObject<ToggleGroup>("ItemList");
        ShortcutItemReference = FindChildGameObject<ShortcutItemReference>("ItemBoxReference");
    }
    ShortcutPackage shortcutPackage;
    public override void InitReferenceData(ShortcutPackage v)
    {
        base.InitReferenceData(v);
        shortcutPackage = v;
        itemList.InitListData(v.GetShortcutItems(), SelectShortcutItem, toggleGroup);
    }
    int selectShortIndex;

    void SelectShortcutItem(ShortcutItem item,bool select)
    {
        if(select)
        {
            if (selectShortIndex != 0 && selectShortIndex != item.index)
            {
                ChangeShortcutItemIndex changeShortcutItemIndex = new ChangeShortcutItemIndex
                {
                    characterId = shortcutPackage.characterId,
                    sourceIndex = selectShortIndex,
                    targetIndex = item.index
                };
                GameActionManager.instance.QueueAction(changeShortcutItemIndex);
                selectShortIndex = item.index;
            }
            else
            {
                selectShortIndex = item.index;
                if (selectPackageItem.dataId != 0)
                {
                    Item newItem = new Item
                    {
                        instanceId = selectPackageItem.instanceId,
                        dataId = selectPackageItem.dataId,
                        count = PackageManager.instance.GetPackageItemCount(shortcutPackage.packagerId, selectPackageItem.dataId)
                    }; 
                    SetShortcutItem setShortcutItem = new SetShortcutItem
                    {
                        characterId = shortcutPackage.characterId,
                        index = item.index,
                        Item = newItem
                    };
                    GameActionManager.instance.QueueAction(setShortcutItem);
                }
            }
        }
        else
        {
            if (selectShortIndex == item.index)
            {
                selectShortIndex=0;
            }
        }
       
    }
}
