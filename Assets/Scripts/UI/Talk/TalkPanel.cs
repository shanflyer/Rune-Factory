using System.Collections.Generic;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class TalkPanel : GamePanel<NPCTalkOperateData>
{
    [SerializeField]
    Image rightHead, leftHead; 

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
    Transform close;
    [SerializeField]
    private Button closeButton;

    private DisplayList<NPCFunctionReference, NPCFunctionData> NPCFunctionList;

    private TalkData talkData;
    private NPCTalkOperateData NPCTalkOperateData;

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
        base.Close();
    }

    private async void NextAction()
    {
        var actionData = await GameDataManager.instance.GetAsyncData<GameActionData>(talkData.actionId.ToString());
        if (actionData != null)
        {
            actionData.Action();
        }
        talkData = await GameDataManager.instance.GetAsyncData<TalkData>(talkData.nexTalkId);
        InitData();
    }

    private async void SelectNPCFunctionData(NPCFunctionData NPCFunctionData, bool selected = true)
    {
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
            NPCFunctionList.InitListData(null);
        }

        InitData();
    }

    private bool runNextTalkEvent = false;

    private async void InitData()
    {
        if (talkData == null)
        {
            if (!runNextTalkEvent)
            {
                runNextTalkEvent = true;
                bool nextEvent = await GameEventManager.instance.AddGameEvent(NPCTalkOperateData.nextTalkEventId);
                if (!nextEvent)
                {
                    Close(); 
                }
            }
            else
            {
                Close(); 
            }
           
        }else
        {
            close.localScale= talkData.DisplayClose? Vector3.one : Vector3.zero;  
            talkValue.text = talkData.text; 
            var talkerName = talkData.talkerName;
            Sprite talkerIcon = talkData.talkerIcon;
            if (talkData.myTalk)
            {
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
            }

            switch (talkData.talkerDir)
            {
                case TalkerDir.左:
                    leftNameValue.text = talkerName;
                    leftNameBg.gameObject.SetActive(true);
                    rightNameBg.gameObject.SetActive(!talkData.clearTalkIcon);
                    leftHead.color = Color.white;
                    leftHead.sprite = talkerIcon;
                    leftHead.SetNativeSize();
                    rightHead.color = new Color(0.5f, 0.5f, 0.5f);
                    leftHead.enabled = true;
                    rightHead.enabled= !talkData.clearTalkIcon;
                    break;

                case TalkerDir.右:
                    rightNameValue.text = talkerName;
                    leftNameBg.gameObject.SetActive(!talkData.clearTalkIcon);
                    rightNameBg.gameObject.SetActive(true);
                    rightHead.color = Color.white;
                    rightHead.sprite = talkerIcon;
                    rightHead.SetNativeSize();
                    leftHead.color = new Color(0.5f, 0.5f, 0.5f);
                    rightHead.enabled = true;
                    leftHead.enabled = !talkData.clearTalkIcon;
                    break;

                case TalkerDir.无:
                    leftNameBg.gameObject.SetActive(!talkData.clearTalkIcon);
                    rightNameBg.gameObject.SetActive(!talkData.clearTalkIcon);
                    rightHead.enabled = !talkData.clearTalkIcon;
                    leftHead.enabled = !talkData.clearTalkIcon;
                    rightHead.color = new Color(0.5f, 0.5f, 0.5f);
                    leftHead.color = new Color(0.5f, 0.5f, 0.5f);
                    break;
            }

            NPCFunctionParent.localScale = !talkData.clearTalkIcon ? Vector3.one : Vector3.zero;
        } 
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        rightHead = FindChildGameObject<Image>("RightHead");
        leftHead = FindChildGameObject<Image>("LeftHead");

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
}