using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using static MapCellController;
using System.Collections.Generic;
using Unity.Mathematics;

[TaskCategory("Game/Character")]
[TaskName("获取角色周围特定格子")]
public class GetACoordinateForCharacter : Action
{
    [Header("角色id")]
    private SharedInt characterId;
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
	private SharedInt3List results;
	public override void OnStart()
	{
        if (characterId==null|| characterId.IsNull())
        {
            characterId= (SharedInt)Owner.GetVariable("CharacterId");
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
        Character character = CharacterManager.instance.GetCharacter(characterId.Value);
        RuntimeMapRoom runtimeMapRoom;
        if (MapCellController.instance.GetRuntimeMapRoom(character.objCoordinate.mapInstance, out runtimeMapRoom))
        {
            var rangeCoordinates = runtimeMapRoom.roomCellData.GetCoordinates(character.objCoordinate.coordinate,
                minRange.Value, maxRange.Value, isWalkable.Value);
            GameRandomData gameRandomData = new GameRandomData
            {
                id = -1,
                weightRandom = true,
                barrels = new List<WeightBarrel>(),
                randomItems = new List<RandomItem>(),
                text = "选择目标"
            };
            for (int i = 0; i < rangeCoordinates.Count; i++)
            {
                RandomItem randomItem = new RandomItem
                {
                    itemId = i,
                    itemValue = i.ToString(),
                    randomValue = 10,
                    maxCount = 1,
                    minCount = 1
                };
                gameRandomData.randomItems.Add(randomItem);

            }
            gameRandomData.Pretreatment();

            var randomResults= GameRandom.instance.GetRandomValue(gameRandomData, randomResultCount: resultCount.Value);
            if (randomResults.Count == 0)
            {
                return TaskStatus.Failure;
            }
            else
            {
                List<int3> resultValue = new List<int3>();
                for(int i = 0; i < randomResults.Count; i++)
                {
                    int index =int.Parse(randomResults[i].result);
                    int2 coordinate = rangeCoordinates[index];
                    resultValue.Add(new int3(coordinate,character.objCoordinate.mapInstance));
                }
                results.Value=resultValue;
            }

            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
	}
}