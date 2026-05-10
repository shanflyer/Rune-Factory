// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetCharacterAnimator
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetCharacterAnimator")]
public class SetCharacterAnimatorActionData : GameActionBaseData
{
        public int characterId;
        public string parameter;
        public int intValue;
        public bool boolValue;
        public float floatValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCharacterAnimator
        {
                characterId = this.characterId,
                parameter = this.parameter,
                intValue = this.intValue,
                boolValue = this.boolValue,
                floatValue = this.floatValue
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
