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
            FightController.instance.ActionSkill();
            Close();
        });
        CancleButton.onClick.AddListener(() => 
        {
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
