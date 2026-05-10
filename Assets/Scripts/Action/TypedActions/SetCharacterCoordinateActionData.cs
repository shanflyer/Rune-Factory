// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetCharacterCoordinate
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetCharacterCoordinate")]
public class SetCharacterCoordinateActionData : GameActionBaseData
{
        public int characterId;
        public bool fiexedDisplay;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCharacterCoordinate
        {
                characterId = this.characterId,
                fiexedDisplay = this.fiexedDisplay
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
