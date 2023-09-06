using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;

public class CharacterSelectInformationPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Text PlayerText, BrothText;
    [SerializeField]
    Image MealIcon, FemaleIcon;
    [SerializeField]
    Image Meal,Female;
    [SerializeField]
    Button yes, Return;
     

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        PlayerText=FindChildGameObject<Text>("NameValue");
        BrothText = FindChildGameObject<Text>("BrothValue");
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
        CharacterSaveData characterSaveData = GameDataManager.instance.UserGameSaveData.playerData;

        Meal.enabled = characterSaveData.gender == Gender.male;
        Female.enabled = characterSaveData.gender == Gender.female;
        MealIcon.enabled = characterSaveData.gender == Gender.male;
        FemaleIcon.enabled = characterSaveData.gender == Gender.female;

        PlayerText.text = characterSaveData.name;
        string month = characterSaveData.brithDay.season.ToString() + "之月"; 
        BrothText.text = LanguageManage.SwitchStr(month) + characterSaveData.brithDay.day + LanguageManage.SwitchStr("日");

        return base.InitData(dataKay);
    }
     

    void NoButtonAction()
    {
        AudioController.instance.PlayAudio(SE.Return); 
        Close();
    }
    void YesButtonAction()
    {
        AudioController.instance.PlayAudio(SE.click);
        Close();
        UIManager.instance.CloseGamePanel<SelectCharacterPanel>();
        GameActionManager.instance.QueueAction(new PlayFilm
        {
            filmName = "StartStory"
        });
    }
  
}
