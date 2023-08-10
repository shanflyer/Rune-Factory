using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightPanelAction : MonoBehaviour
{
    public GameObject skillPanel;
    public GameObject skillPro;
    public Transform skillParent;
    public Text SkillCounText;
    public Image skillValueImage;
    public Button skillButton;
    public Text ExploreTitalText,RetreatButtonText,AutoButtonText,UsingButtonText,
        goingButtonText,FightButtonText,ItemButtonText,AutoButtonText1,BinSiText0,BinSiText1,BinSiText2;
    public Button FightButton, ItemButton, AutoButton, SkillButton;
    public Text mapName, ExplorValueText;
    public Button startButton, autoButton,ChetuiButton,FoodButton;
    public GameObject AutoObj;
    public GameObject functionButtons;
    public GameObject SkillObj;
    public GameObject informationObj;
    public Transform informaParent0, informaParent1;
    public GameObject FightFunction0, Fightfunction1;

    public GameObject player0, player1, player2;

    public Text Name0, level0, AT0, DF0, Name1, level1, AT1, DF1, Name2, level2, AT2, DF2;

    public Slider Exp0, Hp0, Exp1, Hp1,Exp2, Hp2;
    public Text playerType0, playerType1, playerType2;
    public Image attribute0, attribute1, attribute2;
    public Sprite fire, ice, dark, light, wide;
    public bool isAuto;
    // Use this for initialization

    public void DisplaySkillList()
    {
        List<int> skillIds = GameComponentData.gameData.BattleMapAction.GetPlayerSkillList();
        skillPanel.SetActive(true);
        foreach (Transform child in skillParent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < skillIds.Count; i++)
        {
            int skillId = skillIds[i];
            if (skillId != 0)
            {
                SkillData skill = GameComponentData.gameData.skillManager.skillDatas.Find(s => s.id == skillId);
                GameObject skillObj = Instantiate(skillPro);
                skillObj.transform.SetParent(skillParent, false);
                skillObj.GetComponent<SKillProAction>().InitSkillData(skill, i);
            }
           
        }
       
        GameObject nullSkillObj = Instantiate(skillPro);
        nullSkillObj.transform.SetParent(skillParent, false);
        nullSkillObj.GetComponent<SKillProAction>().InitSkillData(null,-1);
    }

    public void SelectSkill(int index)
    {
        AudioManager.PlaySelect();
        skillPanel.SetActive(false);
        if (index!= -1)
        {
            skillButton.interactable = false;
            GameComponentData.gameData.BattleMapAction.PlayerSkillAction(index);
            SetPlayerSkillValue(0);
        }
        
    }
    void Start ()
    {
        ExploreTitalText.text = LanguageManage.SwitchStr(ExploreTitalText.text);
        RetreatButtonText.text = LanguageManage.SwitchStr(RetreatButtonText.text);
        AutoButtonText.text = LanguageManage.SwitchStr(AutoButtonText.text);
        UsingButtonText.text = LanguageManage.SwitchStr(UsingButtonText.text);
        goingButtonText.text = LanguageManage.SwitchStr(goingButtonText.text);
        FightButtonText.text = LanguageManage.SwitchStr(FightButtonText.text);
        ItemButtonText.text = LanguageManage.SwitchStr(ItemButtonText.text);
        AutoButtonText1.text = LanguageManage.SwitchStr(AutoButtonText1.text);
        BinSiText0.text = LanguageManage.SwitchStr(BinSiText0.text);
        BinSiText1.text = LanguageManage.SwitchStr(BinSiText1.text);
        BinSiText2.text = LanguageManage.SwitchStr(BinSiText2.text);
    }

    public void SetPlayerSkillValue(float value)
    {

        
        if (value >= 1)
        {
            value = 1;
            skillButton.interactable = true;
            SkillCounText.text = "1";
        }
        else
        {
            SkillCounText.text = "0";
            skillButton.interactable = false;
        }
        skillValueImage.fillAmount = value;
    }
    public void FightEnd()
    {
        functionButtons.SetActive(true);
        informationObj.transform.SetParent(informaParent1, true);
        informationObj.transform.localPosition = Vector3.zero;
        gameObject.SetActive(false);

    }
    public void FightDisplayFunctions()
    {
        FightButton.interactable = true;
        ItemButton.interactable = true;
        AutoButton.interactable = true;
        //SkillButton.interactable = true;
    }
    public void FightHideFunctions()
    {
        FightButton.interactable = false;
        ItemButton.interactable = false;
        AutoButton.interactable = true;
        SkillButton.interactable = false;
    }

    public void AutoHideFunctions()
    {
        FightButton.interactable = false;
        ItemButton.interactable = false;
        AutoButton.interactable = true;
        SkillButton.interactable = false;
    }

    public void SetPlayerAttribute(AttributeType attributeType)
    {
        
        SetAttributeIcon(attributeType,attribute0);
    }
    public void SetAttributeIcon(AttributeType attributeType,Image image)
    {
        image.gameObject.SetActive(true);
        switch (attributeType)
        {
            case AttributeType.无:
                image.gameObject.SetActive(false);
                break;
            case AttributeType.光:
                image.sprite = light;
                break;
            case AttributeType.冰:
                image.sprite = ice;
                break;
            case AttributeType.暗:
                image.sprite = dark;
                break;
            case AttributeType.火:
                image.sprite = fire;
                break;
            case AttributeType.风:
                image.sprite = wide;
                break;

        }
    }
    public void AutoFightButton()
    {
        AudioManager.PlaySE(PlayType.ONCE,"Click");
        isAuto = !isAuto;
        AutoObj.SetActive(isAuto);
        if (isAuto)
        {
            AutoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("手动");
            autoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("手动");
            startButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("暂停");
            AutoHideFunctions();
        }
        else
        {
            AutoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("自动");
            autoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("自动");
            startButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("前进");
            FightDisplayFunctions();
        }
        startButton.interactable = !isAuto;
        ChetuiButton.interactable = !isAuto;
        FoodButton.interactable = !isAuto;
        GameComponentData.gameData.BattleMapAction.AutoFightClick(isAuto);
    }
    public void InitFightData(BatteleMap batteleMap)
    {
        informationObj.transform.SetParent(informaParent0, true);
        informationObj.transform.localPosition = Vector3.zero;
        functionButtons.SetActive(false);
        mapName.text = batteleMap.mapName;
        ExplorValueText.text = "0";
        playerType0.gameObject.SetActive(false);
        playerType1.gameObject.SetActive(false);
        playerType2.gameObject.SetActive(false);
        AutoObj.SetActive(false);
        FightFunction0.SetActive(true);
        Fightfunction1.SetActive(false);
        player0.SetActive(true);
        GamePlayer player = GameComponentData.gameData.gameManager.gamePlayer;
        Name0.text = player.name;
        level0.text = "LV."+player.level;
        Exp0.value = player.property.EXP / (float) player.property.NeedEXP;
        Hp0.value = player.property.HP / (float) player.property.MaxHP;
        AT0.text = "AT." + player.property.AT;
        DF0.text = "DF." + player.property.DF;
        isAuto = false;
        SetAttributeIcon(player.attributeType,attribute0);
        if (isAuto)
        {
            autoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("手动");
        }
        else
        {
            autoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("自动");
        }
        TeamPlayer TeamPlayer1 = player.TeamPlayer0;
        if (TeamPlayer1 != null)
        {
            player1.SetActive(true);
            Name1.text = TeamPlayer1.name;
            level1.text = "LV." + TeamPlayer1.level;
            Exp1.value = TeamPlayer1.property.EXP / (float)TeamPlayer1.property.NeedEXP;
            Hp1.value = TeamPlayer1.property.HP / (float)TeamPlayer1.property.MaxHP;
            AT1.text = "AT." + TeamPlayer1.property.AT;
            DF1.text = "DF." + TeamPlayer1.property.DF;
            SetAttributeIcon(TeamPlayer1.attributeType, attribute1);
        }
        else
        {
            player1.SetActive(false);
        }

        TeamPlayer TeamPlayer2 = player.TeamPlayer1;
        if (TeamPlayer2 != null)
        {
            player2.SetActive(true);
            Name2.text = TeamPlayer2.name;
            level2.text = "LV." + TeamPlayer2.level;
            Exp2.value = TeamPlayer2.property.EXP / (float)TeamPlayer2.property.NeedEXP;
            Hp2.value = TeamPlayer2.property.HP / (float)TeamPlayer2.property.MaxHP;
            AT2.text = "AT." + TeamPlayer2.property.AT;
            DF2.text = "DF." + TeamPlayer2.property.DF;
            SetAttributeIcon(TeamPlayer2.attributeType, attribute2);
        }
        else
        {
            player2.SetActive(false);
        }
    }
    public void ClickStartButton()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        AutoObj.SetActive(false);
        isAuto = false;
        autoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("自动");
        GameComponentData.gameData.BattleMapAction.ClickStartButton(startButton);
    }
    public void ClickAutoButton()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        isAuto = !isAuto;
        //GameComponentData.gameData.BattleMapAction.SetMoving(isAuto);
        if (isAuto)
        {
            AutoObj.SetActive(true);
            autoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("手动");
            startButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("暂停");
            AutoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("手动");
           

            startButton.interactable = false;
            ChetuiButton.interactable = false;
            FoodButton.interactable = false;
            AutoHideFunctions();
        }
        else
        {
            AutoObj.SetActive(false);

            startButton.interactable = true;
            ChetuiButton.interactable = true;
            FoodButton.interactable = true;
            startButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("前进");
            autoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("自动");
            AutoButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("自动");
            
            FightDisplayFunctions();

        }
        GameComponentData.gameData.BattleMapAction.ClickAutoButton(isAuto);
    }
    public void UpDataPlayerHp(int index,int addHpValue)
    {
        if (index == 0)
        {
            GamePlayer gamePlayer= GameComponentData.gameData.gameManager.gamePlayer;

            gamePlayer.property.HP += addHpValue;
            if (gamePlayer.property.HP < 0)
            {
                gamePlayer.property.HP = 0;
            }
            if (gamePlayer.property.HP > gamePlayer.property.MaxHP)
            {
                gamePlayer.property.HP = gamePlayer.property.MaxHP;
            }
            Hp0.value = gamePlayer.property.HP / (float) gamePlayer.property.MaxHP;
            GameComponentData.gameData.gameManager.UpDataPlayer();
        }
        else if (index == 1)
        {
            TeamPlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0;

            gamePlayer.AddHpValue(addHpValue);
            Hp1.value = gamePlayer.property.HP / (float)gamePlayer.property.MaxHP;
        }
        else if (index == 2)
        {
            TeamPlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer1;

            gamePlayer.AddHpValue(addHpValue);
            Hp2.value = gamePlayer.property.HP / (float)gamePlayer.property.MaxHP;
        }
    }
    public void UpDataPlayerEXP(int index, int addExpValue)
    {
        if (index == 0)
        {
            GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;

            gamePlayer.level=gamePlayer.property.AddExp(addExpValue,gamePlayer.id/1000,gamePlayer.level);
           

            level0.text = "LV." + gamePlayer.level;
            Exp0.value = (float)gamePlayer.property.EXP / (float)gamePlayer.property.NeedEXP;
            Hp0.value = (float)gamePlayer.property.HP / (float)gamePlayer.property.MaxHP;
            AT0.text = "AT." + gamePlayer.property.AT;
            DF0.text = "DF." + gamePlayer.property.DF;

            GameComponentData.gameData.gameManager.UpDataPlayer();
        }
        else if (index == 1)
        {
            TeamPlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0;
            gamePlayer.AddExpValue(addExpValue);

            Employer employer = GameComponentData.gameData.employerManger.Employers.Find(e => e.id == gamePlayer.id);
            employer.property = gamePlayer.property;
            employer.level = gamePlayer.level;

            level1.text = "LV." + gamePlayer.level;
            Exp1.value = (float)gamePlayer.property.EXP / (float)gamePlayer.property.NeedEXP;
            Hp1.value = (float)gamePlayer.property.HP / (float)gamePlayer.property.MaxHP;
            AT1.text = "AT." + gamePlayer.property.AT;
            DF1.text = "DF." + gamePlayer.property.DF;
            
        }
        else if (index == 2)
        {
            TeamPlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer1;

            gamePlayer.AddExpValue(addExpValue);

            Employer employer = GameComponentData.gameData.employerManger.Employers.Find(e => e.id == gamePlayer.id);
            employer.property = gamePlayer.property;
            employer.level = gamePlayer.level;
            level2.text = "LV." + gamePlayer.level;
            Exp2.value = (float)gamePlayer.property.EXP / (float)gamePlayer.property.NeedEXP;
            Hp2.value = (float)gamePlayer.property.HP / (float)gamePlayer.property.MaxHP;
            AT2.text = "AT." + gamePlayer.property.AT;
            DF2.text = "DF." + gamePlayer.property.DF;
        }
    }

   
    public void GetOutButtonClick()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");

        startButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("前进");
        GameComponentData.gameData.BattleMapAction.SetMoving(false);
        GameComponentData.gameData.gameManager.InitCareSelectData(LanguageManage.SwitchStr("退出冒险"), LanguageManage.SwitchStr("是否确定退出本次冒险？"),CareType.OutBattle);
    }

    public void MovingFunctionButton()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");

        FightFunction0.SetActive(true);
        Fightfunction1.SetActive(false);
        startButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("前进");
    }
    public void BattleFunctionButton()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        FightFunction0.SetActive(false);
        Fightfunction1.SetActive(true);
    }
    public void GetOutAction()
    {

        GameComponentData.gameData.SaveButtonObj.SetActive(true);
        GameComponentData.gameData.mapParent.gameObject.SetActive(true);
        informationObj.transform.SetParent(informaParent1, true);
        informationObj.transform.localPosition = Vector3.zero;
        functionButtons.SetActive(true);
        GameComponentData.gameData.BattleMapAction.GetOutBattled();
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update () {
		
	}
}
