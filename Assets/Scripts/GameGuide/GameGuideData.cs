using System;
using System.Collections.Generic;
using UnityEngine;

public class GameGuideData : ScriptableObject,IGameData
{
    public List<GuidStepData> guidStepDatas = new List<GuidStepData>();

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
        return name;
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
    public int waitTime;
}