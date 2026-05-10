// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RefreshAnimalPos
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RefreshAnimalPos")]
public class RefreshAnimalPosActionData : GameActionBaseData
{
        public int animalId;
        public bool refreshPos;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshAnimalPos
        {
                animalId = this.animalId,
                refreshPos = this.refreshPos
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
