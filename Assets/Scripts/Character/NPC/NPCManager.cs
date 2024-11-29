using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine; 

public enum NPCState
{
    修养中 = 0, 正常 = 1
}

public enum NPCBehaviorState
{
    闲置 = 0, 工作 = 1, 睡眠 = 2,
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

    public void SetTargetArea(int area, int2 target)
    {
        targetArea = area;
        targetCoordinate = target;
    }

    public void EndMove()
    {
        nowArea = targetArea;
    }

    public TempCharacter(CharacterData characterData, ProfessionData professionData, int instanceId, TempCharacterData tempCharacterData) : base(characterData, professionData, instanceId)
    {
        this.tempCharacterData = tempCharacterData;
        //templevel = 1;
    }

    public int GetAreaEmote()
    {
        int ramdonEmote = 0;
        if (tempCharacterData.areaEmote != null && tempCharacterData.areaEmote.TryGetValue(nowArea, out ramdonEmote))
        {
        }
        else if (tempCharacterData.mapEmote != null && tempCharacterData.mapEmote.TryGetValue(mapInstance, out ramdonEmote))
        {
        }
        else
        {
            ramdonEmote = tempCharacterData.defaultEmote;
        }
        var result = GameRandom.instance.GetRandomValue(ramdonEmote);
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
            if (!externalBehaviorTreeDic.TryGetValue(mapInstance, out externalBehaviorTree))
            {
                externalBehaviorTree = tempCharacterData.defaultBehavior;
            }
        }
        else
        {
            externalBehaviorTree = tempCharacterData.defaultBehavior;
        }
        if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var characterRuntimeObj))
        {
            characterRuntimeObj.SetEnableBehavior(instanceId, externalBehaviorTree); 
        }
        else
        {
            CharacterBehaviorManager.instance.DestroyBehavior(instanceId);
            CharacterBehaviorManager.instance.AddBehavior(instanceId, externalBehaviorTree);
        }
    }
}

public class Player : Character
{
    public Player(CharacterData characterData, int instanceId, ProfessionData professionData) : base(characterData, professionData, instanceId)
    {
    }

    protected override async Task CreatCharacterPackage(int overridePackageId = 0, int instancId = 0)
    {
        await base.CreatCharacterPackage();
        PackageManager.instance.AddPlayerPackage(overridePackageId == 0 ? characterPackage : overridePackageId);
    }
}

public partial class Character
{
    public int selectItem;
    public int mulitGroup;

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
        if (PastureManager.instance.GetAnimal(instanceId, out var animal))
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

public class NPC : IReferenceData
{
    public NPC(int instanceId, NPCData nPCData)
    {
        characterInstance = instanceId;
        npcData = nPCData;
        npcState = NPCState.正常;
        endBehavior = true;
        behaviorCanBreak = false;
    }

    public Character Character
    {
        get
        {
            if (character == null)
            {
                character = CharacterManager.instance.GetCharacter(characterInstance);
            }
            return character;
        }
    }

    private Character character;

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
        return Character.GetInformation();
    }

    private NPCBehaviorData NPCBehaviorData;
    public List<int> functions => npcData.functionIds;
    public List<int> likeItems => NPCBehaviorData.likeItems;
    public string shopName => npcData.shopName;

    public List<int2> Beds => NPCBehaviorData.beds;
    public List<int2> WorkItems => NPCBehaviorData.workItems;
    public int HomeMap => NPCBehaviorData.home;
    public int WorkMap => NPCBehaviorData.workMap;
    public int playerOperateEventId => npcData.playerOperateEventId;
    public int nextTalkEventId => npcData.nextTalkEventId;

    public NPCState npcState;
    private NPCData npcData;
    public bool isActive;

    public bool CheckNpcShop()
    {
        if (character.linkItem == 0)
        {
            return false;
        }
        else
        {
            if (nowScheduleData.taskName == "看守柜台")
            {
                return true;
            }
        }
        return false;
    }

