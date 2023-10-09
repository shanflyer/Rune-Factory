using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBoxReference : UIObjReference<Item>
{
    [SerializeField]
    private Toggle toggle;

    [SerializeField]
    private Image icon;
     

    [SerializeField]
    private TextMeshProUGUI count;

    private Item item;
    public Item Item => item;
    private SelectAction<Item> SelectAction;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon");  
        count = FindChildGameObject<TextMeshProUGUI>("count");
    }

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction.Invoke(item);
            }
        });
    }
    public void ClearData()
    {
        item = default(Item); 
        icon.enabled = false;
        count.enabled = false;
    }
   
    public override async void InitData(Item t, SelectAction<Item> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        item = t;

        toggle.group=toggleGroup;
        this.SelectAction = SelectAction;
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());

        if (itemData != null)
        { 
            icon.sprite = itemData.icon;
            icon.color =item.instanceId>=0? Color.white:new Color(1,1,1,0.5f);
            icon.enabled = true;
            icon.SetNativeSize();
            count.text = item.count.ToString();
            count.enabled = true;
            toggle.enabled = true; 
        }
        else
        {
            toggle.enabled = false;
            icon.enabled = false;
            count.enabled = false;
        }
    } 
}