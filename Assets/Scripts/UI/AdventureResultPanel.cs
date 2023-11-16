using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdventureResultPanel: GamePanel<FightResult>
{
    [SerializeField]
    Transform SuccessTitle, FailureTitle;
    [SerializeField]
    Transform ItemsContent;
    [SerializeField]
    ItemReference itemReference;
    [SerializeField]
    AdventureTeamerRenference teamerRenference;
    [SerializeField]
    Transform Team;
    [SerializeField]
    Button OkButton;

    DisplayList<ItemReference, Item> itemList;
    DisplayList<AdventureTeamerRenference, FighterResult> teamerList;

    protected override void Awake()
    {
        OkButton.onClick.AddListener(OKAction);
        itemList = new DisplayList<ItemReference, Item>(itemReference, ItemsContent);
        teamerList=new DisplayList<AdventureTeamerRenference, FighterResult>(teamerRenference,Team);
        base.Awake();
    }
    void OKAction()
    {
        WaitAction waitAction = new WaitAction();
        Parameter parameter = new Parameter
        {
            value = "2",
            parameters = new List<Parameter>()
        };
        parameter.parameters.Add(new Parameter
        {
            value= "ExploreEnd"
        });
        waitAction.Init(new List<Parameter> {parameter});
        //GameActionManager.instance.QueueAction(new ExploreEnd());
        Close();
    }
 
    public override void InitReferenceData(FightResult fightResult)
    {
        SuccessTitle.transform.localScale = fightResult.victory ? Vector3.one : Vector3.zero;
        FailureTitle.transform.localScale = fightResult.victory ? Vector3.zero : Vector3.one;

        itemList.InitListData(fightResult.getItems);
        teamerList.InitListData(fightResult.fighterResults);
    }
    
    public override void SetPanelUISerializeObj()
    {
        SuccessTitle = FindChildGameObject("SuccessTitle");
        FailureTitle = FindChildGameObject("FailureTitle");
        ItemsContent = FindChildGameObject("ItemsContent");
        itemReference = FindChildGameObject<ItemReference>("ItemBoxReference");
        Team = FindChildGameObject("Team");
        teamerRenference = FindChildGameObject<AdventureTeamerRenference>("AdventureTeamer");
        OkButton = FindChildGameObject<Button>("OkButton");

        base.SetPanelUISerializeObj();
    }

}
