using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TalkPanel : GamePanel<NPCTalkOperateData>
{
    [SerializeField]
    private Image rightHead, leftHead;

    [SerializeField]
    private Transform leftNameBg, rightNameBg;

    [SerializeField]
    private TextMeshProUGUI rightNameValue, leftNameValue;

    [SerializeField]
    private TextMeshProUGUI talkValue;

    [SerializeField]
    private TextMeshProUGUI tipes;

    [SerializeField]
    private Button nextButton;

    [SerializeField]
    private NPCFunctionReference NPCFunctionReference;

    [SerializeField]
    private Transform NPCFunctionParent;

    [SerializeField]
    private Transform close;

    [SerializeField]
    private Button closeButton;
    [SerializeField]
    Vector2 headSize = new Vector2(320, 320);
    private DisplayList<NPCFunctionReference, NPCFunctionData> NPCFunctionList;

    private TalkData talkData;
    private NPCTalkOperateData NPCTalkOperateData;
#if UNITY_EDITOR
    [SerializeField]
    private SpriteResourceRenference testIcon;
#endif


    protected override void Awake()
    {
        base.Awake();
        nextButton.onClick.AddListener(NextAction);
        closeButton.onClick.AddListener(Close);
        NPCFunctionList = new DisplayList<NPCFunctionReference, NPCFunctionData>(NPCFunctionReference, NPCFunctionParent);
    }

    public override void Close()
    {
        if (NPCTalkOperateData.endAction != null)
        {
            NPCTalkOperateData.endAction();
        }
        if (!SingletonType.Cleared)
        {
            TryContinueBehavior tryContinueBehavior = new TryContinueBehavior
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
        nextButton.interactable = false;
        var actionData = await GameDataManager.instance.GetAsyncData<GameActionAsset>(talkData.actionId.ToString());
        if (actionData != null)
        {
            actionData.Action();
        }
        talkData = await GameDataManager.instance.GetAsyncData<TalkData>(talkData.nexTalkId);
        await TalkAction();
        nextButton.interactable = true;
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
                    type = typeof(SimpleTalkPanel)
                };
                GameActionManager.instance.QueueAction(hidePanel);
                break;
            case 2:
                UIManager.instance.CloseGamePanel<SimpleTalkPanel>();
                UIManager.instance.CloseGamePanel<Team>();
                break;
        }
        List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>();

        EventReferenceData eventReferenceData = new EventReferenceData
        {
            name = "CharacterId",
            value = NPCTalkOperateData.characterId
        };
        eventReferenceDatas.Add(eventReferenceData);
        EventReferenceData targetReferenceData = new EventReferenceData
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
        {
            NPCFunctionList.InitListData(v.npcFunctionDatas, SelectNPCFunctionData);
        }
        else
        {
            NPCFunctionList.InitListData(new List<NPCFunctionData>());
        }

        InitData();
    }

    private bool runNextTalkEvent = false;
    private int talkId;
#if UNITY_EDITOR
    public void TestIcon()
    {
        testIcon.SetImageSprite(rightHead, headSize);
    }
