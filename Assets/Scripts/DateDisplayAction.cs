using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DateDisplayAction : MonoBehaviour
{
    public Text ValueText;
    public Image festivalTips;
    private GameDate gameDate;
    private CalendarAction calendarAction;
    public void InitData(GameDate _gameDate,CalendarAction _calendarAction)
    {
        gameDate = _gameDate;
        ValueText.text = _gameDate.date.ToString();
        if (gameDate.FestivaList.Count>0)
        {
            festivalTips.enabled = true;
        }
        else
        {
            festivalTips.enabled = false;
        }
        calendarAction = _calendarAction;
    }

    public void ClickAction()
    {
        AudioManager.PlaySE(PlayType.ONCE,"Select");
        calendarAction.DisplayClickDate(gameDate);
    }
	// Use this for initialization
	void Start () {
		
	}

	
	// Update is called once per frame
	void Update () {
		
	}
}
