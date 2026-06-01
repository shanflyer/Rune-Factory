using System.Collections.Generic;
using System.Threading;
using MyGame;
using Unity.Mathematics;
using UnityEngine;

public class FightChapter : IReferenceData
{
    public int mapId;
    public string mapName;
    public int completeValue;
    public HashSet<int> findItems=new HashSet<int>();
    public List<int> haveItems=new List<int>();
    public int failureEventId;
    public int successEventId;
    public bool open;
    public bool completed;
    public FightMapData fightMapData;
    public int Key => mapId;
}

public class ExploreManager : Singleton<ExploreManager>
{
    private Dictionary<int,FightChapter> fightChapters = new Dictionary<int, FightChapter>();
    private System.Threading.Tasks.Task initializationTask = System.Threading.Tasks.Task.CompletedTask;
    public override System.Threading.Tasks.Task InitializationTask => initializationTask;
    public override System.Collections.Generic.IReadOnlyList<System.Type> InitializationDependencies => new[] { typeof(GameDataManager) };

    public int NowChapter => nowChapter;

    private MyUid myUid;
    private int nowChapter;
    private FightMapData nowFightMapData;
    private FightChapter fightChapter;
    private int nowStep;
    public bool isExplore => NowChapter != 0;
    public FightChapter GetFightChapter(int id)
    {
        if(fightChapters.TryGetValue(id,out var fightChapter))
        {
            return fightChapter;
        }
        return null;
    }

    public int NewUid => myUid.Uid;
    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async System.Threading.Tasks.Task InitAsync()
    {
        myUid = new MyUid();
        fightChapters.Clear();
        var allChapterDatas = await GameDataManager.instance.GetAllAsyncData<FightMapData>();
        for (int i = 0; i < allChapterDatas.Count; i++)
        {
            var chapterData = allChapterDatas[i];
            FightChapter fightChapter = new FightChapter
            {
                mapId = chapterData.id,
                mapName = chapterData.mapName,
                open = chapterData.isOpen,
                failureEventId = chapterData.failureEventId,
                successEventId = chapterData.successEventId,
                fightMapData=chapterData,
            };


            if (chapterData.items != null && chapterData.items.Count > 0)
            {
                for (int j = 0; j < chapterData.items.Count; j++)
                {
                    fightChapter.haveItems.Add(chapterData.items[j]);
                }
            }

            fightChapters.Add(chapterData.id,fightChapter);
        }
        if (GameDataSaveManager.instance.UserGameSaveData.chapters != null &&
            GameDataSaveManager.instance.UserGameSaveData.chapters.Count > 0)
        {
            var chapters = GameDataSaveManager.instance.UserGameSaveData.chapters;
            foreach(var chapter in chapters)
            {
                FightChapter fightChapter;
                if (fightChapters.TryGetValue(chapter.Value.mapId, out fightChapter))
                {
                    fightChapter.open = chapter.Value.open;
                    fightChapter.completed = chapter.Value.completed;
                    for (int j = 0; j < chapter.Value.findItems.Count; j++)
                    {
                        fightChapter.findItems.Add(chapter.Value.findItems[j]);
                    }
                }
            }
        }
        else
        {
            foreach(var fightChapter in fightChapters)
            {
                GameDataSaveManager.instance.UserGameSaveData.SetFightChapter(fightChapter.Value);
            }
        }

        GameActionManager.instance.AddAsyncListener<EnterChapter>(enterChapter => EnterChapterAsync(enterChapter.id), nameof(EnterChapter));
        GameActionManager.instance.AddAsyncListener<ChapterStepAction>(ChapterStepActionAsync, nameof(ChapterStepAction));
        GameActionManager.instance.AddListener<ExploreEnd>(ExploreEnd);
        GameActionManager.instance.AddListener<OpenChapter>(OpenChapter);
    }

