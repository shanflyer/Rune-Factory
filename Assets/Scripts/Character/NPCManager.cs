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
    public Sprite head;
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
        characterInformationData.head = characterData.head.sprite;

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
    }

    protected override void Clear()
    {
        base.Clear();
        npcs.Dispose();
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
                    dataId = NPCData.id
                };
                npcs.SetData(npc);
            }
        }
    }
}