using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SimpleCustomPlayable : BasicPlayableBehaviour
{
    public override void OnGraphStart(Playable playable)
    {
        Debug.Log("Graph start");
    }

    public override void OnGraphStop(Playable playable)
    {
        Debug.Log("Graph stop");
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
       // Debug.Log("1");
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //Debug.Log("2");
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        GameComponentData.gameData.filmManager.FilmEndAction();
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        Debug.Log("Play State Paused");
    }
}