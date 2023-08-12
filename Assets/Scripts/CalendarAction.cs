using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalendarAction : MonoBehaviour
{
    public Button nextMonthButton, forwardMonthButton;
    public Transform DatesParent;
    public GameObject DatePro;
    public Text DataTimeText;
    public Text festivaltext;
    private int date;
    private int year;
    private Season season;
   
	// Use this for initialization
	void Start ()
	{
	    
    }

    public void ClickRetrun()
    {
        AudioController.instance.PlayAudio(SE.Return);
       transform.parent.gameObject.SetActive(false);
    }
    public void ZeroDateDisplay()
    {
        AudioController.instance.PlayAudio(SE.click);
        year = GameTimeManager.nowGameTime.gameDate.year;
        season = GameTimeManager.nowGameTime.gameDate.season;
        GameComponentData.gameData.gameTimeManager.StopTimeRun();
        CreatSeason(GameTimeManager.nowGameTime.gameDate.season);
        if (year <= 1&&season==Season.春)
        {
            forwardMonthButton.interactable = false;
        }
        else
        {
            forwardMonthButton.interactable = true;
        }
        date = GameTimeManager.nowGameTime.gameDate.date;
        DataTimeText.text = year + LanguageManage.SwitchStr("年") + " " + LanguageManage.SwitchStr(season+ "之月");
        StartCoroutine("ClickDate");
    }
    public void ForwardMonth()
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
        StartCoroutine("ClickDate");
       
    }
    public void NextMonth()
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
        StartCoroutine("ClickDate");
    }

    IEnumerator ClickDate()
    {
        yield return new WaitForFixedUpdate();
        DatesParent.GetChild(date - 1).GetComponentInChildren<Toggle>().isOn =
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
    public void CreatSeason(Season season)
    {
        foreach (Transform child in DatesParent)
        {
            Destroy(child.gameObject);
        }
        List<GameDate> gameDates =
            GameComponentData.gameData.gameTimeManager.gameDates.FindAll(g => g.season == season);
        foreach (var gameDate in gameDates)
        {
            GameObject DateObj = Instantiate(DatePro) as GameObject;
            DateObj.transform.SetParent(DatesParent, false);
            DateObj.GetComponent<DateDisplayAction>().InitData(gameDate,this);
            DateObj.transform.GetComponentInChildren<Toggle>().group = DatesParent.GetComponent<ToggleGroup>();
            if ((gameDate.date - 1) % 6 == 0)
            {
                DateObj.transform.GetChild(0).GetComponent<Image>().color=new Color(1,0.76f,0.64f);
            }
        }
        
    }


    public void DisplayClickDate(GameDate _gameDate)
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
	// Update is called once per frame
	void Update () {
		
	}
}
