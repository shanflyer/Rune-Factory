using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class NPCPanel : GamePanel<NPCList>
{
    [SerializeField]
    TextMeshProUGUI Title;
    [SerializeField]
    Button CloseButton,VisitButton,DetailsButton;
    [SerializeField]
    ToggleGroup toggleGroup;
    [SerializeField]
    NPCReference NPCReference;
    [SerializeField]
    Transform NPCParent;
    DisplayList<NPCReference, NPC> displayList;
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
        Title = FindChildGameObject<TextMeshProUGUI>("Title");
        CloseButton = FindChildGameObject<Button>("Close");
        VisitButton = FindChildGameObject<Button>("Visit");
        DetailsButton = FindChildGameObject<Button>("Details");
        NPCParent = FindChildGameObject("NPCList");
        NPCReference = FindChildGameObject<NPCReference>("NPCReference");
        toggleGroup = NPCParent.GetComponent<ToggleGroup>();
    }

    void VisitAction()
    {
        if (selectNpc != null)
        {
            VisitNPC visitNPC = new VisitNPC
            {
                sourceId = CharacterManager.instance.controllerCharacter.instanceId,
                targetId = selectNpc.instanceId
            };
            GameActionManager.instance.QueueAction(visitNPC);
        }
      
    }
    void DetailAction()
    {
        if (selectNpc == null)
        {
            return;
        }
            CharacterInformationData characterInformationData = selectNpc.GetInformation();
        UIManager.instance.ShowGamePanel<CharacterInformationPanel, CharacterInformationData>(characterInformationData);
    }

    NPC selectNpc;
    void SelectAction(NPC npc,bool selected)
    {
        if (selected)
        {
            selectNpc = npc;
        }
    }

    NPCList NPCList;
    public override void InitReferenceData(NPCList v)
    {
        base.InitReferenceData(v);
        NPCList = v;
        displayList.InitListData(NPCList.npcs, SelectAction, toggleGroup);
    }
}
