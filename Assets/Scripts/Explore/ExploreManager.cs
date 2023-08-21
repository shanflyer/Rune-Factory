using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct FightChapter
{
    public int mapId;
    public int completeValue;
    public NativeList<int> findItems;
    public int nowStep;
    public bool open;

    public override int GetHashCode()
    {
        return mapId;
    }
}
public class ExploreManager : Singleton<ExploreManager>
{
    MyNativeData<FightChapter> fightChapters = new MyNativeData<FightChapter>();
    int nowChapter;
    FightMapData nowFightMapData;
    public FightChapter GetFigehtChapter(int id)
    {
        FightChapter fightChapter=default(FightChapter);

        fightChapters.GetData(id, out fightChapter);
        return fightChapter;
    }
    public override async void Init()
    {
        base.Init();

        var allChapterDatas = await GameDataManager.instance.GetAllAsyncObjectDataArray<FightMapData>();
        for (int i = 0; i < allChapterDatas.Count; i++)
        {
            var chapterData = allChapterDatas[i];
            FightChapter fightChapter = new FightChapter
            {
                mapId = chapterData.id,
                open = chapterData.isOpen, 
            };

            fightChapter.findItems = new NativeList<int>(Allocator.Persistent);

            fightChapters.AddData(fightChapter);
        } 
        if (GameDataManager.instance.UserGameSaveData.chapters.Count > 0)
        {
            var chapters = GameDataManager.instance.UserGameSaveData.chapters;
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
    }
    void EnterChapter(EnterChapter enterChapter)
    {
        EnterChapter(enterChapter.id);
    }
    public async void EnterChapter(int id)
    {
        nowChapter = id;
        nowFightMapData = await GameDataManager.instance.GetAsyncObjectDataArray<FightMapData>(id.ToString());
        var actionData = await GameDataManager.instance.GetAsyncObjectData<GameActionData>(nowFightMapData.actionId);
        SceneManager.instance.SwitchScene("Fight", () => {
            LoadFightMap();
            if (actionData != null)
            {
                actionData.Action();
            }
            FightManager.instance.CreatFightPlayer();
        } );
    }
    void LoadFightMap()
    {
        FightController.instance.CreatFightMap(nowFightMapData);
    }
  
    protected override void Clear()
    {
        fightChapters.Dispose();
        base.Clear();
    }
}