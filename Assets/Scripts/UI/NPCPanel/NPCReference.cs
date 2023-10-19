using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCReference : UIObjReference<NPCData>
{
    [SerializeField]
    Image Icon;
    [SerializeField]
    TextMeshProUGUI NPCName;
    [SerializeField]
    TextMeshProUGUI StateValue;
    [SerializeField]
    TextMeshProUGUI FriendValue;
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
        Icon = FindChildGameObject<Image>("Icon");
        NPCName = FindChildGameObject<TextMeshProUGUI>("NPCName");
        StateValue = FindChildGameObject<TextMeshProUGUI>("StateValue");
        FriendValue = FindChildGameObject<TextMeshProUGUI>("FriendValue");
        toggle=GetComponent<Toggle>();
    }
    public override async void InitData(NPCData t, SelectAction<NPCData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;

        CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(data.characterDataId);
        Icon.sprite = characterData.icon;
        Icon.SetNativeSize();
        NPCName.text = $"+ {characterData.characterName} +";
        FriendValue.text = data.friendLevel.ToString();
        StateValue.text = data.npcState.ToString();
        if (data.npcState == NPCState.修养中)
        {
            StateValue.color = new Color(0.5f, 1, 1);
        }
        else
        {
            StateValue.color = new Color(1, 0.5f, 1);
        }
    }
}