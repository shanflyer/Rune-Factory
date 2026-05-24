using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SceneInfoManager : Singleton<SceneInfoManager>
{
    private SceneInfo sceneInfoPre;
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;

    public void DisplaySceneInfo(string str,Vector3 pos)
    {
        AsyncTaskRunner.Run(() => DisplaySceneInfoAsync(str, pos), nameof(DisplaySceneInfo));
    }

    public async System.Threading.Tasks.Task DisplaySceneInfoAsync(string str,Vector3 pos)
    {
        var runtimeSceneInfo =await GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.OTHER.ToString(),
           "", sceneInfoPre, 0);
        SceneInfo sceneInfo = runtimeSceneInfo.obj as SceneInfo;
        sceneInfo.transform.position = pos;
        sceneInfo.SetTextValue(str);



        GameTimerController.instance.DelayAction(500, () =>
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeSceneInfo);
        });
    }
    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
        base.Clear();
    }
    
    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        GameObject infoPre = await GameSourceManager.instance.GetPrefab(DataPath.sceneInfoPath);
        if (infoPre == null)
        {
            Debug.LogError($"SceneInfoManager init failed: missing prefab at {DataPath.sceneInfoPath}");
            return;
        }

        sceneInfoPre = infoPre.GetComponent<SceneInfo>();
        if (sceneInfoPre == null)
        {
            Debug.LogError($"SceneInfoManager init failed: prefab has no SceneInfo at {DataPath.sceneInfoPath}");
        }
    }
}
