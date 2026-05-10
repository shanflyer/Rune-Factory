// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryDeleteAnimal
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryDeleteAnimal")]
public class TryDeleteAnimalActionData : GameActionBaseData
{
        public int animalId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryDeleteAnimal
        {
                animalId = this.animalId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.animalId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
