using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OperateButtonReference : UIObjReference<OperateDataReferenceData>
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private Button button;

    [SerializeField]
    private Transform ValueBg;

    [SerializeField]
    private Image Value;

    [SerializeField]
    private Image Icon;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            if (SelectAction != null)
            {
                SelectAction(data, index);
            }
        });
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<RefreshItemValue>(RefreshItemValue);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshItemValue>(RefreshItemValue);
    }

    private void RefreshItemValue(RefreshItemValue refreshItemValue)
    {
        if (refreshItemValue.characterId == CharacterManager.instance.controllerCharacter.instanceId)
        {
            if (refreshItemValue.itemId == linkItemId)
            {
                Value.fillAmount = refreshItemValue.itemValue;
            }
        }
    }

    private int linkItemId = 0;

    public override async Task InitData(OperateDataReferenceData t, SelectAction<OperateDataReferenceData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
       await  base.InitData(t, SelectAction, toggleGroup);
        nameText.SetSWText(t.operateData.operateName);
        if (t.operateData.linkItem == 0)
        {
            Icon.enabled = false;
            ValueBg.localScale = Vector3.zero;
        }
        else
        {
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(t.operateData.linkItem);
            if (itemData != null)
            {
                linkItemId = t.operateData.linkItem;
                Icon.enabled = true;
                Icon.sprite = itemData.icon;
                ValueBg.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
            }
            else
            {
                Icon.enabled = false;
                ValueBg.localScale = Vector3.zero;
            }
        }
        this.SelectAction = SelectAction;
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        nameText = FindChildGameObject<TextMeshProUGUI>("Name");
        button = GetComponent<Button>();
        ValueBg = FindChildGameObject("ValueBg");
        Value = FindChildGameObject<Image>("Value");
        Icon = FindChildGameObject<Image>("Icon");
    }
}
