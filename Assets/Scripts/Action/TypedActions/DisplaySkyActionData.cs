// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - DisplaySky
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/DisplaySky")]
public class DisplaySkyActionData : GameActionBaseData
{
        public bool display;
        public bool displaySunlight;
        public int skyId;
        public Vector2 startPos;
        public Vector2 endPos;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplaySky
        {
                display = this.display,
                displaySunlight = this.displaySunlight,
                skyId = this.skyId,
                startPos = this.startPos,
                endPos = this.endPos
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
