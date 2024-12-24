using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationShowPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    TextMeshProUGUI info;
    [SerializeField]
    Button display;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        info = FindChildGameObject<TextMeshProUGUI>("Info");
        display = FindChildGameObject<Button>("Display");
        
    }
    protected override void Awake()
    {
        base.Awake();
        display.onClick.AddListener(async () =>
        {
            //AudioController.instance.PlayAudio(SE.click);
           await UIManager.instance.ShowGamePanel<InformationPanel>(layer:20);
        });
    }
    public void SetInfo(string str)
    {
        info.SetSWText(str);
    }
}
