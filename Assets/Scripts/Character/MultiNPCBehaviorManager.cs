using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MultiNPCGroup
{
    public int instanceId;
    public MulitiBehaviorData mulitiBehaviorData;
    public BehaviorTree behaviorTree;
    public MyDic<int, NPCBehaviorTempData> npcBehaviorTempDatas;

    //public int mapInstance;
    // public int2 center;
    public MultiNPCGroup(int instanceId, MulitiBehaviorData mulitiBehaviorData)
    {
        this.instanceId = instanceId;
        this.mulitiBehaviorData = mulitiBehaviorData;
    }

    public void InitBehavior(BehaviorTree behaviorTree, MyDic<int, NPCBehaviorTempData> npcBehaviorTempDatas)
    {
        this.npcBehaviorTempDatas = npcBehaviorTempDatas;
        this.behaviorTree = behaviorTree;
        SharedIntList sharedList = new SharedIntList();
        sharedList.SetValue(npcBehaviorTempDatas.GetKeyList());
        behaviorTree.SetVariable("GroupCharacters", sharedList);
        SharedInt sharedInt = new SharedInt();
        sharedInt.SetValue(instanceId);
        behaviorTree.SetVariable("GroupID", sharedInt);

        behaviorTree.OnBehaviorEnd += BehaviorEndAction;
        behaviorTree.Start();
    }

    public bool IsHaveCharacter(int characterId)
    {
        return npcBehaviorTempDatas.ContainsKey(characterId);
    }

    private void BehaviorEndAction(Behavior behavior)
    {
        if (mulitiBehaviorData.endAction == 0)
        {
            GameActionDataManager.instance.Action(mulitiBehaviorData.endAction);
        }
        if (mulitiBehaviorData.needRecoverSingleBehavior)
        {
            var npcs = npcBehaviorTempDatas.GetKeyList();
            var tempDatas = npcBehaviorTempDatas.GetValueList();
            for (int i = 0; i < npcs.Count; i++)
            {
                var tempData = tempDatas[i];
                Character character = CharacterManager.instance.GetCharacter(npcs[i]);
                character.mulitGroup = 0;
                StartCharacterBehavior startCharacterBehavior = new StartCharacterBehavior
                {
                    characterId = npcs[i]
                };
                GameActionManager.instance.QueueAction(startCharacterBehavior, true);
                if (tempData.targetCell.x > 0)
                {
                    character.MoveCrossMap(tempData.targetCell.z, tempData.targetCell.xy,
                    tempData.moveEndAction,
                    tempData.changeCoordinateAction,
                    tempData.failedMoveAction);
                }
            }
        }
        else
        {
            var npcs = npcBehaviorTempDatas.GetKeyList();
            for (int i = 0; i < npcs.Count; i++)
            {
                Character character = CharacterManager.instance.GetCharacter(npcs[i]);
                character.mulitGroup = 0;
                if (NPCManager.instance.GetNPCFormInstance(npcs[i], out var npc))
                {
                    npc.SetNowBehaviorTree();
                }
            }
        }
        npcBehaviorTempDatas.Clear();
        GameObject.Destroy(behaviorTree);
        MultiNPCBehaviorManager.instance.RemoveMulitGroup(instanceId);
    }

    public void JoinInCharacter(int characterId, bool faceCenter)
    {
        if (!npcBehaviorTempDatas.ContainsKey(characterId))
        {
            Character character = CharacterManager.instance.GetCharacter(characterId);
            character.mulitGroup = instanceId;

            NPCBehaviorTempData nPCBehaviorTempData = new NPCBehaviorTempData
            {
                targetCell = character.moveTarget,
                moveEndAction = character.moveEndAction,
                failedMoveAction = character.failedMoveAction,
                changeCoordinateAction = character.changeCoordinateAction,
                behaviorTree = CharacterBehaviorManager.instance.GetCharacterBehaviorTree(characterId)
            };
            npcBehaviorTempDatas.Add(characterId, nPCBehaviorTempData);

            //停止移动
            character.RemoveMove();
            //暂停单人行为
            PauseCharacterBehavior pauseCharacterBehavior = new PauseCharacterBehavior
            {
                characterId = characterId
            };
            GameActionManager.instance.QueueAction(pauseCharacterBehavior, true);

            int maxX = int.MinValue;
            int maxY = int.MinValue;
            int minX = int.MaxValue; int minY = int.MaxValue;

            if (maxX < character.coordinate.x)
                maxX = character.coordinate.x;
            if (maxY < character.coordinate.y)
                maxY = character.coordinate.y;
            if (minX > character.coordinate.x)
                minX = character.coordinate.x;
            if (minY < character.coordinate.y)
                minY = character.coordinate.y;

            int2 center = new int2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
            if (faceCenter)
            {
                var keys = npcBehaviorTempDatas.GetKeyList();
                for (int i = 0; i < keys.Count; i++)
                {
                    var id = keys[i];
                    SetTargetDirection SetTargetDirection = new SetTargetDirection
                    {
                        characterId = id,
                        targetCoordinate = center
                    };
                    GameActionManager.instance.QueueAction(SetTargetDirection, true);
                }
            }

            SharedIntList sharedList = new SharedIntList();
            sharedList.SetValue(npcBehaviorTempDatas.GetKeyList());
            behaviorTree.SetVariable("GroupCharacters", sharedList);
        }
    }

    public void RemoveCharacter(int characterId)
    {
        if (npcBehaviorTempDatas.TryGetValue(characterId, out var tempData))
        {
            Character character = CharacterManager.instance.GetCharacter(characterId);
            character.mulitGroup = 0;
            if (mulitiBehaviorData.needRecoverSingleBehavior)
            {
                StartCharacterBehavior startCharacterBehavior = new StartCharacterBehavior
                {
                    characterId = character.instanceId
                };
                GameActionManager.instance.QueueAction(startCharacterBehavior, true);
                if (tempData.targetCell.x > 0)
                {
                    character.MoveCrossMap(tempData.targetCell.z, tempData.targetCell.xy,
                    tempData.moveEndAction,
                    tempData.changeCoordinateAction,
                    tempData.failedMoveAction);
                }
            }
            else
            {
                if (NPCManager.instance.GetNPCFormInstance(character.instanceId, out var npc))
                {
                    npc.SetNowBehaviorTree();
                }
            }
            npcBehaviorTempDatas.Remove(character.instanceId);
        }
    }

    public void Destory()
    {
        BehaviorEndAction(null);
    }
}

