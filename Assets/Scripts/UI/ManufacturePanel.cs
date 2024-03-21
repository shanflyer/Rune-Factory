using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static TMPro.TMP_Dropdown;
using System.Threading.Tasks;

public class FormulaOptionData: OptionData
{ 
    public int formulaId; 
    public bool open;
}

public class ManufacturePanel : GamePanel<Manufature>
{
    [SerializeField]
    private TextMeshProUGUI title;
    [SerializeField]
    private Button ReturnButton;
    [SerializeField]
    private TMP_Dropdown FormulaDropdown;
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
        FormulaDropdown = FindChildGameObject<TMP_Dropdown>("FormulaDropdown");

        title = FindChildGameObject<TextMeshProUGUI>("Title");

        OutItemBoxReference = FindChildGameObject<ItemBoxReference>("OutputItemBoxReference");
        ReduceButton = FindChildGameObject<Button>("ReduceButton");
        AddButton = FindChildGameObject<Button>("AddButton");
        ItemCountValue = FindChildGameObject<TextMeshProUGUI>("ItemCountValue");
        CreatButton = FindChildGameObject<Button>("CreatButton");
        AutoSelect = FindChildGameObject<Button>("AutoSelect");
        RPCost = FindChildGameObject<TextMeshProUGUI>("RPCost");

        FormulaItemBoxGroup = FindChildGameObject<ToggleGroup>("Material");
        FormulaItemBoxReferences = new List<ItemBoxReference>
        {
            FindChildGameObject<ItemBoxReference>("ItemBoxReference1"),
            FindChildGameObject<ItemBoxReference>("ItemBoxReference2"),
            FindChildGameObject<ItemBoxReference>("ItemBoxReference3"),
            FindChildGameObject<ItemBoxReference>("ItemBoxReference4")
        };

