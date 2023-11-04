using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct Something:IReferenceData
{
    public Sprite sprite;
    public string info;
    public Action action;
}
public class SomethingGetPanel : GamePanel<Something>
{
    [SerializeField]
    TextMeshProUGUI infoText;
    [SerializeField]
    Image icon;
    [SerializeField]
    Button closeButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        infoText = FindChildGameObject<TextMeshProUGUI>("Info");
        icon = FindChildGameObject<Image>("Icon");
        closeButton = FindChildGameObject<Button>("Close");
        closeButton.onClick.AddListener(() =>
        {
            if (clickAction != null)
            {
                clickAction();
                Close();
            }
        });
    }
    Action clickAction;
    public override void InitReferenceData(Something v)
    {
        base.InitReferenceData(v);
        infoText.text = v.info;
        icon.sprite = v.sprite;
        clickAction = v.action;
    }
}
