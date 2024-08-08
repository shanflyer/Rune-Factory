using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static MapCellController;

[TaskCategory("Game/Map")]
[TaskName("获取个体周围特定格子")]
public class GetACoordinateForCharacter : Action
{
    [Header("个体id")]
    public SharedInt characterId;

    public SharedInt2 itemEditorKey;

    [Header("个体类型")]
    public EntityType entityType;

    [Header("最小范围")]
    public SharedInt minRange;

    [Header("最大范围")]
    public SharedInt maxRange;

    [Header("试图获取的数量")]
    public SharedInt resultCount;

    //[Header("是否有其他角色")]
    //public SharedBool isHaveOther;
    [Header("是否可行走")]
    public SharedBool isWalkable;

    [Header("获取的结果")]
    public SharedInt3List results;

    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }

        if (results == null)
        {
            results = (SharedInt3List)Owner.GetVariable("CoordinateResults");
            if (results == null)
            {
                results = new SharedInt3List();
                Owner.SetVariable("CoordinateResults", results);
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        int2 coordinate = int2.zero;
        int mapInstance = 0;
        switch (entityType)
        {
            case EntityType.地图道具:
                if (!WorldMapManager.instance.GetMapItemPos(characterId.Value, out var objCoordinate))
                {
                    WorldMapManager.instance.GetMapItemPos(itemEditorKey.Value, out objCoordinate);
                }
                coordinate = objCoordinate.xy;
                mapInstance = objCoordinate.z;
                break;

            case EntityType.角色:
                Character character = CharacterManager.instance.GetCharacter(characterId.Value);
                coordinate = character.coordinate;
                mapInstance = character.mapInstance;
                break;
        }
         
        if (MapCellController.instance.GetCoordinates(mapInstance, coordinate, minRange.Value, maxRange.Value, isWalkable.Value, out var rangeCoordinates))
        {
            GameRandomData gameRandomData = new GameRandomData
            {
                id = -1,
                weightRandom = true,
                barrels = new List<int3>(),
                randomItems = new List<RandomItem>(),
                text = "选择目标"
            };
            for (int i = 0; i < rangeCoordinates.Count; i++)
            {
                RandomItem randomItem = new RandomItem
                { 
                    itemValue = i,
                    randomValue = 10,
                    maxCount = 1,
                    minCount = 1
                };
                gameRandomData.randomItems.Add(randomItem);
            }
            gameRandomData.Pretreatment();

            var randomResults = GameRandom.instance.GetRandomValue(gameRandomData, randomResultCount: resultCount.Value);
            if (randomResults.Count == 0)
            {
                return TaskStatus.Failure;
            }
            else
            {
                List<int3> resultValue = new List<int3>();
                for (int i = 0; i < randomResults.Count; i++)
                {
                    int index = randomResults[i].x;
                    int2 targetCoordinate = rangeCoordinates[index];
                    resultValue.Add(new int3(targetCoordinate, mapInstance));
                }
                results.Value = resultValue;
            }

            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }
}