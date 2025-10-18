using System.Collections.Generic;
using System.Threading.Tasks; 
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

    //    RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
   //     RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes()); 
       
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
   //     await UnityServices.InitializeAsync();

        // remote config requires authentication for managing environment information
    //    if (!AuthenticationService.Instance.IsSignedIn)
        {
     //       await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
 
   
    public async Task<bool> GetConfigBool(string key)
    {
      //  var obj = RemoteConfigService.Instance.appConfig.config.GetValue(key);
      //  if (obj == null)
        {
            var data = await GameDataManager.instance.GetAsyncData<DefaultConfigData>(key);
            if (data != null)
            {
                return bool.Parse(data.value);
            }
        }
      return false;
    }
    public async Task<string> GetConfig(string key)
    {
     //  var obj=  RemoteConfigService.Instance.appConfig.config.GetValue(key);
      //  if (obj == null)
        {
            var data =await GameDataManager.instance.GetAsyncData<DefaultConfigData>(key);
            if (data != null)
            {
                return data.value;
            }
        }
        return "";
    }
}
