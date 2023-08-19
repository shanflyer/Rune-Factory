using System.Collections;
using UnityEngine;

public struct FightChapter
{
    public int mapId;
    public int completeValue;
    public int allStep;
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

    public FightChapter GetFigehtChapter(int id)
    {
        FightChapter fightChapter=default(FightChapter);

        fightChapters.GetData(id, out fightChapter);
        return fightChapter;
    }
    public override async void Init()
    {
        base.Init();

        if (GameDataManager.instance.UserGameSaveData.fightChapters.Count > 0)
        {
            var fightChapters = GameDataManager.instance.UserGameSaveData.fightChapters;
            for(int i = 0; i < fightChapters.Count; i++)
            {
                FightChapter fightChapter = fightChapters[i];
                this.fightChapters.AddData(fightChapter);
            }
        }
        else
        {
            var allChapterDatas = await GameDataManager.instance.GetAllAsyncObjectDataArray<FightMapData>();
            for (int i = 0; i < allChapterDatas.Count; i++)
            {
                var chapterData = allChapterDatas[i];
                FightChapter fightChapter = new FightChapter
                {
                    mapId = chapterData.id,
                    open = chapterData.isOpen,
                };
                fightChapters.AddData(fightChapter);
            }
        } 
        
    }

  
    protected override void Clear()
    {
        fightChapters.Dispose();
        base.Clear();
    }
}