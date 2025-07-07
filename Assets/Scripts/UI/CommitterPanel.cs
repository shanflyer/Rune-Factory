using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommitterPanel : GamePanel<IReferenceData>
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
        string text = await CloudRemoteConfig.instance.GetConfig("Committer");
        info.text = text;
    }

    protected override void Awake()
    {
        base.Awake();
        CloseBtn.onClick.AddListener(Close);
    }
}