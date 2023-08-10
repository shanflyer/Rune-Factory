using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GoldCostSelect : MonoBehaviour
{
    public List<Text> Texts;
    public Text TitleText;

    public Text CostValueText;
    public Image MoneyImage0, MoneyImage1,TotalMonet0,TotalMonet1;
    public Text noticeText;
    public Text TotalText;
    
	// Use this for initialization
	void Start () {
	    foreach (var text in Texts)
	    {
	        LanguageManage.TextFanyi(text);
	    }
	}

    public void InitGoldCostData(string Title,int _costValue,string notice,ShopMoneyType shopMoneyType)
    {
        TitleText.text = Title;
        CostValueText.text = _costValue.ToString();
        noticeText.text = notice;
        if (shopMoneyType == ShopMoneyType.红晶)
        {
            
            TotalText.text = GameComponentData.gameData.gameManager.gamePlayer.money1.ToString();
            MoneyImage0.enabled = false;
            TotalMonet0.enabled = false;
            MoneyImage1.enabled = true;
            TotalMonet1.enabled = true;
        }
        else
        {
            MoneyImage0.enabled = true;
            TotalMonet0.enabled = true;
            MoneyImage1.enabled = false;
            TotalMonet1.enabled = false;

            TotalText.text = GameComponentData.gameData.gameManager.gamePlayer.money.ToString();
        }
       
    }

    
	// Update is called once per frame
	void Update () {
		
	}
}