    void OpenChapter(OpenChapter openChapter)
    {
        if(fightChapters.TryGetValue(openChapter.id,out var fightChapter))
        {
            fightChapter.open = true;
            GameDataSaveManager.instance.UserGameSaveData.SetFightChapter(fightChapter);
        }
    }
    void ExploreEnd(ExploreEnd exploreEnd)
    {
        GameActionDataManager.instance.Action(nowFightMapData.endActionId);
        UIManager.instance.CloseGamePanel<FightPanel>();
        fightChapter = default(FightChapter);
        nowFightMapData=default(FightMapData);
        nowChapter = 0;
        nowStep = 0;
        SetFixedPlayerShaderPos setFixedPlayerShaderPos = new SetFixedPlayerShaderPos
        {
            fixedPos = false
        };
        GameActionManager.instance.QueueAction(setFixedPlayerShaderPos);
    }
    private void EnterChapter(EnterChapter enterChapter)
    {
        EnterChapter(enterChapter.id);
    }

    public void EnterChapter(int id)
    {
        AsyncTaskRunner.RunLatest(nameof(EnterChapter), token => EnterChapterAsync(id, token), nameof(EnterChapter));
    }

    public async System.Threading.Tasks.Task EnterChapterAsync(int id, CancellationToken cancellationToken = default)
    {
        nowChapter = id;
        nowFightMapData = await GameDataManager.instance.GetAsyncData<FightMapData>(id.ToString());
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var beforeActionData = await GameDataManager.instance.GetAsyncData<GameActionAsset>(nowFightMapData.beforeActionId);
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var afterActionData = await GameDataManager.instance.GetAsyncData<GameActionAsset>(nowFightMapData.afterActionId);
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        SceneManager.instance.SwitchScene("Fight", () =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (beforeActionData != null)
            {
                beforeActionData.Action();
            }
        },
         () =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            WorldMapObjManager.instance.RecycleMap();
            CharacterManager.instance.RecycleCharacter();

            SetFixedCamera setFixedCamera = new SetFixedCamera
            {
                fixedCamera = true,
                fixedPos = new Vector3(0, -0.86f, 0),
                offsetPos = new Vector3(0, 0.86f, 0),
            };
            GameActionManager.instance.QueueAction(setFixedCamera);

            SetCameraConfiner2D SetCameraConfiner2D = new SetCameraConfiner2D
            {
                enable = false
            };
            GameActionManager.instance.QueueAction(SetCameraConfiner2D);

            FightController.instance.CreateFightMap(nowFightMapData);
            AudioController.instance.ClearBGM(AudioClearType.All, BGMGroup.Theme.ToString());
            AudioController.instance.PlayBGM(nowFightMapData.exploreBGM, Group: BGMGroup.Battle.ToString(), audioClearType: AudioClearType.All,isLerp:true);

            AudioController.instance.SetBGMGroupValue(BGMGroup.Map.ToString(), 0);
            AudioController.instance.SetBGSGroupValue(BGMGroup.Map.ToString(), 0);
            AudioController.instance.SetBGSGroupValue(BGSGroup.Rain.ToString(), 0);
            AudioController.instance.SetBGSGroupValue(BGSGroup.Wind.ToString(), 0);
            AudioController.instance.SetBGSGroupValue(BGSGroup.Lightning.ToString(), 0);

            SetMapOverrideEnvironment setMapOverrideEnvironment = new SetMapOverrideEnvironment
            {
                dawnEnvironmentDataName = nowFightMapData.dawnEnvironmentDataName,
                dayEnvironmentDataName = nowFightMapData.dayEnvironmentDataName,
                duskEnvironmentDataName = nowFightMapData.duskEnvironmentDataName,
                nightEnvironmentDataName = nowFightMapData.nightEnvironmentDataName
            };
            GameActionManager.instance.QueueAction(setMapOverrideEnvironment, true);
            FightManager.instance.CreateFightPlayer();

            GameTimerController.instance.DelayActionAsync(100, async token =>
            {
                if (token.IsCancellationRequested || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await UIManager.instance.ShowGamePanel<FightPanel>(instance.NowChapter.ToString(), layer: 2);
                if (token.IsCancellationRequested || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                UIManager.instance.CloseGamePanel<PlayerTopPanel>();
                UIManager.instance.CloseGamePanel<MainPanel>();
                UIManager.instance.CloseGamePanel<ShortcutPanel>();
                UIManager.instance.CloseGamePanel<ScreenControllerPanel>();
                UIManager.instance.CloseGamePanel<CharacterButtonPanel>();
                if (afterActionData != null)
                {
                    afterActionData.Action();
                }
            }, nameof(EnterChapter), nameof(EnterChapter));


        });


    }
    //探索阶段
    private async System.Threading.Tasks.Task ChapterStepActionAsync(ChapterStepAction chapterStepAction)
    {
        Debug.Log("ChapterStepAction!!!");
        UIManager.instance.CloseGamePanel<WarehousePanel>();
        if (nowChapter == 0)
        {
            return;
        }
        if (fightChapter==null||fightChapter.mapId != nowChapter)
        {
            if (!fightChapters.TryGetValue(nowChapter, out fightChapter))
            {
                return;
            }
        }
        if (nowFightMapData.id != nowChapter)
        {
            nowFightMapData = await GameDataManager.instance.GetAsyncData<FightMapData>(nowChapter);
        }
        if (nowFightMapData.id == 0 || nowFightMapData.monsterDeploys == null || nowFightMapData.monsterDeploys.Count == 0)
        {
            return;
        }
        if (nowFightMapData.monsterDeploys.Count > nowStep)
        {
            if (nowStep == nowFightMapData.monsterDeploys.Count - 1)
            {
                AudioController.instance.PlayBGM(nowFightMapData.fightBGM, isLerp: true, audioClearType: AudioClearType.All, Group: BGMGroup.Battle.ToString());
            }
            else
            {
                AudioController.instance.PlayBGM(nowFightMapData.bossBGM, isLerp: true, audioClearType: AudioClearType.All, Group: BGMGroup.Battle.ToString());
            }

            //当前阶段
            int deployId = nowFightMapData.monsterDeploys[nowStep];
            //怪物分布
            var monsterDeploy = await GameDataManager.instance.GetAsyncData<MonsterDeploy>(deployId);
            if (monsterDeploy.id == deployId)
            {
                //创建怪物
               await FightManager.instance.CreateFightMonster(monsterDeploy);
            }
        }

        SwitchFunctionButton switchFunctionButton = new SwitchFunctionButton
        {
            fight = true,
            auto = FightController.instance.AutoExplore
        };
        GameActionManager.instance.QueueAction(switchFunctionButton,true);
        FightManager.instance.cdTimeMoving = true;
        GameTimerController.instance.DelayAction(500, () =>
        {
            TryStartAutoBehavior tryStartAutoBehavior = new TryStartAutoBehavior();
            GameActionManager.instance.QueueAction(tryStartAutoBehavior);
        });

    }

