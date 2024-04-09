using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine; 

[TaskCategory("Game/Map")]
[TaskName("获取周围特定格子")]
public class GetCellsForCell : Action
{ 
    public SharedInt2List cells;

    public SharedInt2 centerCell; 

    [Header("最小范围")]
    public SharedInt minRange;

    [Header("最大范围")]
    public SharedInt maxRange;
      

    [Header("获取的结果")]
    public SharedInt3 result;
    public SharedInt room;

    public override void OnStart()
    {
        
    }

    public override TaskStatus OnUpdate()
    {

        GameRandomData gameRandomData = new GameRandomData
        {
            id = -1,
            weightRandom = true,
            barrels = new List<WeightBarrel>(),
            randomItems = new List<RandomItem>(),
            text = "选择目标"
        };
        var cellList = cells.Value;
        for (int i = 0; i < cellList.Length; i++)
        {
            var cell = cellList[i];
            int length = math.abs(cell.x - centerCell.Value.x) + math.abs(cell.y - centerCell.Value.y);
            if (length <= maxRange.Value && length >= minRange.Value)
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
        }
        gameRandomData.Pretreatment();

        var randomResults = GameRandom.instance.GetRandomValue(gameRandomData);
        if (randomResults.Count == 0)
        {
            return TaskStatus.Failure;
        }
        else
        {
            result.SetValue(new int3(cellList[int.Parse(randomResults[0].result)],room.Value));
        }

        return TaskStatus.Success; 
    }
}