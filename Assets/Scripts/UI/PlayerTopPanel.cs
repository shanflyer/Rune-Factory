using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTopPanel : GamePanel<IReferenceData>
{
    public override bool changeInputModel => false;

    [SerializeField] private Transform LeftNode, RightNode;
    [SerializeField] private Toggle LeftToggle, RightToggle;
    [SerializeField]
    private TextMeshProUGUI goldValue, crystalValue;

    [SerializeField]
    private Button goldAdd, crystalAdd;

    [SerializeField] private Image GoldAdd_Image, CrystalAdd_Image;

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
    private TextMeshProUGUI FPSText;
    [SerializeField]
    private Button playerButton,MapButton;
    [SerializeField]
    private Vector2 headSize = new Vector2(448, 512);
    private bool actionListenersRegistered;
    private const float FpsRefreshInterval = 0.25f;
    private float fpsRefreshTimer;

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

        FPSText = FindChildGameObject<TextMeshProUGUI>("FPS");
        HPSlider = FindChildGameObject<Image>("HPSlider");
        RPSlider = FindChildGameObject<Image>("RPSlider");
        HPValue = FindChildGameObject<TextMeshProUGUI>("HPValue");
        RPValue = FindChildGameObject<TextMeshProUGUI>("RPValue");
    }
    
    protected override void Awake()
    {
        base.Awake();
        LeftToggle.onValueChanged.AddListener(arg0 => LeftNode.gameObject.SetActive(arg0));
        RightToggle.onValueChanged.AddListener(arg0 => RightNode.gameObject.SetActive(arg0));
        playerButton.onClick.AddListener(() =>
        {
            var characterInformation = CharacterManager.instance.controllerCharacter.GetInformation();
            // 按钮回调保持同步，面板加载异常统一进入异步日志。
            AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<CharacterInformationPanel, CharacterInformationData>(characterInformation), nameof(CharacterInformationPanel));
        });

        calendar.onClick.AddListener(() =>
        {
            AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<CalendarPanel>(), nameof(CalendarPanel));
        });
        goldAdd.onClick.AddListener(PayManager.instance.TryCreatGold);
        crystalAdd.onClick.AddListener(PayManager.instance.TryCreatMoney);

        SetButton.onClick.AddListener(() =>
        {

            // AudioController.instance.PlayAudio(SE.click);
            AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<SetPanel>(), nameof(SetPanel));
        });
        MapButton.onClick.AddListener(() =>
        {
            // 按钮回调保持同步，面板加载异常交给统一异步日志。
            AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<TransmissionPanel>(), nameof(TransmissionPanel));
        });

        AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<CharacterButtonPanel>(), nameof(CharacterButtonPanel));

        if (GameController.instance.startPlay || GameGuideManager.instance.IsEndGuide())
        {
            GoldAdd_Image.enabled = true;
            goldAdd.enabled = true;
        }
        else
        {
            GoldAdd_Image.enabled = false;
            goldAdd.enabled = false;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        RegisterActionListeners();
    }

    public override void OnDisable()
    {
        UnregisterActionListeners();
        base.OnDisable();
    }

    private void RegisterActionListeners()
    {
        if (actionListenersRegistered || SingletonType.Cleared)
        {
            return;
        }

        GameActionManager.instance.AddListener<NewDay>(NewDay);
        GameActionManager.instance.AddListener<RefreshPlayerGold>(RefreshPlayerGold);
        GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);
        GameActionManager.instance.AddListener<CharacterPropertyTrigger>(RefreshCharacterProperty);
        GameActionManager.instance.AddListener<SetWeather>(SetWeather);
        GameActionManager.instance.AddListener<NewHour>(NewHour);
        GameActionManager.instance.AddListener<SaveGuideFilmIndexAction>(SaveGuideFilmIndexAction);
        actionListenersRegistered = true;
    }

    private void UnregisterActionListeners()
    {
        if (!actionListenersRegistered || SingletonType.Cleared || !GameActionManager.HasInstance)
        {
            return;
        }

        // 顶栏可被隐藏再打开，解绑后重开重新注册，避免跨场景或热重启重复刷新。
        GameActionManager.instance.RemoveListener<NewDay>(NewDay);
        GameActionManager.instance.RemoveListener<RefreshPlayerGold>(RefreshPlayerGold);
        GameActionManager.instance.RemoveListener<UpdateGameTime>(UpdateGameTime);
        GameActionManager.instance.RemoveListener<CharacterPropertyTrigger>(RefreshCharacterProperty);
        GameActionManager.instance.RemoveListener<SetWeather>(SetWeather);
        GameActionManager.instance.RemoveListener<NewHour>(NewHour);
        GameActionManager.instance.RemoveListener<SaveGuideFilmIndexAction>(SaveGuideFilmIndexAction);
        actionListenersRegistered = false;
    }

    private void SaveGuideFilmIndexAction(SaveGuideFilmIndexAction SaveGuideFilmIndexAction)
    {
        if (GameController.instance.startPlay || GameGuideManager.instance.IsEndGuide())
        {
            GoldAdd_Image.enabled = true;
            goldAdd.enabled = true;
        }
        else
        {
            GoldAdd_Image.enabled = false;
            goldAdd.enabled = false;
        }
    }
    private void LateUpdate()
    {
        if (!FPSText.gameObject.activeSelf || !FPSText.enabled)
        {
            return;
        }

        fpsRefreshTimer += Time.unscaledDeltaTime;
        if (fpsRefreshTimer < FpsRefreshInterval)
        {
            return;
        }

        // FPS 只是调试展示，限频刷新可以减少顶栏常驻时的字符串分配。
        fpsRefreshTimer = 0f;
        float smoothDeltaTime = Mathf.Max(Time.smoothDeltaTime, 0.0001f);
        FPSText.text = $"FPS{1.0f / smoothDeltaTime:0.0}";
    }

    private void NewHour(NewHour newHour)
    {
        weather.sprite = WeatherManager.instance.GetWeatherIcon();
    }
    void SetWeather(SetWeather setWeather)
    {
        weather.sprite = WeatherManager.instance.GetWeatherIcon(setWeather.weather);
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

        PlayerName.text = GameDataSaveManager.instance.UserGameSaveData.playerData.name;
        var headSprite = CharacterManager.instance.controllerCharacter.characterData.head;
        headSprite.SetImageSprite(PlayerHead, headSize);
       // PlayerHead.SetNativeSize();
        var characterProperty = CharacterManager.instance.controllerCharacter.CharacterProperty;
        HPSlider.fillAmount = characterProperty.HP / (float)characterProperty.MaxHP;
        RPSlider.fillAmount = characterProperty.Power / (float)characterProperty.MaxPower;
        HPValue.text = $"{characterProperty.HP}/{characterProperty.MaxHP}";
        RPValue.text = $"{characterProperty.Power}/{characterProperty.MaxPower}";
        weather.sprite = WeatherManager.instance.GetWeatherIcon();
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
        season.SetSWText(GameTimeManager.instance.Season);
        week.SetSWText(GameTimeManager.instance.Week);
        time.text = $"{GameTimeManager.instance.Hour.ToString("00")}:{GameTimeManager.instance.Minute.ToString("00")}";
    }

    private void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        time.text = $"{GameTimeManager.instance.Hour.ToString("00")}:{GameTimeManager.instance.Minute.ToString("00")}";
    }
}
