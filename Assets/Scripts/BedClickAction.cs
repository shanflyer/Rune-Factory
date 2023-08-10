using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedClickAction : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public void ClickBed()
    {
        GameComponentData.gameData.gameManager.InitCareSelectData(LanguageManage.SwitchStr("睡觉"),LanguageManage.SwitchStr("是否确定睡眠到下一日？"),CareType.SLEEP);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
