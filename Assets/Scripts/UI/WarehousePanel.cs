using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class WarehousePanel : GamePanel<PackageList>
{ 
    [SerializeField]
    PackageSelectReference packageSelect;
    [SerializeField]
    Transform packageSelectParent;
    [SerializeField]
    ToggleGroup packageSelectGroup;
    [SerializeField]
    Button packageLevelUp;

    [SerializeField]
    TextMeshProUGUI Title;
    [SerializeField]
    TextMeshProUGUI caseCount;
    [SerializeField]
    Image ItemIcon;
    [SerializeField]
    TextMeshProUGUI ItemName;
    [SerializeField]
    TextMeshProUGUI Price;
    [SerializeField]
    TextMeshProUGUI Type, Property, Info;
    [SerializeField]
    Button ActionButton,ReturnButton;
    [SerializeField]
    TextMeshProUGUI ActionName;

    [SerializeField]
    ItemBoxReference itemBoxReference;
    [SerializeField]
    Transform itemParent; 
    DisplayList<ItemBoxReference, Item> itemBoxs;
    DisplayList<PackageSelectReference, PackageData> packageSelectList;

    PackageList packageList;
    PackageData selectPackageData;
    Item SelectItem;
    SelectAction<Item> selectItemAction;
    protected override void Awake()
    {
        base.Awake();
        var packageDatas = packageList.packageDatas;
       
        ReturnButton.onClick.AddListener(Close); 
        itemBoxs=new DisplayList<ItemBoxReference,Item>(itemBoxReference,itemParent);

        ActionButton.onClick.AddListener(() =>
        {
            if(selectItemAction != null)
            {
                selectItemAction(SelectItem);
            }
        });

        packageSelectList = new DisplayList<PackageSelectReference, PackageData>(packageSelect, packageSelectParent);
        packageLevelUp.onClick.AddListener(TryPackageLevelUp);
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
        Property = FindChildGameObject<TextMeshProUGUI>("Prooerty");
        Info = FindChildGameObject<TextMeshProUGUI>("Info");
        ActionButton = FindChildGameObject<Button>("ActionButton"); 
        ActionName = FindChildGameObject<TextMeshProUGUI>("ActionName");
        itemBoxReference = FindChildGameObject<ItemBoxReference>("ItemBoxReference");
        itemParent = FindChildGameObject("ItemParent");
        ReturnButton = FindChildGameObject<Button>("ReturnButton");

        packageSelectGroup = FindChildGameObject<ToggleGroup>("PackageSelectParent");
        Price = FindChildGameObject<TextMeshProUGUI>("MoneyValue");
        packageLevelUp = FindChildGameObject<Button>("LevelUp");
    }
    public override void InitReferenceData(PackageList v)
    {
        base.InitReferenceData(v);
        packageList = v;

        packageSelectList.InitListData(packageList.packageDatas, SelectPackage, packageSelectGroup);

        //RefreshPackage();
    }
    async void TryPackageLevelUp()
    {
        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(selectPackageData.dataId);
        if (packageSetData && packageSetData.canLevelUp)
        {
            int cost = packageSetData.levelUpCost * selectPackageData.caseCount;
            string notice = $"拓展{packageSetData.packageName}空间?";
            PayManager.instance.PayAction("空间拓展", notice, cost, PayType.金币, () =>
            {
                PackageManager.instance.AddPackageUpLevel(selectPackageData.instanceId);
            });
        }
    }
    void SelectPackage(PackageData packageData, bool selected = true)
    {
        selectPackageData = packageData;
        
        RefreshPackage();
    }
    
    public void SetSelectItemAction(SelectAction<Item> selectItemAction,string actionName)
    {
        ActionName.text = actionName;
        this.selectItemAction = selectItemAction;
    }
  
    async void SelectPackageItem(Item item,bool selected= true)
    {
        SelectItem = item;
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
        ItemIcon.sprite = itemData.icon;
        ItemIcon.enabled = true;
        ItemIcon.SetNativeSize();
        ItemName.text = itemData.name;
        Type.text = itemData.type.ToString();
        Info.text = itemData.text1.ToString();
        Property.text = itemData.property.ToString();
        Price.text = $"价值:{itemData.sellPrice}G";
    } 
    void RefreshPackage(RefreshPackage RefreshPackage)
    {
        this.RefreshPackage();
    }
    async void RefreshPackage()
    {
        //selectPackageData = packageList.packageDatas[selectIndex];

        List<Item> items = selectPackageData.items;
        for (int i = items.Count; i < selectPackageData.caseCount; i++)
        {
            items.Add(default(Item));
        }
        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(selectPackageData.dataId);
        Title.text = packageSetData.packageName;
        for (int i = 0; i < packageSetData.count; i++)
        {
            Item item = default(Item);
            item.instanceId = -1;
            items.Add(item);
        }
        itemBoxs.InitListData(selectPackageData.items, SelectPackageItem);
        caseCount.text = $"{selectPackageData.items.Count}/{selectPackageData.caseCount}";

    }
}
