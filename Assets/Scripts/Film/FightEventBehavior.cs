using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class FightEventBehavior : PlayableBehaviour
{
    public int index;
    public bool hurtDisplay;
    //public GameActionData gameActionData;
    public SkillEstimateData skillActionData;
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        for(int i = 0; i < skillActionData.target[index].Count; i++)
        {
            ActionSkillEstimate actionSkillEstimate = new ActionSkillEstimate
            {
                skillId = skillActionData.skillId,
                sourceId = skillActionData.source,
                index = index,
                targetId = skillActionData.target[index][i],
                displayHurt = hurtDisplay
            };
            GameActionManager.instance.QueueAction(actionSkillEstimate, true);
        } 
        base.OnBehaviourPlay(playable, info);
    }
}