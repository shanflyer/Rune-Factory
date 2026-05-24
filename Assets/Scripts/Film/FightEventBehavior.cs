using UnityEngine.Playables;

public class FightEventBehavior : PlayableBehaviour
{
    public int index;
    public bool hurtDisplay;
    //public GameActionAsset gameActionData;
    public SkillEstimateData skillEstimateData;
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if(skillEstimateData== null)
        {
            return;
        }
        for(int i = 0; i < skillEstimateData.targets.Count; i++)
        {
            ActionSkillEstimate actionSkillEstimate = new ActionSkillEstimate
            {
                skillId = skillEstimateData.skillRuntime != null?skillEstimateData.skillRuntime.instanceId: skillEstimateData.skillId,
                sourceId = skillEstimateData.source,
                index = index,
                targetId = skillEstimateData.targets[i],
                displayHurt = hurtDisplay
            };
            GameActionManager.instance.QueueAction(actionSkillEstimate, true);
            // Debug.Log($"<color=green>战斗:{skillEstimateData.skillRuntime.instanceId}-SkillData:{skillEstimateData.skillRuntime.skillData.skillName}</color>");
        }
        base.OnBehaviourPlay(playable, info);
    }
}
