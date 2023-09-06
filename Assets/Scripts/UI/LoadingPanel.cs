using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class LoadingPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Slider slider; 
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        slider = FindChildGameObject<Slider>("Slider");
    }
    public override Task InitData(string dataKey)
    {
        return base.InitData(dataKey); 
    }

    public void RefreshLoadValue(float value)
    {
        slider.value = value;
    }

    AsyncOperation AsyncOperation;
 
 
}
