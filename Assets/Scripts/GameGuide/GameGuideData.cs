using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/引导数据")]
public class GameGuideData : ScriptableObject,IGameData
{
    public int id;
    public string guideName;
    public List<GuidStepData> guidStepDatas = new List<GuidStepData>();
    public int endAction;

    public bool GetGuidStepData(int stepIndex, out GuidStepData stepData)
    {
        stepData = null;
        if (guidStepDatas == null || stepIndex < 0 || stepIndex >= guidStepDatas.Count)
        {
            return false;
        }

        stepData = guidStepDatas[stepIndex];
        return true;
    }

    public void TriggerEndAction()
    {
        if (endAction != 0)
        {
            GameActionDataManager.instance.Action(endAction);
        }
    }

    public bool IsValidStepIndex(int stepIndex)
    {
        return guidStepDatas != null && stepIndex >= 0 && stepIndex < guidStepDatas.Count;
    }

    public bool HasValidGuideSteps()
    {
        if (guidStepDatas == null || guidStepDatas.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < guidStepDatas.Count; i++)
        {
            if (guidStepDatas[i] == null || guidStepDatas[i].selectableId <= 0)
            {
                return false;
            }
        }

        return true;
    }

    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
    public void SetReferenceData()
    {
    }
}
[Serializable]
public class GuidStepData:IReferenceData
{
    public int selectableId;
    public string showText;
}
