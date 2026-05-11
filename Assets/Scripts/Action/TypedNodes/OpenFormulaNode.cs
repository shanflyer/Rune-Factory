// OpenFormula
using System;
using UnityEngine;

public class OpenFormulaNode : ActionNode
{
        public int formulaId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new OpenFormula
        {
                formulaId = this.formulaId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.formulaId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}