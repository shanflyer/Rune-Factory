using OldName;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class AdventurePanelAction : MonoBehaviour
{
    public List<Text> functionTexts;
    public GameObject itemIconPro;
    public Transform itemParent;


    public GameObject FightPanelObj;
    public Sprite nullCharactorSprite;
    public Image TeamPlayerImage0, TeamPlayerImage1, TeamPlayerImage2;

    public GameObject PlacePanelPro;

    public Transform PlaceParent;
    private List<GameObject> PlaceObjs;
    public Text noticeText, explorerText;
    public Button explorbutton;
    private BatteleMap selectMap;
	// Use this for initialization
	void Start ()
	{
	    
	}
    public void InitData()
    {
        GameComponentData.gameData.SaveButtonObj.SetActive(false);
        foreach (var functionText in functionTexts)
        {
            LanguageManage.TextFanyi(functionText);
        }


        GameComponentData.gameData.pastureAction.MoveCameraButtonObj.SetActive(false);
        AudioController.instance.PlayAudio(BGM.bgm002);
         
        if (PlaceObjs == null)
        {
            PlaceObjs = new List<GameObject>();
        }
        else
        {
            foreach(var x in PlaceObjs)
            {
                Destroy(x);
            }
            PlaceObjs=new List<GameObject>();
        }
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        TeamPlayerImage0.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.playerImage);
        if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0)
        {
            
            TeamPlayerImage1.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer0.charactorImage);
        }
        else
        {
            TeamPlayerImage1.sprite = nullCharactorSprite;
        }
        if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0 )
        {
            TeamPlayerImage2.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer1.charactorImage);
        }
        else
        {
            TeamPlayerImage2.sprite = nullCharactorSprite;
        }
        List<BatteleMap> battleMaps = GameComponentData.gameData.BattleMapAction.BatteleMaps;
        foreach(var battleMap in battleMaps)
        {
            if (battleMap.id != 4100)
            {
                GameObject battleObj = Instantiate(PlacePanelPro);

                battleObj.GetComponent<PlacePanelAction>().InitData(battleMap);
                battleObj.transform.SetParent(PlaceParent, true);
                battleObj.GetComponentInChildren<Toggle>().group = PlaceParent.GetComponent<ToggleGroup>();
                PlaceObjs.Add(battleObj);
                battleObj.transform.localScale=Vector3.one;
            }
            
        }
        PlaceObjs[0].GetComponent<PlacePanelAction>().ClickToggleButton();
    }

    public void ReturnButton()
    {
        gameObject.SetActive(false);
        GameComponentData.gameData.SaveButtonObj.SetActive(true);
        AudioController.instance.PlayAudio(SE.Return);
        if (GameComponentData.gameData.passDataManager.NowPassData.id == 1001)
        {
            GameComponentData.gameData.gameManager.fieldTool.SetActive(true);
        }
        GameComponentData.gameData.passDataManager.PlayerMapBGM();
    }

    public void ZeroEploring()
    {

        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        gamePlayer.property.HP = gamePlayer.property.MaxHP;
        gamePlayer.property.Power = gamePlayer.property.MaxPower;
        gamePlayer.TeamPlayer0 = new TeamPlayer(2004, LanguageManage.SwitchStr("马"), 1, 1005, gamePlayer.property,0);
        gamePlayer.TeamPlayer0.ObjName = "house";
        gamePlayer.TeamPlayer1 = new TeamPlayer(3015, LanguageManage.SwitchStr("班尼迪克"), 1, 1002, gamePlayer.property,0);
        gamePlayer.TeamPlayer1.ObjName = "man1";
        BatteleMap batteleMap = GameComponentData.gameData.BattleMapAction.BatteleMaps.Find(b => b.id == 4100);
  
        AudioController.instance.PlayAudio(BGM.Move);
        GameComponentData.gameData.mapParent.gameObject.SetActive(false);
        FightPanelObj.SetActive(true);
        FightPanelObj.GetComponent<FightPanelAction>().InitFightData(batteleMap);
        GameComponentData.gameData.gameManager.StartFight();
        GameComponentData.gameData.BattleMapAction.InitBattleData(batteleMap,true);
        

        
        GameComponentData.gameData.informationObj.SetActive(false);
        gameObject.SetActive(false);
    }

    public void SelectBattelMap(BatteleMap batteleMap)
    {
        selectMap = batteleMap;
        explorbutton.interactable = selectMap.isOpen;
        noticeText.text = selectMap.battleNotice;
        explorerText.text = LanguageManage.SwitchStr("探索度:") + selectMap.completeValue;
        explorbutton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("探索");
        DisplayBattleMapItem();

    }

    public void ClickExplorButton()
    {
        AudioController.instance.PlayAudio(SE.select);
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        if (gamePlayer.property.Power >= 5)
        {
            bool isCare0=false, isCare1 = false, isCare2 = false;
            float x = GameComponentData.gameData.BattleMapAction.GetAttritubeEffectValue(selectMap.attributeType,
                gamePlayer.attributeType);
            if (x >= 0.5f)
            {
                GameComponentData.gameData.gameManager.InitCareSelectData(LanguageManage.SwitchStr("提示"),
                    LanguageManage.SwitchStr("你的属性:") + LanguageManage.SwitchStr(gamePlayer.attributeType.ToString())
                    + "," + LanguageManage.SwitchStr("可能会受怪物属性克制") + "," + LanguageManage.SwitchStr("是否确定继续挑战？"),
                    CareType.属性提醒);
                isCare0 = true;
            }
            if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0)
            {
                x = GameComponentData.gameData.BattleMapAction.GetAttritubeEffectValue(selectMap.attributeType,
                    gamePlayer.TeamPlayer0.attributeType);
                if (x >= 0.5f)
                {
                    GameComponentData.gameData.gameManager.InitCareSelectData(LanguageManage.SwitchStr("提示"),
                        gamePlayer.TeamPlayer0.name + LanguageManage.SwitchStr(" 属性:") +
                        LanguageManage.SwitchStr(gamePlayer.TeamPlayer0.attributeType.ToString())
                        + "," + LanguageManage.SwitchStr("可能会受怪物属性克制") + "," + LanguageManage.SwitchStr("是否确定继续挑战？"),
                        CareType.属性提醒);
                    isCare1 = true;
                }

            }
            if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0)
            {
                x = GameComponentData.gameData.BattleMapAction.GetAttritubeEffectValue(selectMap.attributeType,
                    gamePlayer.TeamPlayer1.attributeType);
                if (x >= 0.5f)
                {
                    GameComponentData.gameData.gameManager.InitCareSelectData(LanguageManage.SwitchStr("提示"),
                        gamePlayer.TeamPlayer1.name + LanguageManage.SwitchStr(" 属性:") +
                        LanguageManage.SwitchStr(gamePlayer.TeamPlayer1.attributeType.ToString())
                        + "," + LanguageManage.SwitchStr("可能会受怪物属性克制") + "," + LanguageManage.SwitchStr("是否确定继续挑战？"),
                        CareType.属性提醒);
                    isCare2 = true;
                }
            }
            if (isCare0 || isCare1 || isCare2) { }
            else
            {
                Exploring();
            }

        }
        else
        { 
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("体力不足"),
                LanguageManage.SwitchStr("体力不足，不能探险，在床上休息后可恢复体力！"));
        }
    }

    private async void DisplayBattleMapItem()
    {
        foreach (Transform child in itemParent)
        {
            Destroy(child.gameObject);
        }
        List<int> monsterIds=new List<int>();
        foreach (var selectMapMapMonster in selectMap.mapMonsters)
        {
            foreach (var i in selectMapMapMonster.BattleMnonsters)
            {
                if (!monsterIds.Contains(i.monsterId0)&&i.monsterId0!=0)
                {
                    monsterIds.Add(i.monsterId0);
                }
                if (!monsterIds.Contains(i.monsterId1) && i.monsterId1 != 0)
                {
                    monsterIds.Add(i.monsterId1);
                }
                if (!monsterIds.Contains(i.monsterId2) && i.monsterId2 != 0)
                {
                    monsterIds.Add(i.monsterId2);
                }
            }
        }
        List<int> items=new List<int>();
        foreach (var monsterId in monsterIds)
        {//临时
            /*
            MonsterData monsterData =
                GameComponentData.gameData.monsterManager.MonsterDatas.Find(m => m.id == monsterId);
            foreach (var monsterDataRewardItem in monsterData.RewardItems)
            {
                if (!items.Exists(i => i == monsterDataRewardItem.itemID))
                {
                    items.Add(monsterDataRewardItem.itemID);
                }
            }*/
        }
        foreach (var item in items)
        {
            GameObject itemIconObj = Instantiate(itemIconPro);
            ItemData itemData = await GameDataManager.instance.GetAsyncObjectData<ItemData>(item);
            itemIconObj.GetComponent<Image>().sprite = itemData.iconSprite;
            itemIconObj.transform.SetParent(itemParent,false);
        }

    }
   public void Exploring()
    {
        AudioController.instance.PlayAudio(BGM.Move); 
        GameComponentData.gameData.gameManager.gamePlayer.property.Power -= 5;
        GameComponentData.gameData.gameManager.UpDataPlayer();
        GameComponentData.gameData.mapParent.gameObject.SetActive(false);
        FightPanelObj.SetActive(true);
        FightPanelObj.GetComponent<FightPanelAction>().InitFightData(selectMap);
        GameComponentData.gameData.gameManager.StartFight();
        GameComponentData.gameData.BattleMapAction.InitBattleData(selectMap, false);
        gameObject.SetActive(false);

        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        switch (gamePlayer.attributeType)
        {
                case AttributeType.光:
                    gamePlayer.SkillId = 10400;
                break;
                case AttributeType.冰:
                    gamePlayer.SkillId = 10200;
                break;
                case AttributeType.暗:
                    gamePlayer.SkillId = 10500;
                break;
                case AttributeType.火:
                    gamePlayer.SkillId = 10100;
                break;
                case AttributeType.风:
                    gamePlayer.SkillId = 10300;
                break;
        }


        if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0)
        {
            NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer0.id);
            if (npcx != null)
            {
                if (!npcx.istteamExpAdd)
                {
                    npcx.AddFriendlyexp(2);
                    npcx.istteamExpAdd = true;
                }
            }
        }
        if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0)
        {
            NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer1.id);
            if (npcx != null)
            {
                if (!npcx.istteamExpAdd)
                {
                    npcx.AddFriendlyexp(2);
                    npcx.istteamExpAdd = true;
                }
            }
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}
