using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCReference : UIObjReference<NPC>
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
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshFriendShip>(RefreshFriendShip);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshFriendShip>(RefreshFriendShip);
    }
    void RefreshFriendShip(RefreshFriendShip refreshFriendShip)
    {
        if (refreshFriendShip.characterId == data.characterId)
        {
            FriendValue.text = FriendManager.instance.GetFriendShipLevel(data.characterId).ToString();
        }
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
  
    public override async void InitData(NPC t, SelectAction<NPC> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;

        CharacterData characterData =await data.GetCharacterData();
        Icon.sprite = characterData.icon;
        Icon.SetNativeSize();
        NPCName.text = $"+ {characterData.characterName} +";
        
        FriendValue.text = FriendManager.instance.GetFriendShipLevel(data.characterId).ToString();
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