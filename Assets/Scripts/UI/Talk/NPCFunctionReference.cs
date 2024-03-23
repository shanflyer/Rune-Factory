using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCFunctionReference : UIObjReference<NPCFunctionData>
{
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TextMeshProUGUI Name;

    [SerializeField]
    private Button button;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        icon = FindChildGameObject<Image>("Icon");
        Name = FindChildGameObject<TextMeshProUGUI>("Name");
        button = GetComponent<Button>();
    }

    private void Awake()
    {
        button.onClick.AddListener(ClickAction);
    }

    public override async Task InitData(NPCFunctionData t, SelectAction<NPCFunctionData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        icon.sprite = data.icon;
        Name.text = data.npcFunctionName;
    }
}