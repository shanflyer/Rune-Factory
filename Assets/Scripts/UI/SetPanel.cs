using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SetPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Slider masterSlider, bgmSlider, seSlider;

    [SerializeField]
    private Button saveButton, returnButton;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        masterSlider = FindChildGameObject<Slider>("MasterSlider");
        bgmSlider = FindChildGameObject<Slider>("BGMSlider");
        seSlider = FindChildGameObject<Slider>("SESlider");
        saveButton = FindChildGameObject<Button>("SaveButton");
        returnButton = FindChildGameObject<Button>("ReturnButton");
    }

    protected override void Awake()
    {
        base.Awake();

        returnButton.onClick.AddListener(() =>
        {
            Close();
        });
        masterSlider.onValueChanged.AddListener((float value) =>
        {
            AudioController.instance.SetMasterVolume(value);
        });
        bgmSlider.onValueChanged.AddListener((float value) =>
        {
            AudioController.instance.SetBGMVolume(value);
        });
        seSlider.onValueChanged.AddListener((float value) =>
        {
            AudioController.instance.SetSEVolume(value);
        });
        saveButton.onClick.AddListener(SaveSet);
    }
    public override async Task InitData(string dataKey)
    {
       await base.InitData(dataKey);
        float3 volume = AudioController.instance.GetAudioVolume();
        masterSlider.SetValueWithoutNotify(volume.x);
        bgmSlider.SetValueWithoutNotify(volume.y);
        seSlider.SetValueWithoutNotify(volume.z);
    }
    private async void SaveSet()
    {
        Close();
       await UIManager.instance.ShowGamePanel<SavePanel, UserGameSaveDataList>(GameDataSaveManager.instance.UserGameSaveDataList);
    }
}