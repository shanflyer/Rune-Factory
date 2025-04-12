using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using System.Collections.Generic; 

public enum BindItemType
{
    床,工作台
}
[TaskCategory("Game/Character")]
[TaskName("选择Npc绑定物体并找到移动的目标")]
public class SelectBlindItemAndFindMoveTarget: Action
{
    [Header("角色ID")]
    [SerializeField]
    private SharedInt characterId;
    [Header("最小范围")]
    [SerializeField]
    private SharedInt minRange; 
    [Header("最大范围")]
    [SerializeField]
    private SharedInt maxRange;
    [SerializeField]
    BindItemType selectBindItemType;

    [Header("移动目标")]
    [SerializeField]
    private SharedInt3 targetCoordinate;
    [SerializeField]
    private SharedInt2 SelectItem;
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        taskStatus = TaskStatus.Running;
        if (NPCManager.instance.GetNPCFormInstance(characterId.Value,out NPC npc))
        {
            List<int2> items = new List<int2>();

            List<int2> checkItems = npc.Beds;
            if(selectBindItemType== BindItemType.工作台)
            {
                checkItems = npc.WorkItems;
            }
            //Debug.LogWarning($"尝试寻找:{npc.Character.name}");
            for (int i = 0; i < checkItems.Count; i++)
            {
                int index = i;
                CheckMapEditorItemLinkCharacter checkMapEditorItemLinkCharacter = new CheckMapEditorItemLinkCharacter
                { 
                    mapId = checkItems[index].x,
                    itemEditorId = checkItems[index].y,
                    characterId=characterId.Value,
                    setResult = (bool value) =>
                    { 
                        if (value)
                        {
                            items.Add(checkItems[index]);
                            if (index >= checkItems.Count - 1)
                            {
                                if (items.Count > 0)
                                {
                                    taskStatus = TaskStatus.Running;

                                    int index = GameRandom.RandomInt(0, items.Count);
                                    SelectItem.Value = items[index];
                                    SetMapEditorItemLinkCharacter setMapEditorItemLinkCharacter = new SetMapEditorItemLinkCharacter
                                    {
                                        mapId = SelectItem.Value.x,
                                        mapItemEditorId = SelectItem.Value.y,
                                        linkInstanceId = characterId.Value,
                                        setResult = SetMapEditorItemLinkResult
                                    };
                                    GameActionManager.instance.QueueAction(setMapEditorItemLinkCharacter, true);
                                }
                                else
                                {
                                    taskStatus = TaskStatus.Failure;
                                }
                            }
                        }
                        else
                        {
                            taskStatus = TaskStatus.Failure;
                        }
                       
                    }
                };
                GameActionManager.instance.QueueAction(checkMapEditorItemLinkCharacter, true);
            }  
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }

    }
    void SetMapEditorItemLinkResult(bool value)
    {
        if (value)
        {
            if(WorldMapManager.instance.GetMapItemPos(SelectItem.Value, out var coordinate))
            {
                if(maxRange.Value<=0)
                {
                   int2 cell= WorldMapManager.instance.GetItemCommonCenterTriggerCellForEditorInstance(SelectItem.Value.x, SelectItem.Value.y);
                    if (cell.x > int.MinValue)
                    {
                        targetCoordinate.Value = new int3(cell, SelectItem.Value.x);
                        taskStatus = TaskStatus.Success;
                        return; 
                    }
                }
                if (MapCellController.instance.GetCoordinates(SelectItem.Value.x, coordinate.xy, minRange.Value, maxRange.Value, true, out var rangeCoordinates))
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

                    var randomResults = GameRandom.instance.GetRandomValue(gameRandomData, randomResultCount: 1);
                    if (randomResults.Count >= 0)
                    {
                        int index = randomResults[0].x;
                        targetCoordinate.Value = new int3(rangeCoordinates[index], SelectItem.Value.x);
                        taskStatus = TaskStatus.Success;
                        return;
                    }
                }
            } 
        }
        taskStatus = TaskStatus.Failure;
    }
    TaskStatus taskStatus= TaskStatus.Failure;
    public override TaskStatus OnUpdate()
    { 
        return taskStatus;
    }
}