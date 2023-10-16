using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalkPanel : GamePanel<NPCTalkOperateData>
{
    [SerializeField]
    Image rightHead, leftHead;
    [SerializeField]
    Transform leftNameBg, rightNameBg;
    [SerializeField]
    TextMeshProUGUI rightNameValue, leftNameValue;
    [SerializeField]
    TextMeshProUGUI talkValue;
    [SerializeField]
    TextMeshProUGUI tipes;
    [SerializeField]
    Button nextButton;
    [SerializeField]
    NPCFunctionReference NPCFunctionReference;
    [SerializeField]
    Transform NPCFunctionParent;
    [SerializeField]
    Button closeButton;

    DisplayList<NPCFunctionReference, NPCFunctionData> NPCFunctionList;

    TalkData talkData;
    NPCTalkOperateData NPCTalkOperateData;
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
    async void NextAction()
    {
        var actionData = await GameDataManager.instance.GetAsyncData<GameActionData>(talkData.actionId.ToString());
        if (actionData != null)
        {
            actionData.Action();
        }
        talkData = await GameDataManager.instance.GetAsyncData<TalkData>(talkData.nexTalkId); 
        InitData();
    }
    public override void InitReferenceData(NPCTalkOperateData v)
    {
        base.InitReferenceData(v);
        NPCTalkOperateData = v;
        talkData = v.defaultTalk;
        if (v.displayFunction)
        {
            NPCFunctionList.InitListData(v.npcFunctionDatas);
        }
        else
        {
            NPCFunctionList.InitListData(null);
        }
        
        InitData();
    }
    void InitData()
    {
        if (talkData == null)
        {
            Close();
            return;
        }
        talkValue.text = talkData.text;

        var talkerName = talkData.talkerName;
        Sprite talkerIcon =  talkData.talkerIcon;
        if (talkData.myTalk)
        {
            Character character = CharacterManager.instance.GetCharacter(NPCTalkOperateData.characterId);
            if (character == null)
            {
                talkerName =  CharacterManager.instance.PlayerName;
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
                leftNameBg.transform.localScale = Vector3.one;
                rightNameBg.transform.localScale = Vector3.zero;
                leftHead.color = Color.white;
                leftHead.sprite = talkerIcon;
                rightHead.color = new Color(0.5f, 0.5f, 0.5f);
                rightHead.enabled = !talkData.clearTalkIcon;
                break;
            case TalkerDir.右:
                rightNameValue.text = talkerName;
                leftNameBg.transform.localScale = Vector3.zero;
                rightNameBg.transform.localScale = Vector3.one;
                rightHead.color = Color.white;
                rightHead.sprite = talkerIcon;
                leftHead.color = new Color(0.5f, 0.5f, 0.5f);
                leftHead.enabled = !talkData.clearTalkIcon;
                break;
            case TalkerDir.无:
                leftNameBg.transform.localScale = Vector3.zero;
                rightNameBg.transform.localScale = Vector3.zero;
                rightHead.enabled = !talkData.clearTalkIcon;
                leftHead.enabled = !talkData.clearTalkIcon;
                rightHead.color = new Color(0.5f, 0.5f, 0.5f);
                leftHead.color = new Color(0.5f, 0.5f, 0.5f);
                break;
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
        closeButton = FindChildGameObject<Button>("Close");

        NPCFunctionReference = FindChildGameObject<NPCFunctionReference>("NPCFunctionReference");
        NPCFunctionParent = FindChildGameObject("Functions");
    }

}