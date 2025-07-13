using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Map")]
[TaskName("获取个体坐标")]
public class GetEntityCoordinate : Action
{
    [Header("保存坐标的共享变量")]
    public SharedInt3 targetCoordinate;
    [Header("地图")]
    [SerializeField]
    private SharedInt mapInstance;
    [Header("坐标")]
    [SerializeField]
    private SharedInt2 coordinate;

    [Header("个体id")]
    public SharedInt entityId; 

    public SharedInt2 itemEditorKey;

    public EntityType entityType;

    private bool getSuccess;

    // Start is called before the first frame update
    public override void OnStart()
    {
        if ((entityId == null || entityId.IsNull()) && (itemEditorKey == null || itemEditorKey.IsNull()))
        {
            getSuccess = false;
            return;
        }

        int3 coordinate = int3.zero;
        switch (entityType)
        {
            case EntityType.地图道具:
                if (itemEditorKey.IsNull())
                {
                    getSuccess = WorldMapManager.instance.GetMapItemPos(entityId.Value, out coordinate);
                }
                else
                {
                    getSuccess = WorldMapManager.instance.GetMapItemPos(itemEditorKey.Value, out coordinate);
                }

                break;

            case EntityType.角色:
                getSuccess = CharacterManager.instance.GetCharacterCoordinate(entityId.Value, out coordinate);
                break;
        }
        targetCoordinate.Value = coordinate;
        if (mapInstance != null)
            mapInstance.SetValue(coordinate.z);
        if (this.coordinate != null)
        {
            this.coordinate.SetValue(coordinate.xy);
        }
    }

    public override TaskStatus OnUpdate()
    {
        return getSuccess ? TaskStatus.Success : TaskStatus.Failure;
    }
}