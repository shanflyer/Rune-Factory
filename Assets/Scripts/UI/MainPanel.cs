using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : GamePanel<IReferenceData>
{
    public override bool changeInputModel => false;
    [SerializeField]
    Button InfoButton;
    [SerializeField]
    Button TeamButton, HomeEquipmentButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        InfoButton = FindChildGameObject<Button>("Info");
        TeamButton = FindChildGameObject<Button>("Team");
        HomeEquipmentButton = FindChildGameObject<Button>("HomeEquipment");
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshTeam>(RefreshTeam);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshTeam>(RefreshTeam);
    }
    void RefreshTeam(RefreshTeam refreshTeam)
    {
        var playerTeam = TeamManager.instance.playerTeam;
        if (playerTeam != null && playerTeam.Teamers.Count > 1)
        {
            TeamButton.gameObject.SetActive(true);
        }
        else
        {
            TeamButton.gameObject.SetActive(false);
        }
    }
    protected override void Awake()
    {
        base.Awake();
        InfoButton.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<BookPanel>();
        });
        TeamButton.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<TeamPanel, CharacterInformationDataList>(TeamManager.instance.GetMyTeamCharacterInfo());
        });
        HomeEquipmentButton.onClick.AddListener(() =>
        {
            var homeEquipList= HomeEquipManager.instance.GetHomeEquipList(CharacterManager.instance.controllerCharacter.instanceId);
            UIManager.instance.ShowGamePanel<PlayerHomeEquipPanel,HomeEquipList>(homeEquipList);
        });
    }
    public override Task InitData(string dataKey)
    {
        var playerTeam = TeamManager.instance.playerTeam;
        if (playerTeam != null && playerTeam.Teamers.Count > 1)
        {
            TeamButton.gameObject.SetActive(true);
        }
        else
        {
            TeamButton.gameObject.SetActive(false);
        }
        return base.InitData(dataKey); 
    }
}
