using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCPanel : GamePanel<NPCDataList>
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
    DisplayList<NPCReference, NPCData> displayList;
    protected override void Awake()
    {
        base.Awake();
        CloseButton.onClick.AddListener(Close);
        VisitButton.onClick.AddListener(VisitAction);
        DetailsButton.onClick.AddListener(DetailAction);

        displayList = new DisplayList<NPCReference, NPCData>(NPCReference, NPCParent);
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

    }
    void DetailAction()
    {

    }

    void SelectAction(NPCData nPCData,bool selected)
    {

    }

    NPCDataList NPCDatas;
    public override void InitReferenceData(NPCDataList v)
    {
        base.InitReferenceData(v);
        NPCDatas = v;
        displayList.InitListData(NPCDatas.NPCDatas, SelectAction, toggleGroup);
    }
}
