 
using UnityEngine;
 
using Unity.Mathematics;   
using UnityEngine.InputSystem; 
using UnityEngine.UI;
using MyGame; 
using Unity.Transforms;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameController : MonoBehaviour
{
    [SerializeField]
    private AudioClip startBGM;
    public bool startPlay = true;
#if UNITY_EDITOR
    public Selectable selectable;
    public Weather weather;

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

    public GameActionData gameActionData;

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

    private void OnApplicationQuit()
    {
        GameDataSaveManager.instance.TryAutoSaveData();
        //Shader.SetGlobalInt("_backColor", 0);
        if (!SingletonType.Cleared&& SingletonType.instance!=null)
        {
            SingletonType.instance.ClearAll();
        }
        
        instance = null;
    }

    private void OnEnable()
    {

        CloudServices.OnUserChange += OnUserChange;
        CloudServices.OnSavedDataChange += OnSavedDataChange;
        CloudServices.OnSynchronizeComplete += OnSynchronizeComplete;
    }


    private void OnDisable()
    {
        CloudServices.OnUserChange -= OnUserChange;
        CloudServices.OnSavedDataChange -= OnSavedDataChange;
        CloudServices.OnSynchronizeComplete -= OnSynchronizeComplete;

        // unregister from events

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
        GameTimeManager.instance.ZeroGameTime();

        GameTimerController.instance.DelayAction(100, () => { GameTimeManager.instance.SetTime(12, 0); });

        var audio = transform.Find("Audio");
        AudioController.instance.SetAudioSource(audio.gameObject);
        GameRuntimeObjManager.instance.CreatParent<RuntimeObjType>(transform);
        LanguageManage.instance.SystemLanguageMatch(SetLanguage?SetSystemLanguage:0);
        UIManager.instance.ShowGamePanel<ZeroPanel>();
         
        SwitchInputMap switchInputMap = new SwitchInputMap
        {
            UI = true
        };
        GameActionManager.instance.QueueAction(switchInputMap, true);
        ZeroSetCloudGlobal();
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
        SingletonType.instance.LateUpData();
    }
    private void Update()
    {
        //GraphicsSettings.useScriptableRenderPipelineBatching = false;
        SingletonType.instance.UpData();
#if UNITY_EDITOR
        seasonValue += Time.deltaTime * testSeasonSpeed;
        if(seasonValue>4)
        {
            seasonValue = 0;
        }

        if (seasonValue != _seasonValue)
        {
            _seasonValue = seasonValue;
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
#endif
    }
#if UNITY_EDITOR
    public Transform testObj;
    public bool autoChangeLanguage;
    public MyLanguage myLanguage;
    public void TestInstanceId()
    {
        int instanceId = selectable.GetInstanceID();
        Debug.LogWarning($"instanceId:{instanceId}");
    }
    public void Test()
    {
        Vector2 pos = Camera.main.WorldToScreenPoint(testObj.position);
        Vector2 screenSize = GameCommon.GetScreenResolution();
        Vector2 screenValue = new Vector2(pos.x / screenSize.x, pos.y / screenSize.y);
        Debug.Log($"pos:{pos}--ScreenSize:{screenSize}--ScreenValue:{screenValue}");
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
            weather = weather
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
        gameController.runTime = EditorGUILayout.Toggle("RunTime", gameController.runTime);
        gameController.runTimeDate = EditorGUILayout.IntField("Date", gameController.runTimeDate);
        gameController.runTimeHour = EditorGUILayout.IntSlider("Hour", gameController.runTimeHour, 0, 24);
        gameController.runTimeMinute= EditorGUILayout.IntSlider("Minute", gameController.runTimeMinute, 0, 60);
        if (GUILayout.Button("test"))
        {
            gameController.Test();
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