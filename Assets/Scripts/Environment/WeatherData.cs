using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Datas/天气数据")]
public class WeatherData : ScriptableObject, IGameData
{
    public int id;
    public string weatherName; 
    public float2 waterFall;
    public float2 fog;
    public float2 windStrength; 
    public float2 cloud;
    public float2 lightning;
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