using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HomeEquipReference : UIObjReference<HomeEquip>
{
    [SerializeField]
    private Toggle toggle;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Transform setTips, unSetTips;
    [SerializeField]
    private Image AnimationIcon;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon");
        setTips = FindChildGameObject("SetTips");
        unSetTips = FindChildGameObject("UnSetTips");
        AnimationIcon = FindChildGameObject<Image>("AnimationIcon");
    }

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (!icon.enabled)
            {
                toggle.SetIsOnWithoutNotify(false);
                return;
            }
            if (SelectAction != null)
            {
                SelectAction.Invoke(data,index, value);
            }
        });
    }

    public void SetAnimationIcon(bool isShow)
    {
        AnimationIcon.enabled = isShow;
    }
    public override void ClearData()
    {
        data = null;
        icon.enabled = false;
    }
    public override void ClearSelect()
    {
        base.ClearSelect();
        toggle.SetIsOnWithoutNotify(false);
    }
    public override void SelectDefault()
    {
        base.SelectDefault();
        toggle.isOn = true;
    }

    public override async Task InitData(HomeEquip t, SelectAction<HomeEquip> SelectAction = null, ToggleGroup toggleGroup = null)
    {
       await base.InitData(t, SelectAction, toggleGroup);

        toggle.group = toggleGroup;
        HomeEquipmentData homeEquipmentData = data.homeEquipmentData; 
        toggle.enabled = true;
        setTips.gameObject.SetActive(data.mapInstance > 0);
        unSetTips.gameObject.SetActive(data.mapInstance <= 0);
        AnimationIcon.enabled = false;
        if (homeEquipmentData != null)
        {
            AnimationIcon.sprite = icon.sprite = homeEquipmentData.icon;
            icon.color = data.mapInstance == 0 ? Color.white : new Color(1, 1, 1, 0.5f);
            icon.enabled = true;
            icon.SetNativeSize();
            AnimationIcon.SetNativeSize();
            toggle.enabled = true;
        }
        else
        {
            toggle.SetIsOnWithoutNotify(false);
            toggle.enabled = false;
            icon.enabled = false;
        }
    }
}