// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - StopCharacterMove
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/StopCharacterMove")]
public class StopCharacterMoveActionData : GameActionBaseData
{
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new StopCharacterMove
        {
                characterId = this.characterId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
