
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LitJson;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameDataManager : Singleton<GameDataManager> 
{
    public Dictionary<Type, Dictionary<string, IGameData>> allGameStaticDatas = new Dictionary<Type, Dictionary<string, IGameData>>();
    public override void Init()
    {
        base.Init();
    }
    public T GetData<T>(string key) where T : IGameData
    {
        Type type = typeof(T);
        if (allGameStaticDatas.TryGetValue(type, out var dataDic))
        {
            if (dataDic.TryGetValue(key, out var data))
            {
                return (T)data;
            }
        }
        else
        {
            var dataAsset = Resources.Load<TextAsset>(DataPath.GetDataPath(type));
            if (dataAsset != null)
            {
                dataDic = new Dictionary<string, IGameData>();

                try
                {
                    var datas = JsonMapper.ToObject<List<T>>(dataAsset.text);
                    for (int i = 0; i < datas.Count; i++)
                    {
                        var data = datas[i];
                        dataDic.Add(data.GetKey(), data);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
                allGameStaticDatas[type] = dataDic;
            }

        }
        return default(T);
    }

    public async Task<T> GetAsyncObjectData<T>(string key) where T : Object,IGameData
    {
        Type type = typeof(T);
        if (allGameStaticDatas.TryGetValue(type, out var dataDic))
        {
            if (dataDic.TryGetValue(key, out var data))
            {
                return (T)data;
            }
        }
        else
        {
            var dataAsset = await ExtensionsResources.LoadResourceAsync(DataPath.GetDataPath(type));

            if(dataAsset is T)
            {
                dataDic = new Dictionary<string, IGameData>();
                var data=(T)dataAsset;
                dataDic[data.GetKey()] = data;
                allGameStaticDatas[type] = dataDic;
            } 
        }
        return default(T); ;
    }
    public async Task<T> GetAsyncData<T>(string key) where T : IGameData
    {
        Type type = typeof(T); 
        if (allGameStaticDatas.TryGetValue(type,out var dataDic))
        {
            if (dataDic.TryGetValue(key,out var data))
            {
                return (T)data;
            }
        }
        else
        { 
           var dataAsset=await ExtensionsResources.LoadResourceAsync<TextAsset>(DataPath.GetDataPath(type));
             
            if (dataAsset != null)
            {
                dataDic=new Dictionary<string, IGameData>();

                try
                {
                    var datas = JsonMapper.ToObject<List<T>>(dataAsset.text);
                    for(int i = 0;i<datas.Count;i++)
                    {
                        var data = datas[i];
                        dataDic.Add(data.GetKey(), data);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogWarning(e);
                }
                allGameStaticDatas[type] = dataDic;
            }
            
        }
        return default(T); ;
    }
}

public interface IGameData 
{ 
    public string GetKey();
     
}

public struct ShowData : IGameData
{
    public int id;
    public string name;
    public string filmName;
    public string GetKey()
    {
        return id.ToString();
    }
}

[System.Serializable]
public struct LangLanguageSwitch : IGameData
{ 
    public string cn, jp, en, ko;
     

    public  string GetKey()
    {
        return cn;
    }

    public string GetValue(SystemLanguage systemLanguage)
    {
        switch(systemLanguage)
        {
            case SystemLanguage.Chinese:
                return cn;
            case SystemLanguage.Japanese:
                return jp;
            case SystemLanguage.Korean:
                return ko;
            default:
                return en;
        }
    }
}