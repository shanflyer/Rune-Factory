using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillActionPanel :GamePanel<IReferenceData>
{
    [SerializeField]
    Button ActionButton, CancleButton;

    public override void Close()
    {
        base.Close();
    }
    protected override void Awake()
    {
        base.Awake();
        ActionButton.onClick.AddListener(() =>
        {
            ManualSkillAction manualSkillAction = new ManualSkillAction();
            GameActionManager.instance.QueueAction(manualSkillAction, true);
            Debug.Log($"设置暂停true");
            if (FightManager.instance.isFight)
            {
                SkillPauseAction skillPauseAction = new SkillPauseAction
                {
                    pause = true,
                };
                GameActionManager.instance.QueueAction(skillPauseAction, true);
            }

            Close();
        });
        CancleButton.onClick.AddListener(() =>
        {
            Debug.Log($"取消暂停false");
            if (FightManager.instance.isFight)
            {
                SkillPauseAction skillPauseAction = new SkillPauseAction
                {
                    pause = false,
                };
                GameActionManager.instance.QueueAction(skillPauseAction, true);
            }
            NoSelectSkillAction noSelectSkillAction = new NoSelectSkillAction();
            GameActionManager.instance.QueueAction(noSelectSkillAction, true);
            Close();
        });
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        ActionButton = FindChildGameObject<Button>("SkillAction");
        CancleButton = FindChildGameObject<Button>("SkillCancel");
    }
    public override void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v);
    }
}
