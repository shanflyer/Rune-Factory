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
    public void ShowCoin(ShowCoin ShowCoin)
    {
        GameRuntimeObjManager.instance.CreateRuntimeObj(RuntimeObjType.STOREITEM.ToString(), "Coin", CoinPrefab, 0,setComponent:(RuntimeObj coinRuntimeObj) =>
        {
            Animation animation = coinRuntimeObj.obj as Animation;
            animation.transform.position = ShowCoin.pos;
            animation.Play();
            AudioController.instance.PlayAudio(SE.coinitem_acquired01);
            GameTimerController.instance.DelayAction(GameCommon.storeCoinTime, () =>
            {
                GameRuntimeObjManager.instance.RecycleRuntimeObj(coinRuntimeObj);
            });
        }); 
    }
}