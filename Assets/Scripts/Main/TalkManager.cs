
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
        Talk(talk.talkId, talk.characterId);
    }
    public void Talk(int talkId,int characterId=-1)
    {
        UIManager.instance.ShowGamePanel<TalkPanel>(talkId.ToString(), 2);
    }
}