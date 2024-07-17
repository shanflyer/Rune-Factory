using BehaviorDesigner.Runtime;
using OfficeOpenXml.ConditionalFormatting;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static BehaviorDesigner.Runtime.Behavior;

public enum NPCState
{
    修养中 = 0, 正常 = 1
}

public struct CharacterInformationDataList : IReferenceData
{
    public List<CharacterInformationData> characterInformationDatas;
}

public struct CharacterInformationData : IReferenceData
{
    public string name;
    //public Sprite icon;
    public SpriteResourceRenference head;
    public int characterId;
    public bool isNpc;
    public NPCState NPCState;
    public bool isAnimal;
    public AnimalState animalState;
    public CharacterProperty characterProperty;
    public int level;
    public Exp exp;
    public Equip equip;
    public int friendValue;
    public AttributeType attributeType;
}

public class TempCharacter : Character
{
    public TempCharacterData tempCharacterData;
    public int templevel
    {
        set
        {
            if (_templevel != value)
            {
                _templevel = value;
                RefreshBehavior();
            }
        }
        get => _templevel;
    }

    public int _templevel = 0;

    public int targetArea { get; private set; }
    public int nowArea { get; private set; }
    public int2 targetCoordinate { get; private set; }
    public void SetTargetArea(int area,int2 target)
    {
        targetArea = area;
        targetCoordinate = target;
    }
    public void EndMove()
    {
        nowArea = targetArea;
    }
    public TempCharacter(CharacterData characterData,ProfessionData professionData, int instanceId, TempCharacterData tempCharacterData) : base(characterData, professionData, instanceId)
    {
        this.tempCharacterData = tempCharacterData;
        templevel = 1;
    }
    public int GetAreaEmote()
    {
        int ramdonEmote = 0;
        if(tempCharacterData.areaEmote!=null&& tempCharacterData.areaEmote.TryGetValue(nowArea, out ramdonEmote))
        {
        }
        else if (tempCharacterData.mapEmote != null && tempCharacterData.mapEmote.TryGetValue(mapInstance, out ramdonEmote))
        {

        }
        else
        {
            ramdonEmote = tempCharacterData.defaultEmote;
        }
        var result=GameRandom.instance.GetRandomValue(ramdonEmote);
        if (result.Count > 0)
        {
            return result[0].x;
        }
        return 0;
    }
    public int GetTalk()
    {
        int ramdonTalk = tempCharacterData.defaultTalk;
        if (tempCharacterData.areaTalk != null && tempCharacterData.areaTalk.TryGetValue(nowArea, out ramdonTalk))
        {
        }
        else if (tempCharacterData.mapTalk != null && tempCharacterData.mapTalk.TryGetValue(mapInstance, out ramdonTalk))
        {

        }
        var result = GameRandom.instance.GetRandomValue(ramdonTalk);
        if (result.Count > 0)
        {
            return result[0].x;
        }
        return 0;
    }
    private void RefreshBehavior()
    {
        ExternalBehaviorTree externalBehaviorTree;
        if (tempCharacterData.levelMapBehaviors.TryGetValue(templevel, out var externalBehaviorTreeDic))
        {
            if(!externalBehaviorTreeDic.TryGetValue(mapInstance,out externalBehaviorTree))
            {
                externalBehaviorTreeDic.TryGetValue(0, out externalBehaviorTree);
            }
            externalBehaviorTree = tempCharacterData.defaultBehavior; 
        }
        else
        {
            externalBehaviorTree = tempCharacterData.defaultBehavior;
        }
        CharacterBehaviorManager.instance.DestroyBehavior(instanceId);
        CharacterBehaviorManager.instance.AddBehavior(instanceId, externalBehaviorTree,true);
    }
}

public class Player : Character
{
    public Player(CharacterData characterData, int instanceId, ProfessionData professionData) : base(characterData, professionData, instanceId)
    {
    }

    protected override async Task CreatCharacterPackage(int overridePackageId = 0,int instancId=0)
    {
        await base.CreatCharacterPackage();
        PackageManager.instance.AddPlayerPackage(overridePackageId == 0 ? characterPackage : overridePackageId);
    }

}

public partial class Character
{

    public int selectItem;