    public void LerpExploreTime(float waitTime)
    {
        if (nowFightMapData.id == 0 || nowFightMapData.monsterDeploys == null || nowFightMapData.monsterDeploys.Count == 0)
        {
            return;
        }
        int perMinute = GameCommon.explorCostMinute / nowFightMapData.monsterDeploys.Count;
        int hour = GameTimeManager.instance.Hour;
        int minute = GameTimeManager.instance.Minute;

        minute += perMinute;
        if (minute >= 60)
        {
            hour+= minute / 60;
            minute = minute % 60;
            if (hour > 24)
            {
                hour -= 24;
            }
        }

        LerpGameTime lerpGameTime = new LerpGameTime
        {
            totalTime = waitTime,
            targetHour = hour,
            targetMinute = minute
        };
        GameActionManager.instance.QueueAction(lerpGameTime,true);
    }

    public void FightFail()
    {
        FightManager.instance.cdTimeMoving = false;
        ExploreFailed();
    }
    public void SetChapterFindItem(List<int2> items)
    {
        if (fightChapter == null || items == null)
        {
            return;
        }
        for(int i=0;i<items.Count; i++)
        {
            fightChapter.findItems.Add(items[i].x);
        }
        GameDataSaveManager.instance.UserGameSaveData.SetFightChapter(fightChapter);
    }
    public bool StepFightSucceed()
    {
        FightManager.instance.cdTimeMoving = false;
        if (fightChapter == null)
        {
            return false;
        }

        nowStep++;
        int stepCount = nowFightMapData.monsterDeploys != null ? nowFightMapData.monsterDeploys.Count : 0;
        int itemCount = fightChapter != null && fightChapter.haveItems != null ? fightChapter.haveItems.Count : 0;
        float value = stepCount > 0 ? nowStep / (float)stepCount : 1;
        float itemValue = itemCount > 0 ? fightChapter.findItems.Count / (float)itemCount : 1;

        fightChapter.completeValue = (int)(value * 50)+ (int)(itemValue * 50);

        EndNowRoundFight endNowRoundFight = new EndNowRoundFight { };
        GameActionManager.instance.QueueAction(endNowRoundFight, true);

        if (nowStep >= stepCount)
        {
            ExploreSuccessful();
            return true;
        }
        else
        {
            AudioController.instance.PlayBGM(nowFightMapData.exploreBGM, Group: BGMGroup.Battle.ToString(),
                audioClearType: AudioClearType.All, isLerp: false);
            RefreshFightChapter refreshFightChapter = new RefreshFightChapter
            {
                id = nowChapter
            };
            SwitchFunctionButton switchFunctionButton = new SwitchFunctionButton
            {
                fight = false,
                auto = FightController.instance.AutoExplore
            };
            TryStartAutoExplore tryStartAutoExplore = new TryStartAutoExplore();

            GameTimerController.instance.DelayAction(1000, () =>
            {
                GameActionManager.instance.QueueAction(switchFunctionButton);
                GameActionManager.instance.QueueAction(refreshFightChapter);
                GameActionManager.instance.QueueAction(tryStartAutoExplore);
            }
            );
            return false;
        }
    }

