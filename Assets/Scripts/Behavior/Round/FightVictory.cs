using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using UnityEngine;

[TaskCategory("NewGame/回合/事件")]
[TaskName("检查战斗胜利失败")]
public class CheckFightVictory : Action
{
    [Header("对比的结果，是否胜利")]
    public bool isVictory;
    SharedBool FightResult;
    // Use this for initialization
    public override void OnStart()
    {
        if (FightResult == null)
            FightResult = Owner.GetVariable("FightResult") as SharedBool;
    }

    // Update is called once per frame
    public override TaskStatus OnUpdate()
    {
        if (FightResult.Value==isVictory)
        {
            ExploreManager.instance.StepFightSuccessful();
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failure;
        } 
    }
}