        ItemType = FindChildGameObject<TextMeshProUGUI>("ItemType");
        selectItemName = FindChildGameObject<TextMeshProUGUI>("ItemName");
        selectActionButton = FindChildGameObject<Button>("ActionButton");
        itemInfo = FindChildGameObject<TextMeshProUGUI>("Info");
        itemProperty = FindChildGameObject<TextMeshProUGUI>("Property");
        moneyValue = FindChildGameObject<TextMeshProUGUI>("MoneyValue");
        ItemIcon = FindChildGameObject<Image>("ItemIcon");
        InformationObj = FindChildGameObject("InformationObj");
        selectActionButtonName = FindChildGameObject<TextMeshProUGUI>("ActionName");

    }

    protected override void Awake()
    {
        base.Awake();

        formulaTypes = new DisplayList<FormulaTypeReference, FormulaTypeData>(formulaTypeReference, formulaTypeParent);

        ReduceButton.onClick.AddListener(() =>
        {
            produceCount--;
            produceCount = math.clamp(produceCount, 1, produceCount); 
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

                GameNotificationManager.instance.DisplayTips("生产", "材料不足");
               // InformationController.instance.AddInformation("材料不足",true,true);
                produceCount--;
            }
        });

        FormulaDropdown.onValueChanged.AddListener(async (int index) =>
        {
            FormulaOptionData formulaOptionData = FormulaDropdown.options[index] as FormulaOptionData;
            int seleciId = formulaOptionData.formulaId;
            if (seleciId == 0)
            {
                selectFormula = default(Formula);
                selectFormulaData = null; 
            }
            else
            {
                selectFormula = manufature.formulas[seleciId];
                selectFormulaData=await GameDataManager.instance.GetAsyncData<FormulaData>(seleciId);
            }

            DisplayFormula();
        });


        AutoSelect.onClick.AddListener(AutoSelectMaterials);
        CreatButton.onClick.AddListener(CreatItem);
        ReturnButton.onClick.AddListener(Close);
    }

    private Dictionary<FormulaType, List<FormulaData>> allFormulaDatas;
    private Formula selectFormula;
    private FormulaData selectFormulaData;
    private ManufactureData manufactureData;
    private FormulaData matchFormula;
    private int formulaCost = 0;
    private int produceCount
    {
        get
        {
            return _produceCount;
        }
        set
        {
            _produceCount = value;
            ItemCountValue.text = produceCount.ToString();
            ReduceButton.transform.localScale = produceCount > 1 ? Vector3.one : Vector3.zero;
            RefreshCost();
        }
    }
    private int _produceCount = 1;

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

            if (manufature.formulas.TryGetValue(matchFormula.id, out var formula) && formula.isOpen)
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
                if (matchFormula.id !=0)
                {
                    productId = manufactureData.defaultProduct;
                }else if (manufature.formulas.TryGetValue(matchFormula.id, out var formula) && !formula.isOpen)
                {
                    ManufatureManager.instance.OpenFormula(manufature.instanceId, formula.id);
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

        List<OptionData> formulaOptionDatas = new List<OptionData>
        {
           new FormulaOptionData
            {
                open=true,
                text="无"
            }
        };

        foreach(var type in nowSelectFormulaTypes)
        {
            if(allFormulaDatas.TryGetValue(type,out var formulaDatas))
            {
                for(int i = 0; i < formulaDatas.Count; i++)
                {
                    FormulaData formulaData = formulaDatas[i];
                    Formula formula = manufature.formulas[formulaData.id];
                    FormulaOptionData formulaOptionData = new FormulaOptionData
                    {
                        formulaId=formula.id,
                        open = formula.isOpen,
                        text = formula.isOpen ? formulaData.formulaName : "???"
                    };
                    formulaOptionDatas.Add(formulaOptionData);
                }
               
            }
        }
        FormulaDropdown.options= formulaOptionDatas; 
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

        int formulaCount = 0;

        int product = GameCommon.defaultProduct;
        int instanceId = 0;
        for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
        {
            var referene = FormulaItemBoxReferences[i];
            if (referene.Item.instanceId != 0)
            {
                formulaCount++;
                formulaCost += GameCommon.defaultPerRPCost;
            }
        } 

        if (matchFormula != null)
        {
            if(manufature.formulas.TryGetValue(matchFormula.id, out var formula))
            {
                if(formula.isOpen)
                {
                    instanceId = formulaCount >= matchFormula.Stuffs.Count ? 0 : -1;
                    product = matchFormula.Product;
                    formulaCost = matchFormula.PowerCost;
                }
                else
                {
                    instanceId = 0; 
                }
                Item item = new Item
                {
                    instanceId = instanceId,
                    dataId = product,
                    count = produceCount
                };
                SetOutItemBoxReference(item);
               
            }
        }       
        else
        {
            if (selectFormula.id != 0 && selectFormula.isOpen)
            {
                product = selectFormulaData.Product;

            } 
            if (product != GameCommon.defaultProduct)
            {
                bool match = true;
                bool isSetMatch = true; 
                foreach(var id in selectFormulaData.Stuffs)
                {
                    var reference = FormulaItemBoxReferences.Find(f => f.Item.dataId == id);
                    if (reference == null)
                    {
                        match = false;
                        break;
                    }
                    else if(reference.Item.instanceId==-1)
                    {
                        isSetMatch = false;
                    }
                }
                if (!match)
                {
                    product = GameCommon.defaultProduct;
                }
                else if(!isSetMatch)
                {
                    instanceId = -1;
                }

                formulaCost=selectFormulaData.PowerCost;
            }
            else if(FormulaItemBoxReferences.Exists(f => f.Item.instanceId != -1&&f.Item.instanceId!=0))
            {
                instanceId = 0;
            }
             

            Item item = new Item
            {
                instanceId = instanceId,
                dataId = product,
                count = produceCount
            };
            SetOutItemBoxReference(item);

        }
        RefreshCost();
        CreatButton.interactable=instanceId==0;
        
    }

    private void RefreshCost()
    {
        int nowPower = CharacterManager.instance.controllerCharacter.CharacterProperty.Power;
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

  
    private void DisplayFormula()
    {
        ClearFormulaItemBoxReferences();
        produceCount = 1;

        bool selectOpenFormula = selectFormula.id != 0 && selectFormula.isOpen;
        AutoSelect.interactable = selectOpenFormula;

        if (selectFormula.id!=0)
        {
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
            //SetOutItemBoxReference(outItem);
            RefreshRPCostAndOut();
        }
    }
    //自动选择材料
    private void AutoSelectMaterials()
    {
        if (selectFormulaData!=null)
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
            int instanceId = 0;
            if (!successSelect)
            {
                instanceId = -1; 
                InformationController.instance.AddInformation("原料不足!",true,true);
            }
            Item outItem = new Item
            {
                instanceId = instanceId,
                dataId = selectFormulaData.Product,
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

       foreach(var formulaList in allFormulaDatas)
        {
            for(int i = 0; i < formulaList.Value.Count; i++)
            {
                var formulaData = formulaList.Value[i];
                if (formulaData.Check(items))
                {
                    return formulaData;
                }
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
        produceCount = 1;
         
        ClearFormulaItemBoxReferences();
    }
    Manufature manufature;
    public override Task InitData(string dataKey)
    {

        Manufature manufature = ManufatureManager.instance.GetManufature(int.Parse(dataKey));
        if (manufature.instanceId != 0)
        {
            InitData(manufature);
        }

        return base.InitData(dataKey);
    }

    public override void InitReferenceData(Manufature v)
    {
        base.InitReferenceData(v);
        InitData(v);

        bool selectOpenFormula = selectFormula.id != 0 && selectFormula.isOpen; 
        AutoSelect.transform.localScale = selectOpenFormula ? Vector3.one : Vector3.zero;
    }

    async void InitData(Manufature v)
    {
        manufature = v;
        manufactureData = await GameDataManager.instance.GetAsyncData<ManufactureData>(v.dataId);
        title.text = manufactureData.manufactureName;
        CreatButton.interactable = false;
        AutoSelect.interactable = false;

        List<FormulaTypeData> formulaTypeDatas = new List<FormulaTypeData>();
        HashSet<FormulaType> formulaTypes = new HashSet<FormulaType>();

        allFormulaDatas = new Dictionary<FormulaType, List<FormulaData>>();
        List<int> typeFormulas = new List<int>();
        var formulas = ManufatureManager.instance.GetManufatureAllFormulas(manufature.instanceId);
        for (int i = 0; i < formulas.Count; i++)
        {
            var formaula = formulas[i];
            var formanulaData = await GameDataManager.instance.GetAsyncData<FormulaData>(formaula.id);
            formulaTypes.Add(formanulaData.formulaType);
            if (allFormulaDatas.TryGetValue(formanulaData.formulaType, out var ints))
            {
                ints.Add(formanulaData);
            }
            else
            {
                allFormulaDatas.Add(formanulaData.formulaType, new List<FormulaData> { formanulaData });
            }
        }

        foreach (var formulaType in formulaTypes)
        {
            formulaTypeDatas.Add(new FormulaTypeData { formulaType = formulaType });
            nowSelectFormulaTypes.Add(formulaType);
        }
        this.formulaTypes.InitListData(formulaTypeDatas, SelectFormulaTypeData);
        this.formulaTypes.ClearAll();

        RefreshFormulaSelect();
        InitDisplay();
    }
    void SelectFormulaTypeData(FormulaTypeData formulaTypeData,bool seleted)
    {
        if (seleted)
        {
            nowSelectFormulaTypes.Remove(formulaTypeData.formulaType);
        }
        else
        {
            nowSelectFormulaTypes.Add(formulaTypeData.formulaType);
        }
        RefreshFormulaSelect();
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