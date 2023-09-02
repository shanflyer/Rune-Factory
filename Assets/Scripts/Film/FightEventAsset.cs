using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class FightEventAsset : PlayableAsset
{
    public int index;
    public bool hurtDisplay;
   // public GameActionData gameActionData;

    private SkillEstimateData skillEstimateData;

    public void SetSkillEstimateData(SkillEstimateData skillEstimateData)
    {
        this.skillEstimateData = skillEstimateData;
    }
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playble = ScriptPlayable<FightEventBehavior>.Create(graph);
        var fightEventBehavior = playble.GetBehaviour();
        fightEventBehavior.index = index;
        //fightEventBehavior.gameActionData = gameActionData;
        fightEventBehavior.skillEstimateData = skillEstimateData;
        fightEventBehavior.hurtDisplay = hurtDisplay;

        return playble;
    }

     
}