#endif

    async Task TalkAction()
    {
        if (talkData == null)
        {
            if (!runNextTalkEvent)
            {
                runNextTalkEvent = true;
                List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>();
                EventReferenceData eventReferenceData1 = new EventReferenceData
                {
                    name = "withOutSource",
                    valueType = ReferenceValueType.IntList,
                    valeList = new List<int>()
                };
                eventReferenceData1.valeList.Add(talkId);
                eventReferenceDatas.Add(eventReferenceData1);
                bool nextEvent = await GameEventManager.instance.AddGameEvent(NPCTalkOperateData.nextTalkEventId,
                    eventReferenceDatas, true);
                if (!nextEvent)
                {
                    Close();
                }
            }
            else
            {
                Close();
            }
        }
        else
        {
            close.localScale = talkData.DisplayClose ? Vector3.one : Vector3.zero;
            talkValue.SetSWText(talkData.text);
            talkId = talkData.id;
            var talkerName = talkData.talkerName;
            var talkerIcon = talkData.talkerIcon;
            switch (talkData.talkSource)
            {
                case TalkSource.Player:
                    talkerName = GameDataSaveManager.instance.UserGameSaveData.playerData.name;
                    talkerIcon = CharacterManager.instance.PlayerHead;
                    break;

                case TalkSource.Dynamic:
                    Character character = CharacterManager.instance.GetCharacter(NPCTalkOperateData.characterId);
                    if (character == null)
                    {
                        talkerName = GameDataSaveManager.instance.UserGameSaveData.playerData.name;
                        talkerIcon = CharacterManager.instance.PlayerHead;
                    }
                    else
                    {
                        talkerName = character.name;
                        talkerIcon = character.characterData.head;
                    }
                    break;

                case TalkSource.Fixed:
                    break;
            }
            switch (talkData.talkerDir)
            {
                case TalkerDir.左:
                    leftNameValue.SetSWText(talkerName);
                    leftNameBg.gameObject.SetActive(true);
                    rightNameBg.gameObject.SetActive(false);
                    leftHead.color = Color.white;

                    talkerIcon.SetImageSprite(leftHead, headSize);
                    //leftHead.sprite = talkerIcon;
                    //leftHead.SetNativeSize(); 
                    rightHead.color = new Color(0.5f, 0.5f, 0.5f);
                    leftHead.enabled = true;
                    rightHead.enabled = !talkData.clearTalkIcon;
                    break;

                case TalkerDir.右:
                    rightNameValue.SetSWText(talkerName);
                    leftNameBg.gameObject.SetActive(false);
                    rightNameBg.gameObject.SetActive(true);
                    rightHead.color = Color.white;
                    talkerIcon.SetImageSprite(rightHead, headSize);
                    //rightHead.sprite = talkerIcon;
                    //rightHead.SetNativeSize();
                    leftHead.color = new Color(0.5f, 0.5f, 0.5f);
                    rightHead.enabled = true;
                    leftHead.enabled = !talkData.clearTalkIcon;
                    break;

                case TalkerDir.无:
                    leftNameBg.gameObject.SetActive(false);
                    rightNameBg.gameObject.SetActive(false);
                    rightHead.enabled = !talkData.clearTalkIcon;
                    leftHead.enabled = !talkData.clearTalkIcon;
                    rightHead.color = new Color(0.5f, 0.5f, 0.5f);
                    leftHead.color = new Color(0.5f, 0.5f, 0.5f);
                    break;
            }

            NPCFunctionParent.localScale = NPCTalkOperateData.displayFunction ? Vector3.one : Vector3.zero;
        }
    }
    private async void InitData()
    {
        await TalkAction();
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        rightHead = FindChildGameObject<Image>("RightHeadIcon");
        leftHead = FindChildGameObject<Image>("LeftHeadIcon");

        rightNameBg = FindChildGameObject("RightName");
        leftNameBg = FindChildGameObject("LeftName");
        rightNameValue = FindChildGameObject<TextMeshProUGUI>("RightNameValue");
        leftNameValue = FindChildGameObject<TextMeshProUGUI>("LeftNameValue");
        talkValue = FindChildGameObject<TextMeshProUGUI>("Value");

        tipes = FindChildGameObject<TextMeshProUGUI>("Tipes");
        nextButton = FindChildGameObject<Button>("Next");
        close = FindChildGameObject("Close");
        closeButton = FindChildGameObject<Button>("CloseButton");

        NPCFunctionReference = FindChildGameObject<NPCFunctionReference>("NPCFunctionReference");
        NPCFunctionParent = FindChildGameObject("Functions");
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TalkPanel))]
    public class TalkPanelEditor : Editor
    {
        public TalkPanel TalkPanel=>target as TalkPanel;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("TestIcon"))
            {
                TalkPanel.TestIcon();
            }
        }
    }
#endif
}