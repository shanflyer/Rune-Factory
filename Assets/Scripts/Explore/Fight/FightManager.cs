using NUnit.Framework.Interfaces;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OldName;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Purchasing;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;

public class FightCharacter
{
    public virtual CharacterProperty characterProperty { get; }
    public int instanceId { get; set; } 
    public int behaviorId { get; set; }
    public Dictionary<int,SkillRuntime> skillRuntimes { get; set; }
    public virtual bool CheckAction() { return false; }
    public virtual void CreatSkillRuntime(IGameData gameData = null)
    {

    }

    public virtual void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    { 
    }
    public virtual  Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All) 
    {
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        return results;
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

    private Character character
    {
        get
        {
            if (_character == null)
            {
                _character = CharacterManager.instance.GetCharacter(instanceId);
            }
            return _character;
        }
    }
    private Character _character;
     

    public int dataId;

    public override bool CheckAction()
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        if (character.CharacterProperty.HP > 0)
        {
            return true;
        }

        return false;
    }
    public override Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
    {
        var character = CharacterManager.instance.GetCharacter(instanceId); 
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>(); 
        for(int i=0;i<skillRuntimes.Count;i++)
        {
            if (fightType != FightType.All && skillRuntimes[i].fightType != fightType)
            {
                continue;
            }
            if (skillRuntimes[i].skillCd == 0 && character.CharacterProperty.MP > skillRuntimes[i].cost)
            {
                
                if (!results.TryGetValue(skillRuntimes[i].fightType,out var skills))
                {
                    skills = new List<int>();
                    results.Add(skillRuntimes[i].fightType, skills);
                }
                skills.Add(skillRuntimes[i].instanceId);
            }             
        }
        return results;
    }
    public async override void CreatSkillRuntime(IGameData gameData= null)
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        skillRuntimes=new Dictionary<int, SkillRuntime>();
        for (int i = 0; i < character.skills.Count; i++)
        {
            int skillId = character.skills[i];
            SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
            skillRuntimes.Add(skillRuntime.instanceId,skillRuntime);
        }
    }
    public override void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        character.SetProperty(setCharacterProperty);
    }
}
public class FightMonster : FightCharacter
{  
    public int dataId;  
    public override Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
    { 
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        for (int i = 0; i < skillRuntimes.Count; i++)
        {
            if (fightType != FightType.All&&skillRuntimes[i].fightType != fightType)
            {
                continue;
            }
            if (skillRuntimes[i].skillCd == 0)
            {
                if (!results.TryGetValue(skillRuntimes[i].fightType, out var skills))
                {
                    skills = new List<int>();
                    results.Add(skillRuntimes[i].fightType, skills);
                }
                skills.Add(skillRuntimes[i].instanceId);
            }
        }
        return results;
    }
    public override bool CheckAction()
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        if (character.CharacterProperty.HP > 0)
        {
            return true;
        }

        return false;
    }
    public async override void CreatSkillRuntime(IGameData gameData)
    {
        MonsterData monsterData=gameData as MonsterData;
        if(monsterData!=null)
        {
            skillRuntimes = new Dictionary<int, SkillRuntime>();
            for (int i = 0; i < monsterData.skills.Count; i++)
            {
                int skillId = monsterData.skills[i];
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime.instanceId,skillRuntime);
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

    public void InitCharacterProperty(MonsterData monsterData)
    {
        _characterProperty.HP = monsterData.HP;
        _characterProperty.AT = monsterData.AT;
        _characterProperty.DF = monsterData.DF;
        _characterProperty.Crit = monsterData.Crit;
        _characterProperty.Dodge = monsterData.Dodge;
    }

    public override void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        _characterProperty.SetProperty(setCharacterProperty);
        CharacterPropertyTrigger CharacterPropertyTrigger = new CharacterPropertyTrigger
        {
            characterId = instanceId,
            characterProperty = _characterProperty
        };
        GameActionManager.instance.QueueAction(CharacterPropertyTrigger);
    }
}

public enum HurtResultType
{
    Default=0,暴击=1,Miss=2
}
public class FightManager :Singleton<FightManager>
{
    MyInstance myInstance = new MyInstance(); 

