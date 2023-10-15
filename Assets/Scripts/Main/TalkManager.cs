
using System.Collections;
using UnityEngine;

public class TalkManager : Singleton<TalkManager>
{
    
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<Talk>(Talk);
    }
    void Talk(Talk talk)
    {
        Talk(talk.talkId, talk.characterId,talk.displayFunction);
    }
    public async void Talk(int talkId,int characterId=-1,bool displayFunction=false)
    {
        TalkData talkData = await GameDataManager.instance.GetAsyncData<TalkData>(talkId);

        UIManager.instance.ShowGamePanel<TalkPanel>(talkId.ToString(), 2);
    }
}