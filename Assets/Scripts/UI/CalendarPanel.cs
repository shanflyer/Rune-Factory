using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
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
        GameTimeManager.instance.runTime = false;
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
        DataTimeText.text = LanguageManage.instance.GameTimeToString(year, season);


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
    void CreatSeason(Season season)
    {
        RunLifecycleTask(token => CreatSeasonAsync(season, token), nameof(CreatSeason));
    }

    async System.Threading.Tasks.Task CreatSeasonAsync(Season season, System.Threading.CancellationToken cancellationToken)
    {
        List<GameDate> gameDates = GameTimeManager.instance.GetGameDataForSeason(season);
        if (DatesParent.transform.childCount > gameDates.Count)
        {
            for(int i = gameDates.Count; i < DatesParent.transform.childCount; i++)
            {
                Destroy(DatesParent.transform.GetChild(i).gameObject);
            }
        }
        // 日历翻页会频繁触发，旧月份列表完成后不能覆盖当前月份。
        await dateReferences.InitListData(gameDates, DisplayClickDate, DatesParent, Async: false, cancellationToken: cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        AfterDisplay();
    }


    void DisplayClickDate(GameDate _gameDate, int index, bool value)
    {
        if (value)
        {
            date = _gameDate.date;
            DataTimeText.text=_gameDate.ToString(year);

            List<string> festivalStr = new List<string>();
            if (_gameDate.FestivaList != null)
            {
                foreach (var festivalData in _gameDate.FestivaList)
                {
                    festivalStr.Add("·");
                    festivalStr.Add(festivalData.name);
                    festivalStr.Add("\n");
                }
            }

            if (year == GameTimeManager.instance.Year)
            {
                if (_gameDate.CustomFestival != null)
                {
                    foreach (var festivalData in _gameDate.CustomFestival)
                    {
                        festivalStr.Add("·");
                        festivalStr.Add(festivalData.name);
                        festivalStr.Add("\n");
                    }
                }
            }
            if (festivalStr.Count > 0)
            {
                festivaltext.SetADDText("", festivalStr.ToArray());
            }
            else
            {
                festivaltext.text="";
            }

        }
    }

}
