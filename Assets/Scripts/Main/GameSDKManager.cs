using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using TapSDK.Core;
using TapSDK.Login;
using TapSDK.Compliance;
using TapSDK.CloudSave;
using TapSDK.Update;
using UnityEngine;

public class GameSDKManager : Singleton<GameSDKManager>, ITapCloudSaveCallback
{
    // —— Tap 配置 ——（替换成你的）
    private readonly string clientId    = "egnipuxvuxfwykoisu";
    private readonly string clientToken = "wjNbrqaa34f0OkmSNJuSsOGYV0v5eh4Cndk7jAVw";

    private bool hasInit = false;
    public  bool hasCheckedCompliance { get; private set; }

    // 登录状态
    private bool   _isLoggedIn;
    private string _userId;      // 登录成功后记录的 unionId（合规用）

    // 云档标识
    private string archiveUuid;
    private string archiveFileId;

    // ========== 初始化 ==========
    public void InitSDK()
    {
        if (hasInit) return;

        var coreOptions = new TapTapSdkOptions
        {
            clientId = clientId,
            clientToken = clientToken,
            region = TapTapRegionType.CN,
            preferredLanguage = TapTapLanguageType.zh_Hans,
            enableLog = true
        };

        var complianceOption = new TapTapComplianceOption
        {
            showSwitchAccount = false,
            useAgeRange = true
        };

        TapTapSDK.Init(coreOptions, new TapTapSdkBaseOptions[] { complianceOption });

        TapTapCompliance.RegisterComplianceCallback(ComplianceCallback);
        TapTapCloudSave.RegisterCloudSaveCallback(this);
        
        hasInit = true;
    }

    // ========== 登录（用 LoginWithScopes），成功后立即合规 ==========
    public async Task LoginThenComplianceAsync()
    {
        // 1) 定义授权范围
        List<string> scopes = new List<string>
        {
            TapTapLogin.TAP_LOGIN_SCOPE_PUBLIC_PROFILE
        };

        // 2) 发起 Tap 登录（v4）
        var userInfo = await TapTapLogin.Instance.LoginWithScopes(scopes.ToArray());
        // 重要：用 unionId 作为“稳定唯一用户标识”用于合规与你的账号体系绑定 
        OfflineSave.instance.SetUserName(userInfo.unionId);
     
        _isLoggedIn = true;
        _userId     = userInfo.unionId;

        // 3) 立刻发起合规检查
        StartCheckCompliance(_userId);
    }

    // ========== 合规 ==========
    public void StartCheckCompliance(string userIdentifier)
    {
        hasCheckedCompliance = false;
        TapTapCompliance.Startup(userIdentifier);
    }

    private void ComplianceCallback(int code, string errorMsg)
    {
        switch (code)
        {
            case 500: // 通过
                hasCheckedCompliance = true;
                TapTapUpdate.CheckForceUpdate(); 
                // 合规通过 → 拉云档或进入游戏
                DownSaveData();
                break;

            case 1000: // 实名凭证无效
            case 1001: // 触发时长限制，点击“切换账号”
            case 9002: // 实名窗口被关闭
                SafeLogout();
                GameActionManager.instance.QueueAction(new ShowZeroStart { show = false });
                break;

            case 1100: // 年龄限制
                GameNotificationManager.instance.DisplayTips(
                    "认证失败",
                    "由于年龄限制，您无法进入游戏，即将退出。",
                    () => Application.Quit()
                );
                break;

            case 1200: // 网络/配置异常
                GameActionManager.instance.QueueAction(new ShowZeroStart { show = false });
                GameNotificationManager.instance.DisplayTips("数据请求失败", "应用信息错误或网络连接异常");
                break;

            default:
                Debug.Log($"[Compliance] other code={code}, msg={errorMsg}");
                break;
        }
    }

    private void SafeLogout()
    {
        try { TapTapLogin.Instance.Logout(); } catch { /* ignore */ }
        _isLoggedIn = false;
        _userId = null;
        archiveUuid = null;
        archiveFileId = null;
    }

    // ========== CloudSave 全局回调（v4 常见） ==========
    public void OnResult(int resultCode)
    {
        Debug.Log($"[CloudSaveCallback] code={resultCode}");
        switch (resultCode)
        {
            case 300001: // 需要登录
                // 可在 UI 提示后再次调用 LoginThenComplianceAsync（避免死循环）
                break;
            case 300002: // 初始化失败 → 重新初始化
                hasInit = false;
                InitSDK();
                break;
        }
    }

    // ========== 云存档：创建 ==========
    public void TryCreateCloudSave()
    {
        if (archiveUuid == null) CreateCloudSave();
    }

    private ArchiveMetadata metadata=>new ArchiveMetadata(
        archiveName: OfflineSave.instance.UserName,
        archiveSummary: GameDataSaveManager.instance.UserGameSaveData.saveTime,
        archiveExtra: $"schema=0",
        archivePlaytime: 0
    );
    
