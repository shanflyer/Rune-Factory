using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class WarehousePanel : GamePanel<PackageList>
{
    [SerializeField]
    PlayerEquipQuickReference playerEquipQuickReference;
    [SerializeField]
    Dropdown packageSelect;
    [SerializeField]
    Text caseCount;
    [SerializeField]
    Image ItemIcon;
    [SerializeField]
    Text ItemName;
    [SerializeField]
    Text Price;
    [SerializeField]
    Text Type, Property, Info;
    [SerializeField]
    Button ActionButton,ReturnButton;
    [SerializeField]
    Text ActionName;

    [SerializeField]
    ItemBoxReference itemBoxReference;
    [SerializeField]
    Transform itemParent; 
    DisplayList<ItemBoxReference, Item> itemBoxs;

    PackageList packageList;
    int selectIndex;

    Item SelectItem;
    SelectAction<Item> selectItemAction;
    protected override void Awake()
    {
        base.Awake();
        var packageDatas = packageList.packageDatas;
        packageSelect.options.Clear();
        for (int i = 0; i < packageDatas.Count; i++)
        {
            packageSelect.options.Add(new Dropdown.OptionData(packageDatas[i].name));
        } 
        packageSelect.onValueChanged.AddListener((int index) =>
        {
            if (selectIndex != index)
            {
                selectIndex = index;
                RefreshPackage();
            }
            
        });
        ReturnButton.onClick.AddListener(Close); 
        itemBoxs=new DisplayList<ItemBoxReference,Item>(itemBoxReference,itemParent);

        ActionButton.onClick.AddListener(() =>
        {
            if(selectItemAction != null)
            {
                selectItemAction(SelectItem);
            }
        });

        playerEquipQuickReference.InitData(CharacterManager.instance.TeamerEquipAndProperty);
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
        playerEquipQuickReference = FindChildGameObject<PlayerEquipQuickReference>("PlayerEquipQuickReference");
        packageSelect = FindChildGameObject<Dropdown>("PackageSelect");
        caseCount = FindChildGameObject<Text>("CaseCount");
        ItemIcon = FindChildGameObject<Image>("ItemIcon");
        ItemName = FindChildGameObject<Text>("ItemName");
        Price = FindChildGameObject<Text>("Price");
        Type = FindChildGameObject<Text>("Type");
        Property = FindChildGameObject<Text>("Prooerty");
        Info = FindChildGameObject<Text>("Info");
        ActionButton = FindChildGameObject<Button>("ActionButton"); 
        ActionName = FindChildGameObject<Text>("ActionName");
        itemBoxReference = FindChildGameObject<ItemBoxReference>("ItemBoxReference");
        itemParent = FindChildGameObject("ItemParent");
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
    }
    public override void InitReferenceData(PackageList v)
    {
        base.InitReferenceData(v);
        packageList = v;
        selectIndex = 0;
        RefreshPackage();
    }

    
    public void SetSelectItemAction(SelectAction<Item> selectItemAction,string actionName)
    {
        ActionName.text = actionName;
        this.selectItemAction = selectItemAction;
    }
  
    async void SelectPackageItem(Item item)
    {
        if (item.instanceId < 0)
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
        else
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
      


    }

    PackageData selectPackageData;
    void RefreshPackage(RefreshPackage RefreshPackage)
    {
        this.RefreshPackage();
    }
     async void RefreshPackage()
    {
        selectPackageData = packageList.packageDatas[selectIndex];

        List<Item> items = selectPackageData.items;
        for(int i=items.Count;i< selectPackageData.caseCount;i++)
        {
            items.Add(default(Item));
        }
        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(selectPackageData.dataId);

        for(int i = 0; i < packageSetData.count; i++)
        {
            Item item = default(Item);
            item.instanceId = -1;
            items.Add(item);
        }
        itemBoxs.InitListData(selectPackageData.items, SelectPackageItem);
        caseCount.text = $"{selectPackageData.items.Count}/{selectPackageData.caseCount}";

    }
}
