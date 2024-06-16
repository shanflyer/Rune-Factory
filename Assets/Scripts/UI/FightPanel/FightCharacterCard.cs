using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FightCharacterCard : UIObjReference<FightCharacter>
{
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Image value; 
    [SerializeField]
    private Transform ActionTips; 
    public override void ClearData()
    {
        base.ClearData();
        data = null;
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        value = FindChildGameObject<Image>("Value");
        icon = FindChildGameObject<Image>("Icon");
        ActionTips = FindChildGameObject("ActionTips");
    }

    public override Task InitData(FightCharacter t, SelectAction<FightCharacter> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        icon.sprite = data.icon;
        icon.rectTransform.sizeDelta = GameCommon.SetImageSize(data.icon, new Vector2(32, 32));
        ActionTips.localScale = Vector2.zero;
        return base.InitData(t, SelectAction, toggleGroup);
    }

    private void Awake()
    {
    }

    private void Update()
    {
        value.fillAmount = t.WaiteValue();
    }
}