using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LianjinAction : MonoBehaviour
{
    public List<Text> Texts;
    public ShopMoneyType shopMoneyType;
    public int Spar0, Spar1, Spar2, Spar3, Spar4, Spar5;
    public int gold0, gold1, gold2, gold3, gold4, gold5;
    public Toggle Toggle0, Toggle1, Toggle2, Toggle3, Toggle4, Toggle5;
   
    public Text GoldText;
    public Button ActionButton;

    private int value,spar;
	// Use this for initialization
	void Start () {
	    foreach (var text in Texts)
	    {
	        LanguageManage.TextFanyi(text);
	    }
	}

    public void InitData()
    {
        Toggle0.isOn = true;
        Toggle1.isOn = false;
        Toggle2.isOn = false;
        Toggle3.isOn = false;
        Toggle4.isOn = false;
        Toggle5.isOn = false;
        GoldText.text = gold0.ToString();
        value = gold0;
        spar = Spar0;
    }

    public void SelectTogle(Toggle _toggle)
    {
        AudioManager.PlaySE(PlayType.ONCE,"Select");
        if (_toggle.isOn)
        {
            if (_toggle == Toggle0)
            {
                value = gold0;
                spar = Spar0;
            }
            if (_toggle == Toggle1)
            {
                value = gold1;
                spar = Spar1;
            }
            if (_toggle == Toggle2)
            {
                value = gold2;
                spar = Spar2;
            }
            if (_toggle == Toggle3)
            {
                value = gold3;
                spar = Spar3;
            }
            if (_toggle == Toggle4)
            {
                value = gold4;
                spar = Spar4;
            }
            if (_toggle == Toggle5)
            {
                value = gold5;
                spar = Spar5;
            }

            GoldText.text = value.ToString();
        }
           
    }

    public void Lianjin()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Shop");
        GameComponentData.gameData.gameManager.ChangePlayerMoney(value);
        GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("炼金"), LanguageManage.SwitchStr("消耗红晶:") + spar + LanguageManage.SwitchStr(",获得金币:") + value);
        GameComponentData.gameData.informationManager.AddInformation(LanguageManage.SwitchStr("*炼金成功") + LanguageManage.SwitchStr("消耗红晶:") + spar + LanguageManage.SwitchStr(",获得金币:") + value);
        gameObject.SetActive(false);
        
    }
    public void ReturnButton()
    {
        AudioManager.PlaySE(PlayType.ONCE,"Return");
        gameObject.SetActive(false);
    }
    public void ActionButtonClick()
    {
        gameObject.SetActive(false);
        GameComponentData.gameData.gameManager.InitCostData(LanguageManage.SwitchStr("炼金"),spar, LanguageManage.SwitchStr(",获得金币:") + value,  CostType.炼金, ShopMoneyType.红晶);

    }
	// Update is called once per frame
	void Update () {
		
	}
}
