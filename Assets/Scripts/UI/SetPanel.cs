using UnityEngine;
using UnityEngine.UI;

public class SetPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Slider bgmSlider, seSlider;

    [SerializeField]
    private Button saveButton, returnButton;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
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
            // AudioController.instance.PlayAudio(SE.Return);
            Close();
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

    private async void SaveSet()
    {
        Close();
       await UIManager.instance.ShowGamePanel<SavePanel, UserGameSaveDataList>(GameDataSaveManager.instance.UserGameSaveDataList);
    }
}