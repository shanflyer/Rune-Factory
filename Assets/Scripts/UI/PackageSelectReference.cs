using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PackageSelectReference : UIObjReference<PackageData>
{
    [SerializeField]
    private Image background;

    [SerializeField]
    private Image check;

    [SerializeField]
    private Toggle toggle;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        background = FindChildGameObject<Image>("Background");
        check = FindChildGameObject<Image>("Checkmark");
        toggle = GetComponent<Toggle>();
    }

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(PackageData);
            }
        });
    }

    private PackageData PackageData;
    private SelectAction<PackageData> SelectAction;

    public override async Task InitData(PackageData t, SelectAction<PackageData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        this.SelectAction = SelectAction;
        PackageData = t;
        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(PackageData.dataId);
        toggle.group = toggleGroup;
        background.sprite = check.sprite = packageSetData.icon.sprite;
        background.SetNativeSize();
        check.SetNativeSize();
    }
}