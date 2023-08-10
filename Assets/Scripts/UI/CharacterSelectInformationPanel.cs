using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;

public class CharacterSelectInformationPanel : GamePanel
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
    public override Task InitData(int dataId)
    {
        Meal.enabled = PlayerDate.gender == Gender.male;
        Female.enabled = PlayerDate.gender == Gender.female;
        MealIcon.enabled = PlayerDate.gender == Gender.male;
        FemaleIcon.enabled = PlayerDate.gender == Gender.female;

        PlayerText.text = PlayerDate.playerName;
        string month = PlayerDate.season.ToString() + "之月"; 
        BrothText.text = LanguageManage.SwitchStr(month) + PlayerDate.date + LanguageManage.SwitchStr("日");

        return base.InitData(dataId);
    }
     

    void NoButtonAction()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Return");
        Close();
    }
    void YesButtonAction()
    { 
        AudioManager.PlaySE(PlayType.ONCE,"Click");
        Close();
        UIManager.instance.CloseGamePanel<SelectCharacterPanel>();
        GameActionManager.instance.QueueAction(new PlayFilm
        {
            filmName = "StartStory"
        });
    }
  
}
