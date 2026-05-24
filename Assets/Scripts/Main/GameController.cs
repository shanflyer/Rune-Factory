using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyGame;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameController : MonoBehaviour
{
    [SerializeField]
    private AudioClip startBGM;
    public bool startPlay = true;
    public bool noGuide;
#if UNITY_EDITOR

    public int testMap;
    public int4 testCoordinate;

    public Selectable selectable;
    public Weather weather;
    public bool autoWeather;
    public bool test = false;
    public bool runTime
    {
        get
        {
            if (Application.isPlaying)
            {
                return GameTimeManager.instance.runTime;
            }
            else
            {
                return true;
            }

        }
        set
        {
            if (Application.isPlaying)
                GameTimeManager.instance.runTime = value;

        }
    }
    public int runTimeDate
    {
        get
        {
            if (Application.isPlaying)
            {
                return GameTimeManager.instance.Day;
            }
            else
            {
                return 0;
            }

        }
        set
        {
            if (Application.isPlaying && value != GameTimeManager.instance.Day)
            {
                GameTimeManager.instance.SetDate(value);
            }
        }

    }

    public int runTimeHour {
        get
        {
            if (Application.isPlaying)
            {
                return GameTimeManager.instance.Hour;
            }
            else
            {
                return 0;
            }

        }
        set
        {
            if (Application.isPlaying&&value != GameTimeManager.instance.Hour)
            {
                GameTimeManager.instance.SetTime(value);
            }
        }

    }

    public int runTimeMinute {
        get
        {
            if (Application.isPlaying)
            {
                return GameTimeManager.instance.Minute;
            }
            else
            {
                return 0;
            }

        }
        set
        {
            if (Application.isPlaying && value != GameTimeManager.instance.Minute)
            {
                GameTimeManager.instance.SetTime(minute: value);
            }
        }
    }
    [Range(0,4)]
    public float seasonValue;
    public float testSeasonSpeed;

    public float _CloudValue;

    public GameActionAsset gameActionData;

    public void TestGameAction()
    {
        gameActionData.Action();
    }

#endif
    [SerializeField]
    private Vector4 _WindDir, _NoiseSet0, _NoiseSet1;
    private float _seasonValue;

    [HideInInspector]
    public EnvironmentManger environmentManger;

    public static GameController instance;
    public bool SetLanguage;
    public MyLanguage SetSystemLanguage;

    public Item[] testPlayerItems;
    [SerializeField]
    string worldName;
    [SerializeField]
    int characterId;
    [SerializeField]
    int mapInstance;
    [SerializeField]
    int2 coordinate;

    [SerializeField]
    int zeroMapInstance;
    [SerializeField]
    int2 zeroCoordinate;

    public int MapInstance => mapInstance;
    public int2 Coordinate => coordinate;

    public int ZeroMapInstance => zeroMapInstance;
    public int2 ZeroCoordinate => zeroCoordinate;

    private bool loadMap;
    private void OnApplicationQuit()
    {
        if (loadMap) GameDataSaveManager.instance.TryAutoSaveData();


        //Shader.SetGlobalInt("_backColor", 0);
        if (!SingletonType.Cleared&& SingletonType.instance!=null)
        {
            SingletonType.instance.ClearAll();
        }

        instance = null;
    }


    private void OnSavedDataChange(CloudServicesSavedDataChangeResult arg)
    {
        switch (arg.ChangeReason)
        {
            case CloudSavedDataChangeReasonCode.ServerChange:
                break;
            case CloudSavedDataChangeReasonCode.InitialSyncChange:
                break;
            case CloudSavedDataChangeReasonCode.QuotaViolationChange:
                break;
            case CloudSavedDataChangeReasonCode.AccountChange:
                break;
        }
        if (GameDataSaveManager.instance.LoadDataSuccess)
        {
            hideSave = true;
          //  GameManager.instance.ShowTwoSelectAction("Error", LanguageManage.SwitchStr($"云存档数据发生变化！--ChangeReason:{arg.ChangeReason}"), Application.Quit, Application.Quit);
         //   Debug.Log($"云存档数据发生变化！--ChangeReason:{arg.ChangeReason}");
        }

    }

    string nowUserId;
    public bool hideSave { get; private set; }
    private void OnUserChange(CloudServicesUserChangeResult result, Error error)
    {
       // Debug.Log($"云存档OnUserChange！--result.User.UserId:{result.User.UserId}");
        if (string.IsNullOrEmpty(nowUserId))
        {
            nowUserId = result.User.UserId;
        }
        else
        {
            if (result.User.UserId != nowUserId)
            {
                hideSave = true;
                GameManager.instance.ShowTwoSelectAction("用户改变", LanguageManage.SwitchStr("云存档用户发生变化，请退出游戏重新进入"), Application.Quit, Application.Quit);
            }
        }

    }
    bool startGameCompleted = false;
    private void OnSynchronizeComplete(CloudServicesSynchronizeResult result)
    {
        AsyncTaskRunner.Run(() => OnSynchronizeCompleteAsync(result), nameof(OnSynchronizeComplete));
    }

    private async System.Threading.Tasks.Task OnSynchronizeCompleteAsync(CloudServicesSynchronizeResult result)
    {
       // if (GameDataManager.instance.GlobalData.debug)
            Debug.Log($"云存档OnSynchronizeComplete:{result.Success}");
        // var gameDataSaveManager= GameDataSaveManager.instance;

        if (result.Success)
        {
            if (!startGameCompleted)
            {
                GameDataSaveManager.instance.LoadCloudData();
                startGameCompleted = true;
                if (!await StartGame())
                {
                    startGameCompleted = false;
                }
            }

        }
        else if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            hideSave = true;
            GameManager.instance.ShowTwoSelectAction("NetError", LanguageManage.SwitchStr("没有网络连接无法同步存档，请退出重试"), Application.Quit, Application.Quit);
        }
        else
        {
            hideSave = true;
            GameManager.instance.ShowTwoSelectAction("Error", LanguageManage.SwitchStr("云存档加载错误"), Application.Quit, Application.Quit);
        }
        /*
#if UNITY_EDITOR
        GameDataSaveManager.instance.LoadCloudData();
        StartGame();
#else
if (result.Success)
        {
            GameDataSaveManager.instance.LoadCloudData();
            StartGame();
        }
        else if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            hideSave = true;
            GameManager.instance.ShowTwoSelectAction("NetError", LanguageManage.SwitchStr("没有网络连接无法同步存档，请退出重试"), Application.Quit, Application.Quit);
        }
        else
        {
            hideSave = true;
            GameManager.instance.ShowTwoSelectAction("Error", LanguageManage.SwitchStr("云存档加载错误"), Application.Quit, Application.Quit);
        }
#endif
        */

    }

    public void AddCrystal()
    {

    }
    private void Awake()
    {
        CloudServices.OnUserChange += OnUserChange;
        CloudServices.OnSavedDataChange += OnSavedDataChange;
        CloudServices.OnSynchronizeComplete += OnSynchronizeComplete;
        //Unity.Collections.NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;

        var gameDataManager = GameDataManager.instance;
        startGameCompleted = false;
        Screen.SetResolution(Screen.width, Screen.height, true);
        instance = this;
        //GameObject.DontDestroyOnLoad(gameObject);
        var UIParent = transform.Find("UIController");
        var filmParent = transform.Find("FilmController");
        if (Camera.main == null)
        {
            var cameraPrefab = Resources.Load<GameObject>(DataPath.cameraPrefabPath);
            if (cameraPrefab != null)
            {
                Instantiate(cameraPrefab);
            }
        }
        FilmController.instance.SetParent(filmParent);
        UIManager.instance.SetParent(UIParent);
    }
    // Start is called beforee the first frame update
    void Start()
    {
        //Debug.Log($"Application.platform:{Application.platform}");
        CloudServices.Synchronize();
        BillingServices.InitializeStore();
 /*
#if UNITY_EDITOR

        GameDataSaveManager.instance.InitUserSaveData("Test");
        StartGame();
#else
 Debug.Log($"云存档初始化11");
            CloudServices.Synchronize();
            BillingServices.InitializeStore();
#endif*/
        CloudRemoteConfig cloudRemoteConfig = CloudRemoteConfig.instance;
    }

    async Task<bool> StartGame()
    {
        try
        {
            await GameDataManager.instance.WaitForInitialization();
            await GameSourceManager.instance.WaitForInitialization();
        }
        catch (System.Exception e)
        {
            hideSave = true;
            Debug.LogException(e);
            GameManager.instance.ShowTwoSelectAction("Error", LanguageManage.SwitchStr("基础数据加载失败，请退出游戏后重试"), Application.Quit, Application.Quit);
            return false;
        }

        var startupCompletedManagers = new HashSet<System.Type>
        {
            typeof(GameDataManager),
            typeof(GameSourceManager)
        };

        if (!Application.isPlaying || SingletonType.Cleared)
        {
            return false;
        }

        if (GameDataManager.instance.GlobalData == null)
        {
            hideSave = true;
            Debug.LogError("StartGame failed: GameGlobalData is null.");
            GameManager.instance.ShowTwoSelectAction("Error", LanguageManage.SwitchStr("基础配置加载失败，请退出游戏后重试"), Application.Quit, Application.Quit);
            return false;
        }

        if (!await InitializeStartupManagers(startupCompletedManagers)) return false;

        environmentManger = EnvironmentManger.instance;
        var audioController = AudioController.instance;
        var languageManage = LanguageManage.instance;
        var uiManager = UIManager.instance;

        GameTimeManager.instance.ZeroGameTime();

        GameTimerController.instance.DelayAction(100, () => { GameTimeManager.instance.SetTime(12, 0); });

        var audio = transform.Find("Audio");
        audioController.SetAudioSource(audio.gameObject);
        GameRuntimeObjManager.instance.CreatParent<RuntimeObjType>(transform);
        languageManage.SystemLanguageMatch(SetLanguage ? SetSystemLanguage : MyLanguage.NULL);
        await uiManager.ShowGamePanel<ZeroPanel>();

        SwitchInputMap switchInputMap = new SwitchInputMap
        {
            UI = true
        };
        GameActionManager.instance.QueueAction(switchInputMap, true);
        ZeroSetCloudGlobal();
        loadMap = true;
        return true;
    }

    private async Task<bool> WaitForStartupManager<T>(Singleton<T> manager, string managerName, HashSet<System.Type> completedManagers) where T : Singleton<T>
    {
        try
        {
            ValidateStartupDependencies(manager, managerName, completedManagers);
            await manager.WaitForInitialization();
            completedManagers.Add(typeof(T));
            return true;
        }
        catch (System.Exception e)
        {
            hideSave = true;
            Debug.LogException(e);
            GameManager.instance.ShowTwoSelectAction("Error", $"{managerName} 初始化失败，请退出游戏后重试", Application.Quit, Application.Quit);
            return false;
        }
    }

    private void ValidateStartupDependencies<T>(Singleton<T> manager, string managerName, HashSet<System.Type> completedManagers) where T : Singleton<T>
    {
        var dependencies = manager.InitializationDependencies;
        for (int i = 0; i < dependencies.Count; i++)
        {
            var dependency = dependencies[i];
            if (!completedManagers.Contains(dependency))
            {
                // 启动依赖只做顺序诊断，不主动创建依赖，避免隐藏初始化顺序错误。
                Debug.LogWarning($"Startup manager dependency is not completed before wait: manager={managerName}, dependency={dependency.Name}");
            }
        }
    }
    private async Task<bool> InitializeStartupManagers(HashSet<Type> completedManagers)
    {
        List<StartupManagerRegistration> startupManagers;
        try
        {
            startupManagers = SortStartupManagers(CreateStartupManagerRegistrations(), completedManagers);
        }
        catch (Exception e)
        {
            hideSave = true;
            Debug.LogException(e);
            GameManager.instance.ShowTwoSelectAction("Error", LanguageManage.SwitchStr("管理器启动依赖配置错误，请退出游戏后重试"), Application.Quit, Application.Quit);
            return false;
        }

        for (int i = 0; i < startupManagers.Count; i++)
        {
            if (!await WaitForStartupManager(startupManagers[i], completedManagers))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> WaitForStartupManager(StartupManagerRegistration registration, HashSet<Type> completedManagers)
    {
        try
        {
            var manager = registration.CreateManager();
            ValidateStartupDependencies(manager, completedManagers);
            await manager.WaitForInitialization();
            completedManagers.Add(manager.ManagerType);
            return true;
        }
        catch (Exception e)
        {
            hideSave = true;
            Debug.LogException(e);
            GameManager.instance.ShowTwoSelectAction("Error", $"{registration.ManagerType.Name} 初始化失败，请退出游戏后重试", Application.Quit, Application.Quit);
            return false;
        }
    }

    private static void ValidateStartupDependencies(IStartupManager manager, HashSet<Type> completedManagers)
    {
        var dependencies = manager.InitializationDependencies;
        for (int i = 0; i < dependencies.Count; i++)
        {
            var dependency = dependencies[i];
            if (!completedManagers.Contains(dependency))
            {
                throw new InvalidOperationException($"Startup manager dependency is not completed: manager={manager.ManagerName}, dependency={dependency.Name}");
            }
        }
    }

    private static List<StartupManagerRegistration> SortStartupManagers(List<StartupManagerRegistration> registrations, HashSet<Type> completedManagers)
    {
        var registrationByType = registrations.ToDictionary(registration => registration.ManagerType);
        var sortedRegistrations = new List<StartupManagerRegistration>();
        var visitingManagers = new HashSet<Type>();
        var visitedManagers = new HashSet<Type>(completedManagers);

        for (int i = 0; i < registrations.Count; i++)
        {
            Visit(registrations[i]);
        }

        return sortedRegistrations;

        void Visit(StartupManagerRegistration registration)
        {
            if (visitedManagers.Contains(registration.ManagerType))
            {
                return;
            }

            if (!visitingManagers.Add(registration.ManagerType))
            {
                throw new InvalidOperationException($"Startup manager dependency cycle: {registration.ManagerType.Name}");
            }

            for (int i = 0; i < registration.Dependencies.Count; i++)
            {
                var dependency = registration.Dependencies[i];
                if (visitedManagers.Contains(dependency))
                {
                    continue;
                }

                if (!registrationByType.TryGetValue(dependency, out var dependencyRegistration))
                {
                    throw new InvalidOperationException($"Startup manager dependency is not registered: manager={registration.ManagerType.Name}, dependency={dependency.Name}");
                }

                Visit(dependencyRegistration);
            }

            visitingManagers.Remove(registration.ManagerType);
            visitedManagers.Add(registration.ManagerType);
            sortedRegistrations.Add(registration);
        }
    }

    private static List<StartupManagerRegistration> CreateStartupManagerRegistrations()
    {
        return new List<StartupManagerRegistration>
        {
            Register<GameActionManager>(() => GameActionManager.instance),
            Register<GameActionDataManager>(() => GameActionDataManager.instance, typeof(GameActionManager)),
            Register<GameVolumeManager>(() => GameVolumeManager.instance, typeof(GameActionManager)),
            Register<CameraManager>(() => CameraManager.instance, typeof(GameVolumeManager), typeof(GameActionManager)),
            Register<GameManager>(() => GameManager.instance, typeof(GameActionManager)),
            Register<GameTimeManager>(() => GameTimeManager.instance, typeof(GameActionManager)),
            Register<LanguageManage>(() => LanguageManage.instance, typeof(GameDataManager)),
            Register<GameRandom>(() => GameRandom.instance, typeof(GameDataManager)),
            Register<PayManager>(() => PayManager.instance, typeof(GameSourceManager), typeof(GameActionManager)),
            Register<WorldMapObjManager>(() => WorldMapObjManager.instance, typeof(GameActionManager)),
            Register<ShopManager>(() => ShopManager.instance, typeof(GameDataManager), typeof(GameActionManager)),
            Register<ExploreManager>(() => ExploreManager.instance, typeof(GameDataManager)),
            Register<SceneManager>(() => SceneManager.instance, typeof(GameActionManager)),
            Register<FightManager>(() => FightManager.instance, typeof(GameDataManager), typeof(GameSourceManager), typeof(GameRandom), typeof(GameActionManager)),
            Register<TalkManager>(() => TalkManager.instance, typeof(GameDataManager), typeof(GameActionManager)),
            Register<FestivalManager>(() => FestivalManager.instance, typeof(GameDataManager), typeof(LanguageManage), typeof(GameTimeManager)),
            Register<GameTimeEventManager>(() => GameTimeEventManager.instance, typeof(GameDataManager), typeof(GameActionManager)),
            Register<TeamManager>(() => TeamManager.instance, typeof(GameActionManager)),
            Register<GameGuideManager>(() => GameGuideManager.instance, typeof(GameActionManager)),
            Register<ShowItemManager>(() => ShowItemManager.instance, typeof(GameActionManager)),
            Register<AudioController>(() => AudioController.instance, typeof(GameSourceManager)),
            Register<EnvironmentManger>(() => EnvironmentManger.instance, typeof(GameDataManager), typeof(CameraManager), typeof(GameActionManager)),
            Register<UIManager>(() => UIManager.instance, typeof(GameSourceManager)),
            Register<InputManager>(() => InputManager.instance, typeof(GameSourceManager))
        };
    }

    private static StartupManagerRegistration Register<T>(Func<IStartupManager> createManager, params Type[] dependencies)
    {
        return new StartupManagerRegistration(typeof(T), createManager, dependencies);
    }

    private sealed class StartupManagerRegistration
    {
        private readonly Func<IStartupManager> createManager;

        public StartupManagerRegistration(Type managerType, Func<IStartupManager> createManager, IReadOnlyList<Type> dependencies)
        {
            ManagerType = managerType;
            this.createManager = createManager;
            Dependencies = dependencies;
        }

        public Type ManagerType { get; }
        public IReadOnlyList<Type> Dependencies { get; }

        public IStartupManager CreateManager()
        {
            return createManager();
        }
    }

    void ZeroSetCloudGlobal()
    {
        Shader.SetGlobalVector("_WindDir", _WindDir);
        Shader.SetGlobalVector("_NoiseSet0", _NoiseSet0);
        Shader.SetGlobalVector("_NoiseSet1", _NoiseSet1);
    }

    void TestMoveAction(object obj)
    {
        if (obj != null)
        {
           Debug.Log("TestMoveeA:" + obj);

        }
    }
    private void LateUpdate()
    {
        SingletonType.instance.LateUpdate();
    }

    private void FixedUpdate()
    {
        SingletonType.instance.FixedUpdate();
    }

    private void Update()
    {
        //GraphicsSettings.useScriptableRenderPipelineBatching = false;
        SingletonType.instance.Update();
#if UNITY_EDITOR
        seasonValue += Time.deltaTime * testSeasonSpeed;
        if(seasonValue>4)
        {
            seasonValue = 0;
        }

        if (autoWeather)
        {
            if (nowWeather != weather)
            {
                nowWeather.temperature = weather.temperature;
                nowWeather.waterFall = weather.waterFall;
                nowWeather.cloud = weather.cloud;
                nowWeather.wind = weather.wind;
                nowWeather.fog = weather.fog;
                nowWeather.lightning= weather.lightning;
                SetWeatherTest();
            }
        }
        if (nowTimeValue != timeValue)
        {
            nowTimeValue = timeValue;

            if (autoTime)
            {
                int hour = (int)math.floor(nowTimeValue);
                int minute = (int)math.floor((nowTimeValue - hour) * 60);
                hour = hour >= 24 ? 0 : hour;
                GameTimeManager.instance.SetTime(hour, minute);
            }

        }

        if (seasonValue != _seasonValue)
        {
            _seasonValue = seasonValue;
            GameTimeManager.instance.SetSeasonValue(seasonValue);
            Shader.SetGlobalFloat("_SeasonValue", seasonValue);
        }
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            // 编辑器快捷键不阻塞 Update，但需要保留面板加载异常。
            AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<AllItemPanel>(), nameof(AllItemPanel));
        }
        if (autoChangeLanguage && myLanguage != SetSystemLanguage)
        {
            LanguageManage.instance.SetLanguage(SetSystemLanguage);
            myLanguage = SetSystemLanguage;
        }
        tempCount = TempCharacterManager.instance.totalCharacterCount;
#endif
    }
