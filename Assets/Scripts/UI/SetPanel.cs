using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Slider bgmSlider, seSlider;
    [SerializeField]
    Button saveButton,returnButton;
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
    void SaveSet()
    {
        UIManager.instance.ShowGamePanel<SavePanel>(layer: 2);
    }
}
