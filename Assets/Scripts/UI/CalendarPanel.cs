using System.Collections;
using System.Collections.Generic; 
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalendarPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Button nextMonthButton, forwardMonthButton;
    [SerializeField]
    private ToggleGroup DatesParent;
    [SerializeField]
    private DateReference dateReference;
    [SerializeField]
    private TextMeshProUGUI DataTimeText;
    [SerializeField]
    private TextMeshProUGUI festivaltext;
    [SerializeField]
    private Button returnButton;
    [SerializeField]
    Animator BookPaper;
    private int date;
    private int year;
    private Season season;
    private DisplayList<DateReference, GameDate> dateReferences;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        nextMonthButton = FindChildGameObject<Button>("NextButton");
        forwardMonthButton = FindChildGameObject<Button>("ForwardButton");
        DatesParent = FindChildGameObject<ToggleGroup>("DatesParent");
        dateReference = FindChildGameObject<DateReference>("DateReference");
        returnButton = FindChildGameObject<Button>("ReturnButton");
        DataTimeText = FindChildGameObject<TextMeshProUGUI>("time");
        festivaltext = FindChildGameObject<TextMeshProUGUI>("festival");
        BookPaper = FindChildGameObject<Animator>("Book2p");
    }
    protected override void Awake()
    {
        base.Awake();

        dateReferences = new DisplayList<DateReference, GameDate>(dateReference, DatesParent.transform);
        returnButton.onClick.AddListener(() =>
        { 
            Close();
        });

        nextMonthButton.onClick.AddListener(NextMonth);
        forwardMonthButton.onClick.AddListener(ForwardMonth);
    }
    public override Task InitData(string dataKay)
    {
        BookPaper.gameObject.SetActive(false);
        year = GameTimeManager.instance.Year;
        season = GameTimeManager.instance.Season;
        GameTimeManager.instance.StopTimeRun();
        CreatSeason(season);
        if (year <= 1 && season == Season.春)
        {
            forwardMonthButton.gameObject.SetActive(false);
        }
        else
        {
            forwardMonthButton.gameObject.SetActive(true);
        } 
        
        return base.InitData(dataKay);
    }
    
    void AfterDisplay()
    {
        date = GameTimeManager.instance.Day;
        DataTimeText.text = year + LanguageManage.SwitchStr("年") + " " + LanguageManage.SwitchStr(season + "之月");
        /* DatesParent.transform.GetChild(date - 1).GetComponentInChildren<Toggle>().isOn =
           true;*/
        dateReferences.SelectIndex(date - 1);
        if (year <= 1 && season == Season.春)
        {
            forwardMonthButton.gameObject.SetActive(false);
        }
        else
        {
            forwardMonthButton.gameObject.SetActive(true);
        }
        if (year == GameTimeManager.instance.Year&&season==GameTimeManager.instance.Season)
        {
            dateReferences.GetReference(date - 1).DisplayToday();
        }
    }
    void ForwardMonth()
    {
       // BookPaper.gameObject.SetActive(true);
       //BookPaper.Play("Paper1");
       // GameTimerController.instance.DelayAction(820, () => BookPaper.gameObject.SetActive(false));
        int seasonId = (int)season;
        if (seasonId > 1)
        {
            seasonId--;
        }
        else if (seasonId == 1)
        {
            seasonId = 4;
            year--;
        }
        season = (Season)seasonId;
        CreatSeason(season);
        AfterDisplay(); 
    }
    void NextMonth()
    {
        //BookPaper.gameObject.SetActive(true);
        //BookPaper.Play("Paper");
        GameTimerController.instance.DelayAction(820, ()=>BookPaper.gameObject.SetActive(false));
        int seasonId = (int)season;
        if (seasonId < 4)
        {
            seasonId++;
        }
        else if(seasonId==4)
        {
            seasonId = 1;
            year++;
        }
        season = (Season) seasonId;
        CreatSeason(season);
        AfterDisplay();
    } 
    async void CreatSeason(Season season)
    {
        List<GameDate> gameDates = GameTimeManager.instance.GetGameDataForSeason(season);  
        if (DatesParent.transform.childCount > gameDates.Count)
        {
            for(int i = gameDates.Count; i < DatesParent.transform.childCount; i++)
            {
                Destroy(DatesParent.transform.GetChild(i).gameObject);
            }
        }
        await  dateReferences.InitListData(gameDates,DisplayClickDate,DatesParent,Async:false);
        AfterDisplay();
    }


    void DisplayClickDate(GameDate _gameDate,bool value)
    {
        if (value)
        {
            date = _gameDate.date;
            DataTimeText.text = year + LanguageManage.SwitchStr("年") + "  " + LanguageManage.SwitchStr(_gameDate.season + "之月");
            string festivalStr = "";
            List<FestivalData> festivalDatas = FestivalManager.instance.FestivalDatas;
            List<FestivalData> customFestivalDatas = FestivalManager.instance.customFestivalDatas;
            foreach (var festivalId in _gameDate.FestivaList)
            {
                FestivalData festivalData = festivalDatas.Find(f => f.id == festivalId);
                festivalStr += "·" + festivalData.name + "\n";
            }
            if (year == GameTimeManager.instance.Year)
            {
                foreach (var festivalId in _gameDate.CustomFestival)
                {
                    FestivalData festivalData = customFestivalDatas.Find(f => f.id == festivalId);
                    festivalStr += "·" + festivalData.name + "\n";
                }
            }

            festivaltext.text = festivalStr;
        } 
    }
	
}
