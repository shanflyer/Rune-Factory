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
        GameActionManager.instance.RemoveListener<RefreshShortcut>(RefreshShortcut);
    }

    private void RefreshShortcut(RefreshShortcut refreshShortcut)
    {
        if (refreshShortcut.packageId == packageId0)
        {
            var packageData = PackageManager.instance.GetPackageData(packageId0);
            PackageCaseCount0 = packageData.caseCount;

            Item defaultItem = default(Item);
            List<Item> items0 = new List<Item>();
            if (packageData.items != null)
            {
                items0.AddRange(packageData.items);
            }
            for (int i = items0.Count; i < packageData.caseCount; i++)
            {
                items0.Add(defaultItem);
            }
           
            Title0.text = packageSetData0.packageName;
            itemBoxs0.InitListData(items0, SelectPackageItem, toggleGroup: itemSelectGroup);
            caseCount0.text = $"{packageData.items.Count}/{packageData.caseCount}";
            packageLevelUp0.transform.localScale = packageSetData0.canLevelUp ? Vector3.one : Vector3.zero;
        }
        if (refreshShortcut.packageId == packageId1)
        {
            var packageData = PackageManager.instance.GetPackageData(packageId1);
            PackageCaseCount1 = packageData.caseCount;

            Item defaultItem = default(Item);
            List<Item> items1 = new List<Item>();
            if (packageData.items != null)
            {
                items1.AddRange(packageData.items);
            }
            for (int i = items1.Count; i < packageData.caseCount; i++)
            {
                items1.Add(defaultItem);
            }
            Title1.text = packageSetData1.packageName;
            itemBoxs1.InitListData(items1, SelectPackageItem, toggleGroup: itemSelectGroup);
            caseCount1.text = $"{packageData.items.Count}/{packageData.caseCount}";
            packageLevelUp1.transform.localScale = packageSetData1.canLevelUp ? Vector3.one : Vector3.zero;
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
    PackageSetData packageSetData0, packageSetData1;
    int PackageCaseCount0, PackageCaseCount1;
    void TryPackageLevelUp(PackageSetData packageSetData, int packageCaseCount,int packageId)
    { 
        if (packageSetData && packageSetData.canLevelUp)
        {
            int cost = packageSetData.levelUpCost * packageCaseCount;
            string notice = $"拓展{packageSetData.packageName}空间?";
            PayManager.instance.PayAction("空间拓展", notice, cost, PayType.金币, (bool result) =>
            {
                if (result)
                {
                    PackageManager.instance.AddPackageUpLevel(packageId);
                }

            });
        }
    }
    private async void SelectPackageItem(Item item, bool selected = true)
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
                ItemIcon.sprite = itemData.icon;
                ItemIcon.enabled = true;
                ItemIcon.SetNativeSize();
                ItemName.text = $"+ {itemData.itemName} +";
                Type.text = itemData.type.ToString();
                Info.text = itemData.info;
                Property.text = itemData.property.ToString();
                Price.text = itemData.sellPrice.ToString();
                InfoItemValueBg.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
                InfoItemValue.fillAmount = item.value;
                SelectCount = item.count;
                buyCountValue.SetTextWithoutNotify(SelectCount.ToString());

                if (SelectItem.value == packageId0)
                {
                    UPMove.localScale = Vector3.zero;
                    DownMove.localScale = Vector3.one;
                }
                if (SelectItem.value == packageId1)
                {
                    UPMove.localScale = Vector3.one;
                    DownMove.localScale = Vector3.zero;
                }
            }
        }
    }

    private void MoveSelectItem()
    {
        int inPackageId = SelectItem.value == packageId0 ? packageId1 : packageId0;
        int outPackageId = SelectItem.value == packageId0 ? packageId0 : packageId1;
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
        int count = PackageManager.instance.GetPackageItemCount((int)SelectItem.value, SelectItem.dataId);
        SelectCount = math.clamp(int.Parse(value), 1, count);
        buyCountValue.SetTextWithoutNotify(SelectCount.ToString());
    }

    private void AddSelectCount()
    {
        int count = PackageManager.instance.GetPackageItemCount((int)SelectItem.value, SelectItem.dataId);
        SelectCount++;
        SelectCount = math.clamp(SelectCount, 1, count);
        buyCountValue.SetTextWithoutNotify(SelectCount.ToString());
    }

    private void ReduceSelectCount()
    {
        int count = PackageManager.instance.GetPackageItemCount((int)SelectItem.value, SelectItem.dataId);
        SelectCount--;
        SelectCount = math.clamp(SelectCount, 1, count);
        buyCountValue.SetTextWithoutNotify(SelectCount.ToString());
    }

   
    public override async void InitReferenceData(PackageList v)
    {
        base.InitReferenceData(v);
        var packageData0 = v.packageDatas[0];
        var packageData1 = v.packageDatas[1];
        PackageCaseCount0 = packageData0.caseCount;
        packageSetData0 = await GameDataManager.instance.GetAsyncData<PackageSetData>(packageData0.dataId);
        PackageCaseCount1 = packageData1.caseCount;
        packageSetData1 = await GameDataManager.instance.GetAsyncData<PackageSetData>(packageData1.dataId);

        Item defaultItem = default(Item); 
        List<Item> items0 = new List<Item>();
        int itemCaseCount0 = 0;
        if (packageData0.items != null)
        {
            for(int i = 0; i < packageData0.items.Count; i++)
            {
                Item item = packageData0.items[i];
                if (packageSetData1.packageType == PackageType.鲜活&&!item.isFresh)
                {
                    item.locked = true;
                }
                if (packageSetData1.packageType == PackageType.非鲜活&& item.isFresh)
                {
                    item.locked = true;
                }
                items0.Add(item);
            } 
            itemCaseCount0 = packageData0.items.Count;
        }
        for (int i = items0.Count; i < packageData0.caseCount; i++)
        {
            items0.Add(defaultItem);
        }

       

        Title0.text = packageSetData0.packageName;
        itemBoxs0.InitListData(items0, SelectPackageItem, toggleGroup: itemSelectGroup);
        caseCount0.text = $"{itemCaseCount0}/{packageData0.caseCount}";
        packageLevelUp0.transform.localScale = packageSetData0.canLevelUp ? Vector3.one : Vector3.zero;
        packageId0 = packageData0.instanceId;

       
        List<Item> items1 = new List<Item>();
        int itemCaseCount1 = 0;
        if (packageData1.items != null)
        {
            for (int i = 0; i < packageData1.items.Count; i++)
            {
                Item item = packageData1.items[i];
                if (packageSetData0.packageType == PackageType.鲜活 && !item.isFresh)
                {
                    item.locked = true;
                }
                if (packageSetData0.packageType == PackageType.非鲜活 && item.isFresh)
                {
                    item.locked = true;
                }
                items1.Add(item);
            }
             
            itemCaseCount1 = packageData1.items.Count;
        }
        for (int i = items1.Count; i < packageData1.caseCount; i++)
        {
            items1.Add(defaultItem);
        }

       

        Title1.text = packageSetData1.packageName;
        itemBoxs1.InitListData(items1, SelectPackageItem, toggleGroup: itemSelectGroup);
        caseCount1.text = $"{itemCaseCount1}/{packageData1.caseCount}";
        packageLevelUp1.transform.localScale = packageSetData1.canLevelUp ? Vector3.one : Vector3.zero;
        packageId1 = packageData1.instanceId;

        itemBoxs0.ClearSelect();
        itemBoxs1.ClearSelect();
    }
}