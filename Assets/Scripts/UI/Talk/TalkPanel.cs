using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalkPanel : GamePanel<NPCTalkOperateData>
{
    [SerializeField]
    Image rightHead, leftHead, leftNameBg, rightNameBg;
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

    DisplayList<NPCFunctionReference, NPCFunctionData> NPCFunctionList;

    TalkData talkData;
    protected override void Awake()
    {
        base.Awake();
        nextButton.onClick.AddListener(NextAction);
        NPCFunctionList = new DisplayList<NPCFunctionReference, NPCFunctionData>(NPCFunctionReference, NPCFunctionParent);
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
        talkData = v.defaultTalk;
        NPCFunctionList.InitListData(v.npcFunctionDatas); 
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
        var talkerName = talkData.myTalk ? CharacterManager.instance.player.name : talkData.talkerName;
        Sprite talkerIcon = talkData.myTalk ? CharacterManager.instance.PlayerIcon : talkData.talkerIcon;

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
        rightNameBg = FindChildGameObject<Image>("RightName");
        leftNameBg = FindChildGameObject<Image>("LeftName");
        rightNameValue = FindChildGameObject<TextMeshProUGUI>("RightNameValue");
        leftNameValue = FindChildGameObject<TextMeshProUGUI>("LeftNameValue");
        talkValue = FindChildGameObject<TextMeshProUGUI>("Value"); 

        tipes = FindChildGameObject<TextMeshProUGUI>("Tipes");
        nextButton = FindChildGameObject<Button>("Next");

        NPCFunctionReference = FindChildGameObject<NPCFunctionReference>("NPCFunctionReference");
        NPCFunctionParent = FindChildGameObject("Functions");
    }

}