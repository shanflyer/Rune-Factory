using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
#endif

public class SimpleTalkPanel : GamePanel<NPCTalkOperateData>
{
    [SerializeField] private Image RightHandIcon, LeftHandIcon;

    [SerializeField] private Transform RightHand, LeftHand, Content;

    [SerializeField] private TextMeshProUGUI RightNameValue, LeftNameValue;

    [SerializeField] private TextMeshProUGUI TalkValue;


    [SerializeField] private Button _nextButton;

    [SerializeField] private NPCFunctionReference NPCFunctionReference;

    [SerializeField] private Transform NPCFunctions;

    [SerializeField] private Button CloseBtn;
    private DisplayList<NPCFunctionReference, NPCFunctionData> NPCFunctionList;

    private TalkData talkData;
    private NPCTalkOperateData NPCTalkOperateData;

    private Vector3 zeroContentPosition;
    protected override void Awake()
    {
        base.Awake();
        zeroContentPosition = Content.localPosition;
        _nextButton.onClick.AddListener(NextAction);
        CloseBtn.onClick.AddListener(Close);
        NPCFunctionList = new DisplayList<NPCFunctionReference, NPCFunctionData>(NPCFunctionReference, NPCFunctions);
    }

    public void TryClose(int characterId)
    {
        if (NPCTalkOperateData.characterId == characterId) Close();
    }

    public override void Close()
    {
        if (NPCTalkOperateData.endAction != null) NPCTalkOperateData.endAction();
        if (!SingletonType.Cleared)
        {
            var tryContinueBehavior = new TryContinueBehavior
            {
                characterId = NPCTalkOperateData.characterId
            };
            GameActionManager.instance.QueueAction(tryContinueBehavior);
        }

        base.Close();
    }

    private async void NextAction()
    {
        // Debug.Log("Talk:NextAction!!!");
        _nextButton.interactable = false;
        var actionData = await GameDataManager.instance.GetAsyncData<GameActionData>(talkData.actionId.ToString());
        if (actionData != null) actionData.Action();
        talkData = await GameDataManager.instance.GetAsyncData<TalkData>(talkData.nexTalkId);
        await TalkAction();
        _nextButton.interactable = true;
    }

    private async void SelectNPCFunctionData(NPCFunctionData NPCFunctionData, int index, bool selected = true)
    {
        switch (NPCFunctionData.closeTalk)
        {
            case 0:
                break;
            case 1:
                var hidePanel = new HidePanel
                {
                    hide = true,
                    type = typeof(SimpleTalkPanel)
                };
                GameActionManager.instance.QueueAction(hidePanel);
                break;
            case 2:
                UIManager.instance.CloseGamePanel<SimpleTalkPanel>();
                UIManager.instance.CloseGamePanel<Team>();
                break;
        }


        var eventReferenceDatas = new List<EventReferenceData>();

        var eventReferenceData = new EventReferenceData
        {
            name = "CharacterId",
            value = NPCTalkOperateData.characterId
        };
        eventReferenceDatas.Add(eventReferenceData);
        var targetReferenceData = new EventReferenceData
        {
            name = "SourceCharacter",
            value = CharacterManager.instance.controllerCharacter.instanceId
        };
        eventReferenceDatas.Add(targetReferenceData);
        var GameEventData = await GameDataManager.instance.GetAsyncData<GameEventData>(NPCFunctionData.OperateAction);
        GameEventManager.instance.AddGameEvent(GameEventData, eventReferenceDatas);
    }

    public override void InitReferenceData(NPCTalkOperateData v)
    {
        base.InitReferenceData(v);
        runNextTalkEvent = false;
        NPCTalkOperateData = v;
        talkData = v.defaultTalk;

        if (v.displayFunction)
            NPCFunctionList.InitListData(v.npcFunctionDatas, SelectNPCFunctionData);
        else
            NPCFunctionList.InitListData(new List<NPCFunctionData>());

        InitData();
    }

    private bool runNextTalkEvent;
    private int talkId;

