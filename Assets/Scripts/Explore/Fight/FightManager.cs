using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public enum FightStatus
{
    准备, 行动,
}

public enum FightCharacterStaues
{
    正常, 死亡, 濒死
}

public class FightCharacter : IReferenceData
{
    public FightStatus fightStatus = FightStatus.准备;
    public FightCharacterStaues fightCharacterStaues = FightCharacterStaues.正常;
    public virtual AttributeType AttributeType { get; }
    public virtual CharacterProperty characterProperty { get; }
    public virtual Sprite icon { get; set; }
    public int instanceId { get; set; }
    public virtual int behaviorId { get; }
    public int2 fightPos;
    public Dictionary<int, SkillRuntime> skillRuntimes { get; set; }

    public virtual bool CheckAction()
    { return false; }

    public virtual void CreatSkillRuntime(IGameData gameData = null)
    {
    }

    public SkillRuntime GetSkillRuntime(int id)
    {
        if (skillRuntimes.TryGetValue(id, out var skillRuntime))
        {
            return skillRuntime;
        }
        return null;
    }

    public virtual int attackType { get; }

    private float PerRoundTime = 0;
    private float waiteTime = 0;
    public bool waiteEnd;
    public float WaiteValue()
    {
        float value =1- waiteTime / PerRoundTime;
        return math.clamp(value, 0, 1);
    }
    public void InitRundTime()
    {
        float speed = characterProperty.Speed;
        PerRoundTime = GameCommon.DefaultPerRoundCd * (100.0f / speed);
    }
    public void Reset()
    {
        fightStatus = FightStatus.准备;
        waiteTime = 0;
        waiteEnd = false;
    }
    public void UpData(float timeValue)
    {
        if (fightCharacterStaues == FightCharacterStaues.正常)
        {
            if (waiteTime < PerRoundTime)
            {
                waiteTime += timeValue;

                if (waiteTime >= PerRoundTime)
                {
                    waiteEnd = true;
                    foreach (var skillRuntime in skillRuntimes)
                    {
                        skillRuntime.Value.UpData(1);
                    }
                }
            }
        }
    }

    public virtual void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        InitRundTime();
    }

    public virtual Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
    {
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        return results;
    }

    public virtual void Clear()
    {
    }
}

public class FightPlayer : FightCharacter
{
    public override CharacterProperty characterProperty
    {
        get
        {
            return character.CharacterProperty;
        }
    }

    public Character character;
    public override Sprite icon { get => character.characterData.icon.sprite; set => base.icon = value; }
    public override AttributeType AttributeType => character.AttributeType;
    private int characterSkill, equipSkill;

    public SkillRuntime GetPlayerEquipSkill()
    {
        return GetSkillRuntime(equipSkill);
    }

    public int dataId=>character.dataId;

    public override int attackType
    {
        get
        {
            return character.attackType;
        }
    }

    public override bool CheckAction()
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        if (character.CharacterProperty.HP > 0 && fightCharacterStaues == FightCharacterStaues.正常 && fightStatus == FightStatus.准备)
        {
            return true;
        }

        return false;
    }

    public override Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
    {
        var character = CharacterManager.instance.GetCharacter(instanceId);
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        using (var e = skillRuntimes.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var skillRuntime = e.Current.Value;
                if (fightType != FightType.All && skillRuntime.fightType != fightType)
                {
                    continue;
                }
                if (skillRuntime.instanceId == equipSkill)
                {
                    continue;
                }
                if (skillRuntime.waiteCDEnd && character.CharacterProperty.MP >= skillRuntime.skillData.cost)
                {
                    if (!results.TryGetValue(skillRuntime.fightType, out var skills))
                    {
                        skills = new List<int>();
                        results.Add(skillRuntime.fightType, skills);
                    }
                    skills.Add(skillRuntime.instanceId);
                }
            }
        }

        return results;
    }

    public override async void CreatSkillRuntime(IGameData gameData = null)
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        skillRuntimes = new Dictionary<int, SkillRuntime>();
        for (int i = 0; i < character.skills.Count; i++)
        {
            int skillId = character.skills[i];
            SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
            skillRuntimes.Add(skillRuntime.instanceId, skillRuntime); 
        }
        equipSkill = 0;
        var equip = character.Equip;
        ItemData weappon = await GameDataManager.instance.GetAsyncData<ItemData>(equip.weapon.x);
        if (weappon != null)
        {
            int skillId = weappon.typeValue;

            if (skillId != 0)
            {
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime.instanceId, skillRuntime); 

                equipSkill = skillRuntime.instanceId;
            }
        }
        ItemData clothes = await GameDataManager.instance.GetAsyncData<ItemData>(equip.clothes.x);
        if (clothes != null)
        {
            int skillId = clothes.typeValue;
            if (skillId != 0)
            {
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime.instanceId, skillRuntime); 
            }
        }
        ItemData shoes = await GameDataManager.instance.GetAsyncData<ItemData>(equip.shoes.x);
        if (shoes != null)
        {
            int skillId = shoes.typeValue;
            if (skillId != 0)
            {
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime.instanceId, skillRuntime); 
            }
        }
    }

    public override void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        character.SetProperty(setCharacterProperty);
        base.SetCharacterValue(setCharacterProperty);
    }

    public FightPlayer(Character character)
    {
        this.character = character;
        instanceId = character.instanceId;
        InitRundTime();
    }

  

    public override void Clear()
    {
        base.Clear(); 
    }
}

public class FightMonster : FightCharacter
{ 
    public override int attackType => monsterData.attackType; 

