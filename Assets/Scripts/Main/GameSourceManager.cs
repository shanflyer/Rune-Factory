using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using BehaviorDesigner;
using BehaviorDesigner.Runtime;

public class GameSourceManager:Singleton<GameSourceManager>
{
    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    private Dictionary<string, ScriptableObject> scriptableObjects = new Dictionary<string, ScriptableObject>();
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

    private Dictionary<string, ExternalBehavior> behaviors = new Dictionary<string, ExternalBehavior>();

    public SpriteRenderer dropItem;
    public async override void Init()
    {
        //º”‘ÿµÙ¬‰‘§÷∆ÃÂ
        var dropItemObj = await GameSourceManager.instance.GetPrefab(DataPath.DropItemPrefabPath);
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
        if(sprites.TryGetValue(path,out Sprite sprite))
        {
            return sprite;
        }
        sprite = await ExtensionsResources.LoadResourceAsync<Sprite>(path);
        if (sprite == null)
        {
            var texture = await GetTexture(path);
            if (texture)
            {
                sprite = Sprite.Create(texture,new Rect(0,0,texture.width,texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        sprites.Add(path, sprite);
        return sprite;
    }
    public async Task<AudioClip> GetAudioClip(string path)
    {
        if(audioClips.TryGetValue(path,out AudioClip audioClip))
        {
            return audioClip;
        }
        audioClip= await ExtensionsResources.LoadResourceAsync<AudioClip>(path);
        audioClips.Add(path, audioClip);
        return audioClip;
    }
    public async Task<Texture2D> GetTexture(string path)
    {
        if(textures.TryGetValue(path,out Texture2D texture))
        {
            return texture;
        }
        texture = await ExtensionsResources.LoadResourceAsync<Texture2D>(path);
        textures.Add(path, texture);
        return texture;
    }
    public async Task<GameObject> GetPrefab(string path)
    {
        if(prefabs.TryGetValue(path,out GameObject obj))
        {
            return obj;
        }
        obj =await ExtensionsResources.LoadResourceAsync<GameObject>(path);
        prefabs.Add(path, obj);
        return obj;
    }

    public async Task<T> GetScriptableObject<T>(string path) where T : ScriptableObject
    {
        if(scriptableObjects.TryGetValue(path,out ScriptableObject scriptableObject))
        {
            return scriptableObject as T;
        }
        else
        {
            scriptableObject= await ExtensionsResources.LoadResourceAsync<T>(path);
            scriptableObjects.Add(path, scriptableObject);
            return scriptableObject as T;
        }
    }
    public async Task<T> GetSingleScriptableObject<T>(string path) where T: ScriptableObject
    {
        return await ExtensionsResources.LoadResourceAsync<T>(path);
    }


}
