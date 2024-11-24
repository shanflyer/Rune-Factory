using System.Collections.Generic;
using Unity.Collections; 
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
     
    public int Key => mapId;
}

public class ExploreManager : Singleton<ExploreManager>
{
    private Dictionary<int,FightChapter> fightChapters = new Dictionary<int, FightChapter>();

    public int NowChapter => nowChapter;

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

    public override async void Init()
    {
        base.Init();
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
                successEventId = chapterData.successEventId
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

        GameActionManager.instance.AddListener<EnterChapter>(EnterChapter);
        GameActionManager.instance.AddListener<ChapterStepAction>(ChapterStepAction);
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
         () =>
        {
            WorldMapObjManager.instance.RecycleMap();
            CharacterManager.instance.RecycleCharacter();

            SetFixedCamera setFixedCamera = new SetFixedCamera
            {
                fixedCamera = true,
                fixedPos = Vector3.zero
            };
            GameActionManager.instance.QueueAction(setFixedCamera);

            FightController.instance.CreateFightMap(nowFightMapData);
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

            GameTimerController.instance.DelayAction(100, async () =>
            {
                await UIManager.instance.ShowGamePanel<FightPanel>(ExploreManager.instance.NowChapter.ToString(), layer: 2);
                UIManager.instance.CloseGamePanel<PlayerTopPanel>();
                UIManager.instance.CloseGamePanel<MainPanel>();
                UIManager.instance.CloseGamePanel<ShortcutPanel>();
                UIManager.instance.CloseGamePanel<ScreenControllerPanel>();
                UIManager.instance.CloseGamePanel<CharacterButtonPanel>();
                if (afterActionData != null)
                {
                    afterActionData.Action();
                }
            });
           

        });

        
    }
    //探索阶段
    private async void ChapterStepAction(ChapterStepAction chapterStepAction)
    {
        UIManager.instance.CloseGamePanel<WarehousePanel>();
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
        if (nowFightMapData.monsterDeploys.Count > nowStep)
        {
            nowFightMapData = await GameDataManager.instance.GetAsyncData<FightMapData>(nowChapter);
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
        for(int i=0;i<items.Count; i++)
        {
            fightChapter.findItems.Add(items[i].x);
        }
        GameDataSaveManager.instance.UserGameSaveData.SetFightChapter(fightChapter);
    }
    public bool StepFightSucceed()
    {
        FightManager.instance.cdTimeMoving = false;

        nowStep++;
        float value = nowStep / (float)nowFightMapData.monsterDeploys.Count;
        float itemValue = fightChapter.findItems.Count / (float)fightChapter.haveItems.Count;

        fightChapter.completeValue = (int)(value * 50)+ (int)(itemValue * 50); 

        EndNowRoundFight endNowRoundFight = new EndNowRoundFight { };
        GameActionManager.instance.QueueAction(endNowRoundFight, true);

        if (nowStep >= nowFightMapData.monsterDeploys.Count)
        {
            ExploreSuccessful();
            return true;
        }
        else
        {
            AudioController.instance.PlayBGM(nowFightMapData.exploreBGM, Group: BGMGroup.Battle.ToString(), audioClearType: AudioClearType.All, isLerp: true);
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
        EndNowRoundFight endNowRoundFight = new EndNowRoundFight { };
        GameActionManager.instance.QueueAction(endNowRoundFight, true);

        var gameEventData = await GameDataManager.instance.GetAsyncData<GameEventData>(fightChapter.failureEventId);
        var FightResult = FightManager.instance.FightResult;
        FightResult.victory = false;
       await UIManager.instance.ShowGamePanel<AdventureResultPanel, FightResult>(FightResult, layer: 2);
        Debug.Log("章节探索失败");
        if (gameEventData != null)
        {
            GameEventManager.instance.AddGameEvent(gameEventData, null);
        } 
    }

    private async void ExploreSuccessful()
    {
        EndNowRoundFight endNowRoundFight = new EndNowRoundFight { };
        GameActionManager.instance.QueueAction(endNowRoundFight, true);

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
        fightChapters.Clear();
        base.Clear();
    }
}