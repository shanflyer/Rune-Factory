
using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlacePanelAction : MonoBehaviour
{
 

    public Text NameText,TypeText;
    public Button SelectButton;

    public Image background;
    //private BatteleMap batteleMap;
	// Use this for initialization
	void Start ()
	{

	}
 
   
    public void ClickToggleButton()
    {
        AudioController.instance.PlayAudio(SE.select);
        GetComponentInChildren<Toggle>().isOn = !GetComponentInChildren<Toggle>().isOn;
        if (GetComponentInChildren<Toggle>().isOn)
        {
           //GameComponentData.gameData.adventurePanelAction.SelectBattelMap(batteleMap);
        }
    }
    
	// Update is called once per frame
	void Update () {
		
	}
}
