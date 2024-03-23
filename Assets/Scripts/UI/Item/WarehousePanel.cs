using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate void PackageItemAction(Item item, int packageId);

public class WarehousePanel : GamePanel<PackageList>
{
    [SerializeField]
    private PackageSelectReference packageSelect;

    [SerializeField]
    private Transform packageSelectParent;

    [SerializeField]
    private ToggleGroup packageSelectGroup;

    [SerializeField]
    private Button packageLevelUp;

    [SerializeField]
    private Transform ItemInformation;

    [SerializeField]
    private Button ShortCutActionButton;

    [SerializeField]
    private TextMeshProUGUI Title;

    [SerializeField]
    private TextMeshProUGUI caseCount;

    [SerializeField]
    private Image ItemIcon;

    [SerializeField]
    private TextMeshProUGUI ItemName;

    [SerializeField]
    private TextMeshProUGUI Price;

    [SerializeField]
    private TextMeshProUGUI Type, Property, Info;

    [SerializeField]
    private Button ActionButton, ReturnButton;

    [SerializeField]
    private TextMeshProUGUI ActionName;

    [SerializeField]
    private Transform InfoItemValueBg;

    [SerializeField]
    private Image InfoItemValue;

    [SerializeField]
    private ItemBoxReference itemBoxReference;

    [SerializeField]
    private Transform itemParent;

    [SerializeField]
    private ToggleGroup itemSelectGroup;

    private DisplayList<ItemBoxReference, Item> itemBoxs;
    private DisplayList<PackageSelectReference, PackageData> packageSelectList;

    private PackageList packageList;
    private PackageData selectPackageData;
    private Item SelectItem;
    private PackageItemAction selectItemAction;

    protected override void Awake()
    {
        base.Awake();
        var packageDatas = packageList.packageDatas;

        ReturnButton.onClick.AddListener(Close);
        itemBoxs = new DisplayList<ItemBoxReference, Item>(itemBoxReference, itemParent);

        ActionButton.onClick.AddListener(() =>
        {
            if (selectItemAction != null)
            {
                selectItemAction(SelectItem, selectPackageData.instanceId);
            }
        });

        ItemInformation.localScale = Vector3.zero;

        packageSelectList = new DisplayList<PackageSelectReference, PackageData>(packageSelect, packageSelectParent);
        packageLevelUp.onClick.AddListener(TryPackageLevelUp);

        ShortCutActionButton.onClick.AddListener(() =>
        {
            if (SelectItem.instanceId != 0)
            {
                SelectPackageItemAction selectPackageItemAction = new SelectPackageItemAction
                {
                    item = SelectItem,
                    packageId = selectPackageData.instanceId
                };
                GameActionManager.instance.QueueAction(selectPackageItemAction);
            }
        });
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshPackage>(RefreshPackage);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshPackage>(RefreshPackage);
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        packageSelect = FindChildGameObject<PackageSelectReference>("PackageSelect");
        packageSelectParent = FindChildGameObject("PackageSelectParent");

        Title = FindChildGameObject<TextMeshProUGUI>("Title");
        caseCount = FindChildGameObject<TextMeshProUGUI>("CaseCount");
        ItemIcon = FindChildGameObject<Image>("ItemIcon");
        ItemName = FindChildGameObject<TextMeshProUGUI>("ItemName");
        Price = FindChildGameObject<TextMeshProUGUI>("Price");
        Type = FindChildGameObject<TextMeshProUGUI>("Type");
        Property = FindChildGameObject<TextMeshProUGUI>("Property");
        Info = FindChildGameObject<TextMeshProUGUI>("Info");
        ActionButton = FindChildGameObject<Button>("ActionButton");
        ActionName = FindChildGameObject<TextMeshProUGUI>("ActionName");
        itemBoxReference = FindChildGameObject<ItemBoxReference>("ItemBoxReference");
        itemParent = FindChildGameObject("ItemParent");
        itemSelectGroup = FindChildGameObject<ToggleGroup>("ItemParent");
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        ItemInformation = FindChildGameObject("InformationObj");
        packageSelectGroup = FindChildGameObject<ToggleGroup>("PackageSelectParent");
        Price = FindChildGameObject<TextMeshProUGUI>("MoneyValue");
        packageLevelUp = FindChildGameObject<Button>("LevelUp");
        InfoItemValueBg = FindChildGameObject("InfoItemValueBg");
        InfoItemValue = FindChildGameObject<Image>("InfoItemValue");
        ShortCutActionButton = FindChildGameObject<Button>("ShortCutAction");
    }