    public MonsterData monsterData;
    public override int behaviorId => monsterData.behaviorId;
    public override Sprite icon { get => monsterData.monsterSprite.sprite; set => base.icon = value; }
    public FightMonster(MonsterData monsterData,int instanceId,int2 fightPos)
    {
        this.monsterData = monsterData;
        this.instanceId = instanceId;
        this.fightPos = fightPos; 

        InitCharacterProperty();
        CreatSkillRuntime(monsterData);
        InitRundTime();
    }
    public override Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
    {
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();

        using (var e = skillRuntimes.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var skillRuntime = e.Current.Value;
                if (fightType != FightType.All && skillRuntime.fightType != fightType)
                {
                    continue;
                }
                if (skillRuntime.waiteCDEnd)
                {
                    if (!results.TryGetValue(skillRuntime.fightType, out var skills))
                    {
                        skills = new List<int>();
                        results.Add(skillRuntime.fightType, skills);
                    }
                    skills.Add(skillRuntime.instanceId);
                }
            }
        }

        return results;
    }

    public override bool CheckAction()
    {
        if (characterProperty.HP > 0 && fightCharacterStaues == FightCharacterStaues.正常 && fightStatus == FightStatus.准备)
        {
            return true;
        }

        return false;
    }

    public override async void CreatSkillRuntime(IGameData gameData)
    {
        MonsterData monsterData = gameData as MonsterData;
        if (monsterData != null)
        {
            skillRuntimes = new Dictionary<int, SkillRuntime>();
            for (int i = 0; i < monsterData.skills.Count; i++)
            {
                int skillId = monsterData.skills[i];
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime.instanceId, skillRuntime); 
            }
        }
    }

    public override CharacterProperty characterProperty
    {
        get
        {
            return _characterProperty;
        }
    }

    private CharacterProperty _characterProperty;
    public override AttributeType AttributeType => attributeType;
    private AttributeType attributeType;

    void InitCharacterProperty()
    {
        _characterProperty.HP = monsterData.HP;
        _characterProperty.AT = monsterData.AT;
        _characterProperty.DF = monsterData.DF;
        _characterProperty.Speed = monsterData.Speed;
        _characterProperty.Lucky = monsterData.Lucky;
        attributeType = monsterData.attributeType; 
    }

    public override void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        _characterProperty.SetProperty(setCharacterProperty);
        CharacterPropertyTrigger CharacterPropertyTrigger = new CharacterPropertyTrigger
        {
            characterId = instanceId,
            characterProperty = _characterProperty
        };
        GameActionManager.instance.QueueAction(CharacterPropertyTrigger, true);
        base.SetCharacterValue(setCharacterProperty);
        InitRundTime();
    }
}

public enum HurtResultType
{
    Default = 0, 暴击 = 1, Miss = 2
}

public class FightManager : Singleton<FightManager>
{
    private MyInstance myInstance = new MyInstance();
    public override bool NeedUpdata => true;
    private Dictionary<int, FightCharacter> fightCharacters = new Dictionary<int, FightCharacter>();
    private List<int> fightPlayers = new List<int>();
    private List<int> fightMonsters = new List<int>();
    private Dictionary<int2, int> singleMonsterDic = new Dictionary<int2, int>();
    private Dictionary<int2, List<int>> horizontalMonsterDic = new Dictionary<int2, List<int>>();
    private Dictionary<int2, List<int>> verticalMonsterDic = new Dictionary<int2, List<int>>();

    public override async void Init()
    {
        base.Init();

        maxRoundCount = Enum.GetValues(typeof(FightRoundType)).Length;

        GameActionManager.instance.AddListener<CreatFightPlayer>(CreatFightPlayer);
        GameActionManager.instance.AddListener<ActionSkillEstimate>(ActionSkillEstimate);

        deathTimeLineData = await GameSourceManager.instance.GetScriptableObject<MyTimeLineData>(DataPath.MonsterDeathPath);
        GameActionManager.instance.AddListener<CharacterDeath>(CharacterDeath);
        GameActionManager.instance.AddListener<CharacterLevelUp>(CharacterLevelUp);
        GameActionManager.instance.AddListener<ExploreEnd>(ExploreEnd);
        GameActionManager.instance.AddListener<AllCharacterTryAutoFight>(AllCharacterTryAutoFight);
        GameActionManager.instance.AddListener<StopAllCharacterAutoFight>(StopAllCharacterAutoFight);
        GameActionManager.instance.AddListener<SkillPauseAction>(SkillPauseAction);

        fightResult = new FightResult
        {
            fighterResults = new List<FighterResult>(),
            getItems = new List<Item>()
        };
        GetItemIndexs = new Dictionary<int, int>();
    }

    public FightResult FightResult
    { get { return fightResult; } }
    private FightResult fightResult;

