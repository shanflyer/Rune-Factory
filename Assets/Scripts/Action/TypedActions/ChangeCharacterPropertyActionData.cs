// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ChangeCharacterProperty
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ChangeCharacterProperty")]
public class ChangeCharacterPropertyActionData : GameActionBaseData
{
        public int characterId;
        public int changeValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeCharacterProperty
        {
                characterId = this.characterId,
                changeValue = this.changeValue
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
