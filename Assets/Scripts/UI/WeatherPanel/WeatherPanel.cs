using UnityEngine;
using UnityEngine.UI;

public class WeatherPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Animation animation;

    [SerializeField]
    private Transform nowWeatherParent, nextWeatherParent;

    private DisplayList<WeatherReference, WeatherReferenceData> nowWeathers;
    private DisplayList<WeatherReference, WeatherReferenceData> nextWeathers;

    [SerializeField]
    private WeatherReference weatherReference;

    [SerializeField]
    private Button closeBtn;

    public override void SetPanelUISerializeObj()
    {
        animation = FindChildGameObject<Animation>("Mask");
        nowWeatherParent = FindChildGameObject("NowWeathers");
        nextWeatherParent = FindChildGameObject("NextWeathers");
        weatherReference = FindChildGameObject<WeatherReference>("WeatherReference");
        closeBtn = FindChildGameObject<Button>("Close");
        base.SetPanelUISerializeObj();
    }

    protected override void Awake()
    {
        base.Awake();
        nowWeathers = new DisplayList<WeatherReference, WeatherReferenceData>(weatherReference, nowWeatherParent);
        nextWeathers = new DisplayList<WeatherReference, WeatherReferenceData>(weatherReference, nextWeatherParent);
        closeBtn.onClick.AddListener(Close);
    }

    public override void Close()
    {
        base.Close();
        CameraManager.instance.SetUICameraPostProcessing(false);
    }

    public override void InitData(string dataKey)
    {
        CameraManager.instance.SetUICameraPostProcessing(true);
        animation.Play();
        var nowWeatherReferences = WeatherManager.instance.GetNowWeatherReferenceDatas();
        nowWeathers.InitListData(nowWeatherReferences);
        var nextWeatherReferences = WeatherManager.instance.GetNextWeatherReferenceDatas();
        nextWeathers.InitListData(nextWeatherReferences);
        base.InitData(dataKey);
    }

    public override void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v);
    }
}