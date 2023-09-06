using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InformationShowPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Text info;
    [SerializeField]
    Button display;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        info = FindChildGameObject<Text>("Info");
        display = FindChildGameObject<Button>("Display");
        
    }
    protected override void Awake()
    {
        base.Awake();
        display.onClick.AddListener(() =>
        {
            AudioController.instance.PlayAudio(SE.click);
            UIManager.instance.ShowGamePanel<InformationPanel>(layer:20);
        });
    }
    public void SetInfo(string str)
    {
        info.text = str;
    }
}
