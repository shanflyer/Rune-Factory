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
    public bool Equals(Object @object)
    {
        ShortcutItem other = (ShortcutItem)@object;
        if (other!=null)
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

    public static explicit operator ShortcutItem(Object v)
    {
        throw new System.NotImplementedException();
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
    [SerializeField]
    private Button useButton, unEquipButton;

    public override void OnEnable()
    {
        base.OnEnable(); 
    }

    public override void OnDisable()
    {
        base.OnDisable();
         
    }

    protected override void Awake()
    {
        base.Awake();
        itemList = new DisplayList<ShortcutItemReference, ShortcutItem>(ShortcutItemReference, itemParent);
        bagButton.onClick.AddListener( () =>
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
        useButton.onClick.AddListener(UseItemAction);
        unEquipButton.onClick.AddListener(UnEquipAction);
    }
    void UseItemAction()
    {
        ItemUseAction itemUseAction = new ItemUseAction
        {
            itemId = selectPackageItem.dataId,
            itemCount = 1,
            packageId = selectPackageItem.packageId
        };
        GameActionManager.instance.QueueAction(itemUseAction, true);
    }
    void UnEquipAction()
    {
        RemoveShortcutItem removeShortcutItem = new RemoveShortcutItem
        {
            characterId = CharacterManager.instance.controllerCharacter.instanceId,
            index = selectShortIndex
        };
        GameActionManager.instance.QueueAction(removeShortcutItem);
        unEquipButton.interactable = false;
    }

    private Item selectPackageItem;

  
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
        unEquipButton = FindChildGameObject<Button>("UnSetButton");
        useButton = FindChildGameObject<Button>("UseButton");
    }

    private ShortcutPackage shortcutPackage;

    public override Task InitData(string dataKey)
    {
        useButton.interactable = unEquipButton.interactable = false;
        var shortcutPackage = ShortcutManager.instance.GetShortcutPackage(CharacterManager.instance.controllerCharacter.instanceId);
        InitReferenceData(shortcutPackage);
        return base.InitData(dataKey);
    }

    public override async void InitReferenceData(ShortcutPackage v)
    {
        useButton.interactable = unEquipButton.interactable = false;
        base.InitReferenceData(v);
        shortcutPackage = v;
        var items = v.GetShortcutItems();

        await itemList.InitListData(items, SelectShortcutItem,toggleGroup,Async:false);
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
            selectShortIndex = item.index;
            shortcutItem = item;
            selectPackageItem = item.Item;
            unEquipButton.interactable = selectPackageItem.dataId != 0;
            useButton.interactable = selectPackageItem.dataId != 0;
            SetPackageSelectItem setPackageSelectItem = new SetPackageSelectItem
            {
                packageId = shortcutPackage.packagerId,
                selectItem = selectPackageItem.instanceId
            };
            GameActionManager.instance.QueueAction(setPackageSelectItem);
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

        ResetOperateData resetOperateData = new ResetOperateData
        {
            mapItemInstanceId = CharacterManager.instance.controllerCharacter.OperateItem,
            operates = null
        };
        GameActionManager.instance.QueueAction(resetOperateData);
    }
}