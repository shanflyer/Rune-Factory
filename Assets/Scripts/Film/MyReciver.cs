using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MyReciver : MonoBehaviour,INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if(notification != null&&  notification is MyMarker)
        {
            MyMarker myMarker = notification as MyMarker;
            for(int i = 0; i < myMarker.gameActionDatas.Count; i++)
            {
                var data = myMarker.gameActionDatas[i];
                data.Action();
            }
        }
    }

     
}
