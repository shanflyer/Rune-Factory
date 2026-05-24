using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace MyGame
{
    public class SceneManager : Singleton<SceneManager>
    {
        public override bool NeedUpdate => true;
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
            GameActionManager.instance.AddAsyncListener<SwitchScene>(SwitchSceneAsync, nameof(SwitchScene));
        }

        private async System.Threading.Tasks.Task SwitchSceneAsync(SwitchScene switchScene)
        {
            GameActionAsset beforeActionData = await GameDataManager.instance.GetAsyncData<GameActionAsset>(switchScene.beforeLoadActionId);
            GameActionAsset afterActionData = await GameDataManager.instance.GetAsyncData<GameActionAsset>(switchScene.afterLoadActionId);

            SwitchScene(switchScene.sceneName, beforeActionData != null ? () => { beforeActionData.Action(0, 0); }
            : null,
                afterActionData != null ? () => { afterActionData.Action(0, 0); }
            : null);
        }

        public void SwitchScene(string sceneName, Action beforeLoadSceneAction = null, Action afterSceneAction = null)
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

        public void UnloadNowScene(bool Async = true)
        {
            AsyncTaskRunner.Run(() => UnloadNowSceneAsync(Async), nameof(UnloadNowScene));
        }

        public async System.Threading.Tasks.Task UnloadNowSceneAsync(bool Async = true)
        {
            if (!string.IsNullOrEmpty(nowSceen))
            {
                if (Async)
                {
                    await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(nowSceen);
                }
                else
                {
                    // Unity 6 已废弃同步卸载，非异步分支也统一等待异步卸载完成。
                    await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(nowSceen);
                }
                nowSceen = null;
            }
        }

        private AsyncOperation AsyncOperation;
        private Action loadSceneAction;

        protected override void Update()
        {
            base.Update();
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
}
