using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCPanel : GamePanel<NPCList>
{
   // [SerializeField]
  //  private TextMeshProUGUI Title;

    [SerializeField]
    private Button CloseButton, VisitButton, DetailsButton;

    [SerializeField]
    private ToggleGroup toggleGroup;

    [SerializeField]
    private NPCReference NPCReference;

    [SerializeField]
    private Transform NPCParent;

    private DisplayList<NPCReference, NPC> displayList;

    protected override void Awake()
    {
        base.Awake();
        CloseButton.onClick.AddListener(Close);
        VisitButton.onClick.AddListener(VisitAction);
        DetailsButton.onClick.AddListener(DetailAction);

        displayList = new DisplayList<NPCReference, NPC>(NPCReference, NPCParent);
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        //Title = FindChildGameObject<TextMeshProUGUI>("Title");
        CloseButton = FindChildGameObject<Button>("Close");
        VisitButton = FindChildGameObject<Button>("Visit");
        DetailsButton = FindChildGameObject<Button>("Details");
        NPCParent = FindChildGameObject("NPCList");
        NPCReference = FindChildGameObject<NPCReference>("NPCReference");
        toggleGroup = NPCParent.GetComponent<ToggleGroup>();
    }

    private void VisitAction()
    {
        VisitNPC visitNPC = new VisitNPC
        {
            sourceId = CharacterManager.instance.controllerCharacter.instanceId,
            targetId = selectNpc.characterId
        };
        GameActionManager.instance.QueueAction(visitNPC);
    }

    private void DetailAction()
    {
        CharacterInformationData characterInformationData = selectNpc.GetInformation();
        UIManager.instance.ShowGamePanel<CharacterInformationPanel, CharacterInformationData>(characterInformationData);
    }

    private NPC selectNpc;

    private void SelectAction(NPC npc, bool selected)
    {
        if (selected)
        {
            selectNpc = npc;
        }
    }

    private NPCList NPCList;

    public override void InitReferenceData(NPCList v)
    {
        base.InitReferenceData(v);
        NPCList = v;
        displayList.InitListData(NPCList.npcs, SelectAction, toggleGroup);
    }
}