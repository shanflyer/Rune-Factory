
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("NewGame/Common")]
[TaskName("消耗物体")]
public class ItemCostBehavior : Action
{
    public SharedInt characterId;
    public SharedString title;
    public SharedString description;

    public PayType payType;
    public int money;
    public List<int2> costItems = new List<int2>();

    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        List<MyInt3> items = new List<MyInt3>();
        for(int i = 0; i < costItems.Count; i++)
        {
            int2 costItem = costItems[i];
            int totalCount = PackageManager.instance.GetPlayerItemCount(costItem.x);
            items.Add(new MyInt3
            {
                value = new int3(costItem.xy, totalCount)
            });
        }
        ItemCostEventData itemCostEventData = new ItemCostEventData
        {
            title = title.Value,
            notice = description.Value,
            costValue = money,
            payType = payType,
            items = items,
            afterAction=SetResult
        };
        UIManager.instance.ShowGamePanel<ItemCostSelectPanel, ItemCostEventData>(itemCostEventData);
    }
    void SetResult(bool result)
    {
        if (result)
        {
            taskStatus = TaskStatus.Success;
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