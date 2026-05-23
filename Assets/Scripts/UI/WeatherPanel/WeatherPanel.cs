using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class WeatherPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    new Animation animation;
    [SerializeField]   
    Transform nowWeatherParent, nextWeatherParent;
    DisplayList<WeatherReference, WeatherReferenceData> nowWeathers;
    DisplayList<WeatherReference, WeatherReferenceData> nextWeathers;
    [SerializeField]
    WeatherReference weatherReference;
    [SerializeField]
    Button closeBtn;

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
        nextWeathers=new DisplayList<WeatherReference, WeatherReferenceData>(weatherReference,nextWeatherParent);
        closeBtn.onClick.AddListener(Close);
    }
    public override void Close()
    {
        base.Close();
        CameraManager.instance.SetUICameraPostProcessing(false);
    }
    public override async Task InitData(string dataKey)
    {
        CameraManager.instance.SetUICameraPostProcessing(true);
        animation.Play();
        var nowWeatherReferences = WeatherManager.instance.GetNowWeatherReferenceDatas();
        await nowWeathers.InitListData(nowWeatherReferences);
        var nextWeatherReferences = WeatherManager.instance.GetNextWeatherReferenceDatas();
        await  nextWeathers.InitListData(nextWeatherReferences);
        await base.InitData(dataKey);
    }
    public override void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v);
        
    }

}
