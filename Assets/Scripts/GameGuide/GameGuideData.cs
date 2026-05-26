using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/指引数据")]
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
