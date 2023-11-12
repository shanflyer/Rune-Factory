using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public struct TwoSelectData:IReferenceData
{
    public string title;
    public string notice;
    public Action yesAction, noAction;
}
public class TwoSelectPanel : GamePanel<TwoSelectData>
{
    public override bool changeInputModel => false;
    [SerializeField]
    TextMeshProUGUI TitleText, NoticeText;
    [SerializeField]
    Button YesButton, NoButton;

    Action yesAction, noAction;
    public override void SetPanelUISerializeObj()
    { 
        base.SetPanelUISerializeObj();
        TitleText = FindChildGameObject<TextMeshProUGUI>("Title");
        NoticeText = FindChildGameObject<TextMeshProUGUI>("Notice");
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
        yesAction=v.yesAction; noAction=v.noAction;
        TitleText.text = v.title;
        NoticeText.text = v.notice;
    } 
}
