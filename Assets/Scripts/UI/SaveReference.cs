using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveReference : UIObjReference<UserGameSaveData>
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
    public override void InitData(UserGameSaveData t, SelectAction<UserGameSaveData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        SelectToggle.group = toggleGroup;
        if (string.IsNullOrEmpty(data.saveTime))
        {
            Meal.enabled = data.playerData.gender == Gender.male;
            Femeal.enabled = data.playerData.gender == Gender.female;


            Level.text = GameCommon.AddString("Lv.", data.playerData.level.ToString());
            Name.text = data.playerData.name;
            Money.text = data.otherSaveData.gold.ToString();
            Time.text = $"{data.dateData.day}/{LanguageManage.SwitchStr(data.dateData.season.ToString())}/{data.dateData.year}";
            SaveTime.text = data.saveTime;
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
        SelectToggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data, value);
            }
        });
    }
     
    public void Awake()
    { 
    }
}
