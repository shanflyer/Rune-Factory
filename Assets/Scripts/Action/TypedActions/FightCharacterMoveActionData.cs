// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - FightCharacterMove
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/FightCharacterMove")]
public class FightCharacterMoveActionData : GameActionBaseData
{
        public int characterId;
        public int newIndex;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new FightCharacterMove
        {
                characterId = this.characterId,
                newIndex = this.newIndex
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
