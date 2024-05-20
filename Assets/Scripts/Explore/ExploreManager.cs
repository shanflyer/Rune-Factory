using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct FightChapter : IReferenceData, INativeData
{
    public int mapId;
    public FixedString128Bytes mapName;
    public int completeValue;
    public NativeHashSet<int> findItems;
    public NativeList<int> haveItems;
    public int failureEventId;
    public int successEventId; 
    public bool open;

    public void Dispose()
    {
        findItems.Dispose();
        haveItems.Dispose();
        mapName.Clear();
    }

    public int Key => mapId;
}

public class ExploreManager : Singleton<ExploreManager>
{
    private MyNativeData<FightChapter> fightChapters = new MyNativeData<FightChapter>();

    public int NowCharpter => nowChapter;

    private int nowChapter;
    private FightMapData nowFightMapData;
    private FightChapter fightChapter;
    private int nowStep;
    public bool isExplore => NowCharpter != 0;
    public FightChapter GetFigehtChapter(int id)
    {
        fightChapter = default(FightChapter);

        fightChapters.GetData(id, out fightChapter);
        return fightChapter;
    }

    public override async void Init()
    {
        base.Init();
        fightChapters.Init(10);
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
                successEventId = chapterData.successEventId
            };
            fightChapter.haveItems = new NativeList<int>(Allocator.Persistent);
            fightChapter.findItems = new NativeHashSet<int>(8, Allocator.Persistent);

            if (chapterData.items != null && chapterData.items.Count > 0)
            {
                for (int j = 0; j < chapterData.items.Count; j++)
                {
                    fightChapter.haveItems.Add(chapterData.items[j]);
                }
            }

