using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SleepReference : UIObjReference<SleepSetData>
{
    [SerializeField]
    private Image Icon;

    [SerializeField]
    private TextMeshProUGUI sleepText;

    [SerializeField]
    private Button sleepButton;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        sleepButton = FindChildGameObject<Button>("Sleep");
        sleepText = FindChildGameObject<TextMeshProUGUI>("Name");
        Icon = FindChildGameObject<Image>("Icon");
    }

    private void Awake()
    {
        sleepButton.onClick.AddListener(() => { SelectAction(data, true); });
        sleepText.text = data.text;
    }

    public override async Task InitData(SleepSetData t, SelectAction<SleepSetData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        sleepText.text = data.text;
        Icon.sprite = t.icon;
    }
}