using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FormulaPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Button closeBtn;
    [SerializeField]
    Transform tagParent; 
    [SerializeField]
    FormulaReference formulaReference; 
    [SerializeField]
    FormulaTagReference formulaTagReference;
    [SerializeField]
    Button nextButton, frontButton;
    [SerializeField]
    Animator BookPaper;
    [SerializeField]
    ToggleGroup toggleGroup;
    [SerializeField]
    Transform leftParent, rightParent;
    [SerializeField]
    TextMeshProUGUI formulaNameText, formulaInfoText, formulaTypeText,formulaMaterialText;

    DisplayList<FormulaReference, FormulaReferenceData> leftFormulaList, rightFormulaList;
    DisplayList<FormulaTagReference, FormulaType> formulaTagList;

    Dictionary<FormulaType, List<FormulaData>> formulaDataDic;
    List<FormulaType> formulaTypes;
    protected override void Awake()
    {
        base.Awake();

        formulaTagList = new DisplayList<FormulaTagReference, FormulaType>(formulaTagReference, tagParent);
        leftFormulaList = new DisplayList<FormulaReference, FormulaReferenceData>(formulaReference, leftParent);
        rightFormulaList = new DisplayList<FormulaReference, FormulaReferenceData>(formulaReference, rightParent);

        closeBtn.onClick.AddListener(Close);
        nextButton.onClick.AddListener(() =>
        {
            displayIndex += 2;
            if (displayIndex > maxIndex - 2)
            {
                displayIndex = maxIndex - 2;
            }
            InitButton();
            DelayDisplayFormulas(true);
        });
        frontButton.onClick.AddListener(() =>
        {
            displayIndex -= 2;
            if (displayIndex < 0)
            {
                displayIndex = 0;
            }
            InitButton();
            DelayDisplayFormulas(false);
        });

        
    }
    void DelayDisplayFormulas(bool next)
    {
        leftParent.localScale = Vector3.zero;
        rightParent.localScale = Vector3.zero;
        nextButton.transform.localScale = Vector3.zero;
        frontButton.transform.localScale = Vector3.zero;
        if (next)
        {
            BookPaper.transform.localScale = new Vector3(-400, 640, 400);
        }
        else
        {
            BookPaper.transform.localScale = new Vector3(400, 640, 400);
        }
        BookPaper.gameObject.SetActive(true);
        BookPaper.Play("Paper");
        GameTimerController.instance.DelayAction(820, DisplayFormulas);
    }
    void DisplayFormulas()
    {
        leftParent.localScale = Vector3.one;
        rightParent.localScale = Vector3.one;
        nextButton.transform.localScale = Vector3.one;
        frontButton.transform.localScale = Vector3.one;
        BookPaper.gameObject.SetActive(false);

       
        InitButton(); 
        List<FormulaReferenceData> leftFormulaReferenceDatas = new List<FormulaReferenceData>();
        for (int i = 0; i < 8; i++)
        {
            int index = i + displayIndex * 8;
            if (index < formulaReferenceDatas.Count)
            {
                leftFormulaReferenceDatas.Add(formulaReferenceDatas[index]);
            }
        }
        leftFormulaList.InitListData(leftFormulaReferenceDatas, SelectFormulaData, toggleGroup);

        List<FormulaReferenceData> rightFormulaReferenceDatas = new List<FormulaReferenceData>();
        for (int i = 0; i < 8; i++)
        {
            int index = i + displayIndex * 8 + 8;
            if (index < formulaReferenceDatas.Count)
            {
                rightFormulaReferenceDatas.Add(formulaReferenceDatas[index]);
            }
        }
        rightFormulaList.InitListData(rightFormulaReferenceDatas, SelectFormulaData, toggleGroup);
        leftFormulaList.SelectDefault();
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        closeBtn = FindChildGameObject<Button>("Close");

        formulaTagReference = FindChildGameObject<FormulaTagReference>("Tag");

        formulaReference = FindChildGameObject<FormulaReference>("FormulaReference");
        toggleGroup = GetComponent<ToggleGroup>();
        leftParent = FindChildGameObject("List0");
        rightParent = FindChildGameObject("List1");
        tagParent = FindChildGameObject("TagList");

        formulaNameText = FindChildGameObject<TextMeshProUGUI>("FormulaName");
        formulaInfoText = FindChildGameObject<TextMeshProUGUI>("Info");
        formulaTypeText = FindChildGameObject<TextMeshProUGUI>("TypeValue");
        formulaMaterialText = FindChildGameObject<TextMeshProUGUI>("Material");

        nextButton = FindChildGameObject<Button>("Next");
        frontButton = FindChildGameObject<Button>("Front");
        BookPaper = FindChildGameObject<Animator>("Book2p");
    }
    public override void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v);
    }
    void SelectFormulaData(FormulaReferenceData formulaReferenceData, int index, bool selected)
    {
        if (selected)
        {
            if (formulaReferenceData.open)
            {
                formulaNameText.SetSWText(formulaReferenceData.formulaData.formulaName);
                formulaTypeText.SetSWText(formulaReferenceData.formulaData.formulaType.ToString());
                List<string> formulaMats = new List<string>();
                for(int i = 0; i < formulaReferenceData.formulaData.StuffItems.Count; i++)
                {
                    ItemData itemData = formulaReferenceData.formulaData.StuffItems[i]; 
                    formulaMats.Add(itemData.itemName);
                    if(i< formulaReferenceData.formulaData.StuffItems.Count - 1)
                    {
                        formulaMats.Add(",");
                    }
                }
                formulaMaterialText.SetADDText("需要材料:", formulaMats); 
                formulaInfoText.SetSWText(formulaReferenceData.formulaData.ProductItem.GetInfo());
            }
            else
            {
                formulaNameText.text = "????";
                formulaInfoText.text = "??????????????????";
                formulaTypeText.text = "????";
                formulaMaterialText.SetADDText("需要材料:", "??????");
            }
        }
    }

    int displayIndex = 0;
    int maxIndex = 0;
    FormulaType formulaType;
    List<FormulaReferenceData> formulaReferenceDatas;
    void InitButton()
    {
        if (displayIndex >= maxIndex - 2)
        {
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
        }
        if (displayIndex <= 0)
        {
            frontButton.gameObject.SetActive(false);
        }
        else
        {
            frontButton.gameObject.SetActive(true);
        }
    }
    async void SelectFormulaType(FormulaType formulaType, int index, bool select)
    {
        if (select)
        {
            this.formulaType = formulaType;
            List<FormulaData> formulaDatas = formulaDataDic[formulaType];
            formulaReferenceDatas = new List<FormulaReferenceData>();
            for (int i = 0; i < formulaDatas.Count; i++)
            {
                FormulaReferenceData formulaReferenceData = new FormulaReferenceData
                {
                    formulaData = formulaDatas[i],
                    open = ManufactureManager.instance.IsFormulaOpened(formulaDatas[i].id)
                };
                formulaReferenceDatas.Add(formulaReferenceData);
            }


            displayIndex = 0;
            maxIndex = formulaDatas.Count / 8 + 1;
            DisplayFormulas();
            leftFormulaList.SelectDefault();
        }
    }
    public override async Task InitData(string dataKey)
    {
        if (formulaDataDic == null)
        {
            formulaDataDic = new Dictionary<FormulaType, List<FormulaData>>();
            formulaTypes = new List<FormulaType>();
            var allFormula = await GameDataManager.instance.GetAllAsyncData<FormulaData>();
            for (int i = 0; i < allFormula.Count; i++)
            {
                if (!formulaDataDic.TryGetValue(allFormula[i].formulaType, out var formulaDatas))
                {
                    formulaDatas = new List<FormulaData>();
                    formulaTypes.Add(allFormula[i].formulaType);
                    formulaDataDic.Add(allFormula[i].formulaType,formulaDatas);
                }
                formulaDatas.Add(allFormula[i]);
            }
        }
       await formulaTagList.InitListData(formulaTypes, SelectFormulaType);
        formulaTagList.SelectDefault();
       
    }
}