#if UNITY_EDITOR
    Weather nowWeather;
    public bool autoTime;
    [SerializeField]
    private float timeValue;
    private float nowTimeValue;
    public Transform testObj;
    public bool autoChangeLanguage;
    public MyLanguage myLanguage;
    public int tempCount;
    public List<Character> tempCharacters = new List<Character>();
    public void TestInstanceId()
    {
        // 测试输出跟随 Unity 6 的 EntityId，避免继续调用已废弃的 InstanceID。
        EntityId instanceId = selectable.GetEntityId();
        Debug.LogWarning($"instanceId:{instanceId}");
    }

    public void TestLookup()
    {
        MapCellJobController.instance.AddPathRequest(testCoordinate.xy, testCoordinate.zw, testMap,
            (Stack<int2> path, int map, int2 start, int2 end) =>
            {
                string pathStr = "path";
                var pList = path.ToList();
                for (int j = 0; j < pList.Count; j++)
                {
                    pathStr = GameCommon.BlendString(pathStr, ",", pList[j].ToString());
                }
                Debug.Log($"PlayerMove Job  pathStr{pathStr}");
            });
    }

    public void SetCloudGlobal()
    {
        Shader.SetGlobalFloat("_CloudValue", _CloudValue);
        Shader.SetGlobalVector("_WindDir", _WindDir);
        Shader.SetGlobalVector("_NoiseSet0", _NoiseSet0);
        Shader.SetGlobalVector("_NoiseSet1", _NoiseSet1);
    }
    public void SetWeatherTest()
    {
        SetWeather setWeather = new SetWeather
        {
            weather = weather,
            noLerp = true
        };
        GameActionManager.instance.QueueAction(setWeather);
    }
    public void TestLanguage()
    {
        LanguageManage.instance.SetLanguage(SetSystemLanguage);
    }
