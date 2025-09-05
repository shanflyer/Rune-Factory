using UnityEngine;

public class ShowItemManager : Singleton<ShowItemManager>
{
    private ShowItem prefab;

    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<ShowItemAction>(ShowItemAction);
        prefab = ExtensionsResources.LoadResource<ShowItem>("Prefabs/Other/ShowItem");
    }

    public async void ShowItemAction(ShowItemAction ShowItemAction)
    {
        var itemData = await GameDataManager.instance.GetAsyncData<ItemData>(ShowItemAction.itemId);
        var runtimeObj = await GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.OTHER.ToString(),
            "ShowItem", prefab, 0);
        (runtimeObj.obj as ShowItem).Show(itemData.icon, ShowItemAction.mapInstanceId, ShowItemAction.position,
            runtimeObj);
    }

    public async void Show(Sprite sprite, int mapInstance, Vector3 position)
    {
        var runtimeObj = await GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.OTHER.ToString(),
            "ShowItem", prefab, 0);
        (runtimeObj.obj as ShowItem).Show(sprite, mapInstance, position, runtimeObj);
    }

    protected override void Clear()
    {
        base.Clear();
    }
}