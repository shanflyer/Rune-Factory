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
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<CharacterPropertyTrigger>(RefreshPlayerProperty);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<CharacterPropertyTrigger>(RefreshPlayerProperty);
    }
    public override async Task InitData(int dataId)
    {
       var characterProperty= CharacterManager.instance.player.CharacterProperty;
        hpSlider.value = characterProperty.HP / (float)characterProperty.MaxHP;
        rpSlider.value = characterProperty.MP / (float)characterProperty.MaxMP;

        icon.sprite = await CharacterManager.instance.GetPlayerIcon();
        base.InitData(dataId);
    }
    void RefreshPlayerProperty(CharacterPropertyTrigger characterPropertyTrigger)
    {
        if (characterPropertyTrigger.characterId == 0)
        {
            var characterProperty = characterPropertyTrigger.characterProperty;

            hpSlider.value = characterProperty.HP / (float)characterProperty.MaxHP;
            rpSlider.value = characterProperty.MP / (float)characterProperty.MaxMP;
        }
    } 
}
