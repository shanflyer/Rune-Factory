using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyGame;
using TapSDK.Login;
using TapSDK.Update;
using UnityEngine;
using UnityEngine.UI;

public class ZeroPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Color cloudColor;

    [SerializeField]
    private Image titleIcon;

    [SerializeField]
    private Button start,TapStart;
     

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
        TapStart.onClick.AddListener(TapStartAction);
    }

    async void TapStartAction()
    {
        try
        {
            // 定义授权范围
            List<string> scopes = new List<string>
            {
                TapTapLogin.TAP_LOGIN_SCOPE_PUBLIC_PROFILE
            };
            // 发起 Tap 登录
            var userInfo = await TapTapLogin.Instance.LoginWithScopes(scopes.ToArray());
            Debug.Log($"登录成功，当前用户 ID：{userInfo.unionId}");
            
            TapStart.transform.localScale = Vector3.zero;
            start.transform.localScale=Vector3.one;
            TapTapAccount taptapAccount = await TapTapLogin.Instance.GetCurrentTapAccount();
            AccessToken accessToken = taptapAccount.accessToken;
            string openId = taptapAccount.openId;
            OfflineSave.instance.SetUserName(openId);
            GameController.instance.AfterLoginAction();
        }
        catch (TaskCanceledException)
        {   TapStart.transform.localScale=Vector3.one;
            start.transform.localScale = Vector3.zero;
            Debug.Log("用户取消登录");
        }
        catch (Exception exception)
        {   TapStart.transform.localScale=Vector3.one;
            start.transform.localScale = Vector3.zero;
            Debug.Log($"登录失败，出现异常：{exception}");
        }
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
            GameDataSaveManager.instance.InitPlayerData("Test", Gender.female, Season.春, 1);
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
            filmName = "角色选择",
            assetName = "Default"
        };
        GameActionManager.instance.QueueAction(playFilm, true);
        await UIManager.instance.ShowGamePanel<SelectCharacterPanel>();
    }

    public override async Task InitData(string dataKay)
    {
        selectPanel.localScale = Vector3.zero;
        start.transform.localScale = Vector3.one;
        PlayZeroBGM();
        
        
        try
        {
            TapTapUpdate.CheckForceUpdate();
        }
        catch (Exception e)
        {
            
        }
        
        TapTapAccount account = null;
        try
        {
            // 检查本地是否已存在 account 信息
            account = await TapTapLogin.Instance.GetCurrentTapAccount();
        }
        catch (Exception e)
        {
            Debug.Log("本地无有效用户信息");
        }

        if (account == null)
        {
            TapStart.transform.localScale=Vector3.one;
            start.transform.localScale = Vector3.zero;
        }
        else
        {
            // 如果当前还未通过合规认证检查，开始认证
            if (!GameSDKManager.instance.hasCheckedCompliance)
            {
                // 开始合规认证检查
                StartCheckCompliance();
            }
            else
            {
                string openId = account.openId;
                TapStart.transform.localScale = Vector3.zero;
                start.transform.localScale = Vector3.one;
                OfflineSave.instance.SetUserName(openId);
                GameController.instance.AfterLoginAction();
            }
        }
         
        
        base.InitData(dataKay);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<ShowZeroStart>(ShowZeroStart);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if(!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<ShowZeroStart>(ShowZeroStart);
    }

    async void ShowZeroStart(ShowZeroStart showZeroStart)
    {
        if (showZeroStart.show)
        {
            var   account = await TapTapLogin.Instance.GetCurrentTapAccount();
            string openId = account.openId;
            TapStart.transform.localScale = Vector3.zero;
            start.transform.localScale = Vector3.one;
            OfflineSave.instance.SetUserName(openId);
            GameController.instance.AfterLoginAction();
        }
        else
        {
            TapStart.transform.localScale = Vector3.one;
            start.transform.localScale = Vector3.zero;
        }
    }
    
    /// <summary>
    /// 开启合规认证检查
    /// </summary>
    public async void StartCheckCompliance()
    {
        // 获取当前已登录用户的 account 信息
        TapTapAccount account = null;
        try
        {
            account = await TapTapLogin.Instance.GetCurrentTapAccount();
        }
        catch (Exception exception)
        {
            Debug.Log($"获取用户信息出现异常：{exception}");
        }
        if (account == null)
        {
            // 无法获取用户信息时，登出并显示登录按钮
            TapTapLogin.Instance.Logout();
            TapStart.transform.localScale=Vector3.one;
            start.transform.localScale = Vector3.zero;
            // TODO: 显示登录按钮
            return;
        }

        // 使用当前 Tap 用户的 unionid 作为用户标识进行合规认证检查
        string userIdentifier = account.unionId;
        GameSDKManager.instance.StartCheckCompliance(userIdentifier);
         
    }
}