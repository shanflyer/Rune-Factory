using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PastureGrassAddPanelAction : MonoBehaviour
{
    public Text TitleText, TouText, MeiriText, ShengyuText, ZhuijiaText,YesButtonText;
    public Text pastureName,pastureGrass,dailyCost,totalCount;

    public InputField addValue;

    public Button reduceButton0, reduceButton1, addButton0, addButton1;

    private int grassCount,addgrass;
    private Pasture pasture;
    private Item grass;
	// Use this for initialization
	void Start ()
	{
	    TitleText.text = LanguageManage.SwitchStr(TitleText.text);
        LanguageManage.TextFanyi(TouText);
        LanguageManage.TextFanyi(MeiriText);
        LanguageManage.TextFanyi(ShengyuText);
        LanguageManage.TextFanyi(ZhuijiaText);
        LanguageManage.TextFanyi(YesButtonText);
	}

    public void InitPanelData(Pasture _pasture)
    {
        addgrass = 0;
        addValue.text =addgrass.ToString();
        pasture = _pasture;
        pastureName.text = pasture.name;
        pastureGrass.text = pasture.grassCount.ToString();
        if (pasture.Animals == null)
        {
            dailyCost.text = "0";
        }
        else
        {
            dailyCost.text = pasture.Animals.Count.ToString();
        }
        
        grass =
            GameComponentData.gameData.gameManager.gamePlayer.package.items.Find(i => i.ItemId / 1000 == 1018);
        if (grass != null)
        {
            grassCount = grass.count;
        }
        else
        {
            grassCount = 0;
        }
        totalCount.text = grassCount.ToString();
        reduceButton0.interactable = false;
        reduceButton1.interactable = false;
        if (grassCount == 0)
        {
            addButton0.interactable = false;
            addButton1.interactable = false;
        }
        else
        {
            addButton0.interactable = true;
            addButton1.interactable = true;
        }
    }

    public void GrassChangeInput(InputField inputValue)
    {
        int value = int.Parse(inputValue.text);
        if (value < 0)
        {
            value = 0;
            reduceButton0.interactable = false;
            reduceButton1.interactable = false;
            if (grassCount > 0)
            {
                addButton0.interactable = true;
                addButton1.interactable = true;
            }
        }
        else if(value>= grassCount)
        {
            value = grassCount;
            addButton0.interactable = false;
            addButton1.interactable = false;
            reduceButton0.interactable = true;
            reduceButton1.interactable = true;
        }
        addgrass = value;
        inputValue.text = value.ToString();
    }

    public void GrassChange(int value)
    {
        
        AudioController.instance.PlayAudio(SE.click);
        if (value > 0)
        {
            addgrass += value;
            if ((value + addgrass) >=grassCount)
            {
                addgrass = grassCount;
                addButton0.interactable = false;
                addButton1.interactable = false;
                reduceButton0.interactable = true;
                reduceButton1.interactable = true;
            }
            addValue.text = addgrass.ToString();
        }
        else
        {
            if (addgrass < -value)
            {
                addgrass = 0;
                addValue.text = "0";
                reduceButton0.interactable = false;
                reduceButton1.interactable = false;
                if (grassCount > 0)
                {
                    addButton1.interactable = true;
                    addButton0.interactable = true;
                }
            }
            else
            {
                addgrass--;
                addValue.text = addgrass.ToString();
                if (addgrass > 0)
                {
                    reduceButton0.interactable = true;
                    reduceButton1.interactable = true;
                }
                else
                {
                    reduceButton0.interactable = false;
                    reduceButton1.interactable = false;
                }
                addButton1.interactable = true;
                addButton0.interactable = true;
            }
        }
    }

    public void EnterGrass()
    {
        
        AudioController.instance.PlayAudio(SE.click);
        if (grass != null)
        {
            GameComponentData.gameData.gameManager.gamePlayer.package.GetItemOutPackage(grass.ItemId,addgrass);
            
        }
        
        pasture.grassCount += addgrass;
        GameComponentData.gameData.pasturePanelAction.InitPasturePanelData(pasture);
        gameObject.SetActive(false);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
