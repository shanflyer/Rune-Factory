using System.Collections.Generic;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class GameSourceManager:Singleton<GameSourceManager>
{  
    private Dictionary<string, ScriptableObject> scriptableObjects = new Dictionary<string, ScriptableObject>();
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

    private Dictionary<string, ExternalBehavior> behaviors = new Dictionary<string, ExternalBehavior>();

    public SpriteRenderer dropItem;
    protected override void Clear()
    {
        base.Clear();  
        scriptableObjects.Clear();
        audioClips.Clear();
        behaviors.Clear();
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
        if (behaviors.TryGetValue(path, out ExternalBehavior behavior))
        {
            return behavior;
        }
        behavior = await ExtensionsResources.LoadResourceAsync<ExternalBehavior>(path);
        
        behaviors[path]= behavior;
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
        if(audioClips.TryGetValue(path,out AudioClip audioClip))
        {
            return audioClip;
        }
        audioClip= await ExtensionsResources.LoadResourceAsync<AudioClip>(path);
        audioClips[path]= audioClip;
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

    public async Task<T> GetScriptableObject<T>(string path, bool saveTemp = false) where T : ScriptableObject
    {
        if (saveTemp)
        {
            if (scriptableObjects.TryGetValue(path, out var scriptableObject))
            {
                return scriptableObject as T;
            }
            else
            {
                scriptableObject = await ExtensionsResources.LoadResourceAsync<T>(path);
                scriptableObjects.Add(path, scriptableObject);
                return scriptableObject as T;
            }
        }
        else
        {
            var scriptableObject = await ExtensionsResources.LoadResourceAsync<T>(path); 
            return scriptableObject as T;
        } 
    }
    public async Task<T> GetSingleScriptableObject<T>(string path) where T: ScriptableObject
    {
        return await ExtensionsResources.LoadResourceAsync<T>(path);
    }


}
