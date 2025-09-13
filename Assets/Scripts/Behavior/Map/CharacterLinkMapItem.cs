using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Game/Map")]
[TaskName("角色链接地图物体")]
public class CharacterLinkMapItem : Action
{
    [SerializeField] private SharedInt characterId;
    public SharedInt linkId;

    public override void OnStart()
    {
    }

    public override TaskStatus OnUpdate()
    {
        if (characterId != null)
        {
            var character = CharacterManager.instance.GetCharacter(characterId.Value);
            if (WorldMapManager.instance.GetRuntimeMapItem(linkId.Value, out var mapItem))
            {
                mapItem.linkCharacter = character.instanceId;
                var Pos = mapItem.mapItemData.offsetLinkPos + (Vector3)mapItem.pos;
                var SetCharacterTempPos = new SetCharacterTempPos
                {
                    characterId = character.instanceId,
                    pos = Pos
                };
                GameActionManager.instance.QueueAction(SetCharacterTempPos);

                var setDirection = new SetDirection
                {
                    directionEnum = mapItem.mapItemData.linkDirection,
                    characterId = character.instanceId
                };
                GameActionManager.instance.QueueAction(setDirection);
                character.linkItem = linkId.Value;
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Failure;
    }
}