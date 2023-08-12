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
    public void ClickObjAction(int index)
    {
        if (DataSaveAndLoadTest.isJsonData)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.boxSelectFunctionObj.SetActive(true);
            GameComponentData.gameData.boxSelectFunctionObj.GetComponent<BoxSelectAction>().InitBoxSelectData((PackageType)(index));
        }
        else 
        if (GameComponentData.gameData.guideController.nowGuide == null &&
            GameComponentData.gameData.filmManager.nowFilm == null)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.boxSelectFunctionObj.SetActive(true);
            GameComponentData.gameData.boxSelectFunctionObj.GetComponent<BoxSelectAction>().InitBoxSelectData((PackageType)(index));
        }
           
    }

    public void ClickYiGui()
    {
        if (DataSaveAndLoadTest.isJsonData)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.talkTextsManager.TalkAction("1000", TalkActionType.普通);
        }
        else
      
        if (GameComponentData.gameData.guideController.nowGuide == null &&
            GameComponentData.gameData.filmManager.nowFilm == null)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.talkTextsManager.TalkAction("1000", TalkActionType.普通);
        }
        
    }
    public void ClickJingzi()
    {
        if (DataSaveAndLoadTest.isJsonData)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.talkTextsManager.TalkAction("1001", TalkActionType.普通);
        }
        else
        if (GameComponentData.gameData.guideController.nowGuide == null &&
            GameComponentData.gameData.filmManager.nowFilm == null)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.talkTextsManager.TalkAction("1001",TalkActionType.普通);
        }
       
    }
    public void ClickChouti()
    {
        if (DataSaveAndLoadTest.isJsonData)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.talkTextsManager.TalkAction("1002", TalkActionType.普通);
        }
        else
        if (GameComponentData.gameData.guideController.nowGuide == null &&
            GameComponentData.gameData.filmManager.nowFilm == null)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.talkTextsManager.TalkAction("1002", TalkActionType.普通);
        }
        
    }
    public void ProduceClickAction(int i)
    {
        AudioController.instance.PlayAudio(SE.click);
        if (GameComponentData.gameData.heritageAction.Heritages.Find(o => o.id == 1004).isGet)
        {
            GameComponentData.gameData.formulaAction.DisplayManufacturePanel(i);
        }
        else
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("未许可"),LanguageManage.SwitchStr("制作许可未获得，不能制作，详情见许可清单"));
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
        if (DataSaveAndLoadTest.isJsonData)
        {
            coordinate = AStarTest.PosToCoordinate(obj.transform.position);
            GameComponentData.gameData.gameManager.euqipmentCoordinate = coordinate;
        }
        else
        if (GameComponentData.gameData.guideController.nowGuide == null &&
            GameComponentData.gameData.filmManager.nowFilm == null)
        {
            coordinate = AStarTest.PosToCoordinate(obj.transform.position);
            GameComponentData.gameData.gameManager.euqipmentCoordinate = coordinate;
        }
        
    }
    // Update is called once per frame
    void Update () {
		
	}
}
