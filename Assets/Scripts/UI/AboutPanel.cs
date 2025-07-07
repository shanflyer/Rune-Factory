using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AboutPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Button CloseBtn;

    [SerializeField]
    private TextMeshProUGUI info;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        CloseBtn = FindChildGameObject<Button>("Close");
        info = FindChildGameObject<TextMeshProUGUI>("Info");
    }

    public override async void InitData(string dataKey)
    {
        string text = await CloudRemoteConfig.instance.GetConfig("AboutInfo");
        info.text = text;
    }

    protected override void Awake()
    {
        base.Awake();
        CloseBtn.onClick.AddListener(Close);
    }
}