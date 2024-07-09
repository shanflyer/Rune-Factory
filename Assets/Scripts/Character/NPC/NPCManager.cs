using BehaviorDesigner.Runtime;
using OfficeOpenXml.ConditionalFormatting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

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
    
    public void SetTargetArea(int area)
    {
        targetArea = area;
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
        if (!tempCharacterData.levelBehavior.TryGetValue(templevel, out var externalBehaviorTree))
        {
            externalBehaviorTree = tempCharacterData.defaultBehavior;
        }
        CharacterBehaviorManager.instance.DestroyBehavior(instanceId);
        CharacterBehaviorManager.instance.AddBehavior(instanceId, externalBehaviorTree);
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

    public NPCState npcState;
    public NPCData npcData;
    public bool isActive;
    public int characterId;
    public bool hide=>npcData.hide;

    public void SetNPCTaskScheduleTimeList(List<int> dailyTasks)
    {
        List<NPCTaskScheduleData> timeTaskSheduleDatas = new List<NPCTaskScheduleData>();
        for(int i=0;i<dailyTasks.Count;i++)
        {
            if(NPCTaskScheduleManager.instance.GetTaskScheduleData(dailyTasks[i],out var nPCTaskScheduleData))
            {
                timeTaskSheduleDatas.Add(nPCTaskScheduleData);
            } 
        }
        nPCTaskScheduleTimeList = new NPCTaskScheduleTimeList(timeTaskSheduleDatas);
    }
    public async Task<CharacterData> GetCharacterData()
    { 
        CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(npcData.linkCharacterId);
        return characterData;
    }

    private NPCTaskScheduleTimeList nPCTaskScheduleTimeList;

    public bool SetNowBehaviorTree()
    {
        var exterNalBehavior = GetNowTaskScheduleBehavior();
        if (exterNalBehavior != null)
        {
            var taskSheduleData = nPCTaskScheduleTimeList.GetNowTaskSheduleData();
            CharacterBehaviorManager.instance.AddBehavior(characterId, exterNalBehavior, taskSheduleData.loopBehavior);
            return true;
        }
        return false;
    }
    void EndNowBehaviorTree()
    {
        NPCTaskScheduleData nPCTaskScheduleData=null;
        if (nPCTaskScheduleTimeList.GetTaskScheduleDataOrder(new int2(GameTimeManager.instance.Hour, GameTimeManager.instance.Minute),ref nPCTaskScheduleData))
        {
            CharacterBehaviorManager.instance.AddBehavior(characterId, nPCTaskScheduleData.externalBehavior, nPCTaskScheduleData.loopBehavior);
        }
        else
        {
            ReStartCharacterBehavior reStartCharacterBehavior = new ReStartCharacterBehavior
            {
                characterId = characterId
            };
            GameActionManager.instance.QueueAction(reStartCharacterBehavior, true);
        }
    }
    public ExternalBehaviorTree GetNowTaskScheduleBehavior()
    { 
        var data = nPCTaskScheduleTimeList.GetTaskScheduleData(new int2(GameTimeManager.instance.Hour,GameTimeManager.instance.Minute));
        if (data != null)
        {
            return data.externalBehavior;
        }
        return null;
    }
    public void Dispose()
    {
    }

    public int Key => npcData.id;

  

}

public class NPCTaskScheduleTimeList
{
    private List<NPCTaskScheduleData> timeTaskSheduleDatas = new List<NPCTaskScheduleData>();
    private List<GameTimeKey> gameTimeKeys = new List<GameTimeKey>();

    public NPCTaskScheduleData GetNowTaskSheduleData()
    {
        return timeTaskSheduleDatas[nowTimeKeyIndex];
    }
    public NPCTaskScheduleTimeList(List<NPCTaskScheduleData> timeTaskSheduleDatas)
    {
        this.timeTaskSheduleDatas = timeTaskSheduleDatas;
        gameTimeKeys.Clear();
        for(int i = 0; i < timeTaskSheduleDatas.Count; i++)
        {
            var timeTaskSheduleData = timeTaskSheduleDatas[i];
            gameTimeKeys.Add(new GameTimeKey(timeTaskSheduleData.gameTimeRange));
        }
        nowTimeKeyIndex = 0;
    }
    int nowTimeKeyIndex;
    public bool GetTaskScheduleDataOrder(int2 time,ref NPCTaskScheduleData nPCTaskScheduleData)
    {
        if (gameTimeKeys[nowTimeKeyIndex] == time)
        {
            return false;
        }
        nowTimeKeyIndex ++;
        if(nowTimeKeyIndex >= gameTimeKeys.Count)
        {
            nowTimeKeyIndex = 0;
        }
        nPCTaskScheduleData = timeTaskSheduleDatas[nowTimeKeyIndex]; 
        
        return true;
    }
    public NPCTaskScheduleData GetTaskScheduleData(int2 time)
    {
        for(int i = 0; i < gameTimeKeys.Count; i++)
        {
            if (gameTimeKeys[i]==time)
            {
                nowTimeKeyIndex = i;
                return timeTaskSheduleDatas[i];
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

    }
    private void GiveGift(GiveGift giveGift)
    {
        if (GetNPCFormInstance(giveGift.receiveCharacter, out var npc))
        {
            Character receiver = CharacterManager.instance.GetCharacter(giveGift.receiveCharacter);

            int likeState = 0;
            NPCData nPCData = npc.npcData;
            if (nPCData.likeItems.Contains(giveGift.giftId))
            {
                likeState = 1;
            }
            else if (nPCData.unLikeItems.Contains(giveGift.giftId))
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
                    talkRandomResults = GameRandom.instance.GetRandomValue(nPCData.likeTalk);
                    emoteRandomResults = GameRandom.instance.GetRandomValue(nPCData.likeEmote);
                    break;

                case 0:
                    friendValue = 2;
                    talkRandomResults = GameRandom.instance.GetRandomValue(nPCData.defaultTalk);
                    emoteRandomResults = GameRandom.instance.GetRandomValue(nPCData.defaultEmote);
                    break;

                case -1:
                    talkRandomResults = GameRandom.instance.GetRandomValue(nPCData.unlikeTalk);
                    emoteRandomResults = GameRandom.instance.GetRandomValue(nPCData.unlikeEmote);
                    break;
            }
            talkId = talkRandomResults[0].x;
            emoteId = emoteRandomResults[0].x;
            GameTimerController.instance.DelayAction(1000, () =>
            {
                if (CharacterManager.instance.controllerCharacter.instanceId == giveGift.giveCharacter)
                {
                    Talk talk = new Talk
                    {
                        characterId = giveGift.receiveCharacter,
                        talkId = talkId,
                        displayFunction = false,
                        endAction = () =>
                        {
                            CharacterManager.instance.controllerCharacter.SetNeighborhood(giveGift.receiveCharacter);
                        }
                    };
                    GameActionManager.instance.QueueAction(talk);

                    AddFriendShipValue addFriendShipValue = new AddFriendShipValue
                    {
                        characterId = giveGift.receiveCharacter,
                        friendAddType = FriendAddType.礼物,
                        value = friendValue
                    };
                    GameActionManager.instance.QueueAction(addFriendShipValue);
                }
                ShowEmote showEmote = new ShowEmote
                {
                    emoteId = emoteId,
                    entityType = EntityType.角色,
                    id = giveGift.receiveCharacter
                };
                GameActionManager.instance.QueueAction(showEmote);
            });
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