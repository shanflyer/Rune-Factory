// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetItemAnimation
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetItemAnimation")]
public class SetItemAnimationActionData : GameActionBaseData
{
        public int mapId;
        public new int id;
        public int editorId;
        public int keyX;
        public int keyY;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetItemAnimation
        {
                mapId = this.mapId,
                id = this.id,
                editorId = this.editorId,
                keyX = this.keyX,
                keyY = this.keyY
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.id = source;
            if (target != 0 && target != int.MinValue) action.keyX = target;
            if (value != -1) action.keyY = value;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
