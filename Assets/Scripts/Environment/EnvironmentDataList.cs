using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName ="Datas/环境效果数据")]
public class EnvironmentDataList: ScriptableObject, IGameData, IDataArray<EnvironmentData>
{
    [SerializeField]
    EnvironmentData[] EnvironmentDatas;
    public EnvironmentData[] DataList => EnvironmentDatas;

    public string GetKey()
    {
        return "EnvironmentDataList";
    }

    public void SetReferenceData()
    { 
    }
}

[Serializable]
public struct EnvironmentData : IGameData
{
    public string name;
    public Gradient Color;
    public Gradient GlobalColor;
    public Gradient CloudColor;
    public Gradient SkyTopColor, SkyBottomColor;
    public AnimationCurve SkyHalfValue;
    public AnimationCurve GlobalIntensity;
    public AnimationCurve shadowValue;
    public AnimationCurve directionXValue, directionYValue, directionZValue, intensity;
    public AnimationCurve sunXValue, sunYValue,sunScaleValue;
    public Gradient sunColor;
    public AnimationCurve sunColorValue;
    public bool overrideDirection;
    public bool overrideGlobal;

    public string GetKey()
    {
        return name;
    }
    public override string ToString()
    {
        return name;
    }
    public void SetReferenceData()
    { 
    }
}