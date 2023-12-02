using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
using Unity.Mathematics; 
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
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
        SingletonType.instance.ClearAll();
        instance = null;
    }
    private void OnEnable()
    {  
        instance = this;
        var UIParent = transform.Find("UIController");
        var filmParent = transform.Find("FilmController");
        GameObjectCurveController.instance.SetUpDataComponent(this);

        var environmentManger = EnvironmentManger.instance;

        var gameManager = GameManager.instance;
        var gameActionDataManager = GameActionDataManager.instance;
        var gameRandom = GameRandom.instance;
        var exploreManger = ExploreManager.instance;
        var sceneManager = SceneManager.instance;
        var fightManager = FightManager.instance;
        var talkManager= TalkManager.instance;
        var festivalManager = FestivalManager.instance;
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

    public void Test()
    {
       NativeList<TriggerArea> list = new NativeList<TriggerArea>(2, Allocator.Temp);
        TriggerArea triggerArea = new TriggerArea
        {
            cells = new UnsafeHashSet<int2>(2, Allocator.Temp)
        };
        triggerArea.cells.Add(new int2(0, 0));
        list.Add(triggerArea);

        var test = list[0];
        foreach(var v in test.cells)
        {
            Debug.Log(v);
        }

        TriggerArea triggerArea1 = new TriggerArea
        {
            cells = new UnsafeHashSet<int2>(2, Allocator.Temp)
        };
        triggerArea1.cells.Add(new int2(1, 1));
        list.Add(triggerArea1);

        var test1 = list[0];
        foreach (var v in test1.cells)
        {
            Debug.Log(v);
        }

        TriggerArea triggerArea2 = new TriggerArea
        {
            cells = new UnsafeHashSet<int2>(2, Allocator.Temp)
        };
        triggerArea2.cells.Add(new int2(2, 2));
        list.Add(triggerArea2);

        var test2 = list[0];
        foreach (var v in test2.cells)
        {
            Debug.Log(v);
        }

        list.RemoveAt(1);
        var test3 = list[0];
        foreach (var v in test3.cells)
        {
            Debug.Log(v);
        }
        list.RemoveAt(0);
        var test4 = list[0];
        foreach (var v in test4.cells)
        {
            Debug.Log(v);
        }
        list.Dispose();
    }
    
    
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