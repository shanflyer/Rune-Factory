using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamerReference : UIObjReference<CharacterInformationData>
{
    [SerializeField]
    private Image NPCImage;

    [SerializeField]
    private TextMeshProUGUI NPCName;

    [SerializeField]
    private Toggle toggle;

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data, value);
            }
        });
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        NPCImage = FindChildGameObject<Image>("NPCImage");
        toggle = GetComponent<Toggle>();
        NPCName = FindChildGameObject<TextMeshProUGUI>("NPCName");
    }

    public override async Task InitData(CharacterInformationData t, SelectAction<CharacterInformationData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        data.head.SetImageSprite(NPCImage);
       // NPCImage.sprite = data.icon;
        // data.head.SetImageSprite(NPCImage);
        NPCName.text = data.name;
        toggle.group = toggleGroup;
    }
}