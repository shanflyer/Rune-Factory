using System;
using System.Threading;
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
    private bool actionListenersRegistered;

    private void OnEnable()
    {
        instance = this;
        worldMapManager = WorldMapManager.instance;
        if (eventSystemObj && SceneManager.sceneCount > 1)
        {
            eventSystemObj.SetActive(false);
        }

        RegisterActionListeners();
    }

    private void OnDisable()
    {
        initVersion++;
        if (instance == this)
        {
            instance = null;
        }

        if (!SingletonType.Cleared && GameTimerController.HasInstance)
        {
            GameTimerController.instance.RemoveWaiter(AutoSave);
        }

        UnregisterActionListeners();
    }

    private void RegisterActionListeners()
    {
        if (actionListenersRegistered || SingletonType.Cleared)
        {
            return;
        }

        GameActionManager.instance.AddListener<StartWorldInit>(StartWorldInit);
        GameActionManager.instance.AddListener<LoadMapCompleted>(LoadMapCompleted);
        actionListenersRegistered = true;
    }

    private void UnregisterActionListeners()
    {
        if (!actionListenersRegistered || SingletonType.Cleared || !GameActionManager.HasInstance)
        {
            return;
        }

        // 场景卸载或对象禁用时解绑世界初始化监听，防止旧场景控制器继续响应。
        GameActionManager.instance.RemoveListener<StartWorldInit>(StartWorldInit);
        GameActionManager.instance.RemoveListener<LoadMapCompleted>(LoadMapCompleted);
        actionListenersRegistered = false;
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
        // 世界初始化只保留最后一次请求，场景重载或重复 StartWorldInit 会取消旧链路。
        AsyncTaskRunner.RunLatest(nameof(WorldMapController.Init), InitAsync, nameof(WorldMapController.Init));
    }

    private async Task InitAsync(CancellationToken cancellationToken)
    {
        int version = ++initVersion;
        GameTimerController.instance.RemoveWaiter(AutoSave);

        try
        {
            await EnsureCameraAsync(cancellationToken);
            if (!IsInitCurrent(version)) return;

            await GameDataSaveManager.instance.InitLoadSaveData();
            if (cancellationToken.IsCancellationRequested) return;
            if (!IsInitCurrent(version)) return;

            await InitWorldFlowAsync(version, cancellationToken);
        }
        catch (Exception e)
        {
            if (!cancellationToken.IsCancellationRequested && IsInitCurrent(version))
            {
                Debug.LogException(e);
            }
        }
    }

    private async Task EnsureCameraAsync(CancellationToken cancellationToken)
    {
        if (Camera.main != null) return;

        var cameraPrefab = await GameSourceManager.instance.GetPrefab(DataPath.cameraPrefabPath);
        if (cancellationToken.IsCancellationRequested) return;
        if (cameraPrefab == null) return;

        var async = InstantiateAsync(cameraPrefab);
        await async;
    }

    private async Task InitWorldFlowAsync(int version, CancellationToken cancellationToken)
    {
        _ = GameDataSaveManager.instance.loadGameSaveData;
        if (GameGuideManager.instance.endGuideFilmIndex > 0)
        {
            await UIManager.instance.ShowGamePanel<LoadingPanel>();
            if (cancellationToken.IsCancellationRequested || !IsInitCurrent(version)) return;
        }

        if (!await InitSingletonAsync<TeamManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<ShortcutManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<MapCellController>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<ItemManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<CharacterBehaviorManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<NPCTaskScheduleManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<EnvironmentManger>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<NPCManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<GameEventManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<TempCharacterManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<CharacterManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<GameManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<PlayerStoreManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<TalkManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<FarmManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<TempMapItemController>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<FestivalManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<GameTimeEventManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<GameVolumeManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<TimeLineManger>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<EmoteManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<SceneInfoManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<HomeEquipManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<ManufactureManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<PastureManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<FishingManager>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<FishController>(version, cancellationToken)) return;
        if (!await InitSingletonAsync<WeatherManager>(version, cancellationToken)) return;

        AudioController.instance.ClearBGM(AudioClearType.All, BGMGroup.Theme.ToString());
        if (!await YieldInitFrame(version, cancellationToken)) return;

        InputManager.instance.SwitchInputMap(false);
        GameActionManager.instance.QueueAction(new InitInputAction());
        GameTimeManager.instance.ZeroGameTime();
        if (!await YieldInitFrame(version, cancellationToken)) return;

        GameDataSaveManager.instance.InitSaveDate();
        if (!await YieldInitFrame(version, cancellationToken)) return;

        QueueInitialWorldActions();
        if (!await YieldInitFrame(version, cancellationToken)) return;

        if (GameController.instance.startPlay)
        {
            NPCManager.instance.CreateZeroNPC();
            GameActionManager.instance.QueueAction(new SwitchInputMap { UI = false });
        }

        if (!await YieldInitFrame(version, cancellationToken)) return;
        if (!await YieldInitFrame(version, cancellationToken)) return;

        GameTimeManager.instance.runTime = true;

        if (!await YieldInitFrame(version, cancellationToken)) return;
        await UIManager.instance.ShowGamePanel<MainPanel>();

        if (!await YieldInitFrame(version, cancellationToken)) return;
        await UIManager.instance.ShowGamePanel<ScreenControllerPanel>();
        if (cancellationToken.IsCancellationRequested || !IsInitCurrent(version)) return;
        await UIManager.instance.ShowGamePanel<PlayerTopPanel>();
        if (cancellationToken.IsCancellationRequested || !IsInitCurrent(version)) return;
        await UIManager.instance.ShowGamePanel<ShortcutPanel>();

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

    private async Task<bool> YieldInitFrame(int version, CancellationToken cancellationToken)
    {
        await Task.Yield();
        return !cancellationToken.IsCancellationRequested && IsInitCurrent(version);
    }

    private async Task<bool> InitSingletonAsync<T>(int version, CancellationToken cancellationToken) where T : Singleton<T>
    {
        var manager = Singleton<T>.instance;
        await manager.WaitForInitialization();
        return await YieldInitFrame(version, cancellationToken);
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
