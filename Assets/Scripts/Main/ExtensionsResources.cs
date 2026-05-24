using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public  static class ExtensionsResources
{
    private static readonly Dictionary<string, Object> ResourceCache = new Dictionary<string, Object>();
    private static readonly Dictionary<string, Object[]> ResourceAllCache = new Dictionary<string, Object[]>();

    public static ResourceRequestAwaiter GetAwaiter(this ResourceRequest request) => new ResourceRequestAwaiter(request);

    public static void ClearCache()
    {
        ResourceCache.Clear();
        ResourceAllCache.Clear();
    }

    private static string GetResourceCacheKey(Type type, string path)
    {
        return $"{type.FullName}:{path}";
    }

    public static async Task<T> LoadResourceAsync<T>(string path)where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"LoadResourceAsync failed: empty path for {typeof(T).FullName}");
            return null;
        }

        var cacheKey = GetResourceCacheKey(typeof(T), path);
        if (ResourceCache.TryGetValue(cacheKey, out var cachedAsset))
        {
            return cachedAsset as T;
        }

        var gres = Resources.LoadAsync(path, typeof(T));
        await gres;
        var asset = gres.asset as T;
        CacheResource(cacheKey, asset);
        return asset;
    }
    public static T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"LoadResource failed: empty path for {typeof(T).FullName}");
            return null;
        }

        var cacheKey = GetResourceCacheKey(typeof(T), path);
        if (ResourceCache.TryGetValue(cacheKey, out var cachedAsset))
        {
            return cachedAsset as T;
        }

        var asset = Resources.Load<T>(path);
        CacheResource(cacheKey, asset);
        return asset;
    }
    public static T LoadIGameData<T>(string path) where T : IGameData
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"LoadIGameData failed: empty path for {typeof(T).FullName}");
            return default(T);
        }

        var cacheKey = GetResourceCacheKey(typeof(T), path);
        if (!ResourceCache.TryGetValue(cacheKey, out var asset))
        {
            asset = Resources.Load(path);
            CacheResource(cacheKey, asset);
        }

        if (asset is T gameData)
        {
            return gameData;
        }

        if (asset != null)
        {
            Debug.LogError($"LoadIGameData failed: incompatible asset. type={typeof(T).FullName}, path={path}, asset={asset.name}");
        }
        return default(T);
    }
    public static async Task<T> LoadResourceIGameData<T>(string path) where T : IGameData
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"LoadResourceIGameData failed: empty path for {typeof(T).FullName}");
            return default(T);
        }

        var cacheKey = GetResourceCacheKey(typeof(T), path);
        if (ResourceCache.TryGetValue(cacheKey, out var cachedAsset))
        {
            return cachedAsset is T cachedGameData ? cachedGameData : default(T);
        }

        var gres = Resources.LoadAsync(path);
        await gres;
        CacheResource(cacheKey, gres.asset);
        if(gres.asset is T gameData)
        {
            return gameData;
        }
        if (gres.asset != null)
        {
            Debug.LogError($"LoadResourceIGameData failed: incompatible asset. type={typeof(T).FullName}, path={path}, asset={gres.asset.name}");
        }
        return default(T);
    }

    public static List<T> LoadAllIGameData<T>(string path) where T: IGameData
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"LoadAllIGameData failed: empty path for {typeof(T).FullName}");
            return new List<T>();
        }

        var cacheKey = GetResourceCacheKey(typeof(T), path);
        if (!ResourceAllCache.TryGetValue(cacheKey, out var gres))
        {
            // LoadAll 的结果统一缓存，数据展开仍每次返回新 List，避免调用方误改共享集合。
            gres = Resources.LoadAll(path);
            ResourceAllCache[cacheKey] = gres;
        }
        List<T> ts = new List<T>();
        try
        {
            for (int i = 0; i < gres.Length; i++)
            {
                if (gres[i] is T t)
                {
                    ts.Add(t);
                }
                else if (gres[i] is IDataArray<T> dataArray)
                {
                    var dataList = dataArray.DataList;
                    if (dataList == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < dataList.Length; j++)
                    {
                        ts.Add(dataList[j]);
                    }
                }
                else if (gres[i] != null)
                {
                    Debug.LogError($"LoadAllIGameData skipped incompatible asset. type={typeof(T).FullName}, path={path}, asset={gres[i].name}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LoadAllIGameData failed: type={typeof(T).FullName}, path={path}, error={e}");
        }

        return ts;
    }


    public static T[] LoadAllResource<T>(string path) where T : UnityEngine.Object
    {
        var cacheKey = GetResourceCacheKey(typeof(T), path);
        if (ResourceAllCache.TryGetValue(cacheKey, out var cachedAssets) && cachedAssets is T[] typedCachedAssets)
        {
            return typedCachedAssets;
        }

        var assets = Resources.LoadAll<T>(path);
        ResourceAllCache[cacheKey] = assets;
        return assets;
    }
    public static async Task<Object> LoadResourceAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("LoadResourceAsync failed: empty path");
            return null;
        }

        var cacheKey = GetResourceCacheKey(typeof(Object), path);
        if (ResourceCache.TryGetValue(cacheKey, out var cachedAsset))
        {
            return cachedAsset;
        }

        var gres = Resources.LoadAsync(path);
        await gres;
        CacheResource(cacheKey, gres.asset);
        return gres.asset;
    }
    public static async Task<Object> LoadResourceAsync(Type type,string path)
    {
        if (type == null || string.IsNullOrEmpty(path))
        {
            Debug.LogError($"LoadResourceAsync failed: type={type}, path={path}");
            return null;
        }

        var cacheKey = GetResourceCacheKey(type, path);
        if (ResourceCache.TryGetValue(cacheKey, out var cachedAsset))
        {
            return cachedAsset;
        }

        var gres = Resources.LoadAsync(path,type);
        await gres;
        CacheResource(cacheKey, gres.asset);
        return gres.asset;
    }

    private static void CacheResource(string cacheKey, Object asset)
    {
        if (asset != null)
        {
            ResourceCache[cacheKey] = asset;
        }
    }

    public static async Task<UnityEngine.Object[]> LoadAsyncBundle(string url)
    {

        string path = Path.Combine(Application.streamingAssetsPath, url);

        var uri = new System.Uri(path);

        var getRequest = UnityWebRequestAssetBundle.GetAssetBundle(uri.AbsoluteUri);
        await getRequest.SendWebRequest();

        AssetBundle ab = (getRequest.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
        var ddd = ab.LoadAllAssetsAsync();



        return ddd.allAssets;
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
