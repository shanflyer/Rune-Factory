using System;
using TapSDK.Compliance;
using TapSDK.Core;
using TapSDK.Login;
using UnityEngine;

 public class GameSDKManager:Singleton<GameSDKManager>
    {
         // 游戏在 TapTap 开发者中心对应的 Client ID
    private readonly string clientId = "ulio9wxa6ssgnoyfjx";
    // 游戏在 TapTap 开发者中心对应的 Client Token
    private readonly string clientToken = "2iblbUrSDZ80hrl0ImKykJuJF3UXZkSliWEOMr9J";

    // 是否已初始化
    private readonly bool hasInit = false;
    
    // 是否已通过合规认证检查
    public bool hasCheckedCompliance { get; private set; }
 

    // 声明合规认证回调
    private readonly Action<int, string> ComplianceCallback = (code, errorMsg) =>
    {
        // 根据回调返回的参数 code 添加不同情况的处理
        switch (code)
        {

            case 500: // 玩家未受限制，可正常进入
                instance.hasCheckedCompliance = true;
                // TODO: 显示开始游戏按钮      
                ShowZeroStart showZeroStart = new ShowZeroStart
                {
                    show = true
                };
                GameActionManager.instance.QueueAction(showZeroStart);
                break;

            case 1000: // 防沉迷认证凭证无效时触发
            case 1001: // 当玩家触发时长限制时，点击了拦截窗口中「切换账号」按钮
            case 9002: // 实名认证过程中玩家关闭了实名窗口
                TapTapLogin.Instance.Logout(); // 如果游戏有其他账户系统，此时也应执行退出
                // TODO: 切换到登录页面 例如：SceneManager.LoadScene("Login");
                  showZeroStart = new ShowZeroStart
                {
                    show = false
                };
                GameActionManager.instance.QueueAction(showZeroStart);
                break;

            case 1100: // 当前用户因触发应用设置的年龄限制无法进入游戏
                // TODO: 游戏应自行绘制适龄限制提示，并引导玩家退出游戏
                
                GameNotificationManager.instance.DisplayTips("认证失败","由于年龄限制，您无法进入游戏,即将退出。", () =>
                {
                    Application.Quit();
                });
                break;

            case 1200: // 数据请求失败，应用信息错误或网络连接异常  
                showZeroStart = new ShowZeroStart
                {
                    show = false
                };
                GameActionManager.instance.QueueAction(showZeroStart);
                GameNotificationManager.instance.DisplayTips("数据请求失败","用信息错误或网络连接异常");
                break;

            default:
                Debug.Log("其他可选回调");
                break;
        }

    };

    /// <summary>
    /// 初始化 TapSDK 并注册合规认证回调
    /// </summary>
    public void InitSDK()
    {
        if (!hasInit)
        {
            TapTapSdkOptions coreOptions = new TapTapSdkOptions
            {
                clientId = clientId,
                clientToken = clientToken,
                region = TapTapRegionType.CN,
                preferredLanguage = TapTapLanguageType.zh_Hans,
                enableLog = true
            };
            TapTapComplianceOption complianceOption = new TapTapComplianceOption
            {
                showSwitchAccount = false, // 是否显示切换账号按钮
                useAgeRange = true // 是否使用年龄段信息
            };
            // 创建其他选项数组
            TapTapSdkBaseOptions[] otherOptions = new TapTapSdkBaseOptions[]
            {
                complianceOption
            };
            TapTapSDK.Init(coreOptions, otherOptions);
            TapTapCompliance.RegisterComplianceCallback(ComplianceCallback);
        }
    }
    
    /// <summary>
    /// 开始合规认证检查
    /// </summary>
    /// <param name="userIdentifier">用户唯一标识</param>
    public void StartCheckCompliance(string userIdentifier)
    {
        hasCheckedCompliance = false;
        TapTapCompliance.Startup(userIdentifier);
    }
    }