    public int GetHomeArea()
    {
        if (NPCBehaviorData.homeAreas.Count>0)
        {
            int index = GameRandom.RandomInt(0, NPCBehaviorData.homeAreas.Count);
            return NPCBehaviorData.homeAreas[index];
        }
        return 0;
    }
    public int GetWorkArea()
    {
        if (NPCBehaviorData.workMapAreas.Count > 0)
        {
            int index = GameRandom.RandomInt(0, NPCBehaviorData.workMapAreas.Count);
            return NPCBehaviorData.workMapAreas[index];
        }
        return 0;
    }
    /// <summary>
    ///
    /// </summary>
    public int characterInstance;

    public string npcName => npcData.npcName;
    public int dataId => npcData.id;
    public bool hide => npcData.hide;

    private MyDic<int, int3> visitMaps = new MyDic<int, int3>();
    private MyDic<int, int2> visitFriends = new MyDic<int, int2>();
    private MyDic<int, int> visitShops = new MyDic<int, int>();

    public int GetTalkId()
    {
        int friendShipLevel = FriendManager.instance.GetFriendShipLevel(dataId);
        return npcData.GetTalk(friendShipLevel,character.mapInstance);
    }

    public async void InitBehaviorData()
    {
        NPCBehaviorData = await GameDataManager.instance.GetAsyncData<NPCBehaviorData>(npcData.id);
        SetNPCTaskScheduleTimeList(NPCBehaviorData.dailyTasks, NPCBehaviorData.externalBehavior);
        visitMaps.Clear();
        InitNowVisitMap();
        visitFriends.Clear();
        try
        {
            for (int i = 0; i < NPCBehaviorData.npcFriends.Count; i++)
            {
                visitFriends.Add(NPCBehaviorData.npcFriends[i].x, NPCBehaviorData.npcFriends[i]);
            }
            for (int i = 0; i < NPCBehaviorData.visitShops.Count; i++)
            {
                visitShops.Add(NPCBehaviorData.visitShops[i].x, NPCBehaviorData.visitShops[i].y);
            }
        }
        catch
        {
            Debug.LogError($"{npcData.npcName}--error");
        }
       
    }

    public bool IsInHome()
    {
        if (Character != null)
        {
            return Character.mapInstance == HomeMap;
        }
        return false;
    }

    private void InitNowVisitMap()
    {
        if (NPCBehaviorData.gameTimeKeyVisitMapDic.TryGetValue(GameTimeManager.instance.nowHourMinute, out var value))
        {
            var int2 = GameRandom.instance.GetRandomItemValueList(value);
            HashSet<int> nowMaps = new HashSet<int>();
            for (int i = 0; i < int2.Count; i++)
            {
                if (visitMaps.TryGetValue(int2[i].x, out var visitMap))
                {
                    visitMap = new int3(int2[i].x, int2[i].y, visitMap.z);
                    visitMaps.TrySetValue(int2[i].x, visitMap);
                }
                else
                {
                    visitMap = new int3(int2[i].x, int2[i].y, 10000);
                    visitMaps.TrySetValue(int2[i].x, visitMap);
                }
                nowMaps.Add(int2[i].x);
            }
            if (visitMaps.length > 0)
            {
                for (int i = visitMaps.length - 1; i >= 0; i--)
                {
                    if (!nowMaps.Contains(visitMaps[i].x))
                    {
                        visitMaps.RemoveAt(i);
                    }
                }
            }
        }
    }

