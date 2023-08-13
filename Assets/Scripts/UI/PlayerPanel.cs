using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : GamePanel
{
    [SerializeField]
    Slider hpSlider, rpSlider;
    [SerializeField]
    Image icon;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        icon = FindChildGameObject<Image>("Icon");
        hpSlider = FindChildGameObject<Slider>("HPSlider");
        rpSlider = FindChildGameObject<Slider>("RPSlider");
    }
    protected override void Awake()
    {
        base.Awake();
    }
    public override Task InitData(int dataId)
    {
        return base.InitData(dataId);
    }
    public void RefreshPlayerHPAndRP()
    {

    }
}
