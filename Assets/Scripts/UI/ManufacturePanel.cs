using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ManufacturePanel : GamePanel<ManufactureData>
{
    [SerializeField]
    private Text title;

    [SerializeField]
    private Button ReturnButton;

    [SerializeField]
    private Dropdown FormulaDropdown;

    [SerializeField]
    private List<ItemBoxReference> FormulaItemBoxReferences;

    [SerializeField]
    private ItemBoxReference OutItemBoxReference;

    [SerializeField]
    private Button ReduceButton, AddButton;

    [SerializeField]
    private Text ItemCountValue;

    [SerializeField]
    private Button CreatButton;

    [SerializeField]
    private Button AutoSelect;

    [SerializeField]
    private Text RPCost;

    [SerializeField]
    private Text ItemType;

    [SerializeField]
    private Text selectItemName;

    [SerializeField]
    private Button selectActionButton;

    [SerializeField]
    private Text selectActionButtonName;

    [SerializeField]
    private Button selectBoxInfoButton;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        FormulaDropdown = FindChildGameObject<Dropdown>("FormulaDropdown");
        OutItemBoxReference = FindChildGameObject<ItemBoxReference>("OutputItemBoxReference");
        ReduceButton = FindChildGameObject<Button>("ReduceButton");
        AddButton = FindChildGameObject<Button>("AddButton");
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

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
        {
            var formulaItemBoxRefrence = FormulaItemBoxReferences[i];
            SetFormulaItemBoxData(formulaItemBoxRefrence);
        }

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

            DisplayDefaultOutItem();
        }
        else
        {
            formulaCost = matchFormula.PowerCost;

            Item outItem = new Item
            {
                instanceId = 1,
                dataId = matchFormula.Product,
                count = 1
            };
            SetOutItemBoxReference(outItem);
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
            if (!successSelect)
            {
                InformationController.instance.AddInformation("原料不足!");
            }

            RefreshRPCostAndOut();
        }
    }

    private void SetOutItemBoxReference(Item item)
    {
        OutItemBoxReference.InitData(item, (Item item) =>
        {
            SelectAction = null; DisplayItem(item);
        });
    }

    private void DisplayDefaultOutItem()
    {
        Item item = new Item
        {
            instanceId = -1,
            dataId = GameCommon.DefaultOutItemId,
            count = produceCount
        };
        SetOutItemBoxReference(item);
    }

    private FormulaData CheckFormula()
    {
        List<int> items = new List<int>();
        for (int i = 0; i < FormulaItemBoxReferences.Count; i++)
        {
            var reference = FormulaItemBoxReferences[i];
            if (reference.Item.instanceId != 0)
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
    public override async void InitReferenceData(ManufactureData v)
    {
        base.InitReferenceData(v);
        manufactureData = v;
        title.text = manufactureData.manufactureName;
       

        var formulaDatas = await GameDataManager.instance.GetAllAsyncData<FormulaData>();
        allFormulaDatas = new List<FormulaData>();
        for (int i = 0; i < formulaDatas.Count; i++)
        {
            var formulaData = formulaDatas[i];
            if (manufactureData.formulaTypes.Contains(formulaData.formulaType))
            {
                allFormulaDatas.Add(formulaData);
            }
        }

        RefreshFormulaSelect(); 
        InitDisplay();
    }

    //ItemBoxReference SelectItemBoxRefrence;

    private void SetFormulaItemBoxData(ItemBoxReference formulaItemBoxRefrence)
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
                        RefreshRPCostAndOut();
                    };
                });
            }
            RefreshRPCostAndOut();
        });
    }

    private Item selectItem;

    private SelectAction<Item> SelectAction;
    private string SelectActionName;

    private async void ClickItemInfoAction()
    {
        if (selectItem.instanceId != 0)
        {
            var panel = await UIManager.instance.ShowGamePanel<ItemInfoPanel, Item>(selectItem);
            if (panel != null && SelectAction != null)
            {
                panel.SetAction(SelectAction, SelectActionName);
            }
        }
    }

    private async void DisplayItem(Item item)
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

            selectActionButton.gameObject.SetActive(SelectAction != null);
        }
    }
}