public struct NPCBehaviorTempData
{
    public int3 targetCell;
    public MoveEndAction moveEndAction;
    public MoveEndAction changeCoordinateAction;
    public Int3Action failedMoveAction;
    public BehaviorTree behaviorTree;
}

public class MultiNPCBehaviorManager : Singleton<MultiNPCBehaviorManager>
{ 
    private MyDic<int, MultiNPCGroup> mulitNpcGroups = new MyDic<int, MultiNPCGroup>();
    private GameObject obj;

    public override void Init()
    {
        base.Init();
        obj = GameObject.Find("MultiNPCBehaviorManager");
        if (obj == null)
        {
            obj = new GameObject("MultiNPCBehaviorManager");
        }
         
        mulitNpcGroups.Clear();
        GameActionManager.instance.AddListener<JoinInMultiNPCBehaviorGroup>(JoinInMultiNPCBehaviorGroup);
        GameActionManager.instance.AddListener<LeaveMultiNPCBehaviorGroup>(LeaveMultiNPCBehaviorGroup);
        GameActionManager.instance.AddListener<CreatMultiNPCBehaviorGroup>(CreatMultiNPCBehaviorGroup);
        GameActionManager.instance.AddListener<DestoryMultiNPCBehaviorGroup>(DestoryMultiNPCBehaviorGroup);
    }

    public bool IsInMulitGroup(int characterId)
    {
        for (int i = 0; i < mulitNpcGroups.length; i++)
        {
            if (mulitNpcGroups[i].IsHaveCharacter(characterId))
            {
                return true;
            }
        }
        return false;
    }

    private void DestoryMultiNPCBehaviorGroup(DestoryMultiNPCBehaviorGroup destoryMultiNPCBehaviorGroup)
    {
        if (mulitNpcGroups.TryGetValue(destoryMultiNPCBehaviorGroup.groupId, out var multiNPCGroup))
        {
            multiNPCGroup.Destory();
            //  multiNPCGroup.
        }
    }

