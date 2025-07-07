using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ZeroPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Color cloudColor;

    [SerializeField]
    private Image titleIcon;

    [SerializeField]
    private Button start;
     

    [SerializeField]
    private ParticleSystemRenderer systemRenderer;

    [SerializeField]
    private Transform selectPanel;

    [SerializeField]
    private Button newButton, loadButton;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        titleIcon = FindChildGameObject<Image>("Icon");
        start = FindChildGameObject<Button>("StartButton"); 
        systemRenderer = FindChildGameObject<ParticleSystemRenderer>("Cloud");
        selectPanel = FindChildGameObject("SelectPanel");
        newButton = FindChildGameObject<Button>("New");
        loadButton = FindChildGameObject<Button>("Load");
    }

    protected override void Awake()
    {
        base.Awake();
        Shader.SetGlobalColor("_CloudColor", cloudColor);
       // await InitTitleIcon();
        start.onClick.AddListener(ClickStart);
        newButton.onClick.AddListener(StartGame);
        loadButton.onClick.AddListener(LoadDataPanel);
    }

    public override void Show(int layer = -1)
    {
        var uiLayer = LayerMask.NameToLayer("UI"); 
        systemRenderer.gameObject.layer = uiLayer;
        base.Show(layer);
    }

    public override void Close()
    {
        var hideLayer = LayerMask.NameToLayer("Hide"); 
        systemRenderer.gameObject.layer = hideLayer;

        base.Close();
    }
    [SerializeField]
    private AudioClip startBGM;
    public void PlayZeroBGM()
    {
        AudioController.instance.PlayBGM(startBGM, true, AudioClearType.All, Group: BGMGroup.Theme.ToString()); 
    }

    private void ClickStart()
    {
        if (GameController.instance.startPlay)
        {
            Close();
            GameDataSaveManager.instance.InitPlayerData("Test", Gender.male, Season.´º, 1);
            StartWorldInit startWorldInit = new StartWorldInit();
            GameActionManager.instance.QueueAction(startWorldInit);
            //SceneManager.instance.SwitchScene("World");
        }
        else
        {
            if (!GameDataSaveManager.instance.LoadDataSuccess)
            {
                StartGame();
            }
            else
            {
                selectPanel.localScale = Vector3.one;
                start.transform.localScale = Vector3.zero;
            }
        }
    }

    private async void LoadDataPanel()
    {
        Close();
        UIManager.instance.ShowGamePanel<SelectLoadPanel, UserGameSaveDataList>(GameDataSaveManager.instance.UserGameSaveDataList);
    }

    private async void StartGame()
    {
        Close();
        PlayFilm playFilm = new PlayFilm
        {
            filmName = "½ÇÉ«Ñ¡Ôñ",
            assetName = "Default"
        };
        GameActionManager.instance.QueueAction(playFilm, true);
        UIManager.instance.ShowGamePanel<SelectCharacterPanel>();
    }

    public override void InitData(string dataKay)
    {
        selectPanel.localScale = Vector3.zero;
        start.transform.localScale = Vector3.one;
        PlayZeroBGM();
        base.InitData(dataKay);
    }
}