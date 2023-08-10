using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartEventPanel : GamePanel
{ 
    [SerializeField]
    Button LetterButton;
    [SerializeField]
    Text contentValue;
    [SerializeField]
    Button contentButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        LetterButton = FindChildGameObject<Button>("LetterButton");
        contentValue = FindChildGameObject<Text>("ContentValue");
        contentButton = FindChildGameObject<Button>("Content");

       
    }
    protected override void Awake()
    {
        base.Awake();
        LetterButton.onClick.AddListener(ClickXinStart);
        contentButton.onClick.AddListener(ClickXinEnd);
    }

    void ClickXinStart()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Book");
        AudioManager.PlayBGM(PlayType.CYCLE,"tt2");
        contentButton.gameObject.SetActive(true);
    }
    void ClickXinEnd()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Book");
        Close();
        GameActionManager.instance.QueueAction(new PlayFilm
        {
            filmName = "StartStory"
        });       
        
    }

  
    // Update is called once per frame
    void Update () {
       
	}
}
