// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetCharacterRandomCoordinate
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetCharacterRandomCoordinate")]
public class SetCharacterRandomCoordinateActionData : GameActionBaseData
{
        public int characterId;
        public int range;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCharacterRandomCoordinate
        {
                characterId = this.characterId,
                range = this.range
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
