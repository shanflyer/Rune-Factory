using System.Threading.Tasks;

public partial class PackageManager
{
    public override void Init()
    {
        base.Init();
        LoadObjPackageAnimationData();
        GameActionManager.instance.AddAsyncListener<ItemUseAction>(UsetItemAsync, nameof(UsetItemAsync));
        GameActionManager.instance.AddAsyncListener<CreatRuntimePackage>(CreatRuntimePackageAsync, nameof(CreatRuntimePackage));
        GameActionManager.instance.AddListener<RemoveRuntimePackage>(RemoveRuntimePackage);
        GameActionManager.instance.AddAsyncListener<AddPackageItem>(AddPackageItemActionAsync, nameof(AddPackageItemActionAsync));
        GameActionManager.instance.AddAsyncListener<OpenPackage>(OpenPackageAsync, nameof(OpenPackage));
        GameActionManager.instance.AddAsyncListener<GiveGift>(GiveGiftAsync, nameof(GiveGift));
        GameActionManager.instance.AddListener<CheckItemValue>(CheckItemValue);
        GameActionManager.instance.AddAsyncListener<CreatPackage>(CreatPackageAsync, nameof(CreatPackage));
        GameActionManager.instance.AddListener<RemovePackage>(RemovePackage);
        GameActionManager.instance.AddListener<RemovePackageItem>(RemovePackageItemAction);
        GameActionManager.instance.AddAsyncListener<ShowMultiPackagePanel>(ShowMultiPackagePanelAsync, nameof(ShowMultiPackagePanel));
        GameActionManager.instance.AddListener<SetPackageSelectItem>(SetPackageSelectItem);
        GameActionManager.instance.AddListener<RemovePlayerPackageItem>(RemovePlayerPackageItem);
        GameActionManager.instance.AddAsyncListener<AddItemValue>(AddItemValueAsync, nameof(AddItemValue));
        GameActionManager.instance.AddAsyncListener<SetItemValue>(SetItemValueAsync, nameof(SetItemValue));
        GameActionManager.instance.AddListener<CheckCharacterItemValue>(CheckCharacterItemValue);
        GameActionManager.instance.AddListener<CheckCharacterPackageFull>(CheckCharacterPackageFull);
        GameActionManager.instance.AddListener<ChangePackageInnstance>(ChangePackageInnstance);
        GameActionManager.instance.AddListener<RefreshShortcut>(RefreshShortcut);
        GameActionManager.instance.AddListener<RemovePackageItemInstance>(RemovePackageItemInstance);
        GameActionManager.instance.AddAsyncListener<AddPackageItemList>(AddPackageItemListAsync, nameof(AddPackageItemList));
        GameActionManager.instance.AddListener<SortPackageItem>(SortPackageItem);
    }

}
