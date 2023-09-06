using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CalendarPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Button nextMonthButton, forwardMonthButton;
    [SerializeField]
    private ToggleGroup DatesParent;
    [SerializeField]
    private DateReference dateReference;
    [SerializeField]
    private Text DataTimeText;
    [SerializeField]
    private Text festivaltext;
    [SerializeField]
    private Button returnButton;

    private int date;
    private int year;
    private Season season;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        nextMonthButton = FindChildGameObject<Button>("NextButton");
        forwardMonthButton = FindChildGameObject<Button>("ForwardButton");
        DatesParent = FindChildGameObject<ToggleGroup>("DatesParent");
        dateReference = FindChildGameObject<DateReference>("DateReference");
        returnButton = FindChildGameObject<Button>("ReturnButton");
        DataTimeText = FindChildGameObject<Text>("time");
        festivaltext = FindChildGameObject<Text>("festival");
    }
    protected override void Awake()
    {
        base.Awake();
        returnButton.onClick.AddListener(() =>
        {
            AudioController.instance.PlayAudio(SE.Return);
            Close();
        });

        nextMonthButton.onClick.AddListener(NextMonth);
        forwardMonthButton.onClick.AddListener(ForwardMonth);
    }
    public override Task InitData(string dataKay)
    {

        year = GameTimeManager.nowGameTime.gameDate.year;
        season = GameTimeManager.nowGameTime.gameDate.season;
        GameComponentData.gameData.gameTimeManager.StopTimeRun();
        CreatSeason(GameTimeManager.nowGameTime.gameDate.season);
        if (year <= 1 && season == Season.春)
        {
            forwardMonthButton.interactable = false;
        }
        else
        {
            forwardMonthButton.interactable = true;
        } 
        AfterDisplay(); 
        return base.InitData(dataKay);
    }
    
    void AfterDisplay()
    {
        date = GameTimeManager.nowGameTime.gameDate.date;
        DataTimeText.text = year + LanguageManage.SwitchStr("年") + " " + LanguageManage.SwitchStr(season + "之月");
        DatesParent.transform.GetChild(date - 1).GetComponentInChildren<Toggle>().isOn =
          true;
         
        if (year <= 1300 && season == Season.春)
        {
            forwardMonthButton.gameObject.SetActive(false);
        }
        else
        {
            forwardMonthButton.gameObject.SetActive(true);
        }
    }
    void ForwardMonth()
    {
        AudioController.instance.PlayAudio(SE.Book);
         
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
        AudioController.instance.PlayAudio(SE.Book);
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
    void CreatSeason(Season season)
    {
        List<GameDate> gameDates =
            GameComponentData.gameData.gameTimeManager.gameDates.FindAll(g => g.season == season);

        if (DatesParent.transform.childCount > gameDates.Count)
        {
            for(int i = gameDates.Count; i < DatesParent.transform.childCount; i++)
            {
                Destroy(DatesParent.transform.GetChild(i).gameObject);
            }
        }
        
        for(int i=0;i<gameDates.Count;i++)
        {
            if (DatesParent.transform.childCount > i)
            {
                DateReference dateReference=DatesParent.transform.GetChild(i).GetComponent<DateReference>();
                dateReference.InitData(gameDates[i]);
                dateReference.enabled = true;
                dateReference.transform.localScale = Vector3.one;
            }
            else
            {
                DateReference dateReference =Instantiate(this.dateReference, DatesParent.transform);
                dateReference.InitData(gameDates[i]);
                dateReference.enabled = true;
                dateReference.transform.localScale = Vector3.one;
                dateReference.SetToggleAction(DatesParent, (bool value) =>
                {
                    if (value)
                    {
                        DisplayClickDate(gameDates[i]);
                    }
                });
            }
        }
        
    }


    void DisplayClickDate(GameDate _gameDate)
    {
        
        date = _gameDate.date;
        DataTimeText.text = year + LanguageManage.SwitchStr("年") + "  " + LanguageManage.SwitchStr(_gameDate.season + "之月");
        string festivalStr = "";
        List<FestivalData> festivalDatas = GameComponentData.gameData.festivalManager.FestivalDatas;
        List<FestivalData> customFestivalDatas = GameComponentData.gameData.festivalManager.customFestivalDatas;
        foreach (var festivalId in _gameDate.FestivaList)
        {
            FestivalData festivalData = festivalDatas.Find(f => f.id == festivalId);
            festivalStr += "·" + festivalData.name+"\n";
        }
        if (year == GameTimeManager.nowGameTime.gameDate.year)
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
