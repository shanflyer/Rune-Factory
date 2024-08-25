using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Button closeButton;
    [SerializeField]
    TextMeshProUGUI fishName,record,desc;
    [SerializeField]
    TextMeshProUGUI season, place;
    DisplayList<FishUIReference,FishReferenceData> leftFishList, rightFishList;
    [SerializeField]
    Button nextButton, frontButton;
    [SerializeField]
    Animator BookPaper;
    [SerializeField]
    ToggleGroup toggleGroup;
    [SerializeField]
    Transform leftParent,rightParent;
    [SerializeField]
    FishUIReference fishUIReference;


    protected override async void Awake()
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
            DelyDisplayFishes(true);
        });
        frontButton.onClick.AddListener(() =>
        {
            displayIndex -= 2;
            if (displayIndex < 0)
            {
                displayIndex = 0; 
            } 
            InitButton();
            DelyDisplayFishes(false);
        });
        closeButton.onClick.AddListener(Close);

        leftFishList = new DisplayList<FishUIReference, FishReferenceData>(fishUIReference, leftParent);
        rightFishList = new DisplayList<FishUIReference, FishReferenceData>(fishUIReference, rightParent);

       
    }
    List<FishData> allFishes;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        closeButton = FindChildGameObject<Button>("Close");
        fishName = FindChildGameObject<TextMeshProUGUI>("FishName");
        record = FindChildGameObject<TextMeshProUGUI>("MaxValue");
        desc = FindChildGameObject<TextMeshProUGUI>("Info");
        season = FindChildGameObject<TextMeshProUGUI>("SeasonValue");
        place = FindChildGameObject<TextMeshProUGUI>("PlaceValue");
        fishUIReference = FindChildGameObject<FishUIReference>("FishReference");
        toggleGroup = GetComponent<ToggleGroup>();
        leftParent = FindChildGameObject("FishList0");
        rightParent = FindChildGameObject("FishList1");

        nextButton = FindChildGameObject<Button>("Next");
        frontButton = FindChildGameObject<Button>("Front");
        BookPaper = FindChildGameObject<Animator>("Book2p");

    }
    List<FishReferenceData> fishReferenceDatas = new List<FishReferenceData>();
    int displayIndex = 0;
    int maxIndex = 0;

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
    public override async Task InitData(string dataKey)
    {
        base.InitData(dataKey);

        if (allFishes==null||allFishes.Count > 0)
            allFishes = await GameDataManager.instance.GetAllAsyncData<FishData>();


        nextButton.transform.localScale = Vector3.one;
        frontButton.transform.localScale = Vector3.one;
        BookPaper.gameObject.SetActive(false);
        
        fishReferenceDatas.Clear();
        displayIndex = 0;
        maxIndex = allFishes.Count / 8 + 1;
        for (int i = 0; i < allFishes.Count; i++)
        {
            var fishData = allFishes[i];
            var FishSaveData = GameDataSaveManager.instance.GetFishDataSave(fishData.id);
            if (FishSaveData != null)
            {
                FishReferenceData fishReferenceData = new FishReferenceData
                {
                    dataId = fishData.id,
                    seasons = fishData.seasons,
                    record = FishSaveData.length,
                    places = FishSaveData.places
                };
                fishReferenceDatas.Add(fishReferenceData);
            }else
            {
                FishReferenceData fishReferenceData = new FishReferenceData
                {
                    dataId = fishData.id,
                    seasons = fishData.seasons, 
                };
                fishReferenceDatas.Add(fishReferenceData);
            }
           
        }
        DisplayFishes();
    } 
    void DelyDisplayFishes(bool next)
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
        GameTimerController.instance.DelayAction(820, DisplayFishes);
    }
    void DisplayFishes()
    {
        leftParent.localScale = Vector3.one;
        rightParent.localScale = Vector3.one;
        nextButton.transform.localScale = Vector3.one;
        frontButton.transform.localScale = Vector3.one;
        BookPaper.gameObject.SetActive(false);
        List<FishReferenceData> leftFishReferenceDatas = new List<FishReferenceData>();
        for(int i = 0; i < 8; i++)
        {
            int index = i + displayIndex * 8;
            if (index < fishReferenceDatas.Count)
            {
                leftFishReferenceDatas.Add(fishReferenceDatas[index]);
            }            
        }
        leftFishList.InitListData(leftFishReferenceDatas, SelectFishReference, toggleGroup);

        List<FishReferenceData> rightFishReferenceDatas = new List<FishReferenceData>();
        for (int i = 0; i < 8; i++)
        {
            int index = i + displayIndex * 8+8;
            if (index < fishReferenceDatas.Count)
            {
                rightFishReferenceDatas.Add(fishReferenceDatas[index]);
            }
        }
        rightFishList.InitListData(rightFishReferenceDatas, SelectFishReference, toggleGroup);
    }

    int selectFishDataId;
    async void SelectFishReference(FishReferenceData fishReferenceData,bool selected)
    {
        if (selected)
        {
            if (fishReferenceData.record != 0)
            {
                FishData fishData = await GameDataManager.instance.GetAsyncData<FishData>(fishReferenceData.dataId);
                fishName.text = fishData.fishName;
                desc.text = fishData.info;
                record.text = $"{fishReferenceData.record}cm";
                string seasonStr = "";
                for(int i = 0; i < fishReferenceData.seasons.Count; i++)
                {
                    if (i > 0)
                    {
                        seasonStr = $"{seasonStr}¡¢";
                    }
                    seasonStr = $"{seasonStr}{(Season)(fishReferenceData.seasons[i])}";
                }
                string placeStr = "";
                for(int i = 0; i < fishData.places.Count; i++)
                {
                    int place = fishData.places[i];
                    bool find = fishReferenceData.places.Contains(place); 
                    var roomName = find?WorldMapManager.instance.GetWorldMap(place).mapRoomData.name :"???";
                    if (i > 0)
                    {
                        placeStr = $"{placeStr}¡¢";
                    }
                    placeStr = $"{placeStr}{roomName}";
                }
                season.text = seasonStr;
                place.text = placeStr;
            }
            else
            {
                fishName.text = "???";
                desc.text = "???";
                record.text = "???";
                season.text = "???";
                place.text = "????";
            }
            

        }else if (selectFishDataId == fishReferenceData.dataId)
        {

        }
    }
}
