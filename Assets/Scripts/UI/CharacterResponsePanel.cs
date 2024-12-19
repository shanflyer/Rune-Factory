using System;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

public struct CharacterResponseData : IReferenceData
{
    public string talkValue;
    public Sprite icon;
    public AnimationClip clip;
    public int displayTime;
    public Action endAction;
}

public class CharacterResponsePanel : GamePanel<CharacterResponseData>
{ 

    [SerializeField]
    private TextMeshProUGUI TalkValue;

    [SerializeField]
    private Image Icon;

    [SerializeField]
    private Animator Animator;

    private PlayableGraph graph;
    private AnimationPlayableOutput animationPlayableOutput;

    protected override void Awake()
    {
        base.Awake();
        graph = PlayableGraph.Create("ResponseGraph");
        animationPlayableOutput = AnimationPlayableOutput.Create(graph, "AnimationOutput", Animator);
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        TalkValue = FindChildGameObject<TextMeshProUGUI>("TalkValue");
        Icon = FindChildGameObject<Image>("Icon");
        Animator = FindChildGameObject<Animator>("Icon");
    }

    private CharacterResponseData characterResponseData;

    public override void InitReferenceData(CharacterResponseData v)
    {
        base.InitReferenceData(v);
        characterResponseData = v;
        TalkValue.text = characterResponseData.talkValue;
        Icon.sprite = characterResponseData.icon;
        Icon.SetNativeSize();
        var clipPlayable = AnimationClipPlayable.Create(graph, characterResponseData.clip);
        animationPlayableOutput.SetSourcePlayable(clipPlayable);
        graph.Play();

        GameTimerController.instance.DelayAction(characterResponseData.displayTime == 0 ? GameCommon.defaultPlayerTalkTime : characterResponseData.displayTime,
            () =>
            {
                if (characterResponseData.endAction != null)
                {
                    characterResponseData.endAction();
                }
                Close();
            });
    }
}