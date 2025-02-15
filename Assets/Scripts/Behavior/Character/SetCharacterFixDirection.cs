using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



[TaskCategory("Game/Character")]
[TaskName("强制设置角色动画方向")]
public class SetCharacterFixDirection : Action
{ 
    [SerializeField]
    private Direction Direction;
    [SerializeField]
    private SharedInt characterId;
    public override void OnStart()
    {
        Character character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character != null)
        {
            character.SetDirection(Direction);
        }
      
    }
    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}