    private async void CreatMultiNPCBehaviorGroup(CreatMultiNPCBehaviorGroup creatMultiNPCBehaviorGroup)
    {
        List<int> characters = creatMultiNPCBehaviorGroup.characters;
        int multiDataId = creatMultiNPCBehaviorGroup.dataId;
        bool faceCenter = creatMultiNPCBehaviorGroup.faceCenter;

        int groupInstance = MyInstance.instance.uid;
        var mulitiBehaviorData = await GameDataManager.instance.GetAsyncData<MulitiBehaviorData>(multiDataId);

        int maxX = int.MinValue;
        int maxY = int.MinValue;
        int minX = int.MaxValue; int minY = int.MaxValue;

        int mapInstance = 0;
        MyDic<int, NPCBehaviorTempData> npcBehaviorTempDatas = new MyDic<int, NPCBehaviorTempData>();
        for (int i = 0; i < characters.Count; i++)
        {
            int characterId = characters[i];
            Character character = CharacterManager.instance.GetCharacter(characterId);
            if (character != null)
            {
                mapInstance = character.mapInstance;

                NPCBehaviorTempData nPCBehaviorTempData = new NPCBehaviorTempData
                {
                    targetCell = character.moveTarget,
                    moveEndAction = character.moveEndAction,
                    failedMoveAction = character.failedMoveAction,
                    changeCoordinateAction = character.changeCoordinateAction,
                    behaviorTree = CharacterBehaviorManager.instance.GetCharacterBehaviorTree(characterId)
                };
                npcBehaviorTempDatas.Add(characterId, nPCBehaviorTempData);

                //停止移动
                character.RemoveMove();
                //暂停单人行为
                PauseCharacterBehavior pauseCharacterBehavior = new PauseCharacterBehavior
                {
                    characterId = characterId
                };
                GameActionManager.instance.QueueAction(pauseCharacterBehavior, true);

                if (maxX < character.coordinate.x)
                    maxX = character.coordinate.x;
                if (maxY < character.coordinate.y)
                    maxY = character.coordinate.y;
                if (minX > character.coordinate.x)
                    minX = character.coordinate.x;
                if (minY < character.coordinate.y)
                    minY = character.coordinate.y;

                character.mulitGroup = groupInstance;
            }
        }
        int2 center = new int2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
        if (faceCenter)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                var characterId = characters[i];
                SetTargetDirection SetTargetDirection = new SetTargetDirection
                {
                    characterId = characterId,
                    targetCoordinate = center
                };
                GameActionManager.instance.QueueAction(SetTargetDirection, true);
            }
        }

        var behaviorTree = obj.AddComponent<BehaviorTree>();
        behaviorTree.RestartWhenComplete = false;
        behaviorTree.PauseWhenDisabled = false;
        behaviorTree.StartWhenEnabled = false;
        MultiNPCGroup multiNPCGroup = new MultiNPCGroup(groupInstance, mulitiBehaviorData);
        // multiNPCGroup.mapInstance = mapInstance;
        // multiNPCGroup.center = center;
        multiNPCGroup.InitBehavior(behaviorTree, npcBehaviorTempDatas);

        mulitNpcGroups.Add(groupInstance, multiNPCGroup);
    }

    private void JoinInMultiNPCBehaviorGroup(JoinInMultiNPCBehaviorGroup JoinInMultiNPCBehaviorGroup)
    {
        if (mulitNpcGroups.TryGetValue(JoinInMultiNPCBehaviorGroup.groupId, out var multiNPCGroup))
        {
            multiNPCGroup.JoinInCharacter(JoinInMultiNPCBehaviorGroup.characterId, JoinInMultiNPCBehaviorGroup.faceCenter);
        }
    }

    private void LeaveMultiNPCBehaviorGroup(LeaveMultiNPCBehaviorGroup leaveMultiNPCBehaviorGroup)
    {
        if (mulitNpcGroups.TryGetValue(leaveMultiNPCBehaviorGroup.groupId, out var multiNPCGroup))
        {
            multiNPCGroup.RemoveCharacter(leaveMultiNPCBehaviorGroup.characterId);
        }
    }

    public void RemoveMulitGroup(int groupInstance)
    {
        mulitNpcGroups.Remove(groupInstance);
    }
}