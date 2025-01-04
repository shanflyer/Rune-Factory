
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("NewGame/Common")]
[TaskName("播放音效")]
public class PlaySEBehavior : Action
{
    [SerializeField]
    SE SE;

    public override void OnStart()
    {
        AudioController.instance.PlayAudio(SE);
    }
    
    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}