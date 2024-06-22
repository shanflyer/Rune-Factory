using System.Threading.Tasks;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class FightPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Transform AutoTips,  ExploreButtons;

    [SerializeField]
    private TextMeshProUGUI MapName, ExploreValue;

 
    [SerializeField]
    private Button GoingButton, UsingButton, AutoButton, RetreatButton, SwitchButton;
    [SerializeField]
    private TextMeshProUGUI goingText, autoText,autoExploreText;
    [SerializeField]
    FightCharacterReference fightCharacterReference;
    [SerializeField]
    Transform fightCharacterParent;
    [SerializeField]
    Button operateButton;
    [SerializeField]
    Transform operateIcon;
    [SerializeField]
    Animator animator;
    [SerializeField]
    FightCharacterCard fightCharacterCard;
    [SerializeField]
    Transform fightCharacterCardParent;
    DisplayList<FightCharacterCard, FightCharacter> fightCharacterCardList;

    DisplayList<FightCharacterReference, FightCharacter> fightCharacterReferenceList;

    private int dataId;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        AutoTips = FindChildGameObject("AutoTips");
        MapName = FindChildGameObject<TextMeshProUGUI>("MapName");
        ExploreValue = FindChildGameObject<TextMeshProUGUI>("ExploreValue"); 
        ExploreButtons = FindChildGameObject("ExploreButtons");
         

        GoingButton = FindChildGameObject<Button>("GoingButton");
        UsingButton = FindChildGameObject<Button>("UsingButton");
        AutoButton = FindChildGameObject<Button>("AutoButton");
        RetreatButton = FindChildGameObject<Button>("RetreatButton");
        SwitchButton = FindChildGameObject<Button>("SwitchButton");

        goingText = GoingButton.transform.GetComponentInChildren<TextMeshProUGUI>(); 
        autoExploreText=AutoButton.transform.GetComponentInChildren<TextMeshProUGUI>();

        fightCharacterParent = FindChildGameObject("CharacterInfoes");
        fightCharacterReference = FindChildGameObject<FightCharacterReference>("CharacterInfoReference");
        operateButton = FindChildGameObject<Button>("UpAndDown");
        operateIcon = FindChildGameObject("OperateIcon");
        animator = FindChildGameObject<Animator>("CharacterInfoes");

        fightCharacterCard = FindChildGameObject<FightCharacterCard>("FightCard");
        fightCharacterCardParent = FindChildGameObject("FightCardParent");
    }

    bool fight = false;
    bool auto = false;
    private void SwitchFunctionButton(SwitchFunctionButton switchFunctionButton)
    {
        fight = switchFunctionButton.fight;
        auto = switchFunctionButton.auto;

        DisplayAutoExplore(auto,fight);
        fightCharacterCardParent.gameObject.SetActive(fight); 
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
    public override void Close()
    {
        fight = false;
        auto = false;
        base.Close();
    }

    public override void OnDisable()
    {
        GameActionManager.instance.RemoveListener<RefreshFightChapter>(RefreshFightChapter);
        GameActionManager.instance.RemoveListener<SwitchFunctionButton>(SwitchFunctionButton); 
        GameActionManager.instance.RemoveListener<StopAutoFight>(StopAutoFight);
        GameActionManager.instance.RemoveListener<RefreshFightCharacterList>(RefreshFightCharacterList);
        base.OnDisable();
    }

    public override void OnEnable()
    {
        GameActionManager.instance.AddListener<RefreshFightChapter>(RefreshFightChapter);
        GameActionManager.instance.AddListener<SwitchFunctionButton>(SwitchFunctionButton); 
        GameActionManager.instance.AddListener<StopAutoFight>(StopAutoFight);
        GameActionManager.instance.AddListener<RefreshFightCharacterList>(RefreshFightCharacterList);
        AutoTips.localScale = Vector3.zero;
        base.OnEnable();
    }

    void RefreshFightCharacterList(RefreshFightCharacterList refreshFightCharacterList)
    {
        RefreshFightCardList();
    }

    PlayerFight playerFight = new PlayerFight();
   // StartRoundFight startRoundFight = new StartRoundFight();
    protected override void Awake()
    {
        base.Awake(); 

        AutoButton.onClick.AddListener(() =>
        {
            SwitchAutoExplore switchAutoExplore = new SwitchAutoExplore
            {
                setResult=(bool value)=> 
                {
                    auto = value;
                    DisplayAutoExplore(value, fight);
                }
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
        
        SwitchButton.onClick.AddListener(FightManager.instance.RoundPlayer);
        operateButton.onClick.AddListener(() =>
        {
            up = !up;
            animator.SetBool("Up", up);
            operateIcon.transform.localScale = up ? new Vector3(1,-1,1) : new Vector3(1, 1, 1);
        }); 

        fightCharacterReferenceList = new DisplayList<FightCharacterReference, FightCharacter>(fightCharacterReference, fightCharacterParent);
        fightCharacterCardList = new DisplayList<FightCharacterCard, FightCharacter>(fightCharacterCard, fightCharacterCardParent);
    } 
    void DisplayAutoExplore(bool auto,bool isfight)
    {
        going = true;
        goingText.text = "前进";
        autoExploreText.text = auto? "手动":"自动";
        AutoTips.localScale = auto ? Vector3.one:Vector3.zero;
        if (isfight)
        {
            GoingButton.interactable = false;
            RetreatButton.interactable = false;
            UsingButton.interactable = false;
            fightCharacterCardParent.gameObject.SetActive(true);
        }
        else
        {
            GoingButton.interactable = !auto;
            RetreatButton.interactable = !auto;
            fightCharacterCardParent.gameObject.SetActive(false);
        }
        
        UIManager.instance.CloseGamePanel<WarehousePanel>(); 
    }
    void StopAutoFight(StopAutoFight stopAutoFight)
    { 
        autoText.text =  "自动";
        AutoTips.localScale =  Vector3.zero;
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

    bool up = false;

    public override async Task InitData(string dataKey)
    {
        ExploreButtons.gameObject.SetActive(true);
         
        GoingButton.interactable = true; UsingButton.interactable = true; AutoButton.interactable = true; RetreatButton.interactable = true; SwitchButton.interactable = true;
        going = true;
        goingText.text = "前进";
        up = false;
        animator.SetBool("Up", up);
        operateIcon.transform.localScale = up ? new Vector3(1, -1, 1) : new Vector3(1, 1, 1);

        fight = false;
        auto = false;

        dataId = int.Parse(dataKey);
        FightChapter fightChapter = ExploreManager.instance.GetFigehtChapter(dataId);
        if (fightChapter.mapId == dataId)
        {
            FightMapData fightMapData = await GameDataManager.instance.GetAsyncData<FightMapData>(dataId.ToString());
            MapName.text = fightMapData.mapName;
            ExploreValue.text = $"{fightChapter.completeValue}%";
        }
        FightManager.instance.RefreshFightPlayerInfo();
        DisplayAutoExplore(false,false);

        var teamers = TeamManager.instance.TeamerEquipAndProperty;
        List<FightCharacter> teamerData = new List<FightCharacter>();
        for(int i = 0; i < teamers.characterEquipAndPropertyDatas.Length; i++)
        {
            var id = teamers.characterEquipAndPropertyDatas[i].id;
            if(FightManager.instance.GetFightCharacter(id,out var fightCharacter))
            {
                teamerData.Add(fightCharacter);
            } 
        }
         
        for (int i = teamerData.Count; i < 3; i++)
        {
            teamerData.Add(null);
        }
        fightCharacterReferenceList.InitListData(teamerData);
        up = false;
        animator.SetBool("Up", false);
        operateIcon.transform.localScale = up ? new Vector3(1, -1, 1) : new Vector3(1, 1, 1);

        //RefreshFightCardList();

    }
 
    void RefreshFightCardList()
    {
        fightCharacterCardParent.gameObject.SetActive(true);
        var fightCharacters = FightManager.instance.GetAllFightCharacters();
        fightCharacterCardList.InitListData(fightCharacters);
    }
}