using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Map")]
[TaskName("获取地图最近的物体")]
public class GetNearestItem : Action
{
    [SerializeField] private SharedInt characterId;
    [SerializeField] private SharedIntList items;


    [Header("获取的结果")] [SerializeField] private SharedInt result;

    public override void OnStart()
    {
    }

    public override TaskStatus OnUpdate()
    {
        var cell = int3.zero;

        if (characterId != null)
        {
            var character = CharacterManager.instance.GetCharacter(characterId.Value);

            var itemSet = new HashSet<int>();
            for (var i = 0; i < items.Value.Count; i++) itemSet.Add(items.Value[i]);

            int value =
                WorldMapManager.instance.GetNearestItem(character.mapInstance, itemSet, character.coordinate);
            if (value != 0)
            {
                result.Value = value;
                return TaskStatus.Success;
            }

        }


        return TaskStatus.Failure;
    }
}
