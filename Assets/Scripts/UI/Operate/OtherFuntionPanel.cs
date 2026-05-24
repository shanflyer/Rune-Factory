using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherFuntionPanel : GamePanel<FunctionButtonList>
{
    public override bool changeInputModel => false;
    [SerializeField]
    OtherFunctionButtonReference OtherFunctionButtonReference;
    [SerializeField]
    Transform buttonParent;
    DisplayList<OtherFunctionButtonReference, FunctionButton> buttonList;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        OtherFunctionButtonReference = FindChildGameObject<OtherFunctionButtonReference>("Function");
        buttonParent = FindChildGameObject("FuncList");
    }
    protected override void Awake()
    {
        base.Awake();
        buttonList = new DisplayList<OtherFunctionButtonReference, FunctionButton>(OtherFunctionButtonReference, buttonParent);
    }
    public override void InitReferenceData(FunctionButtonList v)
    {
        base.InitReferenceData(v);
        // 功能按钮列表绑定面板生命周期，关闭后旧按钮不再写回。
        RunLifecycleTask(token => buttonList.InitListData(v.buttons, cancellationToken: token), nameof(InitReferenceData));
        HidePanel hidePanel = new HidePanel
        {
            hide = true,
            type = typeof(OperateButtonPanel)
        };
        GameActionManager.instance.QueueAction(hidePanel);
    }
    public override void Close()
    {
        base.Close();
        HidePanel hidePanel = new HidePanel
        {
            hide = false,
            type = typeof(OperateButtonPanel)
        };
        GameActionManager.instance.QueueAction(hidePanel);
    }
}
