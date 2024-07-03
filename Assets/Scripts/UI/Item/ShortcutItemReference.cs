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
    [SerializeField]
    Button UseButton, UnSetButton;
    [SerializeField]
    Image setImage;

    public override void ClearSelect()
    {
        base.ClearSelect();
        toggle.SetIsOnWithoutNotify(false);
        setImage.transform.localScale = UnSetButton.transform.localScale = UseButton.transform.localScale = Vector3.zero;
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon");
        count = FindChildGameObject<TextMeshProUGUI>("count");
        ItemValue = FindChildGameObject<Image>("ItemValue");
        ItemValueBg = FindChildGameObject("ItemValueBg");
        UseButton = FindChildGameObject<Button>("UseButton");
        UnSetButton = FindChildGameObject<Button>("UnSetButton");
        setImage = FindChildGameObject<Image>("setImage");
    }
    public override void SelectDefault()
    {
        base.SelectDefault();
        if (SelectAction != null)
        {
            SelectAction(data, true);
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
    void RefreshItemValue(RefreshItemValue refreshItemValue)
    {
        if (refreshItemValue.itemId == data.Item.instanceId)
        {
            ItemValue.fillAmount = refreshItemValue.itemValue;
        }
    }
    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            setImage.transform.localScale= setImage.transform.localScale = UnSetButton.transform.localScale = value ? Vector3.one : Vector3.zero;
            if (SelectAction != null)
            {
                SelectAction.Invoke(data,value);
            }
            if (value&& itemData!=null)
            { 
                UseButton.transform.localScale = itemData.useEventId.Count > 0 ? Vector3.one : Vector3.zero;
            }
            else
            {
                UseButton.transform.localScale = Vector3.zero;
            }
        });
        UseButton.onClick.AddListener(() =>
        {
            if(itemData!=null)
            {
                ItemUseAction itemUseAction = new ItemUseAction
                {
                    itemCount = 0,
                    packageId = CharacterManager.instance.controllerCharacter.characterPackage,
                    itemId = itemData.id
                };
                GameActionManager.instance.QueueAction(itemUseAction, true);

            }
        });
        UseButton.transform.localScale = Vector3.zero;
        UnSetButton.onClick.AddListener(UnSetAction);
        UnSetButton.transform.localScale = Vector3.zero;
        setImage.transform.localScale = Vector3.zero;
    }

    void UnSetAction()
    {
        RemoveShortcutItem removeShortcutItem = new RemoveShortcutItem
        {
            characterId = CharacterManager.instance.controllerCharacter.instanceId,
            index = data.index
        };
        GameActionManager.instance.QueueAction(removeShortcutItem);
        UnSetButton.transform.localScale=UseButton.transform.localScale=setImage.transform.localScale = Vector3.zero;
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
        base.InitData(t, SelectAction, toggleGroup); 

        toggle.group = toggleGroup;
        this.SelectAction = SelectAction;
        itemData = await GameDataManager.instance.GetAsyncData<ItemData>(data.Item.dataId.ToString());
        toggle.enabled = true;
        if (itemData != null)
        {
            icon.sprite = itemData.icon;
            icon.color = (data.Item.instanceId != -1) ? Color.white : new Color(1, 1, 1, 0.3f);
            icon.enabled = true;
            icon.SetNativeSize();
            count.text = data.Item.count.ToString();
            count.enabled = data.Item.count > 0;
            toggle.enabled = true;
            ItemValueBg.transform.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
            ItemValue.fillAmount = data.Item.value; 

        }
        else
        {
            ItemValueBg.transform.localScale = UseButton.transform.localScale =setImage.transform.localScale= Vector3.zero; 
            toggle.SetIsOnWithoutNotify(false);
            toggle.enabled = false;
            // toggle.graphic.enabled = false;
            icon.enabled = false;
            count.enabled = false;
        }
    }
}