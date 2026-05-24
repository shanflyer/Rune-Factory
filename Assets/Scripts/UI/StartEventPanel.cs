using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using TMPro;

public class StartEventPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Button LetterButton;
    [SerializeField]
    Transform content;
    [SerializeField]
    TextMeshProUGUI[] texts;
    [SerializeField]
    Button contentButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        content = FindChildGameObject("Content");
        LetterButton = FindChildGameObject<Button>("LetterButton");
        contentButton = FindChildGameObject<Button>("EndButton");
        texts = transform.GetComponentsInChildren<TextMeshProUGUI>(true);

    }
    protected override void Awake()
    {
        base.Awake();

        for(int i=0;i<texts.Length;i++)
        {
            texts[i].SetSWText(texts[i].text, GameDataSaveManager.instance.UserGameSaveData.playerData.name);
        }

        LetterButton.onClick.AddListener(ClickXinStart);
        contentButton.onClick.AddListener(ClickXinEnd);
    }

    void ClickXinStart()
    {
        content.gameObject.SetActive(true);
    }
    void ClickXinEnd()
    {
        Close();
        GameActionManager.instance.QueueAction(new PlayFilm
        {
            filmName = "角色选择"
        });

    }


    // Update is called once per frame
    void Update () {

	}
}
