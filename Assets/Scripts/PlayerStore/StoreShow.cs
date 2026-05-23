using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class StoreShow:Singleton<StoreShow>
{
    //private Transform ShowParent;
    private Animation CoinPrefab;
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        CoinPrefab =await GameSourceManager.instance.GetComponent<Animation>(DataPath.StoreCoinPrefab);
        if (CoinPrefab == null)
        {
            Debug.LogError($"StoreShow init failed: missing coin prefab '{DataPath.StoreCoinPrefab}'.");
            return;
        }
        GameActionManager.instance.AddListener<ShowCoin>(ShowCoin);
    }

    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
        base.Clear();
    }
    public async void ShowCoin(ShowCoin ShowCoin)
    {
        var coinRuntimeObj =await GameRuntimeObjManager.instance.CreatRuntimeObj<Animation>(RuntimeObjType.STOREITEM.ToString(), "Coin", CoinPrefab, 0);
        Animation animation = coinRuntimeObj.obj as Animation;
        animation.transform.position = ShowCoin.pos;
        animation.Play();
        AudioController.instance.PlayAudio(SE.coinitem_acquired01);
        GameTimerController.instance.DelayAction(GameCommon.storeCoinTime, () =>
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(coinRuntimeObj);
        });
    }
}
