using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FightCharacterReference : UIObjReference<MyInt>
{
    [SerializeField]
    private TextMeshProUGUI ATText, DFText, NameText;

    [SerializeField]
    private Image MPSlider, HPSlider;

    [SerializeField]
    private TextMeshProUGUI HPText, MPText;

    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private TextMeshProUGUI ExpText;

    [SerializeField]
    private TextMeshProUGUI LuckyText,SpeedText;

    [SerializeField]
    private Transform Info, Null;

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshCharacter>(RefreshCharacter);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshCharacter>(RefreshCharacter);
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        ATText = FindChildGameObject<TextMeshProUGUI>("AT");
        DFText = FindChildGameObject<TextMeshProUGUI>("DF");
        NameText = FindChildGameObject<TextMeshProUGUI>("Name");
        LevelText = FindChildGameObject<TextMeshProUGUI>("Level");
        ExpText = FindChildGameObject<TextMeshProUGUI>("Exp");
        LuckyText = FindChildGameObject<TextMeshProUGUI>("Lock");
        SpeedText = FindChildGameObject<TextMeshProUGUI>("Speed");
        HPText = FindChildGameObject<TextMeshProUGUI>("HPValue");
        MPText = FindChildGameObject<TextMeshProUGUI>("MPValue");

        HPSlider = FindChildGameObject<Image>("HPSlider");
        MPSlider = FindChildGameObject<Image>("MPSlider");

        Info = FindChildGameObject("Info");
        Null = FindChildGameObject("Null");
    }

    public override async Task InitData(MyInt t, SelectAction<MyInt> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        InitData(t.value);
    }

    private void RefreshCharacter(RefreshCharacter refreshCharacter)
    {
        if (data.value == refreshCharacter.id)
        {
            InitData(refreshCharacter.id);
        }
    }

    private void InitData(int characterId)
    {
        Character character = CharacterManager.instance.GetCharacter(characterId);
        if (character != null)
        {
            Info.gameObject.SetActive(true);
            Null.gameObject.SetActive(false);

            CharacterInformationData characterInformationData = character.GetInformation();
            CharacterProperty characterProperty = characterInformationData.characterProperty;
            NameText.text = characterInformationData.name;
            LevelText.text = $"Lv.{characterInformationData.level}";
            HPText.text = $"HP,{characterProperty.HP}/{characterProperty.MaxHP}";
            MPText.text = $"MP,{characterProperty.MP}/{characterProperty.MaxMP}";
            ATText.text = $"AT.{characterProperty.AT}";
            DFText.text = $"DF.{characterProperty.DF}";
            LuckyText.text = $"{LanguageManage.SwitchStr("пртк")}.{characterProperty.Lucky}";
            ExpText.text = $"Exp.{characterInformationData.exp.nowExp}/{characterInformationData.exp.nowLevelExp}";
            SpeedText.text = $"{LanguageManage.SwitchStr("цТ╫щ")}.{characterProperty.Speed}";

            HPSlider.fillAmount = (float)characterProperty.HP / characterProperty.MaxHP;
            MPSlider.fillAmount = (float)characterProperty.MP / characterProperty.MaxMP;
        }
        else
        {
            Info.gameObject.SetActive(false);
            Null.gameObject.SetActive(true);
        }
    }
}