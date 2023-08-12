using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SalesetPanelAction : MonoBehaviour
{
    public List<Text> titles;

    public Text saleText;
    [HideInInspector]
    public Text saleText1;

    public Button AddButton;

    public Button ReduceButton;
    
	// Use this for initialization
	void Start () {
	    foreach (var title in titles)
	    {
	        LanguageManage.TextFanyi(title);
	    }
	}

    public void InitData(Text _saleText1)
    {
        saleText1 = _saleText1;
        int x = GameComponentData.gameData.shopGoldDeskAction.saleValue;
        if (x >= 100)
        {
            AddButton.interactable = false;
        }
        else if(x<=5)
        {
            ReduceButton.interactable = false;
        }
        saleText.text = x + "%";
        saleText1.text = x + "%";
    }

    public void AddSale()
    {
        AudioController.instance.PlayAudio(SE.click);
        int x = GameComponentData.gameData.shopGoldDeskAction.saleValue;
        
        x += 5;
        AddButton.interactable = true;
        ReduceButton.interactable = true;
        if (x >= 100)
        {
            AddButton.interactable = false;
        }
        else if (x <= 5)
        {
            ReduceButton.interactable = false;
        }
        GameComponentData.gameData.shopGoldDeskAction.saleValue = x;
        saleText.text = x + "%";
        saleText1.text = x + "%";
    }
    public void ReduceSale()
    {
        AudioController.instance.PlayAudio(SE.click);
        int x = GameComponentData.gameData.shopGoldDeskAction.saleValue;
        x -= 5;
        AddButton.interactable = true;
        ReduceButton.interactable = true;
        if (x >= 100)
        {
            AddButton.interactable = false;
        }
        else if (x <= 5)
        {
            ReduceButton.interactable = false;
        }
        GameComponentData.gameData.shopGoldDeskAction.saleValue = x;
        saleText.text = x + "%";
        saleText1.text = x + "%";
    }
    // Update is called once per frame
    void Update () {
		
	}
}
