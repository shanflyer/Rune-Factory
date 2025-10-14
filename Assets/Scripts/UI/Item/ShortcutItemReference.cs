using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShortcutItemReference : UIObjReference<ShortcutItem>
{
    [SerializeField]
    private Toggle toggle;
    //[SerializeField]
    // private Image icon;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Transform ItemValueBg;
    [SerializeField]
    private Image ItemValue;
    [SerializeField]
    private TextMeshProUGUI count;

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if(SelectAction != null)
            {
                SelectAction(data,index, value);
            }
        });
    }
    public override void ClearSelect()
    {
        base.ClearSelect();
        toggle.SetIsOnWithoutNotify(false);
       
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon");
        count = FindChildGameObject<TextMeshProUGUI>("count");
        ItemValue = FindChildGameObject<Image>("ItemValue");
        ItemValueBg = FindChildGameObject("ItemValueBg");
    }
    public override void SelectDefault()
    {
        base.SelectDefault();
        if (SelectAction != null)
        {
            SelectAction(data,index, true);
        }
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshItemValue>(RefreshItemValue);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<RefreshItemValue>(RefreshItemValue);
    }
    async void RefreshItemValue(RefreshItemValue refreshItemValue)
    {
        if (refreshItemValue.itemId == data.Item.instanceId)
        {
            data.Item=await Item.SetValue(data.Item,refreshItemValue.itemValue);
            ItemValue.fillAmount =await data.Item.GetValue();
        }
    }
    public override void ClearData()
    {
        base.ClearData();
        data = default(ShortcutItem);
        icon.enabled = false;
        count.enabled = false; 
    }
    ItemData itemData;
    public override async Task InitData(ShortcutItem t, SelectAction<ShortcutItem> SelectAction = null, ToggleGroup toggleGroup = null)
    {
       await base.InitData(t, SelectAction, toggleGroup); 

        toggle.group = toggleGroup;
        this.SelectAction = SelectAction;
        itemData = await GameDataManager.instance.GetAsyncData<ItemData>(data.Item.dataId.ToString());
        toggle.enabled = true;
        if (itemData != null)
        {
            icon.sprite = itemData.icon;
            icon.color = (data.Item.instanceId != -1) ? Color.white : new Color(1, 1, 1, 0.3f);
            icon.enabled = true;
            // icon.SetNativeSize();
            count.text = data.Item.count.ToString();
            count.enabled = data.Item.count > 0;
            toggle.enabled = true;
            ItemValueBg.transform.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
            ItemValue.fillAmount =await data.Item.GetValue(); 

        }
        else
        {
            ItemValueBg.transform.localScale = Vector3.zero; 
            toggle.SetIsOnWithoutNotify(false);
            toggle.enabled = false;
            // toggle.graphic.enabled = false;
            icon.enabled = false;
            count.enabled = false; 
        }
    }
}