    public CharacterInformationData GetInformation()
    {
        CharacterInformationData characterInformationData = new CharacterInformationData();
        characterInformationData.characterId = instanceId;
        characterInformationData.characterProperty = CharacterProperty;
        characterInformationData.level = level;
        characterInformationData.exp = exp;
        characterInformationData.equip = equip;
        characterInformationData.name = name;
        characterInformationData.head = characterData.head;
        //characterInformationData.icon = characterData.icon.sprite;
        //characterInformationData.attributeType = attributeType; 

        if (NPCManager.instance.GetNPC(instanceId, out var npc))
        {
            characterInformationData.isNpc = true;
            characterInformationData.NPCState = npc.npcState;
        }
        if(PastureManager.instance.GetAnimal(instanceId,out var animal))
        {
            characterInformationData.isAnimal = true;
            characterInformationData.animalState = animal.animalState;
        }

        return characterInformationData;
    }
}

public struct NPCList : IReferenceData
{
    public List<NPC> npcs;
}

public class NPC :  IReferenceData
{
    public NPC(int instanceId,NPCData nPCData)
    {
        characterId = instanceId;
        npcData = nPCData;
        npcState = NPCState.正常;
        endBehavior = true;
        behaviorCanBreak = false;
    } 
    public static Color GetStateColor(NPCState state)
    {
        if (state == NPCState.正常)
        {
            return new Color(0, 0.5f, 0, 1);
        }
        return new Color(0.5f, 0, 0, 1);
    }

    public CharacterInformationData GetInformation()
    {
        Character character = CharacterManager.instance.GetCharacter(characterId);
        if (character != null)
        {
        }
        return character.GetInformation();
    }
    private NPCBehaviorData NPCBehaviorData; 
    private int homeMap;

    public List<int2> Beds => NPCBehaviorData.beds;
    public List<int2> WorkItems=> NPCBehaviorData.workItems;
    public int HomeMap=>homeMap;

    public NPCState npcState;
    public NPCData npcData;
    public bool isActive;
    public int characterId;
    public bool hide=>npcData.hide;

    public async void InitBehaviorData()
    {
        NPCBehaviorData = await GameDataManager.instance.GetAsyncData<NPCBehaviorData>(npcData.id);
        SetNPCTaskScheduleTimeList(NPCBehaviorData.dailyTasks, NPCBehaviorData.externalBehavior);
    }
    
    async void SetNPCTaskScheduleTimeList(List<int> dailyTasks, ExternalBehaviorTree externalBehavior)
    {
        List<TaskScheduleModelData> taskSheduleModelDatas = new List<TaskScheduleModelData>();
        for(int i=0;i<dailyTasks.Count;i++)
        {
            var taskSheduleModelData =await GameDataManager.instance.GetAsyncData<TaskScheduleModelData>(dailyTasks[i]);
            taskSheduleModelDatas.Add(taskSheduleModelData);
        }
        nPCTaskScheduleTimeList = new NPCTaskScheduleTimeList(taskSheduleModelDatas);

        if (!SetNowBehaviorTree())
        {
            AddNpcBehavior(externalBehavior, true);
        }
    }
    public async Task<CharacterData> GetCharacterData()
    { 
        CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(npcData.linkCharacterId);
        return characterData;
    }
    private NPCTaskScheduleTimeList nPCTaskScheduleTimeList;
    private bool endBehavior=true;
    private bool behaviorCanBreak = false;
    void ResetBehaviorState(Behavior behavior)
    {
        if (SingletonType.Cleared)
        {
            return;
        }
        endBehavior = true;
        behaviorCanBreak = false;
        var externalBehavior = GetNowTaskScheduleBehavior(out var loopBehavior, out behaviorCanBreak);
        if (externalBehavior != null)
        {
            AddNpcBehavior(externalBehavior, loopBehavior); 
        }
    }
    public bool SetTimeBehaviorTree(UpdateGameTime UpdateGameTime)
    {
        if (endBehavior || behaviorCanBreak)
        {
            var externalBehavior = GetTimeTaskScheduleBehavior(UpdateGameTime, out var loopBehavior,out behaviorCanBreak);
            if (externalBehavior != null)
            {
                AddNpcBehavior(externalBehavior, loopBehavior); 
                return true;
            }
        } 
        return false;
    }
    public void AddNpcBehavior(ExternalBehaviorTree externalBehavior, bool loopBehavior = true)
    {
        CharacterBehaviorManager.instance.AddBehavior(characterId, externalBehavior, loopBehavior, ResetBehaviorState);
        endBehavior = false;
    }
    public bool SetNowBehaviorTree()
    {
        var externalBehavior = GetNowTaskScheduleBehavior(out var loopBehavior,out behaviorCanBreak);
        if (externalBehavior != null)
        {
            AddNpcBehavior(externalBehavior, loopBehavior);
            return true;
        }
        return false;
    }
 