    Dictionary<int, FightCharacter> fightCharacters = new Dictionary<int, FightCharacter>();
    List<int> fightPlayers = new List<int>();
    List<int> fightMonsters = new List<int>();

    public FightController fightController;
    
    public override void Init()
    {
        base.Init();

        maxRundCount = Enum.GetValues(typeof(FightRundType)).Length;

        GameActionManager.instance.AddListener<CreatFightPlayer>(CreatFightPlayer);
        GameActionManager.instance.AddListener<ActionSkillEstimate>(ActionSkillEstimate);
    }
    protected override void Clear()
    {
        myInstance.Clear();
        base.Clear();
    }
    public Dictionary<FightType, List<int>> GetReadySkills(int id,FightType fightType=FightType.All)
    {
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        if(fightCharacters.TryGetValue(id,out var fightCharacter))
        {
            return fightCharacter.GetReadySkills();
        }

        return results;
    }
    public void CreatFightPlayer(CreatFightPlayer creatFightPlayer)
    { 
        for (int i = 0; i < creatFightPlayer.players.Count; i++)
        {
            Character character = CharacterManager.instance.GetCharacterForDataId(creatFightPlayer.players[i]);
             
            FightPlayer fightPlayer = new FightPlayer
            {
                instanceId = character.instanceId,
                dataId = character.dataId,
            };
            fightPlayer.CreatSkillRuntime();
            fightCharacters.Add(fightPlayer.instanceId, fightPlayer);
            fightPlayers.Add(fightPlayer.instanceId);
            FightController.instance.CreatFightPlayer(character.dataId, character.instanceId, i);
        }

    }

    public bool GetFightCharacter(int id,out FightCharacter fightCharacter)
    {
        if(fightCharacters.TryGetValue(id,out fightCharacter))
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
        var player = CharacterManager.instance.player;
        FightPlayer fightPlayer = new FightPlayer
        {
            instanceId = player.instanceId,
            dataId = player.dataId,
        };
        fightCharacters.Add(fightPlayer.instanceId, fightPlayer);
        fightPlayers.Add(fightPlayer.instanceId); 
        FightController.instance.CreatFightPlayer(player.dataId, player.instanceId, 0);

        var teamPlayers = CharacterManager.instance.teamPlayers;
        if (teamPlayers != null && teamPlayers.Count > 0)
        {
            for(int i = 0; i < teamPlayers.Count; i++)
            {
                FightPlayer fightTeamPlayer = new FightPlayer
                {
                    instanceId = teamPlayers[i].instanceId,
                    dataId = teamPlayers[i].dataId,
                }; 

                fightCharacters.Add(fightTeamPlayer.instanceId, fightTeamPlayer);
                fightPlayers.Add(fightTeamPlayer.instanceId);

                FightController.instance.CreatFightPlayer(teamPlayers[i].dataId, teamPlayers[i].instanceId, i+1);
            }
        } 
        await  UIManager.instance.ShowGamePanel<FightPanel>(ExploreManager.instance.NowCharpter.ToString(),layer:2);
        RefreshFightPlayerInfo();
    }

    
    public async void CreatFightMonster(MonsterDeploy monsterDeploy)
    {
        var beforeAction =await GameDataManager.instance.GetAsyncData<GameActionData>(monsterDeploy.beforeActionId);
        if (beforeAction != null)
        {
            beforeAction.Action();
        }

        List<int> monsterIds = new List<int>();
        var randomResults = GameRandom.instance.GetRandomValue(monsterDeploy.refreshId);
        for(int i = 0; i < randomResults.Count; i++)
        {
            var result = randomResults[i];
            var characterGroupData = await GameDataManager.instance.GetAsyncData<CharacterGroupData>(result.result);
            if(characterGroupData != null)
            {
                monsterIds.AddRange(characterGroupData.characters);
            }
        }
        for(int i = 0; i < 6; i++)
        {
            if (monsterIds.Count <= i)
            {
                break;
            }
            MonsterData monsterData = await GameDataManager.instance.GetAsyncData<MonsterData>(monsterIds[i]);
            FightMonster fightMonster = new FightMonster
            {
                instanceId = myInstance.CreatInstanceId(),
                dataId = monsterData.id,
                behaviorId = monsterData.behaviorId,
               
            };
            fightMonster.InitCharacterProperty(monsterData); 
            fightMonster.CreatSkillRuntime(monsterData);
            fightCharacters.Add(fightMonster.instanceId, fightMonster);
            fightMonsters.Add(fightMonster.instanceId);

            FightController.instance.CreatFightMonster(monsterData, fightMonster.instanceId, i);
        }

        var afterAction = await GameDataManager.instance.GetAsyncData<GameActionData>(monsterDeploy.afterActionId);
        if (afterAction != null)
        {
            afterAction.Action();
        }
    }
    public int HurtValue(int AT,int DF,int Crit, int Dodge0, int Dodge1,out HurtResultType hurtResultType)
    {
        int hurt = AT - DF;
        hurt = math.clamp(hurt, 1, hurt);

        int dodgeValue = Dodge0 - Dodge1;
        hurtResultType = HurtResultType.Default;
        if (dodgeValue < 0)
        {
            int trueDodge =math.clamp( dodgeValue * 2,0,Dodge1);
            if (GameRandom.RandomInt(0, 100) < trueDodge)
            {
                hurtResultType = HurtResultType.Miss;
                return 0;
            }
            else
            {
                int trueCrit =  Crit- dodgeValue*2;
                if (GameRandom.RandomInt(0, 100) < trueCrit)
                {
                    hurtResultType = HurtResultType.暴击;
                    return (int)(hurt * GameRandom.RandomFloat(1.5f, 2.0f));
                }
            }
        }
        else
        {
            int trueDodge = math.clamp(math.abs(dodgeValue /2), 0, Dodge1);
            if (GameRandom.RandomInt(0, 100) < trueDodge)
            {
                hurtResultType = HurtResultType.Miss;
                return 0;
            }
            else
            {
                int trueCrit = math.abs(dodgeValue / 2)+Crit;
                if (GameRandom.RandomInt(0, 100) < trueCrit)
                {
                    hurtResultType = HurtResultType.暴击;
                    return (int)(hurt * GameRandom.RandomFloat(1.5f, 2.0f));
                }

            }
        }
        return hurt;
    }


