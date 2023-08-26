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
    public static ResourceRequestAwaiter GetAwaiter(this ResourceRequest request) => new ResourceRequestAwaiter(request);
    public static async Task<T> LoadResourceAsync<T>(string path)where T : UnityEngine.Object
    {
        var gres = Resources.LoadAsync(path);
        await gres;
        return gres.asset as T;
    }
    public static async Task<T> LoadResourceIGameData<T>(string path) where T : IGameData
    {
        var gres = Resources.LoadAsync(path);
        await gres;
        if(gres.asset != null)
        {
            return (T)(IGameData)gres.asset;
        }
        return default(T);
    }
    public static List<T> LoadAllIGameData<T>(string path) where T: IGameData
    {
        var gres = Resources.LoadAll(path);
        List<T> ts = new List<T>();
        try
        {
            for (int i = 0; i < gres.Length; i++)
            {
                var t= (T)((IGameData)gres[i]);
                if (t != null)
                {
                    ts.Add(t);
                }
            }
        }
        catch
        {

        }
       
        return ts;
    }
    
    public static T[] LoadAllResource<T>(string path) where T : UnityEngine.Object
    {
        var gres = Resources.LoadAll<T>(path); 
        return gres;
    }
    public static async Task<Object> LoadResourceAsync(string path)
    {
        var gres = Resources.LoadAsync(path);
        await gres;
        return gres.asset;
    }
    public static async Task<Object> LoadResourceAsync(Type type,string path)
    {
        var gres = Resources.LoadAsync(path,type);
        await gres;
        return gres.asset;
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