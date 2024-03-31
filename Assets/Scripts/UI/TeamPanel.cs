using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class TeamPanel : GamePanel<CharacterInformationDataList>
{
    [SerializeField]
    Transform teamerparent;
    [SerializeField]
    TeamerReference TeamerReference;
    [SerializeField]
    ToggleGroup toggleGroup;

    [SerializeField]
    Button leaveButton;
    [SerializeField]
    Button talkButton;
    [SerializeField]
    Transform operatePanel;
    DisplayList<TeamerReference, CharacterInformationData> teamerList;
    protected override void Awake()
    {
        base.Awake();
        teamerList = new DisplayList<TeamerReference, CharacterInformationData>(TeamerReference, teamerparent);
        talkButton.onClick.AddListener(TalkAction);
        leaveButton.onClick.AddListener(LeaveAction);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        teamerparent = FindChildGameObject("TeamerList");
        TeamerReference = FindChildGameObject<TeamerReference>("TeamerReference");
        toggleGroup = teamerparent.gameObject.GetComponent<ToggleGroup>();
        talkButton = FindChildGameObject<Button>("Talk");
        leaveButton = FindChildGameObject<Button>("Leave");
        operatePanel = FindChildGameObject("Operate");
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshTeam>(RefreshTeam);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshTeam>(RefreshTeam);
    }
    void TalkAction()
    {
        Character character = CharacterManager.instance.GetCharacter(SelectCharacterId);
        if (character != null)
        {
            EventReferenceData eventReferenceData = new EventReferenceData
            {
                name = "CharacterId",
                value = SelectCharacterId
            };
            EventReferenceData targetReferenceData = new EventReferenceData
            {
                name = "TargetCharacter",
                value = CharacterManager.instance.controllerCharacter.instanceId
            };
            EventReferenceData NextTalkReferenceData = new EventReferenceData
            {
                name = "NextTalkEventId",
                value = character.characterData.nextTalkEventId
            };
            bool temp = character is TempCharacter;
            GameEventManager.instance.AddGameEvent(
            temp ? character.characterData.playerOperateEventId : character.characterData.playerOperateEventId, new List<EventReferenceData>
            {
                    eventReferenceData,targetReferenceData,NextTalkReferenceData
            });
        }
    }
    void LeaveAction()
    {
        if(PastureManager.instance.GetAnimal(SelectCharacterId,out var animal))
        {
            GameManager.instance.ShowTwoSelectAction("移除", "该动物没有分配牧场，移除后将消失，是否确定移除？", () =>
            { 
                LeaveTeam leaveTeam = new LeaveTeam
                {
                    teamCharacterId = SelectCharacterId, 
                };
                GameActionManager.instance.QueueAction(leaveTeam);
            },null);
        }
        else
        {

            LeaveTeam leaveTeam = new LeaveTeam
            {
                teamCharacterId = SelectCharacterId, 
            };
            GameActionManager.instance.QueueAction(leaveTeam);
        } 
        
    }
    public override void Close()
    {
        UIManager.instance.CloseGamePanel<CharacterInformationPanel>();
        base.Close();
    }
    void RefreshTeam(RefreshTeam refreshTeam)
    {
        var myTeamInfo = TeamManager.instance.GetMyTeamCharacterInfo();
        if (myTeamInfo.characterInformationDatas.Count <= 1)
        {
            Close();
        }
        else
        {
            InitReferenceData(myTeamInfo);
        }
    }

    int SelectCharacterId = 0;
    void SelectAction(CharacterInformationData characterInformationData,bool select)
    {
        SelectCharacterId = characterInformationData.characterId;
        if (characterInformationData.characterId == CharacterManager.instance.controllerCharacter.instanceId)
        {
            operatePanel.gameObject.SetActive(false);
        }
        else
        {
            operatePanel.gameObject.SetActive(true);
        }
        UIManager.instance.ShowGamePanel<CharacterInformationPanel, CharacterInformationData>(characterInformationData);
    }
    public override void InitReferenceData(CharacterInformationDataList v)
    {
        base.InitReferenceData(v);
        teamerList.InitListData(v.characterInformationDatas, SelectAction, toggleGroup);
        teamerList.Select(v.characterInformationDatas[0]);
    }
}
