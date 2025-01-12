using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameDataManager : Singleton<GameDataManager>
{
    public Dictionary<Type, Dictionary<string, IGameData>> allGameStaticDatas = new Dictionary<Type, Dictionary<string, IGameData>>();
    
    public GameGlobalData GlobalData { get; private set; }
    protected override void Clear()
    {
        allGameStaticDatas.Clear();
        base.Clear();
    }
    public override async void Init()
    {
        base.Init();
        GlobalData=await GetAsyncData<GameGlobalData>();
        var gameDataSaveManager = GameDataSaveManager.instance;
        //初始加载
        await LoadAllAsyncData<GameActionData>();
        await LoadAllAsyncData<GrowModelData>();
        await LoadAllAsyncData<ProfessionData>();
        await LoadAllAsyncData<FunctionData>();
    }

    private async Task LoadAllAsyncData<T>() where T : IGameData
    {
        Type type = typeof(T);
        var _results = ExtensionsResources.LoadAllIGameData<T>(DataPath.GetDataPath(type));
        if (_results != null && _results.Count != 0)
        {
            var dataDic = new Dictionary<string, IGameData>();
            for (int i = 0; i < _results.Count; i++)
            {
                var data = _results[i];
                data.Init();
                dataDic.Add(data.GetKey(), data);
            }
            allGameStaticDatas[type] = dataDic;

            return;
        }

        var dataAsset = await ExtensionsResources.LoadResourceAsync(DataPath.GetDataPath(type));
        if (dataAsset != null && dataAsset is IDataArray<T> dataArray)
        {
            var dataDic = new Dictionary<string, IGameData>();
            try
            {
                var results = dataArray.DataList;
                for (int i = 0; i < results.Length; i++)
                {
                    var data = results[i];
                    data.Init();
                    dataDic.Add(data.GetKey(), data);
                }
                allGameStaticDatas[type] = dataDic;
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
        if (dataAsset != null && dataAsset is TextAsset)
        {
            var textAsset = dataAsset as TextAsset;
            var dataDic = new Dictionary<string, IGameData>();

            try
            {
                var results = JsonConvert.DeserializeObject<List<T>>(textAsset.text);
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

    public async Task<List<T>> GetAllAsyncData<T>() where T : IGameData
    {
        List<T> results = new List<T>(); ;
        Type type = typeof(T);
        /*if (allGameStaticDatas.TryGetValue(type, out var dataDic))
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
        else*/
        var dataDic = new Dictionary<string, IGameData>();
        {
            var _results = ExtensionsResources.LoadAllIGameData<T>(DataPath.GetDataPath(type));
            if (_results != null && _results.Count != 0)
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
        if (results.Count == 0)
        {
            var dataAsset = await ExtensionsResources.LoadResourceAsync(DataPath.GetDataPath(type));
            if (dataAsset != null && dataAsset is IDataArray<T> dataArray)
            {
                dataDic = new Dictionary<string, IGameData>();
                try
                {
                    results = dataArray.DataList.ToList();
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
        if (results.Count == 0)
        {
            var dataAsset = await ExtensionsResources.LoadResourceAsync<TextAsset>(DataPath.GetDataPath(type));

            if (dataAsset != null)
            {
                dataDic = new Dictionary<string, IGameData>();

                try
                {
                    results = JsonConvert.DeserializeObject<List<T>>(dataAsset.text);
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
                    var datas = JsonConvert.DeserializeObject<List<T>>(dataAsset.text);
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

    public async Task<T> GetAsyncData<T>(int key) where T : IGameData
    {
        return await GetAsyncData<T>(key.ToString());
    }

    public async Task<T> GetAsyncData<T>(string key = "") where T : IGameData
    {
        Type type = typeof(T);
        string dataPath = $"{DataPath.GetDataPath(type)}/{key}";
        if (allGameStaticDatas.TryGetValue(type, out var dataDic))
        {
            if (dataDic.TryGetValue(key, out var data))
            {
                return (T)data;
            }
            else
            {
                data = await ExtensionsResources.LoadResourceIGameData<T>(dataPath);
                if (data != null)
                {
                    data.Init();
                    dataDic[data.GetKey()] = data;
                    allGameStaticDatas[type] = dataDic;
                    return (T)data;
                }
            }
        }

        var _data = await ExtensionsResources.LoadResourceIGameData<T>(dataPath);
        if (_data != null && _data.GetKey() == key)
        {
            _data.Init();
            if (!string.IsNullOrEmpty(_data.GetKey()))
            {
                dataDic = new Dictionary<string, IGameData>();
                dataDic[_data.GetKey()] = _data;
                allGameStaticDatas[type] = dataDic;
            }

            return _data;
        }

        var dataAsset = await ExtensionsResources.LoadResourceAsync(DataPath.GetDataPath(type));
        if (dataAsset != null)
        {
            if(dataAsset is IDataArray<T> dataArray)
            {
                dataDic = new Dictionary<string, IGameData>();
                for (int i = 0; i < dataArray.DataList.Length; i++)
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
            else if(dataAsset is T t)
            {
                t.Init();
                dataDic = new Dictionary<string, IGameData>();
                dataDic[key] = t;
                allGameStaticDatas[type] = dataDic;
                return t;
            } 
        }

        var textAsset = dataAsset as TextAsset;
        if (textAsset != null)
        {
            dataDic = new Dictionary<string, IGameData>();
            try
            {
                var datas = JsonConvert.DeserializeObject<List<T>>(textAsset.text);
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
        return default(T); ;
    }
}

public interface IGameData
{
    public string GetKey();
    public void SetKey(string key) { }
    public string ToString()
    {
       return GetKey();
    }
    public void SetObjList(List<object> list) { }
    public string GetName() { return ToString(); }
    public bool isSingleGroup() { return false; }

#if UNITY_EDITOR
    public StringStringDictionary GetDataDic()
    {
        return null;
    }
    public void SetReferenceData();

#endif

    public void Init() { }

    public void Clear() { }
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

    public string GetName()
    {
        return filmName;
    }

#if UNITY_EDITOR

    public void SetReferenceData()
    {
    }

#endif
}

[System.Serializable]
public struct LangLanguageSwitch : IGameData
{
    public string cn, jp, en, ko;

    public string GetName()
    {
        return ToString();
    }

#if UNITY_EDITOR

    public void SetReferenceData()
    {
    }

#endif

    public string GetKey()
    {
        return cn;
    }

    public string GetValue(SystemLanguage systemLanguage)
    {
        switch (systemLanguage)
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