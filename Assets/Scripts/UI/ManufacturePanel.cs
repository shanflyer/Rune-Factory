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

        formulaTypeReference = FindChildGameObject<FormulaTypeReference>("FormulaTypeReference");
        formulaTypeParent = FindChildGameObject("TypeList");

        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        FormulaDropdown = FindChildGameObject<TMP_Dropdown>("FormulaDropdown");

        title = FindChildGameObject<TextMeshProUGUI>("Title");

        CostItemBoxReference = FindChildGameObject<ItemBoxReference>("CostItem");
        OutItemBoxReference = FindChildGameObject<ItemBoxReference>("OutputItemBoxReference");
        ReduceButton = FindChildGameObject<Button>("ReduceButton");
        AddButton = FindChildGameObject<Button>("AddButton");
        ItemCountValue = FindChildGameObject<TextMeshProUGUI>("ItemCountValue");
        CreatButton = FindChildGameObject<Button>("CreatButton");
        creatButtonName = CreatButton.gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
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
        InfoItemValueImage = FindChildGameObject<Image>("InfoItemValue");

        timeSlider = FindChildGameObject<Image>("TimeSlider");
        timeValue = FindChildGameObject<TextMeshProUGUI>("TimeValue");
        outEffect = FindChildGameObject<ParticleSystem>("OutEffect");
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
                GameNotificationManager.instance.DisplayTips("生产", "材料不足");
                // InformationController.instance.AddInformation("材料不足",true,true);
                produceCount--;
            }
        });

        FormulaDropdown.onValueChanged.AddListener(async (int index) =>
        {
           await  SelectFormulaAsync(index);
        });

        AutoSelect.onClick.AddListener(AutoSelectMaterials);
        CreatButton.onClick.AddListener(CreatItem);
        ReturnButton.onClick.AddListener(Close);
    }

    async Task SelectFormulaAsync(int index)
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
            selectFormulaData = await GameDataManager.instance.GetAsyncData<FormulaData>(seleciId);
        }

        DisplayFormula();
    }
    private Dictionary<FormulaType, List<FormulaData>> allFormulaDatas;
    private Formula selectFormula;
    private FormulaData selectFormulaData;
    private ManufactureData manufactureData;
    private FormulaData matchFormula;
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
            _produceCount = value;
            ItemCountValue.text = produceCount.ToString();
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
            GameManager.instance.ShowTwoSelectAction("", noticeStr, () =>
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
                creatButtonName.text = "制作";
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
                else if (selectFormulaData != null && outItem.dataId == selectFormulaData.Product)
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
                            manufature.matchFormula = matchFormula.id;
                            productId = matchFormula.Product;
                            if (manufature.formulas.TryGetValue(matchFormula.id, out var formula))
                            {
                                if (!formula.isOpen)
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
                        manufature.waitTime = GameTimeManager.instance.totalMinute + matchFormula.produceTime;
                    }
                    if (matchFormula != null)
                    {
                        manufature.matchFormula = matchFormula.id;
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
                    creatButtonName.text = "中止";
                }
                GameManager.instance.ShowTwoSelectAction("", noticeStr, CreatAction, null);
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
            foreach (var formulaDatas in allFormulaDatas)
            {
                for (int i = 0; i < formulaDatas.Value.Count; i++)
                {
                    FormulaData formulaData = formulaDatas.Value[i];
                    Formula formula = manufature.formulas[formulaData.id];
                    if (!formula.isOpen)
                    {
                        continue;
                    }
                    FormulaOptionData formulaOptionData = new FormulaOptionData
                    {
                        formulaId = formula.id,
                        open = formula.isOpen,
                        text = formulaData.formulaName
                    };
                    formulaOptionDatas.Add(formulaOptionData);
                }
            }
        }
        else
        {
            foreach (var type in nowSelectFormulaTypes)
            {
                if (allFormulaDatas.TryGetValue(type, out var formulaDatas))
                {
                    for (int i = 0; i < formulaDatas.Count; i++)
                    {
                        FormulaData formulaData = formulaDatas[i];
                        Formula formula = manufature.formulas[formulaData.id];
                        if (!formula.isOpen)
                        {
                            continue;
                        }
                        FormulaOptionData formulaOptionData = new FormulaOptionData
                        {
                            formulaId = formula.id,
                            open = formula.isOpen,
                            text = formulaData.formulaName
                        };
                        formulaOptionDatas.Add(formulaOptionData);
                    }
                }
            }
        }

        FormulaDropdown.options = formulaOptionDatas;
        await SelectFormulaAsync(0);
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
            if (manufature.formulas.TryGetValue(matchFormula.id, out var formula))
            {
                if (formula.isOpen)
                {
                    instanceId = formulaCount >= matchFormula.Stuffs.Count ? 1 : -1;
                    product = matchFormula.Product;
                    formulaCost = matchFormula.PowerCost; 
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
            if (selectFormula.id != 0 && selectFormula.isOpen)
            {
                product = selectFormulaData.Product;
            }
            if (product != GameCommon.defaultProduct)
            {
                bool match = true;
                bool isSetMatch = true;
                foreach (var id in selectFormulaData.Stuffs)
                {
                    var reference = FormulaItemBoxReferences.Find(f => f.Item.dataId == id);
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

                formulaCost = selectFormulaData.PowerCost;
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
        creatButtonName.text = "制作";

        if (manufature.product.x != 0)
        {
            CreatButton.interactable = true;
            if (manufature.waitTime <= GameTimeManager.instance.totalMinute)
            {
                creatButtonName.text = "取出";
            }
            else
            {
                creatButtonName.text = "中止";
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
            RPCost.text = $"<color=green>{totalCost}</color>/({nowPower})";
        }
        else
        {
            RPCost.text = $"<color=red>{totalCost}</color>/({nowPower})";
        }
    }

    private async void DisplayFormula()
    {
        ClearFormulaItemBoxReferences();
        produceCount = 1;

        bool selectOpenFormula = selectFormula.id != 0 && selectFormula.isOpen;
        AutoSelect.interactable = selectOpenFormula;

        if (selectFormula.id != 0)
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
                await FormulaItemBoxReferences[i].InitData(item, null, FormulaItemBoxGroup);
            }
            //SetOutItemBoxReference(outItem);
            RefreshRPCostAndOut();
        }
    }

    //自动选择材料
    private async void AutoSelectMaterials()
    {
        if (selectFormulaData != null)
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
                InformationController.instance.AddInformation("原料不足!", true, true);
            }
            outItem = new Item
            {
                instanceId = instanceId,
                dataId = selectFormulaData.Product,
                count = 1
            };
            await OutItemBoxReference.InitData(outItem, null, FormulaItemBoxGroup);
            RefreshRPCostAndOut();
        }
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

        foreach (var formulaList in allFormulaDatas)
        {
            for (int i = 0; i < formulaList.Value.Count; i++)
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
            OutItemBoxReference.ClearData();
        }
        else
        {
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
        Manufature manufature = ManufatureManager.instance.GetManufature(int.Parse(dataKey));
        if (manufature!=null)
        {
            InitData(manufature);
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
                this.timeValue.text = "制作完成！";
                if (manufature.waitTime > 0)
                {
                    CreatProduct();
                }
            }
            else
            {
                this.timeValue.text = "等待制作";
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
        creatButtonName.text = "取出";
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

        bool selectOpenFormula = selectFormula.id != 0 && selectFormula.isOpen;
        AutoSelect.transform.localScale = selectOpenFormula ? Vector3.one : Vector3.zero;
    }

    private async void InitData(Manufature v)
    {
        outEffect.Stop();
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
            this.formulaTypes.InitListData(null, SelectFormulaTypeData);
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

    private void SelectFormulaTypeData(FormulaTypeData formulaTypeData, bool seleted)
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
        bool allSet = await PackageManager.instance.CheckPackageTryItemIn(CharacterManager.instance.controllerCharacter.characterPackage, manufature.product.x, manufature.product.y);
        if (!allSet)
        {
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("背包空间不足!"));
        }
        else
        {
            if (matchFormula != null && manufature.formulas.TryGetValue(matchFormula.id, out var formula) && !formula.isOpen)
            {
                ManufatureManager.instance.OpenFormula(manufature.instanceId, formula.id);
                GameNotificationManager.instance.DisplayTips("新配方获得!", $"发现了制作<color=blue>{matchFormula.formulaName}</color>的配方");
                RefreshFormulaSelect();
            }

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
            creatButtonName.text = "制作";

            if (SelectItemBoxRefrence != null)
            {
                SelectItemBoxRefrence.ClearSelect();
                InformationObj.transform.localScale = Vector3.zero;
            }
            ClearFormulaItemBoxReferences(true);
        }
    }


    async void SetFormulaItem(Item item, bool select)
    {
        item.count = 0;
        await SelectItemBoxRefrence.InitData(item, null, FormulaItemBoxGroup);
        selectActionButtonName.text = "移除";
        selectActionButton.onClick.RemoveAllListeners();
        selectActionButton.onClick.AddListener(ClearFormulaItem);
        UIManager.instance.CloseGamePanel<WarehousePanel>();
        SelectItemBoxRefrence.SelectDefault();
        DisplayItem(SelectItemBoxRefrence, true);
        RefreshRPCostAndOut();
    }
    async void ClearFormulaItem()
    {
        selectActionButtonName.text = "放入";
        selectActionButton.onClick.RemoveAllListeners();
        selectActionButton.onClick.AddListener(() =>
        {
            PackageManager.instance.ShowAllPlayerPackage(SetFormulaItem, "选择");
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

                selectActionButtonName.text = "放入";
                selectActionButton.onClick.RemoveAllListeners();
                selectActionButton.onClick.AddListener(() =>
                {
                    PackageManager.instance.ShowAllPlayerPackage(SetFormulaItem, "选择");
                });
            }
            else
            {
                InformationObj.transform.localScale = Vector3.one;
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
                if (itemData != null)
                {
                    selectItemName.text = itemData.itemName;
                    ItemType.text = itemData.type.ToString();
                    itemInfo.text = itemData.info;
                    itemProperty.text = itemData.GetProperty();
                    moneyValue.text = itemData.sellPrice.ToString();
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
                        selectActionButtonName.text = "取出";
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
                        selectActionButtonName.text = "放入";
                        selectActionButton.onClick.RemoveAllListeners();
                        selectActionButton.onClick.AddListener(() =>
                        {
                            PackageManager.instance.ShowAllPlayerPackage(SetFormulaItem, "选择");
                        });
                    }
                    else
                    {
                        selectActionButtonName.text = "移除";
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