    public SkillEstimateData EstimateSkill(int skillId, int characterId)
    {
        SkillEstimateData SkillEstimateData = new SkillEstimateData
        {
            source=characterId,
            skillId = skillId,
            target = new List<List<int>>(),
            utlilityValue = 1
        };
        if(fightCharacters.TryGetValue(characterId,out var fightCharacter))
        {
            for(int i = 0; i < fightCharacter.skillRuntimes.Count; i++)
            {
                if (fightCharacter.skillRuntimes[i].instanceId == skillId)
                {
                    var skillData = fightCharacter.skillRuntimes[i].skillData;
                    for(int j = 0; j < skillData.actionCount; j++)
                    {
                        var targets = GetTarget(skillData.targetType, fightCharacter, skillData.targetCount);
                        SkillEstimateData.target.Add(targets);
                    }


                    switch (skillData.skillActionType)
                    {
                        case SkillActionType.伤害:
                            float hurtValue = 0;
                            for (int x = 0; x < SkillEstimateData.target.Count;x++)
                            {
                                for (int y = 0; y < SkillEstimateData.target[x].Count; y++)
                                {
                                    int t = SkillEstimateData.target[x][y];
                                    FightCharacter tagetFighter = fightCharacters[t];
                                    int hurt = HurtValue(fightCharacter.characterProperty.AT, tagetFighter.characterProperty.DF,
                                        fightCharacter.characterProperty.Crit, fightCharacter.characterProperty.Dodge,
                                        tagetFighter.characterProperty.DF, out var hurtResultType);
                                    float _hurtValue = (1 - tagetFighter.characterProperty.HP / hurt) * (1 - GameCommon.HurtUtlility) + GameCommon.HurtUtlility;
                                    _hurtValue = math.clamp(_hurtValue, 0, 1);
                                    hurtValue += _hurtValue;
                                }
                                hurtValue = math.clamp(hurtValue, 0, 1);
                            }
                            SkillEstimateData.utlilityValue = hurtValue;
                            break;
                    }

                    break;
                }
            }
        }
        return SkillEstimateData;
    }

