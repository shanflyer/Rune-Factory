using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.UI; 
using System.Threading.Tasks;
using TMPro;
using UnityEngine.Analytics;
using VoxelBusters.EssentialKit.GameServicesCore;

public class CharacterSelectInformationPanel : GamePanel<SelectCharacterData>
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
    public override void InitReferenceData(SelectCharacterData v)
    {

        Meal.enabled = v.gender == Gender.male;
        Female.enabled = v.gender == Gender.female;
        MealIcon.enabled = v.gender == Gender.male;
        FemaleIcon.enabled = v.gender == Gender.female;

        PlayerText.text = v.name;
        BrothText.SetADDText(v.brithSeason, "之月", v.brithDay, "日");

        base.InitReferenceData(v);
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
        AudioController.instance.PlayBGM(null, audioClearType: AudioClearType.All, isLerp: true, Group: BGMGroup.Theme.ToString());
        GameActionManager.instance.QueueAction(new PlayFilm
        {
            filmName = "角色选择",
            assetName= "ZeroStory"
        });
        GameDataSaveManager.instance.InitPlayerData(data.name, data.gender, data.brithSeason, data.brithDay);
    }
  
}
