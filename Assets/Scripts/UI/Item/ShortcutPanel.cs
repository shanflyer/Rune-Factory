using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public struct ShortcutItem : IReferenceData
{
    public Item Item;
    public int index;

    public bool Equals(IReferenceData referenceData)
    {
        if (referenceData is ShortcutItem other)
        {
            if (index == other.index)
            {
                return true;
            }
            /*
            if (Item.instanceId == other.Item.instanceId)
            {
                return true;
            }*/
        }

        return false;
    }

    public static bool operator ==(ShortcutItem item0, ShortcutItem item1)
    {
        if (item0.Item.instanceId == item1.Item.instanceId)
        {
            return true;
        }
        return false;
    }

    public static bool operator !=(ShortcutItem item0, ShortcutItem item1)
    {
        if (item0.Item.instanceId != item1.Item.instanceId)
        {
            return true;
        }
        return false;
    }
}

public class ShortcutPanel : GamePanel<ShortcutPackage>
{
    public override bool changeInputModel => false;
    private DisplayList<ShortcutItemReference, ShortcutItem> itemList;

    [SerializeField]
    private Transform itemParent;

    [SerializeField]
    private Button bagButton;

    [SerializeField]
    private ToggleGroup toggleGroup;

    [SerializeField]
    private ShortcutItemReference ShortcutItemReference;

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<SelectPackageItemAction>(SelectPackageItemAction);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<SelectPackageItemAction>(SelectPackageItemAction);
    }

    protected override void Awake()
    {
        base.Awake();
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
                    // selectItemTypes = new List<ItemType> { ItemType.Default },
                    targetObj = CharacterManager.instance.controllerCharacter.instanceId,
                    selectActionName = "สนำร",
                    selectAction = TryUsedItem,
                    canSetShortcut = true
                    /*
                    setPanel=(BaseReference reference) =>
                    {
                        if(reference is WarehousePanel warehousePanel)
                        {
                            warehousePanel.SetOtherSelectAction(SelectPackageItemAction);
                        }
                    }*/
                };
                GameActionManager.instance.QueueAction(openPackage, true);
            }
        });
    }

    private Item selectPackageItem;

    private void SelectPackageItemAction(SelectPackageItemAction selectPackageItemAction)
    {
        int packageId = selectPackageItemAction.packageId;
        var item = selectPackageItemAction.item;
        if (packageId == CharacterManager.instance.controllerCharacter.characterPackage)
        {
            selectPackageItem = item;
            if (selectPackageItem.dataId != 0)
            {
                Item newItem = new Item
                {
                    instanceId = selectPackageItem.instanceId,
                    dataId = selectPackageItem.dataId,
                    value = selectPackageItem.value,
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
            SetPackageSelectItem setPackageSelectItem = new SetPackageSelectItem
            {
                packageId = packageId,
                selectItem = item.instanceId
            };
            GameActionManager.instance.QueueAction(setPackageSelectItem);
        }
    }

    private void TryUsedItem(Item item,bool select)
    {
        ItemUseAction itemUseAction = new ItemUseAction
        {
            itemId = item.dataId,
            itemCount = 1,
            packageId = item.packageId
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

    private ShortcutPackage shortcutPackage;

    public override Task InitData(string dataKey)
    {
        var shortcutPackage = ShortcutManager.instance.GetShortcutPackage(CharacterManager.instance.controllerCharacter.instanceId);
        InitReferenceData(shortcutPackage);
        return base.InitData(dataKey);
    }

    public override void InitReferenceData(ShortcutPackage v)
    {
        base.InitReferenceData(v);
        shortcutPackage = v;
        var items = v.GetShortcutItems();

        itemList.InitListData(items, SelectShortcutItem);
        if (shortcutItem.Item.instanceId == 0)
        {
        }
        else
        {
            bool oldSelect = false;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Equals(shortcutItem))
                {
                    oldSelect = true;
                    itemList.Select(shortcutItem);
                    break;
                }
            }
            if (!oldSelect)
            {
                shortcutItem = default(ShortcutItem);
                selectShortIndex = 0;
            }
        }
    }

    private int selectShortIndex;
    private ShortcutItem shortcutItem;

    private void SelectShortcutItem(ShortcutItem item, bool select)
    {
        if (select)
        {
            if (selectPackageItem.instanceId != 0 && item.Item.instanceId != 0)
            {
                SetPackageSelectItem setPackageSelectItem = new SetPackageSelectItem
                {
                    packageId = shortcutPackage.packagerId,
                    selectItem = item.Item.instanceId
                };
                GameActionManager.instance.QueueAction(setPackageSelectItem);
            }
            else
            {
                if (selectShortIndex != 0)
                {
                    if (selectShortIndex != item.index)
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
                            value = selectPackageItem.value,
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
            selectShortIndex = item.index;
            shortcutItem = item;
            selectPackageItem = item.Item;
            itemList.ClearSelect(item);
        }
        else
        {
            if (selectShortIndex == item.index)
            {
                selectShortIndex = 0;
            }
            if (shortcutItem == item)
            {
                shortcutItem = default(ShortcutItem);
            }
        }
    }
}