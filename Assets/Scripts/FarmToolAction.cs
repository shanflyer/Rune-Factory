using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FarmToolAction : MonoBehaviour
{
    public Text TitleText, seedText;
    public Text SelectButtonText;
    

    public Image SeedImage;

    public Sprite DefaultSeedSprite;

    public Toggle ChuToggle, ShuiToggle, SeedToggle;
	// Use this for initialization
	void Start ()
	{
	    TitleText.text = LanguageManage.SwitchStr(TitleText.text);
	    seedText.text = LanguageManage.SwitchStr(seedText.text);
	    SelectButtonText.text = LanguageManage.SwitchStr(SelectButtonText.text);
	}

    public void SetTitleText()
    {
        
    }

    public void ClickChuButton()
    {
        ChuToggle.isOn = !ChuToggle.isOn;
    }
    public void ClickShuiButton()
    {
        ShuiToggle.isOn = !ShuiToggle.isOn;
    }
    public void ClickZhongButton()
    {
        SeedToggle.isOn = !SeedToggle.isOn;
    }
    public void ClieckedToggle(Toggle toggle)
    {
        if (toggle.isOn)
        {
            AudioManager.PlaySE(PlayType.ONCE,"Select");
            if (toggle == ChuToggle)
            {
                GameComponentData.gameData.farmAction.SelectFarmToolType(1);
                TitleText.text = LanguageManage.SwitchStr("锄头");
            }
            else if(toggle==ShuiToggle)
            {
                GameComponentData.gameData.farmAction.SelectFarmToolType(2);
                TitleText.text = LanguageManage.SwitchStr("洒水器");
            }
            else
            {
                GameComponentData.gameData.farmAction.SelectSeed();
            }
        }
    }

  
    public void DefaultSeedAction()
    {
        SeedImage.sprite = DefaultSeedSprite;
        seedText.text = TitleText.text = LanguageManage.SwitchStr("未选择");
    }
	// Update is called once per frame
	void Update () {
		
	}
}
