using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : GamePanel<IReferenceData>
{
    public override bool changeInputModel => false;
    [SerializeField]
    private Toggle Toggle;
    [SerializeField]
    private Transform List;
    [SerializeField] private Button InfoButton, Instagram;
    [SerializeField]
    Button TeamButton, HomeEquipmentButton,MyTalk;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        InfoButton = FindChildGameObject<Button>("Info");
        TeamButton = FindChildGameObject<Button>("Team");
        HomeEquipmentButton = FindChildGameObject<Button>("HomeEquipment");
        MyTalk = FindChildGameObject<Button>("MyTalk");
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshTeam>(RefreshTeam);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared)
        {
            GameActionManager.instance.RemoveListener<RefreshTeam>(RefreshTeam);
        }
      
    }
    void RefreshTeam(RefreshTeam refreshTeam)
    {
        var playerTeam = TeamManager.instance.playerTeam;
        if (playerTeam != null && playerTeam.Teamers.Count > 1)
        {
            TeamButton.gameObject.SetActive(true);
        }
        else
        {
            TeamButton.gameObject.SetActive(false);
        }
    }

    private const string InstagramUser = "shanflyingmountain";
    private const string InstagramWeb = "https://www.instagram.com/" + InstagramUser + "/";
    protected override void Awake()
    {
        base.Awake();
        Instagram.onClick.AddListener(() =>
        {
#if UNITY_ANDROID
        // Android 打开 App，否则打开网页
        string appUri = "instagram://user?username=" + InstagramUser;
        try
        {
            Application.OpenURL(appUri);
        }
        catch
        {
            Application.OpenURL(InstagramWeb);
        }
#elif UNITY_IOS
            // iOS 打开 App，否则网页
            var appUri = "instagram://user?username=" + InstagramUser;
            if (Application.CanStreamedLevelBeLoaded(appUri))
                Application.OpenURL(appUri);
            else
                Application.OpenURL(InstagramWeb);
#else
        Application.OpenURL(InstagramWeb);
#endif
        });
        InfoButton.onClick.AddListener(async () =>
        {
           await UIManager.instance.ShowGamePanel<BookPanel>();
        });
        TeamButton.onClick.AddListener(async () =>
        {
           await UIManager.instance.ShowGamePanel<TeamPanel, CharacterInformationDataList>(TeamManager.instance.GetMyTeamCharacterInfo());
        });
        HomeEquipmentButton.onClick.AddListener(async () =>
        {
            var homeEquipList= HomeEquipManager.instance.GetHomeEquipList(CharacterManager.instance.controllerCharacter.instanceId);
          await  UIManager.instance.ShowGamePanel<PlayerHomeEquipPanel,HomeEquipList>(homeEquipList);
        });
        MyTalk.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<MyTalkPanel>();
        });
        Toggle.onValueChanged.AddListener((value)=>
        {
            List.gameObject.SetActive(value);
        });
    }
    public override Task InitData(string dataKey)
    {
        var playerTeam = TeamManager.instance.playerTeam;
        if (playerTeam != null && playerTeam.Teamers.Count > 1)
        {
            TeamButton.gameObject.SetActive(true);
        }
        else
        {
            TeamButton.gameObject.SetActive(false);
        }
        
        return base.InitData(dataKey);
       
    }
}
