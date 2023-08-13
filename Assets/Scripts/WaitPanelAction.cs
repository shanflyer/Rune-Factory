using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WaitType
{
    SLEEP,
    Transition,
    约会,
    婚礼,
    GAMEEND
}
public class WaitPanelAction : MonoBehaviour
{
    private WaitType waitType;
    public Text noticeText;
    public void InitData(WaitType _waitType, string _text)
    {
        waitType = _waitType;
        noticeText.text =LanguageManage.SwitchStr(_text);
        GetComponent<Animator>().SetBool("IsSleep",true);
    }

    public void InWaitEndAction()
    {
        // GameComponentData.gameData.passDataManager.PlayerMapBGM();
        GetComponent<Animator>().SetBool("IsSleep", false);
        switch (waitType)
        {
            case WaitType.SLEEP:

                GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer; 
                AudioController.instance.PlayAudio(SE.click); 

                NextDate();

                //GameComponentData.gameData.DisplayPrompt(LanguageManage.SwitchStr("距离开始营业还有一段时间，可以出去逛逛哦！"));
                //GameComponentData.gameData.gameTimeManager.NextDate();
                //GameComponentData.gameData.gameManager.UpDataPlayer();
                break;
            case WaitType.Transition:
                GameComponentData.gameData.filmManager.PlayNowFilm();
                break;
            case WaitType.约会:
                GameComponentData.gameData.loveAction.YuehuiEnd();
                break;
            case WaitType.婚礼:
                GameComponentData.gameData.filmManager.CreatFilm(8003);
                break;

        }
       

    }
    public void SleepEndAction()
    {
        GameComponentData.gameData.DisplayPrompt(LanguageManage.SwitchStr("新的一天开始了！"));
        GameComponentData.gameData.passDataManager.PlayerMapBGM();
        GetComponent<Animator>().SetBool("IsSleep", false);
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        gamePlayer.property.HP = gamePlayer.property.MaxHP;
        gamePlayer.property.Power = gamePlayer.property.MaxPower;
        if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0)
        {
            gamePlayer.TeamPlayer0.property.HP = gamePlayer.TeamPlayer0.property.MaxHP;
        }
        if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0)
        {
            gamePlayer.TeamPlayer1.property.HP = gamePlayer.TeamPlayer1.property.MaxHP;
        }
        GameComponentData.gameData.gameManager.UpDataPlayer();
        GameComponentData.gameData.gameManager.MoveToMap(1003);
        Euqipment euqipment = GameComponentData.gameData.equipmentManager.Euqipments.Find(e => e.id == 2006);
        if (GameComponentData.gameData.gameDebugAction.PregnancyTest &&
            DataSaveAndLoadTest.gameSaveData.marryData.pregnancyTime == null)
        {
            GameComponentData.gameData.filmManager.CreatFilm(8004);
        }
        else
        {
            if (DataSaveAndLoadTest.gameSaveData.marryData.pregnancyTime != null &&
                DataSaveAndLoadTest.gameSaveData.marryData.haveChildrenTime == null)
            {
                if (GameComponentData.gameData.gameDebugAction.HaveChildTest)
                {
                    GameComponentData.gameData.filmManager.CreatFilm(8005);
                }
                else
                {
                    int days = GameComponentData.gameData.gameTimeManager.DaysToSave(DataSaveAndLoadTest.gameSaveData.marryData
                        .pregnancyTime);
                    if (days >= 9)
                    {
                        GameComponentData.gameData.filmManager.CreatFilm(8005);
                    }
                }

                
            }
            else if (euqipment.isBuy && GameComponentData.gameData.gameManager.gamePlayer.isMarried &&
                     DataSaveAndLoadTest.gameSaveData.marryData.pregnancyTime == null)
            {
                int randomValue = Random.Range(0, 100);
                if (randomValue >= 50)
                {
                    /* 怀孕   */
                    GameComponentData.gameData.filmManager.CreatFilm(8004);
                }
            }
        }



    }

    public void NextDate()
    {
        GameComponentData.gameData.gameTimeManager.NextDate();

       
        Debug.Log(GameTimeManager.nowGameTime.gameDate.year + "," + GameTimeManager.nowGameTime.gameDate.season.ToString() + "," + GameTimeManager.nowGameTime.gameDate.date);

        //
        GameComponentData.gameData.gameManager.functionButtn.SetActive(true);

        if (GameTimeManager.nowGameTime.gameDate.year == 1300 && GameTimeManager.nowGameTime.gameDate.season == Season.夏
            && GameTimeManager.nowGameTime.gameDate.date == 21)
        { 
            if (PackageManager.instance.GetPackageItems(0).Count > 0)
            {
                //GameComponentData.gameData.filmAction.SecondDayEvent();
                GameComponentData.gameData.informationObj.SetActive(false);
            }
            else
            {
                GameComponentData.gameData.gameManager.functionButtn.SetActive(true);
            }

        }
       // gameObject.SetActive(false);
    }
    public void OutWaitEndAction()
    {
        switch (waitType)
        {
            case WaitType.SLEEP:

                //GameComponentData.gameData.cookingController.AfterSleep();
                SleepEndAction();
                GameComponentData.gameData.eventManager.CheckEvents();

                break;
           
        }
  
        gameObject.SetActive(false);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
