using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MyReciver : MonoBehaviour,INotificationReceiver
{
   // public int showId;
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if(notification != null&&  notification is MyGameMarker)
        {
            MyGameMarker myMarker = notification as MyGameMarker;
            for(int i = 0; i < myMarker.gameActionDatas.Count; i++)
            {
                var data = myMarker.gameActionDatas[i];
#if UNITY_EDITOR
                Debug.Log($"film Action:{data.name}");
#endif
                data.Action();
            }
        }
    }

     
}
