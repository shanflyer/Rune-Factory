using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class AddressableRemoteManager : Singleton<AddressableRemoteManager>
{
    public override void Init()
    {
        base.Init();
        InitAndUpdateCatalogs();
    }

    private readonly Dictionary<string, AsyncOperationHandle> AsyncOperationDic = new();

    public T LoadAddressable<T>(string key) where T : Object
    {
        if (!AsyncOperationDic.TryGetValue(key, out var handle))
        {
            handle = Addressables.LoadAssetAsync<T>(key);
            AsyncOperationDic[key] = handle;
        }

        var obj = handle.WaitForCompletion();
        return obj as T;
    }

    public T LoadAddressablePrefab<T>(string key) where T : Component
    {
        if (!AsyncOperationDic.TryGetValue(key, out var handle))
        {
            handle = Addressables.LoadAssetAsync<GameObject>(key);
            AsyncOperationDic[key] = handle;
        }

        var obj = handle.WaitForCompletion();
        return (obj as GameObject).GetComponent<T>();
    }

    public void Release(string key)
    {
        if (AsyncOperationDic.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            AsyncOperationDic.Remove(key);
        }
    }

    private async void InitAndUpdateCatalogs()
    {
        var checkHandle = Addressables.CheckForCatalogUpdates();
        IList<string> catalogsToUpdate = await checkHandle.Task;
        if (catalogsToUpdate == null || catalogsToUpdate.Count == 0)
        {
            Debug.Log("[Addr] 没有需要更新的 catalog");
            return;
        }

        Debug.Log($"[Addr] 发现 {catalogsToUpdate.Count} 个 catalog 需要更新");
        var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
        var locators = await updateHandle.Task;
        Addressables.Release(updateHandle);
    }

    private async void DownloadLabel(string label, Action<float> onProgress = null)
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync(label);
        var downloadSize = await sizeHandle.Task;
        Addressables.Release(sizeHandle);
        if (downloadSize <= 0)
        {
            Debug.Log($"[Addr] Label {label} 不需要下载（可能已在本地缓存）");
            return;
        }

        Debug.Log($"[Addr] Label {label} 需要下载大小：{downloadSize / 1024f / 1024f:F2} MB");

        var downloadHandle = Addressables.DownloadDependenciesAsync(label, true);
        downloadHandle.Completed += Complete;

        void Complete(AsyncOperationHandle handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                Debug.Log($"[Addr] Label {label} 下载完成");
            else
                Debug.LogError($"[Addr] Label {label} 下载失败：{handle.OperationException}");
        }
    }


    protected override void Clear()
    {
        base.Clear();
    }
}