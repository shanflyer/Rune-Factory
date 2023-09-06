using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class StartEventPanel : GamePanel<IReferenceData>
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
        AudioController.instance.PlayAudio(SE.Book);
        AudioController.instance.PlayAudio(BGM.tt2);
         
        contentButton.gameObject.SetActive(true);
    }
    void ClickXinEnd()
    {
        AudioController.instance.PlayAudio(SE.Book);
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
