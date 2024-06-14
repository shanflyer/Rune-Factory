using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class FightEventBehavior : PlayableBehaviour
{
    public int index;
    public bool hurtDisplay;
    //public GameActionData gameActionData;
    public SkillEstimateData skillEstimateData;
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        for(int i = 0; i < skillEstimateData.targets.Count; i++)
        {
            ActionSkillEstimate actionSkillEstimate = new ActionSkillEstimate
            {
                skillId = skillEstimateData.skillId,
                sourceId = skillEstimateData.source,
                index = index,
                targetId = skillEstimateData.targets[i],
                displayHurt = hurtDisplay
            };
            GameActionManager.instance.QueueAction(actionSkillEstimate, true);
        } 
        base.OnBehaviourPlay(playable, info);
    }
}