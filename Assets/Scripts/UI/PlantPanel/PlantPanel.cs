using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private TextMeshProUGUI PlantName, desc;
    [SerializeField]
    TextMeshProUGUI GoodSeason, BadSeason;
    [SerializeField]
    TextMeshProUGUI price,sellValue;

    private DisplayList<PlantReference, PlantData> leftList, rightList;

    [SerializeField]
    private Button nextButton, frontButton;

    [SerializeField]
    private Animator BookPaper;

    [SerializeField]
    private ToggleGroup toggleGroup;

    [SerializeField]
    private Transform leftParent, rightParent;

    [SerializeField]
    private PlantReference plantUIReference;

    protected override void Awake()
    {
        base.Awake();
        nextButton.onClick.AddListener(() =>
        {
            displayIndex += 2;
            if (displayIndex > maxIndex - 2)
            {
                displayIndex = maxIndex - 2;
            }
            InitButton();
            DelyDisplayPlantes(true);
        });
        frontButton.onClick.AddListener(() =>
        {
            displayIndex -= 2;
            if (displayIndex < 0)
            {
                displayIndex = 0;
            }
            InitButton();
            DelyDisplayPlantes(false);
        });
        closeButton.onClick.AddListener(Close);

        leftList = new DisplayList<PlantReference, PlantData>(plantUIReference, leftParent);
        rightList = new DisplayList<PlantReference, PlantData>(plantUIReference, rightParent);
    }

    private List<PlantData> allPlantData;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        closeButton = FindChildGameObject<Button>("Close");
        PlantName = FindChildGameObject<TextMeshProUGUI>("PlantName");
        desc = FindChildGameObject<TextMeshProUGUI>("Info");
        plantUIReference = FindChildGameObject<PlantReference>("PlantReference");
        toggleGroup = GetComponent<ToggleGroup>();
        leftParent = FindChildGameObject("List0");
        rightParent = FindChildGameObject("List1");

        nextButton = FindChildGameObject<Button>("Next");
        frontButton = FindChildGameObject<Button>("Front");
        BookPaper = FindChildGameObject<Animator>("Book2p");

        price = FindChildGameObject<TextMeshProUGUI>("PriceValue");
        GoodSeason = FindChildGameObject<TextMeshProUGUI>("GoodSeasonValue");
        BadSeason = FindChildGameObject<TextMeshProUGUI>("BadSeasonValue");
        sellValue = FindChildGameObject<TextMeshProUGUI>("SellValue");
    }

    private int displayIndex = 0;
    private int maxIndex = 0;

    private void InitButton()
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

    public override async Task InitData(string dataKey)
    {
        await base.InitData(dataKey);

        if (allPlantData == null || allPlantData.Count > 0)
            allPlantData = await GameDataManager.instance.GetAllAsyncData<PlantData>();

        nextButton.transform.localScale = Vector3.one;
        frontButton.transform.localScale = Vector3.one;
        BookPaper.gameObject.SetActive(false);

        displayIndex = 0;
        maxIndex = allPlantData.Count / 8 + 1;

        DisplayPlants();
    }

    private void DelyDisplayPlantes(bool next)
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
        GameTimerController.instance.DelayAction(820, DisplayPlants);
    }

    private void DisplayPlants()
    {
        leftParent.localScale = Vector3.one;
        rightParent.localScale = Vector3.one;
        nextButton.transform.localScale = Vector3.one;
        frontButton.transform.localScale = Vector3.one;
        BookPaper.gameObject.SetActive(false);
        List<PlantData> leftPlantDatas = new List<PlantData>();
        for (int i = 0; i < 8; i++)
        {
            int index = i + displayIndex * 8;
            if (index < allPlantData.Count)
            {
                leftPlantDatas.Add(allPlantData[index]);
            }
        }
        leftList.InitListData(leftPlantDatas, SelectPlantReference, toggleGroup);

        List<PlantData> rightPlantDatas = new List<PlantData>();
        for (int i = 0; i < 8; i++)
        {
            int index = i + displayIndex * 8 + 8;
            if (index < allPlantData.Count)
            {
                rightPlantDatas.Add(allPlantData[index]);
            }
        }
        rightList.InitListData(rightPlantDatas, SelectPlantReference, toggleGroup);
        leftList.SelectDefault();
    }
     

    private async void SelectPlantReference(PlantData plantData, bool selected)
    {
        if (selected)
        {
            PlantName.SetSWText(plantData.plantName);
            ItemData seedData = await GameDataManager.instance.GetAsyncData<ItemData>(plantData.seed);
            desc.SetSWText(seedData.info);

            string _seasonStr = "";
            for(int i=0;i<plantData.goodSeason.Count;i++)
            {
                _seasonStr =$"{_seasonStr}  {LanguageManage.SwitchStr((Season)plantData.goodSeason[i])}"; 
            }
            string _seasonStr1 = "";
            for (int i = 0; i < plantData.badSeason.Count; i++)
            {
                _seasonStr1 = $"{_seasonStr1}  {LanguageManage.SwitchStr((Season)plantData.badSeason[i])}";
            }
            GoodSeason.text=(_seasonStr);
            BadSeason.text = (_seasonStr1);
            sellValue.text = plantData.openLevel.ToString();

            ItemData product = await GameDataManager.instance.GetAsyncData<ItemData>(plantData.fruit);
            price.text = product.sellPrice.ToString();
        }
    }
}