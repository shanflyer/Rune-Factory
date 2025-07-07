using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInfoManager : Singleton<SceneInfoManager>
{
    private SceneInfo sceneInfoPre;
    public void DisplaySceneInfo(string str,Vector3 pos)
    {
          GameRuntimeObjManager.instance.CreateRuntimeObj(FightRuntimeObjType.OTHER.ToString(),
           "", sceneInfoPre, 0,setComponent:(RuntimeObj runtimeSceneInfo) => 
           {
               SceneInfo sceneInfo = runtimeSceneInfo.obj as SceneInfo;
               sceneInfo.transform.position = pos;
               sceneInfo.SetTextValue(str);

               GameTimerController.instance.DelayAction(500, () =>
               {
                   GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeSceneInfo);
               });
           }); 
       
    }
    protected override void Clear()
    {
        base.Clear();
    }
    
    public override async void Init()
    {
        base.Init();
        GameObject infoPre = await GameSourceManager.instance.GetPrefab(DataPath.sceneInfoPath);
        sceneInfoPre = infoPre.GetComponent<SceneInfo>();
    }
}
