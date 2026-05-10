// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - GetAnimalOutFromPasture
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/GetAnimalOutFromPasture")]
public class GetAnimalOutFromPastureActionData : GameActionBaseData
{
        public int pastureId;
        public int animalId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new GetAnimalOutFromPasture
        {
                pastureId = this.pastureId,
                animalId = this.animalId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
