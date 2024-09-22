using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransmissionPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Button closeBtn;

    protected override void Awake()
    {
        base.Awake();
        closeBtn.onClick.AddListener(Close);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        closeBtn = FindChildGameObject<Button>("Close");
    }

}
