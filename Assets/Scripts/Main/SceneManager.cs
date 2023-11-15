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
        GameActionData beforeActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(switchScene.beforeLoadActionId);
        GameActionData afterActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(switchScene.afterLoadActionId);

        SwitchScene(switchScene.sceneName, beforeActionData != null ? ()=> { beforeActionData.Action(0, 0); } : null, 
            afterActionData != null ? ()=> { afterActionData.Action(0, 0); } : null);
    }
    public async void SwitchScene(string sceneName,Action beforeLoadSceneAction=null,Action afterSceneAction=null)
    {
        this.loadSceneAction = afterSceneAction;

        if (beforeLoadSceneAction != null)
        {
            beforeLoadSceneAction.Invoke();
        }
        if (!string.IsNullOrEmpty(nowSceen))
        { 
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(nowSceen);
        }
       
        this.AsyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadingPanel=await UIManager.instance.ShowGamePanel<LoadingPanel>();
        nowSceen = sceneName;

    } 
    public void UnloadNowScene()
    {
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(nowSceen);
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