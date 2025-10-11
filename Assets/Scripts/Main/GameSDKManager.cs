using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TapSDK.Compliance;
using TapSDK.Core;
using TapSDK.Login; 
using TapSDK.CloudSave;
using UnityEngine;

 public class GameSDKManager:Singleton<GameSDKManager>
{ 
        internal class TapCloudSaveCallback : ITapCloudSaveCallback
        {
            public TapCloudSaveCallback(){}

            public void OnResult(int resultCode)
            {
                // 处理状态码
                // 300001：需要登录
                // 300002：初始化失败 需要重新初始化
            }
        }
         // 游戏在 TapTap 开发者中心对应的 Client ID
    private readonly string clientId = "ulio9wxa6ssgnoyfjx";
    // 游戏在 TapTap 开发者中心对应的 Client Token
    private readonly string clientToken = "2iblbUrSDZ80hrl0ImKykJuJF3UXZkSliWEOMr9J";

    // 是否已初始化
    private readonly bool hasInit = false;
    
    // 是否已通过合规认证检查
    public bool hasCheckedCompliance { get; private set; }
 
    TapCloudSaveCallback callback = new TapCloudSaveCallback();

    // 声明合规认证回调 
    void ComplianceCallback(int code, string errorMsg)
    {
        // 根据回调返回的参数 code 添加不同情况的处理
        switch (code)
        {

            case 500: // 玩家未受限制，可正常进入
                instance.hasCheckedCompliance = true;
                // TODO: 显示开始游戏按钮      
              
                DownSaveData();
                break;

            case 1000: // 防沉迷认证凭证无效时触发
            case 1001: // 当玩家触发时长限制时，点击了拦截窗口中「切换账号」按钮
            case 9002: // 实名认证过程中玩家关闭了实名窗口
                TapTapLogin.Instance.Logout(); // 如果游戏有其他账户系统，此时也应执行退出
                // TODO: 切换到登录页面 例如：SceneManager.LoadScene("Login");
                ShowZeroStart showZeroStart = new ShowZeroStart
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
    }

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
            TapTapCloudSave.RegisterCloudSaveCallback(callback);
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

    private string archiveUuid;
    private string archiveFileId;

    public async  void UpdateSaveData()
    {
        try
        { 
            ArchiveMetadata metadata = new ArchiveMetadata(
                archiveName: OfflineSave.instance.UserName,
                archiveSummary: "",
                archiveExtra: "",
                archivePlaytime: 0  // 创建时间
            );
            string archiveFilePath = OfflineSave.instance.saveFilePath; 
            ArchiveData updated = await TapTapCloudSave.UpdateArchive(archiveUuid, metadata, archiveFilePath, null);
            // 处理存档更新成功
        }
        catch (TapException ex)
        {
            switch (ex.code)
            {
                case 400000://非法的存档文件/封面大小
                    break;
                case 400001://存档上传频率超限
                    break;
                case 400002://指定的存档不存在
                    break;
                case 400003://单个应用下存档数量超限
                    break;
                case 400004://单个应用下使用存储空间超限
                    break;
                case 400005://总使用存储空间超限
                    break;
                case 400006://非法的操作令牌，通常是由于网络卡顿，创建/更新存档耗时过长导致
                    break;
                case 400007://不允许并发调用
                    break;
                case 400008://找不到可用的 OSS 供应商
                    break;
                case 400009://存档名称不合法
                    break;
            }
            // 处理错误，可使用 ex.Code 与 ex.Message
        }
        
    }
    public async void DownSaveData()
    {
        try
        {
            List<ArchiveData> archives = await TapTapCloudSave.GetArchiveList();
            if (archives == null || archives.Count == 0)
            {
                archiveUuid = null;
                archiveFileId = null;
                return;
            }
            // 处理存档列表
            for (int i = 0; i < archives.Count; i++)
            {
                var archive = archives[i];
                // 存档UUID
                 archiveUuid = archive.Uuid;
                // 存档文件ID
                archiveFileId = archive.FileId;

                try
                {
                    byte[] data = await TapTapCloudSave.GetArchiveData(archiveUuid, archiveFileId);
                    string text = Encoding.UTF8.GetString(data);
                    OfflineSave.instance.UpdateSaveFileData(text);
                    
                    OfflineSave.instance.LoadData();
                    ShowZeroStart showZeroStart = new ShowZeroStart
                    {
                        show = true
                    };
                    GameActionManager.instance.QueueAction(showZeroStart); 
                }
                catch (TapException ex)
                {
                    // 处理错误，可使用 ex.Code 与 ex.Message
                }
                break;
            }
        }
        catch (TapException ex)
        {
            // 处理错误，可使用 ex.Code 与 ex.Message
        }
       
    }

    public void TryCreateCloudSave()
    {
        if (archiveUuid == null)
        {
            CreateCloudSave();
        }
    }
    public async void CreateCloudSave()
    {
        // 存档元信息
        ArchiveMetadata metadata = new ArchiveMetadata(
            archiveName: OfflineSave.instance.UserName,
            archiveSummary: "",
            archiveExtra: "",
            archivePlaytime: 0  // 创建时间
        );
       // 存档文件路径（单个存档文件大小不超过10MB）
        string archiveFilePath = OfflineSave.instance.saveFilePath; 

        try
        {
            ArchiveData archive = await TapTapCloudSave.CreateArchive(metadata, archiveFilePath, null);
            // 处理存档创建成功
            archiveUuid=archive.Uuid;
            archiveFileId=archive.FileId;
        }
        catch (TapException ex)
        {
            // 处理错误，可使用 ex.Code 与 ex.Message
        }
    }
}