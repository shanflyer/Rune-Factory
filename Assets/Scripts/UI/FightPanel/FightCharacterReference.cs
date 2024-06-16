using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FightCharacterReference : UIObjReference<FightCharacter>
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

    [SerializeField]
    Transform skillPanel;
    [SerializeField]
    Image skillIcon;
    [SerializeField]
    TextMeshProUGUI skillName;
    [SerializeField]
    Image skillValue;
    [SerializeField]
    Button skillButton;

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
    private void Awake()
    {
        skillButton.onClick.AddListener(ActionSkill);
    }

    void ActionSkill()
    {
        FightController.instance.SelectSkill(playerSkillRuntime,fightPlayer.instanceId);
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

        skillPanel = FindChildGameObject("SkillPanel");
        skillIcon = FindChildGameObject<Image>("SkillIcon");
        skillButton = FindChildGameObject<Button>("SkillPanel");
        skillName = FindChildGameObject<TextMeshProUGUI>("SkillName");
        skillValue = FindChildGameObject<Image>("SkillValue");
    }

    public override async Task InitData(FightCharacter t, SelectAction<FightCharacter> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        fightPlayer = t as FightPlayer; 

        InitData();
    }

    private void RefreshCharacter(RefreshCharacter refreshCharacter)
    {
        if (fightPlayer.character.instanceId == refreshCharacter.id)
        {
            InitData();
        }
    }
    FightPlayer fightPlayer;
    SkillRuntime playerSkillRuntime;
    private void InitData()
    { 
        if (fightPlayer != null)
        {
            var character = fightPlayer.character;
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

            playerSkillRuntime= fightPlayer.GetPlayerEquipSkill();
            if (playerSkillRuntime != null)
            {
                skillPanel.localScale = Vector3.one;
                SkillData skillData = playerSkillRuntime.skillData;
                skillIcon.sprite = skillData.icon;
                skillName.text = skillData.skillName;
                skillValue.fillAmount =1- playerSkillRuntime.GetTimeValue();
            }
            else
            {
                skillPanel.localScale = Vector3.zero;
            }
        }
        else
        {
            skillPanel.localScale = Vector3.zero;
            Info.gameObject.SetActive(false);
            Null.gameObject.SetActive(true);
        }

    }

    private void Update()
    {
        if (playerSkillRuntime == null)
            return;
        float value = playerSkillRuntime.GetTimeValue();
        value = Mathf.Clamp(value, 0, 1);
        value = 1 - value;
        skillValue.fillAmount =value;
        if (value <= 0)
        {
            skillButton.interactable = true;
        }
        else
        {
            skillButton.interactable = false;
        }
    }
}