using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FightPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Transform AutoTips, FightButtons, ExploreButtons;

    [SerializeField]
    private TextMeshProUGUI MapName, ExploreValue;

    [SerializeField]
    private Button FightButton, ItemButtom, AutoFightButton, SkillButton;

    [SerializeField]
    private Button GoingButton, UsingButton, AutoButton, RetreatButton, SwitchButton;
    [SerializeField]
    private TextMeshProUGUI goingText, autoText;

    private int dataId;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        AutoTips = FindChildGameObject("AutoTips");
        MapName = FindChildGameObject<TextMeshProUGUI>("MapName");
        ExploreValue = FindChildGameObject<TextMeshProUGUI>("ExploreValue");
        FightButtons = FindChildGameObject("FightButtons");
        ExploreButtons = FindChildGameObject("ExploreButtons");

        SkillButton = FindChildGameObject<Button>("SkillButton");

        FightButton = FindChildGameObject<Button>("FightButton");
        ItemButtom = FindChildGameObject<Button>("ItemButton");
        AutoFightButton = FindChildGameObject<Button>("AutoFightButton");

        GoingButton = FindChildGameObject<Button>("GoingButton");
        UsingButton = FindChildGameObject<Button>("UsingButton");
        AutoButton = FindChildGameObject<Button>("AutoButton");
        RetreatButton = FindChildGameObject<Button>("RetreatButton");
        SwitchButton = FindChildGameObject<Button>("SwitchButton");

        goingText = GoingButton.transform.GetComponentInChildren<TextMeshProUGUI>();
        autoText = AutoFightButton.transform.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void SwitchFunctionButton(SwitchFunctionButton switchFunctionButton)
    {
        bool fight = switchFunctionButton.fight;
        FightButtons.gameObject.SetActive(fight);
        ExploreButtons.gameObject.SetActive(!fight);
    }

    private void RefreshFightChapter(RefreshFightChapter refreshFightChapter)
    {
        if (refreshFightChapter.id == dataId)
        {
            var fightChapter = ExploreManager.instance.GetFigehtChapter(refreshFightChapter.id);
            if (fightChapter.mapId == dataId)
            {
                ExploreValue.text = $"{fightChapter.completeValue}%";
            }
        }
    }

    
    public override void OnDisable()
    {
        GameActionManager.instance.RemoveListener<RefreshFightChapter>(RefreshFightChapter);
        GameActionManager.instance.RemoveListener<SwitchFunctionButton>(SwitchFunctionButton);
        GameActionManager.instance.RemoveListener<EndPlayerRound>(EndPlayerRound);
        base.OnDisable();
    }

    public override void OnEnable()
    {
        GameActionManager.instance.AddListener<RefreshFightChapter>(RefreshFightChapter);
        GameActionManager.instance.AddListener<SwitchFunctionButton>(SwitchFunctionButton);
        GameActionManager.instance.AddListener<EndPlayerRound>(EndPlayerRound);
        AutoTips.localScale = Vector3.zero;
        base.OnEnable();
    }
    PlayerFight playerFight = new PlayerFight();
    StartRoundFight startRoundFight = new StartRoundFight();
    protected override void Awake()
    {
        base.Awake();
        FightButtons.gameObject.SetActive(false);
        ExploreButtons.gameObject.SetActive(true);

        FightButton.onClick.AddListener(() =>
        {
            FightButton.interactable = false;
            GameActionManager.instance.QueueAction(playerFight);
        });

        AutoFightButton.onClick.AddListener(() =>
        {
            // GameActionManager.instance.QueueAction(startRoundFight);
            FightController.instance.SetFightAuto();
            bool auto = FightController.instance.AutoFight;
            autoText.text = auto ? "手动" : "自动";
            AutoTips.localScale = auto ? Vector3.one : Vector3.zero;
            FightButton.interactable = !auto;
        });

        GoingButton.onClick.AddListener(GoingAction);
    }
    void EndPlayerRound(EndPlayerRound endPlayerRound)
    {
        FightButton.interactable = true;
        ItemButtom.interactable = true;
    }
    void GoingAction()
    {
        FightController.instance.StartWalk();
    }
    public override async Task InitData(string dataKey)
    {
        int dataId = int.Parse(dataKey);
        FightChapter fightChapter = ExploreManager.instance.GetFigehtChapter(dataId);
        if (fightChapter.mapId == dataId)
        {
            FightMapData fightMapData = await GameDataManager.instance.GetAsyncData<FightMapData>(dataId.ToString());
            MapName.text = fightMapData.mapName;
            ExploreValue.text = $"{fightChapter.completeValue}%";
        }
        FightManager.instance.RefreshFightPlayerInfo();
    }
}