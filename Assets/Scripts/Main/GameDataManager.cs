using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class GameDataManager : Singleton<GameDataManager>
{
    public sealed class DataIntegrityReport
    {
        public int checkedCount;
        public readonly List<string> missingPaths = new List<string>();
        public readonly List<string> incompatibleAssets = new List<string>();
        public readonly List<string> duplicatePaths = new List<string>();

        public bool HasProblem => missingPaths.Count > 0 || incompatibleAssets.Count > 0 || duplicatePaths.Count > 0;
    }

    public Dictionary<Type, Dictionary<string, IGameData>> allGameStaticDatas = new Dictionary<Type, Dictionary<string, IGameData>>();

    public GameGlobalData GlobalData { get; private set; }
    public DataIntegrityReport LastIntegrityReport { get; private set; }
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;
    public bool InitializationCompleted { get; private set; }
    public Exception InitializationException { get; private set; }

    protected override void Clear()
    {
        allGameStaticDatas.Clear();
        GlobalData = null;
        LastIntegrityReport = null;
        InitializationCompleted = false;
        InitializationException = null;
        initializationTask = Task.CompletedTask;
        base.Clear();
    }

    public override void Init()
    {
        base.Init();
        InitializationCompleted = false;
        InitializationException = null;
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        try
        {
            GlobalData = GetData<GameGlobalData>();
            if (GlobalData == null)
            {
                Debug.LogError($"GameDataManager init failed: missing {nameof(GameGlobalData)} at {DataPath.GetDataPath(typeof(GameGlobalData))}");
            }
            LastIntegrityReport = ValidateDataPathRegistry();
            LogDataIntegrityReport(LastIntegrityReport);

            var gameDataSaveManager = GameDataSaveManager.instance;
            //初始加载
            await LoadAllAsyncData<GameActionAsset>();
            await LoadAllAsyncData<GrowModelData>();
            await LoadAllAsyncData<ProfessionData>();
            await LoadAllAsyncData<FunctionData>();
            InitializationCompleted = true;
        }
        catch (Exception e)
        {
            InitializationException = e;
            Debug.LogException(e);
            throw;
        }
    }

    public override async Task WaitForInitialization()
    {
        await InitializationTask;
    }

    private static bool TryGetDataPath(Type type, out string path)
    {
        path = DataPath.GetDataPath(type);
        if (!string.IsNullOrEmpty(path))
        {
            return true;
        }

        Debug.LogError($"GameDataManager data path is not registered: {type.FullName}");
        return false;
    }

    public static DataIntegrityReport ValidateDataPathRegistry()
    {
        var report = new DataIntegrityReport();
        var pathOwners = new Dictionary<string, Type>();

        foreach (var dataPath in DataPath.dataPathDic)
        {
            report.checkedCount++;
            Type type = dataPath.Key;
            string path = dataPath.Value;
            if (string.IsNullOrEmpty(path))
            {
                report.missingPaths.Add($"{type.FullName}: <empty>");
                continue;
            }

            if (pathOwners.TryGetValue(path, out var ownerType))
            {
                report.duplicatePaths.Add($"{path}: {ownerType.Name}, {type.Name}");
            }
            else
            {
                pathOwners[path] = type;
            }

            UnityEngine.Object singleAsset = Resources.Load(path);
            UnityEngine.Object[] folderAssets = Resources.LoadAll(path);
            if (singleAsset == null && (folderAssets == null || folderAssets.Length == 0))
            {
                report.missingPaths.Add($"{type.FullName}: {path}");
                continue;
            }

            if (singleAsset != null &&
                singleAsset is not IGameData &&
                singleAsset is not TextAsset &&
                !IsDataArrayAsset(singleAsset, type))
            {
                report.incompatibleAssets.Add($"{type.FullName}: {path}, asset={singleAsset.GetType().Name}");
            }
        }

        return report;
    }

    private static bool IsDataArrayAsset(UnityEngine.Object asset, Type dataType)
    {
        Type dataArrayType = typeof(IDataArray<>).MakeGenericType(dataType);
        return dataArrayType.IsInstanceOfType(asset);
    }

    private static void LogDataIntegrityReport(DataIntegrityReport report)
    {
        if (report == null)
        {
            return;
        }

        if (!report.HasProblem)
        {
            Debug.Log($"GameDataManager data integrity report: checked={report.checkedCount}, ok.");
            return;
        }

        Debug.LogWarning($"GameDataManager data integrity report: checked={report.checkedCount}, missing={report.missingPaths.Count}, incompatible={report.incompatibleAssets.Count}, duplicatePath={report.duplicatePaths.Count}.");
        for (int i = 0; i < report.missingPaths.Count; i++)
        {
            Debug.LogWarning($"Missing data path: {report.missingPaths[i]}");
        }
        for (int i = 0; i < report.incompatibleAssets.Count; i++)
        {
            Debug.LogWarning($"Incompatible data asset: {report.incompatibleAssets[i]}");
        }
        for (int i = 0; i < report.duplicatePaths.Count; i++)
        {
            Debug.LogWarning($"Duplicate data path: {report.duplicatePaths[i]}");
        }
    }

    private static void AddLoadedData(Dictionary<string, IGameData> dataDic, IGameData data, Type type, string path)
    {
        if (data == null)
        {
            Debug.LogWarning($"GameDataManager skipped null data: type={type.FullName}, path={path}");
            return;
        }

        string key = data.GetKey();
        if (dataDic.ContainsKey(key))
        {
            Debug.LogError($"GameDataManager duplicate data key: type={type.FullName}, key={key}, path={path}");
            dataDic[key] = data;
            return;
        }

        dataDic.Add(key, data);
    }

    private static void LogMissingData(Type type, string key, string path)
    {
        Debug.LogWarning($"GameDataManager missing data: type={type.FullName}, key={key}, path={path}");
    }

    private static bool TryGetLoadedData<T>(Dictionary<string, IGameData> dataDic, string key, out T result) where T : IGameData
    {
        result = default(T);
        if (dataDic == null)
        {
            return false;
        }

        if (dataDic.TryGetValue(key, out var data) && data is T typedData)
        {
            result = typedData;
            return true;
        }

        return false;
    }

    private async Task LoadAllAsyncData<T>() where T : IGameData
    {
        Type type = typeof(T);
        if (!TryGetDataPath(type, out var path))
        {
            return;
        }

        var _results = ExtensionsResources.LoadAllIGameData<T>(path);
        if (_results != null && _results.Count != 0)
        {
            var dataDic = new Dictionary<string, IGameData>();
            for (int i = 0; i < _results.Count; i++)
            {
                var data = _results[i];
                data.Init();
                AddLoadedData(dataDic, data, type, path);
            }
            allGameStaticDatas[type] = dataDic;

            return;
        }

        var dataAsset = await ExtensionsResources.LoadResourceAsync(path);
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
                    AddLoadedData(dataDic, data, type, path);
                }
                allGameStaticDatas[type] = dataDic;
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"GameDataManager failed to load IDataArray: type={type.FullName}, path={path}, error={e}");
            }
        }
        if (dataAsset != null && dataAsset is TextAsset)
        {
            var textAsset = dataAsset as TextAsset;
            var dataDic = new Dictionary<string, IGameData>();

            try
            {
                var results = JsonConvert.DeserializeObject<List<T>>(textAsset.text, GameJsonSettings.CreateDefaultSettings());
                for (int i = 0; i < results.Count; i++)
                {
                    var data = results[i];
                    data.Init();
                    AddLoadedData(dataDic, data, type, path);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"GameDataManager failed to load TextAsset: type={type.FullName}, path={path}, error={e}");
            }
            allGameStaticDatas[type] = dataDic;
        }
        else if (dataAsset == null)
        {
            LogMissingData(type, string.Empty, path);
        }
    }

    public async Task<List<T>> GetAllAsyncData<T>() where T : IGameData
    {
        List<T> results = new List<T>(); ;
        Type type = typeof(T);
        if (!TryGetDataPath(type, out var path))
        {
            return results;
        }

        var _results = ExtensionsResources.LoadAllIGameData<T>(path);
        if (_results != null && _results.Count != 0)
        {
            results = _results.ToList();

            //allGameStaticDatas[type] = dataDic;
        }
        if (results.Count == 0)
        {
            var dataAsset = await ExtensionsResources.LoadResourceAsync(path);
            if (dataAsset != null && dataAsset is IDataArray<T> dataArray)
            {

                try
                {
                    results = dataArray.DataList.ToList();

                }
                catch (Exception e)
                {
                    Debug.LogWarning($"GameDataManager failed to get all IDataArray: type={type.FullName}, path={path}, error={e}");
                }
                // allGameStaticDatas[type] = dataDic;
            }
        }
        if (results.Count == 0)
        {
            var dataAsset = await ExtensionsResources.LoadResourceAsync<TextAsset>(path);

            if (dataAsset != null)
            {


                try
                {
                    results = JsonConvert.DeserializeObject<List<T>>(dataAsset.text, GameJsonSettings.CreateDefaultSettings());

                }
                catch (Exception e)
                {
                    Debug.LogWarning($"GameDataManager failed to get all TextAsset: type={type.FullName}, path={path}, error={e}");
                }
                //allGameStaticDatas[type] = dataDic;
            }
        }
        return results;
    }

    public T GetData<T>(string key = "") where T : IGameData
    {
        Type type = typeof(T);
        if (!TryGetDataPath(type, out var path))
        {
            return default(T);
        }

        string dataPath = string.IsNullOrEmpty(key) ? path : $"{path}/{key}";
        if (allGameStaticDatas.TryGetValue(type, out var dataDic))
        {
            if (dataDic.TryGetValue(key, out var data))
            {
                return (T)data;
            }
            else
            {
                data = ExtensionsResources.LoadIGameData<T>(dataPath);
                if (data != null)
                {
                    data.Init();
                    dataDic[data.GetKey()] = data;
                    allGameStaticDatas[type] = dataDic;
                    return (T)data;
                }
            }
        }

        var _data = ExtensionsResources.LoadIGameData<T>(dataPath);
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


        LogMissingData(type, key, dataPath);
        return default(T); ;
    }

    public async Task<T> GetAsyncData<T>(int key) where T : IGameData
    {
        return await GetAsyncData<T>(key.ToString());
    }

    public async Task<T> GetAsyncData<T>(string key = "") where T : IGameData
    {
        Type type = typeof(T);
        if (!TryGetDataPath(type, out var path))
        {
            return default(T);
        }

        string dataPath = $"{path}/{key}";
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

        var dataAsset = await ExtensionsResources.LoadResourceAsync(path);
        if (dataAsset != null)
        {
            if(dataAsset is IDataArray<T> dataArray)
            {
                dataDic = new Dictionary<string, IGameData>();
                for (int i = 0; i < dataArray.DataList.Length; i++)
                {
                    var item = dataArray.DataList[i];
                    item.Init();
                    AddLoadedData(dataDic, item, type, path);
                }
                allGameStaticDatas[type] = dataDic;
                // 批量资源加载完成后立即回读目标 key，避免缓存已命中却继续落到 missing 日志。
                if (TryGetLoadedData(dataDic, key, out T loadedData))
                {
                    return loadedData;
                }
            }
            else if(dataAsset is T t)
            {
                t.Init();
                dataDic = new Dictionary<string, IGameData>();
                AddLoadedData(dataDic, t, type, path);
                allGameStaticDatas[type] = dataDic;
                if (string.IsNullOrEmpty(key) || t.GetKey() == key)
                {
                    return t;
                }
            }
        }

        var textAsset = dataAsset as TextAsset;
        if (textAsset != null)
        {
            dataDic = new Dictionary<string, IGameData>();
            try
            {
                var datas = JsonConvert.DeserializeObject<List<T>>(textAsset.text, GameJsonSettings.CreateDefaultSettings());
                for (int i = 0; i < datas.Count; i++)
                {
                    var data = datas[i];
                    data.Init();
                    AddLoadedData(dataDic, data, type, path);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"GameDataManager failed to get async TextAsset: type={type.FullName}, key={key}, path={path}, error={e}");
            }
            allGameStaticDatas[type] = dataDic;
            // 文本资源反序列化后同样要回读一次，避免成功加载后仍返回 default。
            if (TryGetLoadedData(dataDic, key, out T loadedData))
            {
                return loadedData;
            }
        }
        LogMissingData(type, key, dataPath);
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

[Serializable]
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
