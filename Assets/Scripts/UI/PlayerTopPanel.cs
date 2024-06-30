using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTopPanel : GamePanel<IReferenceData>
{
    public override bool changeInputModel => false;

    [SerializeField]
    private TextMeshProUGUI goldValue, crystalValue;

    [SerializeField]
    private Button goldAdd, crystalAdd;

    [SerializeField]
    private TextMeshProUGUI season;

    [SerializeField]
    private TextMeshProUGUI week;

    [SerializeField]
    private TextMeshProUGUI day;

    [SerializeField]
    private TextMeshProUGUI time;

    [SerializeField]
    private Image weather;

    [SerializeField]
    private Button calendar, SetButton;

    [SerializeField]
    private Image PlayerHead;

    [SerializeField]
    private TextMeshProUGUI PlayerName;

    [SerializeField]
    private Image HPSlider, RPSlider;

    [SerializeField]
    private TextMeshProUGUI HPValue, RPValue;

    [SerializeField]
    private Button playerButton;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        playerButton = FindChildGameObject<Button>("Player");

        goldValue = FindChildGameObject<TextMeshProUGUI>("GoldValue");
        crystalValue = FindChildGameObject<TextMeshProUGUI>("CrystalValue");
        goldAdd = FindChildGameObject<Button>("Gold");
        crystalAdd = FindChildGameObject<Button>("Crystal");
        season = FindChildGameObject<TextMeshProUGUI>("Season");
        week = FindChildGameObject<TextMeshProUGUI>("Week");
        day = FindChildGameObject<TextMeshProUGUI>("Day");
        time = FindChildGameObject<TextMeshProUGUI>("TimeValue");
        calendar = FindChildGameObject<Button>("DatePanel");
        SetButton = FindChildGameObject<Button>("SetButton");

        weather = FindChildGameObject<Image>("WeatherIcon");

        PlayerHead = FindChildGameObject<Image>("Head");
        PlayerName = FindChildGameObject<TextMeshProUGUI>("Name");
        HPSlider = FindChildGameObject<Image>("HPSlider");
        RPSlider = FindChildGameObject<Image>("RPSlider");
        HPValue = FindChildGameObject<TextMeshProUGUI>("HPValue");
        RPValue = FindChildGameObject<TextMeshProUGUI>("RPValue");
    }

    protected override void Awake()
    {
        base.Awake();
        playerButton.onClick.AddListener(() =>
        {
            var characterInformation = CharacterManager.instance.controllerCharacter.GetInformation();
            UIManager.instance.ShowGamePanel<CharacterInformationPanel, CharacterInformationData>(characterInformation);
        });

        calendar.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<CalendarPanel>(layer: 3);
        });
        goldAdd.onClick.AddListener(PayManager.instance.TryCreatGold);
        crystalAdd.onClick.AddListener(PayManager.instance.TryCreatMoney);

        SetButton.onClick.AddListener(() =>
        {
            
            // AudioController.instance.PlayAudio(SE.click);
            UIManager.instance.ShowGamePanel<SetPanel>();
        });

        GameActionManager.instance.AddListener<NewDay>(NewDay);
        GameActionManager.instance.AddListener<RefreshPlayerGold>(RefreshPlayerGold);
        GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);
        GameActionManager.instance.AddListener<CharacterPropertyTrigger>(RefreshCharacterProperty);
    }

    private void RefreshCharacterProperty(CharacterPropertyTrigger refreshCharacterProperty)
    {
        if (refreshCharacterProperty.characterId == CharacterManager.instance.controllerCharacter.instanceId)
        {
            var characterProperty = CharacterManager.instance.controllerCharacter.CharacterProperty;
            HPSlider.fillAmount = characterProperty.HP / (float)characterProperty.MaxHP;
            RPSlider.fillAmount = characterProperty.Power / (float)characterProperty.MaxPower;
            HPValue.text = $"{characterProperty.HP}/{characterProperty.MaxHP}";
            RPValue.text = $"{characterProperty.Power}/{characterProperty.MaxPower}";
        }
    }

    public override Task InitData(string dataKay)
    {
        goldValue.text = PayManager.instance.NowGold.ToString();
        crystalValue.text = PayManager.instance.NowDiamond.ToString();

        PlayerName.text = CharacterManager.instance.controllerCharacter.name;
        var headSprite = CharacterManager.instance.controllerCharacter.characterData.head;
        headSprite.SetImageSprite(PlayerHead);
        PlayerHead.SetNativeSize();
        var characterProperty = CharacterManager.instance.controllerCharacter.CharacterProperty;
        HPSlider.fillAmount = characterProperty.HP / (float)characterProperty.MaxHP;
        RPSlider.fillAmount = characterProperty.Power / (float)characterProperty.MaxPower;
        HPValue.text = $"{characterProperty.HP}/{characterProperty.MaxHP}";
        RPValue.text = $"{characterProperty.Power}/{characterProperty.MaxPower}";

        RefreshPlayerGold(default(RefreshPlayerGold));
        NewDay(default(NewDay));

        return base.InitData(dataKay);
    }

    public override void Show(int layer = -1)
    {
        base.Show(layer);
    }

    public override void InitReferenceData(IReferenceData v)
    {
        RefreshPlayerGold(default(RefreshPlayerGold));
        NewDay(default(NewDay));
        base.InitReferenceData(v);
    }

    private void RefreshPlayerGold(RefreshPlayerGold updateMoney)
    {
        goldValue.text = PayManager.instance.NowGold.ToString();
        crystalValue.text = PayManager.instance.NowDiamond.ToString();
    }

    private void NewDay(NewDay newDay)
    {
        day.text = GameTimeManager.instance.Day.ToString();
        season.text = GameTimeManager.instance.Season.ToString();
        week.text = GameTimeManager.instance.Week.ToString();
        time.text = $"{GameTimeManager.instance.Hour.ToString("00")}:{GameTimeManager.instance.Minute.ToString("00")}";
    }

    private void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        time.text = $"{GameTimeManager.instance.Hour.ToString("00")}:{GameTimeManager.instance.Minute.ToString("00")}";
    }
}