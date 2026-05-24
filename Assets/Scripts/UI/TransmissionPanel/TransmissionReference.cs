using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;

public class TransmissionReference:UIObjReference<IReferenceData>
{
    [SerializeField]
    int mapInstance;
    [SerializeField]
    int2 coordinate;
    [SerializeField]
    Button button;
    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            CharacterManager.instance.FixedTransMap(CharacterManager.instance.controllerCharacter, new int3(coordinate.xy, mapInstance));
            UIManager.instance.CloseGamePanel<TransmissionPanel>();
            UIManager.instance.CloseGamePanel<WarehousePanel>();
        });
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        button = gameObject.GetComponentInChildren<Button>(false);
    }

}
