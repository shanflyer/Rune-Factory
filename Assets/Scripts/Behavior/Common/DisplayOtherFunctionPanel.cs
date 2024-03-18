using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("NewGame/Common")]
[TaskName("显示其他功能界面")]
public class DisplayOtherFunctionPanel : Action
{
    public SharedSharedInt3List actionParameters;
    public List<FunctionButtonData> functionButtonDatas = new List<FunctionButtonData>();

    public override void OnStart()
    {
        DisplayPanel();
    }
    void DisplayPanel()
    {
        FunctionButtonList FunctionButtonList = new FunctionButtonList
        {
            buttons = new List<FunctionButton>()
        };
        for (int i = 0; i < functionButtonDatas.Count; i++)
        {
            var functionButtonData= functionButtonDatas[i];
            int3 ParameterValue = actionParameters.Value[i].Value;

            FunctionButton functionButton = new FunctionButton
            {
                name = functionButtonData.buttonName,
                sprite = functionButtonData.icon,
                action = () =>
                {
                    if (functionButtonData.alwaysClosePanel)
                    {
                        UIManager.instance.CloseGamePanel<OtherFuntionPanel>();
                    }
                    functionButtonData.gameActionData.Action(ParameterValue.x, ParameterValue.y, ParameterValue.z, CloseOtherFunctionPanel);
                }
            };
            FunctionButtonList.buttons.Add(functionButton);
        }
        UIManager.instance.ShowGamePanel<OtherFuntionPanel, FunctionButtonList>(FunctionButtonList);
    }

    void CloseOtherFunctionPanel(bool success)
    {
        if(success)
        {
            UIManager.instance.CloseGamePanel<OtherFuntionPanel>();
        }
    }
    public override TaskStatus OnUpdate()
    {

        return TaskStatus.Success;

    }
}