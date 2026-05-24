using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public struct EmoteComponent:IComponentData,IEnableableComponent
{
    public int emoteId;
    public float showTime;
    public float currentTime;
    public EntityType entityType;
    public int entityId;
}
