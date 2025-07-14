using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public struct CharacterTransform:IComponentData
{
    public int3 objCoordinate;
    public int2 forwardCoordinate;
    public float2 moveDirection;
    public Direction direction;
    public bool canMove;
}
public struct CharacterAnimator : IComponentData
{

}