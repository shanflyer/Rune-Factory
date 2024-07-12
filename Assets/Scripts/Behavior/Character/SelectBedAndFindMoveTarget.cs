using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using System.Collections.Generic; 

[TaskCategory("Game/Character")]
[TaskName("选择床并找到移动的目标")]
public class SelectBedAndFindMoveTarget: Action
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
    [Header("床移动目标")]
    [SerializeField]
    private SharedInt3 targetCoordinate;
    private int2 SelectBed;
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        taskStatus = TaskStatus.Failure;
        if (NPCManager.instance.GetNPC(characterId.Value,out NPC npc))
        {
            List<int2> beds = new List<int2>();
            for(int i = 0; i < npc.Beds.Count; i++)
            {
                CheckMapEditorItemLinkCharacter checkMapEditorItemLinkCharacter = new CheckMapEditorItemLinkCharacter
                {
                    mapId = npc.Beds[i].x,
                    itemEditorId = npc.Beds[i].y,
                    setResult = (bool value) =>
                    { 
                        if (value)
                        {
                            beds.Add(npc.Beds[i]);
                        }
                    }
                };
                GameActionManager.instance.QueueAction(checkMapEditorItemLinkCharacter, true);
            } 
            
            if (beds.Count > 0)
            {
                taskStatus = TaskStatus.Running;

                int index=GameRandom.RandomInt(0,beds.Count);
                SelectBed = beds[index];
                SetMapEditorItemLinkCharacter setMapEditorItemLinkCharacter = new SetMapEditorItemLinkCharacter
                {
                    mapId = SelectBed.x,
                    mapItemEditorId = SelectBed.y,
                    linkInstanceId = characterId.Value,
                    setResult= SetMapEditorItemLinkResult
                };
                GameActionManager.instance.QueueAction(setMapEditorItemLinkCharacter);
            }
        } 

    }
    void SetMapEditorItemLinkResult(bool value)
    {
        if (value)
        {
            if(WorldMapManager.instance.GetMapItemPos(SelectBed, out var coordinate))
            {
                if (MapCellController.instance.GetCoordinates(SelectBed.x, coordinate.xy, minRange.Value, maxRange.Value, true, out var rangeCoordinates))
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
                        targetCoordinate =new int3(rangeCoordinates[index],SelectBed.x); 
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