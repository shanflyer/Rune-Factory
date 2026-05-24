using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct TwoSelectData : IReferenceData
{
    public string title;
    public string notice;
    public Action yesAction, noAction;
}

public class TwoSelectPanel : GamePanel<TwoSelectData>
{
    public override bool changeInputModel => false;

    [SerializeField]
    private TextMeshProUGUI TitleText, infoText;

    [SerializeField]
    private Button YesButton, NoButton;

    private Action yesAction, noAction;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        TitleText = FindChildGameObject<TextMeshProUGUI>("Title");
        infoText = FindChildGameObject<TextMeshProUGUI>("info");
        YesButton = FindChildGameObject<Button>("YesButton");
        NoButton = FindChildGameObject<Button>("NoButton");
    }

    protected override void Awake()
    {
        base.Awake();
        YesButton.onClick.AddListener(() =>
        {
            if (yesAction != null)
            {
                yesAction();
            }
            Close();
        });
        NoButton.onClick.AddListener(() =>
        {
            if (noAction != null)
            {
                noAction();
            }
            Close();
        });
    }

    public override void InitReferenceData(TwoSelectData v)
    {
        base.InitReferenceData(v);
        yesAction = v.yesAction; noAction = v.noAction;
        TitleText.SetSWText( v.title);
        infoText.SetSWText(v.notice);
    }
}
