using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FightPanel : GamePanel<IReferenceData>
{ 
    [SerializeField]
    Transform AutoTips,FightButtons,ExploreButtons;
    [SerializeField]
    TextMeshProUGUI MapName, ExploreValue;
    [SerializeField]
    Button FightButton, ItemButtom, AutoFightButton, SkillButton; 
    [SerializeField]
    Button GoingButton, UsingButton, AutoButton, RetreatButton, SwitchButton;


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
        ItemButtom = FindChildGameObject<Button>("ItemButtom");
        AutoFightButton = FindChildGameObject<Button>("AutoFightButton"); 
         
        GoingButton = FindChildGameObject<Button>("GoingButton");
        UsingButton = FindChildGameObject<Button>("UsingButton"); 
        AutoButton = FindChildGameObject<Button>("AutoButton"); 
        RetreatButton = FindChildGameObject<Button>("RetreatButton"); 
        SwitchButton = FindChildGameObject<Button>("SwitchButton");
    }

    void SwitchFunctionButton(SwitchFunctionButton switchFunctionButton)
    {
        bool fight = switchFunctionButton.fight;
        FightButtons.localScale = fight ? Vector3.one : Vector3.zero;
        ExploreButtons.localScale = fight ? Vector3.zero:Vector3.one;
    }
     

    void RefreshFightChapter(RefreshFightChapter refreshFightChapter)
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
    public void SetAuto(bool auto)
    {
        AutoTips.localScale = auto ? Vector3.one : Vector3.zero;
    }


    public override void OnDisable()
    {
        GameActionManager.instance.RemoveListener<RefreshFightChapter>(RefreshFightChapter); 
        GameActionManager.instance.RemoveListener<SwitchFunctionButton>(SwitchFunctionButton); 
        base.OnDisable();
    }
    public override void OnEnable()
    {
        GameActionManager.instance.AddListener<RefreshFightChapter>(RefreshFightChapter); 
        GameActionManager.instance.AddListener<SwitchFunctionButton>(SwitchFunctionButton); 
        base.OnEnable();
    }
    protected override void Awake()
    {
        base.Awake(); 
        ExploreButtons.transform.localScale = Vector3.one;
        FightButtons.transform.localScale = Vector3.zero;
    }
    public override async Task InitData(string dataKey)
    {
        int dataId = int.Parse(dataKey);
        FightChapter fightChapter = ExploreManager.instance.GetFigehtChapter(dataId);
        if (fightChapter.mapId == dataId)
        {
            FightMapData fightMapData=await GameDataManager.instance.GetAsyncData<FightMapData>(dataId.ToString());
            MapName.text = fightMapData.mapName;
            ExploreValue.text = $"{fightChapter.completeValue}%";
        }
        FightManager.instance.RefreshFightPlayerInfo();
    }
}
