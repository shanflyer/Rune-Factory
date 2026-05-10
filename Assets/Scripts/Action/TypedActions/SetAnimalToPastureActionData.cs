// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetAnimalToPasture
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetAnimalToPasture")]
public class SetAnimalToPastureActionData : GameActionBaseData
{
        public int pastureId;
        public int animalId;
        public bool refreshPos;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetAnimalToPasture
        {
                pastureId = this.pastureId,
                animalId = this.animalId,
                refreshPos = this.refreshPos
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
