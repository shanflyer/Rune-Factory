using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeDisplayAction : MonoBehaviour
{
    public GameObject Obj0, Obj1;
    public Text SeasonText0, DateText0, WeakText0, YearText0;
    public Text SeasonText1, DateText1, WeakText1, YearText1;
    private GameTime gameTime;

	// Use this for initialization
	void Start () {
      
	    if (LanguageManage.nowLanguage==SystemLanguage.Chinese)
	    {
	        Obj0.SetActive(true);
	        Obj1.SetActive(false);
	    }
	    else
	    {
	        Obj0.SetActive(false);
	        Obj1.SetActive(true);
	    }
    }
    public void UpdataTime()
    {
        gameTime = GameTimeManager.instance.nowGameTime;
        
        SeasonText0.text = gameTime.gameDate.season.ToString();
        DateText0.text = gameTime.gameDate.date + "日";
        YearText0.text = gameTime.gameDate.year + "年";
        WeakText0.text = "【" + gameTime.week + "】";
        
        DateText1.text = gameTime.gameDate.date.ToString();
        YearText1.text = gameTime.gameDate.year.ToString();
        SeasonText1.text = LanguageManage.SwitchStr(gameTime.gameDate.season.ToString());
       
        switch (gameTime.week)
        {
            case Week.SunDay:
                WeakText0.text = "【休】";
                WeakText1.text = "【SUN】";
                break;
            case Week.ThursDay:
                WeakText0.text = "【四】";
                WeakText1.text = "【THR】";
                break;
            case Week.FriDay:
                WeakText0.text = "【五】";
                WeakText1.text = "【FRI】";
                break;
            case Week.Monday:
                WeakText0.text = "【一】";
                WeakText1.text = "【MON】";
                break;
            case Week.TuesDay:
                WeakText0.text = "【二】";
                WeakText1.text = "【TUE】";
                break;
            case Week.WednesDay:
                WeakText0.text = "【三】";
                WeakText1.text = "【WED】";
                break;
        }
  
    }
	// Update is called once per frame
	void Update () {
		
	}
}
