using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoxClickAction : MonoBehaviour
{
    public GameObject childSprite;
    private Vector2Int coordinate;
	// Use this for initialization
	void Start ()
	{
	    var Euqipments = GameComponentData.gameData.equipmentManager.Euqipments;
	    foreach (var euqipment in Euqipments)
	    {
	        if (euqipment.objName != "")
	        {
	            GameObject obj = transform.Find(euqipment.objName).gameObject;
	            if (obj != null)
	            {
	                obj.SetActive(euqipment.isBuy);
	            }
	        }

	    }
    }

    public void CheckChild()
    {
        if (DataSaveAndLoadTest.gameSaveData.marryData != null)
        {
            if (DataSaveAndLoadTest.gameSaveData.marryData.haveChildrenTime != null)
            {
                childSprite.SetActive(true);
            }
            else
            {
                childSprite.SetActive(false);
            }
        }
        else
        {
            DataSaveAndLoadTest.gameSaveData.marryData=new MarryData();
            childSprite.SetActive(false);
        }
       
    }
    

    public void ClickYiGui()
    {
        if (DataSaveAndLoadTest.isJsonData)
        {
            AudioController.instance.PlayAudio(SE.click);
            //GameComponentData.gameData.talkTextsManager.TalkAction("1000", TalkActionType.普通);
        }
        else
      
        if (GameComponentData.gameData.guideController.nowGuide == null &&
            GameComponentData.gameData.filmManager.nowFilm == null)
        {
            AudioController.instance.PlayAudio(SE.click);
            //GameComponentData.gameData.talkTextsManager.TalkAction("1000", TalkActionType.普通);
        }
        
    }
    public void ClickJingzi()
    {
        if (DataSaveAndLoadTest.isJsonData)
        {
            AudioController.instance.PlayAudio(SE.click);
            //GameComponentData.gameData.talkTextsManager.TalkAction("1001", TalkActionType.普通);
        }
        else
        if (GameComponentData.gameData.guideController.nowGuide == null &&
            GameComponentData.gameData.filmManager.nowFilm == null)
        {
            AudioController.instance.PlayAudio(SE.click);
            //GameComponentData.gameData.talkTextsManager.TalkAction("1001",TalkActionType.普通);
        }
       
    }
    public void ClickChouti()
    {
        if (DataSaveAndLoadTest.isJsonData)
        {
            AudioController.instance.PlayAudio(SE.click);
            //GameComponentData.gameData.talkTextsManager.TalkAction("1002", TalkActionType.普通);
        }
        else
        if (GameComponentData.gameData.guideController.nowGuide == null &&
            GameComponentData.gameData.filmManager.nowFilm == null)
        {
            AudioController.instance.PlayAudio(SE.click);
            //GameComponentData.gameData.talkTextsManager.TalkAction("1002", TalkActionType.普通);
        }
        
    }
    

    public void ClickBabyBed()
    {
        if (DataSaveAndLoadTest.isJsonData)
        {
            if (DataSaveAndLoadTest.gameSaveData.marryData.haveChildrenTime != null)
            {
                GameComponentData.gameData.gameManager.ClickChildAction();
            }
            else
            {
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"), LanguageManage.SwitchStr("一个小小的床，还没有使用者！"));
            }
        }
        else
        if (GameComponentData.gameData.guideController.nowGuide == null &&
            GameComponentData.gameData.filmManager.nowFilm == null)
        {
            if (DataSaveAndLoadTest.gameSaveData.marryData.haveChildrenTime != null)
            {
                GameComponentData.gameData.gameManager.ClickChildAction();
            }
            else
            {
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"), LanguageManage.SwitchStr("一个小小的床，还没有使用者！"));
            }
        }
        
       
    }
    public void ClickBookPanel()
    {
        if (DataSaveAndLoadTest.isJsonData)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.BookPanelObj.SetActive(true);
        }
        else
        if (GameComponentData.gameData.guideController.nowGuide == null &&
            GameComponentData.gameData.filmManager.nowFilm == null)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.BookPanelObj.SetActive(true);
        }
        
    }
    public void SetCoordinate(GameObject obj)
    {
      
        
    }
    // Update is called once per frame
    void Update () {
		
	}
}
