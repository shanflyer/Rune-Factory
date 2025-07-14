using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

public partial struct CharacterEquipSystem:ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CharacterEquipment>();
        state.RequireForUpdate<CharacterProperty>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var setCount =(int)CharacterEquipSpawnBridge.instance.Queue.EstimateCount();
        if (setCount < 12)
        {
            NativeHashMap<Entity, SetCharacterEquipment> setCharacterEquipmentDic = new NativeHashMap<Entity, SetCharacterEquipment>(setCount * 2, Allocator.Temp);
            foreach((var characterEquipment, var CharacterProperty, var Entity) in SystemAPI.Query<RefRW<CharacterEquipment>,RefRW<CharacterProperty>>().WithEntityAccess())
            {
                if(setCharacterEquipmentDic.TryGetValue(Entity,out var setCharacterEquipment))
                {

                }
            }
            setCharacterEquipmentDic.Dispose();
        }
    }
}