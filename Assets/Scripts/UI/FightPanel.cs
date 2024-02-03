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
    private Button FightButton, ItemButtom, AutoFightButton, EscapeFightButton, SkillButton;

    [SerializeField]
    private Button GoingButton, UsingButton, AutoButton, RetreatButton, SwitchButton;
    [SerializeField]
    private TextMeshProUGUI goingText, autoText,autoExploreText;

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
        EscapeFightButton = FindChildGameObject<Button>("EscapeFightButton");

        GoingButton = FindChildGameObject<Button>("GoingButton");
        UsingButton = FindChildGameObject<Button>("UsingButton");
        AutoButton = FindChildGameObject<Button>("AutoButton");
        RetreatButton = FindChildGameObject<Button>("RetreatButton");
        SwitchButton = FindChildGameObject<Button>("SwitchButton");

        goingText = GoingButton.transform.GetComponentInChildren<TextMeshProUGUI>();
        autoText = AutoFightButton.transform.GetComponentInChildren<TextMeshProUGUI>();
        autoExploreText=AutoButton.transform.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void SwitchFunctionButton(SwitchFunctionButton switchFunctionButton)
    {
        bool fight = switchFunctionButton.fight;
        bool auto = switchFunctionButton.auto;
        FightButtons.gameObject.SetActive(fight);
        ExploreButtons.gameObject.SetActive(!fight);
        if (!fight)
        {
            DisplayAutoExplore(auto);
        }
        else
        {
            DisplayAutoFight(auto);
        }
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
        GameActionManager.instance.RemoveListener<StopAutoFight>(StopAutoFight);
        base.OnDisable();
    }

    public override void OnEnable()
    {
        GameActionManager.instance.AddListener<RefreshFightChapter>(RefreshFightChapter);
        GameActionManager.instance.AddListener<SwitchFunctionButton>(SwitchFunctionButton);
        GameActionManager.instance.AddListener<EndPlayerRound>(EndPlayerRound);
        GameActionManager.instance.AddListener<StopAutoFight>(StopAutoFight);
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

        EscapeFightButton.onClick.AddListener(EscapeFightAction);

        FightButton.onClick.AddListener(() =>
        {
            FightButton.interactable = false;
            EscapeFightButton.interactable = false;
            GameActionManager.instance.QueueAction(playerFight);
        });

        AutoFightButton.onClick.AddListener(() =>
        {
            SwitchAutoExplore switchAutoExplore = new SwitchAutoExplore
            {
                explore = false,
                setResult = DisplayAutoFight
            };
            GameActionManager.instance.QueueAction(switchAutoExplore, true); 
        });

        AutoButton.onClick.AddListener(() =>
        {
            SwitchAutoExplore switchAutoExplore = new SwitchAutoExplore
            {
                explore = true,
                setResult= DisplayAutoExplore
            };
            GameActionManager.instance.QueueAction(switchAutoExplore,true); 
        });

        GoingButton.onClick.AddListener(GoingAction);
        RetreatButton.onClick.AddListener(ExitFight);
        UsingButton.onClick.AddListener(() => {
            FightController.instance.StopWalk();
            going = true;
            goingText.text = "前进";
            ItemMatchData itemMatchData = new ItemMatchData
            {
                itemMatchType = ItemMatchType.ItemType,
                matchValues = new System.Collections.Generic.HashSet<int>
                {
                    (int)ItemType.食物
                }
            };
            PackageManager.instance.ShowPlayerBagUse(true, itemMatchData);
        });
        ItemButtom.onClick.AddListener(() => {
            FightController.instance.StopWalk();
            going = true;
            goingText.text = "前进";
            ItemMatchData itemMatchData = new ItemMatchData
            {
                itemMatchType = ItemMatchType.ItemType,
                matchValues = new System.Collections.Generic.HashSet<int>
                {
                    (int)ItemType.食物
                }
            };
            PackageManager.instance.ShowPlayerBagUse(true, itemMatchData);
        }); 
    }
    void EscapeFightAction()
    {

    }
    void DisplayAutoFight(bool auto)
    {
        autoText.text = auto ? "手动" : "自动";
        AutoTips.localScale = auto ? Vector3.one : Vector3.zero;
        FightButton.interactable = !auto;
        EscapeFightButton.interactable = !auto;
        UIManager.instance.CloseGamePanel<WarehousePanel>();
    }
    void DisplayAutoExplore(bool auto)
    {
        going = true;
        goingText.text = "前进";
        autoExploreText.text = auto? "手动":"自动";
        AutoTips.localScale = auto ? Vector3.one:Vector3.zero;
        GoingButton.interactable = !auto;
        RetreatButton.interactable = !auto;
        UIManager.instance.CloseGamePanel<WarehousePanel>();
    }
    void StopAutoFight(StopAutoFight stopAutoFight)
    {
        ItemButtom.interactable = true;
        FightButton.interactable = true;
        EscapeFightButton.interactable = true;
        autoText.text =  "自动";
        AutoTips.localScale =  Vector3.zero;
    }
    void EndPlayerRound(EndPlayerRound endPlayerRound)
    { 
        ItemButtom.interactable = true;
    }
    bool going = true;
    void GoingAction()
    {
        if (going)
        {
            FightController.instance.StartWalk();
        }
        else
        {
            FightController.instance.StopWalk();
        }
        going = !going;
        if (going)
        {
            goingText.text = "前进";
        }
        else
        {
            goingText.text = "停止";
        }
    }

    void ExitFight()
    {
        FightController.instance.StopWalk();
        going = true;
        goingText.text = "前进";
        GameManager.instance.ShowTwoSelectAction("", "是否确认撤出战斗？", () => 
        {
            ExploreEnd exploreEnd = new ExploreEnd();
            GameActionManager.instance.QueueAction(exploreEnd);
        }, null);
    }

    public override async Task InitData(string dataKey)
    {
        dataId = int.Parse(dataKey);
        FightChapter fightChapter = ExploreManager.instance.GetFigehtChapter(dataId);
        if (fightChapter.mapId == dataId)
        {
            FightMapData fightMapData = await GameDataManager.instance.GetAsyncData<FightMapData>(dataId.ToString());
            MapName.text = fightMapData.mapName;
            ExploreValue.text = $"{fightChapter.completeValue}%";
        }
        FightManager.instance.RefreshFightPlayerInfo();
        DisplayAutoExplore(false);
    }
}