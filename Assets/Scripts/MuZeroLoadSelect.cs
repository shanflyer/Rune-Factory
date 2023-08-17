using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuZeroLoadSelect : MuSaveAction
{

    public GameObject moving;
    public Toggle AutoToggle;

    public GameObject AutoObj;

    public Image MealImage, fealImage;

    public Text NameText, LevelText, MoneyText, DateText, SaveTimeText;
    
	// Use this for initialization
	void Start ()
	{
	    AutoToggle.isOn = true;
	    
    }

    public void InitSaveData()
    {
       Initdata();
        DisplayZeroPanel();

    }
    void DisplayZeroPanel()
    {
        if (DataSaveAndLoadTest.LoadUserData())
        {
            AutoObj.SetActive(true);
            GameSaveData gameSaveData = DataSaveAndLoadTest.gameSaveData;
            if (gameSaveData.playerSaveData.gender == Gender.male)
            {
                MealImage.enabled = true;
                fealImage.enabled = false;
            }
            else
            {
                MealImage.enabled = false;
                fealImage.enabled = true;
            }
            LevelText.text = "Lv." + gameSaveData.playerSaveData.level;
            NameText.text = gameSaveData.playerSaveData.name;
            MoneyText.text = gameSaveData.playerMoneyData.money0.ToString();
            DateText.text =gameSaveData.dateData.date+"/"+ LanguageManage.SwitchStr(gameSaveData.dateData.season.ToString())
                            +"/"+ gameSaveData.dateData.year;
            SaveTimeText.text = gameSaveData.saveTime1.value;
            StartButton.interactable = true;
            CopyButton.interactable = true;
        }
        else
        {
            AutoObj.SetActive(false);
            StartButton.interactable = false;
            CopyButton.interactable = false;
        }
        DeleteButton.interactable = false;

    }

    public void SelectAutoData(Toggle toggle)
    {
        if (toggle.isOn)
        {
            SelectedIndex = -1;
            DeleteButton.interactable = false;
            StartButton.interactable = AutoObj.activeSelf;
        }
        
    }
    
    public void CopyData()
    {
        if (!DataObj0.activeSelf)
        {
            if (SelectedIndex == -1)
            {
                DataSaveAndLoadTest.CreatSaveData(0);
               
            }
            else 
            {
                DataSaveAndLoadTest.CopySaveData(SelectedIndex, 0);
            }
            DisplaySavePanel(0);
        }
        else if(!DataObj1.activeSelf)
        {
            if (SelectedIndex == -1)
            {
                DataSaveAndLoadTest.CreatSaveData(1);
            }
            else
            {
                DataSaveAndLoadTest.CopySaveData(SelectedIndex, 1);
            }
            DisplaySavePanel(1);
        }
        else if (!DataObj2.activeSelf)
        {
            if (SelectedIndex == -1)
            {
                DataSaveAndLoadTest.CreatSaveData(2);
            }
            else
            {
                DataSaveAndLoadTest.CopySaveData(SelectedIndex, 2);
            }
            DisplaySavePanel(2);
        }
        else
        {
            //zeroSceneStart.InitTipsData(LanguageManage.SwitchStr("提示"), LanguageManage.SwitchStr("没有空白存档，无法复制！"));
        }
    }

    public void StartAction()
    {
        if (SelectedIndex == 0)
        {
            DataSaveAndLoadTest.LoadSaveData(0);
        }
        else if (SelectedIndex == 1)
        {
            DataSaveAndLoadTest.LoadSaveData(1);
        }
        else if (SelectedIndex == 2)
        {
            DataSaveAndLoadTest.LoadSaveData(2);
        }
        else if (SelectedIndex == -1)
        {
            DataSaveAndLoadTest.LoadUserData();
        }
        DataSaveAndLoadTest.isJsonData = true;

        moving.SetActive(true);
        moving.GetComponent<LoadingAction>().InitData(false);

        transform.parent.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update () {
		
	}
}
