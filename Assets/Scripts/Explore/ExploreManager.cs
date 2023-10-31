using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct FightChapter:IReferenceData,INativeData
{
    public int mapId;
    public int completeValue;
    public NativeList<int> findItems;
    public int failureEventId;
    public int successEventId;
    public int nowStep;
    public bool open;

    public int Key => mapId; 
}
public class ExploreManager : Singleton<ExploreManager>
{
    MyNativeData<FightChapter> fightChapters = new MyNativeData<FightChapter>();

    public int NowCharpter => nowChapter;

    int nowChapter;
    FightMapData nowFightMapData;
    FightChapter fightChapter;
    public FightChapter GetFigehtChapter(int id)
    {
        FightChapter fightChapter=default(FightChapter);

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
                open = chapterData.isOpen, 
            };

            fightChapter.findItems = new NativeList<int>(Allocator.TempJob);

            fightChapters.AddData(fightChapter);
        }
        if (GameDataSaveManager.instance.UserGameSaveData.chapters != null&&
            GameDataSaveManager.instance.UserGameSaveData.chapters.Count > 0)
        {
            var chapters = GameDataSaveManager.instance.UserGameSaveData.chapters;
            for(int i = 0; i < chapters.Count; i++)
            {
                FightChapter fightChapter;
                if (fightChapters.GetData(chapters[i].mapId,out fightChapter))
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
    }
    void EnterChapter(EnterChapter enterChapter)
    {
        EnterChapter(enterChapter.id);
    }
    public async void EnterChapter(int id)
    {
        nowChapter = id;
        nowFightMapData = await GameDataManager.instance.GetAsyncData<FightMapData>(id.ToString());
        var beforeActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(nowFightMapData.beforeActionId); 
        var afterActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(nowFightMapData.afterActionId);
        SceneManager.instance.SwitchScene("Fight", () => {
           
            if (beforeActionData != null)
            {
                beforeActionData.Action();
            } 
        } ,
        () =>
        {
            FightController.instance.CreatFightMap(nowFightMapData);
            AudioController.instance.PlayBGM(nowFightMapData.exploreBGM,true);
            if(nowFightMapData.isZeroTeam)
            {
                FightManager.instance.CreatFightPlayer();
            } 
            if (afterActionData != null)
            {
                afterActionData.Action();
            }
        }); 
    }
    
    async void ChapterStepAction(ChapterStepAction chapterStepAction)
    {
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
        if (nowFightMapData.monsterDeploys.Count > fightChapter.nowStep)
        {
            //当前阶段
            int deployId = nowFightMapData.monsterDeploys[fightChapter.nowStep];
            //怪物分布
            var mosterDeploy = await GameDataManager.instance.GetAsyncData<MonsterDeploy>(deployId);
            if(mosterDeploy.id== deployId)
            {
                //创建怪物
                FightManager.instance.CreatFightMonster(mosterDeploy); 
            }
        }
    }
    public void FightFail()
    {
        ExploreFailed();
    }
    public void StepFightSucceed()
    {
        fightChapter.nowStep++;
        if (fightChapter.nowStep >= nowFightMapData.monsterDeploys.Count)
        {
            ExploreSuccessful();
        }
        else
        {
           
        }
    }
    async void ExploreFailed()
    {
        var gameEventData = await GameDataManager.instance.GetAsyncData<GameEventData>(fightChapter.failureEventId);

        if (gameEventData == null)
        {
            var FightResult = FightManager.instance.FightResult;
            FightResult.victory = false;
            UIManager.instance.ShowGamePanel<AdventureResultPanel, FightResult>(FightResult, layer: 2);
            Debug.Log("章节探索失败");
        }
        else
        {
            GameEventManager.instance.AddGameEvent(gameEventData, null);
        }
    }
    async void ExploreSuccessful()
    {
        var gameEventData = await GameDataManager.instance.GetAsyncData<GameEventData>(fightChapter.successEventId);

        if (gameEventData==null)
        {
            var FightResult = FightManager.instance.FightResult;
            FightResult.victory = true;
            UIManager.instance.ShowGamePanel<AdventureResultPanel,FightResult>(FightResult, layer: 2);
            Debug.Log("章节探索成功");
        }
        else
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