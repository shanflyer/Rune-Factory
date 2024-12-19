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
    public override void SelectDefault()
    {
        base.SelectDefault();
        toggle.SetIsOnWithoutNotify(true);
        if (SelectAction != null)
        {
            SelectAction(data, true);
        }

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
        await base.InitData(t, SelectAction, toggleGroup);
        data.head.SetImageSprite(NPCImage,new Vector2(48,48),Vector2.zero);
       // NPCImage.sprite = data.icon;
        // data.head.SetImageSprite(NPCImage);
        NPCName.text = data.name;
        toggle.group = toggleGroup;
    }
}