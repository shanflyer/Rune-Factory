using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Button teamButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        teamButton = FindChildGameObject<Button>("TeamInfo");
    }
    protected override void Awake()
    {
        base.Awake();
        teamButton.onClick.AddListener(() =>
        {
            Team team = TeamManager.instance.playerTeam;
            if (team != null)
            {
                var characterInfoDataList = team.GetTeamCharacterInfo();
                UIManager.instance.ShowGamePanel<TeamPanel, CharacterInformationDataList>(characterInfoDataList);
            }
        });
    }
}
