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

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon");
        setTips = FindChildGameObject("SetTips");
        unSetTips = FindChildGameObject("UnSetTips");
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
                SelectAction.Invoke(data, value);
            }
        });
    }

    public void ClearData()
    {
        data = default(HomeEquip);
        icon.enabled = false;
    }

    public override void SelectDefault()
    {
        base.SelectDefault();
        toggle.isOn = true;
    }

    public override async Task InitData(HomeEquip t, SelectAction<HomeEquip> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);

        toggle.group = toggleGroup;
        HomeEquipmentData homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(data.equipDataId); 
        toggle.enabled = true;
        setTips.gameObject.SetActive(data.mapInstance > 0);
        unSetTips.gameObject.SetActive(data.mapInstance <= 0);

        if (homeEquipmentData != null)
        {
            icon.sprite = homeEquipmentData.icon;
            icon.color = data.mapInstance == 0 ? Color.white : new Color(1, 1, 1, 0.5f);
            icon.enabled = true;
            icon.SetNativeSize();
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