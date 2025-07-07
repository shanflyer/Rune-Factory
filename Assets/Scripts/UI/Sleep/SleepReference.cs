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
        sleepText.SetSWText(data.text);
    }

    public override void InitData(SleepSetData t, SelectAction<SleepSetData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
      base.InitData(t, SelectAction, toggleGroup);
        sleepText.SetSWText(data.text);
        Icon.sprite = t.icon;
    }
}