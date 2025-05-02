 
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SetPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Slider masterSlider, bgmSlider, seSlider;
    [SerializeField]
    private Slider cameraSlider;
    [SerializeField]
    private Button saveButton, returnButton,languageButton;
    [SerializeField]
    private Toggle level0, level1, level2;
    [SerializeField]
    private TMP_Dropdown dropdown; 
    [SerializeField]
    Button changeColorButton;
    [SerializeField]
    Slider colorASlider;
    [SerializeField]
    Image JoyStickColor;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        masterSlider = FindChildGameObject<Slider>("MasterSlider");
        bgmSlider = FindChildGameObject<Slider>("BGMSlider");
        seSlider = FindChildGameObject<Slider>("SESlider");
        saveButton = FindChildGameObject<Button>("SaveButton");
        returnButton = FindChildGameObject<Button>("ReturnButton");
        languageButton = FindChildGameObject<Button>("LanguageButton");
        level0 = FindChildGameObject<Toggle>("Level0");
        level1 = FindChildGameObject<Toggle>("Level1");
        level2 = FindChildGameObject<Toggle>("Level2");
        colorASlider = FindChildGameObject<Slider>("ASlider");
        changeColorButton = FindChildGameObject<Button>("ChangeButton");
        JoyStickColor = FindChildGameObject<Image>("ColorPreviewBackground");
        cameraSlider = FindChildGameObject<Slider>("CameraSlider");
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

        level0.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                GameVolumeManager.instance.volumeLevel = 0;
            }
        });
        level1.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                GameVolumeManager.instance.volumeLevel = 1;
            }
        });
        level2.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                GameVolumeManager.instance.volumeLevel = 2;
            }
        });

        dropdown.onValueChanged.AddListener((int value) =>
        {
            Shader.SetGlobalInt("testShowType", value);
        });
        languageButton.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<LanguagePanel>();
        });
        changeColorButton.onClick.AddListener(async () =>
        {
            ColorPickerPanel ColorPickerPanel=await UIManager.instance.ShowGamePanel<ColorPickerPanel,MyColor>(
                new MyColor { color =UIManager.instance.JoyStickColor,colorEvent= JoyStickColorChange });
        });
        colorASlider.onValueChanged.AddListener((float value) =>
        {
            Color color=UIManager.instance.JoyStickColor;
            color.a = value;
            JoyStickColorChange(color);
        });
        cameraSlider.onValueChanged.AddListener((float value) =>
        {
            GameVolumeManager.instance.DepthFieldValue = value;
        });
    }
    void JoyStickColorChange(Color color)
    {
        JoyStickColor.color = color;
        color.a = colorASlider.value;
        
        UIManager.instance.SetJoyStickColor(color);
    }
    public override async Task InitData(string dataKey)
    {
       await base.InitData(dataKey);
        float3 volume = AudioController.instance.GetAudioVolume();
        masterSlider.SetValueWithoutNotify(volume.x);
        bgmSlider.SetValueWithoutNotify(volume.y);
        seSlider.SetValueWithoutNotify(volume.z);

        int volumeLevel = GameVolumeManager.instance.volumeLevel;
        switch (volumeLevel)
        {
            case 0:
                level0.SetIsOnWithoutNotify(true);
                break;
            case 1:
                level1.SetIsOnWithoutNotify(true);
                break;
            case 2:
                level2.SetIsOnWithoutNotify(true);
                break;
        }
        var color = UIManager.instance.JoyStickColor;
        color.a = 1;
        JoyStickColor.color = color;
        colorASlider.SetValueWithoutNotify(UIManager.instance.JoyStickColor.a);

        cameraSlider.SetValueWithoutNotify(GameVolumeManager.instance.DepthFieldValue);
    } 
    private async void SaveSet()
    {
        Close();
       await UIManager.instance.ShowGamePanel<SavePanel, UserGameSaveDataList>(GameDataSaveManager.instance.UserGameSaveDataList);
    }
}