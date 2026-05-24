using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public static class ExtensionsResources
{
    private static readonly object CacheSyncRoot = new object();
    private static readonly Dictionary<string, Object> ResourceCache = new Dictionary<string, Object>();
    private static readonly Dictionary<string, Object[]> ResourceAllCache = new Dictionary<string, Object[]>();
    private static readonly Dictionary<string, Task<Object>> ResourceLoadTasks = new Dictionary<string, Task<Object>>();

    public static ResourceRequestAwaiter GetAwaiter(this ResourceRequest request) => new ResourceRequestAwaiter(request);

    public static void ClearCache()
    {
        lock (CacheSyncRoot)
        {
            ResourceCache.Clear();
            ResourceAllCache.Clear();
            ResourceLoadTasks.Clear();
        }
    }

    public static string NormalizeResourcePath(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace('\\', '/').Trim().Trim('/');
    }

    public static bool TryGetCachedResource<T>(string path, out T asset) where T : Object
    {
        asset = null;
        string normalizedPath = NormalizeResourcePath(path);
        if (string.IsNullOrEmpty(normalizedPath))
        {
            return false;
        }

        string cacheKey = GetResourceCacheKey(typeof(T), normalizedPath);
        lock (CacheSyncRoot)
        {
            if (ResourceCache.TryGetValue(cacheKey, out var cachedAsset) && cachedAsset is T typedAsset)
            {
                asset = typedAsset;
                return true;
            }
        }

        return false;
    }

    public static async Task<T> PreloadResourceAsync<T>(string path) where T : Object
    {
        return await LoadResourceAsync<T>(path);
    }

    public static T[] PreloadAllResource<T>(string path) where T : Object
    {
        return LoadAllResource<T>(path);
    }

    private static string GetResourceCacheKey(Type type, string path)
    {
        return $"{type.FullName}:{path}";
    }

    private static bool TryPreparePath(string context, Type type, string path, out string normalizedPath)
    {
        normalizedPath = NormalizeResourcePath(path);
        if (!string.IsNullOrEmpty(normalizedPath))
        {
            return true;
        }

        Debug.LogError($"{context} failed: empty path for {type?.FullName ?? "<null>"}");
        return false;
    }

    public static async Task<T> LoadResourceAsync<T>(string path) where T : Object
    {
        if (!TryPreparePath(nameof(LoadResourceAsync), typeof(T), path, out var normalizedPath))
        {
            return null;
        }

        var asset = await LoadResourceObjectAsync(typeof(T), typeof(T), normalizedPath);
        return asset as T;
    }

    public static T LoadResource<T>(string path) where T : Object
    {
        if (!TryPreparePath(nameof(LoadResource), typeof(T), path, out var normalizedPath))
        {
            return null;
        }

        string cacheKey = GetResourceCacheKey(typeof(T), normalizedPath);
        lock (CacheSyncRoot)
        {
            if (ResourceCache.TryGetValue(cacheKey, out var cachedAsset))
            {
                return cachedAsset as T;
            }
        }

        var asset = Resources.Load<T>(normalizedPath);
        CacheResource(cacheKey, asset);
        return asset;
    }

    public static T LoadIGameData<T>(string path) where T : IGameData
    {
        if (!TryPreparePath(nameof(LoadIGameData), typeof(T), path, out var normalizedPath))
        {
            return default(T);
        }

        string cacheKey = GetResourceCacheKey(typeof(T), normalizedPath);
        Object asset;
        lock (CacheSyncRoot)
        {
            ResourceCache.TryGetValue(cacheKey, out asset);
        }

        if (asset == null)
        {
            asset = Resources.Load(normalizedPath);
            CacheResource(cacheKey, asset);
        }

        if (asset is T gameData)
        {
            return gameData;
        }

        if (asset != null)
        {
            Debug.LogError($"LoadIGameData failed: incompatible asset. type={typeof(T).FullName}, path={normalizedPath}, asset={asset.name}");
        }
        return default(T);
    }

    public static async Task<T> LoadResourceIGameData<T>(string path) where T : IGameData
    {
        if (!TryPreparePath(nameof(LoadResourceIGameData), typeof(T), path, out var normalizedPath))
        {
            return default(T);
        }

        var asset = await LoadResourceObjectAsync(typeof(T), null, normalizedPath);
        if (asset is T gameData)
        {
            return gameData;
        }

        if (asset != null)
        {
            Debug.LogError($"LoadResourceIGameData failed: incompatible asset. type={typeof(T).FullName}, path={normalizedPath}, asset={asset.name}");
        }
        return default(T);
    }

    public static List<T> LoadAllIGameData<T>(string path) where T : IGameData
    {
        if (!TryPreparePath(nameof(LoadAllIGameData), typeof(T), path, out var normalizedPath))
        {
            return new List<T>();
        }

        string cacheKey = GetResourceCacheKey(typeof(T), normalizedPath);
        Object[] loadedAssets;
        lock (CacheSyncRoot)
        {
            ResourceAllCache.TryGetValue(cacheKey, out loadedAssets);
        }

        if (loadedAssets == null)
        {
            // LoadAll 结果集中缓存，展开时返回新的 List，避免调用方误改共享集合。
            loadedAssets = Resources.LoadAll(normalizedPath);
            CacheAllResource(cacheKey, loadedAssets);
        }

        List<T> results = new List<T>();
        try
        {
            for (int i = 0; i < loadedAssets.Length; i++)
            {
                if (loadedAssets[i] is T data)
                {
                    results.Add(data);
                }
                else if (loadedAssets[i] is IDataArray<T> dataArray)
                {
                    var dataList = dataArray.DataList;
                    if (dataList == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < dataList.Length; j++)
                    {
                        results.Add(dataList[j]);
                    }
                }
                else if (loadedAssets[i] != null)
                {
                    Debug.LogError($"LoadAllIGameData skipped incompatible asset. type={typeof(T).FullName}, path={normalizedPath}, asset={loadedAssets[i].name}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LoadAllIGameData failed: type={typeof(T).FullName}, path={normalizedPath}, error={e}");
        }

        return results;
    }

    public static T[] LoadAllResource<T>(string path) where T : Object
    {
        if (!TryPreparePath(nameof(LoadAllResource), typeof(T), path, out var normalizedPath))
        {
            return Array.Empty<T>();
        }

        string cacheKey = GetResourceCacheKey(typeof(T), normalizedPath);
        lock (CacheSyncRoot)
        {
            if (ResourceAllCache.TryGetValue(cacheKey, out var cachedAssets) && cachedAssets is T[] typedCachedAssets)
            {
                return typedCachedAssets;
            }
        }

        var assets = Resources.LoadAll<T>(normalizedPath);
        CacheAllResource(cacheKey, assets);
        return assets;
    }

    public static async Task<Object> LoadResourceAsync(string path)
    {
        if (!TryPreparePath(nameof(LoadResourceAsync), typeof(Object), path, out var normalizedPath))
        {
            return null;
        }

        return await LoadResourceObjectAsync(typeof(Object), null, normalizedPath);
    }

    public static async Task<Object> LoadResourceAsync(Type type, string path)
    {
        if (type == null || !TryPreparePath(nameof(LoadResourceAsync), type, path, out var normalizedPath))
        {
            Debug.LogError($"LoadResourceAsync failed: type={type}, path={path}");
            return null;
        }

        Type loadType = typeof(Object).IsAssignableFrom(type) ? type : null;
        return await LoadResourceObjectAsync(type, loadType, normalizedPath);
    }

    private static async Task<Object> LoadResourceObjectAsync(Type cacheType, Type loadType, string normalizedPath)
    {
        string cacheKey = GetResourceCacheKey(cacheType, normalizedPath);
        Task<Object> loadTask;
        lock (CacheSyncRoot)
        {
            if (ResourceCache.TryGetValue(cacheKey, out var cachedAsset))
            {
                return cachedAsset;
            }

            if (!ResourceLoadTasks.TryGetValue(cacheKey, out loadTask))
            {
                // 同一路径同类型的异步加载只发起一次 Resources 请求，后续调用等待同一个任务。
                loadTask = LoadResourceObjectInternalAsync(cacheKey, loadType, normalizedPath);
                ResourceLoadTasks[cacheKey] = loadTask;
            }
        }

        try
        {
            return await loadTask;
        }
        finally
        {
            lock (CacheSyncRoot)
            {
                if (ResourceLoadTasks.TryGetValue(cacheKey, out var currentTask) && currentTask == loadTask)
                {
                    ResourceLoadTasks.Remove(cacheKey);
                }
            }
        }
    }

    private static async Task<Object> LoadResourceObjectInternalAsync(string cacheKey, Type loadType, string normalizedPath)
    {
        ResourceRequest request = loadType == null
            ? Resources.LoadAsync(normalizedPath)
            : Resources.LoadAsync(normalizedPath, loadType);
        await request;
        CacheResource(cacheKey, request.asset);
        return request.asset;
    }

    private static void CacheResource(string cacheKey, Object asset)
    {
        if (asset == null)
        {
            return;
        }

        lock (CacheSyncRoot)
        {
            ResourceCache[cacheKey] = asset;
        }
    }

    private static void CacheAllResource(string cacheKey, Object[] assets)
    {
        lock (CacheSyncRoot)
        {
            ResourceAllCache[cacheKey] = assets ?? Array.Empty<Object>();
        }
    }

    public static async Task<UnityEngine.Object[]> LoadAsyncBundle(string url)
    {
        string path = Path.Combine(Application.streamingAssetsPath, url);
        var uri = new System.Uri(path);
        var getRequest = UnityWebRequestAssetBundle.GetAssetBundle(uri.AbsoluteUri);
        await getRequest.SendWebRequest();

        AssetBundle assetBundle = (getRequest.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
        if (assetBundle == null)
        {
            Debug.LogError($"LoadAsyncBundle failed: {uri.AbsoluteUri}");
            return Array.Empty<Object>();
        }

        var loadRequest = assetBundle.LoadAllAssetsAsync();
        await loadRequest;
        return loadRequest.allAssets;
    }
}

public class ResourceRequestAwaiter : INotifyCompletion
{
    public Action Continuation;
    public ResourceRequest resourceRequest;
    public bool IsCompleted => resourceRequest.isDone;

    public ResourceRequestAwaiter(ResourceRequest resourceRequest)
    {
        this.resourceRequest = resourceRequest;
        this.resourceRequest.completed += Accomplish;
    }
    public void OnCompleted(Action continuation) => this.Continuation = continuation;
    public void Accomplish(AsyncOperation asyncOperation) => Continuation?.Invoke();
    public void GetResult() { }
}

public static class ExtensionMethods
{
    public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<object>();
        asyncOp.completed += obj => { tcs.SetResult(null); };
        return ((Task)tcs.Task).GetAwaiter();
    }
}
