using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBoxReference : UIObjReference<Item>
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

    [SerializeField]
    private Image LockMask;

    private Item item;
    public Item Item => item;
    public SelectUIAction<ItemBoxReference> SelectUIAction;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon");
        count = FindChildGameObject<TextMeshProUGUI>("count");
        ItemValue = FindChildGameObject<Image>("ItemValue");
        ItemValueBg = FindChildGameObject("ItemValueBg");
        LockMask = FindChildGameObject<Image>("LockMask");
    }

    public override void ClearSelect()
    {
        base.ClearSelect();
        toggle.SetIsOnWithoutNotify(false);
    }

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (!icon.enabled && item.instanceId != -1)
            {
                toggle.SetIsOnWithoutNotify(false);
                return;
            }
            if (SelectAction != null)
            {
                SelectAction.Invoke(item, value);
            }
            else if (SelectUIAction != null)
            {
                SelectUIAction.Invoke(this, value);
            }
        });
    }

    public void ClearData(int instanceId = 0)
    {
        item = default(Item);
        item.instanceId = instanceId;
        icon.enabled = false;
        count.enabled = false;
    }

    public override void SelectDefault()
    {
        base.SelectDefault();
        toggle.isOn = true;
        toggle.SetIsOnWithoutNotify(true);
    }
    public void SetCountColor(Color color)
    {
        count.color = color;
    }

    public override async Task InitData(Item t, SelectAction<Item> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        await base.InitData(t, SelectAction, toggleGroup);
        item = t;

        toggle.group = toggleGroup;
        this.SelectAction = SelectAction;
        ItemData itemData = null;
        if (item.dataId != 0)
        {
            itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
        } 
        toggle.enabled = true;

        if (t.locked && LockMask)
        {
            LockMask.transform.localScale = Vector3.one;
            toggle.interactable = false;
            toggle.SetIsOnWithoutNotify(false);
        }
        else
        {
            if (LockMask)
            {
                LockMask.transform.localScale = Vector3.zero;
            }
            toggle.interactable = true;
        }

        if (itemData != null)
        {
            icon.sprite = itemData.icon;
            if (LockMask)
            {
                if (t.locked)
                {
                    LockMask.transform.localScale = Vector3.one;
                }
                else
                {
                    LockMask.transform.localScale = item.instanceId == -1 ? Vector3.one : Vector3.zero; 
                } 
            }
            else
            {
                icon.color = (item.instanceId != -1) ? Color.white : new Color(1, 1, 1, 0.6f);
            } 
            icon.enabled = true;
            icon.rectTransform.sizeDelta = GameCommon.SetImageSize(icon.sprite, new Vector2(32, 32));
            count.text = item.count.ToString();
            count.enabled = item.count > 0;
            toggle.enabled = true;
            if (ItemValueBg)
                ItemValueBg.transform.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
            if (ItemValue)
                ItemValue.fillAmount = item.value;
        }
        else
        {
            if (ItemValueBg)
                ItemValueBg.transform.localScale = Vector3.zero;
            toggle.SetIsOnWithoutNotify(false);
            toggle.enabled = item.instanceId == -1;
            toggle.graphic.enabled = item.instanceId == -1;
            icon.enabled = false;
            count.enabled = false;
            if (LockMask)
            {
                LockMask.transform.localScale = Vector3.zero;
            }
        }
    }
}