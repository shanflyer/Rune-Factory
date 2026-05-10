// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RemoveCharacterMove
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RemoveCharacterMove")]
public class RemoveCharacterMoveActionData : GameActionBaseData
{
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemoveCharacterMove
        {
                characterId = this.characterId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (target != 0 && target != int.MinValue) action.characterId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
