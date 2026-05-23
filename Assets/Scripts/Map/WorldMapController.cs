using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    private int initVersion;

    private void OnEnable()
    {
        instance = this;
        worldMapManager = WorldMapManager.instance;
        if (eventSystemObj && SceneManager.sceneCount > 1)
        {
            eventSystemObj.SetActive(false);
        }

        GameActionManager.instance.AddListener<StartWorldInit>(StartWorldInit);
        GameActionManager.instance.AddListener<LoadMapCompleted>(LoadMapCompleted);
    }

    void StartWorldInit(StartWorldInit startWorldInit)
    {
        Init();
    }

    void LoadMapCompleted(LoadMapCompleted loadMapCompleted)
    {
        GameDataSaveManager.instance.AfterInitMapLoadSaveData();
        UIManager.instance.CloseGamePanel<LoadingPanel>();
        WeatherManager.instance.RefreshWeather(GameTimeManager.instance.Hour);
    }

    public void Init()
    {
        // MonoBehaviour 事件入口保持同步签名，实际异步流程集中到 InitAsync 里捕获异常。
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        int version = ++initVersion;
        GameTimerController.instance.RemoveWaiter(AutoSave);

        try
        {
            await EnsureCameraAsync();
            if (!IsInitCurrent(version)) return;

            await GameDataSaveManager.instance.InitLoadSaveData();
            if (!IsInitCurrent(version)) return;

            await InitWorldFlowAsync(version);
        }
        catch (Exception e)
        {
            if (IsInitCurrent(version))
            {
                Debug.LogException(e);
            }
        }
    }

    private async Task EnsureCameraAsync()
    {
        if (Camera.main != null) return;

        var cameraPrefab = await GameSourceManager.instance.GetPrefab(DataPath.cameraPrefabPath);
        if (cameraPrefab == null) return;

        var async = InstantiateAsync(cameraPrefab);
        await async;
    }

    private async Task InitWorldFlowAsync(int version)
    {
        _ = GameDataSaveManager.instance.loadGameSaveData;
        if (GameGuideManager.instance.endGuideFilmIndex > 0)
        {
            UIManager.instance.ShowGamePanel<LoadingPanel>();
        }

        if (!await InitSingletonAsync<TeamManager>(version)) return;
        if (!await InitSingletonAsync<ShortcutManager>(version)) return;
        if (!await InitSingletonAsync<MapCellController>(version)) return;
        if (!await InitSingletonAsync<ItemManager>(version)) return;
        if (!await InitSingletonAsync<CharacterBehaviorManager>(version)) return;
        if (!await InitSingletonAsync<NPCTaskScheduleManager>(version)) return;
        if (!await InitSingletonAsync<EnvironmentManger>(version)) return;
        if (!await InitSingletonAsync<NPCManager>(version)) return;
        if (!await InitSingletonAsync<GameEventManager>(version)) return;
        if (!await InitSingletonAsync<TempCharacterManager>(version)) return;
        if (!await InitSingletonAsync<CharacterManager>(version)) return;
        if (!await InitSingletonAsync<GameManager>(version)) return;
        if (!await InitSingletonAsync<PlayerStoreManager>(version)) return;
        if (!await InitSingletonAsync<TalkManager>(version)) return;
        if (!await InitSingletonAsync<FarmManager>(version)) return;
        if (!await InitSingletonAsync<TempMapItemController>(version)) return;
        if (!await InitSingletonAsync<FestivalManager>(version)) return;
        if (!await InitSingletonAsync<GameTimeEventManager>(version)) return;
        if (!await InitSingletonAsync<GameVolumeManager>(version)) return;
        if (!await InitSingletonAsync<TimeLineManger>(version)) return;
        if (!await InitSingletonAsync<EmoteManager>(version)) return;
        if (!await InitSingletonAsync<SceneInfoManager>(version)) return;
        if (!await InitSingletonAsync<HomeEquipManager>(version)) return;
        if (!await InitSingletonAsync<ManufactureManager>(version)) return;
        if (!await InitSingletonAsync<PastureManager>(version)) return;
        if (!await InitSingletonAsync<FishingManager>(version)) return;
        if (!await InitSingletonAsync<FishController>(version)) return;
        if (!await InitSingletonAsync<WeatherManager>(version)) return;

        AudioController.instance.ClearBGM(AudioClearType.All, BGMGroup.Theme.ToString());
        if (!await YieldInitFrame(version)) return;

        InputManager.instance.SwitchInputMap(false);
        GameActionManager.instance.QueueAction(new InitInputAction());
        GameTimeManager.instance.ZeroGameTime();
        if (!await YieldInitFrame(version)) return;

        GameDataSaveManager.instance.InitSaveDate();
        if (!await YieldInitFrame(version)) return;

        QueueInitialWorldActions();
        if (!await YieldInitFrame(version)) return;

        if (GameController.instance.startPlay)
        {
            NPCManager.instance.CreateZeroNPC();
            GameActionManager.instance.QueueAction(new SwitchInputMap { UI = false });
        }

        if (!await YieldInitFrame(version)) return;
        if (!await YieldInitFrame(version)) return;

        GameTimeManager.instance.runTime = true;

        if (!await YieldInitFrame(version)) return;
        UIManager.instance.ShowGamePanel<MainPanel>();

        if (!await YieldInitFrame(version)) return;
        UIManager.instance.ShowGamePanel<ScreenControllerPanel>();
        UIManager.instance.ShowGamePanel<PlayerTopPanel>();
        UIManager.instance.ShowGamePanel<ShortcutPanel>();

        ScheduleAutoSave();
    }

    private void QueueInitialWorldActions()
    {
        if (GameGuideManager.instance.endGuideFilmIndex < 0 && !GameController.instance.noGuide)
        {
            GameActionManager.instance.QueueAction(new ChangeWorld
            {
                worldName = worldName,
                displayMap = GameController.instance.ZeroMapInstance
            }, true);

            if (GameController.instance.startPlay)
            {
                SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
                {
                    characterId = characterId,
                    fiexedDisplay = true,
                    coordinate = new int3(GameController.instance.ZeroCoordinate.xy, GameController.instance.ZeroMapInstance),
                };
                GameActionManager.instance.QueueAction(setCharacterCoordinate);
            }
        }
        else if (GameController.instance.noGuide || GameGuideManager.instance.endGuideFilmIndex >=
                 GameDataManager.instance.GlobalData.endGuideIndex)
        {
            GameActionManager.instance.QueueAction(new ChangeWorld
            {
                worldName = worldName,
                displayMap = GameController.instance.MapInstance
            }, true);

            SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
            {
                characterId = characterId,
                coordinate = new int3(GameController.instance.Coordinate.xy, GameController.instance.MapInstance),
                fiexedDisplay = true
            };
            GameActionManager.instance.QueueAction(setCharacterCoordinate);
        }
        else
        {
            GameGuideManager.instance.SetGameGuidFilmDataAction(characterId, worldName);
        }
    }

    private async Task<bool> YieldInitFrame(int version)
    {
        await Task.Yield();
        return IsInitCurrent(version);
    }

    private async Task<bool> InitSingletonAsync<T>(int version) where T : Singleton<T>
    {
        var manager = Singleton<T>.instance;
        await manager.WaitForInitialization();
        return await YieldInitFrame(version);
    }

    private bool IsInitCurrent(int version)
    {
        return this != null && isActiveAndEnabled && version == initVersion;
    }

    private void ScheduleAutoSave()
    {
        GameTimerController.instance.RemoveWaiter(AutoSave);
        GameTimerController.instance.DelayAction(600000, AutoSave);
    }

    void AutoSave()
    {
        GameDataSaveManager.instance.TryAutoSaveData();
        ScheduleAutoSave();
    }

    void Start()
    {
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (GameController.instance == null)
        {
            SingletonType.instance.Update();
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
        if (Application.isPlaying)
        {
            gameController.runTime = EditorGUILayout.Toggle("RunTime", gameController.runTime);
            gameController.runTimeDate = EditorGUILayout.IntField("Date", gameController.runTimeDate);
            gameController.runTimeHour = EditorGUILayout.IntSlider("Hour", gameController.runTimeHour, 0, 24);
            gameController.runTimeMinute = EditorGUILayout.IntSlider("Minute", gameController.runTimeMinute, 0, 60);
        }
    }
}
#endif
