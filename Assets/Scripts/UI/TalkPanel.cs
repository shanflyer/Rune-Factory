using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TalkPanel : GamePanel
{
    [SerializeField]
    Image rightHead, leftHead, leftNameBg, rightNameBg;
    [SerializeField]
    Text rightNameValue, leftNameValue;
    [SerializeField]
    Text talkValue;
    [SerializeField]
    GameObject normalObj, loveObjObj;
    [SerializeField]
    Text tipes;
    [SerializeField]
    Button nextButton;

    TalkData talkData;
    protected override void Awake()
    {
        base.Awake();
        nextButton.onClick.AddListener(NextAction);
    }
    public void ShowLove(bool love)
    {
        normalObj.transform.localScale = love ? Vector3.zero : Vector3.one;
        loveObjObj.transform.localScale = love ? Vector3.one : Vector3.zero;

        leftNameBg.color=rightNameBg.color=love? new Color(0.95f, 0.3f, 0.8f, 0.7f): new Color(0.4f,0,0.8f,0.7f);
        tipes.color=love? new Color(0.95f, 0.3f, 0.8f, 0.7f) : new Color(0.4f, 0, 0.8f, 0.7f);
    }
    async void NextAction()
    {
        var actionData = await GameDataManager.instance.GetAsyncData<GameActionData>(talkData.actionId.ToString());
        if (actionData != null)
        {
            actionData.Action();
        }
        InitData(talkData.nexTalkId.ToString());
    }

    public override async Task InitData(string dataKey)
    {
        talkData=await GameDataManager.instance.GetAsyncData<TalkData>(dataKey);
        if (talkData == null)
        {
            Close();
            return;
        }
        talkValue.text = talkData.text;
        var talkerName = talkData.myTalk ? CharacterManager.instance.player.name: talkData.talkerName;
        Sprite talkerIcon = talkData.myTalk ? CharacterManager.instance.PlayerIcon : talkData.talkerIcon;
        
        switch (talkData.talkerDir)
        {
            case TalkerDir.左:
                leftNameValue.text = talkerName;
                leftNameBg.transform.localScale=Vector3.one;
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
                leftHead.enabled=!talkData.clearTalkIcon;
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
        rightNameValue = FindChildGameObject<Text>("RightNameValue");
        leftNameValue = FindChildGameObject<Text>("LeftNameValue");
        talkValue = FindChildGameObject<Text>("Value");

        normalObj = FindChildGameObject("Bg").gameObject;
        loveObjObj = FindChildGameObject("LoveBg").gameObject;

        tipes = FindChildGameObject<Text>("Tipes");
        nextButton = FindChildGameObject<Button>("Next");
    }

}