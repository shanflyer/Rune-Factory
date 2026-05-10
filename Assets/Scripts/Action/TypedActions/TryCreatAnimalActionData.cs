// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryCreatAnimal
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryCreatAnimal")]
public class TryCreatAnimalActionData : GameActionBaseData
{
        public int roomId;
        public int dataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryCreatAnimal
        {
                roomId = this.roomId,
                dataId = this.dataId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
