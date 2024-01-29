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
    Button TeamButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        InfoButton = FindChildGameObject<Button>("Info");
        TeamButton = FindChildGameObject<Button>("Team");
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
            TeamButton.transform.localScale = Vector3.one;
        }
        else
        {
            TeamButton.transform.localScale = Vector3.zero;
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
    }
    public override Task InitData(string dataKey)
    {
        var playerTeam = TeamManager.instance.playerTeam;
        if (playerTeam != null && playerTeam.Teamers.Count > 1)
        {
            TeamButton.transform.localScale = Vector3.one;
        }
        else
        {
            TeamButton.transform.localScale = Vector3.zero;
        }
        return base.InitData(dataKey); 
    }
}