    public int GetVisitMap()
    {
        InitNowVisitMap();

        for (int i = 0; i < visitMaps.length; i++)
        {
            int3 visitMap = visitMaps[i];
            if (visitMap.x != Character.mapInstance)
            {
                visitMap.z *= 2;
            }
            else
            {
                visitMap.z /= 2;
            }
            visitMap.z = math.clamp(visitMap.z, 0, 10000);
        }
        GameRandomData gameRandomData = new GameRandomData
        {
            id = -1,
            weightRandom = true,
            barrels = new List<int3>(),
            randomItems = new List<RandomItem>(),
            text = "选择目标"
        };
        for (int i = 0; i < visitMaps.length; i++)
        {
            RandomItem randomItem = new RandomItem
            {
                itemValue = visitMaps[i].x,
                randomValue = visitMaps[i].y * visitMaps[i].z / 10000,
                maxCount = 1,
                minCount = 1
            };
            gameRandomData.randomItems.Add(randomItem);
        }
        gameRandomData.Pretreatment();
        var randomResults = GameRandom.instance.GetRandomValue(gameRandomData, 1);
        if (randomResults.Count > 0)
        {
            int mapId = randomResults[0].x;
            return mapId;
        }
        return -1;
    }

    public int GetVisitFriend()
    {
        if (visitFriends.length == 0)
        {
            for (int i = 0; i < NPCBehaviorData.npcFriends.Count; i++)
            {
                visitFriends.Add(NPCBehaviorData.npcFriends[i].x, NPCBehaviorData.npcFriends[i]);
            }
        }

        GameRandomData gameRandomData = new GameRandomData
        {
            id = -1,
            weightRandom = true,
            barrels = new List<int3>(),
            randomItems = new List<RandomItem>(),
            text = "选择目标"
        };
        for (int i = 0; i < visitFriends.length; i++)
        {
            RandomItem randomItem = new RandomItem
            {
                itemValue = visitFriends[i].x,
                randomValue = visitFriends[i].y,
                maxCount = 1,
                minCount = 1
            };
            gameRandomData.randomItems.Add(randomItem);
        }
        gameRandomData.Pretreatment();
        var randomResults = GameRandom.instance.GetRandomValue(gameRandomData, 1);
        if (randomResults.Count > 0)
        {
            int npcId = randomResults[0].x;
            visitFriends.Remove(npcId);
            return npcId;
        }

        return 0;
    }

    public int GetVisitShop()
    {
        if (visitShops.length == 0)
        {
            for (int i = 0; i < NPCBehaviorData.visitShops.Count; i++)
            {
                visitShops.Add(NPCBehaviorData.visitShops[i].x, NPCBehaviorData.visitShops[i].y);
            }
        }
        GameRandomData gameRandomData = new GameRandomData
        {
            id = -1,
            weightRandom = true,
            barrels = new List<int3>(),
            randomItems = new List<RandomItem>(),
            text = "选择目标"
        };
        for (int i = 0; i < visitShops.length; i++)
        {
            RandomItem randomItem = new RandomItem
            {
                itemValue = visitShops.GetKeyForIndex(i),
                randomValue = visitShops.GetValueForIndex(i),
                maxCount = 1,
                minCount = 1
            };
            gameRandomData.randomItems.Add(randomItem);
        }
        gameRandomData.Pretreatment();
        var randomResults = GameRandom.instance.GetRandomValue(gameRandomData, 1);
        if (randomResults.Count > 0)
        {
            int shopId = randomResults[0].x;
            visitShops.Remove(shopId);
            return shopId;
        }
        return 0;
    }

