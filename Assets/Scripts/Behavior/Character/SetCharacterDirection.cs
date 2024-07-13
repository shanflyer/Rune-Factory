using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime; 
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;

[TaskCategory("Game/Character")]
[TaskName("设置角色面向目标")]
public class SetCharacterDirection : Action
{
    [SerializeField]
    private bool faceItem;
    [SerializeField]
    public SharedInt2 itemEditorInstance;
    [SerializeField]
    private SharedInt3 faceTargetCoordinate;
    [SerializeField]
    private SharedInt characterId; 
    public override void OnStart()
    {
        if (characterId==null|| characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (faceTargetCoordinate==null|| faceTargetCoordinate.IsNull())
        {
            faceTargetCoordinate = (SharedInt3)Owner.GetVariable("FaceTargetCoordinate");
        }
        if (faceItem)
        {
            if(WorldMapManager.instance.GetMapItemPos(itemEditorInstance.Value,out var objCoordinate))
            {
                SetTargetDirection SetTargetDirection = new SetTargetDirection
                {
                    characterId = characterId.Value,
                    targetCoordinate = objCoordinate.xy
                };
                GameActionManager.instance.QueueAction(SetTargetDirection, true);
                taskStatus = TaskStatus.Success;
                return;
            }
            taskStatus = TaskStatus.Failure;
        }
        else
        {
            if(faceTargetCoordinate == null || faceTargetCoordinate.IsNull())
            {
                taskStatus = TaskStatus.Failure;
                return;
            }
            SetTargetDirection SetTargetDirection = new SetTargetDirection
            {
                characterId = characterId.Value,
                targetCoordinate = faceTargetCoordinate.Value.xy
            };
            GameActionManager.instance.QueueAction(SetTargetDirection, true);
            taskStatus = TaskStatus.Success;
        }

    }
    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    { 
        return taskStatus;
    }
}