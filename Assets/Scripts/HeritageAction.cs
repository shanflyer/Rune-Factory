using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum HeritageType
{
    售卖件数=0,
    植物成熟=1,
    收获动物=2,
    冒险次数=3,
    钓鱼次数=4,
    友好度=5,
    金币捡完=7,
    无=6

}
[System.Serializable]
public class Heritage
{
    public int id;
    public string title;
    public string tiaojian;
    public HeritageType heritageType;
    public int heritageValue;
    public int rewardValue;
    public GameObject bookObj;
    public string openNotice;
    public bool isGet;
    public int openId;
    public List<Button> buttonns;
    public void ZeroCheck()
    {
        if (heritageType == HeritageType.无)
        {
            isGet = true;
        }
        else
        {
            CharactorTitleAction charactorTitleAction = GameComponentData.gameData.charactorTitleAction;
            switch (heritageType)
            {
                case HeritageType.金币捡完:
                    //if ((GameComponentData.gameData.coinAction.Coins!=null&&GameComponentData.gameData.coinAction.Coins.Count <= 0)&& charactorTitleAction.busicessExp >0)
                    {
                        isGet = true;
                    }
                    
                    break;
                case HeritageType.冒险次数:
                    if (charactorTitleAction.explorCount >= heritageValue)
                    {

                        isGet = true;

                    }
                    break;
                case HeritageType.植物成熟:
                    if (charactorTitleAction.plantingExp >= heritageValue)
                    {
                        isGet = true;
                    }
                    break;
                case HeritageType.收获动物:
                    if (charactorTitleAction.livestockExp >= heritageValue)
                    {
                        isGet = true;
                    }
                    break;
                case HeritageType.售卖件数:
                    if (charactorTitleAction.busicessExp >= heritageValue)
                    {
                        isGet = true;
                    }
                    break;
                case HeritageType.钓鱼次数:
                    if (charactorTitleAction.fishingExp >= heritageValue)
                    {
                        isGet = true;
                    }
                    break;
                case HeritageType.友好度:
                    int x = 0;
                  
                    break;
            }
        }
        if (!GameComponentData.gameData.gameDebugAction.GameStartTest)
        {
            bookObj.SetActive(isGet);
            foreach (var buttonn in buttonns)
            {
                buttonn.interactable = isGet;
            }
        }
        


    }
    public void Check()
    {

        if (!isGet)
        {
            if (heritageType == HeritageType.无)
            {
                isGet = true;
            }
            else
            {
                CharactorTitleAction charactorTitleAction = GameComponentData.gameData.charactorTitleAction;
                switch (heritageType)
                {
                    case HeritageType.金币捡完:
                        if (DataSaveAndLoadTest.isJsonData)
                        {
                            isGet = true;
                        }
                        else
                        {
                           // if ((GameComponentData.gameData.coinAction.Coins != null && GameComponentData.gameData.coinAction.Coins.Count <= 0) && charactorTitleAction.busicessExp > 0)
                            {
                                isGet = true;
                                GameComponentData.gameData.DisplayPrompt(LanguageManage.SwitchStr("获得红晶:") + rewardValue + "," +
                                                                         openNotice);
                            }
                        }
                       

                        break;
                    case HeritageType.冒险次数:
                        if (charactorTitleAction.explorCount >= heritageValue)
                        {

                            isGet = true;
                            GameComponentData.gameData.gameManager.ChangePlayerMoney1(rewardValue);
                            GameComponentData.gameData.DisplayPrompt(LanguageManage.SwitchStr("获得红晶:") + rewardValue + "," +
                                                                                                       openNotice);

                        }
                        break;
                    case HeritageType.植物成熟:
                        if (charactorTitleAction.plantingExp >= heritageValue)
                        {
                            isGet = true;
                            GameComponentData.gameData.gameManager.ChangePlayerMoney1(rewardValue);
                            GameComponentData.gameData.DisplayPrompt(LanguageManage.SwitchStr("获得红晶:") + rewardValue + "," +
                                                                     openNotice);
                        }
                        break;
                    case HeritageType.收获动物:
                        if (charactorTitleAction.livestockExp >= heritageValue)
                        {
                            isGet = true;
                            GameComponentData.gameData.gameManager.ChangePlayerMoney1(rewardValue);
                            GameComponentData.gameData.DisplayPrompt(LanguageManage.SwitchStr("获得红晶:") + rewardValue + "," +
                                                                     openNotice);
                        }
                        break;
                    case HeritageType.售卖件数:
                        if (charactorTitleAction.busicessExp >= heritageValue)
                        {
                            isGet = true;
                            GameComponentData.gameData.gameManager.ChangePlayerMoney1(rewardValue);
                            GameComponentData.gameData.DisplayPrompt(LanguageManage.SwitchStr("获得红晶:") + rewardValue + "," +
                                                                     openNotice);
                        }
                        break;
                    case HeritageType.钓鱼次数:
                        if (charactorTitleAction.fishingExp >= heritageValue)
                        {
                            isGet = true;
                            GameComponentData.gameData.gameManager.ChangePlayerMoney1(rewardValue);
                            GameComponentData.gameData.DisplayPrompt(LanguageManage.SwitchStr("获得红晶:") + rewardValue + "," +
                                                                     openNotice);
                        }
                        break;
                    case HeritageType.友好度:
                        int x = 0;
                       
                        break;
                }
                bookObj.SetActive(isGet);
            }
            if (isGet)
            {
                GameComponentData.gameData.heritageAction.nowOpen = id;
                GameComponentData.gameData.eventManager.CheckEvents();
            }

        }
        foreach (var buttonn in buttonns)
        {
            buttonn.interactable = isGet;
        }



    }
}

public class HeritageAction : MonoBehaviour
{
    public GameObject heritageObj;
    public List<Heritage> Heritages;
    public Transform heritageParent;
    public GameObject heritagePro;
    [HideInInspector] public int nowOpen;
    public void CheckHeritagesData()
    {
        foreach (var heritage in Heritages)
        {
            heritage.Check();
        }
        DisplayHeritages();
    }
    public void CheckZeroHeritagesData()
    {
        foreach (var heritage in Heritages)
        {
            heritage.ZeroCheck();
        }
        DisplayHeritages();
    }
    public void DisplayHeritages()
    {
        foreach (Transform child in heritageParent)
        {
            Destroy(child.gameObject);
        }
        foreach (var heritage in Heritages)
        {
            GameObject heritageObj = Instantiate(heritagePro);
            heritageObj.transform.SetParent(heritageParent, false);
            heritageObj.GetComponent<HeritageProAction>().InitData(heritage);
        }
    }
	// Use this for initialization
	void Start () {
	    foreach (var heritage in Heritages)
	    {
	        heritage.title = LanguageManage.SwitchStr(heritage.title);
	        heritage.tiaojian = LanguageManage.SwitchStr(heritage.tiaojian);
	        heritage.openNotice = LanguageManage.SwitchStr(heritage.openNotice);
	    }
	    //CheckZeroHeritagesData();

	}

    public void ClickheritageObjbutton()
    {
        heritageObj.SetActive(true);
        CheckZeroHeritagesData();
        AudioController.instance.PlayAudio(SE.Book);
    }

    public void ClickReturnbutton()
    {
        GameComponentData.gameData.eventManager.CheckEvents();
        AudioController.instance.PlayAudio(SE.Return);
        heritageObj.SetActive(false);

    }
	// Update is called once per frame
	void Update () {
		
	}
}
