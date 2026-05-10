// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SampleCreatAnimal
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SampleCreatAnimal")]
public class SampleCreatAnimalActionData : GameActionBaseData
{
        public int dataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SampleCreatAnimal
        {
                dataId = this.dataId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.dataId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
