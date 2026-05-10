// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetItemValue
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetItemValue")]
public class SetItemValueActionData : GameActionBaseData
{
        public int characterId;
        public int selectItem;
        public int value;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetItemValue
        {
                characterId = this.characterId,
                selectItem = this.selectItem,
                value = this.value
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
