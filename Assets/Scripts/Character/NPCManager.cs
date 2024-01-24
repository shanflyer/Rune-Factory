using System.Collections.Generic;
using System.Threading.Tasks;
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
    public SpriteResourceRenference head;
    public int characterId;
    public bool isNpc;
    public NPCState NPCState;
    public CharacterProperty characterProperty;
    public int level;
    public Exp exp;
    public Equip equip;
}

public class TempCharacter : Character
{
    public int tempDataId => _tempDataId;
    private int _tempDataId;

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

    public int _templevel;

    public TempCharacter(CharacterData characterData, int instanceId, int tempDataId) : base(characterData, instanceId)
    {
        this._tempDataId = tempDataId;
    }

    private async void RefreshBehavior()
    {
        var tempCharacterData = await GameDataManager.instance.GetAsyncData<TempCharacterData>(tempDataId);
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
    public Player(CharacterData characterData, int instanceId) : base(characterData, instanceId)
    {
    }

    protected override async Task CreatCharacterPackage(int overridePackageId = 0)
    {
        await base.CreatCharacterPackage();
        PackageManager.instance.AddPlayerPackage(overridePackageId == 0 ? characterPackage : overridePackageId);
    }

    public Player(string name)
    {
        this.name = name;
        //bag = PackageManager.instance.CreatGamePackage(10, "PlayerBag",instanceId);
        //临时
    }

    public Player(CharacterSaveData characterSaveData)
    {
        this.name = characterSaveData.name;
        //SetProperty(characterSaveData.energy, characterSaveData.health, characterSaveData.satiety);
        //bag = PackageManager.instance.CreatGamePackage(characterSaveData.packageCount, characterSaveData.packageName, instanceId);
        //foreach(var item in characterSaveData.items)
        //{
        //    PackageManager.instance.SetItemInPackage(item, bag);
        //}
    }
}

public partial class Character
{
    public int selectItem;

    public CharacterInformationData GetInformation()
    {
        CharacterInformationData characterInformationData = new CharacterInformationData();
        characterInformationData.characterId = instanceId;
        characterInformationData.characterProperty = characterProperty;
        characterInformationData.level = level;
        characterInformationData.exp = exp;
        characterInformationData.equip = equip;
        characterInformationData.name = name;
        characterInformationData.head = characterData.head;

        if (NPCManager.instance.GetNPC(instanceId, out var npc))
        {
            characterInformationData.isNpc = true;
            characterInformationData.NPCState = npc.npcState;
        }

        return characterInformationData;
    }
}

public struct NPCList : IReferenceData
{
    public List<NPC> npcs;
}

public struct NPC : INativeData, IReferenceData
{
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
    public int dataId;
    public int characterId;
    public bool hide;

    public async Task<CharacterData> GetCharacterData()
    {
        NPCData npcData = await GameDataManager.instance.GetAsyncData<NPCData>(dataId);
        CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(npcData.linkCharacterId);
        return characterData;
    }

    public void Dispose()
    {
    }

    public int Key => dataId;
}

public class NPCManager : Singleton<NPCManager>
{
    private MyNativeData<NPC> npcs = new MyNativeData<NPC>();
    private Dictionary<int, int> instanceDatas = new Dictionary<int, int>();

    public override void Init()
    {
        base.Init();
        npcs.Init(16); CreatZeroNPC();
        GameActionManager.instance.AddListener<GiveGift>(GiveGift);
    }

    protected override void Clear()
    {
        base.Clear();
        npcs.Dispose();
    }


    async void GiveGift(GiveGift giveGift)
    {  
        if (GetNPCFormInstance(giveGift.receiveCharacter, out var npc))
        {
            Character receiver = CharacterManager.instance.GetCharacter(giveGift.receiveCharacter);

            int likeState = 0;
            NPCData nPCData = await GameDataManager.instance.GetAsyncData<NPCData>(npc.dataId);
            if (nPCData.likeItems.Contains(giveGift.giftId))
            {
                likeState = 1;
            }else if (nPCData.unLikeItems.Contains(giveGift.giftId))
            {
                likeState = -1;
            }
            int talkId = 0;
            int emoteId = 0;
            int friendValue = 0;
            List<RandomResult> talkRandomResults = new List<RandomResult>();
            List<RandomResult> emoteRandomResults = new List<RandomResult>();
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
            talkId =int.Parse(talkRandomResults[0].result);
            emoteId = int.Parse(emoteRandomResults[0].result);

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
        npc = default(NPC);
        if (instanceDatas.TryGetValue(instanceId, out var id))
        {
            return npcs.GetData(id, out npc);
        }
        return false;
    }

    public bool GetNPC(int id, out NPC npc)
    {
        return npcs.GetData(id, out npc);
    }

    public NPCList GetNPCList()
    {
        NPCList nPCList = new NPCList
        {
            npcs = new List<NPC>(),
        };
        foreach(NPC npc in npcs)
        {
            if (!npc.hide)
            {
                nPCList.npcs.Add(npc);
            }
           
        }
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
                NPC npc = new NPC
                {
                    characterId = instanceId,
                    npcState = NPCState.正常,
                    dataId = NPCData.id,
                    hide=NPCData.hide
                };
                npcs.SetData(npc);
                instanceDatas[instanceId] = NPCData.id;
            }
        }
    }
}