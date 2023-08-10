using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public bool SetLanguage;
    public SystemLanguage SetSystemLanguage;
    private void OnEnable()
    {
        instance = this;
        var UIParent = transform.Find("UIController");
        var filmParent = transform.Find("FilmController");
        FilmController.instance.SetParent(filmParent);
        UIManager.instance.SetParent(UIParent);
    }
    // Start is called before the first frame update
    void Start()
    {
        LanguageManage.instance.SystemLanguageMatch(SetLanguage, SetSystemLanguage);
        AudioManager.InitAudioList();

        UIManager.instance.ShowGamePanel<ZeroPanel>();
    }
    private void Update()
    {
        GameActionManager.instance.UpData();
    }
}
