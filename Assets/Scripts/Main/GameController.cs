 
using UnityEngine;
 
using Unity.Mathematics;   
using UnityEngine.InputSystem; 
using UnityEngine.UI;
using MyGame;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Unity.Transforms;

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
    public SystemLanguage SetSystemLanguage;

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

    private async void OnEnable()
    {  
        Screen.SetResolution(Screen.width, Screen.height,true);
        instance = this;
        //GameObject.DontDestroyOnLoad(gameObject);
        var UIParent = transform.Find("UIController");
        var filmParent = transform.Find("FilmController");
        GameObjectCurveController.instance.SetUpDataComponent(this);
        if (Camera.main == null)
        {
            var cameraPrefab = await GameSourceManager.instance.GetPrefab(DataPath.cameraPrefabPath);
            if (cameraPrefab != null)
            {
                var asyncInstantiateOperation = InstantiateAsync(cameraPrefab);
                await asyncInstantiateOperation;
                var cameraObj = asyncInstantiateOperation.Result[0];
              //  GameObject.DontDestroyOnLoad(cameraObj);
            }
        }
        FilmController.instance.SetParent(filmParent);
        UIManager.instance.SetParent(UIParent);

    }
  
    public void AddCrystal()
    {
        
    }
    // Start is called beforee the first frame update
    async void Start()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication); 
       
    }

    private bool manuallyAuthenticate = false;
    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            var userId=  PlayGamesPlatform.Instance.GetUserId();
            CloudDataManager.instance.ShowSelectUI(userId);
            //StartGame();
        }
        else
        {
            /*
            GameManager.instance.ShowTwoSelectAction($"Google SingInStatus:{status}", "是否在未登录的Google Play的情况下游玩，您可能无法同步线上存档等",()=>{

                GameDataSaveManager.instance.InitUserSaveData("测试", null);
                StartGame();
            } , () =>
            {
                Application.Quit();
            });*/

            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            if (manuallyAuthenticate == false)
            {
                PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
                manuallyAuthenticate = true;

            }
            else
            {
                GameManager.instance.ShowTwoSelectAction($"Google SingInStatus:{status}", "是否在未登录的Google Play的情况下游玩，您可能无法同步线上存档等", () => {

                    GameDataSaveManager.instance.InitUserSaveData("测试", null);
                    StartGame();
                }, () =>
                {
                    Application.Quit();
                });
            }
               
        }
    }
    public  void StartGame()
    {
        environmentManger = EnvironmentManger.instance;
        // var appStoreManager= AppStoreManager.instance;
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

       

        var audio = transform.Find("Audio");
        AudioController.instance.SetAudioSource(audio.gameObject);
        GameRuntimeObjManager.instance.CreatParent<RuntimeObjType>(transform);
        LanguageManage.instance.SystemLanguageMatch(SetLanguage, SetSystemLanguage);
        UIManager.instance.ShowGamePanel<ZeroPanel>();

        GameTimeManager.instance.SetTime(12, 0);
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
#endif 
    }
#if UNITY_EDITOR
    public Transform testObj;
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
    }
}
#endif