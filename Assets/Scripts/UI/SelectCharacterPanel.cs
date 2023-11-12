
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading.Tasks;
using TMPro;

public class SelectCharacterPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    TMP_Dropdown SeasonDropdown, DateDropdown;
    [SerializeField]
    TMP_InputField NameInputField;
    [SerializeField]
    Toggle MealSelcet;
    [SerializeField]
    Toggle FemealSelect;

    [SerializeField]
    Button Ok, Return;

    public GameObject SelecCharactorInformationPanel;
    
    private Gender gender;
    private string playerName;
    private Season brothSeason;
    private int brothDate;


    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        SeasonDropdown = FindChildGameObject<TMP_Dropdown>("Season");
        DateDropdown = FindChildGameObject<TMP_Dropdown>("Date");
        NameInputField = FindChildGameObject<TMP_InputField>("NameInputField");
        MealSelcet = FindChildGameObject<Toggle>("MealSelect");
        FemealSelect = FindChildGameObject<Toggle>("FemealSelect");
        Ok = FindChildGameObject<Button>("OKButton");
        Return = FindChildGameObject<Button>("ReturnButton"); 
    }

    protected override void Awake()
    {
        base.Awake();
        MealSelcet.onValueChanged.AddListener((bool value) =>
        {
            if (value&&gender!=Gender.male)
            {
                AudioController.instance.PlayAudio(SE.select);
                gender = Gender.male;
                PlayFilm playFilm = new PlayFilm
                {
                    filmName = "角色选择",
                    assetName = "SelectMeal",
                };
                GameActionManager.instance.QueueAction(playFilm);
            }
            
        });
        FemealSelect.onValueChanged.AddListener((bool value) =>
        {
            if (value && gender != Gender.female)
            {
                AudioController.instance.PlayAudio(SE.select);
                gender = Gender.female;
                PlayFilm playFilm = new PlayFilm
                {
                    filmName = "角色选择",
                    assetName = "SelectFeMeal",
                };
                GameActionManager.instance.QueueAction(playFilm);
            }
            
        });


        SeasonDropdown.options.Clear();
        for (int i = 0; i < Enum.GetValues(typeof(Season)).Length; i++)
        {
            var season = Enum.GetValues(typeof(Season)).GetValue(i);
            if ((int)season >= 0)
            {
                var seasonStr = season.ToString(); 
                SeasonDropdown.options.Add(new TMP_Dropdown.OptionData(seasonStr));
            }
            
        }
        SeasonDropdown.onValueChanged.AddListener(SelectSeason);
        SeasonDropdown.value = 0;

        DateDropdown.options.Clear();
        for(int i = 1; i <= GameCommon.SeasonDays; i++)
        {
            DateDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }
        DateDropdown.onValueChanged.AddListener(SelectDate);
        DateDropdown.value = 0;
        NameInputField.onValueChanged.AddListener(NameInputAction);

        Ok.onClick.AddListener(OkButtonAction);
        Return.onClick.AddListener(()=> {
            AudioController.instance.PlayAudio(SE.Return);
            Close();
            UIManager.instance.ShowGamePanel<ZeroPanel>();
        });

        InitData();
    }

    public override Task InitData(string dataKay)
    {
        return base.InitData(dataKay);
    }

    void SelectDate(int index)
    {
        AudioController.instance.PlayAudio(SE.select);
        brothDate = index+1;
    }
    void SelectSeason(int index)
    {
        AudioController.instance.PlayAudio(SE.select);
        index++;
         brothSeason = (Season)index; 
    }

    public void NameInputAction(string value)
    {
        string nameStr = value;
        playerName = nameStr;
        NameInputField.text = nameStr;
    }

   

    void InitData()
    {
        AudioController.instance.StopBgm();
        
        
        NameInputField.text = LanguageManage.SwitchStr(NameInputField.text);
        foreach (var optionData in SeasonDropdown.options)
        {
            optionData.text = LanguageManage.SwitchStr(optionData.text);
        }

        //DataSaveAndLoadTest.isJsonData = false;
        AudioController.instance.PlayAudio(SE.click);
        gender = Gender.male;
        playerName = LanguageManage.SwitchStr("亚历克斯");
        brothSeason = Season.春;
        brothDate = 1;
        SeasonDropdown.value = 0;
        DateDropdown.value = 0;
        NameInputField.text = playerName;

    }
    void OkButtonAction()
    {
        AudioController.instance.PlayAudio(SE.click);

        GameDataSaveManager.instance.InitPlayerData(playerName, gender, brothSeason, brothDate); 
        //DataSaveAndLoadTest.IniteZerodata();
        UIManager.instance.ShowGamePanel<CharacterSelectInformationPanel>(layer: 3);

    }
	
}
