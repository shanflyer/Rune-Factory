using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[System.Serializable]
public class MuSaveAction : MonoBehaviour
{
    public Image mealImage0, mealImage1, mealImage2;
    public Image femealImage0, femealImage1, femealImage2;
    public Text LevelText0, LevelText1, LevelText2;

    public Text NameText0, NameText1, NameText2;

    public Text MoneyText0, MoneyText1, MoneyText2;
    public Text DateText0, DateText1, DateText2;
    public Text SaveTimeText0, SaveTimeText1, SaveTimeText2;
    public Toggle SaveToggle0, SaveToggle1, SaveToggle2;
    
    public GameObject DataObj0, DataObj1, DataObj2;
    public Button DeleteButton, StartButton;
    public Button CopyButton;
    public int SelectedIndex;
    public void Initdata()
    {
        SelectedIndex = -1;
        if (SceneManager.instance.Now=="001")
        {
            SaveData();
        }
        
        SelectedIndex = 0;
        DisplaySavePanel(0);
        DisplaySavePanel(1);
        DisplaySavePanel(2);
        DataSaveAndLoadTest.LoadUserData();
        SaveToggle0.isOn = true;
        DeleteButton.interactable = DataObj0.activeSelf;
    }

    public void DisplaySavePanel(int index)
    {
        if (index == 0) {
            if (DataSaveAndLoadTest.LoadSaveData(0))
            {
                DataObj0.SetActive(true);
                GameSaveData gameSaveData = DataSaveAndLoadTest.gameSaveData;
                if (gameSaveData.playerSaveData.gender == Gender.male)
                {
                    mealImage0.enabled = true;
                    femealImage0.enabled = false;
                }
                else
                {
                    mealImage0.enabled = false;
                    femealImage0.enabled = true;
                }
                LevelText0.text = "Lv." + gameSaveData.playerSaveData.level;
                NameText0.text = gameSaveData.playerSaveData.name;
                MoneyText0.text = gameSaveData.playerMoneyData.money0.ToString();
                DateText0.text = gameSaveData.dateData.date + "/" + LanguageManage.SwitchStr(gameSaveData.dateData.season.ToString())
                                 + "/" + gameSaveData.dateData.year;
                SaveTimeText0.text = gameSaveData.saveTime1.value;
            }
            else
            {

                DataObj0.SetActive(false);
            }
            DeleteButton.interactable = DataObj0.activeSelf;
            if (StartButton != null)
            {
                StartButton.interactable = DataObj0.activeSelf;
            }
            if (CopyButton != null)
            {
                CopyButton.interactable = DataObj0.activeSelf;
            }

        }
        else if(index==1)
        {
            if (DataSaveAndLoadTest.LoadSaveData(1))
            {
                DataObj1.SetActive(true);
                GameSaveData gameSaveData = DataSaveAndLoadTest.gameSaveData;
                if (gameSaveData.playerSaveData.gender == Gender.male)
                {
                    mealImage1.enabled = true;
                    femealImage1.enabled = false;
                }
                else
                {
                    mealImage1.enabled = false;
                    femealImage1.enabled = true;
                }
                LevelText1.text = "Lv." + gameSaveData.playerSaveData.level;
                NameText1.text = gameSaveData.playerSaveData.name;
                MoneyText1.text = gameSaveData.playerMoneyData.money0.ToString();
                DateText1.text = gameSaveData.dateData.date + "/" + LanguageManage.SwitchStr(gameSaveData.dateData.season.ToString())
                                 + "/" + gameSaveData.dateData.year; 
                SaveTimeText1.text = gameSaveData.saveTime1.value;
            }
            else
            {
                DataObj1.SetActive(false);
            }
            DeleteButton.interactable = DataObj1.activeSelf;
            if (StartButton != null)
            {
                StartButton.interactable = DataObj1.activeSelf;
            }
            if (CopyButton != null)
            {
                CopyButton.interactable = DataObj1.activeSelf;
            }
        }
        else if (index == 2)
        {
            if (DataSaveAndLoadTest.LoadSaveData(2))
            {
                DataObj2.SetActive(true);
                GameSaveData gameSaveData = DataSaveAndLoadTest.gameSaveData;
                if (gameSaveData.playerSaveData.gender == Gender.male)
                {
                    mealImage2.enabled = true;
                    femealImage2.enabled = false;
                }
                else
                {
                    mealImage2.enabled = false;
                    femealImage2.enabled = true;
                }
                LevelText2.text = "Lv." + gameSaveData.playerSaveData.level;
                NameText2.text = gameSaveData.playerSaveData.name;
                MoneyText2.text = gameSaveData.playerMoneyData.money0.ToString();
                DateText2.text = gameSaveData.dateData.date + "/" + LanguageManage.SwitchStr(gameSaveData.dateData.season.ToString())
                                 + "/" + gameSaveData.dateData.year;
                SaveTimeText2.text = gameSaveData.saveTime1.value;
            }
            else
            {
                DataObj2.SetActive(false);
            }
            DeleteButton.interactable = DataObj2.activeSelf;
            if (StartButton != null)
            {
                StartButton.interactable = DataObj2.activeSelf;
            }
            if (CopyButton != null)
            {
                CopyButton.interactable = DataObj2.activeSelf;
            }
        }
 
    }
    public void SaveData()
    {
        DataSaveAndLoadTest.gameSaveData.SaveData();
        DataSaveAndLoadTest.CreatSaveData(SelectedIndex);
        DisplaySavePanel(SelectedIndex);

    }
    
    public void SelectIndex(Toggle toggle)
    {
        if (toggle.isOn)
        {
            if (toggle == SaveToggle0)
            {
                SelectedIndex = 0;
                DeleteButton.interactable = DataObj0.activeSelf;
                if (StartButton != null)
                {
                    StartButton.interactable= DataObj0.activeSelf;
                }
                if (CopyButton != null)
                {
                    CopyButton.interactable = DataObj0.activeSelf;
                }
            }
            if (toggle == SaveToggle1)
            {
                SelectedIndex = 1;
                DeleteButton.interactable = DataObj1.activeSelf;
                if (StartButton != null)
                {
                    StartButton.interactable = DataObj1.activeSelf;
                }
                if (CopyButton != null)
                {
                    CopyButton.interactable = DataObj1.activeSelf;
                }
            }
            if (toggle == SaveToggle2)
            {
                SelectedIndex = 2;
                DeleteButton.interactable = DataObj2.activeSelf;
                if (StartButton != null)
                {
                    StartButton.interactable = DataObj2.activeSelf;
                }
                if (CopyButton != null)
                {
                    CopyButton.interactable = DataObj2.activeSelf;
                }
            }
        }
       
    }
	// Use this for initialization
	void Start () {
		
	}

    public void DeleteData()
    {
        DataSaveAndLoadTest.DeletaSaveData(SelectedIndex);
        DisplaySavePanel(SelectedIndex);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
