using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine .UI;

public class TipsPanel : GamePanel
{
    [SerializeField]
    Text TitleText, NotceText;
    [SerializeField]
    Button CloseButton;
    // Use this for initialization
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        CloseButton = FindChildGameObject<Button>("YesButton");
        TitleText=FindChildGameObject<Text>("Title");
        NotceText = FindChildGameObject<Text>("Notce");
    }
    protected override void Awake()
    {
        base.Awake();
        CloseButton.onClick.AddListener(Close);
    }
  
    public void InitTipsData(string title, string Notice)
    {
        TitleText.text = title;
        NotceText.text = Notice;
    }
	 
}
