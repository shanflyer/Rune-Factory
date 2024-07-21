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
    public override async void Init()
    {
        base.Init();
        CoinPrefab =await GameSourceManager.instance.GetComponent<Animation>(DataPath.StoreCoinPrefab);
        GameActionManager.instance.AddListener<ShowCoin>(ShowCoin);
    }
    public async void ShowCoin(ShowCoin ShowCoin)
    {
        var coinRuntimeObj =await GameRuntimeObjManager.instance.CreatRuntimeObj<Animation>(RuntimeObjType.STOREITEM.ToString(), "Coin", CoinPrefab, 0);
        Animation animation = coinRuntimeObj.obj as Animation;
        animation.transform.position = ShowCoin.pos;
        animation.Play();
         
        GameTimerController.instance.DeleyActionMain(GameCommon.storeCoinTime, () =>
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(coinRuntimeObj);
        });
    }
}