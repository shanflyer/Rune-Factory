using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEvent : MonoBehaviour {
    public void clickRiverFishing(string coordinateStr)
    {
        if (GameComponentData.gameData.heritageAction.Heritages.Find(o => o.id == 1003).isGet)
        {
            var x = coordinateStr.Split(',');
            Vector2Int coordinate = new Vector2Int(int.Parse(x[0]), int.Parse(x[1]));
            GameComponentData.gameData.gameManager.playerCharactor.Obj.GetComponent<NPCAnimationAction>().FishIngMove(coordinate);
        }
        else
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("未许可"), LanguageManage.SwitchStr("垂钓许可未获得，不能垂钓，详情见许可清单"));
        }
        
    }

    public void ClickMove(int mapID)
    {
        GameComponentData.gameData.gameManager.MoveToMap(mapID);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
