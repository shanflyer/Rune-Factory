using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/PlayerStore")]
[TaskName("获取柜台可行走范围坐标")]
public class GetStoreCounterPos : Action
{ 
    public SharedInt3 targetCoordinate;
    public SharedInt selectStoreCounterId;
    public SharedInt2 faceTargetCoordinate;
    public override void OnStart()
    {
        if (selectStoreCounterId == null)
        {
            selectStoreCounterId = (SharedInt)Owner.GetVariable("SelectStoreCounterId");
            if (selectStoreCounterId == null)
            {
                selectStoreCounterId = new SharedInt();
                Owner.SetVariable("TargetCoordinate", selectStoreCounterId);
            }
        }
        if (targetCoordinate == null)
        {
            targetCoordinate = (SharedInt3)Owner.GetVariable("TargetCoordinate");
            if (targetCoordinate == null)
            {
                targetCoordinate = new SharedInt3();
                Owner.SetVariable("TargetCoordinate", targetCoordinate);
            }
        }
        if (faceTargetCoordinate == null)
        {
            faceTargetCoordinate = (SharedInt2)Owner.GetVariable("FaceTargetCoordinate");
            if (faceTargetCoordinate == null)
            {
                faceTargetCoordinate = new SharedInt2();
                Owner.SetVariable("FaceTargetCoordinate", faceTargetCoordinate);
            }
        }

        SelectItemGrid();
    } 
    async void SelectItemGrid()
    {
        taskStatus = TaskStatus.Running;
        if (WorldMapManager.instance.GetRuntimeMapItem(selectStoreCounterId.Value, out var runtimeMapItem))
        {
            faceTargetCoordinate.Value = runtimeMapItem.coordinate;

            int mapInstance = runtimeMapItem.mapInstanceId;
            MapItemData mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(runtimeMapItem.dataId);
            var triggerCells= mapItemData.triggerCells;

            List<int2> walkableCells = new List<int2>();
            for(int i = 0; i < triggerCells.Length; i++)
            {
                if (MapCellController.instance.CheckIsWalk(triggerCells[i], mapInstance))
                {
                    walkableCells.Add(triggerCells[i]);
                }
            }
            if (walkableCells.Count == 0)
            {
                taskStatus = TaskStatus.Failure;
            }
            else
            {
                int index = GameRandom.RandomInt(0, walkableCells.Count);
                int2 coordinate = walkableCells[index] + runtimeMapItem.coordinate;
                targetCoordinate.Value = new int3(coordinate.xy, mapInstance);
                taskStatus = TaskStatus.Success; 
            } 
        }
        else
        {
            taskStatus = TaskStatus.Failure; 
        }
    }

    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
	{
        return taskStatus;
    }
}