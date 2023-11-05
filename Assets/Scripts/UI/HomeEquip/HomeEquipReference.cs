using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeEquipReference : UIObjReference<HomeEquip>
{
    [SerializeField]
    private Toggle toggle; 
    [SerializeField]
    private Image icon; 
     
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon"); 
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
    public override async void InitData(HomeEquip t, SelectAction<HomeEquip> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup); 

        toggle.group = toggleGroup; 
        ItemData homeEquipData = await GameDataManager.instance.GetAsyncData<ItemData>(data.dataId);
        toggle.enabled = true;
        if (homeEquipData != null)
        {
            icon.sprite = homeEquipData.icon;
            icon.color = data.mapInstance==0? Color.white : new Color(1,1,1, 0.5f);
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