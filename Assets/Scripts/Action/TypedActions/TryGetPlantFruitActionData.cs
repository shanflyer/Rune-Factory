// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryGetPlantFruit
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryGetPlantFruit")]
public class TryGetPlantFruitActionData : GameActionBaseData
{
        public int characterId;
        public int fieldId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryGetPlantFruit
        {
                characterId = this.characterId,
                fieldId = this.fieldId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.fieldId = source;
            if (target != 0 && target != int.MinValue) action.characterId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
