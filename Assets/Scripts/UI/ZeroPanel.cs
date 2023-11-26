using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using static UnityEditor.Experimental.GraphView.GraphView;
using Unity.Entities.UniversalDelegates;

public class ZeroPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Image titleIcon;
    [SerializeField]
    Button start;
    [SerializeField]
    Canvas[] canvas; 
    [SerializeField]
    ParticleSystemRenderer systemRenderer;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        titleIcon = FindChildGameObject<Image>("Icon");
        start = FindChildGameObject<Button>("StartButton");
        canvas = GetComponentsInChildren<Canvas>(); 
        systemRenderer = FindChildGameObject<ParticleSystemRenderer>("Cloud");
    }
    protected override void Awake()
    {
         
        base.Awake();
        InitTitleIcon();
        start.onClick.AddListener(StartGame);
    }
    public override void Show(int layer = -1)
    {
        var uiLayer= LayerMask.NameToLayer("UI");
        for (int i = 0; i < canvas.Length; i++)
        {
            canvas[i].gameObject.layer = uiLayer;
            canvas[i].enabled = true;
        }
        systemRenderer.gameObject.layer= uiLayer; 
        base.Show(layer);
    }
    public override void Close()
    {
        var hideLayer = LayerMask.NameToLayer("Hide");
        for (int i = 0; i < canvas.Length; i++)
        {
            canvas[i].gameObject.layer = hideLayer;
            canvas[i].enabled = false;
        }
        systemRenderer.gameObject.layer = hideLayer;
        
        base.Close();
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
