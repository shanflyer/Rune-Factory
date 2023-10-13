using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;
using Unity.Mathematics;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public bool SetLanguage;
    public SystemLanguage SetSystemLanguage;

    public Item[] testPlayerItems;

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

        var gameManager = GameManager.instance;
        var gameActionDataManager = GameActionDataManager.instance;
        var gameRandom = GameRandom.instance;
        var exploreManger = ExploreManager.instance;
        var sceneManager = SceneManager.instance;
        var fightManager = FightManager.instance;
        var talkManager= TalkManager.instance;

        GameObjectCurveController.instance.SetUpDataComponent(this);

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
    void Start()
    {
        GameRuntimeObjManager.instance.CreatParent<RuntimeObjType>(transform);
        LanguageManage.instance.SystemLanguageMatch(SetLanguage, SetSystemLanguage);
        AudioController.instance.PlayAudio(BGM.Town1);
        UIManager.instance.ShowGamePanel<ZeroPanel>();
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

public  class A
{
    public static bool Test() { return true; }
    public  static bool test { get; }
}
public class B:A
{

    public static bool test {
        get => true;
    }
}
public class C : A
{

    public static bool test
    {
        get => false;
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
        if (GUILayout.Button("test"))
        {
            gameController.Test();
        }
    }
}
#endif