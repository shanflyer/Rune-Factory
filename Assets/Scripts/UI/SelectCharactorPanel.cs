
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading.Tasks;

public class SelectCharacterPanel : GamePanel
{
    [SerializeField]
    Dropdown SeasonDropdown, DateDropdown;
    [SerializeField]
    InputField NameInputField;
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
        SeasonDropdown = FindChildGameObject<Dropdown>("Season");
        DateDropdown = FindChildGameObject<Dropdown>("Date");
        NameInputField = FindChildGameObject<InputField>("NameInputField");
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
            AudioController.instance.PlayAudio(SE.select); 
            gender = Gender.male;
        });
        FemealSelect.onValueChanged.AddListener((bool value) =>
        {
            AudioController.instance.PlayAudio(SE.select);
            gender = Gender.female;
        });


        SeasonDropdown.options.Clear();
        for (int i = 0; i < Enum.GetValues(typeof(Season)).Length; i++)
        {
            var season = Enum.GetValues(typeof(Season)).GetValue(i).ToString();
            SeasonDropdown.options.Add(new Dropdown.OptionData(season));
        }
        SeasonDropdown.onValueChanged.AddListener(SelectSeason);

        DateDropdown.options.Clear();
        for(int i = 1; i <= GameCommon.SeasonDays; i++)
        {
            DateDropdown.options.Add(new Dropdown.OptionData(i.ToString()));
        }
        DateDropdown.onValueChanged.AddListener(SelectDate); 
        NameInputField.onValueChanged.AddListener(NameInputAction);

        Ok.onClick.AddListener(OkButtonAction);
        Return.onClick.AddListener(()=> {
            AudioController.instance.PlayAudio(SE.Return);
            Close();
            UIManager.instance.ShowGamePanel<ZeroPanel>();
        });

        InitData();
    }

    public override Task InitData(int dataId)
    {
        return base.InitData(dataId);
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
        string nameStr = InputFieldAction.Ctr(value, 12);
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

        DataSaveAndLoadTest.isJsonData = false;
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
        PlayerDate.InitPlayerData(playerName,1,gender,brothSeason,brothDate,false,false,false);
        DataSaveAndLoadTest.IniteZerodata();
        UIManager.instance.ShowGamePanel<CharacterSelectInformationPanel>(layer: 3);

    }
	
}
