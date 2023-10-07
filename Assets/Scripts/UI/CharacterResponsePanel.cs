using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Animations;

public struct CharacterResponseData:IReferenceData
{
    public string talkValue;
    public Sprite icon;
    public AnimationClip clip;
    public int displayTime;
}
public class CharacterResponsePanel : GamePanel<CharacterResponseData>
{
    public override bool pluralUI => true;

    [SerializeField]
    TextMeshProUGUI TalkValue;
    [SerializeField]
    Image Icon;
    [SerializeField]
    Animator Animator;

    PlayableGraph graph;
    AnimationPlayableOutput animationPlayableOutput;
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
    CharacterResponseData characterResponseData;
    public override void InitReferenceData(CharacterResponseData v)
    {
        base.InitReferenceData(v);
        characterResponseData = v;
        TalkValue.text = characterResponseData.talkValue;
        Icon.sprite = characterResponseData.icon;
        Icon.SetNativeSize();
        var clipPlayable= AnimationClipPlayable.Create(graph, characterResponseData.clip);
        animationPlayableOutput.SetSourcePlayable(clipPlayable);
        graph.Play();
        GameTimerController.instance.DeleyActionMain(characterResponseData.displayTime, Close);
    }
    
}
