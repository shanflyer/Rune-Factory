using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    [SerializeField]
    Button nextButton, frontButton;
    [SerializeField]
    Animator BookPaper;
    private DisplayList<NPCReference, NPC> displayList;

    protected override void Awake()
    {
        base.Awake();
        CloseButton.onClick.AddListener(Close);
        VisitButton.onClick.AddListener(VisitAction);
        DetailsButton.onClick.AddListener(DetailAction);

        nextButton.onClick.AddListener(() =>
        {
            displayIndex += 1;
            if (displayIndex > maxIndex - 1)
            {
                displayIndex = maxIndex - 1;
            }
            InitButton();
            DisplayNpc();
        });
        frontButton.onClick.AddListener(() =>
        {
            displayIndex -= 1;
            if (displayIndex < 0)
            {
                displayIndex = 0;
            }
            InitButton();
            DisplayNpc();
        });


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
        nextButton = FindChildGameObject<Button>("Next");
        frontButton = FindChildGameObject<Button>("Front");
        BookPaper = FindChildGameObject<Animator>("Book2p");
    }

    private void VisitAction()
    {
        VisitNPC visitNPC = new VisitNPC
        {
            sourceId = CharacterManager.instance.controllerCharacter.instanceId,
            targetId = selectNpc.characterInstance
        };
        GameActionManager.instance.QueueAction(visitNPC);
        Close();
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
    int displayIndex = 0;
    int maxIndex = 0;

    void InitButton()
    {
        if (displayIndex >= maxIndex - 1)
        {
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
        }
        if (displayIndex <= 0)
        {
            frontButton.gameObject.SetActive(false);
        }
        else
        {
            frontButton.gameObject.SetActive(true);
        }
    }
    void DisplayNpc()
    {
        var npcs = new List<NPC>();
        for(int i = 0; i < 10; i++)
        {
            int index = i + displayIndex * 10;
            if (index < NPCList.npcs.Count)
            {
                npcs.Add(NPCList.npcs[index]);
            }
        }
        displayList.InitListData(npcs, SelectAction, toggleGroup);
    }
    public override void InitReferenceData(NPCList v)
    {
        nextButton.transform.localScale = Vector3.one;
        frontButton.transform.localScale = Vector3.one;
        BookPaper.gameObject.SetActive(false);
        base.InitReferenceData(v);
        NPCList = v;
        maxIndex = NPCList.npcs.Count / 10 + 1;
        DisplayNpc();
    }
}