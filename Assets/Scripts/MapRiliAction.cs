using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRiliAction : MonoBehaviour
{
    public GameObject canlendarPanel;

    public CalendarAction calendarAction;
	// Use this for initialization
	void Start () {
		
	}

    public void ClickAction()
    {
        GameComponentData.gameData.calenderPanel.SetActive(true);
        GameComponentData.gameData.calendarAction.ZeroDateDisplay();
    }
	// Update is called once per frame
	void Update () {
		
	}
}
