using System.Collections.Generic;
using System.Linq;
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
       // if (GameDataManager.instance.GlobalData.debug)
            Debug.Log($"云存档OnSynchronizeComplete:{result.Success}");
        // var gameDataSaveManager= GameDataSaveManager.instance;

        if (result.Success)
        {
            if (!startGameCompleted)
            { 
                GameDataSaveManager.instance.LoadCloudData();
                StartGame();
                startGameCompleted = true;
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
        GameObjectCurveController.instance.SetUpDataComponent(this);
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

    void StartGame()
    {
        environmentManger = EnvironmentManger.instance;
        // var appStoreManager= AppStoreManager.instance;
        var worldMapObjManager = WorldMapObjManager.instance;
        var shopManager = ShopManager.instance;
        var payManager = PayManager.instance;
        var gameVolumeManager = GameVolumeManager.instance;
        var gameManager = GameManager.instance;
        var gameActionDataManager = GameActionDataManager.instance;
        var gameRandom = GameRandom.instance;
        var exploreManger = ExploreManager.instance;
        var sceneManager = SceneManager.instance;
        var fightManager = FightManager.instance;
        var talkManager = TalkManager.instance;
        var festivalManager = FestivalManager.instance;
        var gameTimeEventManager = GameTimeEventManager.instance;
        var teamManager = TeamManager.instance;
        var gameGuideManager = GameGuideManager.instance;
        var showItemManager = ShowItemManager.instance;
        GameTimeManager.instance.ZeroGameTime();

        GameTimerController.instance.DelayAction(100, () => { GameTimeManager.instance.SetTime(12, 0); });

        var audio = transform.Find("Audio");
        AudioController.instance.SetAudioSource(audio.gameObject);
        GameRuntimeObjManager.instance.CreatParent<RuntimeObjType>(transform);
        LanguageManage.instance.SystemLanguageMatch(SetLanguage ? SetSystemLanguage : MyLanguage.NULL);
        UIManager.instance.ShowGamePanel<ZeroPanel>();
         
        SwitchInputMap switchInputMap = new SwitchInputMap
        {
            UI = true
        };
        GameActionManager.instance.QueueAction(switchInputMap, true);
        ZeroSetCloudGlobal();
        loadMap = true;
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
            UIManager.instance.ShowGamePanel<AllItemPanel>();
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
        int instanceId = selectable.GetInstanceID();
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