    private async void SetNPCTaskScheduleTimeList(List<int> dailyTasks, ExternalBehaviorTree externalBehavior)
    {
        List<TaskScheduleModelData> taskSheduleModelDatas = new List<TaskScheduleModelData>();
        for (int i = 0; i < dailyTasks.Count; i++)
        {
            var taskSheduleModelData = await GameDataManager.instance.GetAsyncData<TaskScheduleModelData>(dailyTasks[i]);
            taskSheduleModelDatas.Add(taskSheduleModelData);
        }
        nPCTaskScheduleTimeList = new NPCTaskScheduleTimeList(taskSheduleModelDatas);
         //Debug.Log($"nPCTaskScheduleTimeList.ini{npcData.npcName}");
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
    private bool endBehavior = true;
    private bool behaviorCanBreak = false;

    private void ResetBehaviorState(Behavior behavior)
    {
        if (SingletonType.Cleared)
        {
            return;
        }
        endBehavior = true;
        behaviorCanBreak = false;
       // Debug.Log($"进入回调:{npcData.npcName}");
        var externalBehavior = GetNowTaskScheduleBehavior(out behaviorCanBreak, out var pauseWhenDisabled);
        if (externalBehavior != null)
        {
           // Debug.Log($"ResetBehaviorStat:{externalBehavior.name}--{npcData.npcName}");
            AddNpcBehavior(externalBehavior, PauseWhenDisabled: pauseWhenDisabled);
        }
    }

    public bool SetTimeBehaviorTree(UpdateGameTime UpdateGameTime)
    {
        if (endBehavior || behaviorCanBreak)
        { 
            var externalBehavior = GetTimeTaskScheduleBehavior(UpdateGameTime, out behaviorCanBreak, out var pauseWhenDisabled);
            if (externalBehavior != null)
            {
                AddNpcBehavior(externalBehavior, PauseWhenDisabled: pauseWhenDisabled);
                return true;
            }
        }
        return false;
    }

    public void AddNpcBehavior(ExternalBehaviorTree externalBehavior,bool PauseWhenDisabled = false)
    {
        if (externalBehavior == null)
        {
            return;
        }
        try
        {
            CharacterBehaviorManager.instance.AddBehavior(characterInstance, externalBehavior,
          ResetBehaviorState, PauseWhenDisabled, Character.name);
            endBehavior = false;
        }
        catch
        {
          Debug.LogError($"NPCbehavior:{npcData.name}!!!!");
        }
    }

    public bool SetNowBehaviorTree()
    {
        var externalBehavior = GetNowTaskScheduleBehavior(out behaviorCanBreak, out var pauseWhenDisabled);
        if (externalBehavior != null)
        {
            AddNpcBehavior(externalBehavior, PauseWhenDisabled: pauseWhenDisabled);
            return true;
        }
        return false;
    }

    public ExternalBehaviorTree GetTimeTaskScheduleBehavior(UpdateGameTime UpdateGameTime,  out bool behaviorCanBreak
        , out bool pauseWhenDisabled)
    {
        if (nPCTaskScheduleTimeList == null)
        {
           // Debug.Log($"null nPCTaskScheduleTimeList{npcData.npcName}"); 
            behaviorCanBreak = false;
            pauseWhenDisabled = false;
            return null;
        }
        if(nPCTaskScheduleTimeList.GetTaskScheduleDataOrder(new int2(UpdateGameTime.hour, UpdateGameTime.minute),ref nowScheduleData))
        {  
            behaviorCanBreak = nowScheduleData.canBreak;
            pauseWhenDisabled = nowScheduleData.PauseWhenDisabled;
            return nowScheduleData.externalBehavior;
        } 
        behaviorCanBreak = false;
        pauseWhenDisabled = false;
        return null;
    }


    private NPCTaskScheduleData nowScheduleData;
    public NPCBehaviorState behaviorState => nowScheduleData.behaviorState;
    public bool holdPos
    {
        get
        {
            if (overrideHold)
            {
                return _holdPos;
            }else
            {
                return nowScheduleData.holdPos;
            }
        }
    }

    bool overrideHold;
    bool _holdPos;
    public void SetOverrideHold(bool hold)
    {
        overrideHold = true;
        _holdPos = hold; 
    }

    public void RemoveOverrideHold()
    {
        overrideHold = false; 
    }


    bool behaviorIsPause;
    float pauseTime;
    public void PauseCharacterBehavior()
    {
        behaviorIsPause = true;
        pauseTime = Time.time;
    }
    public void ResetCharacterBehavior()
    {
        if (behaviorIsPause)
        {
            behaviorIsPause = false;
            if (Time.time - pauseTime > nowScheduleData.maxPauseTime)
            {
                SetNowBehaviorTree();
            }
            else
            {
                StartCharacterBehavior startCharacterBehavior = new StartCharacterBehavior
                {
                    characterId = characterInstance
                };
                GameActionManager.instance.QueueAction(startCharacterBehavior);
            }
        }
    }

    public ExternalBehaviorTree GetNowTaskScheduleBehavior(out bool behaviorCanBreak, out bool PauseWhenDisabled)
    {
        try
        {
            if (nPCTaskScheduleTimeList != null)
            { 
                if (nPCTaskScheduleTimeList.GetTaskScheduleDataOrder(GameTimeManager.instance.nowHourMinute,ref nowScheduleData))
                { 
                    behaviorCanBreak = nowScheduleData.canBreak;
                    PauseWhenDisabled = nowScheduleData.PauseWhenDisabled;
                    return nowScheduleData.externalBehavior;
                }
            }
            else
            {
                Debug.Log($"{npcName}-无nPCTaskScheduleTimeList");
            }
        }
        catch
        {
        }
         
        behaviorCanBreak = false;
        PauseWhenDisabled = false;
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
                    characterId = characterInstance,
                    talkId = talkId,
                    displayFunction = false,
                    endAction = () =>
                    {
                        CharacterManager.instance.controllerCharacter.SetNeighborhood(characterInstance); 
                    }
                };
                GameActionManager.instance.QueueAction(talk);

                AddFriendShipValue addFriendShipValue = new AddFriendShipValue
                {
                    characterId = characterInstance,
                    friendAddType = FriendAddType.礼物,
                    value = friendValue
                };
                GameActionManager.instance.QueueAction(addFriendShipValue);
            }
            ShowEmote showEmote = new ShowEmote
            {
                emoteId = emoteId,
                entityType = EntityType.角色,
                id = characterInstance
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

    private int nowTimeKeyIndex;

    public bool GetTaskScheduleDataOrder(int2 time, ref NPCTaskScheduleData nPCTaskScheduleData)
    {
        if (taskScheduleModelDatas[nowTimeKeyIndex].gameTimeKey != time)
        {
           for(int i = 0; i < taskScheduleModelDatas.Count; i++)
            {
                if (taskScheduleModelDatas[i].gameTimeKey == time)
                {
                    nowTimeKeyIndex = i;
                    break;
                }
            }
        } 
        if (nowTimeKeyIndex >= taskScheduleModelDatas.Count)
        {
            nowTimeKeyIndex = 0;
        }
        nPCTaskScheduleData = GetTaskSheduleData(time);

        return true;
    }

    private NPCTaskScheduleData GetTaskSheduleData(int2 time)
    {
        if (nowTimeKeyIndex >= taskScheduleModelDatas.Count)
        {
            return null;
        }
        TaskScheduleModelData taskScheduleModelData = taskScheduleModelDatas[nowTimeKeyIndex];
        int startM = taskScheduleModelData.gameTimeKey.minHour * 60 + taskScheduleModelData.gameTimeKey.minMinute;
        int endM = taskScheduleModelData.gameTimeKey.maxHour * 60 + taskScheduleModelData.gameTimeKey.maxMinute;
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
        for (int i = 0; i < taskScheduleModelData.dailyTaskDataItems.Count; i++)
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
            if (NPCTaskScheduleManager.instance.GetTaskScheduleData(randomResults[0].x, out var nPCTaskScheduleData))
            {
                return nPCTaskScheduleData;
            }
        }
        return null;
    }
}

