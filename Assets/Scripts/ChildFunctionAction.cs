using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum GrowStatus
{
    生长停滞=0,
    慢速成长=1,
    中速成长=2,
    高速成长=3,
}
public class ChildFunctionAction : MonoBehaviour
{
    public InputField NameInputField;
    public Text BodyText, MindText, BodyStatusText, MindStatusText;

    public ValueSetDisplay FoodValueSetDisplay, MoodValueSetDisplay, CleanValueSetDisplay;

    private ChildData childData;
    public GameObject milkObj, niaobuObj, flyObj;
    public Image PlayerImage0, PlayerImage1, PlayerImage2;
    private GameObject displayObj;
    private string displayText;
    public void ChangeName()
    {
        NameInputField.text = InputFieldAction.Ctr(NameInputField.text,15);
        GameComponentData.gameData.gameManager.childData.name = NameInputField.text;
        DataSaveAndLoadTest.gameSaveData.marryData.SetChildData();
    }
    public void DisplayChildData()
    {
        childData = GameComponentData.gameData.gameManager.childData;
        NameInputField.text = childData.name;
        BodyText.text = childData.bodyValue.ToString();
        MindText.text = childData.mindValue.ToString();
        BodyStatusText.text = childData.BodyGrowStatus.ToString();
        MindStatusText.text = childData.MindGrowStatus.ToString();

        FoodValueSetDisplay.SetValue(childData.foodValue);
        MoodValueSetDisplay.SetValue(childData.moodValue);
        CleanValueSetDisplay.SetValue(childData.cleanValue);

        int bodyGrowValue = Mathf.RoundToInt((childData.foodValue + childData.moodValue) / 2.0f);
        if (childData.foodValue == 0)
        {
            bodyGrowValue = 0;
        }
        int mindGrowValue = Mathf.RoundToInt((childData.cleanValue + childData.moodValue) / 2.0f);
        if (childData.moodValue == 0)
        {
            mindGrowValue = 0;
        }
        if (bodyGrowValue >= 8)
        {
            childData.BodyGrowStatus = GrowStatus.高速成长;
            
        }
        else if (bodyGrowValue >= 4)
        {
            childData.BodyGrowStatus = GrowStatus.中速成长;
            
        }
        else if (bodyGrowValue > 0)
        {
            childData.BodyGrowStatus = GrowStatus.慢速成长;
            
        }
        else
        {
            childData.BodyGrowStatus = GrowStatus.生长停滞;
        }
        if (mindGrowValue >= 8)
        {
            childData.MindGrowStatus = GrowStatus.高速成长;
            
        }
        else if (mindGrowValue >= 4)
        {
            childData.MindGrowStatus = GrowStatus.中速成长;
            
        }
        else if (mindGrowValue > 0)
        {
            childData.MindGrowStatus = GrowStatus.慢速成长;
           
        }
        else
        {
            childData.MindGrowStatus = GrowStatus.生长停滞;
        }


        LanguageManage.TextFanyi(BodyStatusText);
        LanguageManage.TextFanyi(MindStatusText);

        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        PlayerImage2.sprite = PlayerImage1.sprite = PlayerImage0.sprite = 
            GameComponent.charactorIcon.Find(icon => icon.name == gamePlayer.playerImage);
        
    }

    public void MilkAction()
    {
        if (GameComponentData.gameData.gameManager.gamePlayer.package.IsHaveItem(1131))
        {
            GameComponentData.gameData.gameManager.gamePlayer.package.GetItemOutPackage(1131,1);
            displayObj = milkObj;
            displayObj.SetActive(true);
            GameComponentData.gameData.gameManager.childData.foodValue += 2;
            if (GameComponentData.gameData.gameManager.childData.foodValue > 10)
            {
                GameComponentData.gameData.gameManager.childData.foodValue = 10;
            }
            StartCoroutine("Displaying");
            displayText = "宝宝的饮食系数增加了！";
            DataSaveAndLoadTest.gameSaveData.marryData.SetChildData();
        }
        else
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),
                LanguageManage.SwitchStr("缺少牛奶，牛奶可在动物店购买"));
        }
    }

    public void NiaoBuAction()
    {
        if (GameComponentData.gameData.gameManager.gamePlayer.package.IsHaveItem(1266))
        {
            GameComponentData.gameData.gameManager.gamePlayer.package.GetItemOutPackage(1266, 1);
            displayObj = niaobuObj;
            displayObj.SetActive(true);
            GameComponentData.gameData.gameManager.childData.cleanValue += 2;
            if (GameComponentData.gameData.gameManager.childData.cleanValue > 10)
            {
                GameComponentData.gameData.gameManager.childData.cleanValue = 10;
            }
            StartCoroutine("Displaying");
            displayText = "宝宝的清洁系数增加了！";
            DataSaveAndLoadTest.gameSaveData.marryData.SetChildData();
        }
        else
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),
                LanguageManage.SwitchStr("缺少尿布，尿布可在动物店购买"));
        }
    }

    public void ChildFly()
    {
        if (GameComponentData.gameData.gameManager.gamePlayer.package.IsHaveItem(1267))
        {
            GameComponentData.gameData.gameManager.gamePlayer.package.GetItemOutPackage(1267, 1);
            displayObj = flyObj;
            displayObj.SetActive(true);
            GameComponentData.gameData.gameManager.childData.moodValue += 2;
            if (GameComponentData.gameData.gameManager.childData.moodValue > 10)
            {
                GameComponentData.gameData.gameManager.childData.moodValue = 10;
            }
            StartCoroutine("Displaying");
            displayText = "宝宝的情绪系数增加了！";
            DataSaveAndLoadTest.gameSaveData.marryData.SetChildData();
        }
        else
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),
                LanguageManage.SwitchStr("缺少保险绳，保险绳可在动物店购买"));
        }
    }
    IEnumerator Displaying()
    {
        yield return new WaitForSeconds(2.0f);
        displayObj.SetActive(false);
        DisplayChildData();
        AudioController.instance.PlayAudio(SE.Item);

        GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),LanguageManage.SwitchStr(displayText));
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
