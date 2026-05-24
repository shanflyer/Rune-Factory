using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//计划任务模板
public class TaskScheduleModelDataList : ScriptableObject, IGameData,IDataArray<TaskScheduleModelData>
{
    public TaskScheduleModelData[] taskScheduleModelDatas;
    public override string ToString()
    {
        return "TaskScheduleModelDataList";
    }


    public TaskScheduleModelData[] DataList => taskScheduleModelDatas;
    public string GetKey()
    {
        return "TaskScheduleModelDataList";
    }
#if UNITY_EDITOR

    TaskScheduleModelEditorData[] TaskScheduleModelEditorDatas;
    public void SetReferenceData()
    {
        var growModelDataList = ExtensionsResources.LoadResource<GrowModelDataList>(DataPath.GetDataPath(typeof(GrowModelData)));
        Dictionary<int, GrowModelData> GrowModelDataDic = new Dictionary<int, GrowModelData>();
        for(int i = 0; i < growModelDataList.DataList.Length; i++)
        {
            GrowModelDataDic.Add(growModelDataList.DataList[i].id, growModelDataList.DataList[i]);
        }

        List<TaskScheduleModelData> taskScheduleModelDataList = new List<TaskScheduleModelData>();
        for (int i = 0; i < TaskScheduleModelEditorDatas.Length; i++)
        {
            var data = TaskScheduleModelEditorDatas[i];
            var taskScheduleModelData = taskScheduleModelDataList.Find(t=>t.id==data.id);
            if (taskScheduleModelData==null)
            {
                taskScheduleModelData = new TaskScheduleModelData
                {
                    id = data.id,
                    modelName = data.modelName,
                    gameTimeKey = new GameTimeKey(data.gameTimeRange)
                };
                taskScheduleModelDataList.Add(taskScheduleModelData);
            }

            DailyTaskDataItem dailyTaskDataItem = new DailyTaskDataItem
            {
                itemValue = data.itemValue,
                weight = data.weight,
            };
            if(GrowModelDataDic.TryGetValue(data.growCurve,out var d))
            {
                dailyTaskDataItem.growCurve = d.curve;
            }

            taskScheduleModelData.dailyTaskDataItems.Add(dailyTaskDataItem);
        }
        taskScheduleModelDatas = taskScheduleModelDataList.ToArray();
    }


#endif
}
[Serializable]
public class TaskScheduleModelData : IGameData
{
    public string modelName;
    public int id;
    public GameTimeKey gameTimeKey;

    public List<DailyTaskDataItem> dailyTaskDataItems = new List<DailyTaskDataItem>();

    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
    public void SetReferenceData()
    {
       // throw new NotImplementedException();
    }
}
#if UNITY_EDITOR
public struct TaskScheduleModelEditorData
{
    public int id;
    public string modelName;
    public int[] gameTimeRange;
    public string taskName;
    public int itemValue;
    public int weight;
    public int growCurve;
}
#endif


[Serializable]
public class DailyTaskDataItem
{
    public int itemValue;
    public int weight;
    public GameActionAsset GameActionData;
    public AnimationCurve growCurve;
    public DailyTaskDataItem() { }

    public int2 GetNowTaskRandomValue(float value)
    {
        return new int2(itemValue, (int)(weight * growCurve.Evaluate(value)));
    }
}
