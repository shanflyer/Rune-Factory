using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;

public class CloudRemoteConfig:Singleton<CloudRemoteConfig>
{
    Dictionary<string, object> defaultConfigs = new Dictionary<string, object>();
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            await InitializeRemoteConfigAsync();
            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
            RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());
        }

    }

    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
        base.Clear();
    }
    public struct userAttributes { }
    public struct appAttributes { }

    async Task InitializeRemoteConfigAsync()
    {
        // initialize handlers for unity game services
        await UnityServices.InitializeAsync();

        // remote config requires authentication for managing environment information
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
 
    void ApplyRemoteSettings(ConfigResponse configResponse)
    { 
        if (GameDataManager.instance.GlobalData.debug)
            Debug.Log("RemoteConfigService.Instance.appConfig fetched: " + RemoteConfigService.Instance.appConfig.config.ToString());
    }
    public async Task<bool> GetConfigBool(string key)
    {
        var obj = RemoteConfigService.Instance.appConfig.config.GetValue(key);
        if (obj == null)
        {
            var data = await GameDataManager.instance.GetAsyncData<DefaultConfigData>(key);
            if (data != null)
            {
                return bool.Parse(data.value);
            }
        }
        return (bool)obj;
    }
    public async Task<string> GetConfig(string key)
    {
       var obj=  RemoteConfigService.Instance.appConfig.config.GetValue(key);
        if (obj == null)
        {
            var data =await GameDataManager.instance.GetAsyncData<DefaultConfigData>(key);
            if (data != null)
            {
                return data.value;
            }
        }
        return (string)obj;
    }
}
