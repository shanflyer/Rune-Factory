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

#if UNITY_EDITOR
    public bool runTime { get => GameTimeManager.instance.runTime; set => GameTimeManager.instance.runTime = value; }
   
    public int runTimeHour { 
        get => GameTimeManager.instance.Hour;
        set
        {
            if (value != GameTimeManager.instance.Hour)
            {
                GameTimeManager.instance.SetTime(value);
            }
        }
    }
     
    public int runTimeMinute { 
        get => GameTimeManager.instance.Minute;
        set
        {
            if (value != GameTimeManager.instance.Minute)
            {
                GameTimeManager.instance.SetTime(minute: value);
            }
        } 
    }
#endif

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
        SingletonType.instance.ClearAll();
        instance = null;
    }
    private async void OnEnable()
    {  
        instance = this;
        GameObject.DontDestroyOnLoad(gameObject);
        var UIParent = transform.Find("UIController");
        var filmParent = transform.Find("FilmController");
        GameObjectCurveController.instance.SetUpDataComponent(this);
        if (Camera.main == null)
        {
            var cameraPrefab = await GameSourceManager.instance.GetPrefab(DataPath.cameraPrefabPath);
            if (cameraPrefab != null)
            {
                var cameraObj = Instantiate(cameraPrefab);
                GameObject.DontDestroyOnLoad(cameraObj);
            }
        }
        var environmentManger = EnvironmentManger.instance;
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
        GameTimeManager.instance.ZeroGameTime();
         
        FilmController.instance.SetParent(filmParent);
        UIManager.instance.SetParent(UIParent);

        var audio = transform.Find("Audio");
        AudioController.instance.SetAudioSource(audio.gameObject); 
    }
  
    public void AddCrystal()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            AudioController.instance.PlayAudio(SE.click);
             
        }

    }
    // Start is called beforee the first frame update
    async void Start()
    {
        Shader.SetGlobalInt("_backColor", 1);
        GameRuntimeObjManager.instance.CreatParent<RuntimeObjType>(transform);
        LanguageManage.instance.SystemLanguageMatch(SetLanguage, SetSystemLanguage);
        AudioController.instance.PlayAudio(BGM.Town1);
        await UIManager.instance.ShowGamePanel<ZeroPanel>();
         
        GameTimeManager.instance.SetTime(12, 0);
        // GameActionManager.instance.AddListener<ZeroWorld>(ZeroWorld);
    }
    private void Update()
    {
        SingletonType.instance.UpData(); 
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
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        gameController.runTime = EditorGUILayout.Toggle("RunTime", gameController.runTime);
        gameController.runTimeHour = EditorGUILayout.IntSlider("Hour", gameController.runTimeHour, 0, 24);
        gameController.runTimeMinute= EditorGUILayout.IntSlider("Minute", gameController.runTimeMinute, 0, 60);
        if (GUILayout.Button("test"))
        {
            gameController.Test();
        }
    }
}
#endif