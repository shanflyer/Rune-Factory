using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaleAction : MonoBehaviour
{
    public Text SaleValueText;
	// Use this for initialization
	void Start ()
	{
	    int x =  GameComponentData.gameData.shopGoldDeskAction.saleValue;

        SaleValueText.text=x+"%";
    }
    public void ClickOpenListButton()
    {
        GameComponentData.gameData.heritageAction.ClickheritageObjbutton();
        //GameComponentData.gameData.salesetPanelAction.InitData(SaleValueText);
    }
    public void ClickChangeSaleButto()
    {
        GameComponentData.gameData.salesetPanelAction.gameObject.SetActive(true);
        GameComponentData.gameData.salesetPanelAction.InitData(SaleValueText);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
