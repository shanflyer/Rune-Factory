using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PackageSelectReference : UIObjReference<PackageData>
{
    [SerializeField]
    Image background;
    [SerializeField]
    Image check;
    [SerializeField]
    Toggle toggle;
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
    PackageData PackageData;
    SelectAction<PackageData> SelectAction;
    public override async void InitData(PackageData t, SelectAction<PackageData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        this.SelectAction = SelectAction;
        PackageData= t;
        PackageSetData  packageSetData= await GameDataManager.instance.GetAsyncData<PackageSetData>(PackageData.dataId);
        toggle.group = toggleGroup;
        background.sprite= check.sprite = packageSetData.icon.sprite;
    }

}