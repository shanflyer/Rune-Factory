using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCReference : UIObjReference<NPC>
{
    [SerializeField]
    private Image Icon;

    [SerializeField]
    private TextMeshProUGUI NPCName;

    [SerializeField]
    private TextMeshProUGUI StateValue;

    [SerializeField]
    private TextMeshProUGUI FriendValue;

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

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshFriendShip>(RefreshFriendShip);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<RefreshFriendShip>(RefreshFriendShip);
    }

    private void RefreshFriendShip(RefreshFriendShip refreshFriendShip)
    {
        if (refreshFriendShip.characterId == data.characterInstance)
        {
            FriendValue.SetSWText(FriendManager.instance.GetFriendShipLevel(data.dataId).ToString());
        }
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        Icon = FindChildGameObject<Image>("NPCImage");
        NPCName = FindChildGameObject<TextMeshProUGUI>("NPCName");
        StateValue = FindChildGameObject<TextMeshProUGUI>("StateValue");
        FriendValue = FindChildGameObject<TextMeshProUGUI>("FriendValue");
        toggle = GetComponent<Toggle>();
    }

    public override async Task InitData(NPC t, SelectAction<NPC> SelectAction = null, ToggleGroup toggleGroup = null)
    {
         await base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;

        CharacterData characterData = await data.GetCharacterData();
        characterData.head.SetImageSprite(Icon,new Vector2(512,512));
         
        NPCName.SetADDText("+ ", characterData.characterName," +");

        FriendValue.SetSWText(FriendManager.instance.GetFriendShipLevel(data.characterInstance));
        StateValue.SetSWText(data.npcState.ToString());
        if (data.npcState == NPCState.修养中)
        {
            StateValue.color = new Color(1, 0, 0);
        }
        else
        {
            StateValue.color = new Color(0.5f, 0, 0);
        }
    }
}