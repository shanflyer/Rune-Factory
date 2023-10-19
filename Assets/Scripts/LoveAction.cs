using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public enum YueStatus
{
    对话=0,
    互动=1
}
public class LoveAction : MonoBehaviour {
     
    public GameObject loveFunctionObj,yueTalkObj1, yueTalkObj2;
    public GameObject MarriedPro;
    private int selectIndex0, selectIndex1;
   
    // Use this for initialization
    void Start () {
		
	}

    public void YueAction()
    {
        int index = Random.Range(0, 3);
        if (index == 0)
        {
            //GameComponentData.gameData.talkTextsManager.TalkAction("1470",lover.npcData.headName,lover.Name,lover,TalkActionType.约会中0);
        }
        else if(index==1)
        {
            //GameComponentData.gameData.talkTextsManager.TalkAction("1471", lover.npcData.headName, lover.Name, lover, TalkActionType.约会中0);
        }
        else if(index==2)
        {
            //GameComponentData.gameData.talkTextsManager.TalkAction("1472", lover.npcData.headName, lover.Name, lover, TalkActionType.约会中0);
        }
        selectIndex0 = index;
    }
    public void YueHuiEndTalkAction()
    {
        GameComponentData.gameData.gameManager.MoveToMap(2010);

    }
    public void YueTalk1End() {
        if (selectIndex1 != -1)
        {
            loveFunctionObj.SetActive(true);
            yueTalkObj2.SetActive(true);
            yueTalkObj1.SetActive(false);
        }
        else
        {
          //  GameComponentData.gameData.DisplayWaitPanelData(WaitType.约会, "甜蜜的时光，总是短暂的...");
        }
        
    }

    public void YuehuiEnd()
    { 
        GameComponentData.gameData.gameManager.MoveToOldMap();
       // Destroy(GameComponentData.gameData.mapParent.GetComponentInChildren<SeasonSelect>().LoverTransform.GetChild(0).gameObject);
    }
    public void YueTalk0End()
    {
        loveFunctionObj.SetActive(true);
        yueTalkObj1.SetActive(true);
        yueTalkObj2.SetActive(false);
    }

    public void SelectYueButton1(int x)
    {
        loveFunctionObj.SetActive(false);
        if (x == selectIndex1)
        { 
            //GameComponentData.gameData.talkTextsManager.TalkAction("1481", lover.npcData.headName, lover.Name, lover,TalkActionType.约会中2);
        }
        else
        {
            //GameComponentData.gameData.talkTextsManager.TalkAction("1480", lover.npcData.headName, lover.Name, lover, TalkActionType.约会中2);
        }
        selectIndex1 = -1;
    }

    public void MarriedAction( )
    {
        
        //GameComponentData.gameData.DisplayWaitPanelData(WaitType.婚礼, "婚礼即将举行的消息传遍了小镇..");
        


       // GameComponentData.gameData.gameManager.functionButtn.SetActive(false);
        //StartCoroutine("Marring");
    }

 

    public void WeddingEnd()
    { 
        GameComponentData.gameData.gameManager.gamePlayer.isMarried = true;
       
        GameComponentData.gameData.gameManager.MoveToMap(1003);
    }

 
    public void SelectYueButton0(int x)
    {
        loveFunctionObj.SetActive(false);
        if (x == selectIndex0)
        { 
            selectIndex1 = Random.Range(0, 5);
            if (selectIndex1 == 0)
            {
                //GameComponentData.gameData.talkTextsManager.TalkAction("1474", lover.npcData.headName, lover.Name, lover, TalkActionType.约会中2);
            }
            else if (selectIndex1 == 1)
            {
                //GameComponentData.gameData.talkTextsManager.TalkAction("1475", lover.npcData.headName, lover.Name, lover, TalkActionType.约会中2);
            }
            else if (selectIndex1 == 2)
            {
                //GameComponentData.gameData.talkTextsManager.TalkAction("1476", lover.npcData.headName, lover.Name, lover, TalkActionType.约会中2);
            }
            else if (selectIndex1 == 3)
            {
                //GameComponentData.gameData.talkTextsManager.TalkAction("1477", lover.npcData.headName, lover.Name, lover, TalkActionType.约会中2);
            }
            else if (selectIndex1 == 4)
            {
                //GameComponentData.gameData.talkTextsManager.TalkAction("1478", lover.npcData.headName, lover.Name, lover, TalkActionType.约会中2);
            }
        }
        else
        {
            selectIndex1 = -1;
            //GameComponentData.gameData.talkTextsManager.TalkAction("1473", lover.npcData.headName, lover.Name, lover, TalkActionType.约会中2);
        }
        
       
    }
    // Update is called once per frame
    void Update () {
		
	}
}
