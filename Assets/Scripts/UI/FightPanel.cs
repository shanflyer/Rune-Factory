using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FightPanel : GamePanel
{
    [SerializeField]
    Transform FightCharacterParent;
    [SerializeField]
    FightCharacterReference FightCharacterReference;
    [SerializeField]
    Transform AutoTips,FightButtons,ExploreButtons;
    [SerializeField]
    Text MapName, ExploreValue;
    [SerializeField]
    Button FightButton, ItemButtom, AutoFightButton, SkillButton;
    [SerializeField]
    Image ValueImage;
    [SerializeField]
    Text SkillValue;
    [SerializeField]
    Button GoingButton, UsingButton, AutoButton, RetreatButton, SwitchButton;


    private int dataId;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        FightCharacterReference = FindChildGameObject<FightCharacterReference>("FightCharacterReference");
       
        FightCharacterParent = FindChildGameObject("FightCharacterParent");

        AutoTips = FindChildGameObject("AutoTips");
        MapName = FindChildGameObject<Text>("MapName");
        ExploreValue = FindChildGameObject<Text>("ExploreValue");
        FightButtons = FindChildGameObject("FightButtons");
        ExploreButtons = FindChildGameObject("ExploreButtons");

        ValueImage = FindChildGameObject<Image>("ValueImage");
        SkillValue = FindChildGameObject<Text>("SkillValue ");
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

    List<FightCharacterReference> fightCharacterReferences = new List<FightCharacterReference>();
    void RefreshFightCharacters(RefreshFightCharactersInfo refreshFightCharactersInfo)
    {
        if (fightCharacterReferences.Count> refreshFightCharactersInfo.characters.Count)
        {
            for (int i = refreshFightCharactersInfo.characters.Count; i < fightCharacterReferences.Count; i++)
            {
                fightCharacterReferences[i].transform.localScale = Vector3.zero;
            }
        }

        FightCharacterReference.transform.localScale = Vector3.zero;
        for (int i=0;i< refreshFightCharactersInfo.characters.Count;i++)
        {
            if (fightCharacterReferences.Count > i)
            {
                fightCharacterReferences[i].InitData(refreshFightCharactersInfo.characters[i]);
            }
            else
            {
                var fightCharacterReference = Instantiate(this.FightCharacterReference, FightCharacterParent);
                fightCharacterReference.transform.localScale = Vector3.one;
                fightCharacterReferences.Add(fightCharacterReference);
                fightCharacterReference.InitData(refreshFightCharactersInfo.characters[i]);
            }
        }
        
       
    }
    
    void RefreshFightCharacter(RefreshFightCharacterInfo refreshFightCharacterInfo)
    {
        for(int i = 0; i < fightCharacterReferences.Count; i++)
        {
            var fightCharacterReference = fightCharacterReferences[i];
            if (fightCharacterReference.CharacterId == refreshFightCharacterInfo.characterId)
            {
                fightCharacterReference.InitData(refreshFightCharacterInfo.characterId);
                break;
            }
        }
    }

    void RefreshFightChapter(RefreshFightChapter refreshFightChapter)
    {
        if (refreshFightChapter.id == dataId)
        {
            var fightChapter = ExploreManager.instance.GetFigehtChapter(refreshFightChapter.id);
            if (fightChapter.mapId == dataId)
            {
                ExploreValue.text = $"Ì½Ë÷¶È:{fightChapter.completeValue}%";
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
        GameActionManager.instance.RemoveListener<RefreshFightCharactersInfo>(RefreshFightCharacters);
        GameActionManager.instance.RemoveListener<SwitchFunctionButton>(SwitchFunctionButton);
        GameActionManager.instance.RemoveListener<RefreshFightCharacterInfo>(RefreshFightCharacter);
        base.OnDisable();
    }
    public override void OnEnable()
    {
        GameActionManager.instance.AddListener<RefreshFightChapter>(RefreshFightChapter);
        GameActionManager.instance.AddListener<RefreshFightCharactersInfo>(RefreshFightCharacters);
        GameActionManager.instance.AddListener<SwitchFunctionButton>(SwitchFunctionButton);
        GameActionManager.instance.AddListener<RefreshFightCharacterInfo>(RefreshFightCharacter);
        base.OnEnable();
    }
    protected override void Awake()
    {
        base.Awake();
        FightCharacterReference.transform.localScale = Vector3.zero;
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
            ExploreValue.text = $"Ì½Ë÷¶È:{fightChapter.completeValue}%";
        }
        FightManager.instance.RefreshFightPlayerInfo();
    }
}
