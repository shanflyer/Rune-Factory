using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TipsPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    TextMeshProUGUI TitleText, NoticeText;
    [SerializeField]
    Button CloseButton;
    // Use this for initialization
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        CloseButton = FindChildGameObject<Button>("YesButton");
        TitleText=FindChildGameObject<TextMeshProUGUI>("Title");
        NoticeText = FindChildGameObject<TextMeshProUGUI>("Notice");
    }
    protected override void Awake()
    {
        base.Awake();
        CloseButton.onClick.AddListener(Close);
    }
  
    public void InitTipsData(string title, string Notice)
    {
        TitleText.text = title;
        NoticeText.text = Notice;
    }
	 
}
