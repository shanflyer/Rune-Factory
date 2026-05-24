using System.Collections.Generic;
using UnityEngine;

public class ShowItemManager : Singleton<ShowItemManager>
{
    private ShowItem prefab;
    public override IReadOnlyList<System.Type> InitializationDependencies => new[] { typeof(GameActionManager), typeof(GameSourceManager) };

    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddAsyncListener<ShowItemAction>(ShowItemActionAsync, nameof(ShowItemAction));
        // 展示物 prefab 统一走 GameSourceManager 缓存，避免每次系统初始化直接打到 Resources。
        prefab = GameSourceManager.instance.GetComponentImmediately<ShowItem>("Prefabs/Other/ShowItem");
        if (prefab == null)
        {
            Debug.LogError("ShowItemManager init failed: missing Prefabs/Other/ShowItem.");
        }
    }

    private async System.Threading.Tasks.Task ShowItemActionAsync(ShowItemAction ShowItemAction)
    {
        var itemData = await GameDataManager.instance.GetAsyncData<ItemData>(ShowItemAction.itemId);
        var runtimeObj = await GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.OTHER.ToString(),
            "ShowItem", prefab, 0);
        (runtimeObj.obj as ShowItem).Show(itemData.icon, ShowItemAction.mapInstanceId, ShowItemAction.position,
            runtimeObj);
    }

    public void Show(Sprite sprite, int mapInstance, Vector3 position)
    {
        AsyncTaskRunner.Run(() => ShowAsync(sprite, mapInstance, position), nameof(Show));
    }

    public async System.Threading.Tasks.Task ShowAsync(Sprite sprite, int mapInstance, Vector3 position)
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
