using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : GamePanel
{
    [SerializeField]
    Button setButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        setButton = FindChildGameObject<Button>("SetButton");
    }
    protected override void Awake()
    {
        base.Awake();
        setButton.onClick.AddListener(() =>
        {
            AudioController.instance.PlayAudio(SE.click);
            UIManager.instance.ShowGamePanel<SetPanel>(layer:3);
        });
    }
}
