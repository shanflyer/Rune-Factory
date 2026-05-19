using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class MyGameMarker : Marker, INotification, INotificationOptionProvider
{
    [SerializeField] bool m_Retroactive;
    [SerializeField] bool m_EmitOnce;
    public bool retroactive
    {
        get { return m_Retroactive; }
        set { m_Retroactive = value; }
    }

    /// <summary>
    /// Use emitOnce to emit this signal once during loops.
    /// </summary>
    public bool emitOnce
    {
        get { return m_EmitOnce; }
        set { m_EmitOnce = value; }
    }
    public List<GameActionAsset> gameActionDatas = new List<GameActionAsset>();
   
    public PropertyName id => throw new System.NotImplementedException();

    public NotificationFlags flags
    {
        get
        {
            return (retroactive ? NotificationFlags.Retroactive : default(NotificationFlags)) |
                (emitOnce ? NotificationFlags.TriggerOnce : default(NotificationFlags)) |
                NotificationFlags.TriggerInEditMode;
        }
    }

    
}
