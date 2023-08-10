using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAction : MonoBehaviour
{
    public MapRiliAction mapRiliAction;
	// Use this for initialization
	void Start () {
		
	}

    public void InitRoomData(GameObject calenderPanel, CalendarAction calendarAction)
    {
        mapRiliAction.calendarAction = calendarAction;
        mapRiliAction.canlendarPanel = calenderPanel;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
