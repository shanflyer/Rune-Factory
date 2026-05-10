// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ChangeWorld
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ChangeWorld")]
public class ChangeWorldActionData : GameActionBaseData
{
        public string worldName;
        public int displayMap;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeWorld
        {
                worldName = this.worldName,
                displayMap = this.displayMap
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
