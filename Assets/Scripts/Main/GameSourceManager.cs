using System.Threading.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class GameSourceManager : Singleton<GameSourceManager>
{
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;

    public SpriteRenderer dropItem;

    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
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
        var behavior = await ExtensionsResources.LoadResourceAsync<ExternalBehavior>(path);
        return behavior;
    }

    public async Task<Sprite> GetSprite(string path)
    {
        var sprite = await ExtensionsResources.LoadResourceAsync<Sprite>(path);
        if (sprite == null)
        {
            var spriteReference = await ExtensionsResources.LoadResourceAsync<SpriteResourceRenference>(path);
            if (spriteReference != null)
            {
                sprite = spriteReference.sprite;
            }
        }

        return sprite;
    }

    public async Task<AudioClip> GetAudioClip(string path)
    {
        var audioClip = await ExtensionsResources.LoadResourceAsync<AudioClip>(path);
        return audioClip;
    }

    public async Task<T> GetComponent<T>(string path) where T : Component
    {
        var obj = await ExtensionsResources.LoadResourceAsync<GameObject>(path);
        return obj != null ? obj.GetComponentInChildren<T>() : null;
    }

    public GameObject GetPrefabImmediately(string path)
    {
        var obj = Resources.Load<GameObject>(path);
        return obj;
    }

    public async Task<GameObject> GetPrefab(string path)
    {
        var obj = await ExtensionsResources.LoadResourceAsync<GameObject>(path);
        return obj;
    }

    public async Task<T> GetScriptableObject<T>(string path) where T : ScriptableObject
    {
        var scriptableObject = await ExtensionsResources.LoadResourceAsync<T>(path);
        return scriptableObject;
    }

    public async Task<T> GetSingleScriptableObject<T>(string path) where T : ScriptableObject
    {
        return await ExtensionsResources.LoadResourceAsync<T>(path);
    }
}
