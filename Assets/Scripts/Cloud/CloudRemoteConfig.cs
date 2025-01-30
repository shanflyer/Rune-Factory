using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
public class CloudRemoteConfig:Singleton<CloudRemoteConfig>
{
    Dictionary<string, object> defaultConfigs = new Dictionary<string, object>();
    public override async void Init()
    {
        base.Init();
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            await InitializeRemoteConfigAsync();
        }

        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes()); 
       
    }

    protected override void Clear()
    {
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


    bool isGetConfig = false;
    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        isGetConfig = true;
        if (GameDataManager.instance.GlobalData.debug)
            Debug.Log("RemoteConfigService.Instance.appConfig fetched: " + RemoteConfigService.Instance.appConfig.config.ToString());
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
