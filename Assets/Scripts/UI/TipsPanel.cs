using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TipsPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private TextMeshProUGUI TitleText, NoticeText;

    [SerializeField]
    private Button CloseButton;

    // Use this for initialization
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        CloseButton = FindChildGameObject<Button>("YesButton");
        TitleText = FindChildGameObject<TextMeshProUGUI>("Title");
        NoticeText = FindChildGameObject<TextMeshProUGUI>("Notice");
    }

    protected override void Awake()
    {
        base.Awake();
        CloseButton.onClick.AddListener(Close);
    }

    public void InitTipsData(string title, string Notice)
    {
        TitleText.SetSWText(title);
        NoticeText.SetSWText(Notice);
    }
}