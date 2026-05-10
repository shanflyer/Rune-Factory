// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetCharacterProperty
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetCharacterProperty")]
public class SetCharacterPropertyActionData : GameActionBaseData
{
        public int characterId;
        public int Value;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCharacterProperty
        {
                characterId = this.characterId,
                Value = this.Value
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
