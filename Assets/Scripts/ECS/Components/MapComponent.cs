using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public struct MapComponent:IComponentData
{
    public int id;
    public int3 coordinate;
    public int roomCellDataIndex;
}
