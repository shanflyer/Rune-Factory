using System.Collections;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldMapController : MonoBehaviour
{
#if UNITY_EDITOR
    public bool runTime { get => GameTimeManager.instance.runTime; set => GameTimeManager.instance.runTime = value; }
    public int runTimeDate { get => GameTimeManager.instance.Day; set => GameTimeManager.instance.SetDate(value); }
    public int runTimeHour { get => GameTimeManager.instance.Hour; set => GameTimeManager.instance.SetTime(value); }

    public int runTimeMinute { get => GameTimeManager.instance.Minute; set => GameTimeManager.instance.SetTime(minute: value); }


#endif
    public static WorldMapController instance;
    WorldMapManager worldMapManager;
    [SerializeField]
    GameObject eventSystemObj;
    [SerializeField]
    string worldName;
    [SerializeField]
    int characterId;
    [SerializeField]
    int mapInstance;
    [SerializeField]
    int2 coordinate;
    private void OnEnable()
    {
        instance = this;
        worldMapManager = WorldMapManager.instance;
        if (eventSystemObj&&UnityEngine.SceneManagement.SceneManager.sceneCount > 1)
        {
            eventSystemObj.SetActive(false);
        }
        Init();
       
    }

    public async void Init()
    {
        if (Camera.main == null)
        {
            var cameraPrefab = await GameSourceManager.instance.GetPrefab(DataPath.cameraPrefabPath);
            if (cameraPrefab != null)
            {
                var async = InstantiateAsync(cameraPrefab);
                await async;
                //Instantiate(cameraPrefab);
            } 
        }
       
        GameObjectCurveController.instance.SetUpDataComponent(this); 

        await GameDataSaveManager.instance.InitLoadSaveData();

        var teamManager = TeamManager.instance;
        var npcManager = NPCManager.instance;

        var gameEventManager = GameEventManager.instance;
        var tempCharacterManager = TempCharacterManager.instance;
        var characterManager = CharacterManager.instance;
        var gameManager = GameManager.instance;
        var playerStoreManager = PlayerStoreManager.instance;
        var talkManager = TalkManager.instance; 
        var farmManager = FarmManager.instance;
        var tempMapItemController = TempMapItemController.instance;
        var festivalManager = FestivalManager.instance;
        var gameTimeEventManager = GameTimeEventManager.instance;
        var gameVolumeMangaer = GameVolumeManager.instance; 
        var timeLineManager= TimeLineManger.instance;
        var emote= EmoteManager.instance;
        var homeEquipManager= HomeEquipManager.instance;
        var manufatureManager = ManufatureManager.instance;
        var pastureManager = PastureManager.instance;
        var fisinghManager = FishingManager.instance;
        var fishController = FishController.instance;
        var weatherManager = WeatherManager.instance;

        GameActionManager.instance.QueueAction(new ChangeWorld
        {
            worldName = worldName,
            displayMap = mapInstance
        },true);
        
         
        InputManager.instance.SwitchInputMap(false);
        GameActionManager.instance.QueueAction(new InitInputAction());

        var environmentManger = EnvironmentManger.instance;
        GameTimeManager.instance.ZeroGameTime();


        //if (GameController.instance == null||GameController.instance.startPlay)
        {
            GameTimeManager.instance.StartTimeRun();
            SetCharacterCoordinate setCharacterCoordinate=new SetCharacterCoordinate 
            { 
                characterId = characterId,
                coordinate =new int3(coordinate, mapInstance),  
            };
            GameActionManager.instance.QueueAction(setCharacterCoordinate);
            /*
            GameActionManager.instance.QueueAction(new CreatCharacter
            {
                characterId = characterId,
                mapInstance = mapInstance,
                coordinateX = coordinate.x,
                coordinateY = coordinate.y,
                controller = true,
                isPlayer=true,
            });*/
            await UIManager.instance.ShowGamePanel<MainPanel>(); 
           
           await UIManager.instance.ShowGamePanel<ScreenControllerPanel>();
        }
       
        if (GameController.instance.startPlay)
        {
            GameActionManager.instance.QueueAction(new SwitchInputMap { UI = false });
        }

        GameDataSaveManager.instance.AfterInitMapLoadSaveData();
        GameDataSaveManager.instance.InitSaveDate();
        WeatherManager.instance.RefreshWeather(GameTimeManager.instance.Hour);
    }
    // Use this for initialization
    void Start()
    {
       
    }


#if UNITY_EDITOR
    private void Update()
    {
        if (GameController.instance == null)
        {
            SingletonType.instance.UpData();
        }
    }
#endif 
}
#if UNITY_EDITOR
[CustomEditor(typeof(WorldMapController))]
public class WorldMapControllerEditor : Editor
{
    public WorldMapController gameController
    {
        get
        {
           return target as WorldMapController;
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        gameController.runTime = EditorGUILayout.Toggle("RunTime", gameController.runTime);
        gameController.runTimeDate = EditorGUILayout.IntField("Date", gameController.runTimeDate);
        gameController.runTimeHour = EditorGUILayout.IntSlider("Hour", gameController.runTimeHour, 0, 24);
        gameController.runTimeMinute = EditorGUILayout.IntSlider("Minute", gameController.runTimeMinute, 0, 60);
    }
}
#endif