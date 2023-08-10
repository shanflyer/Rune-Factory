
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlacePanelAction : MonoBehaviour
{
 

    public Text NameText,TypeText;
    public Button SelectButton;

    public Image background;
    private BatteleMap batteleMap;
	// Use this for initialization
	void Start ()
	{

	}
    public void InitData(BatteleMap _batteleMap)
    {
        batteleMap = _batteleMap;
        NameText.text = _batteleMap.mapName;
        TypeText.text = "("+LanguageManage.SwitchStr(batteleMap.battleMapType.ToString()) + ")";
        switch (batteleMap.battleMapType)
        {
            case BattleMapType.初阶:
                background.color = new Color(0.44f, 1, 0.55f);
                break;
            case BattleMapType.中阶:
                background.color = new Color(0.36f, 0.8f, 1f);
                break;
            case BattleMapType.高阶:
                background.color = new Color(1f, 0.62f, 0.55f);
                break;
        }
        if (batteleMap.isOpen)
        {
            SelectButton.interactable = true;
        }
        else
        {
            SelectButton.interactable = false;
        }
    }
   
    public void ClickToggleButton()
    {
        AudioManager.PlaySelect();
        GetComponentInChildren<Toggle>().isOn = !GetComponentInChildren<Toggle>().isOn;
        if (GetComponentInChildren<Toggle>().isOn)
        {
            GameComponentData.gameData.adventurePanelAction.SelectBattelMap(batteleMap);
        }
    }
    
	// Update is called once per frame
	void Update () {
		
	}
}
