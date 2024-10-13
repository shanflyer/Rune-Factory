using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct WeatherReferenceData:IReferenceData
{
    public string time;
    public Weather weather;
}
public class WeatherReference : UIObjReference<WeatherReferenceData>
{
    [SerializeField]
    TextMeshProUGUI timeText;
    [SerializeField]
    Image weatherIcon;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        timeText = FindChildGameObject<TextMeshProUGUI>("Time");
        weatherIcon = FindChildGameObject<Image>("icon");
    }
    public override Task InitData(WeatherReferenceData t, SelectAction<WeatherReferenceData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        timeText.text = data.time;
        weatherIcon.sprite = WeatherManager.instance.GetWeatherIcon(data.weather);
        return base.InitData(t, SelectAction, toggleGroup);
    }
}
