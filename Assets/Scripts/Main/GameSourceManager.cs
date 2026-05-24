using System.Collections.Generic;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class GameSourceManager : Singleton<GameSourceManager>
{
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;

    public SpriteRenderer dropItem;
    private readonly Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, ScriptableObject> scriptableObjectCache = new Dictionary<string, ScriptableObject>();
    private readonly Dictionary<string, ExternalBehavior> behaviorCache = new Dictionary<string, ExternalBehavior>();

    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
        prefabCache.Clear();
        spriteCache.Clear();
        audioClipCache.Clear();
        scriptableObjectCache.Clear();
        behaviorCache.Clear();
        ExtensionsResources.ClearCache();
        base.Clear();
    }

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        var dropItemObj = await GetPrefab(DataPath.DropItemPrefabPath);
        if (dropItemObj == null)
        {
            Debug.LogError($"GameSourceManager init failed: missing prefab at {DataPath.DropItemPrefabPath}");
            return;
        }

        dropItem = dropItemObj.GetComponent<SpriteRenderer>();
        if (dropItem == null)
        {
            Debug.LogError($"GameSourceManager init failed: prefab has no SpriteRenderer at {DataPath.DropItemPrefabPath}");
        }
    }

    public async Task<ExternalBehavior> GetBehavior(string path)
    {
        if (behaviorCache.TryGetValue(path, out var behavior))
        {
            return behavior;
        }

        behavior = await ExtensionsResources.LoadResourceAsync<ExternalBehavior>(path);
        CacheLoadedAsset(behaviorCache, path, behavior);
        return behavior;
    }

    public async Task<Sprite> GetSprite(string path)
    {
        if (spriteCache.TryGetValue(path, out var sprite))
        {
            return sprite;
        }

        sprite = await ExtensionsResources.LoadResourceAsync<Sprite>(path);
        if (sprite == null)
        {
            var spriteReference = await ExtensionsResources.LoadResourceAsync<SpriteResourceRenference>(path);
            if (spriteReference != null)
            {
                sprite = spriteReference.sprite;
            }
        }

        CacheLoadedAsset(spriteCache, path, sprite);
        return sprite;
    }

    public async Task<AudioClip> GetAudioClip(string path)
    {
        if (audioClipCache.TryGetValue(path, out var audioClip))
        {
            return audioClip;
        }

        audioClip = await ExtensionsResources.LoadResourceAsync<AudioClip>(path);
        CacheLoadedAsset(audioClipCache, path, audioClip);
        return audioClip;
    }

    public async Task<T> GetComponent<T>(string path) where T : Component
    {
        var obj = await GetPrefab(path);
        return obj != null ? obj.GetComponentInChildren<T>() : null;
    }

    public T GetComponentImmediately<T>(string path) where T : Component
    {
        var obj = GetPrefabImmediately(path);
        return obj != null ? obj.GetComponentInChildren<T>() : null;
    }

    public GameObject GetPrefabImmediately(string path)
    {
        if (prefabCache.TryGetValue(path, out var obj))
        {
            return obj;
        }

        obj = ExtensionsResources.LoadResource<GameObject>(path);
        CacheLoadedAsset(prefabCache, path, obj);
        return obj;
    }

    public async Task<GameObject> GetPrefab(string path)
    {
        if (prefabCache.TryGetValue(path, out var obj))
        {
            return obj;
        }

        obj = await ExtensionsResources.LoadResourceAsync<GameObject>(path);
        CacheLoadedAsset(prefabCache, path, obj);
        return obj;
    }

    public async Task<T> GetScriptableObject<T>(string path) where T : ScriptableObject
    {
        if (scriptableObjectCache.TryGetValue(path, out var cached) && cached is T typedCached)
        {
            return typedCached;
        }

        var scriptableObject = await ExtensionsResources.LoadResourceAsync<T>(path);
        CacheLoadedAsset(scriptableObjectCache, path, scriptableObject);
        return scriptableObject;
    }

    public async Task<T> GetSingleScriptableObject<T>(string path) where T : ScriptableObject
    {
        return await GetScriptableObject<T>(path);
    }

    private static void CacheLoadedAsset<T>(Dictionary<string, T> cache, string path, T asset) where T : UnityEngine.Object
    {
        if (!string.IsNullOrEmpty(path) && asset != null)
        {
            // Resources 资源本身由 Unity 管理生命周期，这里只缓存引用，减少重复查找和异步请求。
            cache[path] = asset;
        }
    }
}
