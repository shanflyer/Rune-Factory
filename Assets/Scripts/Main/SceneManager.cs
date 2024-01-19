using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : Singleton<SceneManager>
{
    public override bool NeedUpdata => true;
    private string nowSceen;

    public string Now
    {
        get
        {
            if (string.IsNullOrEmpty(nowSceen))
            {
                nowSceen = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }
            return nowSceen;
        }
    }

    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<SwitchScene>(SwitchScene);
    }

    private async void SwitchScene(SwitchScene switchScene)
    {
        GameActionData beforeActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(switchScene.beforeLoadActionId);
        GameActionData afterActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(switchScene.afterLoadActionId);

        SwitchScene(switchScene.sceneName, beforeActionData != null ? () => { beforeActionData.Action(0, 0); }
        : null,
            afterActionData != null ? () => { afterActionData.Action(0, 0); }
        : null);
    }

    public async void SwitchScene(string sceneName, Action beforeLoadSceneAction = null, Action afterSceneAction = null)
    {
        this.loadSceneAction = afterSceneAction;

        if (beforeLoadSceneAction != null)
        {
            beforeLoadSceneAction.Invoke();
        }
        if (!string.IsNullOrEmpty(nowSceen))
        {
            //UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(nowSceen);
            nowSceen = null;
        }

        this.AsyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        //loadingPanel=await UIManager.instance.ShowGamePanel<LoadingPanel>();
        nowSceen = sceneName;
    }

    public void UnloadNowScene()
    {
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(nowSceen);
        nowSceen = null;
    }

    private AsyncOperation AsyncOperation;
    private Action loadSceneAction;

    protected override void UpData()
    {
        base.UpData();
        if (AsyncOperation == null)
        {
            return;
        }
        // if (loadingPanel != null)
        //     loadingPanel.RefreshLoadValue(AsyncOperation.progress);
        if (AsyncOperation.progress >= 1)
        {
            if (loadSceneAction != null)
            {
                loadSceneAction.Invoke();
            }
            loadSceneAction = null;
            // if (loadingPanel != null)
            {
                //    loadingPanel.Close();
            }

            //loadingPanel = null;
            AsyncOperation = null;
        }
    }
}