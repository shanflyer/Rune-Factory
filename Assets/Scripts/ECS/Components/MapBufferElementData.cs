using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public struct MapLink : IBufferElementData
{
    public int targetMap;
    public int2 targetPos;
    public int targetDirection;
    public int3 linkAction;
}