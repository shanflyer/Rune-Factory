using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : GamePanel<IReferenceData>
{
    public override bool changeInputModel => false;
    [SerializeField]
    Button InfoButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        InfoButton = FindChildGameObject<Button>("Info");
    }
    protected override void Awake()
    {
        base.Awake();
        InfoButton.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<BookPanel>();
        });
    }
}
