using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectStartPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Button NewButton, LoadButton;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        NewButton = FindChildGameObject<Button>("New");
        LoadButton = FindChildGameObject<Button>("Load");
    }
    protected override void Awake()
    {
        base.Awake();
        NewButton.onClick.AddListener(() =>
        {
            RunLifecycleTask(_ => NewGameAsync(), nameof(NewButton));
        });
        LoadButton.onClick.AddListener(() =>
        {
            Close();
            UIManager.instance.CloseGamePanel<ZeroPanel>();

        });
    }

    private async System.Threading.Tasks.Task NewGameAsync()
    {
        Close();
        UIManager.instance.CloseGamePanel<ZeroPanel>();
        await UIManager.instance.ShowGamePanel<SelectCharacterPanel>();
    }
}
