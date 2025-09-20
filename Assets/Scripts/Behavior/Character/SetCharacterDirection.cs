using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("设置角色面向目标")]
public class SetCharacterDirection : Action
{
    [SerializeField]
    private bool faceItem;
    [SerializeField]
    private bool faceCharacter;
    [SerializeField]
    public SharedInt targetNPC;
    [SerializeField]
    public SharedInt2 itemEditorInstance;
    [SerializeField]
    private SharedInt3 faceTargetCoordinate;
    [SerializeField]
    private SharedInt characterId; 
    public override void OnStart()
    {
       
        if (faceItem)
        {
            bool getItem =false;
            if (!WorldMapManager.instance.GetMapItemPos(targetNPC.Value, out var objCoordinate))
            {
                if(WorldMapManager.instance.GetMapItemPos(itemEditorInstance.Value, out objCoordinate))
                {
                    getItem = true;
                }
            }
            else
            {
                getItem = true;
            }
            if (getItem)
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
        }else
        if (faceCharacter)
        {
            if (NPCManager.instance.GetNPCFormInstance(targetNPC.Value, out var target))
            {
                SetTargetDirection SetTargetDirection = new SetTargetDirection
                {
                    characterId = characterId.Value,
                    targetCoordinate = target.Character.coordinate
                };
                GameActionManager.instance.QueueAction(SetTargetDirection, true);
            }
            else if (CharacterManager.instance.controllerCharacter.instanceId == targetNPC.Value)
            {
            }

            {
                var SetTargetDirection = new SetTargetDirection
                {
                    characterId = characterId.Value,
                    targetCoordinate = CharacterManager.instance.controllerCharacter.coordinate
                };
                GameActionManager.instance.QueueAction(SetTargetDirection, true);
            }
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