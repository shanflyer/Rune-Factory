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
            if (PastureManager.instance.GetAnimal(character.instanceId, out var animal))
            {
                Talk talk = new Talk
                {
                    characterId = character.instanceId,
                    talkId =animal.growthStage<1? animal.animalData.talkId.x:animal.animalData.talkId.y,
                    displayFunction = true,
                    fixedFunctions = new List<int>
                    {
                        9,10,11
                    },
                   
                };
                GameActionManager.instance.QueueAction(talk);
            }
            else
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
        SelectCharacterId = characterInformationData.characterId;
        if (characterInformationData.characterId == CharacterManager.instance.controllerCharacter.instanceId)
        {
            operatePanel.gameObject.SetActive(false);
        }
        else
        {
            operatePanel.gameObject.SetActive(true);
        }
       var panel= await  UIManager.instance.ShowGamePanel<CharacterInformationPanel, CharacterInformationData>(characterInformationData);
        panel.HideBackGround(true);
    }
    public override async void InitReferenceData(CharacterInformationDataList v)
    {
        base.InitReferenceData(v);
        await teamerList.InitListData(v.characterInformationDatas, SelectAction, toggleGroup);
        //teamerList.Select(v.characterInformationDatas[0]); 
    }
}
