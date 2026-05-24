
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class AboutPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Button CloseBtn;
    [SerializeField]
    TextMeshProUGUI info;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        CloseBtn = FindChildGameObject<Button>("Close");
        info = FindChildGameObject<TextMeshProUGUI>("Info");
    }
    public override async Task InitData(string dataKey)
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
