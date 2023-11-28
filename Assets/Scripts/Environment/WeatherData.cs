using UnityEngine;

[CreateAssetMenu(menuName = "Datas/天气数据")]
public class WeatherData : ScriptableObject, IGameData
{
    public int id;
    public string weatherName;
    public int temperature;
    public int rainfall;
    public int fog;
    public int wind;
    public int cloud;

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