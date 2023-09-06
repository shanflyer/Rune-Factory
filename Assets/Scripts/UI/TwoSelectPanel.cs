using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    Text TitleText, NoticeText;
    Button YesButton, NoButton;

    Action yesAction, noAction;
    public override void SetPanelUISerializeObj()
    { 
        base.SetPanelUISerializeObj();
        TitleText = FindChildGameObject<Text>("Title");
        NoticeText = FindChildGameObject<Text>("Notice");
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
    } 
}
