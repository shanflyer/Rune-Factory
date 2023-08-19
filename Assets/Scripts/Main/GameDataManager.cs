
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LitJson;
using Mono.Cecil;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameDataManager : Singleton<GameDataManager> 
{
    private string userName = "User";
    private UserGameSaveData userGameSaveData;
    public UserGameSaveData UserGameSaveData
    {
        get => userGameSaveData;
    }
    private async void InitUserSaveData()
    {
        userGameSaveData = await LoadUserGameSaveData(userName);
    }
    public async Task<UserGameSaveData> LoadUserGameSaveData(string userName)
    {
        string saveDataPath = $"{DataPath.gameSaveDataPath}{"/"}{userName}";
        if (File.Exists(saveDataPath))
        {
            string dataStr = await File.ReadAllTextAsync(saveDataPath);
            UserGameSaveData userGameSaveData = JsonMapper.ToObject<UserGameSaveData>(dataStr);
            return userGameSaveData;
        }
        else
        {
            return new UserGameSaveData();
        }
    }

    public void InitPlayerData(string playerName,Gender gender, Season season,int day,int year=1300)
    { 
        userGameSaveData.playerData.name = playerName;
        userGameSaveData.playerData.gender = gender;
        userGameSaveData.playerData.brithDay = new BrithDay
        {
            year = year,
            season = season,
            day = day
        };

    }

    void SaveUserGameSaveData()
    {
        userGameSaveData.packageSaveDatas = PackageManager.instance.GetPackageSaveData();
        string strs = JsonMapper.ToJson(userGameSaveData);
        string saveDataPath = $"{DataPath.gameSaveDataPath}{"/"}{userName}";

        File.WriteAllText(saveDataPath, strs);
    }
    public Dictionary<Type, Dictionary<string, IGameData>> allGameStaticDatas = new Dictionary<Type, Dictionary<string, IGameData>>();
    public override async void Init()
    {
        base.Init();
        InitUserSaveData();
        //初始加载
        LoadAllAsyncObjectData<GameActionData>();
        await LoadAllAsyncData<GrowModelData>();
        LoadAllAsyncObjectData<ProfessionData>();

        await LoadAllAsyncData<FunctionData>();
    }
    void LoadAllAsyncObjectData<T>() where T :Object,IGameData
    {
        Type type = typeof(T);
        if (!allGameStaticDatas.TryGetValue(type, out var dataDic))
        {
            var _results = ExtensionsResources.LoadAllResource<T>(DataPath.GetDataPath(type));
            if (_results != null)
            {
                dataDic = new Dictionary<string, IGameData>();
                var results = _results.ToList();
                for (int i = 0; i < results.Count; i++)
                {
                    var data = results[i];
                    data.Init();
                    dataDic.Add(data.GetKey(), data);
                }
                allGameStaticDatas[type] = dataDic;
            }

        }
    }
    async Task LoadAllAsyncData<T>() where T:IGameData
    {
        Type type = typeof(T);
        if (!allGameStaticDatas.TryGetValue(type, out var dataDic))
        { 
            var dataAsset = await ExtensionsResources.LoadResourceAsync(DataPath.GetDataPath(type));

            if (dataAsset != null && dataAsset is IDataArray<T> dataArray)
            {
                dataDic = new Dictionary<string, IGameData>();
                try
                {
                   var  results = dataArray.DataList;
                    for (int i = 0; i < results.Count; i++)
                    {
                        var data = results[i];
                        data.Init();
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
                        data.Init();
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

    public async Task<List<T>> GetAllAsyncData<T>()where T : IGameData
    {
        List<T> results = new List<T>();
        Type type = typeof(T);
        if (allGameStaticDatas.TryGetValue(type, out var dataDic))
        {
            using(var e = dataDic.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    var data = e.Current.Value;
                    results.Add((T)data);
                }
            }
        }
        else
        {
            var dataAsset = await ExtensionsResources.LoadResourceAsync<TextAsset>(DataPath.GetDataPath(type));

            if (dataAsset != null)
            {
                dataDic = new Dictionary<string, IGameData>();

                try
                {
                    results = JsonMapper.ToObject<List<T>>(dataAsset.text);
                    for (int i = 0; i < results.Count; i++)
                    {
                        var data = results[i];
                        data.Init();
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
        return results ;
    }

    public async Task<List<T>> GetAllAsyncObjectDataArray<T>() where T : IGameData
    {
        List<T> results = new List<T>();
        Type type = typeof(T);
        if (allGameStaticDatas.TryGetValue(type, out var dataDic))
        {
            using (var e = dataDic.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    var data = e.Current.Value;
                    results.Add((T)data);
                }
            }
        }
        else
        {
            var dataAsset = await ExtensionsResources.LoadResourceAsync(DataPath.GetDataPath(type));

            if (dataAsset != null && dataAsset is IDataArray<T> dataArray)
            {
                dataDic = new Dictionary<string, IGameData>();
                try
                {
                    results = dataArray.DataList;
                    for (int i = 0; i < results.Count; i++)
                    {
                        var data = results[i];
                        data.Init();
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
        return results;
    }
    public List<T> GetAllAsyncObjectData<T>() where T : Object, IGameData
    {
        List<T> results = new List<T>();
        Type type = typeof(T);
        if (allGameStaticDatas.TryGetValue(type, out var dataDic))
        {
            using (var e = dataDic.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    var data = e.Current.Value;
                    results.Add((T)data);
                }
            }
        }
        else
        {
            var _results = ExtensionsResources.LoadAllResource<T>(DataPath.GetDataPath(type));
            if (_results != null)
            {
                dataDic = new Dictionary<string, IGameData>();
                results = _results.ToList();
                for (int i = 0; i < results.Count; i++)
                {
                    var data = results[i];
                    data.Init();
                    dataDic.Add(data.GetKey(), data);
                }
                allGameStaticDatas[type] = dataDic;
            }

        }
        return results;
    } 
    public async Task<T> GetAsyncObjectData<T>(int key) where T : Object, IGameData
    {
        return await GetAsyncObjectData<T>(key.ToString());
    }
    public async Task<T> GetAsyncObjectData<T>(string key="") where T : Object,IGameData
    {
        Type type = typeof(T);
        if (allGameStaticDatas.TryGetValue(type, out var dataDic))
        {
            if (dataDic.TryGetValue(key, out var data))
            {
                return (T)data;
            }
            else
            {
                string dataPath = $"{DataPath.GetDataPath(type)}{key}";
                data = await ExtensionsResources.LoadResourceAsync<T>(dataPath);
                if (data != null)
                {
                    data.Init();
                    dataDic[data.GetKey()] = data;
                    allGameStaticDatas[type] = dataDic;
                    return (T)data;
                }
            }
        }
        else
        {
            string dataPath = $"{DataPath.GetDataPath(type)}{key}";
            var data = await ExtensionsResources.LoadResourceAsync<T>(dataPath);
            if (data!= null)
            {
                data.Init();
                dataDic = new Dictionary<string, IGameData>();
                dataDic[data.GetKey()] = data;
                allGameStaticDatas[type] = dataDic;
                return data;
            }
        }
        return default(T); ;
    }

    public async Task<T> GetAsyncObjectDataArray<T>(string key = "") where T : IGameData
    {
        Type type = typeof(T);
        var dataAsset = await ExtensionsResources.LoadResourceAsync(DataPath.GetDataPath(type));
        if (dataAsset != null && dataAsset is IDataArray<T> dataArray)
        {
           var dataDic = new Dictionary<string, IGameData>();
            for (int i = 0; i < dataArray.DataList.Count; i++)
            {
                var item = dataArray.DataList[i];
                item.Init();
                dataDic[dataArray.DataList[i].GetKey()] = item;
            }
            allGameStaticDatas[type] = dataDic;
            if (dataDic.TryGetValue(key, out var data1))
            {
                return (T)data1;
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
                        data.Init();
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
    public async void Init() { }
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