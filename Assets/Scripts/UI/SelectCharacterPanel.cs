using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct SelectCharacterData:IReferenceData
{
    public string name;
    public Gender gender;
    public Season brithSeason;
    public int brithDay;
}
public class SelectCharacterPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private TMP_Dropdown SeasonDropdown, DateDropdown;

    [SerializeField]
    private TMP_InputField NameInputField;

    [SerializeField]
    private Toggle MealSelcet;

    [SerializeField]
    private Toggle FemealSelect;

    [SerializeField]
    private Button Ok, Return;
     

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
            if (value && gender != Gender.male)
            { 
                gender = Gender.male;
                PlayFilm playFilm = new PlayFilm
                {
                    filmName = "角色选择",
                    assetName = "SelectMeal",
                };
                GameActionManager.instance.QueueAction(playFilm);
                RefreshName();
            }
        });
        FemealSelect.onValueChanged.AddListener((bool value) =>
        {
            if (value && gender != Gender.female)
            { 
                gender = Gender.female;
                PlayFilm playFilm = new PlayFilm
                {
                    filmName = "角色选择",
                    assetName = "SelectFeMeal",
                };
                GameActionManager.instance.QueueAction(playFilm);
                RefreshName();
            }
        });

        SeasonDropdown.options.Clear();
        for (int i = 0; i < Enum.GetValues(typeof(Season)).Length; i++)
        {
            var season = Enum.GetValues(typeof(Season)).GetValue(i);
            if ((int)season > 0)
            {
                var seasonStr = season.ToString();
                SeasonDropdown.options.Add(new TMP_Dropdown.OptionData(seasonStr));
            }
        }
        SeasonDropdown.onValueChanged.AddListener(SelectSeason);
        SeasonDropdown.value = 1;

        DateDropdown.options.Clear();
        for (int i = 1; i <= GameCommon.SeasonDays; i++)
        {
            DateDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }
        DateDropdown.onValueChanged.AddListener(SelectDate);
        DateDropdown.value = 0;
        NameInputField.onValueChanged.AddListener(NameInputAction);

        Ok.onClick.AddListener(OkButtonAction);
        Return.onClick.AddListener(async () =>
        { 
            Close();
            GameActionManager.instance.QueueAction(new StopFilm
            {
                filmName = "角色选择"
            });
           var zeroPanel= await UIManager.instance.ShowGamePanel<ZeroPanel>(); 
            zeroPanel.PlayZeroBGM();
        });

        InitData();
    }

    public override Task InitData(string dataKay)
    {
        AudioController.instance.StopBgm();
        Shader.SetGlobalVector("_PlayerPos", Vector3.zero);
        return base.InitData(dataKay);
    }

    private void SelectDate(int index)
    { 
        brothDate = index + 1;
    }

    private void SelectSeason(int index)
    { 
        index++;
        brothSeason = (Season)index;
    }

    public void NameInputAction(string value)
    {
        string nameStr = value;
        playerName = nameStr;
        userChangeName = true;
    }

    const string boyName = "亚历克斯";
    const string girlName = "艾丽西亚";
    void RefreshName()
    {
        if (!userChangeName)
        {
            if(gender != Gender.female)
            {
                playerName = LanguageManage.SwitchStr(boyName);
            }
            else
            {
                playerName = LanguageManage.SwitchStr(girlName);
            }
            NameInputField.SetTextWithoutNotify(playerName);
        }
    }
    bool userChangeName = false;
    private void InitData()
    {
      
        userChangeName = false; 
        foreach (var optionData in SeasonDropdown.options)
        {
            optionData.text = LanguageManage.SwitchStr(optionData.text); 
        }

        //DataSaveAndLoadTest.isJsonData = false; 
        gender = Gender.male;
        RefreshName();
        brothSeason = Season.春;
        brothDate = 1;
        SeasonDropdown.value = 0;
        DateDropdown.value = 0;
    }

    private void OkButtonAction()
    {
        AsyncTaskRunner.Run(OkButtonActionAsync, nameof(OkButtonAction));
    }

    private async System.Threading.Tasks.Task OkButtonActionAsync()
    { 
        
        //DataSaveAndLoadTest.IniteZerodata();
       await UIManager.instance.ShowGamePanel<CharacterSelectInformationPanel,SelectCharacterData>(new SelectCharacterData
       {
           name=playerName,
           brithDay=brothDate,
           brithSeason=brothSeason,
           gender=gender
       },layer: 3);
    }
}
