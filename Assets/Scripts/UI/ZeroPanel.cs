using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class ZeroPanel : GamePanel
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
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        Close();
        UIManager.instance.ShowGamePanel<SelectCharacterPanel>(layer:2);
    }
    public override Task InitData(int dataId)
    {
        return base.InitData(dataId);
    }
     
}
