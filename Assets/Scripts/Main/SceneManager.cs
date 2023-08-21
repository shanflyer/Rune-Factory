using System;
using System.Collections; 
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : Singleton<SceneManager>
{
    public override bool NeedUpdata => true;
    string nowSceen;
    public string Now => nowSceen;
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<SwitchScene>(SwitchScene);
    }
    async void SwitchScene(SwitchScene switchScene)
    {
        GameActionData beforeActionData = await GameDataManager.instance.GetAsyncObjectData<GameActionData>(switchScene.beforeLoadActionId);
        GameActionData afterActionData = await GameDataManager.instance.GetAsyncObjectData<GameActionData>(switchScene.afterLoadActionId);

        SwitchScene(switchScene.sceneName, beforeActionData != null ? beforeActionData.Action : null, 
            afterActionData != null ? afterActionData.Action : null);
    }
    public async void SwitchScene(string sceneName,Action unLoadSceneAction=null,Action loadSceneAction=null)
    {
        this.loadSceneAction = loadSceneAction;

        if (!string.IsNullOrEmpty(nowSceen))
        {
            if (unLoadSceneAction != null)
            {
                unLoadSceneAction.Invoke();
            }
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(nowSceen);
        }
       
        this.AsyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadingPanel=await UIManager.instance.ShowGamePanel<LoadingPanel>();
        nowSceen = sceneName;

    }
    LoadingPanel loadingPanel;
    AsyncOperation AsyncOperation;
    Action loadSceneAction;


    protected override void UpData()
    {
        base.UpData();
        if (AsyncOperation == null||loadingPanel==null)
        {
            return;
        }
        loadingPanel.RefreshLoadValue(AsyncOperation.progress);
        if (AsyncOperation.progress >= 1)
        {
            if (loadSceneAction != null)
            {
                loadSceneAction.Invoke();
            }
            loadSceneAction = null;
            loadingPanel.Close();
            loadingPanel = null;
            AsyncOperation = null;
        }
    }
     
}