            fightChapters.AddData(fightChapter);
        }
        if (GameDataSaveManager.instance.UserGameSaveData.chapters != null &&
            GameDataSaveManager.instance.UserGameSaveData.chapters.Count > 0)
        {
            var chapters = GameDataSaveManager.instance.UserGameSaveData.chapters;
            for (int i = 0; i < chapters.Count; i++)
            {
                FightChapter fightChapter;
                if (fightChapters.GetData(chapters[i].mapId, out fightChapter))
                {
                    fightChapter.completeValue = chapters[i].completeValue;
                    fightChapter.open = chapters[i].open;

                    for (int j = 0; j < chapters[i].findItems.Count; j++)
                    {
                        fightChapter.findItems.Add(chapters[i].findItems[j]);
                    }
                }
                this.fightChapters.SetData(fightChapter);
            }
        }

        GameActionManager.instance.AddListener<EnterChapter>(EnterChapter);
        GameActionManager.instance.AddListener<ChapterStepAction>(ChapterStepAction);
        GameActionManager.instance.AddListener<ExploreEnd>(ExploreEnd);
        GameActionManager.instance.AddListener<OpenChapter>(OpenChapter);
    }

    void OpenChapter(OpenChapter openChapter)
    {
        if(fightChapters.GetData(openChapter.id,out var fightChapter))
        {
            fightChapter.open = true;
            fightChapters.SetData(fightChapter);
        }
    }
    void ExploreEnd(ExploreEnd exploreEnd)
    {
        fightChapter = default(FightChapter);
        nowFightMapData=default(FightMapData);
        nowChapter = 0;
        nowStep = 0;
    }
    private void EnterChapter(EnterChapter enterChapter)
    {
        EnterChapter(enterChapter.id);
    }

    public async void EnterChapter(int id)
    {
        nowChapter = id;
        nowFightMapData = await GameDataManager.instance.GetAsyncData<FightMapData>(id.ToString());
        var beforeActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(nowFightMapData.beforeActionId);
        var afterActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(nowFightMapData.afterActionId);
        SceneManager.instance.SwitchScene("Fight", () =>
        {
            if (beforeActionData != null)
            {
                beforeActionData.Action();
            }
        },
        async () =>
        {
            WorldMapObjManager.instance.RecycleMap();
            CharacterManager.instance.RecycleCharacter();

            SetFixedCamera setFixedCamera = new SetFixedCamera
            {
                fixedCamera = true,
                fixedPos = Vector3.zero
            };
            GameActionManager.instance.QueueAction(setFixedCamera);

            FightController.instance.CreatFightMap(nowFightMapData);
            AudioController.instance.PlayBGM(nowFightMapData.exploreBGM, true);

            SetMapOverrideEnvironment setMapOverrideEnvironment = new SetMapOverrideEnvironment
            {
                dawnEnvironmentDataName = nowFightMapData.dawnEnvironmentDataName,
                dayEnvironmentDataName = nowFightMapData.dayEnvironmentDataName,
                duskEnvironmentDataName = nowFightMapData.duskEnvironmentDataName,
                nightEnvironmentDataName = nowFightMapData.nightEnvironmentDataName
            };
            GameActionManager.instance.QueueAction(setMapOverrideEnvironment, true);


            //if(!nowFightMapData.isZeroTeam)
            {
                FightManager.instance.CreatFightPlayer(); 

                await UIManager.instance.ShowGamePanel<FightPanel>(ExploreManager.instance.NowCharpter.ToString(), layer: 2);
            }
            UIManager.instance.CloseGamePanel<PlayerTopPanel>();
            UIManager.instance.CloseGamePanel<MainPanel>();
            UIManager.instance.CloseGamePanel<ShortcutPanel>();
            UIManager.instance.CloseGamePanel<ScreenControllerPanel>();
            if (afterActionData != null)
            {
                afterActionData.Action();
            }

        });

        
    }

    private async void ChapterStepAction(ChapterStepAction chapterStepAction)
    {
        UIManager.instance.CloseGamePanel<WarehousePanel>();
        if (fightChapter.mapId != nowChapter)
        {
            if (!fightChapters.GetData(nowChapter, out fightChapter))
            {
                return;
            }
        }
        if (nowFightMapData.id != nowChapter)
        {
            nowFightMapData = await GameDataManager.instance.GetAsyncData<FightMapData>(nowChapter);
            AudioController.instance.PlayBGM(nowFightMapData.fightBGM, true);
        }
        if (nowFightMapData.monsterDeploys.Count > nowStep)
        {
            //当前阶段
            int deployId = nowFightMapData.monsterDeploys[nowStep];
            //怪物分布
            var mosterDeploy = await GameDataManager.instance.GetAsyncData<MonsterDeploy>(deployId);
            if (mosterDeploy.id == deployId)
            {
                //创建怪物
                FightManager.instance.CreatFightMonster(mosterDeploy);
            }
        }

        
        SwitchFunctionButton switchFunctionButton = new SwitchFunctionButton
        {
            fight = true,
            auto = FightController.instance.AutoFight
        };
        GameActionManager.instance.QueueAction(switchFunctionButton);
        GameTimerController.instance.DelayAction(100, () =>
        {
            TryStartAutoBehavior tryStartAutoBehavior = new TryStartAutoBehavior();
            GameActionManager.instance.QueueAction(tryStartAutoBehavior);
        });
      
    }

    public void LerpExploreTime(float waitTime)
    {
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
        ExploreFailed();
    }
    public void SetChapterFindItem(List<int2> items)
    {
        for(int i=0;i<items.Count; i++)
        {
            fightChapter.findItems.Add(items[i].x);
        }
        fightChapters.SetData(fightChapter);
    }
    public bool StepFightSucceed()
    {
        nowStep++;
        float value = nowStep / (float)nowFightMapData.monsterDeploys.Count;
        float itemValue = fightChapter.findItems.Count / (float)fightChapter.haveItems.Length;

        fightChapter.completeValue = (int)(value * 50)+ (int)(itemValue * 50);
        fightChapters.SetData(fightChapter);

        if (nowStep >= nowFightMapData.monsterDeploys.Count)
        {
            ExploreSuccessful();
            return true;
        }
        else
        {
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

    private async void ExploreFailed()
    {
        var gameEventData = await GameDataManager.instance.GetAsyncData<GameEventData>(fightChapter.failureEventId);
        var FightResult = FightManager.instance.FightResult;
        FightResult.victory = false;
        UIManager.instance.ShowGamePanel<AdventureResultPanel, FightResult>(FightResult, layer: 2);
        Debug.Log("章节探索失败");
        if (gameEventData != null)
        {
            GameEventManager.instance.AddGameEvent(gameEventData, null);
        } 
    }

    private async void ExploreSuccessful()
    {
        var gameEventData = await GameDataManager.instance.GetAsyncData<GameEventData>(fightChapter.successEventId);
        var FightResult = FightManager.instance.FightResult;
        FightResult.victory = true;
        UIManager.instance.ShowGamePanel<AdventureResultPanel, FightResult>(FightResult, layer: 2);
        Debug.Log("章节探索成功");
        if (gameEventData != null)
        {
            GameEventManager.instance.AddGameEvent(gameEventData, null);
        } 
    }

    protected override void Clear()
    {
        fightChapters.Dispose();
        base.Clear();
    }
}