#endif

}


#if UNITY_EDITOR
[CustomEditor(typeof(GameController))]
public class GameControllerEditor : Editor
{
    public GameController gameController
    {
        get
        {
            return target as GameController;
        }
    }
    float cloudValue;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (Application.isPlaying)
        {
            gameController.runTime = EditorGUILayout.Toggle("RunTime", gameController.runTime);
            gameController.runTimeDate = EditorGUILayout.IntField("Date", gameController.runTimeDate);
            gameController.runTimeHour = EditorGUILayout.IntSlider("Hour", gameController.runTimeHour, 0, 24);
            gameController.runTimeMinute = EditorGUILayout.IntSlider("Minute", gameController.runTimeMinute, 0, 60);
        }

        if (GUILayout.Button("test"))
        {
            gameController.TestLookup();
        }


        if (GUILayout.Button("SetCloud"))
        {
            gameController.SetCloudGlobal();
        }
        if (GUILayout.Button("SetWeather"))
        {
            gameController.SetWeatherTest();
        }
        if (GUILayout.Button("TestInstance"))
        {
            gameController.TestInstanceId();
        }
        if (GUILayout.Button("TestAction"))
        {
            gameController.TestGameAction();
        }
        if (GUILayout.Button("TestLanguage"))
        {
            gameController.TestLanguage();
        }

    }
}
#endif
