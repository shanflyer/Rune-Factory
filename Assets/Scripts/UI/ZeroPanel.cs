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
    private Canvas[] canvaes;

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
        canvaes = GetComponentsInChildren<Canvas>();
        systemRenderer = FindChildGameObject<ParticleSystemRenderer>("Cloud");
        selectPanel = FindChildGameObject("SelectPanel");
        newButton = FindChildGameObject<Button>("New");
        loadButton = FindChildGameObject<Button>("Load");
    }

    protected override async void Awake()
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
        for (int i = 0; i < canvaes.Length; i++)
        {
            canvaes[i].gameObject.layer = uiLayer;
            canvaes[i].enabled = true;
        }
        systemRenderer.gameObject.layer = uiLayer;
        base.Show(layer);
    }

    public override void Close()
    {
        var hideLayer = LayerMask.NameToLayer("Hide");
        for (int i = 0; i < canvaes.Length; i++)
        {
            canvaes[i].gameObject.layer = hideLayer;
            canvaes[i].enabled = false;
        }
        systemRenderer.gameObject.layer = hideLayer;

        base.Close();
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
        await UIManager.instance.ShowGamePanel<SelectLoadPanel, UserGameSaveDataList>(GameDataSaveManager.instance.UserGameSaveDataList);
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
        await UIManager.instance.ShowGamePanel<SelectCharacterPanel>();
    }

    public override Task InitData(string dataKay)
    {
        selectPanel.localScale = Vector3.zero;
        start.transform.localScale = Vector3.one;
        return base.InitData(dataKay);
    }
}