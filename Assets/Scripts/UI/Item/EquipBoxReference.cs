using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipBoxReference : UIObjReference<Equipment>
{
    [SerializeField]
    private TextMeshProUGUI typeText, NameText;

    [SerializeField]
    private Image equipMentIcon;

    [SerializeField]
    private Button clickButton;

    [SerializeField]
    private Transform itemValueBg;

    [SerializeField]
    private Image itemValue;

    [SerializeField]
    private Image hideMask;

    private void Awake()
    {
        clickButton.onClick.AddListener(() =>
        {
            if (SelectAction != null)
            {
                SelectAction(data);
            }
        });
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        typeText = FindChildGameObject<TextMeshProUGUI>("Type");
        equipMentIcon = FindChildGameObject<Image>("Icon");
        NameText = FindChildGameObject<TextMeshProUGUI>("Name");
        clickButton = GetComponent<Button>();
        itemValueBg = FindChildGameObject("ItemValueBg");
        itemValue = FindChildGameObject<Image>("ItemValue");
        hideMask = FindChildGameObject<Image>("Hide");
    }

    public override async void InitData(Equipment t, SelectAction<Equipment> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        typeText.SetSWText(data.ItemType.ToString());
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(data.dataId);
        if (itemData != null)
        {
            NameText.SetSWText(itemData.itemName);
            equipMentIcon.sprite = itemData.icon;

            NameText.enabled = true;
            equipMentIcon.enabled = true;
            itemValueBg.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
        }
        else
        {
            NameText.enabled = false;
            equipMentIcon.enabled = false;
            itemValueBg.localScale = Vector3.zero;
            itemValue.fillAmount = t.itemValue;
        }
        hideMask.gameObject.SetActive(t.hide);
        clickButton.interactable = !t.hide;
    }
}