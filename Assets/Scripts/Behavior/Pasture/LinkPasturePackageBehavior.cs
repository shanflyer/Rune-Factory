using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks; 
using UnityEngine;
using Unity.Mathematics;
[TaskCategory("Game/牧场")]
[TaskName("链接牧场package")]
public class LinkPasturePackageBehavior : Action
{
    [SerializeField]
    private SharedInt pastureInstance;
    [SerializeField]
    private SharedInt roomId;
    [SerializeField]
    private SharedInt foodPackage, waterPackage, productPackage;
    public bool editorId;
    public override void OnStart()
    {
        int foodPackageId = foodPackage.Value;
        int waterPackageId = waterPackage.Value;
        int productPackageId = productPackage.Value;
        if (editorId)
        {
            foodPackageId = WorldMapManager.instance.GetInstanceFromEditorId(new int2(roomId.Value, foodPackageId));
            waterPackageId = WorldMapManager.instance.GetInstanceFromEditorId(new int2(roomId.Value, waterPackageId));
            productPackageId = WorldMapManager.instance.GetInstanceFromEditorId(new int2(roomId.Value, productPackageId));
        }
        LinkPasturePackage linkPasturePackage = new LinkPasturePackage
        {
            pastureInstance = pastureInstance.Value,
            foodPackage = foodPackageId,
            waterPackage = waterPackageId,
            productPackage = productPackageId
        };
        GameActionManager.instance.QueueAction(linkPasturePackage);

        taskStatus = TaskStatus.Success;
    }

    private TaskStatus taskStatus;

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}