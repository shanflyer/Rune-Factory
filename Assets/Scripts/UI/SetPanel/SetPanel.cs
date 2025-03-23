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
    private Button saveButton, returnButton;
    [SerializeField]
    private Toggle level0, level1, level2;
    [SerializeField]
    private TMP_Dropdown dropdown;
    [SerializeField]
    private LanguageReference languageReference;
    [SerializeField]
    private Transform languageParent;
    DisplayList<LanguageReference, LanguageData> languages;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        masterSlider = FindChildGameObject<Slider>("MasterSlider");
        bgmSlider = FindChildGameObject<Slider>("BGMSlider");
        seSlider = FindChildGameObject<Slider>("SESlider");
        saveButton = FindChildGameObject<Button>("SaveButton");
        returnButton = FindChildGameObject<Button>("ReturnButton"); 
        level0 = FindChildGameObject<Toggle>("Level0");
        level1 = FindChildGameObject<Toggle>("Level1");
        level2 = FindChildGameObject<Toggle>("Level2");
        languageReference = FindChildGameObject<LanguageReference>("LanguageReference");
        languageParent = FindChildGameObject("LanguageParent");

    }

    protected override void Awake()
    {
        base.Awake();
        languages = new DisplayList<LanguageReference, LanguageData>(languageReference, languageParent);

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
        RefreshLanguage();
    }
    void RefreshLanguage()
    {
        languages.InitListData(LanguageManage.instance.languageDatas, (LanguageData languageData, bool selected) =>
        {
            if (selected)
            {
                LanguageManage.instance.SetLanguage(languageData.languageType);
            }
        });
    }
    private async void SaveSet()
    {
        Close();
       await UIManager.instance.ShowGamePanel<SavePanel, UserGameSaveDataList>(GameDataSaveManager.instance.UserGameSaveDataList);
    }
}