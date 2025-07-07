using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventureResultPanel : GamePanel<FightResult>
{
    [SerializeField]
    private AudioClip successAudioClip, failedAudioClip;

    [SerializeField]
    private Transform SuccessTitle, FailureTitle;

    [SerializeField]
    private Transform ItemsContent;

    [SerializeField]
    private ItemReference itemReference;

    [SerializeField]
    private AdventureTeamerRenference teamerRenference;

    [SerializeField]
    private Transform Team;

    [SerializeField]
    private Button OkButton;

    private DisplayList<ItemReference, Item> itemList;
    private DisplayList<AdventureTeamerRenference, FighterResult> teamerList;

    protected override void Awake()
    {
        OkButton.onClick.AddListener(OKAction);
        itemList = new DisplayList<ItemReference, Item>(itemReference, ItemsContent);
        teamerList = new DisplayList<AdventureTeamerRenference, FighterResult>(teamerRenference, Team);
        base.Awake();
    }

    private async void OKAction()
    {
        /*WaitAction waitAction = new WaitAction();
        Parameter parameter = new Parameter
        {
            value = "2",
            parameters = new List<Parameter>()
        };
        parameter.parameters.Add(new Parameter
        {
            value= "ExploreEnd"
        });
        waitAction.Init(new List<Parameter> {parameter});*/
        //GameActionManager.instance.QueueAction(new ExploreEnd());
        Close();

        /*
        DisplayMap displayMap = new DisplayMap
        {
            displayMap = CharacterManager.instance.controllerCharacter.mapInstance
        };
        GameActionManager.instance.QueueAction(displayMap,true);*/

        GameRuntimeObjManager.instance.ClearRuntime<FightRuntimeObjType>();

        ExploreEnd exploreEnd = new ExploreEnd();
        GameActionManager.instance.QueueAction(exploreEnd, true);

        await UIManager.instance.ShowGamePanel<CharacterButtonPanel>();
        await UIManager.instance.ShowGamePanel<PlayerTopPanel>();
        await UIManager.instance.ShowGamePanel<MainPanel>();
        await UIManager.instance.ShowGamePanel<ShortcutPanel>();
        await UIManager.instance.ShowGamePanel<ScreenControllerPanel>();

        if (!data.victory)
        {
            var teamers = TeamManager.instance.playerTeam.Teamers;
            for (int i = 0; i < characters.Count; i++)
            {
                var characterId = characters[i];
                if (NPCManager.instance.GetNPCFormInstance(characterId, out var npc))
                {
                    npc.Rest();
                    SimpleTalk simpleTalk = new SimpleTalk
                    {
                        characterId = characterId,
                        talkId = GameCommon.TeamLeave
                    };
                    GameActionManager.instance.QueueAction(simpleTalk);
                    LeaveTeam leaveTeam = new LeaveTeam
                    {
                        teamCharacterId = characterId,
                        setResult = (bool value) =>
                        {
                            npc.BackHome();
                        }
                    };
                    GameActionManager.instance.QueueAction(leaveTeam, true);
                }
            }
        }
        // SceneManager.instance.UnloadNowScene();
        // UIManager.instance.
    }

    private List<int> characters = new List<int>();

    public override void InitReferenceData(FightResult fightResult)
    {
        base.InitReferenceData(fightResult);
        SuccessTitle.transform.localScale = fightResult.victory ? Vector3.one : Vector3.zero;
        FailureTitle.transform.localScale = fightResult.victory ? Vector3.zero : Vector3.one;
        AudioController.instance.PlayBGM(null, Group: BGMGroup.Battle.ToString(), audioClearType: AudioClearType.All);
        AudioController.instance.PlayAudioME(fightResult.victory ? successAudioClip : failedAudioClip, Group: MEGroup.Battle.ToString());
        Debug.Log("fightResult.getItems");
        itemList.InitListData(fightResult.getItems);
        Debug.Log("fightResult.fighterResults");
        for (int i = 0; i < fightResult.fighterResults.Count; i++)
        {
            characters.Add(fightResult.fighterResults[i].Character.instanceId);
        }
        teamerList.InitListData(fightResult.fighterResults);
        UIManager.instance.CloseGamePanel<FightPanel>();
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        SuccessTitle = FindChildGameObject("SuccessTitle");
        FailureTitle = FindChildGameObject("FailureTitle");
        ItemsContent = FindChildGameObject("ItemsContent");
        itemReference = FindChildGameObject<ItemReference>("ItemBoxReference");
        Team = FindChildGameObject("Team");
        teamerRenference = FindChildGameObject<AdventureTeamerRenference>("AdventureTeamer");
        OkButton = FindChildGameObject<Button>("OkButton");
    }
}