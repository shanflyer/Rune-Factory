using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveReference : UIObjReference<UserGameSaveData>
{
    [SerializeField]
    private Toggle SelectToggle;

    [SerializeField]
    private Image Icon;

    [SerializeField]
    private TextMeshProUGUI Name;

    [SerializeField]
    private TextMeshProUGUI Level;

    [SerializeField]
    private TextMeshProUGUI Money;

    [SerializeField]
    private TextMeshProUGUI Time;

    [SerializeField]
    private TextMeshProUGUI SaveTime;

    public override void SelectDefault()
    {
        base.SelectDefault();
        SelectToggle.SetIsOnWithoutNotify(true);
        if (SelectAction != null)
        {
            SelectAction(data, true);
        }
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        SelectToggle = GetComponent<Toggle>();
        Icon = FindChildGameObject<Image>("NPCImage");
        Name = FindChildGameObject<TextMeshProUGUI>("Name");
        Level = FindChildGameObject<TextMeshProUGUI>("Level");
        Money = FindChildGameObject<TextMeshProUGUI>("Money");
        Time = FindChildGameObject<TextMeshProUGUI>("Time");
        SaveTime = FindChildGameObject<TextMeshProUGUI>("SaveTime");
    }

    public override async Task InitData(UserGameSaveData t, SelectAction<UserGameSaveData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        SelectToggle.group = toggleGroup;
        if (!string.IsNullOrEmpty(data.saveTime))
        {
            Icon.enabled = true;
            CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(data.playerData.dataId);
            characterData.head.SetImageSprite(Icon);
            //Icon.sprite = characterData.icon.sprite;
            Level.text = GameCommon.AddString("Lv.", data.playerData.level.ToString());
            Name.text = data.playerData.name;
            Money.text = data.otherSaveData.gold.ToString();
            Time.text = data.dateData.ToString();
            SaveTime.text = data.saveTime;
        }
        else
        {
            Icon.enabled = false;
            Level.text = "-";
            Name.text = "-";
            Money.text = "-";
            Time.text = "-";
            SaveTime.text = "-";
        }
        SelectToggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                if (SelectAction != null)
                {
                    SelectAction(data, value);
                }
            }
            
        });
    }

    public void Awake()
    {
        Icon.enabled = false;
        Level.text = "-";
        Name.text = "-";
        Money.text = "-";
        Time.text = "-";
        SaveTime.text = "-";
    }
}