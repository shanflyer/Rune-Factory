using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
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
    [SerializeField]
    GameObject ActiveObj;

    [SerializeField]
    GameObject Mask;
    [SerializeField]
    Transform effect;
    public override void OnEnable()
    {
        base.OnEnable();
       
    }

    public override void OnDisable()
    {
        base.OnDisable(); 
    }
    private void Awake()
    {
        skillButton.onClick.AddListener(ActionSkill);
    }
    void SkillAutoLock(SkillAutoLock skillAutoLock)
    {
        autoLock = skillAutoLock.autoLock;
        Mask.SetActive(!autoLock && !pauseBehavior);
    }
    void NoSelectSkillAction(NoSelectSkillAction noSelectSkillAction)
    { 
        skillButton.interactable = true;
    }
    void SkillPauseAction(SkillPauseAction skillPauseAction)
    {
        pauseBehavior = skillPauseAction.pause;
        Mask.SetActive(!autoLock && !pauseBehavior);
    } 
    bool pauseBehavior;
    bool autoLock;
    public override void ClearData()
    {
        pauseBehavior = false;
        autoLock = false;
        Mask.SetActive(false);
        if (!SingletonType.Cleared)
        {
            GameActionManager.instance.RemoveListener<RefreshCharacter>(RefreshCharacter);
            GameActionManager.instance.RemoveListener<SkillPauseAction>(SkillPauseAction);
            GameActionManager.instance.RemoveListener<NoSelectSkillAction>(NoSelectSkillAction);
            GameActionManager.instance.RemoveListener<SkillAutoLock>(SkillAutoLock);
            GameActionManager.instance.RemoveListener<SetChapterFight>(SetChapterFight);
        }
      
        base.ClearData();
    }
    void ActionSkill()
    {
        if (!autoLock&&!pauseBehavior&&playerSkillRuntime.GetTimeValue()<=0)
        {
            skillButton.interactable = true;
            SelectSkillAction selectSkillAction = new SelectSkillAction
            {
                skillRuntime = playerSkillRuntime,
                ActionCharacter = fightPlayer.instanceId,
            };
            GameActionManager.instance.QueueAction(selectSkillAction,true);
            if (FightManager.instance.isFight)
            {
                SkillPauseAction skillPauseAction = new SkillPauseAction
                {
                    pause = true,
                };
                Debug.Log($"…Ë÷√‘›Õ£true1");
                GameActionManager.instance.QueueAction(skillPauseAction, true);
            }
         
        }
       
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
        skillButton = FindChildGameObject<Button>("BG");
        skillName = FindChildGameObject<TextMeshProUGUI>("SkillName");
        skillValue = FindChildGameObject<Image>("SkillValue");
        ActiveObj = FindChildGameObject("Active").gameObject;
        Mask = FindChildGameObject("Mask").gameObject;
        effect = FindChildGameObject("Effect");
    }

    public override async Task InitData(FightCharacter t, SelectAction<FightCharacter> SelectAction = null, ToggleGroup toggleGroup = null)
    {
      await  base.InitData(t, SelectAction, toggleGroup);
        fightPlayer = t as FightPlayer;

        GameActionManager.instance.AddListener<RefreshCharacter>(RefreshCharacter);
        GameActionManager.instance.AddListener<SkillPauseAction>(SkillPauseAction);
        GameActionManager.instance.AddListener<NoSelectSkillAction>(NoSelectSkillAction);
        GameActionManager.instance.AddListener<SkillAutoLock>(SkillAutoLock);
        GameActionManager.instance.AddListener<SetChapterFight>(SetChapterFight);

        InitData();
        //Mask.gameObject.SetActive(false);
    }
    bool isInFight;
    void SetChapterFight(SetChapterFight setChapterFight)
    {
        isInFight = setChapterFight.isInFight;
        Mask.SetActive(!setChapterFight.isInFight);
        RefreshEffect();
    }
    void RefreshEffect()
    {
        effect.transform.localScale = Vector3.zero;
        if (isInFight && playerSkillRuntime != null)
        {
            effect.transform.localScale = playerSkillRuntime.GetTimeValue() >= 1 ? Vector3.one : Vector3.zero;
        }
    }
    private void RefreshCharacter(RefreshCharacter refreshCharacter)
    {
        if (fightPlayer!=null&&fightPlayer.character.instanceId == refreshCharacter.id)
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
            CharacterProperty characterProperty = fightPlayer.characterProperty;
            if (character == CharacterManager.instance.controllerCharacter)
            {
                NameText.text=(characterInformationData.name);
            }
            else
            {
                NameText.SetSWText(characterInformationData.name);
            }
           
            LevelText.text = $"Lv.{characterInformationData.level}";
            HPText.text = $"HP,{characterProperty.HP}/{characterProperty.MaxHP}";
            MPText.text = $"MP,{characterProperty.MP}/{characterProperty.MaxMP}";
            ATText.text = $"AT.{characterProperty.AT}";
            DFText.text = $"DF.{characterProperty.DF}";
            LuckyText.text = $"{LanguageManage.SwitchStr("–“‘À")}.{characterProperty.Lucky}";
            ExpText.text = $"Exp.{characterInformationData.exp.nowExp}/{characterInformationData.exp.nowLevelExp}";

            int speed = characterProperty.Speed;
            speed = math.clamp(speed, 0, 1);
            SpeedText.text = $"{LanguageManage.SwitchStr("√ÙΩ›")}.{speed}";

            HPSlider.fillAmount = (float)characterProperty.HP / characterProperty.MaxHP;
            MPSlider.fillAmount = (float)characterProperty.MP / characterProperty.MaxMP;

            playerSkillRuntime= fightPlayer.GetPlayerEquipSkill();
            if (playerSkillRuntime != null)
            {
                skillPanel.localScale = Vector3.one;
                SkillData skillData = playerSkillRuntime.skillData;
                skillIcon.sprite = skillData.icon;
                skillName.SetSWText( skillData.skillName);
                skillValue.fillAmount =playerSkillRuntime.GetTimeValue();
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

    public void SetSkillPanel(bool show)
    {
        skillPanel.transform.localScale = show ? Vector3.one : Vector3.zero;
    }
    private void Update()
    {
        if (playerSkillRuntime == null)
            return;
        float value = playerSkillRuntime.GetTimeValue();
        value = Mathf.Clamp(value, 0, 1); 
        skillValue.fillAmount =value; 

        if (value <= 0)
        {
            effect.transform.localScale =Vector3.zero;
            ActiveObj.gameObject.SetActive(true);
        }
        else
        {
            RefreshEffect();
            skillButton.interactable = true;
            ActiveObj.gameObject.SetActive(false);
        }
    }
}