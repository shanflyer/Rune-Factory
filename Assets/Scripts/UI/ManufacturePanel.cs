using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManufacturePanel : GamePanel<ManufactureData>
{
    [SerializeField]
    private TextMeshProUGUI title;
    [SerializeField]
    private Button ReturnButton;
    [SerializeField]
    private Dropdown FormulaDropdown;
    [SerializeField]
    private FormulaTypeReference formulaTypeReference;
    [SerializeField]
    private Transform formulaTypeParent;
    DisplayList<FormulaTypeReference, FormulaTypeData> formulaTypes;
    [SerializeField]
    ToggleGroup FormulaItemBoxGroup;
    [SerializeField]
    private List<ItemBoxReference> FormulaItemBoxReferences;
    [SerializeField]
    private ItemBoxReference OutItemBoxReference;
    [SerializeField]
    private Button ReduceButton, AddButton;
    [SerializeField]
    private TextMeshProUGUI ItemCountValue;
    [SerializeField]
    private Button CreatButton;
    [SerializeField]
    private Button AutoSelect;
    [SerializeField]
    private TextMeshProUGUI RPCost;

    [SerializeField]
    private Image ItemIcon;
    [SerializeField]
    private TextMeshProUGUI ItemType;
    [SerializeField]
    private TextMeshProUGUI selectItemName;
    [SerializeField]
    private TextMeshProUGUI moneyValue;
    [SerializeField]
    private TextMeshProUGUI itemInfo;
    [SerializeField]
    private TextMeshProUGUI itemProperty;
    [SerializeField]
    private Transform InformationObj;

    [SerializeField]
    private Button selectActionButton;
    [SerializeField]
    private TextMeshProUGUI selectActionButtonName; 

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        formulaTypeReference = FindChildGameObject<FormulaTypeReference>("FormulaTypeReference");
        formulaTypeParent = FindChildGameObject("TypeList");

        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        FormulaDropdown = FindChildGameObject<Dropdown>("FormulaDropdown");



        OutItemBoxReference = FindChildGameObject<ItemBoxReference>("OutputItemBoxReference");
        ReduceButton = FindChildGameObject<Button>("ReduceButton");
        AddButton = FindChildGameObject<Button>("AddButton");
        ItemCountValue = FindChildGameObject<TextMeshProUGUI>("ItemCountValue");
        CreatButton = FindChildGameObject<Button>("CreatButton");
        AutoSelect = FindChildGameObject<Button>("AutoSelect");
        RPCost = FindChildGameObject<TextMeshProUGUI>("RPCost");

        FormulaItemBoxGroup = FindChildGameObject<ToggleGroup>("");
        FormulaItemBoxReferences = new List<ItemBoxReference>();
        FormulaItemBoxReferences.Add(FindChildGameObject<ItemBoxReference>("ItemBoxReference1"));
        FormulaItemBoxReferences.Add(FindChildGameObject<ItemBoxReference>("ItemBoxReference2"));
        FormulaItemBoxReferences.Add(FindChildGameObject<ItemBoxReference>("ItemBoxReference3"));
        FormulaItemBoxReferences.Add(FindChildGameObject<ItemBoxReference>("ItemBoxReference4"));

        ItemType = FindChildGameObject<TextMeshProUGUI>("ItemType");
        selectItemName = FindChildGameObject<TextMeshProUGUI>("ItemName");
        selectActionButton = FindChildGameObject<Button>("ActionButton");
        itemInfo = FindChildGameObject<TextMeshProUGUI>("Info");
        itemProperty = FindChildGameObject<TextMeshProUGUI>("Property");
        moneyValue = FindChildGameObject<TextMeshProUGUI>("MoneyValue");
        ItemIcon = FindChildGameObject<Image>("ItemIcon");
        InformationObj = FindChildGameObject("InformationObj");

    }

    protected override void Awake()
    {
        base.Awake();

        formulaTypes = new DisplayList<FormulaTypeReference, FormulaTypeData>(formulaTypeReference, formulaTypeParent);

        ReduceButton.onClick.AddListener(() =>
        {
            produceCount--;
            produceCount = math.clamp(produceCount, 1, produceCount);
            RefreshProductCount();
        });
        AddButton.onClick.AddListener(() =>
        {
            produceCount++;
            bool canAdd = true;
            for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
            {
                Item item = FormulaItemBoxReferences[i].Item;
                if (item.instanceId != 0)
                {
                    int nowCount = PackageManager.instance.GetPackageCaseCount(item.dataId);
                    if (nowCount < produceCount)
                    {
                        canAdd = false;
                        break;
                    }
                }
            }
            if (!canAdd)
            {
                InformationController.instance.AddInformation("材料不足");
                produceCount--;
            }
            RefreshProductCount();
        });

        AutoSelect.onClick.AddListener(AutoSelectMaterials);
        CreatButton.onClick.AddListener(CreatItem);
        ReturnButton.onClick.AddListener(Close);
    }

    private List<FormulaData> allFormulaDatas;
    private FormulaData selectFormulaData;
    private ManufactureData manufactureData;
    private FormulaData matchFormula;
    private int formulaCost = 0;
    private int produceCount = 1;

    private void CreatItem()
    {
        int totalCost = formulaCost * produceCount;
        int nowPower = CharacterManager.instance.player.CharacterProperty.Power;
        if (totalCost >= nowPower)
        {
            AudioController.instance.PlayAudio(SE.Return);
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("RP消耗过大"),
              $"{LanguageManage.SwitchStr("需消耗RP:")}{totalCost},{LanguageManage.SwitchStr("超过拥有RP;")}"
                + LanguageManage.SwitchStr("无法制作！"));
        }
        else
        {
            string noticeStr = "";
            AudioController.instance.PlayAudio(SE.Return);

            if (matchFormula != null && matchFormula.isOpen)
            {
                noticeStr += LanguageManage.SwitchStr("是否确定按配方开始制作？");
            }
            else
            {
                noticeStr += LanguageManage.SwitchStr("无法确定产出物，是否开始制作？");
            }

            async void CreatAction()
            {
                int productId = OutItemBoxReference.Item.dataId;
                if (matchFormula != null)
                {
                    productId = manufactureData.defaultProduct;
                }else if (!matchFormula.isOpen)
                {
                    matchFormula.isOpen = true;
                    GameNotificationManager.instance.DisplayTips("新配方获得!", $"发现了制作 {matchFormula.formulaName} 的配方");
                }
                 

                ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
                {
                    changeValue = -totalCost,
                    characterId = 0,
                    propertyType = CharacterPropertyType.体力
                };
                GameActionManager.instance.QueueAction(changeCharacterProperty, true);
                for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
                {
                    Item item = FormulaItemBoxReferences[i].Item;
                    if (item.instanceId != 0)
                    {
                        PackageManager.instance.RemovePlayerPackageItem(item.dataId, produceCount);
                    }
                }
                bool allSet = await PackageManager.instance.SetPlayerPackageItem(matchFormula.Product, produceCount);
                if (!allSet)
                {
                    InformationController.instance.AddInformation(LanguageManage.SwitchStr("空间不足，部分物体没有获得"));
                }
                InitDisplay();
            }
            GameManager.instance.ShowTwoSelectAction("", noticeStr, CreatAction, null);
        }
    }

    private void RefreshFormulaSelect()
    {
        List<FormulaData> seletFormulaDatas = new List<FormulaData>();
        List<string> seletFormulaNames = new List<string>();
        seletFormulaDatas.Add(null);
        seletFormulaNames.Add("无");
        for (int i = 0; i < allFormulaDatas.Count; i++)
        {
            FormulaData formulaData = allFormulaDatas[i];

            seletFormulaDatas.Add(formulaData);
            seletFormulaNames.Add(formulaData.formulaName);
        }
        FormulaDropdown.onValueChanged.RemoveAllListeners();

        FormulaDropdown.AddOptions(seletFormulaNames);
        FormulaDropdown.onValueChanged.AddListener((int index) =>
        {
            selectFormulaData = seletFormulaDatas[index];
            DisplayFormula();
        });
    }

    private void ClearFormulaItemBoxReferences()
    {
        for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
        {
            FormulaItemBoxReferences[i].ClearData();
        }
        OutItemBoxReference.ClearData();
        
    }

    private void RefreshRPCostAndOut()
    {
        matchFormula = CheckFormula();
        formulaCost = 0;
        if (matchFormula == null || !matchFormula.isOpen)
        {
            int formulaCount = 0;
            for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
            {
                var referene = FormulaItemBoxReferences[i];
                if (referene.Item.instanceId != 0)
                {
                    formulaCount++;
                    formulaCost += GameCommon.defaultPerRPCost;
                }
            }
            formulaCost *= formulaCount;

            Item item = new Item
            {
                instanceId = formulaCount>= FormulaItemBoxReferences.Count?0:-1,
                dataId = GameCommon.DefaultOutItemId,
                count = produceCount
            };
            SetOutItemBoxReference(item);
        }
        else
        {
            int clearCount = 0;
            for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
            {
                var referene = FormulaItemBoxReferences[i];
                if (referene.Item.instanceId < 0)
                {
                    clearCount++; 
                }
            }

            Item item = new Item
            {
                instanceId = clearCount >= FormulaItemBoxReferences.Count ? -1 : 0,
                dataId = matchFormula.Product,
                count = produceCount
            };
            SetOutItemBoxReference(item);

        }
        RefreshCost(); 
    }

    private void RefreshCost()
    {
        int nowPower = CharacterManager.instance.player.CharacterProperty.Power;
        int totalCost = formulaCost * produceCount;
        if (totalCost >= nowPower)
        {
            RPCost.text = $"<color=green>{totalCost}</color>/({nowPower})";
        }
        else
        {
            RPCost.text = $"<color=red>{totalCost}</color>/({nowPower})";
        }
    }

    private void RefreshProductCount()
    {
        ItemCountValue.text = produceCount.ToString();
        ReduceButton.transform.localScale = produceCount > 1 ? Vector3.one : Vector3.zero;
    }
    private void DisplayFormula()
    {
        if (selectFormulaData)
        {
            ClearFormulaItemBoxReferences(); 
            produceCount = 1;
            for (int i = 0; i < selectFormulaData.Stuffs.Count; i++)
            {
                int itemDataId = selectFormulaData.Stuffs[i];
                Item item = new Item
                {
                    instanceId = -1,
                    dataId = itemDataId,
                    count = 1
                };
                FormulaItemBoxReferences[i].InitData(item, DisplayItem, FormulaItemBoxGroup);
            } 
            Item outItem = new Item
            {
                instanceId = -1,
                dataId = matchFormula.Product,
                count = 1
            };
            SetOutItemBoxReference(outItem);
            RefreshRPCostAndOut();
        }
    }
    //自动选择材料
    private void AutoSelectMaterials()
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
                    FormulaItemBoxReferences[i].InitData(item, DisplayItem, FormulaItemBoxGroup);
                }
                else
                {
                    Item item = new Item
                    {
                        instanceId = -1,
                        dataId = itemDataId,
                        count = 1
                    };
                    FormulaItemBoxReferences[i].InitData(item, DisplayItem, FormulaItemBoxGroup);
                    successSelect = false;
                }
            }
            if (!successSelect)
            {
                InformationController.instance.AddInformation("原料不足!");
            }
            Item outItem = new Item
            {
                instanceId = !successSelect?-1:0,
                dataId = matchFormula.Product,
                count = 1
            };
            SetOutItemBoxReference(outItem);
            RefreshRPCostAndOut();
        }
    }

    private void SetOutItemBoxReference(Item item)
    {
        OutItemBoxReference.InitData(item,DisplayItem,FormulaItemBoxGroup);
    }
 
    private FormulaData CheckFormula()
    {
        List<int> items = new List<int>();
        for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
        {
            var reference = FormulaItemBoxReferences[i];
           // if (reference.Item.instanceId != 0)
            {
                items.Add(reference.Item.dataId);
            }
        }
        for (int i = 0; i < allFormulaDatas.Count; i++)
        {
            var formulaData = allFormulaDatas[i];
            if (formulaData.Check(items))
            {
                return formulaData;
            }
        }
        return null;
    }

    void InitDisplay()
    {
        for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
        {
            FormulaItemBoxReferences[i].ClearData();
        }
        OutItemBoxReference.ClearData();
        FormulaDropdown.value = 0;
        formulaCost = 0;
        produceCount = 0;

        RefreshCost();
        ClearFormulaItemBoxReferences();
    }
    public override void InitReferenceData(ManufactureData v)
    {
        base.InitReferenceData(v);
        manufactureData = v;
        title.text = manufactureData.manufactureName;
         
        List<FormulaTypeData> formulaTypeDatas = new List<FormulaTypeData>();
        for(int i=0;i< manufactureData.formulaTypes.Count; i++)
        {
            formulaTypeDatas.Add(new FormulaTypeData { formulaType = manufactureData.formulaTypes[i] });
        }
        formulaTypes.InitListData(formulaTypeDatas, SelectFormulaTypeData);

        RefreshFormulaSelect(); 
        InitDisplay();
    }
    async void SelectFormulaTypeData(FormulaTypeData formulaTypeData,bool seleted)
    {
        if (seleted)
        {
            nowSelectFormulaTypes.Remove(formulaTypeData.formulaType);
        }
        else
        {
            nowSelectFormulaTypes.Add(formulaTypeData.formulaType);
        }

        var formulaDatas = await GameDataManager.instance.GetAllAsyncData<FormulaData>();
        allFormulaDatas = new List<FormulaData>(); 
        for (int i = 0; i < formulaDatas.Count; i++)
        {
            var formulaData = formulaDatas[i];
            if (nowSelectFormulaTypes.Contains(formulaData.formulaType))
            {
                allFormulaDatas.Add(formulaData);
            }
        }
    }
    HashSet<FormulaType> nowSelectFormulaTypes = new HashSet<FormulaType>();
    ItemBoxReference SelectItemBoxRefrence;

    private async void DisplayItem(Item item,bool selected=true)
    {
        if (item.instanceId == 0)
        {
            InformationObj.transform.localScale = Vector3.zero;
        }
        else
        {
            InformationObj.transform.localScale = Vector3.one;
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
            selectItemName.text = itemData.itemName;
            ItemType.text = itemData.type.ToString();
            itemInfo.text = itemData.info;
            itemProperty.text = itemData.property.ToString();
            moneyValue.text = itemData.sellPrice.ToString();
            ItemIcon.sprite = itemData.icon;

            if (item.instanceId == 0)
            {
                selectActionButton.transform.localScale = Vector3.zero;
            }
            else
            {
                selectActionButton.transform.localScale = Vector3.one;
                if (item.instanceId == -1)
                {
                    selectActionButtonName.text = "放入";
                    selectActionButton.onClick.RemoveAllListeners();
                    selectActionButton.onClick.AddListener(() =>
                    {
                        PackageManager.instance.ShowAllPlayerPackage(SetFormulaItem, "选择");
                    });
                    
                    void SetFormulaItem(Item item, int packageId)
                    {
                        SelectItemBoxRefrence.InitData(item, DisplayItem,FormulaItemBoxGroup);
                        selectActionButtonName.text = "移除";
                    }
                    RefreshRPCostAndOut();
                }
                else
                {
                    selectActionButtonName.text = "移除";
                    selectActionButton.onClick.RemoveAllListeners();
                    selectActionButton.onClick.AddListener(() =>
                    {
                        selectActionButtonName.text = "放入";
                        SelectItemBoxRefrence.InitData(default(Item), DisplayItem, FormulaItemBoxGroup);
                    });
                    
                    RefreshRPCostAndOut();
                }
            }
            
        }
    }
}