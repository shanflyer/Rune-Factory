using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveReference : UIObjReference
{
    [SerializeField]
    Toggle SelectToggle;
    [SerializeField]
    Image Meal,Femeal;
    [SerializeField]
    Text Name;
    [SerializeField]
    Text Level;
    [SerializeField]
    Text Money; 
    [SerializeField]
    Text Time;
    [SerializeField]
    Text SaveTime;

    GameSaveData gameSaveData;
    int DataIndex = 0;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        SelectToggle = GetComponent<Toggle>();
        Meal = FindChildGameObject<Image>("Meal");
        Femeal = FindChildGameObject<Image>("Femeal");
        Name = FindChildGameObject<Text>("Name");
        Level = FindChildGameObject<Text>("Level");
        Money = FindChildGameObject<Text>("Money");
        Time = FindChildGameObject<Text>("Time");
        SaveTime = FindChildGameObject<Text>("SaveTime");
    }
    public void Refresh(GameSaveData gameSaveData,int index)
    {
        this.DataIndex = index;
        this.gameSaveData = gameSaveData;
        if (gameSaveData != null)
        {
            Meal.enabled = gameSaveData.playerSaveData.gender==Gender.male;
            Femeal.enabled = gameSaveData.playerSaveData.gender == Gender.female;


            Level.text =GameCommon.AddString("Lv." ,gameSaveData.playerSaveData.level.ToString());
            Name.text = gameSaveData.playerSaveData.name;
            Money.text = gameSaveData.playerMoneyData.money0.ToString();
            Time.text =$"{gameSaveData.dateData.date}/{LanguageManage.SwitchStr(gameSaveData.dateData.season.ToString())}/{gameSaveData.dateData.year}";
            SaveTime.text = gameSaveData.saveTime1.value;
        }
        else
        {
            Meal.enabled = false;
            Femeal.enabled = false;
            Level.text = "-";
            Name.text = "-";
            Money.text = "-";
            Time.text = "-";
            SaveTime.text = "-";
        }
    }
    public void SetToggleGroup(ToggleGroup toggleGroup)
    {
        SelectToggle.group = toggleGroup;
    }
    void SelectAction(bool value)
    {
        if (value)
        {
           var selectLoadPanel=  UIManager.instance.GetGamePanel<SelectLoadPanel>();
            selectLoadPanel.RefreshDataFuncButton(DataIndex,gameSaveData==null);
        }
    }
  
    public void Awake()
    {
        SelectToggle.onValueChanged.AddListener(SelectAction);
    }
}
