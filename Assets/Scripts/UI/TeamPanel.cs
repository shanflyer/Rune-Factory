using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamPanel : GamePanel<CharacterInformationDataList>
{
    [SerializeField]
    Transform teamerparent;
    [SerializeField]
    TeamerReference TeamerReference;
    [SerializeField]
    ToggleGroup toggleGroup;
    DisplayList<TeamerReference, CharacterInformationData> teamerList;
    protected override void Awake()
    {
        base.Awake();
        teamerList = new DisplayList<TeamerReference, CharacterInformationData>(TeamerReference, teamerparent);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        teamerparent = FindChildGameObject("TeamerList");
        TeamerReference = FindChildGameObject<TeamerReference>("TeamerReference");
        toggleGroup = teamerparent.gameObject.GetComponent<ToggleGroup>();
    }
    void SelectAction(CharacterInformationData characterInformationData,bool select)
    {
        UIManager.instance.ShowGamePanel<CharacterInformationPanel, CharacterInformationData>(characterInformationData);
    }
    public override void InitReferenceData(CharacterInformationDataList v)
    {
        base.InitReferenceData(v);
        teamerList.InitListData(v.characterInformationDatas, SelectAction, toggleGroup);
    }
}
