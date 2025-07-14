using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public struct CharacterEquipment : IComponentData
{
    public int2 weapon;
    public int2 clothes;
    public int2 shoes;
    public int2 headgear;
}