    public ExternalBehaviorTree GetTimeTaskScheduleBehavior(UpdateGameTime UpdateGameTime,out bool loopBehavior,out bool behaviorCanBreak)
    {
        var data = nPCTaskScheduleTimeList.GetTaskSheduleData(new int2(UpdateGameTime.hour, UpdateGameTime.minute));
        if (data != null)
        {
            loopBehavior = data.loopBehavior;
            behaviorCanBreak = data.canBreak;
            return data.externalBehavior;
        }
        loopBehavior = false;
        behaviorCanBreak = false;
        return null;
    }
    public ExternalBehaviorTree GetNowTaskScheduleBehavior(out bool loopBehavior, out bool behaviorCanBreak)
    {
        try
        {
            if (nPCTaskScheduleTimeList != null)
            {
                var data = nPCTaskScheduleTimeList.GetTaskSheduleData(GameTimeManager.instance.nowHourMinute);
                if (data != null)
                {
                    loopBehavior = data.loopBehavior;
                    behaviorCanBreak = data.canBreak;
                    return data.externalBehavior;
                }
            }
        }
        catch
        {

        }
       
       
        loopBehavior = false;
        behaviorCanBreak = false;
        return null;
    }
    public void Dispose()
    {
    }

    public int Key => npcData.id;

    public void GetGift(int giveCharacter, int giftId)
    {
        int likeState = 0;
        if (NPCBehaviorData.likeItems.Contains(giftId))
        {
            likeState = 1;
        }
        else if (NPCBehaviorData.unLikeItems.Contains(giftId))
        {
            likeState = -1;
        }
        int talkId = 0;
        int emoteId = 0;
        int friendValue = 0;
        List<int2> talkRandomResults = new List<int2>();
        List<int2> emoteRandomResults = new List<int2>();
        switch (likeState)
        {
            case 1:
                friendValue = 4;
                talkRandomResults = GameRandom.instance.GetRandomValue(NPCBehaviorData.likeTalk);
                emoteRandomResults = GameRandom.instance.GetRandomValue(NPCBehaviorData.likeEmote);
                break;

            case 0:
                friendValue = 2;
                talkRandomResults = GameRandom.instance.GetRandomValue(NPCBehaviorData.defaultTalk);
                emoteRandomResults = GameRandom.instance.GetRandomValue(NPCBehaviorData.defaultEmote);
                break;

            case -1:
                talkRandomResults = GameRandom.instance.GetRandomValue(NPCBehaviorData.unlikeTalk);
                emoteRandomResults = GameRandom.instance.GetRandomValue(NPCBehaviorData.unlikeEmote);
                break;
        }
        talkId = talkRandomResults[0].x;
        emoteId = emoteRandomResults[0].x;
        GameTimerController.instance.DelayAction(1000, () =>
        {
            if (CharacterManager.instance.controllerCharacter.instanceId == giveCharacter)
            {
                Talk talk = new Talk
                {
                    characterId = characterId,
                    talkId = talkId,
                    displayFunction = false,
                    endAction = () =>
                    {
                        CharacterManager.instance.controllerCharacter.SetNeighborhood(characterId);
                    }
                };
                GameActionManager.instance.QueueAction(talk);

                AddFriendShipValue addFriendShipValue = new AddFriendShipValue
                {
                    characterId = characterId,
                    friendAddType = FriendAddType.礼物,
                    value = friendValue
                };
                GameActionManager.instance.QueueAction(addFriendShipValue);
            }
            ShowEmote showEmote = new ShowEmote
            {
                emoteId = emoteId,
                entityType = EntityType.角色,
                id = characterId
            };
            GameActionManager.instance.QueueAction(showEmote);
        });
    }

}

public class NPCTaskScheduleTimeList
{
    private List<TaskScheduleModelData> taskScheduleModelDatas = new List<TaskScheduleModelData>(); 

    public NPCTaskScheduleTimeList(List<TaskScheduleModelData> taskScheduleModelDatas)
    {
        this.taskScheduleModelDatas = taskScheduleModelDatas;  
        nowTimeKeyIndex = 0;
    }
    int nowTimeKeyIndex;
    public bool GetTaskScheduleDataOrder(int2 time,ref NPCTaskScheduleData nPCTaskScheduleData)
    {
        if (taskScheduleModelDatas[nowTimeKeyIndex].gameTimeKey == time)
        {
            return false;
        }
        nowTimeKeyIndex ++;
        if(nowTimeKeyIndex >= taskScheduleModelDatas.Count)
        {
            nowTimeKeyIndex = 0;
        }
        nPCTaskScheduleData = GetTaskSheduleData(time); 
        
        return true;
    }


