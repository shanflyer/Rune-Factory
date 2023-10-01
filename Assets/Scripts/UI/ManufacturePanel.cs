using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class ManufacturePanel : GamePanel<IReferenceData> 
{
    [SerializeField]
    Button ReturnButton;
    [SerializeField]
    Dropdown FormulaDropdown;
    [SerializeField]
    Toggle WeaponSelect, ClothesSelect;
    [SerializeField]
    List<ItemBoxReference> FormulaItemBoxReferences;
    [SerializeField]
    ItemBoxReference OutItemBoxReference;
    [SerializeField]
    Button ReduceButton, AddButton;
    [SerializeField]
    Text ItemCountValue;
    [SerializeField]
    Button CreatButton;
    [SerializeField]
    Button AutoSelect;
    [SerializeField]
    Text RPCost;

    [SerializeField]
    Text ItemType;
    [SerializeField]
    Text selectItemName;
    [SerializeField]
    Button selectActionButton;
    [SerializeField]
    Text selectActionButtonName;
    [SerializeField]
    Button selectBoxInfoButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        FormulaDropdown = FindChildGameObject<Dropdown>("FormulaDropdown");
        WeaponSelect = FindChildGameObject<Toggle>("WeaponSelect");
        ClothesSelect = FindChildGameObject<Toggle>("ClothesSelect");
        OutItemBoxReference = FindChildGameObject<ItemBoxReference>("OutputItemBoxReference");
        ReduceButton = FindChildGameObject<Button>("ReduceButton");
        AddButton=FindChildGameObject<Button>("AddButton");
        ItemCountValue = FindChildGameObject<Text>("ItemCountValue");
        CreatButton = FindChildGameObject<Button>("CreatButton");
        AutoSelect = FindChildGameObject<Button>("AutoSelect");
        RPCost = FindChildGameObject<Text>("RPCost");
        FormulaItemBoxReferences = new List<ItemBoxReference>();
        FormulaItemBoxReferences.Add(FindChildGameObject<ItemBoxReference>("ItemBoxReference1"));
        FormulaItemBoxReferences.Add(FindChildGameObject<ItemBoxReference>("ItemBoxReference2"));
        FormulaItemBoxReferences.Add(FindChildGameObject<ItemBoxReference>("ItemBoxReference3"));
        FormulaItemBoxReferences.Add(FindChildGameObject<ItemBoxReference>("ItemBoxReference4"));

        ItemType = FindChildGameObject<Text>("ItemType");
        selectItemName = FindChildGameObject<Text>("SelectItemName");
        selectActionButton = FindChildGameObject<Button>("ActionButton");
        selectBoxInfoButton = FindChildGameObject<Button>("Item");
    }
    protected override async void Awake()
    {
        base.Awake();
        ClearFormulaItemBoxReferences();
        allFormulaDatas = await GameDataManager.instance.GetAllAsyncData<FormulaData>();
        RefreshFormulaSelect();

        WeaponSelect.onValueChanged.AddListener((bool value) =>
        {
            RefreshFormulaSelect();
        });
        ClothesSelect.onValueChanged.AddListener((bool value) =>
        {
            RefreshFormulaSelect();
        });
        AutoSelect.onClick.AddListener(AutoSelectMaterials);


        ReturnButton.onClick.AddListener(Close);
    }

    List<FormulaData> allFormulaDatas;
    FormulaData selectFormulaData;

    int produceCount = 1;

    void RefreshFormulaSelect()
    {
        List<FormulaData> seletFormulaDatas = new List<FormulaData>();
        List<string> seletFormulaNames = new List<string>();
        seletFormulaDatas.Add(null);
        seletFormulaNames.Add("无");
        for (int i = 0; i < allFormulaDatas.Count; i++)
        {
            FormulaData formulaData = allFormulaDatas[i];
            if(formulaData.formulaType==FormulaType.武器&&!WeaponSelect.isOn)
            {
                continue;
            }
            if (formulaData.formulaType == FormulaType.防具 && !ClothesSelect.isOn)
            {
                continue;
            }
            seletFormulaDatas.Add(formulaData);
            seletFormulaNames.Add(formulaData.formulaName);
        }
        FormulaDropdown.onValueChanged.RemoveAllListeners();
         
        FormulaDropdown.AddOptions(seletFormulaNames);
        FormulaDropdown.onValueChanged.AddListener((int index) =>
        {
            selectFormulaData = seletFormulaDatas[index];
        });

    }

    void ClearFormulaItemBoxReferences()
    {
        for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
        {
            var formulaItemBoxRefrence = FormulaItemBoxReferences[i];
            SetFormulaItemBoxData(formulaItemBoxRefrence);
        }
    }

    //自动选择材料
    void AutoSelectMaterials()
    {
        if (selectFormulaData)
        {
            ClearFormulaItemBoxReferences();
            bool successSelect = true;
            produceCount = 1;
            for (int i = 0; i < selectFormulaData.Stuffs.Count; i++)
            {
                int itemDataId = selectFormulaData.Stuffs[i];
                int itemCount = PackageManager.instance.GetPlayerItemCount(itemDataId);
                if (itemCount > 0)
                {
                    Item item = new Item
                    {
                        instanceId = 1,
                        dataId = itemDataId,
                        count = 1
                    };
                    FormulaItemBoxReferences[i].InitData(item, (Item item) =>
                    {
                        DisplayItem(item);
                        SelectActionName = "移除";
                        selectActionButtonName.text = SelectActionName;
                        SelectAction = (Item item) =>
                        {
                            selectItem = default(Item);
                            DisplayItem(selectItem);
                            SetFormulaItemBoxData(FormulaItemBoxReferences[i]);
                        };
                    });
                }
                else
                {
                    successSelect = false;
                }
            }
            if (successSelect)
            {
                Item outItem = new Item
                {
                    instanceId = 1,
                    dataId = selectFormulaData.Product,
                    count = 1
                };
                SetOutItemBoxReference(outItem);
            }
            else
            {
                InformationController.instance.AddInformation("原料不足!");
                DisplayDefaultOutItem();
            } 
        }
    }

    void SetOutItemBoxReference(Item item)
    {
        OutItemBoxReference.InitData(item, (Item item) =>
        {
            SelectAction = null; DisplayItem(item);
        });
    }
    void DisplayDefaultOutItem()
    {
        Item item = new Item
        {
            instanceId = -1,
            dataId = GameCommon.DefaultOutItemId,
            count = produceCount
        };
        SetOutItemBoxReference(item);
    }


    public override async void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v); 
       
    }



    //ItemBoxReference SelectItemBoxRefrence;

    void SetFormulaItemBoxData(ItemBoxReference formulaItemBoxRefrence)
    {
        formulaItemBoxRefrence.InitData(default(Item), (Item item) =>
        {
            //SelectItemBoxRefrence = formulaItemBoxRefrence;
            PackageManager.instance.ShowAllPlayerPackage(SetFormulaItem, "选择");
            void SetFormulaItem(Item item)
            {
                formulaItemBoxRefrence.InitData(item, (Item item) =>
                {
                    DisplayItem(item);
                    SelectActionName = "移除";
                    selectActionButtonName.text = SelectActionName;
                    SelectAction = (Item item) =>
                    {
                        selectItem = default(Item);
                        DisplayItem(selectItem);
                        SetFormulaItemBoxData(formulaItemBoxRefrence);
                    };
                });
            }
        });
    }

    Item selectItem;

    SelectAction<Item> SelectAction;
    string SelectActionName;
    async void ClickItemInfoAction()
    {
        if (selectItem.instanceId != 0)
        {
           var panel =await UIManager.instance.ShowGamePanel<ItemInfoPanel, Item>(selectItem);
            if (panel != null && SelectAction != null)
            {
                panel.SetAction(SelectAction, SelectActionName);
            }
        }
    }
    async void DisplayItem(Item item)
    {
        if (item.instanceId == 0)
        {
            selectItemName.enabled = false;
            ItemType.enabled = false;
            selectActionButton.gameObject.SetActive(false);
            selectBoxInfoButton.gameObject.SetActive(false);
        }
        else
        {
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
            selectItemName.text = itemData.itemName;
            ItemType.text = itemData.type.ToString();

            selectItemName.enabled = true;
            ItemType.enabled = true; 
            selectBoxInfoButton.gameObject.SetActive(true);
             
            selectActionButton.gameObject.SetActive(SelectAction!=null);
        }
    }

   
     
    
}