    List<int> GetTarget(TargetType targetType,FightCharacter fightCharacter,int targetCount)
    { 
        switch (targetType)
        {
            case TargetType.敌方: 
                if (fightPlayers.Contains(fightCharacter.instanceId))
                {
                    return GetRandomValue(fightMonsters, targetCount); 
                }
                else
                {
                    return GetRandomValue(fightPlayers, targetCount); 
                }
                 
            case TargetType.我方:
                if (fightPlayers.Contains(fightCharacter.instanceId))
                {
                    return GetRandomValue(fightPlayers, targetCount);
                }
                else
                {
                    return GetRandomValue(fightMonsters, targetCount);
                } 
            case TargetType.自身:
                List<int> result = new List<int>();
                result.Add(fightCharacter.instanceId);
                return result;
        }
        return null;
    }

    List<int> GetRandomValue(List<int> characters,int targetCount)
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

        for (int i = 0; i < fightMonsters.Count; i++)
        {
            RandomItem randomItem = new RandomItem
            {
                itemId = fightMonsters.Count - i,
                itemValue = fightMonsters[fightMonsters.Count - i].ToString(),
                randomValue = 20 * i,
                maxCount = 1,
                minCount = 1
            };
            gameRandomData.randomItems.Add(randomItem);

        }
        gameRandomData.Pretreatment();

        var results = GameRandom.instance.GetRandomValue(gameRandomData, targetCount);
        for (int i = 0; i < results.Count; i++)
        {
            result.Add(int.Parse(results[i].result));
        }
        return result;
    }


    void ActionSkillEstimate(ActionSkillEstimate actionSkillEstimate)
    {
        int skillId = actionSkillEstimate.skillId;
        int sourceId = actionSkillEstimate.sourceId;
        int targetId = actionSkillEstimate.targetId;
        int index = actionSkillEstimate.index;

        FightCharacter source = fightCharacters[sourceId]; 
        FightCharacter target= fightCharacters[targetId];
        var skillRuntime = source.skillRuntimes[skillId];
        var skillData = skillRuntime.skillData;

        switch (skillData.skillActionType)
        {
            case SkillActionType.伤害:

                int hurt = HurtValue(source.characterProperty.AT, target.characterProperty.DF,
                    source.characterProperty.Crit, source.characterProperty.Dodge,
                    target.characterProperty.DF, out var hurtResultType);
                int hp = target.characterProperty.HP - hurt;
                hp = math.clamp(hp, 0, hp);
                SetCharacterProperty setCharacterProperty = new SetCharacterProperty
                {
                    characterId = target.instanceId,
                    propertyType = CharacterPropertyType.生命,
                    setValue = hp
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
                break;
        }
    }


    FightRundType nowFightRund;
    int maxRundCount;
    void InitFightCharacter()
    {
        int rundType = (int)nowFightRund;
        rundType++;
        if (rundType > maxRundCount)
        {
            rundType = 0;
        }
        nowFightRund = (FightRundType)rundType;

        List<FightCharacter> nowFighrCharacters = new List<FightCharacter>();
        switch (nowFightRund)
        {
            case FightRundType.Player:
                for(int i = 0; i < fightPlayers.Count; i++)
                {
                    int id = fightPlayers[i];
                    if (fightCharacters[id].CheckAction())
                    {
                        nowFighrCharacters.Add(fightCharacters[id]);
                    }
                }
                break;
            case FightRundType.Monster:
                for (int i = 0; i < fightMonsters.Count; i++)
                {
                    int id = fightMonsters[i];
                    if (fightCharacters[id].CheckAction())
                    {
                        nowFighrCharacters.Add(fightCharacters[id]);
                    }
                }
                break;
        }
    }
    FightCharacter fightSource;
    List<FightCharacter> fightTargets = new List<FightCharacter>();

     
}

public enum FightRundType
{
    Player,Monster
}
