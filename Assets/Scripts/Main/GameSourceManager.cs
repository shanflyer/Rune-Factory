using System.Threading.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class GameSourceManager:Singleton<GameSourceManager>
{   
    public SpriteRenderer dropItem;
    protected override void Clear()
    {
        base.Clear();   
    }
    public async override void Init()
    {
        //加载掉落预制体
        var dropItemObj = await instance.GetPrefab(DataPath.DropItemPrefabPath);
        dropItem = dropItemObj.GetComponent<SpriteRenderer>();
        base.Init();
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
           var spriteReference= await ExtensionsResources.LoadResourceAsync<SpriteResourceRenference>(path);
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
    
    public async Task<T> GetComponent<T>(string path) where T: Component
    {
        var obj = await ExtensionsResources.LoadResourceAsync<GameObject>(path); 
        return obj.GetComponentInChildren<T>();
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
    public async Task<T> GetSingleScriptableObject<T>(string path) where T: ScriptableObject
    {
        return await ExtensionsResources.LoadResourceAsync<T>(path);
    }


}
