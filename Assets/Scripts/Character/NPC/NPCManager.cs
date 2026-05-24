using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime;
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
    public bool Equals(IReferenceData other)
    {
        if(other is CharacterInformationData informationData)
        {
            return informationData.characterId == characterId;
        }
            return false;
    }
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

    public TempCharacter(CharacterData characterData, ProfessionData professionData, int instanceId, TempCharacterData tempCharacterData) : 
        base(characterData, professionData, instanceId,false)
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
            TryRemoveLinkMapItemCharacter tryRemoveLinkMapItemCharacter = new TryRemoveLinkMapItemCharacter
            {
                linkInstanceId = instanceId,
                mapItemInstanceId = linkItem
            };
            GameActionManager.instance.QueueAction(tryRemoveLinkMapItemCharacter, true);

            CharacterBehaviorManager.instance.DestroyBehavior(instanceId);
            CharacterBehaviorManager.instance.AddBehavior(instanceId, externalBehaviorTree);
        }
    }
}

public class Player : Character
{
    public Player(CharacterData characterData, int instanceId, string playerName, ProfessionData professionData, int overridePackage = 0) : 
        base(characterData, professionData, instanceId,true, overridePackage)
    {
        name = playerName;
    }

    protected override async Task CreatCharacterPackage(int overridePackageId = 0, int instanceId = 0)
    {
        await base.CreatCharacterPackage();
        PackageManager.instance.AddPlayerPackage(overridePackageId == 0 ? characterPackage : overridePackageId);

        RefreshShortcut refreshShortcut = new RefreshShortcut
        {
            packageId = overridePackageId == 0 ? characterPackage : overridePackageId
        };
        GameActionManager.instance.QueueAction(refreshShortcut);
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

        if (NPCManager.instance.GetNPCFormInstance(instanceId, out var npc))
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
    Season birthSeason;
    int birthDay;
    //NPCBehavior nPCBehavior;
    public NPC(int instanceId, NPCData nPCData, Season birthSeason, int birthDay, int startSleepHour)
    {
        characterInstance = instanceId;
        npcData = nPCData;
        npcState = NPCState.正常;
        
        this.birthSeason = birthSeason; 
        this.birthDay = birthDay;
        NPCTaskScheduleManager.instance.AddNPCBehavior(instanceId);
        this.startSleepHour = startSleepHour;
        if (startSleepHour >= 0) Debug.Log($"new Npc:{npcData.npcName}--startSleepHour:{startSleepHour}");
        // nPCBehavior = new NPCBehavior(instanceId);
    }
    public FestivalData GetNpcBirthDay()
    {
        FestivalData festivalData = new FestivalData
        {
            date = birthDay,
            season=birthSeason,
            festivalType = FestivalType.纪念,
            id = npcData.id,
            name = npcData.npcName + LanguageManage.SwitchStr(" 的生日"),
        };
        return festivalData;
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
        var CharacterInformationData= Character.GetInformation();
        CharacterInformationData.isNpc = true;
        return CharacterInformationData;
    }
    public int friendLevel=>FriendManager.instance.GetFriendShipLevel(npcData.id);
    
    public List<int> functions
    {
        get
        {
            List<int> _functions = new List<int>();
            int nowLevel = friendLevel;
            for(int i = 0; i < npcData.functionIds.Count; i++)
            {
                int level = npcData.friendLevels[i];
                if (nowLevel >= level)
                {
                    _functions.Add(npcData.functionIds[i]);
                }
            }

            return _functions;
        }
    }
    public List<int> likeItems => likeItemSet.ToList();
    private HashSet<int> likeItemSet = new HashSet<int>();
    private HashSet<int> unLikeItemSet = new HashSet<int>();
    public string shopName => npcData.shopName;

    public List<int2> Beds => NPCBehaviorData.beds;
    public List<int2> WorkItems => NPCBehaviorData.workItems;
    public int HomeMap => NPCBehaviorData.home;
    public int WorkMap => NPCBehaviorData.workMap;
    public int playerOperateEventId => npcData.playerOperateEventId;
    public int nextTalkEventId => npcData.nextTalkEventId;

    public NPCState npcState { get; set; } 
    public NPCData npcData { get; private set; }
    public bool isActive;

    private int resetDay = 0;

    public int startSleepHour { get; private set; }

    public void SetSleep(int startSleepHour)
    {
        // Debug.Log($"{character.name} setSleep {startSleepHour}");
        this.startSleepHour = startSleepHour;
        GameDataSaveManager.instance.loadGameSaveData.SetNpcSleepTime(npcData.id, startSleepHour);
    }
    public void Rest()
    {
        npcState = NPCState.修养中;
        resetDay = 2;
    }
    public void NewDay()
    {
        if (resetDay > 0)
        {
            resetDay--;
            if (resetDay <= 0)
            {
                npcState = NPCState.正常;
            }
        }
       
    }
    public bool CheckTeamFriend()
    {
        return friendLevel >= npcData.teamFriendShip;
    }
    public bool CheckNpcShop()
    {
        if (character.linkItem == 0)
        {
            return false;
        }
        else 
        {
            if (NPCTaskScheduleManager.instance.NowTaskName(characterInstance) == "看守柜台")
            {
                return true;
            }
        }
        return false;
    }

    public int GetHomeArea()
    {
        if (NPCBehaviorData.homeAreas.Count > 0)
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
    private NPCBehaviorData NPCBehaviorData;
    public async Task InitBehaviorDataAsync()
    {
        NPCBehaviorData = await GameDataManager.instance.GetAsyncData<NPCBehaviorData>(npcData.id);
        likeItemSet.Clear();
        unLikeItemSet.Clear();
        likeItemSet = ItemManager.instance.GetItemsForTag(NPCBehaviorData.likeItem);
        unLikeItemSet = ItemManager.instance.GetItemsForTag(NPCBehaviorData.unLikeItem);

      
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
        if (NPCBehaviorData!=null)
        {
            NPCTaskScheduleManager.instance.SetNPCTaskScheduleTimeList(characterInstance,NPCBehaviorData.dailyTasks, NPCBehaviorData.externalBehavior);
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

    

    public async Task<CharacterData> GetCharacterData()
    {
        CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(npcData.linkCharacterId);
        return characterData;
    }


    public void BackHome()
    {
        if (NPCBehaviorData != null && NPCBehaviorData.externalBehavior != null)
        {
            NPCTaskScheduleManager.instance.AddNpcBehavior(characterInstance, CharacterBehaviorManager.instance.backHomeExternalBehavior);
        }  
    }


    public bool SetTimeBehaviorTree(UpdateGameTime UpdateGameTime)
    {
        if (NPCBehaviorData == null || NPCBehaviorData.externalBehavior == null)
        {
            return false;
        }
        return NPCTaskScheduleManager.instance.SetNowBehaviorTree(characterInstance, UpdateGameTime);
    }



    public void Dispose()
    {
    }

    public int Key => npcData.id;

    public void GetGift(int giveCharacter, int giftId)
    {
        int likeState = 0;
        if (likeItemSet.Contains(giftId))
        {
            likeState = 1;
        }
        else if (unLikeItemSet.Contains(giftId))
        {
            likeState = -1;
        }
        int talkId = 0;
        int emoteId = 0;
        int friendValue = 0;
        List<int2> talkRandomResults = new List<int2>();
        List<int2> emoteRandomResults = new List<int2>();

        int mulValue = 1;
        if (GameTimeManager.instance.Season == birthSeason && GameTimeManager.instance.Day == birthDay)
        {
            mulValue = 2;
        }
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
                    value = friendValue*mulValue
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



public class NPCManager : Singleton<NPCManager>
{
    private MyDic<int, NPC> npcs = new MyDic<int, NPC>();
    private Dictionary<int, int> instanceDatas = new Dictionary<int, int>();

    public override void Init()
    {
        base.Init();
        npcs.Clear(); 
        GameActionManager.instance.AddListener<GiveGift>(GiveGift); 

        GameActionManager.instance.AddListener<CheckNpcShopLink>(CheckNpcShopLink);
        GameActionManager.instance.AddListener<TryNPCJoinTeam>(TryNPCJoinTeam);
        GameActionManager.instance.AddListener<NewDay>(NewDay);
    }

    public void TryWakeUp(bool force = false)
    {
        for (var i = 0; i < npcs.length; i++)
        {
            var npc = npcs[i];
            if (npc.startSleepHour >= 0)
            {
                var wakeUp = force ? true : GameRandom.RandomInt(0, 100) > 50;
                if (wakeUp)
                {
                    npc.SetSleep(-1);
                    var sleepHour = 0;
                    var nowHour = GameTimeManager.instance.Hour;
                    if (nowHour < npc.startSleepHour)
                        sleepHour = 24 - npc.startSleepHour + nowHour;
                    else
                        sleepHour = nowHour - npc.startSleepHour;
                    npc.Character.WakeUp(sleepHour);
                    NPCTaskScheduleManager.instance.SetNowBehaviorTree(npc.Character.instanceId);
                }
            }
        }
    }
    protected override void Clear()
    {
        base.Clear();
        npcs.Clear();
    }
    void NewDay(NewDay newDay)
    {
       for(int i = 0; i < npcs.length; i++)
        {
            npcs[i].NewDay();
        }
    }
    public List<FestivalData> GetNpcBirthFestivalDatas()
    {
        List<FestivalData> festivalDatas = new List<FestivalData>();
        for(int i = 0; i < npcs.length; i++)
        {
            festivalDatas.Add(npcs[i].GetNpcBirthDay());
        }
        return festivalDatas;
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
            AsyncTaskRunner.Run(npcs[i].InitBehaviorDataAsync(), nameof(InitNPCBehavior));
        }
    }
    void TryNPCJoinTeam(TryNPCJoinTeam tryNPCJoinTeam)
    {
        if(GetNPCFormInstance(tryNPCJoinTeam.characterId,out var npc))
        {
            if (npc.npcState == NPCState.修养中)
            {
                Talk talk = new Talk
                {
                    talkId = GameCommon.TeamHurt,
                    characterId = tryNPCJoinTeam.characterId
                };
                GameActionManager.instance.QueueAction(talk);
                return;
            }

            if (npc.CheckTeamFriend())
            {
                if (TeamManager.instance.playerTeam.TeamCharacters.Count >= 3)
                {
                    Talk talk = new Talk
                    {
                        talkId = GameCommon.TeamFull,
                        characterId = tryNPCJoinTeam.characterId
                    };
                    GameActionManager.instance.QueueAction(talk);
                }
                else
                {
                    TryRemoveLinkMapItemCharacter tryRemoveLinkMapItemCharacter = new TryRemoveLinkMapItemCharacter
                    {
                        linkInstanceId = tryNPCJoinTeam.characterId,
                        mapItemInstanceId = npc.Character.linkItem
                    };
                    GameActionManager.instance.QueueAction(tryRemoveLinkMapItemCharacter);
                    JoinTeam joinTeam = new JoinTeam
                    {
                        characterId = tryNPCJoinTeam.characterId,
                        teamCharacterId = tryNPCJoinTeam.teamCharacterId
                    };
                    GameActionManager.instance.QueueAction(joinTeam);
                }
            }
            else
            {
                Talk talk = new Talk
                {
                    talkId = npc.npcData.failTeamTalk,
                    characterId = tryNPCJoinTeam.characterId
                };
                GameActionManager.instance.QueueAction(talk);
            }
        }
        else
        {
            JoinTeam joinTeam = new JoinTeam
            {
                characterId = tryNPCJoinTeam.characterId,
                teamCharacterId = tryNPCJoinTeam.teamCharacterId
            };
            GameActionManager.instance.QueueAction(joinTeam);
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

    public void CreateZeroNPC()
    {
        AsyncTaskRunner.Run(CreateZeroNPCAsync, nameof(CreateZeroNPC));
    }

    public async System.Threading.Tasks.Task CreateZeroNPCAsync()
    {
        var NPCDatas = await GameDataManager.instance.GetAllAsyncData<NPCData>();
        for (int i = 0; i < NPCDatas.Count; i++)
        {
            var NPCData = NPCDatas[i];
            if (NPCData.zeroCreate)
            {
                var saveBirthDay=GameDataSaveManager.instance.UserGameSaveData.GetNpcBirthDay(NPCData.id);
                if (saveBirthDay.x == -1)
                {
                    saveBirthDay.x = GameRandom.RandomInt(1, 5);
                    saveBirthDay.y = GameRandom.RandomInt(1, 31);
                    GameDataSaveManager.instance.UserGameSaveData.SetNpcBirthDay(NPCData.id, (Season)saveBirthDay.x, saveBirthDay.y);
                }
                int instanceId = MyInstance.instance.CharacterId;
                var sleepTime = GameDataSaveManager.instance.UserGameSaveData.GetNpcSleepHour(NPCData.id);
                var npc = new NPC(instanceId, NPCData, (Season)saveBirthDay.x, saveBirthDay.y, sleepTime);
                npcs.Add(npc.Key, npc);
                instanceDatas[instanceId] = NPCData.id;
                FriendManager.instance.ZeroFriendShip(NPCData.id, NPCData.zeroFriendShipLevel);

                FestivalManager.instance.AddNPCBrothDay(npc.npcName, (Season)saveBirthDay.x, saveBirthDay.y,NPCData.id);

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
}
