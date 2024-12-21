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
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<RefreshTeam>(RefreshTeam);
    }
    async void TalkAction()
    {
        Character character = CharacterManager.instance.GetCharacter(SelectCharacterId);
        if (character != null)
        {
            if (!PastureManager.instance.TalkAnimal(character.instanceId))
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

                int nextTalkEventId = 0; int eventId = 0;
                if (character is TempCharacter tempCharacter)
                {
                    nextTalkEventId = tempCharacter.tempCharacterData.nextTalkEventId;
                    eventId = tempCharacter.tempCharacterData.tempTalkEventId;
                }
                else if (NPCManager.instance.GetNPCFormInstance(character.instanceId, out var NPC))
                {
                    nextTalkEventId = NPC.nextTalkEventId;
                    eventId = NPC.playerOperateEventId;
                }

                EventReferenceData NextTalkReferenceData = new EventReferenceData
                {
                    name = "NextTalkEventId",
                    value = nextTalkEventId
                };
                bool temp = character is TempCharacter;
                await GameEventManager.instance.AddGameEvent(eventId, new List<EventReferenceData>
            {
                    eventReferenceData,targetReferenceData,NextTalkReferenceData
            });

            }
            
        }
    }
    void LeaveAction()
    {
        if(PastureManager.instance.GetAnimal(SelectCharacterId,out var animal))
        {
            GameManager.instance.ShowTwoSelectAction("移除队伍", "该动物没有分配牧场，移除后将消失，是否确定移除？", () =>
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
            InformationController.instance.AddInformation("动物已经回到牧场", true, true);
        } 
        
    }
    public override void Close()
    {
        base.Close();
        SelectCharacterId = 0;
        UIManager.instance.CloseGamePanel<CharacterInformationPanel>(); 
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
    async void SelectAction(CharacterInformationData characterInformationData,bool select)
    {
        if (select)
        {
            var panel = await UIManager.instance.ShowGamePanel<CharacterInformationPanel, CharacterInformationData>(characterInformationData);
            if (panel == null)
            {
                return;
            }
            SelectCharacterId = characterInformationData.characterId;
            if (characterInformationData.characterId == CharacterManager.instance.controllerCharacter.instanceId)
            {
                operatePanel.gameObject.SetActive(false);
            }
            else
            {
                operatePanel.gameObject.SetActive(true);
            } 
            panel.HideBackGround(true);
        }
        else if(SelectCharacterId != characterInformationData.characterId)
        {
            SelectCharacterId = 0;
            operatePanel.gameObject.SetActive(false);
          //  UIManager.instance.CloseGamePanel<CharacterInformationPanel>();
        }
    }
    public override async void InitReferenceData(CharacterInformationDataList v)
    {
        base.InitReferenceData(v);
        await teamerList.InitListData(v.characterInformationDatas, SelectAction, toggleGroup);
        teamerList.Select(v.characterInformationDatas[0]); 
    }
}
