using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MultiPackagePanel : GamePanel<PackageList>
{
    [SerializeField]
    private ToggleGroup itemSelectGroup;

    [SerializeField]
    private ItemBoxReference ItemBoxReference;

    [SerializeField]
    private Transform itemParent0;

    [SerializeField]
    private Button packageLevelUp0;

    [SerializeField]
    private TextMeshProUGUI Title0;

    [SerializeField]
    private TextMeshProUGUI caseCount0;

    private DisplayList<ItemBoxReference, Item> itemBoxs0;

    [SerializeField]
    private Transform itemParent1;

    [SerializeField]
    private Button packageLevelUp1;

    [SerializeField]
    private TextMeshProUGUI Title1;

    [SerializeField]
    private TextMeshProUGUI caseCount1;

    private DisplayList<ItemBoxReference, Item> itemBoxs1;

    [SerializeField]
    private Transform ItemInformation;

    [SerializeField]
    private Image ItemIcon;

    [SerializeField]
    private TextMeshProUGUI ItemName;

    [SerializeField]
    private TextMeshProUGUI Price;

    [SerializeField]
    private TextMeshProUGUI Type, Property, Info;

    [SerializeField]
    private Transform InfoItemValueBg;

    [SerializeField]
    private Image InfoItemValue;

    [SerializeField]
    private Button ActionButton, ReturnButton;

    [SerializeField]
    private Transform UPMove, DownMove;

    [SerializeField]
    private Button AddButton, ReduceButton;

    [SerializeField]
    private TMP_InputField buyCountValue;

    private Item SelectItem;
    private int SelectCount;

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshShortcut>(RefreshShortcut);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared && GameActionManager.HasInstance)
            GameActionManager.instance.RemoveListener<RefreshShortcut>(RefreshShortcut);
    }

    public override void Close()
    {
        base.Close();
        GameActionManager.instance.QueueAction(new OpenPanelAction
        {
            type = typeof(ShortcutPanel)
        });
    }

    private void RefreshShortcut(RefreshShortcut refreshShortcut)
    {
        if (refreshShortcut.packageId == packageId0)
        {
            var packageData = PackageManager.instance.GetPackageData(packageId0);
            PackageCaseCount0 = packageData.caseCount;
            packageLevel0 = packageData.level;

            Item defaultItem = default(Item);
            List<Item> items0 = new List<Item>();
            if (packageData.items != null)
            {
                for (int i = 0; i < packageData.items.Count; i++)
                {
                    Item item = packageData.items[i];
                    if (packageSetData1.moveItemType == MoveItemType.OnlyGet || !CheckPackageItem(packageSetData1, item.dataId)
                        || (packageSetData1.packageType == PackageType.鲜活 && !item.isFresh) ||
                        (packageSetData1.packageType == PackageType.非鲜活 && item.isFresh))
                    {
                        item.locked = true;
                    }
                    else
                    {
                        item.locked = false;
                    }
                    items0.Add(item);
                }
            }
            for (int i = items0.Count; i < packageData.caseCount; i++)
            {
                items0.Add(defaultItem);
            }

            Title0.SetSWText(packageSetData0.packageName);
            // 快捷刷新来自同步 Action，列表刷新绑定当前多背包面板生命周期。
            RunLifecycleTask(token => itemBoxs0.InitListData(items0, SelectPackageItem, toggleGroup: itemSelectGroup, cancellationToken: token), nameof(RefreshShortcut));
            caseCount0.text = $"{packageData.items.Count}/{packageData.caseCount}";
            bool canLevelUp = packageSetData0.canLevelUp ? packageLevel0 < packageSetData0.maxLevel - 1 : false;
            packageLevelUp0.transform.localScale = canLevelUp ? Vector3.one : Vector3.zero;
        }
        if (refreshShortcut.packageId == packageId1)
        {
            var packageData = PackageManager.instance.GetPackageData(packageId1);
            PackageCaseCount1 = packageData.caseCount;
            packageLevel1 = packageData.level;

            Item defaultItem = default(Item);
            List<Item> items1 = new List<Item>();
            if (packageData.items != null)
            {
                for (int i = 0; i < packageData.items.Count; i++)
                {
                    Item item = packageData.items[i];
                    if (packageSetData0.moveItemType == MoveItemType.OnlyGet || !CheckPackageItem(packageSetData0, item.dataId)
                        || (packageSetData0.packageType == PackageType.鲜活 && !item.isFresh) ||
                        (packageSetData0.packageType == PackageType.非鲜活 && item.isFresh))
                    {
                        item.locked = true;
                    }
                    else
                    {
                        item.locked = false;
                    }
                    items1.Add(item);
                } 
            }
            for (int i = items1.Count; i < packageData.caseCount; i++)
            {
                items1.Add(defaultItem);
            }
            Title1.SetSWText(packageSetData1.packageName);
            RunLifecycleTask(token => itemBoxs1.InitListData(items1, SelectPackageItem, toggleGroup: itemSelectGroup, cancellationToken: token), nameof(RefreshShortcut));
            caseCount1.text = $"{packageData.items.Count}/{packageData.caseCount}";
            bool canLevelUp = packageSetData1.canLevelUp ? packageLevel1 < packageSetData1.maxLevel - 1 :false;
            packageLevelUp1.transform.localScale = canLevelUp ? Vector3.one : Vector3.zero;
        }
        itemBoxs0.ClearSelect();
        itemBoxs1.ClearSelect();
        ItemInformation.localScale = Vector3.zero;
    }

    protected override void Awake()
    {
        base.Awake();

        itemBoxs0 = new DisplayList<ItemBoxReference, Item>(ItemBoxReference, itemParent0);
        itemBoxs1 = new DisplayList<ItemBoxReference, Item>(ItemBoxReference, itemParent1);
        ActionButton.onClick.AddListener(MoveSelectItem);
        ReturnButton.onClick.AddListener(Close);

        buyCountValue.onValueChanged.AddListener(SetSelectCount);
        AddButton.onClick.AddListener(AddSelectCount);
        ReduceButton.onClick.AddListener(ReduceSelectCount);

        packageLevelUp0.onClick.AddListener(() =>
        {
            TryPackageLevelUp(packageSetData0, PackageCaseCount0, packageId0);
        });
        packageLevelUp1.onClick.AddListener(() =>
        {
            TryPackageLevelUp(packageSetData1, PackageCaseCount1, packageId1);
        });
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        itemSelectGroup = FindChildGameObject<ToggleGroup>("MultiPackage");
        ItemBoxReference = FindChildGameObject<ItemBoxReference>("ItemBoxReference");
        ItemInformation = FindChildGameObject("ItemInformation");

        itemParent0 = FindChildGameObject("ItemParent0");
        packageLevelUp0 = FindChildGameObject<Button>("LevelUp0");
        Title0 = FindChildGameObject<TextMeshProUGUI>("Title0");
        caseCount0 = FindChildGameObject<TextMeshProUGUI>("CaseCount0");

        itemParent1 = FindChildGameObject("ItemParent1");
        packageLevelUp1 = FindChildGameObject<Button>("LevelUp1");
        Title1 = FindChildGameObject<TextMeshProUGUI>("Title1");
        caseCount1 = FindChildGameObject<TextMeshProUGUI>("CaseCount1");

        ItemIcon = FindChildGameObject<Image>("ItemIcon");
        ItemName = FindChildGameObject<TextMeshProUGUI>("ItemName");
        Price = FindChildGameObject<TextMeshProUGUI>("MoneyValue");
        Type = FindChildGameObject<TextMeshProUGUI>("Type");
        Property = FindChildGameObject<TextMeshProUGUI>("Property");
        Info = FindChildGameObject<TextMeshProUGUI>("Info");
        ActionButton = FindChildGameObject<Button>("ActionButton");

        InfoItemValueBg = FindChildGameObject("InfoItemValueBg");
        InfoItemValue = FindChildGameObject<Image>("InfoItemValue");
        UPMove = FindChildGameObject("UpMove");
        DownMove = FindChildGameObject("DownMove");

        buyCountValue = FindChildGameObject<TMP_InputField>("BuyCountValue");
        AddButton = FindChildGameObject<Button>("AddButton");
        ReduceButton = FindChildGameObject<Button>("ReduceButton");

        ReturnButton = FindChildGameObject<Button>("ReturnButton");
    }

    private int packageId0, packageId1;
    private PackageSetData packageSetData0, packageSetData1;
    private int PackageCaseCount0, PackageCaseCount1;
    private int packageLevel0, packageLevel1;

    private void TryPackageLevelUp(PackageSetData packageSetData, int packageCaseCount, int packageId)
    {
        if (packageSetData && packageSetData.canLevelUp)
        {
            int cost = packageSetData.levelUpCost * packageCaseCount;
            string notice = string.Format(LanguageManage.SwitchStr("拓展{0}空间?"), packageSetData.packageName);
            PayManager.instance.PayAction("空间拓展", notice, cost, PayType.金币, (bool result) =>
            {
                if (result)
                {
                    PackageManager.instance.AddPackageUpLevel(packageId);
                }
            });
        }
    }

    private void SelectPackageItem(Item item,int index, bool selected = true)
    {
        RunLifecycleTask(token => SelectPackageItemAsync(item, index, selected, token), nameof(SelectPackageItem));
    }

    private async System.Threading.Tasks.Task SelectPackageItemAsync(Item item,int index, bool selected, System.Threading.CancellationToken cancellationToken)
    {
        if (selected)
        {
            if (item.dataId == 0)
            {
                SelectCount = 0;
                ItemInformation.localScale = Vector3.zero;
                // if (otherSelectItemAction != null)
                {
                    //   otherSelectItemAction.Invoke(default(Item), selectPackageData.instanceId);
                }
            }
            else
            {
                ItemInformation.localScale = Vector3.one;
                SelectItem = item;
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
                if (ShouldStopLifecycleTask(cancellationToken))
                {
                    return;
                }

                ItemIcon.sprite = itemData.icon;
                ItemIcon.enabled = true;
                ItemIcon.SetNativeSize();
                ItemName.SetADDText("+ ",itemData.itemName," +");
                Type.SetSWText(itemData.type.ToString());
                Info.SetSWText(itemData.GetInfo());
                Property.text = itemData.GetProperty();
                Price.text = itemData.sellPrice.ToString();
                InfoItemValueBg.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
                InfoItemValue.fillAmount = item.value;
                SelectCount = item.count;
                buyCountValue.SetTextWithoutNotify(SelectCount.ToString());

                if (SelectItem.packageId == packageId0)
                {
                    UPMove.localScale = Vector3.zero;
                    DownMove.localScale = Vector3.one;
                    if (packageSetData0.moveItemType==MoveItemType.OnlyPut)
                    {
                       // DownMove.localScale = Vector3.zero;
                        ActionButton.interactable = false;
                    }
                    else
                    {
                        if (CheckPackageItem(packageSetData1, SelectItem.dataId))
                        {
                           // DownMove.localScale = Vector3.one;
                            ActionButton.interactable = true;
                        }
                        else
                        {
                          //  DownMove.localScale = Vector3.zero;
                            ActionButton.interactable = false;
                        }
                        
                    } 
                  
                }
                if (SelectItem.packageId == packageId1)
                {

                    if (packageSetData1.moveItemType == MoveItemType.OnlyPut)
                    {
                       // UPMove.localScale = Vector3.zero;
                        ActionButton.interactable = false;
                    }
                    else
                    {
                        if (CheckPackageItem(packageSetData0,SelectItem.dataId))
                        {
                            //UPMove.localScale = Vector3.one;
                            ActionButton.interactable = true;
                        }
                        else
                        {
                           // UPMove.localScale = Vector3.zero;
                            ActionButton.interactable = false;
                        }
                            
                    }
                    UPMove.localScale = Vector3.one;
                    DownMove.localScale = Vector3.zero;
                }
            }
        }
    }
    bool CheckPackageItem(PackageSetData packageSetData,int itemDataId)
    {
        if (packageSetData.moveItemType == MoveItemType.OnlyGet)
        {
            return false;
        }
        if (packageSetData.limitItems != null && packageSetData.limitItems.Count > 0)
        {
            return packageSetData.limitItems.Contains(itemDataId);
        }
        return true;
    }

    private void MoveSelectItem()
    {
        int inPackageId = SelectItem.packageId == packageId0 ? packageId1 : packageId0;
        int outPackageId = SelectItem.packageId == packageId0 ? packageId0 : packageId1;
        AddPackageItem addPackageItem = new AddPackageItem
        {
            itemDataId = SelectItem.dataId,
            itemCount = SelectCount,
            packageId = inPackageId,
            setValue = (int count) =>
            {
                if (count < SelectCount)
                {
                    int inCount = SelectCount - count;
                    RemovePackageItem removePackageItem = new RemovePackageItem
                    {
                        itemDataId = SelectItem.dataId,
                        itemCount = inCount,
                        packageId = outPackageId
                    };
                    GameActionManager.instance.QueueAction(removePackageItem);
                }
            }
        };
        GameActionManager.instance.QueueAction(addPackageItem);
    }

    private void SetSelectCount(string value)
    {
        int count = PackageManager.instance.GetPackageItemCount((int)SelectItem.packageId, SelectItem.dataId);
        SelectCount = math.clamp(int.Parse(value), 1, count);
        buyCountValue.SetTextWithoutNotify(SelectCount.ToString());
    }

    private void AddSelectCount()
    {
        int count = PackageManager.instance.GetPackageItemCount((int)SelectItem.packageId, SelectItem.dataId);
        SelectCount++;
        SelectCount = math.clamp(SelectCount, 1, count);
        buyCountValue.SetTextWithoutNotify(SelectCount.ToString());
    }

    private void ReduceSelectCount()
    {
        int count = PackageManager.instance.GetPackageItemCount((int)SelectItem.packageId, SelectItem.dataId);
        SelectCount--;
        SelectCount = math.clamp(SelectCount, 1, count);
        buyCountValue.SetTextWithoutNotify(SelectCount.ToString());
    }

    public override void InitReferenceData(PackageList v)
    {
        base.InitReferenceData(v);
        // 多背包初始化绑定面板生命周期，关闭或重开后旧加载不再覆盖两侧列表。
        RunLifecycleTask(token => InitReferenceDataAsync(v, token), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(PackageList v, System.Threading.CancellationToken cancellationToken)
    {

        GameActionManager.instance.QueueAction(new ClosePanelAction
        {
            type = typeof(ShortcutPanel)
        });

        var packageData0 = v.packageDatas[0];
        var packageData1 = v.packageDatas[1];
        packageLevel0 = packageData0.level;
        packageLevel1 = packageData1.level;
        PackageCaseCount0 = packageData0.caseCount;
        packageSetData0 = await GameDataManager.instance.GetAsyncData<PackageSetData>(packageData0.dataId);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        PackageCaseCount1 = packageData1.caseCount;
        packageSetData1 = await GameDataManager.instance.GetAsyncData<PackageSetData>(packageData1.dataId);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        Item defaultItem = default(Item);
        List<Item> items0 = new List<Item>();
        int itemCaseCount0 = 0;
        if (packageData0.items != null)
        {
            for (int i = 0; i < packageData0.items.Count; i++)
            {
                Item item = packageData0.items[i];
                if (packageSetData1.moveItemType == MoveItemType.OnlyGet|| !CheckPackageItem(packageSetData1, item.dataId)
                    || (packageSetData1.packageType == PackageType.鲜活 && !item.isFresh)
                    || (packageSetData1.packageType == PackageType.非鲜活 && item.isFresh))
                {
                    item.locked = true;
                }
                else
                {
                    item.locked = false;
                }
                items0.Add(item);
            }
            itemCaseCount0 = packageData0.items.Count;
        }
        for (int i = items0.Count; i < packageData0.caseCount; i++)
        {
            items0.Add(defaultItem);
        }

        Title0.SetSWText(packageSetData0.packageName);
        await itemBoxs0.InitListData(items0, SelectPackageItem, toggleGroup: itemSelectGroup, cancellationToken: cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        caseCount0.text = $"{itemCaseCount0}/{packageData0.caseCount}";
        bool canLevelUp = packageSetData0.canLevelUp ? packageLevel0 < packageSetData0.maxLevel - 1 : false;
        packageLevelUp0.transform.localScale = canLevelUp? Vector3.one : Vector3.zero;
        packageId0 = packageData0.instanceId;

        List<Item> items1 = new List<Item>();
        int itemCaseCount1 = 0;
        if (packageData1.items != null)
        {
            for (int i = 0; i < packageData1.items.Count; i++)
            {
                Item item = packageData1.items[i];
                if (packageSetData0.moveItemType == MoveItemType.OnlyGet || !CheckPackageItem(packageSetData0, item.dataId)
                    || (packageSetData0.packageType == PackageType.鲜活 && !item.isFresh)||
                    (packageSetData0.packageType == PackageType.非鲜活 && item.isFresh))
                {
                    item.locked = true;
                }
                else
                {
                    item.locked = false;
                }
                items1.Add(item);
            }

            itemCaseCount1 = packageData1.items.Count;
        }
        for (int i = items1.Count; i < packageData1.caseCount; i++)
        {
            items1.Add(defaultItem);
        }

        Title1.SetSWText(packageSetData1.packageName);
        await itemBoxs1.InitListData(items1, SelectPackageItem, toggleGroup: itemSelectGroup, cancellationToken: cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        caseCount1.text = $"{itemCaseCount1}/{packageData1.caseCount}";
        bool canLevelUp1 = packageSetData1.canLevelUp ? packageLevel1 < packageSetData1.maxLevel-1 : false;
        packageLevelUp1.transform.localScale = canLevelUp1 ? Vector3.one : Vector3.zero;
        packageId1 = packageData1.instanceId;

        itemBoxs0.ClearSelect();
        itemBoxs1.ClearSelect();
    }
}