    private async Task TalkAction()
    {
        if (talkData == null)
        {
            if (!runNextTalkEvent)
            {
                runNextTalkEvent = true;
                var eventReferenceDatas = new List<EventReferenceData>();
                var eventReferenceData1 = new EventReferenceData
                {
                    name = "withOutSource",
                    valueType = ReferenceValueType.IntList,
                    valeList = new List<int>()
                };
                eventReferenceData1.valeList.Add(talkId);
                eventReferenceDatas.Add(eventReferenceData1);
                var nextEvent = await GameEventManager.instance.AddGameEvent(NPCTalkOperateData.nextTalkEventId,
                    eventReferenceDatas, true);
                if (!nextEvent) Close();
            }
            else
            {
                Close();
            }
        }
        else
        {
            CloseBtn.transform.localScale = talkData.DisplayClose ? Vector3.one : Vector3.zero;
            TalkValue.SetSWText(talkData.text);
            talkId = talkData.id;
            var talkerName = talkData.talkerName;
            var talkerIcon = talkData.talkerIcon;
            Transform characterTransform = null;
            switch (talkData.talkSource)
            {
                case TalkSource.Player:
                    talkerName = GameDataSaveManager.instance.UserGameSaveData.playerData.name;
                    talkerIcon = CharacterManager.instance.PlayerHead;
                    if (CharacterManager.instance.ControllerRuntimeObj != null)
                        characterTransform = CharacterManager.instance.ControllerRuntimeObj.transform;
                    break;

                case TalkSource.Dynamic:
                    var character = CharacterManager.instance.GetCharacter(NPCTalkOperateData.characterId);
                    if (character == null)
                    {
                        talkerName = GameDataSaveManager.instance.UserGameSaveData.playerData.name;
                        talkerIcon = CharacterManager.instance.PlayerHead;
                    }
                    else
                    {
                        talkerName = character.name;
                        talkerIcon = character.characterData.head;
                        if (CharacterManager.instance.GetRuntimeCharacterObj(character.instanceId,
                                out var characterObj))
                            characterTransform = characterObj.transform;
                    }

                    break;

                case TalkSource.Fixed:
                    break;
            }

            switch (talkData.talkerDir)
            {
                case TalkerDir.左:
                    LeftNameValue.SetSWText(talkerName);
                    LeftHand.gameObject.SetActive(true);
                    RightHand.gameObject.SetActive(false);
                    LeftHandIcon.color = Color.white;
                    LeftHandIcon.sprite = talkerIcon.sprite;
                    LeftHandIcon.enabled = true;
                    RightHandIcon.color = new Color(0.5f, 0.5f, 0.5f);
                    RightHandIcon.enabled = !talkData.clearTalkIcon;
                    break;

                case TalkerDir.右:
                    RightNameValue.SetSWText(talkerName);
                    LeftHand.gameObject.SetActive(false);
                    RightHand.gameObject.SetActive(true);
                    RightHandIcon.color = Color.white;
                    RightHandIcon.sprite = talkerIcon.sprite;
                    RightHandIcon.enabled = true;
                    LeftHandIcon.color = new Color(0.5f, 0.5f, 0.5f);
                    LeftHandIcon.enabled = !talkData.clearTalkIcon;
                    break;

                case TalkerDir.无:
                    LeftNameValue.gameObject.SetActive(false);
                    RightNameValue.gameObject.SetActive(false);
                    RightHandIcon.enabled = !talkData.clearTalkIcon;
                    LeftHandIcon.enabled = !talkData.clearTalkIcon;
                    RightHandIcon.color = new Color(0.5f, 0.5f, 0.5f);
                    LeftHandIcon.color = new Color(0.5f, 0.5f, 0.5f);
                    break;
            }

            NPCFunctions.localScale = NPCTalkOperateData.displayFunction ? Vector3.one : Vector3.zero;
            if (characterTransform != null)
            {
                var screenPos = CameraManager.WorldPointToScreenPoint(characterTransform.position);
                if (CameraManager.ScreenPointToUILocalPoint(transform as RectTransform, screenPos, out var UIPos))
                {
                    var ContentPosition = Content.localPosition;
                    ContentPosition.y = UIPos.y;
                    Content.localPosition = ContentPosition;
                }
            }
            else
            {
                Content.localPosition = zeroContentPosition;
            }
        }
    }

    private async void InitData()
    {
        await TalkAction();
    }
}