// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryCreateField
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryCreateField")]
public class TryCreateFieldActionData : GameActionBaseData
{
        public int itemInstanceId;
        public int editorInstanceId;
        public int roomId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryCreateField
        {
                itemInstanceId = this.itemInstanceId,
                editorInstanceId = this.editorInstanceId,
                roomId = this.roomId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.itemInstanceId = source;
            if (target != 0 && target != int.MinValue) action.editorInstanceId = target;
            if (value != -1) action.roomId = value;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