    public NPCTaskScheduleData GetTaskSheduleData(int2 time)
    {
        if (nowTimeKeyIndex >= taskScheduleModelDatas.Count)
        {
            return null;
        }
        TaskScheduleModelData taskScheduleModelData = taskScheduleModelDatas[nowTimeKeyIndex];
        int startM = taskScheduleModelData.gameTimeKey.minHour * 60 + taskScheduleModelData.gameTimeKey.minMinute;
        int endM= taskScheduleModelData.gameTimeKey.maxHour * 60 + taskScheduleModelData.gameTimeKey.maxMinute;
        int nowM = time.x * 60 + time.y;

        float e_value = (nowM - startM) / (float)(endM - startM);

        GameRandomData gameRandomData = new GameRandomData
        {
            id = -1,
            weightRandom = true,
            barrels = new List<int3>(),
            randomItems = new List<RandomItem>(),
            text = "选择目标"
        };
        for(int i = 0; i < taskScheduleModelData.dailyTaskDataItems.Count; i++)
        {
            int2 dailyItem = taskScheduleModelData.dailyTaskDataItems[i].GetNowTaskRandomValue(e_value);
            RandomItem randomItem = new RandomItem
            {
                itemValue = dailyItem.x,
                randomValue = dailyItem.y,
                maxCount = 1,
                minCount = 1
            };
            gameRandomData.randomItems.Add(randomItem);
        } 
        gameRandomData.Pretreatment();
        var randomResults = GameRandom.instance.GetRandomValue(gameRandomData, 1);
        if (randomResults.Count > 0)
        {
            if (NPCTaskScheduleManager.instance.GetTaskScheduleData(randomResults[0].x,out var nPCTaskScheduleData))
            {
                return nPCTaskScheduleData;
            }  
        }  
        return null;
    }

  
}

public class NPCManager : Singleton<NPCManager>
{
    private MyDic<int,NPC> npcs = new MyDic<int, NPC>();
    private Dictionary<int, int> instanceDatas = new Dictionary<int, int>(); 
    public override void Init()
    {
        base.Init();
        npcs.Clear(); CreatZeroNPC();
        GameActionManager.instance.AddListener<GiveGift>(GiveGift);
        GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);
    }

    protected override void Clear()
    {
        base.Clear();
        npcs.Clear();
    } 
    void UpdateGameTime(UpdateGameTime updateGameTime)
    {
       var IEnumerator = UpDataNPCTimeBehaviorTree(updateGameTime);
        GameObjectCurveController.instance.UpDataComponent.StartCoroutine(IEnumerator);
        /*
        for (int i = 0; i < npcs.length; i++)
        {
            npcs[i].SetTimeBehaviorTree(updateGameTime);
        }*/
    }
    
    IEnumerator UpDataNPCTimeBehaviorTree(UpdateGameTime updateGameTime)
    {
        int totalNum = 0;
        int perNum = npcs.length / 10;
        for (int i = 0; i < npcs.length; i++)
        {
            npcs[i].SetTimeBehaviorTree(updateGameTime);
            totalNum++;
            if (totalNum >= perNum)
            {
                yield return 0;
                totalNum = 0;
            }
            
        }
    }

    private void GiveGift(GiveGift giveGift)
    {
        if (GetNPCFormInstance(giveGift.receiveCharacter, out var npc))
        {
            npc.GetGift(giveGift.giveCharacter, giveGift.giftId); 
        }
    }

    public bool GetNPCFormInstance(int instanceId, out NPC npc)
    {
        npc =null;
        if (instanceDatas.TryGetValue(instanceId, out var id))
        {
            return npcs.TryGetValue(id, out npc);
        }
        return false;
    }

    public bool GetNPC(int id, out NPC npc)
    {
        return npcs.TryGetValue(id, out npc);
    }

    public NPCList GetNPCList()
    {
        NPCList nPCList = new NPCList
        {
            npcs = npcs.GetValueList(),
        };
        
        return nPCList;
    }

    public async void CreatZeroNPC()
    {
        var NPCDatas = await GameDataManager.instance.GetAllAsyncData<NPCData>();
        for (int i = 0; i < NPCDatas.Count; i++)
        {
            var NPCData = NPCDatas[i];
            if (NPCData.zeroCreate)
            {
                int instanceId = CharacterManager.instance.GetCharacterInstance();
                NPC npc = new NPC(instanceId, NPCData); 
                npcs.Add(npc.Key,npc);
                instanceDatas[instanceId] = NPCData.id;
            }
        }
    }
}