    public async void CreateCloudSave()
    {
        try
        {
            
            EnsureReadyOrThrow(); 

            string archiveFilePath = OfflineSave.instance.saveFilePath;
            if (!File.Exists(archiveFilePath))
            {
                OfflineSave.instance.SaveData(UpdateCloudData:false);
            }

            ArchiveData archive = await TapTapCloudSave.CreateArchive(metadata, archiveFilePath, null);

            archiveUuid  = archive.Uuid;
            archiveFileId = archive.FileId;

            Debug.Log($"[CloudSave] Create ok. uuid={archiveUuid}, fileId={archiveFileId}");
        }
        catch (TapException ex)
        {
            Debug.LogError($"[CloudSave] Create failed: code={ex.Code}, msg={ex.Message}");
            HandleCloudSaveKnownErrors(ex.Code);
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] Create failed: {e.Message}");
        }
    }

    // ========== 云存档：更新 ==========
    public async void UpdateSaveData()
    {
        try
        {
            EnsureReadyOrThrow();

          

            string archiveFilePath = OfflineSave.instance.saveFilePath;

            ArchiveData updated = await TapTapCloudSave.UpdateArchive(archiveUuid, metadata, archiveFilePath, null);
            Debug.Log($"[CloudSave] Update ok. uuid={updated.Uuid}, fileId={updated.FileId}");
        }
        catch (TapException ex)
        { 
            Debug.LogError($"[CloudSave] Update failed: code={ex.Code}, msg={ex.Message}");
            HandleCloudSaveKnownErrors(ex.Code);
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] Update failed: {e.Message}");
        }
    }

    // ========== 云存档：拉列表并下载 ==========
    public async void DownSaveData()
    {
        try
        {
            EnsureLoggedInOrThrow();

            List<ArchiveData> archives = await TapTapCloudSave.GetArchiveList();
            if (archives == null || archives.Count == 0)
            {
                archiveUuid = null;
                archiveFileId = null;
                GameActionManager.instance.QueueAction(new ShowZeroStart { show = true });
                GameController.instance.AfterLoginAction();
                return;
            }

            // 这里简单取第一条；若要“最新”，可按 UpdatedAt 排序
            var archive = archives[0];
            archiveUuid  = archive.Uuid;
            archiveFileId = archive.FileId;

            try
            {
                byte[] data = await TapTapCloudSave.GetArchiveData(archiveUuid, archiveFileId);
                string text = Encoding.UTF8.GetString(data);

                OfflineSave.instance.UpdateSaveFileData(text,false);
                OfflineSave.instance.LoadData();

                GameActionManager.instance.QueueAction(new ShowZeroStart { show = true });
                GameController.instance.AfterLoginAction();
            }
            catch (TapException ex2)
            {
                Debug.LogError($"[CloudSave] Download failed: code={ex2.Code}, msg={ex2.Message}");
            }
            
           
        }
        catch (TapException ex)
        {
            Debug.LogError($"[CloudSave] List failed: code={ex.Code}, msg={ex.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] DownSaveData ex: {e.Message}");
        }
    }

    // ========== 校验 ==========
    private void EnsureReadyOrThrow()
    {
        if (!_isLoggedIn)          throw new Exception("Not logged in. Call LoginThenComplianceAsync first.");
        if (!hasCheckedCompliance) throw new Exception("Compliance not passed. Wait for ComplianceCallback(500).");

        if (string.IsNullOrEmpty(archiveUuid))
        {
            // 如果你要求“必须先创建再更新”，可以在这里自动触发创建
            // 否则保持抛错由上层决定
        }
    }

    private void EnsureLoggedInOrThrow()
    {
        if (!_isLoggedIn) throw new Exception("Not logged in. Call LoginThenComplianceAsync first.");
    }

    // ========== 错误码映射 ==========
    private void HandleCloudSaveKnownErrors(int code)
    {
        switch (code)
        {
            case 400000: Debug.LogWarning("非法的存档文件/封面大小"); break;
            case 400001: Debug.LogWarning("存档上传频率超限"); break;
            case 400002: Debug.LogWarning("指定的存档不存在"); break;
            case 400003: Debug.LogWarning("单个应用下存档数量超限"); break;
            case 400004: Debug.LogWarning("单个应用下使用存储空间超限"); break;
            case 400005: Debug.LogWarning("总使用存储空间超限"); break;
            case 400006: Debug.LogWarning("操作令牌非法/超时"); break;
            case 400007: Debug.LogWarning("不允许并发调用"); break;
            case 400008: Debug.LogWarning("找不到可用的 OSS 供应商"); break;
            case 400009: Debug.LogWarning("存档名称不合法"); break;

            case 300001: Debug.LogWarning("需要登录"); break;
            case 300002: Debug.LogWarning("初始化失败"); break;

            case 401:    Debug.LogWarning("未授权/会话失效"); break;
            case 403:    Debug.LogWarning("禁止访问/权限不足"); break;
            case 404:    Debug.LogWarning("对象不存在"); break;
            case 429:    Debug.LogWarning("流控超限"); break;
            default:     Debug.LogWarning($"未识别的错误码：{code}"); break;
        }
    }
}
