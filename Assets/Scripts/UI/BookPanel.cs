using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookPanel : GamePanel<IReferenceData> 
{
    [SerializeField]
    Button bookButton;
    [SerializeField]
    Button helpButton;
    [SerializeField]
    Button teamButton;
    [SerializeField]
    Button characterButton;
    [SerializeField]
    Button closeButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        bookButton = FindChildGameObject<Button>("Book");
        helpButton = FindChildGameObject<Button>("Help");
        teamButton = FindChildGameObject<Button>("Team");
        characterButton = FindChildGameObject<Button>("Character");
        closeButton = FindChildGameObject<Button>("ReturnButton");
    }
    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(Close);
        characterButton.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<NPCPanel,NPCList>(NPCManager.instance.GetNPCList());
            Close();
        });
        teamButton.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<TeamPanel, CharacterInformationDataList>(TeamManager.instance.GetMyTeamCharacterInfo());
            Close();
        });
    }
}
