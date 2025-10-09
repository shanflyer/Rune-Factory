using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class FormulaOptionData : OptionData
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

    private DisplayList<FormulaTypeReference, FormulaTypeData> formulaTypes;

    [SerializeField]
    private ToggleGroup FormulaItemBoxGroup;

    [SerializeField]
    private List<ItemBoxReference> FormulaItemBoxReferences;

    [SerializeField]
    private ItemBoxReference OutItemBoxReference,CostItemBoxReference;

    [SerializeField]
    private Button ReduceButton, AddButton;

    [SerializeField]
    private TextMeshProUGUI ItemCountValue;

    [SerializeField]
    private Button CreatButton;

    [SerializeField]
    private TextMeshProUGUI creatButtonName;

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

    [SerializeField]
    private TextMeshProUGUI timeValue;

    [SerializeField]
    private Image timeSlider;

    [SerializeField]
    private ParticleSystem outEffect;

    public Image InfoItemValueImage;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        formulaTypeReference = FindChildGameObject<FormulaTypeReference>(LanguageManage.SwitchStr("FormulaTypeReference"));
        formulaTypeParent = FindChildGameObject(LanguageManage.SwitchStr("TypeList"));

        ReturnButton = FindChildGameObject<Button>(LanguageManage.SwitchStr("ReturnButton"));
        FormulaDropdown = FindChildGameObject<TMP_Dropdown>(LanguageManage.SwitchStr("FormulaDropdown"));

        title = FindChildGameObject<TextMeshProUGUI>(LanguageManage.SwitchStr("Title"));

        CostItemBoxReference = FindChildGameObject<ItemBoxReference>(LanguageManage.SwitchStr("CostItem"));
        OutItemBoxReference = FindChildGameObject<ItemBoxReference>(LanguageManage.SwitchStr("OutputItemBoxReference"));
        ReduceButton = FindChildGameObject<Button>(LanguageManage.SwitchStr("ReduceButton"));
        AddButton = FindChildGameObject<Button>(LanguageManage.SwitchStr("AddButton"));
        ItemCountValue = FindChildGameObject<TextMeshProUGUI>(LanguageManage.SwitchStr("ItemCountValue"));
        CreatButton = FindChildGameObject<Button>(LanguageManage.SwitchStr("CreatButton"));
        creatButtonName = CreatButton.gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
        AutoSelect = FindChildGameObject<Button>(LanguageManage.SwitchStr("AutoSelect"));
        RPCost = FindChildGameObject<TextMeshProUGUI>(LanguageManage.SwitchStr("RPCost"));

        FormulaItemBoxGroup = FindChildGameObject<ToggleGroup>(LanguageManage.SwitchStr("Material"));
        FormulaItemBoxReferences = new List<ItemBoxReference>
        {
            FindChildGameObject<ItemBoxReference>(LanguageManage.SwitchStr("ItemBoxReference1")),
            FindChildGameObject<ItemBoxReference>(LanguageManage.SwitchStr("ItemBoxReference2")),
            FindChildGameObject<ItemBoxReference>(LanguageManage.SwitchStr("ItemBoxReference3")),
            FindChildGameObject<ItemBoxReference>(LanguageManage.SwitchStr("ItemBoxReference4"))
        };

        ItemType = FindChildGameObject<TextMeshProUGUI>(LanguageManage.SwitchStr("ItemType"));
        selectItemName = FindChildGameObject<TextMeshProUGUI>(LanguageManage.SwitchStr("ItemName"));
        selectActionButton = FindChildGameObject<Button>(LanguageManage.SwitchStr("ActionButton"));
        itemInfo = FindChildGameObject<TextMeshProUGUI>(LanguageManage.SwitchStr("Info"));
        itemProperty = FindChildGameObject<TextMeshProUGUI>(LanguageManage.SwitchStr("Property"));
        moneyValue = FindChildGameObject<TextMeshProUGUI>(LanguageManage.SwitchStr("MoneyValue"));
        ItemIcon = FindChildGameObject<Image>(LanguageManage.SwitchStr("ItemIcon"));
        InformationObj = FindChildGameObject(LanguageManage.SwitchStr("InformationObj"));
        selectActionButtonName = FindChildGameObject<TextMeshProUGUI>(LanguageManage.SwitchStr("ActionName"));
        InfoItemValueImage = FindChildGameObject<Image>(LanguageManage.SwitchStr("InfoItemValue"));

        timeSlider = FindChildGameObject<Image>(LanguageManage.SwitchStr("TimeSlider"));
        timeValue = FindChildGameObject<TextMeshProUGUI>(LanguageManage.SwitchStr("TimeValue"));
        outEffect = FindChildGameObject<ParticleSystem>(LanguageManage.SwitchStr("OutEffect"));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshManufature>(RefreshManufature);
        GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared)
        {
            GameActionManager.instance.RemoveListener<RefreshManufature>(RefreshManufature);
            GameActionManager.instance.RemoveListener<UpdateGameTime>(UpdateGameTime);
        }
           
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
                if (item.dataId != 0)
                {
                    int nowCount = PackageManager.instance.GetPlayerItemCount(item.dataId);
                    if (nowCount < produceCount)
                    {
                        canAdd = false;
                        break;
                    }
                }
            }
            if (!canAdd)
            {
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("生产"), LanguageManage.SwitchStr("材料不足"));
                // InformationController.instance.AddInformation("材料不足",true,true);
                produceCount--;
            }
        });

        FormulaDropdown.onValueChanged.AddListener((int index) =>
        {
           SelectFormula(index);
        });

        AutoSelect.onClick.AddListener(AutoSelectMaterials);
        CreatButton.onClick.AddListener(CreatItem);
        ReturnButton.onClick.AddListener(Close);
    }

   void SelectFormula(int index)
    {
        int selectId = 0;
        if (FormulaDropdown.options.Count > index)
        {
            FormulaOptionData formulaOptionData = FormulaDropdown.options[index] as FormulaOptionData;
            selectId = formulaOptionData.formulaId;
        }
       
        if (selectId == 0)
        {
            selectFormula =null; 
        }
        else
        {
            ManufactureManager.instance.GetFormula(selectId, out selectFormula);  
        }

        DisplayFormula();
    }
    private Dictionary<FormulaType, List<Formula>> allFormulas;
    private Formula selectFormula; 
    private ManufactureData manufactureData;
    private Formula matchFormula;
    private int formulaCost = 0;
    private Item outItem;
    private Item costItem;
    private int produceCount
    {
        get
        {
            return _produceCount;
        }
        set
        {
            if (value > 1000) value = 1000;
            _produceCount = value;
            ItemCountValue.SetSWText(produceCount.ToString());
            ReduceButton.transform.localScale = produceCount > 1 ? Vector3.one : Vector3.zero;
            RefreshCost();
        }
    }

    private int _produceCount = 1;

    private async void CreatItem()
    {
        if (manufature.waitTime > 0)
        {
            string noticeStr = "是否确定中止生产,消耗的物体将消失？";
            GameManager.instance.ShowTwoSelectAction(LanguageManage.SwitchStr(""), noticeStr, () =>
            {
                ClearManufature clearManufature = new ClearManufature
                {
                    manufatureId = manufature.instanceId
                };
                GameActionManager.instance.QueueAction(clearManufature, true);

                AutoSelect.interactable = true;
                AddButton.interactable = true;
                ReduceButton.interactable = true;
                FormulaDropdown.interactable = true;
                FormulaDropdown.value = 0;
                formulaTypeParent.transform.localScale = Vector3.one;
                creatButtonName.SetSWText(LanguageManage.SwitchStr("制作"));
                InformationObj.transform.localScale = Vector3.zero;
            }, null);
            return;
        }

        if (manufature.product.x > 0 && manufature.waitTime < GameTimeManager.instance.totalMinute)
        {
            GetOutProduct();
            return;
        }

        bool costItemEnough = true;
        int nowCostItemCount = 0;
        if (costItem.dataId != 0)
        {
            nowCostItemCount = PackageManager.instance.GetPlayerItemCount(costItem.dataId);
            costItemEnough = nowCostItemCount >= costItem.count;
        }

        if (!costItemEnough)
        { 
            ItemData costItemData =await GameDataManager.instance.GetAsyncData<ItemData>(costItem.dataId);
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("消耗物品不足"),
              $"{LanguageManage.SwitchStr("需消耗:")}{costItemData.name}*{costItem.count},{LanguageManage.SwitchStr("当前拥有;")}{nowCostItemCount}/n{LanguageManage.SwitchStr("无法制作！")}");
        }
        else
        {
            int totalCost = formulaCost * produceCount;
            int nowPower = CharacterManager.instance.player.CharacterProperty.Power;
            if (totalCost >= nowPower)
            { 
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("RP消耗过大"),
                  $"{LanguageManage.SwitchStr("需消耗RP:")}{totalCost},{LanguageManage.SwitchStr("超过拥有RP;")}"
                    + LanguageManage.SwitchStr("无法制作！"));
            }
            else
            {
                string noticeStr = ""; 
                if (outItem.dataId == GameCommon.defaultProduct)
                {
                    noticeStr = LanguageManage.SwitchStr("无法确定产出物，是否开始制作？");
                }
                else if (selectFormula != null && outItem.dataId == selectFormula.formulaData.ProductItem.id)
                {
                    noticeStr = LanguageManage.SwitchStr("是否确定按配方开始制作？");
                }
                else
                {
                    noticeStr = LanguageManage.SwitchStr("是否确定开始制作？");
                }

                void CreatAction()
                {
                    manufature.startTime = GameTimeManager.instance.totalMinute;
                    int productId = outItem.dataId;
                    if (productId == GameCommon.defaultProduct)
                    {
                        if (matchFormula != null)
                        {
                            manufature.matchFormula = matchFormula;
                            productId = matchFormula.formulaData.ProductItem.id;
                            if (ManufactureManager.instance.GetFormula(matchFormula.formulaData.id, out var formula))
                            {
                                if (!formula.opened)
                                {
                                    manufature.product.z = GameCommon.defaultProduct;
                                }
                            }
                        }
                        else
                        {
                            manufature.product.z = GameCommon.defaultProduct;
                            productId = manufactureData.defaultProduct;
                        }
                        manufature.waitTime = GameTimeManager.instance.totalMinute + manufactureData.defaultProduceTime;
                    }
                    else
                    {
                        manufature.waitTime = GameTimeManager.instance.totalMinute + matchFormula.formulaData.produceTime;
                    }
                    if (matchFormula != null)
                    {
                        manufature.matchFormula = matchFormula;
                    }
                    manufature.product.x = productId;
                    manufature.product.y = produceCount;

                    ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
                    {
                        changeValue = -totalCost,
                        characterId = CharacterManager.instance.controllerCharacter.instanceId,
                        propertyType = CharacterPropertyType.体力
                    };
                    GameActionManager.instance.QueueAction(changeCharacterProperty, true);
                    for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
                    {
                        Item item = FormulaItemBoxReferences[i].Item;
                        if (item.instanceId != 0)
                        {
                            PackageManager.instance.RemovePlayerPackageItem(item.dataId, produceCount);
                            manufature.materials[i] = new int2(item.dataId, item.instanceId);
                        }
                    }

                    SetManufature setManufature = new SetManufature
                    {
                        manufature = manufature
                    };
                    GameActionManager.instance.QueueAction(setManufature, true);

                    SetCharacterAnimator setCharacterAnimatorValue = new SetCharacterAnimator
                    {
                        parameterType = ParameterType.FLOAT,
                        parameter = "CreatState",
                        floatValue = manufactureData.characterAnimatorState,
                        characterId = CharacterManager.instance.controllerCharacter.instanceId
                    };
                    GameActionManager.instance.QueueAction(setCharacterAnimatorValue);
                    SetCharacterAnimator setCharacterAnimator = new SetCharacterAnimator
                    {
                        parameterType = ParameterType.TRIGGER,
                        parameter = "Creat",
                        characterId = CharacterManager.instance.controllerCharacter.instanceId
                    };
                    GameActionManager.instance.QueueAction(setCharacterAnimator);

                    /*
                    bool allSet = await PackageManager.instance.SetPlayerPackageItem(productId, produceCount);
                    if (!allSet)
                    {
                        InformationController.instance.AddInformation(LanguageManage.SwitchStr("空间不足，部分物体没有获得"));
                    }*/
                    InitDisplay();
                    PackageManager.instance.RemovePlayerPackageItem(costItem.dataId, costItem.count);
                    
                    AutoSelect.interactable = false;
                    selectActionButton.transform.localScale = Vector3.zero;
                    AddButton.interactable = false;
                    ReduceButton.interactable = false;
                    FormulaDropdown.interactable = false;
                    formulaTypeParent.transform.localScale = Vector3.zero;
                    creatButtonName.SetSWText(LanguageManage.SwitchStr("中止"));
                }
                GameManager.instance.ShowTwoSelectAction(LanguageManage.SwitchStr(""), noticeStr, CreatAction, null);
            }
        } 
        
    }

    private async void RefreshFormulaSelect()
    {
        List<OptionData> formulaOptionDatas = new List<OptionData>();
        if (manufactureData.hideNull)
        {
            formulaOptionDatas.Add(new FormulaOptionData
            {
                open = true,
                text = "无"
            });
        } 
        if (this.formulaTypes.dataCount == 0)
        {
            foreach (var formulaDatas in allFormulas)
            {
                for (int i = 0; i < formulaDatas.Value.Count; i++)
                { 
                    Formula formula = formulaDatas.Value[i];
                    /*if (!formula.opened)
                    {
                        continue;
                    }*/
                    FormulaOptionData formulaOptionData = new FormulaOptionData
                    {
                        formulaId = formula.id,
                        open = formula.opened,
                        text = formula.opened ? formula.formulaData.formulaName : "????"
                    };
                    formulaOptionDatas.Add(formulaOptionData);
                }
            }
        }
        else
        {
            foreach (var type in nowSelectFormulaTypes)
            {
                if (allFormulas.TryGetValue(type, out var formulaDatas))
                {
                    for (int i = 0; i < formulaDatas.Count; i++)
                    {
                        Formula formula = formulaDatas[i];
                        /*
                        if (!formula.opened)
                        {
                            continue;
                        }*/
                        FormulaOptionData formulaOptionData = new FormulaOptionData
                        {
                            formulaId = formula.id,
                            open = formula.opened,
                            text = formula.opened? formula.formulaData.formulaName:"????"
                        };
                        formulaOptionDatas.Add(formulaOptionData);
                    }
                }
            }
        }

        FormulaDropdown.options = formulaOptionDatas;
        SelectFormula(0);
    }

    private async void ClearFormulaItemBoxReferences(bool clearOutBox = true)
    {
        Item defaultItem = default(Item);
        defaultItem.instanceId = -1;
        for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
        {
           await FormulaItemBoxReferences[i].InitData(defaultItem, null, FormulaItemBoxGroup);
            FormulaItemBoxReferences[i].SelectUIAction = DisplayItem;
        }
        if (clearOutBox)
        {
            OutItemBoxReference.ClearData();
        }
    }

    private async void RefreshRPCostAndOut()
    {
        matchFormula = CheckFormula();
        formulaCost = 0;

        int formulaCount = 0;

        int product = GameCommon.defaultProduct;

        for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
        {
            var referene = FormulaItemBoxReferences[i];
            if (referene.Item.instanceId != 0 && referene.Item.instanceId != -1)
            {
                formulaCount++;
                formulaCost += GameCommon.defaultPerRPCost;
            }
        }

        int instanceId = 1;
        if (matchFormula != null)
        {
            if (manufature.formulas.Contains(matchFormula.formulaData.id))
            { 
                if (matchFormula.opened)
                {
                    instanceId = formulaCount >= matchFormula.formulaData.StuffItems.Count ? 1 : -1;
                    product = matchFormula.formulaData.ProductItem.id;
                    formulaCost = matchFormula.formulaData.PowerCost; 
                }

                outItem = new Item
                {
                    instanceId = instanceId,
                    dataId = product,
                    count = produceCount
                };
            }
        }
        else
        {
            if (selectFormula != null && selectFormula.opened)
            {
                product = selectFormula.formulaData.ProductItem.id;
            }
            if (product != GameCommon.defaultProduct)
            {
                bool match = true;
                bool isSetMatch = true;
                foreach (var item in selectFormula.formulaData.StuffItems)
                {
                    var reference = FormulaItemBoxReferences.Find(f => f.Item.dataId == item.id);
                    if (reference == null)
                    {
                        match = false;
                        break;
                    }
                    else if (reference.Item.instanceId == -1)
                    {
                        isSetMatch = false;
                    }
                }
                if (!match)
                {
                    product = GameCommon.defaultProduct;
                }
                else if (!isSetMatch)
                {
                    instanceId = -1;
                }

                formulaCost = selectFormula.formulaData.PowerCost;
            }
            else if (!FormulaItemBoxReferences.Exists(f => f.Item.instanceId != -1 && f.Item.instanceId != 0))
            {
                instanceId = -1;
            }

            outItem = new Item
            {
                instanceId = instanceId,
                dataId = product,
                count = produceCount
            };

            if (formulaCount == 0)
            { 
                outItem.dataId = 0;
            }
        }

        //outItem.locked = instanceId < 0;
       
        if (manufature.waitTime <= GameTimeManager.instance.totalMinute && manufature.product.x != 0)
        {
            outItem.dataId = manufature.product.x;
        }
       await OutItemBoxReference.InitData(outItem, null, FormulaItemBoxGroup);
        OutItemBoxReference.SelectUIAction = DisplayItem;
        RefreshCost();
        creatButtonName.SetSWText(LanguageManage.SwitchStr("制作"));

        if (manufature.product.x != 0)
        {
            CreatButton.interactable = true;
            if (manufature.waitTime <= GameTimeManager.instance.totalMinute)
            {
                creatButtonName.SetSWText(LanguageManage.SwitchStr("取出"));
            }
            else
            {
                creatButtonName.SetSWText(LanguageManage.SwitchStr("中止"));
            }
        }
        else
        {
            CreatButton.interactable = formulaCount > 0;
        }

        if (manufactureData.defaultCostItem.x != 0)
        {
            costItem = new Item
            {
                dataId = manufactureData.defaultCostItem.x,
                count = manufactureData.defaultCostItem.y * produceCount,
                instanceId = 1
            };
            CostItemBoxReference.transform.localScale = Vector3.one;
           await CostItemBoxReference.InitData(costItem, null, FormulaItemBoxGroup);
            CostItemBoxReference.SelectUIAction= DisplayItem;

            int nowCount = PackageManager.instance.GetPlayerItemCount(costItem.dataId);
            if (nowCount <= costItem.count)
            {
                CostItemBoxReference.SetCountColor(Color.red);
            }
            else
            {
                CostItemBoxReference.SetCountColor(Color.green);
            }
        }
        else
        {
            CostItemBoxReference.transform.localScale = Vector3.zero;
        }
    }

    private void RefreshCost()
    {
        int nowPower = CharacterManager.instance.controllerCharacter.CharacterProperty.Power;
        int totalCost = formulaCost * produceCount;
        if (totalCost >= nowPower)
        {
            RPCost.SetSWText($"<color=green>{totalCost}</color>/({nowPower})");
        }
        else
        {
            RPCost.SetSWText($"<color=red>{totalCost}</color>/({nowPower})");
        }
    }

    private async void DisplayFormula()
    {
        ClearFormulaItemBoxReferences();
        produceCount = 1;

        bool selectOpenFormula =  selectFormula != null && selectFormula.opened;
        AutoSelect.interactable = selectOpenFormula;

        if (selectOpenFormula)
        {
            for (int i = 0; i < selectFormula.formulaData.StuffItems.Count; i++)
            {
                int itemDataId = selectFormula.formulaData.StuffItems[i].id;
                Item item = new Item
                {
                    instanceId = -1,
                    dataId = itemDataId,
                    count = 1
                };
                await FormulaItemBoxReferences[i].InitData(item, null, FormulaItemBoxGroup);
            }
            //SetOutItemBoxReference(outItem);
            RefreshRPCostAndOut();
        }
        else
        {
            for(int i = 0; i < FormulaItemBoxReferences.Count; i++)
            {
                FormulaItemBoxReferences[i].ClearData();
            }
        }
    }

    //自动选择材料
    private async void AutoSelectMaterials()
    {
        if (selectFormula != null)
        {
            ClearFormulaItemBoxReferences();
            bool successSelect = true;
            produceCount = 1;

            for (int i = 0; i < selectFormula.formulaData.StuffItems.Count; i++)
            {
                int itemDataId = selectFormula.formulaData.StuffItems[i].id;
                int itemCount = PackageManager.instance.GetPlayerItemCount(itemDataId);
                if (itemCount > 0)
                {
                    Item item = new Item
                    {
                        instanceId = 1,
                        dataId = itemDataId,
                        count = 1
                    };
                    await FormulaItemBoxReferences[i].InitData(item, null, FormulaItemBoxGroup);
                }
                else
                {
                    Item item = new Item
                    {
                        instanceId = -1,
                        dataId = itemDataId,
                        count = 1
                    };
                    await FormulaItemBoxReferences[i].InitData(item, null, FormulaItemBoxGroup);
                    successSelect = false;
                }
            }
            int instanceId = 1;
            if (!successSelect)
            {
                instanceId = -1;
                InformationController.instance.AddInformation(LanguageManage.SwitchStr("原料不足!"), true, true);
            }
            outItem = new Item
            {
                instanceId = instanceId,
                dataId = selectFormula.formulaData.ProductItem.id,
                count = 1
            };
            await OutItemBoxReference.InitData(outItem, null, FormulaItemBoxGroup);
            RefreshRPCostAndOut();
        }
    }

    private Formula CheckFormula()
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
        return ManufactureManager.instance.CheckFormula(items); 
    }

    private async void InitDisplay()
    {
        Item defaultItem = default(Item);
        defaultItem.instanceId = -1;
        
        for (int i = 0; i < manufature.materials.Length; i++)
        {
            if (manufature.materials[i].x == 0)
            {
                await FormulaItemBoxReferences[i].InitData(defaultItem, null, FormulaItemBoxGroup);
            }
            else
            {
                Item item = new Item
                {
                    dataId = manufature.materials[i].x,
                    instanceId = manufature.materials[i].y
                };
                await FormulaItemBoxReferences[i].InitData(item, null, FormulaItemBoxGroup);
            }
            FormulaItemBoxReferences[i].SelectUIAction = DisplayItem;
        }
        if (manufature.product.x == 0)
        {
            FormulaDropdown.captionText.text = "";
            FormulaDropdown.interactable = true;
            OutItemBoxReference.ClearData();
        }
        else
        {
            FormulaDropdown.interactable = false;
            Item item = new Item
            {
                dataId = manufature.product.z == 0 ? manufature.product.x : GameCommon.defaultProduct,
                count = manufature.product.y,
                instanceId = 1
            };
          await  OutItemBoxReference.InitData(item, null, FormulaItemBoxGroup);
            OutItemBoxReference.SelectUIAction = DisplayItem; 
            
        }
        //FormulaDropdown.SetValueWithoutNotify()
        //FormulaDropdown.value = 0;
        formulaCost = 0;
        produceCount = 1;
    }

    private Manufature manufature;

    public override Task InitData(string dataKey)
    {
        Manufature manufature = ManufactureManager.instance.GetManufature(int.Parse(dataKey));
        if (manufature!=null)
        { 
            InitData(manufature);
            if (manufactureData.needItem != null && manufactureData.needItem.Count > 0)
            {
                var shortcutPackage = ShortcutManager.instance.playerShortcutPackage;
                bool result = false;
                for(int i = 0; i < manufactureData.needItem.Count; i++)
                {
                    if (shortcutPackage.CheckItem(manufactureData.needItem[i],out int itemInstance))
                    {
                        result = true;
                        break;
                    }
                }
                if (!result)
                {
                    SimpleTalk simpleTalk = new SimpleTalk
                    {
                        characterId = CharacterManager.instance.controllerCharacter.instanceId,
                        talkId = manufactureData.noItemTalk
                    };
                    GameActionManager.instance.QueueAction(simpleTalk);
                    Close();
                    return null;
                }
            }
            // ClearFormulaItemBoxReferences();
        }
        if (SelectItemBoxRefrence != null)
        {
            SelectItemBoxRefrence.ClearSelect();
            InformationObj.transform.localScale = Vector3.zero;
        }
        RefreshRPCostAndOut();
        return base.InitData(dataKey);
    }

    private void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        int timeValue = manufature.waitTime - updateGameTime.totalMinute;
        if (timeValue <= 0)
        {
            timeSlider.fillAmount = 1;

            if (manufature.product.x != 0)
            {
                this.timeValue.SetSWText(LanguageManage.SwitchStr("制作完成！"));
                if (manufature.waitTime > 0)
                {
                    CreatProduct();
                }
            }
            else
            {
                this.timeValue.SetSWText(LanguageManage.SwitchStr("等待制作"));
            }
        }
        else
        {
            timeValue = math.clamp(timeValue, 0, timeValue);
            int totalTime = manufature.waitTime - manufature.startTime;

            timeSlider.fillAmount = (totalTime - timeValue) / (float)totalTime;
            int hour = timeValue / 60;
            int minute = timeValue - hour * 60;
            int data = hour / 24;
            hour = hour - data * 24;
            this.timeValue.text = $"{data}:{hour}:{minute}";
        }
    }

    private async void CreatProduct()
    {
        manufature.waitTime = 0;
       await OutItemBoxReference.InitData(new Item { dataId = manufature.product.x, count = manufature.product.y, instanceId = 1 }, null, FormulaItemBoxGroup);
        outEffect.Play();
        SetManufature setManufature = new SetManufature
        {
            manufature = manufature
        };
        GameActionManager.instance.QueueAction(setManufature, true);
        creatButtonName.SetSWText(LanguageManage.SwitchStr("取出"));
        CreatButton.interactable = true;
    }

    private void RefreshManufature(RefreshManufature refreshManufature)
    {
        if (manufature.instanceId == refreshManufature.manufature.instanceId)
        {
            InitData(refreshManufature.manufature);
        }
    }

    public override void InitReferenceData(Manufature v)
    {
        base.InitReferenceData(v);
        InitData(v);

        bool selectOpenFormula = selectFormula != null && selectFormula.opened;
        AutoSelect.transform.localScale = selectOpenFormula ? Vector3.one : Vector3.zero;
    }

    private async void InitData(Manufature v)
    {
        SelectItemBoxRefrence = null;
        InformationObj.transform.localScale = Vector3.zero;
        outEffect.Stop();
        manufature = v;
        manufactureData = await GameDataManager.instance.GetAsyncData<ManufactureData>(v.dataId);
        title.SetSWText(manufactureData.manufactureName);
        CreatButton.interactable = false;
        AutoSelect.interactable = false;

        List<FormulaTypeData> formulaTypeDatas = new List<FormulaTypeData>();
        HashSet<FormulaType> formulaTypes = new HashSet<FormulaType>();

        allFormulas = new Dictionary<FormulaType, List<Formula>>();
        List<int> typeFormulas = new List<int>();
        var formulas = ManufactureManager.instance.GetManufatureAllFormulas(manufature.instanceId);
        for (int i = 0; i < formulas.Count; i++)
        {
           var formula = formulas[i];
             formulaTypes.Add(formula.formulaData.formulaType);

            if (allFormulas.TryGetValue(formula.formulaData.formulaType, out var _formulas))
            {
                _formulas.Add(formula);
            }
            else
            {
                allFormulas.Add(formula.formulaData.formulaType, new List<Formula> { formula });
            }
        }
        if (formulaTypes.Count > 1)
        {
            foreach (var formulaType in formulaTypes)
            {
                formulaTypeDatas.Add(new FormulaTypeData { formulaType = formulaType });
                nowSelectFormulaTypes.Add(formulaType);
            }
            this.formulaTypes.InitListData(formulaTypeDatas, SelectFormulaTypeData);
        }
        else
        {
            this.formulaTypes.InitListData(new List<FormulaTypeData>(), SelectFormulaTypeData);
        }

        RefreshFormulaSelect();
        InitDisplay();
        if (WorldMapObjManager.instance.GetRuntimeMapItemObj(v.instanceId, out var runtimeObj))
        {
            Vector2 pos = runtimeObj.transform.position;
            pos.x += manufactureData.cameraOffset.x;
            pos.y += manufactureData.cameraOffset.y;
            SetFixedCamera setFixedCamera = new SetFixedCamera
            {
                fixedCamera = true,
                fixedPos = pos,
            };
            GameActionManager.instance.QueueAction(setFixedCamera);
        } 
    }

    public override void Close()
    {
        base.Close();
        SetFixedCamera setFixedCamera = new SetFixedCamera
        {
            fixedCamera = false,
            flowCameraType = FlowCameraType.Default
        };
        GameActionManager.instance.QueueAction(setFixedCamera);
    }

    private void SelectFormulaTypeData(FormulaTypeData formulaTypeData, int index, bool seleted)
    {
        if (!seleted)
        {
            nowSelectFormulaTypes.Remove(formulaTypeData.formulaType);
        }
        else
        {
            nowSelectFormulaTypes.Add(formulaTypeData.formulaType);
        }
        RefreshFormulaSelect();
    }

    private HashSet<FormulaType> nowSelectFormulaTypes = new HashSet<FormulaType>();
    private ItemBoxReference SelectItemBoxRefrence;

    private async void GetOutProduct()
    {
        var allSet = PackageManager.instance.CheckPackageTryItemIn(
            CharacterManager.instance.controllerCharacter.characterPackage, manufature.product.x, manufature.product.y);
        if (!allSet)
        {
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("背包空间不足!"));
        }
        else
        {
            if (matchFormula != null && !matchFormula.opened)
            {
                OpenFormula openFormula = new OpenFormula
                {
                    formulaId = matchFormula.id
                };
                GameActionManager.instance.QueueAction(openFormula);   
                RefreshFormulaSelect();
            }
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(manufature.product.x);
            ItemResultInfo itemResultInfo = new ItemResultInfo
            {
                icon = itemData.icon,
                info0 = $"{LanguageManage.SwitchStr("获得")}+ {LanguageManage.SwitchStr(itemData.itemName)} + x{manufature.product.y}"
            };
            GameNotificationManager.instance.ShowItemResultInfo(itemResultInfo);
            //UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);

            await PackageManager.instance.SetItemInPackage(new Item { dataId = manufature.product.x, count = manufature.product.y }, CharacterManager.instance.controllerCharacter.characterPackage);
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("产物已经放到背包!"));
            ClearManufature clearManufature = new ClearManufature { manufatureId = manufature.instanceId };
            GameActionManager.instance.QueueAction(clearManufature);

            AutoSelect.interactable = true;
            AddButton.interactable = true;
            ReduceButton.interactable = true;
            FormulaDropdown.interactable = true;
            FormulaDropdown.value = 0;
            formulaTypeParent.transform.localScale = Vector3.one;
            creatButtonName.SetSWText(LanguageManage.SwitchStr("制作"));

            if (SelectItemBoxRefrence != null)
            {
                SelectItemBoxRefrence.ClearSelect();
                InformationObj.transform.localScale = Vector3.zero;
            }
            ClearFormulaItemBoxReferences(true);
        }
    }


    async void SetFormulaItem(Item item, int index, bool select)
    {
        item.count = 0;
        await SelectItemBoxRefrence.InitData(item, null, FormulaItemBoxGroup);
        selectActionButtonName.SetSWText(LanguageManage.SwitchStr("移除"));
        selectActionButton.onClick.RemoveAllListeners();
        selectActionButton.onClick.AddListener(ClearFormulaItem);
        UIManager.instance.CloseGamePanel<WarehousePanel>();
        SelectItemBoxRefrence.SelectDefault();
        DisplayItem(SelectItemBoxRefrence, true);
        RefreshRPCostAndOut();
    }
    async void ClearFormulaItem()
    {
        selectActionButtonName.SetSWText(LanguageManage.SwitchStr("放入"));
        selectActionButton.onClick.RemoveAllListeners();
        selectActionButton.onClick.AddListener(() =>
        {
            PackageManager.instance.ShowAllPlayerPackage(SetFormulaItem, LanguageManage.SwitchStr("选择"));
        });

        Item defaultItem = default(Item);
        defaultItem.instanceId = -1;
        await SelectItemBoxRefrence.InitData(defaultItem, null, FormulaItemBoxGroup);
        // DisplayItem(SelectItemBoxRefrence, true);
        SelectItemBoxRefrence.SelectDefault();
        RefreshRPCostAndOut();
    }
    private async void DisplayItem(ItemBoxReference itemBoxReference, bool selected = true)
    {
        if (selected)
        {
            SelectItemBoxRefrence = itemBoxReference;
            var item = itemBoxReference.Item;
            if (item.dataId == 0)
            {
                InformationObj.transform.localScale = Vector3.one;
                selectItemName.enabled = false;
                ItemType.enabled = false;
                itemInfo.enabled = false;
                itemProperty.enabled = false;
                moneyValue.enabled = false;
                ItemIcon.enabled = false;
                InfoItemValueImage.transform.parent.gameObject.SetActive(false);

                selectActionButtonName.SetSWText(LanguageManage.SwitchStr("放入"));
                selectActionButton.onClick.RemoveAllListeners();
                selectActionButton.onClick.AddListener(() =>
                {
                    PackageManager.instance.ShowAllPlayerPackage(SetFormulaItem, LanguageManage.SwitchStr("选择"));
                });
            }
            else
            {
                InformationObj.transform.localScale = Vector3.one;
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
                if (itemData != null)
                {
                    selectItemName.SetSWText(itemData.GetInfo());
                    ItemType.SetSWText(itemData.type.ToString());
                    itemInfo.SetSWText(itemData.GetInfo());
                    itemProperty.SetSWText(itemData.GetProperty());
                    moneyValue.SetSWText(itemData.sellPrice.ToString());
                    ItemIcon.sprite = itemData.icon;

                    selectItemName.enabled = true;
                    ItemType.enabled = true;
                    itemInfo.enabled = true;
                    itemProperty.enabled = true;
                    moneyValue.enabled = true;
                    ItemIcon.enabled = true;
                    InfoItemValueImage.transform.parent.gameObject.SetActive(itemData.itemValue);
                    InfoItemValueImage.fillAmount=item.value;
                }
                else
                {
                    InfoItemValueImage.transform.parent.gameObject.SetActive(false);
                    selectItemName.enabled = false;
                    ItemType.enabled = false;
                    itemInfo.enabled = false;
                    itemProperty.enabled = false;
                    moneyValue.enabled = false;
                    ItemIcon.enabled = false;
                }

                if (manufature.waitTime > GameTimeManager.instance.totalMinute)
                {
                    selectActionButton.transform.localScale = Vector3.zero;
                }
                else if (SelectItemBoxRefrence == OutItemBoxReference)
                {
                    selectActionButton.transform.localScale = Vector3.zero;
                    if (manufature.product.x != 0)
                    {
                        selectActionButton.transform.localScale = Vector3.one;
                        selectActionButtonName.SetSWText(LanguageManage.SwitchStr("取出"));
                        selectActionButton.onClick.RemoveAllListeners();
                        selectActionButton.onClick.AddListener(GetOutProduct);
                    }
                }else if (SelectItemBoxRefrence == CostItemBoxReference)
                {
                    selectActionButton.transform.localScale = Vector3.zero;
                }
                else
                {
                    selectActionButton.transform.localScale = manufature.product.x == 0 ? Vector3.one : Vector3.zero;
                    if (item.instanceId == -1)
                    {
                        selectActionButtonName.SetSWText(LanguageManage.SwitchStr("放入"));
                        selectActionButton.onClick.RemoveAllListeners();
                        selectActionButton.onClick.AddListener(() =>
                        {
                            PackageManager.instance.ShowAllPlayerPackage(SetFormulaItem, LanguageManage.SwitchStr("选择"));
                        });
                    }
                    else
                    {
                        selectActionButtonName.SetSWText(LanguageManage.SwitchStr("移除"));
                        selectActionButton.onClick.RemoveAllListeners();
                        selectActionButton.onClick.AddListener(ClearFormulaItem);
                    }
                    RefreshRPCostAndOut(); 
                } 
            }
        }
        else if (SelectItemBoxRefrence == itemBoxReference)
        {
            SelectItemBoxRefrence = null;
            InformationObj.transform.localScale = Vector3.zero;
        }
    }
}