public class NPCManager : Singleton<NPCManager>
{
    private MyDic<int, NPC> npcs = new MyDic<int, NPC>();
    private Dictionary<int, int> instanceDatas = new Dictionary<int, int>();

    public override void Init()
    {
        base.Init();
        npcs.Clear(); CreateZeroNPC();
        GameActionManager.instance.AddListener<GiveGift>(GiveGift);
        GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);
        GameActionManager.instance.AddListener<TryContinueBehavior>(TryContinueBehavior);
        GameActionManager.instance.AddListener<CheckNpcShopLink>(CheckNpcShopLink);
    }

    protected override void Clear()
    {
        base.Clear();
        npcs.Clear();
    }
    public void InitNPCBehavior()
    {
        for(int i = 0; i < npcs.length; i++)
        {
            if (npcs[i].Character == null || npcs[i].Character.mapInstance < 0)
            {
               // Debug.Log($"npc:{npcs[i].npcName}--不适合");
                continue;
            }
            npcs[i].InitBehaviorData();
        }
    }
  
    public void CheckNpcShopLink(CheckNpcShopLink checkNpcShopLink)
    {
        if(GetNPCFormInstance(checkNpcShopLink.characterId,out var npc))
        {
            if (npc.CheckNpcShop())
            {
                checkNpcShopLink.setResult(true);
                return;
            }
        }
        checkNpcShopLink.setResult(false);
    }
    private void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        int perNum = npcs.length / 10;
        for (int i = 0; i < npcs.length; i++)
        {
            if (npcs[i].Character == null || npcs[i].Character.mapInstance <= 0)
            {
                continue;
            }
            npcs[i].SetTimeBehaviorTree(updateGameTime);
        }
    }

    private void GiveGift(GiveGift giveGift)
    {
        if (GetNPCFormInstance(giveGift.receiveCharacter, out var npc))
        {
            npc.GetGift(giveGift.giveCharacter, giveGift.giftId);
        }
    }
    public bool GetNPCIdFromInstance(int instanceId,out int npcId)
    {
        if (instanceDatas.TryGetValue(instanceId, out npcId))
        {
            return true;
        }
        npcId = 0;
        return false ;
    }
    public bool GetNPCFormInstance(int instanceId, out NPC npc)
    {
        npc = null;
        if (instanceDatas.TryGetValue(instanceId, out var id))
        {
            return npcs.TryGetValue(id, out npc);
        }
        return false;
    }

    public Character GetNPCCharacter(int id)
    {
        if (GetNPC(id, out var npc))
        {
            return CharacterManager.instance.GetCharacter(npc.characterInstance);
        }
        return null;
    }

    public bool GetNPC(int id, out NPC npc)
    {
        return npcs.TryGetValue(id, out npc);
    }

    public NPCList GetNPCList()
    {
        NPCList nPCList = new NPCList
        {
            npcs = npcs.GetValueList().FindAll(n=>!n.hide),
        };

        return nPCList;
    }

    async void CreateZeroNPC()
    {
        var NPCDatas = await GameDataManager.instance.GetAllAsyncData<NPCData>();
        for (int i = 0; i < NPCDatas.Count; i++)
        {
            var NPCData = NPCDatas[i];
            if (NPCData.zeroCreate)
            {
                int instanceId = MyInstance.instance.uid;
                NPC npc = new NPC(instanceId, NPCData);
                npcs.Add(npc.Key, npc);
                instanceDatas[instanceId] = NPCData.id;
                FriendManager.instance.ZeroFriendShip(NPCData.id, NPCData.zeroFriendShipLevel);

                Character character = CharacterManager.instance.GetCharacter(instanceId);
                if (character==null)
                {
                    CreatCharacter creatCharacter = new CreatCharacter
                    {
                        characterId = NPCData.linkCharacterId,
                        instanceId = instanceId,
                    };
                    GameActionManager.instance.QueueAction(creatCharacter);
                }
            }
        }
        var shopManager = ShopManager.instance;

    }
    public void TryContinueBehavior(TryContinueBehavior tryContinueBehavior)
    {
        if(GetNPCFormInstance(tryContinueBehavior.characterId,out var npc))
        {
            npc.RemoveOverrideHold(); 
            npc.ResetCharacterBehavior();
        }
    }
}