using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("NewGame/Common")]
[TaskName("检查道具链接角色")]
public class CheckMapItemLinkCharacter : Action
{
    public SharedInt characterId; 
    public SharedInt itemId; 
    public SharedInt mapId;
    public SharedInt editorId;
 
    public override async void OnStart()
    { 
    }

    public override TaskStatus OnUpdate()
    {
        if(mapId != null && mapId.Value > 0)
        {
            if (WorldMapManager.instance.IsCheckRuntimeMapItemLink(new int2(mapId.Value, itemId.Value), characterId.Value))
            {
                return TaskStatus.Success;
            }
        }
        else
        {
            if (WorldMapManager.instance.IsCheckRuntimeMapItemLink(itemId.Value, characterId.Value))
            {
                return TaskStatus.Success;
            }
        }

        

        return TaskStatus.Failure;
    }
}