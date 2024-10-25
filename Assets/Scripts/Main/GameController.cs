using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
using Unity.Mathematics; 
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameController : MonoBehaviour
{
    [SerializeField]
    private AudioClip startBGM;
    public bool startPlay = true;
#if UNITY_EDITOR

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
    async void ZeroWorld(ZeroWorld zeroWorld)
    {
        GameActionManager.instance.QueueAction(new ChangeWorld
        {
            worldName = worldName,
            displayMap = mapInstance
        });
        /* GameActionManager.instance.QueueAction(new CreatCharacter
          {
              characterId = characterId,
              mapInstance = mapInstance,
              coordinateX = coordinate.x,
              coordinateY = coordinate.y,
              controller = true
          });
          GameActionManager.instance.QueueAction(new CreatDefaultNPC());
        */
        await UIManager.instance.ShowGamePanel<MainPanel>();
        InputManager.instance.SwitchInputMap(false);
    }
    private void OnApplicationQuit()
    {
        Shader.SetGlobalInt("_backColor", 0);
        if (!SingletonType.Cleared)
        {
            SingletonType.instance.ClearAll();
        }
       
        instance = null;
    }
    private async void OnEnable()
    {  
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
        environmentManger = EnvironmentManger.instance;
        var gameVolumeManager = GameVolumeManager.instance;
        var gameManager = GameManager.instance;
        var gameActionDataManager = GameActionDataManager.instance;
        var gameRandom = GameRandom.instance;
        var exploreManger = ExploreManager.instance;
        var sceneManager = SceneManager.instance;
        var fightManager = FightManager.instance;
        var talkManager= TalkManager.instance;
        var festivalManager = FestivalManager.instance;
        var gameTimeEventManager = GameTimeEventManager.instance;
        var teamManager = TeamManager.instance;
       GameTimeManager.instance.ZeroGameTime();
         
        FilmController.instance.SetParent(filmParent);
        UIManager.instance.SetParent(UIParent);

        var audio = transform.Find("Audio");
        AudioController.instance.SetAudioSource(audio.gameObject);

        
        
    }
  
    public void AddCrystal()
    {
        
    }
    // Start is called beforee the first frame update
    async void Start()
    {
        Shader.SetGlobalInt("_backColor", 1);
        GameRuntimeObjManager.instance.CreatParent<RuntimeObjType>(transform);
        LanguageManage.instance.SystemLanguageMatch(SetLanguage, SetSystemLanguage);
        await UIManager.instance.ShowGamePanel<ZeroPanel>();
        AudioController.instance.PlayBGM(startBGM, true, AudioClearType.All, Group: BGMGroup.Theme.ToString());
        GameTimeManager.instance.SetTime(12, 0);
        SwitchInputMap switchInputMap = new SwitchInputMap
        {
            UI = true
        };
        GameActionManager.instance.QueueAction(switchInputMap, true);
        ZeroSetCloudGlobal();
        // GameActionManager.instance.AddListener<ZeroWorld>(ZeroWorld);
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
    private void Update()
    {
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
#endif 
    }
#if UNITY_EDITOR
    public Transform testObj;
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
    }
}
#endif