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
    int stepIndex = 0;
    public void Zero()
    {
        stepIndex = 0;
    }
    public bool GetGuidStepData(out  GuidStepData stepData)
    {
        stepData = null;
        if (stepIndex >= guidStepDatas.Count)
        {
            if (endAction != 0)
            {
                GameActionDataManager.instance.Action(endAction);
            }
            return false;
        }
        else
        {
            stepData= guidStepDatas[stepIndex];
            stepIndex++;
            return true;
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
