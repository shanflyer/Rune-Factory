using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.UI; 
using System.Threading.Tasks;
using TMPro;

public class CharacterSelectInformationPanel : GamePanel<IReferenceData>
{
    public override bool changeInputModel => false;
    [SerializeField]
    TextMeshProUGUI PlayerText, BrothText;
    [SerializeField]
    Image MealIcon, FemaleIcon;
    [SerializeField]
    Image Meal,Female;
    [SerializeField]
    Button yes, Return;
     

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        PlayerText=FindChildGameObject<TextMeshProUGUI>("NameValue");
        BrothText = FindChildGameObject<TextMeshProUGUI>("BrothValue");
        MealIcon = FindChildGameObject<Image>("MealIcon");
        FemaleIcon = FindChildGameObject<Image>("FemaleIcon");
        Meal = FindChildGameObject<Image>("Meal");
        Female=FindChildGameObject<Image>("Female");
        yes = FindChildGameObject<Button>("Yes");
        Return = FindChildGameObject<Button>("Return");
    }

    protected override void Awake()
    {
        base.Awake();
        yes.onClick.AddListener(YesButtonAction);
        Return.onClick.AddListener(NoButtonAction);
    }
    public override Task InitData(string dataKay)
    {
        CharacterSaveData characterSaveData = GameDataSaveManager.instance.UserGameSaveData.playerData;

        Meal.enabled = characterSaveData.gender == Gender.male;
        Female.enabled = characterSaveData.gender == Gender.female;
        MealIcon.enabled = characterSaveData.gender == Gender.male;
        FemaleIcon.enabled = characterSaveData.gender == Gender.female;

        PlayerText.text = characterSaveData.name; 
        BrothText.SetADDText(characterSaveData.brithDay.season, "之月", characterSaveData.brithDay.day , "日");

        return base.InitData(dataKay);
    }
     

    void NoButtonAction()
    { 
        Close();
    }
    void YesButtonAction()
    { 
        Close();
        var teamManager = TeamManager.instance;
        NPCManager.instance.CreateZeroNPC();
        UIManager.instance.CloseGamePanel<SelectCharacterPanel>(); 
        GameActionManager.instance.QueueAction(new PlayFilm
        {
            filmName = "角色选择",
            assetName= "ZeroStory"
        }); 
    }
  
}