    public override void InitReferenceData(PackageList v)
    {
        base.InitReferenceData(v);
        packageList = v;
        ShortCutActionButton.transform.localScale = v.canSetShortcut ? Vector3.one : Vector3.zero;
        packageSelectList.InitListData(packageList.packageDatas, SelectPackage, packageSelectGroup);
        SelectPackage(packageList.packageDatas[0]);
        //this.RefreshPackage();
        //RefreshPackage();
    }

    public override void Close()
    {
        base.Close();
        if (otherSelectItemAction != null)
        {
            //  otherSelectItemAction(default(Item), selectPackageData.instanceId);
        }
    }

    private async void TryPackageLevelUp()
    {
        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(selectPackageData.dataId);
        if (packageSetData && packageSetData.canLevelUp)
        {
            int cost = packageSetData.levelUpCost * selectPackageData.caseCount;
            string notice = $"拓展{packageSetData.packageName}空间?";
            PayManager.instance.PayAction("空间拓展", notice, cost, PayType.金币, (bool result) =>
            {
                if (result)
                {
                    PackageManager.instance.AddPackageUpLevel(selectPackageData.instanceId);
                }
            });
        }
    }

    private void SelectPackage(PackageData packageData, bool selected = true)
    {
        if (selected)
        {
            selectPackageData = packageData;
            RefreshPackage();
            ItemInformation.localScale = Vector3.zero;
        }
    }

    public void SetSelectItemAction(PackageItemAction selectItemAction, string actionName)
    {
        ActionName.text = actionName;
        this.selectItemAction = selectItemAction;
    }

    public void SetOtherSelectAction(PackageItemAction selectItemAction)
    {
        otherSelectItemAction = selectItemAction;
    }

    private PackageItemAction otherSelectItemAction;

    private async void SelectPackageItem(Item item, bool selected = true)
    {
        if (selected)
        {
            if (item.dataId == 0)
            {
                ItemInformation.localScale = Vector3.zero;
                if (otherSelectItemAction != null)
                {
                    otherSelectItemAction.Invoke(default(Item), selectPackageData.instanceId);
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

                if (otherSelectItemAction != null)
                {
                    otherSelectItemAction.Invoke(item, selectPackageData.instanceId);
                }
            }
        }
        else if (SelectItem.instanceId == item.instanceId)
        {
            ItemInformation.localScale = Vector3.zero;
        }
    }

    private void RefreshPackage(RefreshPackage RefreshPackage)
    {
        this.RefreshPackage();
    }

    private async void RefreshPackage()
    {
        //selectPackageData = packageList.packageDatas[selectIndex];

        List<Item> items = new List<Item>();
        if (selectPackageData.items != null)
        {
            if (packageList.itemMatchData.matchValues == null)
            {
                items.AddRange(selectPackageData.items);
            }
            else
            {
                for (int i = 0; i < selectPackageData.items.Count; i++)
                {
                    var item = selectPackageData.items[i];
                    item.locked = !packageList.itemMatchData.MatchAction(item);
                    items.Add(item);
                }
            }
        }

        for (int i = items.Count; i < selectPackageData.caseCount; i++)
        {
            items.Add(default(Item));
        }
        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(selectPackageData.dataId);
        Title.text = packageSetData.packageName;

        itemBoxs.InitListData(items, SelectPackageItem, toggleGroup: itemSelectGroup);
        // if(items.Count>0) { SelectPackageItem(items[0]); }
        itemBoxs.ClearSelect();
        ItemInformation.localScale = Vector3.zero;

        caseCount.text = $"{selectPackageData.items.Count}/{selectPackageData.caseCount}";
    }
}