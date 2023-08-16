using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.UIElements.Experimental;
using UnityEngine.Timeline;
using UnityEngine.Rendering;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public bool SetLanguage;
    public SystemLanguage SetSystemLanguage;

    public Item[] testPlayerItems;

    private void OnEnable()
    { 
        instance = this;
        var UIParent = transform.Find("UIController");
        var filmParent = transform.Find("FilmController"); 
        FilmController.instance.SetParent(filmParent);
        UIManager.instance.SetParent(UIParent);

        var audio = transform.Find("Audio");
        AudioController.instance.SetAudioSource(audio.gameObject);
    }
    public void AddGold()
    {
        AudioController.instance.PlayAudio(SE.click); 
    }
    public void AddCrystal()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            AudioController.instance.PlayAudio(SE.click);
             
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        LanguageManage.instance.SystemLanguageMatch(SetLanguage, SetSystemLanguage);
        AudioController.instance.PlayAudio(BGM.Town1);
        UIManager.instance.ShowGamePanel<ZeroPanel>();
    }
    private void Update()
    {
        GameActionManager.instance.UpData();
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
            //gameController.Test();
        }
    }
}
#endif