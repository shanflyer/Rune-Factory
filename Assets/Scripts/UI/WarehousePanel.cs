using System.Collections;
using System.Collections.Generic;
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
    void SelectPackageItem(Item item)
    {

    }
    void RefreshPackage()
    {
        var packageData = packageList.packageDatas[selectIndex];
        itemBoxs.InitListData(packageData.items, SelectPackageItem);
        caseCount.text = $"{packageData.items.Count}/{packageData.caseCount}";
    }
}