    private void ExploreFailed()
    {
        AsyncTaskRunner.Run(ExploreFailedAsync, nameof(ExploreFailed));
    }

    private async System.Threading.Tasks.Task ExploreFailedAsync()
    {
        EndNowRoundFight endNowRoundFight = new EndNowRoundFight { };
        GameActionManager.instance.QueueAction(endNowRoundFight, true);

        var gameEventData = await GameDataManager.instance.GetAsyncData<GameEventData>(fightChapter.failureEventId);
        var FightResult = FightManager.instance.FightResult;
        FightResult.victory = false;
        UIManager.instance.ShowGamePanelImmediately<AdventureResultPanel, FightResult>(FightResult, 2);
        Debug.Log("章节探索失败");
        if (gameEventData != null)
        {
            GameEventManager.instance.AddGameEvent(gameEventData, null);
        }
    }

    private void ExploreSuccessful()
    {
        AsyncTaskRunner.Run(ExploreSuccessfulAsync, nameof(ExploreSuccessful));
    }

    private async System.Threading.Tasks.Task ExploreSuccessfulAsync()
    {
        EndNowRoundFight endNowRoundFight = new EndNowRoundFight { };
        GameActionManager.instance.QueueAction(endNowRoundFight, true);
        fightChapter.completed = true;
        var gameEventData = await GameDataManager.instance.GetAsyncData<GameEventData>(fightChapter.successEventId);
        var FightResult = FightManager.instance.FightResult;
        FightResult.victory = true;
       await UIManager.instance.ShowGamePanel<AdventureResultPanel, FightResult>(FightResult, layer: 2);
        Debug.Log("章节探索成功");
        if (gameEventData != null)
        {
            GameEventManager.instance.AddGameEvent(gameEventData, null);
        }
    }

    protected override void Clear()
    {
        initializationTask = System.Threading.Tasks.Task.CompletedTask;
        fightChapters.Clear();
        base.Clear();
    }
}
