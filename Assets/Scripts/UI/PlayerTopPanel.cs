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
    private TextMeshProUGUI date;

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

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        goldValue = FindChildGameObject<TextMeshProUGUI>("GoldValue");
        crystalValue = FindChildGameObject<TextMeshProUGUI>("CrystalValue");
        goldAdd = FindChildGameObject<Button>("Gold");
        crystalAdd = FindChildGameObject<Button>("Crystal");
        date = FindChildGameObject<TextMeshProUGUI>("Date");
        calendar = FindChildGameObject<Button>("TimeObj");
        SetButton = FindChildGameObject<Button>("SetButton");

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
        PlayerHead.sprite = CharacterManager.instance.controllerCharacter.characterData.head;
        (PlayerHead.transform as RectTransform).sizeDelta = new Vector2(headSprite.rect.width, headSprite.rect.height) * 0.5f;
        var characterProperty = CharacterManager.instance.controllerCharacter.CharacterProperty;
        HPSlider.fillAmount = characterProperty.HP / (float)characterProperty.MaxHP;
        RPSlider.fillAmount = characterProperty.Power / (float)characterProperty.MaxPower;
        HPValue.text = $"{characterProperty.HP}/{characterProperty.MaxHP}";
        RPValue.text = $"{characterProperty.Power}/{characterProperty.MaxPower}";

        date.text = GameTimeManager.instance.NowGameTime;

        return base.InitData(dataKay);
    }

    public override void Show(int layer = -1)
    {
        base.Show(layer);
    }

    public override void InitReferenceData(IReferenceData v)
    {
        RefreshPlayerGold(default(RefreshPlayerGold));
        UpdateGameTime(default(UpdateGameTime));
        base.InitReferenceData(v);
    }

    private void RefreshPlayerGold(RefreshPlayerGold updateMoney)
    {
        goldValue.text = PayManager.instance.NowGold.ToString();
        crystalValue.text = PayManager.instance.NowDiamond.ToString();
    }

    private void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        date.text = GameTimeManager.instance.NowGameTime;
    }
}