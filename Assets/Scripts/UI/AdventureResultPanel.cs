using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventureResultPanel : GamePanel
{
    [SerializeField]
    Transform SuccessTitle, FailureTitle;
    [SerializeField]
    Transform ItemsContent;
    [SerializeField]
    ItemReference itemReference;
    [SerializeField]
    TeamerRenference teamerRenference;
    [SerializeField]
    Transform Team;
    [SerializeField]
    Button OkButton;

    DisplayList<ItemReference, Item> itemList;
    DisplayList<TeamerRenference, FighterResult> teamerList;

    protected override void Awake()
    {
        OkButton.onClick.AddListener(OKAction);
        itemList = new DisplayList<ItemReference, Item>(itemReference, ItemsContent);
        teamerList=new DisplayList<TeamerRenference, FighterResult>(teamerRenference,Team);
        base.Awake();
    }
    void OKAction()
    {

    }

    public void InitData(FightResult fightResult)
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
        itemReference = FindChildGameObject<ItemReference>("Item");
        Team = FindChildGameObject("Team");
        teamerRenference = FindChildGameObject<TeamerRenference>("Teamer");
        OkButton = FindChildGameObject<Button>("OkButton");

        base.SetPanelUISerializeObj();
    }

}
