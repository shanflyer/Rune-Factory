using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Datas/天气数据")]
public class WeatherData : ScriptableObject, IGameData
{
    public int id;
    public string weatherName;
    public float2 temperature;
    public float2 rainfall;
    public float2 fog;
    public float2 windStrength;
    public float4 windDir;
    public float2 cloud;
    public int2 duration;

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