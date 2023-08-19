using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightController : MonoBehaviour
{
    RuntimeObj fightMapRuntime0, fightMapRuntime1;
    private void OnEnable()
    {
        GameRuntimeObjManager.instance.CreatParent<FightRuntimeObjType>(transform);
    }
    private void OnDestroy()
    {
        GameRuntimeObjManager.instance.ClearRuntime<FightRuntimeObjType>();
    }
    float cycleSize;
    Vector3 cyclePos;
    public async void CreatFightMap(int id)
    {
        var fightMapData =await GameDataManager.instance.GetAsyncObjectDataArray<FightMapData>(id.ToString());
        fightMapRuntime0 = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.FIGHTMAP.ToString(), id.ToString(), fightMapData.fightMapObj, 0);
        fightMapRuntime1 = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.FIGHTMAP.ToString(), id.ToString(), fightMapData.fightMapObj, 1);

        Vector3 zeroPos = new Vector3(0, fightMapData.offsetY, 0);
        cyclePos = new Vector3(fightMapData.cycleSize, fightMapData.offsetY, 0);

        cycleSize = fightMapData.cycleSize;
        fightMapRuntime0.obj.transform.localPosition = zeroPos;
        fightMapRuntime1.obj.transform.localPosition = cyclePos;
    }
    public async void CreatFightPlayer()
    {

    }

    public void StartWalk()
    {
        StartCoroutine(MapMoving());
    }
    public void StopWalk()
    {
        StopAllCoroutines();
    }
    IEnumerator MapMoving()
    {
        var wait = new WaitForFixedUpdate();
        Vector3 late = new Vector3(-GameCommon.fightMapMovingSpeed, 0, 0);
        while (true)
        {
            fightMapRuntime0.obj.transform.Translate(late);
            fightMapRuntime1.obj.transform.Translate(late);
            if (fightMapRuntime0.obj.transform.localPosition.x <= -cycleSize)
            {
                fightMapRuntime0.obj.transform.localPosition = cyclePos;
            }
            if (fightMapRuntime1.obj.transform.localPosition.x <= -cycleSize)
            {
                fightMapRuntime1.obj.transform.localPosition = cyclePos;
            }
            yield return wait;
        }
    }
}
