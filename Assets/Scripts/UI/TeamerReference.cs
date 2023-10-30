using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamerReference : UIObjReference<CharacterInformationData>
{
    [SerializeField]
    Image NPCImage;
    [SerializeField]
    TextMeshProUGUI NPCName;
    [SerializeField]
    Toggle toggle;
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
    public override void InitData(CharacterInformationData t, SelectAction<CharacterInformationData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        NPCImage.sprite = data.head;
        NPCName.text = data.name;
        toggle.group = toggleGroup;
    }
}
