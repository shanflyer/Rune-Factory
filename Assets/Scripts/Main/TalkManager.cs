
using System.Collections;
using UnityEngine;

public class TalkManager : Singleton<TalkManager>
{
    public override void Init()
    {
        base.Init();
    }
    public void Talk(int talkId,int characterId=-1)
    {
        UIManager.instance.ShowGamePanel<TalkPanel>(talkId.ToString(), 2);
    }
}