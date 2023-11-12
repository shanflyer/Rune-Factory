using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class ZeroPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Image titleIcon;
    [SerializeField]
    Button start;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        titleIcon = FindChildGameObject<Image>("Icon");
        start = FindChildGameObject<Button>("StartButton");
    }
    protected override void Awake()
    {
         
        base.Awake();
        InitTitleIcon();
        start.onClick.AddListener(StartGame);
    }
    
    async Task InitTitleIcon()
    { 
        LanguageSpriteObj title = await GameSourceManager.instance.GetSingleScriptableObject<LanguageSpriteObj>(DataPath.titlePath);
        var sprite= title.GetSprite(LanguageManage.nowLanguage);
        titleIcon.sprite = sprite;
    }
    void StartGame()
    {
        AudioController.instance.PlayAudio(SE.click); 
        Close();
        PlayFilm playFilm = new PlayFilm
        {
            filmName = "½ÇÉ«Ñ¡Ôñ",
            assetName= "Default"
        };
        GameActionManager.instance.QueueAction(playFilm,true);
        UIManager.instance.ShowGamePanel<SelectCharacterPanel>();
    }
    public override Task InitData(string dataKay)
    {
        return base.InitData(dataKay);
    }
     
}