    public List<FightCharacter> GetAllFightCharacters()
    {
        List<FightCharacter> _fightCharacters = new List<FightCharacter>();
        for(int i = 0; i < fightPlayers.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightPlayers[i],out var fightCharacter))
            {
                _fightCharacters.Add(fightCharacter);
            }
        }
        for (int i = 0; i < fightMonsters.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightMonsters[i], out var fightCharacter))
            {
                _fightCharacters.Add(fightCharacter);
            }
        }
        return _fightCharacters;
    }
    private void ExploreEnd(ExploreEnd exploreEnd)
    {
        myInstance.Clear();
        ClearCharacter();
        GetItemIndexs.Clear();
        fightResult.fighterResults.Clear();
        fightResult.getItems.Clear();
    }

    void SkillPauseAction(SkillPauseAction skillPauseAction)
    {
        pauseBehavior = skillPauseAction.pause;
    }

    protected override void Clear()
    {
        myInstance.Clear();

        ClearCharacter();
        GetItemIndexs.Clear();
        base.Clear();
    }

    private void ClearCharacter()
    {
        fightCharacters.Clear();
        fightPlayers.Clear();
        fightMonsters.Clear();

        singleMonsterDic.Clear();
        horizontalMonsterDic.Clear();
        verticalMonsterDic.Clear();
    }

    public int GetAttackType(int id)
    {
        if (fightCharacters.TryGetValue(id, out var fightCharacter))
        {
            return fightCharacter.attackType;
        }
        return 0;
    }

    private MyTimeLineData deathTimeLineData;

    private void CharacterLevelUp(CharacterLevelUp characterLevelUp)
    {
        if (fightPlayers.Contains(characterLevelUp.characterId))
        {
            for (int i = 0; i < fightResult.fighterResults.Count; i++)
            {
                var fighterResult = fightResult.fighterResults[i];
                fighterResult.levelUp = true;
                fightResult.fighterResults[i] = fighterResult;
                break;
            }
        }
    }

    private void StopAllCharacterAutoFight(StopAllCharacterAutoFight allCharacterTryAutoFight)
    {
        foreach (var fightCharacter in fightCharacters)
        {
            if (fightCharacter.Value.CheckAction())
            {
                FightController.instance.StopFightCharacter(fightCharacter.Key);
                fightCharacter.Value.fightStatus = FightStatus.准备;
            }
        }
    }

    private void AllCharacterTryAutoFight(AllCharacterTryAutoFight allCharacterTryAutoFight)
    {
        foreach (var fightCharacter in fightCharacters)
        {
            if (fightCharacter.Value.CheckAction())
            {
                FightController.instance.RunFightCharacter(fightCharacter.Key);
                fightCharacter.Value.fightStatus = FightStatus.行动;
            }
        }
    }

    private void CharacterDeath(CharacterDeath characterDeath)
    {
        //播放死亡效果
        TimeLineManger.instance.PlaySkillTimeline(characterDeath.characterId, default(SkillEstimateData),
              deathTimeLineData, () =>
              {
                  if (fightMonsters.Contains(characterDeath.characterId))
                  {
                      var monster = (FightMonster)fightCharacters[characterDeath.characterId];
                      singleMonsterDic.Remove(monster.fightPos);
                      horizontalMonsterDic.Remove(monster.fightPos.y);
                      verticalMonsterDic.Remove(monster.fightPos.x);

                      fightMonsters.Remove(characterDeath.characterId);
                      fightCharacters.Remove(characterDeath.characterId);
                      FightController.instance.RemoveFightPlayerRuntime(characterDeath.characterId);
                      GameActionManager.instance.QueueAction(new RefreshFightCharacterList());
                  }
                  else if (fightPlayers.Contains(characterDeath.characterId))
                  {
                      //fightPlayers.Remove(characterDeath.characterId);
                      fightCharacters[characterDeath.characterId].fightCharacterStaues = FightCharacterStaues.濒死;
                  }
              });

        MonsterDeathDrop(characterDeath.characterId);
    }

    private Dictionary<int, int> GetItemIndexs = new Dictionary<int, int>();

    //死亡掉落
    private void MonsterDeathDrop(int characterId)
    {
        if (fightMonsters.Contains(characterId))
        {
            var fightMonster = (FightMonster)fightCharacters[characterId]; 
            var dropResult = GameRandom.instance.GetRandomValue(fightMonster.monsterData.dropId);

            List<int2> items = new List<int2>();
            for (int i = 0; i < dropResult.Count; i++)
            {
                int itemId = int.Parse(dropResult[i].result);
                int count = dropResult[i].count;
                items.Add(new int2(itemId, count));

                if (GetItemIndexs.TryGetValue(itemId, out int index))
                {
                    var item = fightResult.getItems[index];
                    item.count += count;
                    fightResult.getItems[index] = item;
                }
                else
                {
                    var item = new Item
                    {
                        instanceId = -1,
                        dataId = itemId,
                        count = count
                    };
                    fightResult.getItems.Add(item);
                    GetItemIndexs.Add(itemId, fightResult.getItems.Count - 1);
                }

                AddPackageItem addPackageItem = new AddPackageItem
                {
                    packageId = 0,
                    itemDataId = itemId,
                    itemCount = count
                };
                GameActionManager.instance.QueueAction(addPackageItem);
            }
            FightController.instance.DisplayDropItem(items, characterId);
            ExploreManager.instance.SetChapterFindItem(items);
            //获得经验
            int exp = fightMonster.monsterData.exp;
            for (int i = 0; i < fightPlayers.Count; i++)
            {
                int fightPlayerId = fightPlayers[i];
                var fightPlayer = (FightPlayer)fightCharacters[fightPlayerId];
                fightPlayer.character.AddExp(exp);
            }
        }
    }

    public SkillRuntime GetPlayerEquipSkill(int characterId)
    {
        if (fightCharacters.TryGetValue(characterId, out var character))
        {
            if (character is FightPlayer fightPlayer)
            {
                return fightPlayer.GetPlayerEquipSkill();
            }
        }
        return null;
    }

    public Dictionary<FightType, List<int>> GetReadySkills(int id, FightType fightType = FightType.All)
    {
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        if (fightCharacters.TryGetValue(id, out var fightCharacter))
        {
            return fightCharacter.GetReadySkills(fightType);
        }

        return results;
    }

    private void CreatFightPlayer(CreatFightPlayer creatFightPlayer)
    {
        for (int i = 0; i < creatFightPlayer.players.Count; i++)
        {
            Character character = CharacterManager.instance.GetCharacterForDataId(creatFightPlayer.players[i]);

            FightPlayer fightPlayer = new FightPlayer(character);
            fightPlayer.fightPos = i; 

            fightPlayer.CreatSkillRuntime();
            fightCharacters.Add(fightPlayer.instanceId, fightPlayer);
            fightPlayers.Add(fightPlayer.instanceId);
            //FightController.instance.CreatFightPlayer(character.dataId, character.instanceId, i);

            FighterResult fighterResult = new FighterResult
            {
                Character = character,
            };
            fightResult.fighterResults.Add(fighterResult);
        }
    }

    public bool GetFightCharacter(int id, out FightCharacter fightCharacter)
    {
        if (fightCharacters.TryGetValue(id, out fightCharacter))
        {
            return true;
        }
        return false;
    }

    public void RefreshFightPlayerInfo()
    {
        RefreshFightCharactersInfo refreshFightCharactersInfo = new RefreshFightCharactersInfo
        {
            characters = new List<int>()
        };
        refreshFightCharactersInfo.characters.AddRange(fightPlayers);
        GameActionManager.instance.QueueAction(refreshFightCharactersInfo);
    }

    
    public async void CreatFightPlayer()
    {
        /*
        var player = CharacterManager.instance.player;
        FightPlayer fightPlayer = new FightPlayer
        {
            instanceId = player.instanceId,
            dataId = player.dataId,
        };
        fightCharacters.Add(fightPlayer.instanceId, fightPlayer);
        fightPlayers.Add(fightPlayer.instanceId);
        FightController.instance.CreatFightPlayer(player.dataId, player.instanceId, 0);*/

        var playerTeam = TeamManager.instance.playerTeam;
        if (playerTeam != null)
        {
            List<int> players = new List<int>();
            for (int i = 0; i < playerTeam.Teamers.Count; i++)
            {
                var character = playerTeam.Teamers[i].character;
                players.Add(character.dataId);
            }
            CreatFightPlayer CreatFightPlayer = new CreatFightPlayer
            {
                players = players
            };
            GameActionManager.instance.QueueAction(CreatFightPlayer, true);
        }
        RefreshFightPlayerInfo();
    }

    public async void CreatFightMonster(MonsterDeploy monsterDeploy)
    {
        var beforeAction = await GameDataManager.instance.GetAsyncData<GameActionData>(monsterDeploy.beforeActionId);
        if (beforeAction != null)
        {
            beforeAction.Action();
        }

        var randomResults = GameRandom.instance.GetRandomValue(monsterDeploy.refreshId);
        var result = randomResults[0];
        var characterGroupData = await GameDataManager.instance.GetAsyncData<CharacterGroupData>(result.result);
        for (int i = 0; i < characterGroupData.characters.Count; i++)
        {
            int characterId = characterGroupData.characters[i];
            if (characterId == 0)
            {
                continue;
            }
            int col = i / 3;
            int raw = i - col * 3 - 1;

            MonsterData monsterData = await GameDataManager.instance.GetAsyncData<MonsterData>(characterId);
            FightMonster fightMonster = new FightMonster(monsterData, myInstance.CreatInstanceId(), new int2(col, raw));   
            fightCharacters.Add(fightMonster.instanceId, fightMonster);
            fightMonsters.Add(fightMonster.instanceId);

            singleMonsterDic.Add(fightMonster.fightPos, fightMonster.instanceId);
            if (!horizontalMonsterDic.TryGetValue(fightMonster.fightPos.y, out var horizontalMonsters))
            {
                horizontalMonsters = new List<int>();
                horizontalMonsterDic.Add(fightMonster.fightPos.y, horizontalMonsters);
            }
            horizontalMonsters.Add(fightMonster.instanceId);
            if (!verticalMonsterDic.TryGetValue(fightMonster.fightPos.x, out var verticalMonsters))
            {
                verticalMonsters = new List<int>();
                verticalMonsterDic.Add(fightMonster.fightPos.x, verticalMonsters);
            }
            verticalMonsters.Add(fightMonster.instanceId);

            FightController.instance.CreatFightMonster(monsterData, fightMonster.instanceId, fightMonster.fightPos);

            GameActionManager.instance.QueueAction(new RefreshFightCharacterList());
        }

        var afterAction = await GameDataManager.instance.GetAsyncData<GameActionData>(monsterDeploy.afterActionId);
        if (afterAction != null)
        {
            afterAction.Action();
        }
    }

    public bool IsSurvival(int id)
    {
        if (fightCharacters.TryGetValue(id, out var fightCharacter))
        {
            if (fightCharacter.characterProperty.HP > 0)
            {
                return true;
            }
        }
        return false;
    }

    private float GetAttributeTypeValue(AttributeType attributeType0, AttributeType attributeType1)
    {
        if (attributeType0 == AttributeType.无 || attributeType1 == AttributeType.无)
        {
            return 1.0f;
        }
        int value = (int)attributeType0 - (int)attributeType1;
        if (math.abs(value) == 1)
        {
            if (value < 0)
            {
                return 1.25f;
            }
            return 0.75f;
        }
        if (math.abs(value) == 4)
        {
            if (value < 0)
            {
                return 0.75f;
            }
            return 1.25f;
        }
        return 1.0f;
    }

    public HurtResultType GetHurtResultType(int Lucky0, int Lucky1)
    {
        int LuckyValue = (Lucky0 - Lucky1) * 2;
        if (LuckyValue < 0)
        {
            int trueLucky = math.clamp(-LuckyValue, 0, Lucky1);
            if (GameRandom.RandomInt(0, 100) < trueLucky)
            {
                return HurtResultType.Miss;
            }
            else
            {
                float value = -LuckyValue * 1.0f / Lucky0;
                value = 1 - math.clamp(value, 0, 1);
                // int trueCrit = Lucky0 + LuckyValue*2;
                if (GameRandom.RandomFloat(0, 1.0f) < 0.05f * value)
                {
                    return HurtResultType.暴击;
                }
            }
        }
        else
        {
            int trueLucky = math.clamp(LuckyValue, 0, Lucky0);
            if (GameRandom.RandomInt(0, 100) < trueLucky)
            {
                return HurtResultType.暴击;
            }
            else
            {
                float value = LuckyValue * 1.0f / Lucky1;
                value = 1 - math.clamp(value, 0, 1);
                if (GameRandom.RandomFloat(0, 1.0f) < 0.05f * value)
                {
                    return HurtResultType.Miss;
                }
            }
        }
        return HurtResultType.Default;
    }

    public int HurtValue(int AT, int DF, int Lucky0, int Lucky1, out HurtResultType hurtResultType)
    {
        int hurt = 1;
        int ATValue = AT / 2;
        int DFValue = DF / 4;
        if (ATValue > DFValue)
        {
            hurt = ATValue - DFValue;
        }
        int hurtRandomAdd = hurt / 16 + 1;
        hurt += GameRandom.RandomInt(-hurtRandomAdd, hurtRandomAdd);
        hurt = math.clamp(hurt, 1, hurt);

        int LuckyValue = (Lucky0 - Lucky1) * 2;
        hurtResultType = HurtResultType.Default;
        if (LuckyValue < 0)
        {
            int trueLucky = math.clamp(-LuckyValue, 0, Lucky1);
            if (GameRandom.RandomInt(0, 100) < trueLucky)
            {
                hurtResultType = HurtResultType.Miss;
                return 0;
            }
            else
            {
                float value = -LuckyValue * 1.0f / Lucky0;
                value = 1 - math.clamp(value, 0, 1);
                // int trueCrit = Lucky0 + LuckyValue*2;
                if (GameRandom.RandomFloat(0, 1.0f) < 0.05f * value)
                {
                    hurtResultType = HurtResultType.暴击;
                    return (int)(hurt * GameRandom.RandomFloat(1.5f, 2.0f));
                }
            }
        }
        else
        {
            int trueLucky = math.clamp(LuckyValue, 0, Lucky0);
            if (GameRandom.RandomInt(0, 100) < trueLucky)
            {
                hurtResultType = HurtResultType.暴击;
                return (int)(hurt * GameRandom.RandomFloat(1.5f, 2.0f));
            }
            else
            {
                float value = LuckyValue * 1.0f / Lucky1;
                value = 1 - math.clamp(value, 0, 1);
                if (GameRandom.RandomFloat(0, 1.0f) < 0.05f * value)
                {
                    hurtResultType = HurtResultType.Miss;
                    return 0;
                }
            }
        }
        return hurt;
    }

    public List<SkillEstimateData> EstimateSkills(int skillId, int characterId)
    {
        List<SkillEstimateData> skillEstimateDatas = new List<SkillEstimateData>();
        if (fightCharacters.TryGetValue(characterId, out var fightCharacter))
        {
            if (fightCharacter.skillRuntimes.TryGetValue(skillId, out var skillRuntime))
            {
                var skillData = skillRuntime.skillData;

                var targets = GetTarget(skillData.targetType, fightCharacter, skillData.targetRangeType);
                for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
                {
                    var targetList = targets[targetIndex];
                    SkillEstimateData SkillEstimateData = new SkillEstimateData
                    {
                        source = characterId,
                        skillId = skillId,
                        targets = targetList,
                        utlilityValue = 1,
                    };
                    skillEstimateDatas.Add(SkillEstimateData);
                }

                switch (skillData.fightType)
                {
                    case FightType.攻击:
                        for (int i = 0; i < skillEstimateDatas.Count; i++)
                        {
                            var skillEstimateData = skillEstimateDatas[i];
                            Dictionary<int, int> targetHurtValueDic = new Dictionary<int, int>();
                            for (int j = 0; j < skillEstimateData.targets.Count; j++)
                            {
                                int targetId = skillEstimateData.targets[j];
                                FightCharacter tagetFighter = fightCharacters[targetId];
                                int hurt = 0;
                                if (skillData.skillActionType == SkillActionType.属性值)
                                {
                                    hurt = HurtValue(fightCharacter.characterProperty.AT, tagetFighter.characterProperty.DF,
                                  fightCharacter.characterProperty.Lucky, tagetFighter.characterProperty.Lucky, out var hurtResultType);
                                    hurt = (int)(hurt * GetAttributeTypeValue(fightCharacter.AttributeType, tagetFighter.AttributeType));
                                }
                                else
                                {
                                    hurt = skillData.actionValue;
                                }
                                if (!targetHurtValueDic.TryGetValue(targetId, out var value))
                                {
                                    value = hurt;
                                }
                                else
                                {
                                    value += hurt;
                                }
                                targetHurtValueDic[targetId] = value;
                                if (skillEstimateData.target == null)
                                {
                                    skillEstimateData.target = FightController.instance.GetSkillShowTarget(tagetFighter, skillData.targetRangeType, false);
                                }
                            }

                            foreach (var targetHurtValue in targetHurtValueDic)
                            {
                                FightCharacter tagetFighter = fightCharacters[targetHurtValue.Key];
                                float _hurtValue = (targetHurtValue.Value / tagetFighter.characterProperty.HP) * (1 - GameCommon.HurtUtlility) + GameCommon.HurtUtlility;
                                _hurtValue = math.clamp(_hurtValue, 0, 1);
                                skillEstimateData.utlilityValue += _hurtValue;
                            }

                           

                            skillEstimateDatas[i] = skillEstimateData;
                        }
                        break;

                    case FightType.回复:
                        for (int i = 0; i < skillEstimateDatas.Count; i++)
                        {
                            var skillEstimateData = skillEstimateDatas[i];
                            float cureValue = 0;
                            for (int j = 0; j < skillEstimateData.targets.Count; j++)
                            {
                                int targetId = skillEstimateData.targets[j];
                                FightCharacter tagetFighter = fightCharacters[targetId];
                                var targetCureValue = 1 - 1 / 1 + math.pow(math.E * GameCommon.hpUtlility, (-tagetFighter.characterProperty.HP / tagetFighter.characterProperty.MaxHP * 12 + 6));
                                cureValue += targetCureValue;

                                if (skillEstimateData.target == null)
                                {
                                    skillEstimateData.target = FightController.instance.GetSkillShowTarget(tagetFighter, skillData.targetRangeType, true);
                                }
                            }
                            skillEstimateData.utlilityValue = cureValue;
                            skillEstimateDatas[i] = skillEstimateData;
                        }
                        break;
                }
            }
        }
        return skillEstimateDatas;
    }

    private List<List<int>> GetTarget(TargetType targetType, FightCharacter fightCharacter, TargetRangeType targetRangeType)
    {
        List<List<int>> finalTargets = new List<List<int>>();
        List<FightCharacter> targetCharacters = new List<FightCharacter>();
        switch (targetType)
        {
            case TargetType.敌方:

                if (fightPlayers.Contains(fightCharacter.instanceId))
                {
                    for (int i = 0; i < fightMonsters.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightMonsters[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            targetCharacters.Add(targetCharacter);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < fightPlayers.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightPlayers[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            targetCharacters.Add(targetCharacter);
                        }
                    }
                }
                break;

            case TargetType.我方:
                if (fightMonsters.Contains(fightCharacter.instanceId))
                {
                    for (int i = 0; i < fightMonsters.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightMonsters[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            targetCharacters.Add(targetCharacter);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < fightPlayers.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightPlayers[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            targetCharacters.Add(targetCharacter);
                        }
                    }
                }
                break;

            case TargetType.自身:
                finalTargets.Add(new List<int> { fightCharacter.instanceId });
                return finalTargets;
        }

        switch (targetRangeType)
        {
            case TargetRangeType.单体:
                for (int i = 0; i < targetCharacters.Count; i++)
                {
                    var targetCharacter = targetCharacters[i];
                    finalTargets.Add(new List<int> { targetCharacter.instanceId });
                }
                break;

            case TargetRangeType.全部:
                {
                    List<int> targets = new List<int>();
                    for (int i = 0; i < targetCharacters.Count; i++)
                    {
                        var targetCharacter = targetCharacters[i];
                        targets.Add(targetCharacter.instanceId);
                    }
                    finalTargets.Add(targets);
                }
                break;

            case TargetRangeType.横向:
                {
                    Dictionary<int, List<int>> targets = new Dictionary<int, List<int>>();
                    for (int i = 0; i < targetCharacters.Count; i++)
                    {
                        var targetCharacter = targetCharacters[i];
                        if (!targets.TryGetValue(targetCharacter.fightPos.x, out var ints))
                        {
                            ints = new List<int>();
                            targets.Add(targetCharacter.fightPos.x, ints);
                        }
                        ints.Add(targetCharacter.instanceId);
                    }
                    foreach (var target in targets)
                    {
                        finalTargets.Add(target.Value);
                    }
                }
                break;

            case TargetRangeType.纵向:
                {
                    Dictionary<int, List<int>> targets = new Dictionary<int, List<int>>();
                    for (int i = 0; i < targetCharacters.Count; i++)
                    {
                        var targetCharacter = targetCharacters[i];
                        if (!targets.TryGetValue(targetCharacter.fightPos.y, out var ints))
                        {
                            ints = new List<int>();
                            targets.Add(targetCharacter.fightPos.y, ints);
                        }
                        ints.Add(targetCharacter.instanceId);
                    }
                    foreach (var target in targets)
                    {
                        finalTargets.Add(target.Value);
                    }
                }
                break;
        }
        return finalTargets;
    }

    private List<int> GetRandomValue(List<int> characters, int targetCount)
    {
        List<int> result = new List<int>();
        GameRandomData gameRandomData = new GameRandomData
        {
            id = -1,
            weightRandom = true,
            barrels = new List<WeightBarrel>(),
            randomItems = new List<RandomItem>(),
            text = "选择目标"
        };

        for (int i = 0; i < characters.Count; i++)
        {
            RandomItem randomItem = new RandomItem
            {
                itemId = characters.Count - i,
                itemValue = characters[characters.Count - 1 - i].ToString(),
                randomValue = 20 * i,
                maxCount = 1,
                minCount = 1
            };
            gameRandomData.randomItems.Add(randomItem);
        }
        gameRandomData.Pretreatment();

        var results = GameRandom.instance.GetRandomValue(gameRandomData, randomResultCount: targetCount);
        for (int i = 0; i < results.Count; i++)
        {
            result.Add(int.Parse(results[i].result));
        }
        return result;
    }

    private void ActionSkillEstimate(ActionSkillEstimate actionSkillEstimate)
    {
        cdTimeMoving = false;

        int skillId = actionSkillEstimate.skillId;
        int sourceId = actionSkillEstimate.sourceId;
        int targetId = actionSkillEstimate.targetId;
        int index = actionSkillEstimate.index;

        FightCharacter source = fightCharacters[sourceId];
        FightCharacter target = fightCharacters[targetId];
        var skillRuntime = source.skillRuntimes[skillId];
        var skillData = skillRuntime.skillData;

        ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
        {
            characterId = source.instanceId,
            propertyType = CharacterPropertyType.法力,
            changeValue = -skillData.cost
        };
        GameActionManager.instance.QueueAction(changeCharacterProperty, true);

        switch (skillData.fightType)
        {
            case FightType.攻击:

                int hurt = 0;
                HurtResultType hurtResultType;
                if (skillData.skillActionType == SkillActionType.属性值)
                {
                    hurt = HurtValue(source.characterProperty.AT, target.characterProperty.DF,
                    source.characterProperty.Lucky, target.characterProperty.Lucky, out hurtResultType);
                    hurt = (int)(hurt * GetAttributeTypeValue(source.AttributeType, target.AttributeType));
                }
                else
                {
                    hurt = skillData.actionValue;
                    hurtResultType = GetHurtResultType(source.characterProperty.Lucky, target.characterProperty.Lucky);
                }

                int hp = target.characterProperty.HP - hurt;
                hp = math.clamp(hp, 0, hp);
                SetCharacterProperty setCharacterProperty = new SetCharacterProperty
                {
                    characterId = target.instanceId,
                    propertyType = CharacterPropertyType.生命,
                    Value = hp
                };
                GameActionManager.instance.QueueAction(setCharacterProperty, true);
                if (actionSkillEstimate.displayHurt)
                {
                    DisplayHurt displayHurt = new DisplayHurt
                    {
                        targetId = targetId,
                        hurtValue = hurt,
                        hurtResultType = hurtResultType
                    };
                    GameActionManager.instance.QueueAction(displayHurt, true);
                }
                if (hp <= 0)
                {
                    CharacterDeath characterDeath = new CharacterDeath
                    {
                        characterId = targetId
                    };
                    GameActionManager.instance.QueueAction(characterDeath);
                }

                break;
        }
        RefreshFightCharacterInfo refreshFightCharacterInfo = new RefreshFightCharacterInfo
        {
            characterId = target.instanceId
        };
        GameActionManager.instance.QueueAction(refreshFightCharacterInfo, true);
        RefreshFightCharacterInfo refreshFightCharacterInfo1 = new RefreshFightCharacterInfo
        {
            characterId = source.instanceId
        };
        GameActionManager.instance.QueueAction(refreshFightCharacterInfo1, true);
    }

    private FightRoundType nowFightRound;
    private int maxRoundCount;

    public int GetActiveFightCharacterCount()
    {
        int ActiveCount = 0;
        foreach (var fightCharacter in fightCharacters)
        {
            if (fightCharacter.Value.fightCharacterStaues == FightCharacterStaues.正常 && fightCharacter.Value.characterProperty.HP > 0)
            {
                ActiveCount++;
            }
        }
        return ActiveCount;
    }

    public int GetActiveFightCharacterCount(int characterId, Force force)
    {
        int ActiveCount = 0;
        int playerCount = 0;
        int monsterCount = 0;
        foreach (var fightCharacter in fightCharacters)
        {
            if (fightCharacter.Value.fightCharacterStaues == FightCharacterStaues.正常 && fightCharacter.Value.characterProperty.HP > 0)
            {
                if (fightPlayers.Contains(fightCharacter.Key))
                {
                    playerCount++;
                }
                else
                {
                    monsterCount++;
                }
                ActiveCount++;
            }
        }
        if (force == Force.全部)
        {
            return ActiveCount;
        }
        if (force == Force.我方)
        {
            if (fightPlayers.Contains(characterId))
            {
                return playerCount;
            }
            return monsterCount;
        }
        else
        {
            if (fightMonsters.Contains(characterId))
            {
                return playerCount;
            }
            return monsterCount;
        }
    }
   
    
    public Queue<int> GetReadyFighter()
    {
        Queue<int> nowFightCharacters = new Queue<int>();
        foreach(var fightCharacter in fightCharacters)
        {
            if (fightCharacter.Value.waiteEnd)
            {
                nowFightCharacters.Enqueue(fightCharacter.Value.instanceId);
            }
        }
        return nowFightCharacters;
    }


    public  bool cdTimeMoving= false;
    bool pauseBehavior = false;

    public Queue<int> InitFightCharacter()
    {
        int roundType = (int)nowFightRound;
        roundType++;
        if (roundType >= maxRoundCount)
        {
            roundType = 0;
        }
        nowFightRound = (FightRoundType)roundType;

        Debug.Log($"回合轮转{nowFightRound}");

        Queue<int> nowFightCharacters = new Queue<int>();
        switch (nowFightRound)
        {
            case FightRoundType.Player:
                for (int i = 0; i < fightPlayers.Count; i++)
                {
                    int id = fightPlayers[i];
                    if (fightCharacters[id].CheckAction())
                    {
                        fightCharacters[id].fightStatus = FightStatus.准备;
                        nowFightCharacters.Enqueue(id);
                    }
                }
                break;

            case FightRoundType.Monster:
                for (int i = 0; i < fightMonsters.Count; i++)
                {
                    int id = fightMonsters[i];
                    if (fightCharacters[id].CheckAction())
                    {
                        fightCharacters[id].fightStatus = FightStatus.准备;
                        nowFightCharacters.Enqueue(id);
                    }
                }
                break;
        }

        return nowFightCharacters;
    }

    public void SetManualSelectTargets(TargetRangeType targetRangeType, int2 value)
    {
        List<int> targets = new List<int>();
        switch (targetRangeType)
        {
            case TargetRangeType.全部:
                for (int i = 0; i < fightMonsters.Count; i++)
                {
                    var fightCharacter = fightCharacters[fightMonsters[i]];
                    if (fightCharacter.characterProperty.HP > 0)
                    {
                        targets.Add(fightMonsters[i]);
                    }
                }
                break;

            case TargetRangeType.单体:
                if (singleMonsterDic.TryGetValue(value, out var monster))
                {
                    var fightCharacter = fightCharacters[monster];
                    if (fightCharacter.characterProperty.HP > 0)
                    {
                        targets.Add(monster);
                    }
                }
                break;

            case TargetRangeType.横向:

                if (horizontalMonsterDic.TryGetValue(value, out var ints))
                {
                    for (int i = 0; i < ints.Count; i++)
                    {
                        var fightCharacter = fightCharacters[ints[i]];
                        if (fightCharacter.characterProperty.HP > 0)
                        {
                            targets.Add(ints[i]);
                        }
                    }
                }
                break;

            case TargetRangeType.纵向:
                if (verticalMonsterDic.TryGetValue(value, out ints))
                {
                    for (int i = 0; i < ints.Count; i++)
                    {
                        var fightCharacter = fightCharacters[ints[i]];
                        if (fightCharacter.characterProperty.HP > 0)
                        {
                            targets.Add(ints[i]);
                        }
                    }
                }
                break;
        }
    }

    public List<FightMonster> GetAllFightMonster()
    {
        List<FightMonster> result = new List<FightMonster>();
        for (int i = 0; i < fightMonsters.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightMonsters[i], out var fightCharacter))
            {
                if (fightCharacter.characterProperty.HP > 0)
                {
                    result.Add((FightMonster)fightCharacter);
                }
            }
        }
        return result;
    }

    public bool2 IsFightEnd()
    {
        bool2 result = true;
        for (int i = 0; i < fightMonsters.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightMonsters[i], out FightCharacter fightCharacter))
            {
                if (fightCharacter.characterProperty.HP > 0)
                {
                    result.y = false;
                    break;
                }
            }
        }
        if (!result.y)
        {
            for (int i = 0; i < fightPlayers.Count; i++)
            {
                if (fightCharacters.TryGetValue(fightPlayers[i], out FightCharacter fightCharacter))
                {
                    if (fightCharacter.characterProperty.HP > 0)
                    {
                        result.x = false;
                        break;
                    }
                }
            }
        }

        return result;
    }

    public void RoundPlayer()
    {
        List<int> newFightPlayers = new List<int>();
        if (fightCharacters.Count > 1)
        {
            for (int i = 1; i < fightPlayers.Count; i++)
            {
                newFightPlayers.Add(fightPlayers[i]);
            }
            newFightPlayers.Add(fightPlayers[0]);
            fightPlayers = newFightPlayers;

            for (int i = 0; i < fightPlayers.Count; i++)
            {
                FightCharacterMove fightCharacterMove = new FightCharacterMove
                {
                    characterId = fightPlayers[i],
                    newIndex = i
                };
                GameActionManager.instance.QueueAction(fightCharacterMove, true);
            }
        }
    }

    public void EscapeAction()
    {
        int playerValue = 0;
        int monsterValue = 0;

        foreach (var fightCharacter in fightCharacters)
        {
            if (fightPlayers.Contains(fightCharacter.Key))
            {
                playerValue += fightCharacter.Value.characterProperty.Lucky;
            }
            else
            {
                monsterValue += fightCharacter.Value.characterProperty.Lucky;
            }
        }
        playerValue /= fightPlayers.Count;
        monsterValue /= fightMonsters.Count;
        var totalValue = 50 + playerValue - monsterValue;
        if (GameRandom.RandomInt(0, 100) < totalValue)
        {
            InformationController.instance.AddInformation("逃离成功!", true, true);
            for (int i = 0; i < fightMonsters.Count; i++)
            {
                int characterId = fightMonsters[i];
                fightCharacters.Remove(characterId);
                FightController.instance.RemoveFightPlayerRuntime(characterId);
            }
            fightMonsters.Clear();
            singleMonsterDic.Clear();
            horizontalMonsterDic.Clear();
            verticalMonsterDic.Clear();

            ExploreManager.instance.StepFightSucceed();
        }
        else
        {
            InformationController.instance.AddInformation("逃离失败!", true, true);
            nowFightRound = FightRoundType.Player;
            PlayerFight playerFight = new PlayerFight();
            GameActionManager.instance.QueueAction(playerFight);
        }
    }

    public List<int> GetTargets(int2 key, TargetRangeType targetRangeType)
    {
        List<int> targets = new List<int>();
        switch (targetRangeType)
        {
            case TargetRangeType.全部:
                targets = new List<int>();
                for (int i = 0; i < fightMonsters.Count; i++)
                {
                    var fightCharacter = fightCharacters[fightMonsters[i]];
                    if (fightCharacter.characterProperty.HP > 0)
                    {
                        targets.Add(fightMonsters[i]);
                    }
                }
                break;

            case TargetRangeType.单体:
                if (singleMonsterDic.TryGetValue(key, out var monster))
                {
                    var fightCharacter = fightCharacters[monster];
                    if (fightCharacter.characterProperty.HP > 0)
                    {
                        targets = new List<int> { monster };
                    }
                }
                break;

            case TargetRangeType.横向:
                if (horizontalMonsterDic.TryGetValue(key, out var ints))
                {
                    targets = new List<int>();
                    for (int i = 0; i < ints.Count; i++)
                    {
                        var fightCharacter = fightCharacters[ints[i]];
                        if (fightCharacter.characterProperty.HP > 0)
                        {
                            targets.Add(ints[i]);
                        }
                    }
                }
                break;

            case TargetRangeType.纵向:
                if (verticalMonsterDic.TryGetValue(key, out ints))
                {
                    targets = new List<int>();
                    for (int i = 0; i < ints.Count; i++)
                    {
                        var fightCharacter = fightCharacters[ints[i]];
                        if (fightCharacter.characterProperty.HP > 0)
                        {
                            targets.Add(ints[i]);
                        }
                    }
                }
                break;
        }
        return targets;
    }

    protected override void UpData()
    {
        base.UpData();
        if (cdTimeMoving&&!pauseBehavior)
        {
            foreach (var fightCharacter in fightCharacters)
            {
                if(fightCharacter.Value.characterProperty.HP > 0)
                {
                    fightCharacter.Value.UpData(Time.deltaTime);
                }
               
            }
        }
    }
}

public enum Force
{
    全部, 我方, 敌方
}

public enum FightRoundType
{
    Player, Monster
}

public struct FighterResult : IReferenceData
{
    public Character Character;
    public bool levelUp, skillUp;
}

public struct FightResult : IReferenceData
{
    public bool victory;
    public List<Item> getItems;
    public List<FighterResult> fighterResults;
}