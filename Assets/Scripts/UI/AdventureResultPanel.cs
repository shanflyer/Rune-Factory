using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventureResultPanel: GamePanel<FightResult>
{
    [SerializeField]
    AudioClip successAudioClip, failedAudioClip;
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

    private void OKAction()
    { 
        Close();
   
        GameRuntimeObjManager.instance.ClearRuntime<FightRuntimeObjType>();

        ExploreEnd exploreEnd = new ExploreEnd();
        GameActionManager.instance.QueueAction(exploreEnd, true);
  
        if (!data.victory)
        {
           var teamers=  TeamManager.instance.playerTeam.Teamers;
            for(int i = 0; i < characters.Count; i++)
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
    }
    List<int> characters = new List<int>();
    public override void InitReferenceData(FightResult fightResult)
    {
        base.InitReferenceData(fightResult);
        SuccessTitle.transform.localScale = fightResult.victory ? Vector3.one : Vector3.zero;
        FailureTitle.transform.localScale = fightResult.victory ? Vector3.zero : Vector3.one;
        AudioController.instance.ClearBGM(Group: BGMGroup.Battle.ToString(), audioClearType: AudioClearType.All);
        AudioController.instance.PlayAudioME(fightResult.victory ? successAudioClip : failedAudioClip, Group: MEGroup.Battle.ToString());
        // 结算列表绑定面板生命周期，关闭后旧结算不再继续写 UI 或关闭战斗面板。
        RunLifecycleTask(token => InitReferenceDataAsync(fightResult, token), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(FightResult fightResult, System.Threading.CancellationToken cancellationToken)
    {
        Debug.Log("fightResult.getItems");
        await itemList.InitListData(fightResult.getItems, cancellationToken: cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        Debug.Log("fightResult.fighterResults");
        characters.Clear();
        for(int i = 0; i < fightResult.fighterResults.Count; i++)
        {
            characters.Add(fightResult.fighterResults[i].Character.instanceId);
        }
        await teamerList.InitListData(fightResult.fighterResults, cancellationToken: cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

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
