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
using static UnityEngine.GraphicsBuffer;

public interface IFightCharacter
{
    public CharacterProperty characterProperty { get;}
    public int instanceId { get; set; } 
    public int behaviorId { get; set; }
    public List<SkillRuntime> skillRuntimes { get; set; }
    public bool CheckAction();
    public void CreatSkillRuntime(IGameData gameData=null);
    public Dictionary<FightType, List<int>> GetReadySkills(FightType fightType=FightType.All);
}

public struct FightPlayer : IFightCharacter
{
    public int instanceId { get ; set ; }
    public int behaviorId { get; set; }
     

    public List<SkillRuntime> skillRuntimes { get => _skillRuntimes; set => _skillRuntimes=value; }
    public CharacterProperty characterProperty 
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


    private List<SkillRuntime> _skillRuntimes;

    public int dataId;

    public bool CheckAction()
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        if (character.CharacterProperty.HP > 0)
        {
            return true;
        }

        return false;
    }
    public Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
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
    public async void CreatSkillRuntime(IGameData gameData= null)
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        skillRuntimes=new List<SkillRuntime>();
        for (int i = 0; i < character.skills.Count; i++)
        {
            int skillId = character.skills[i];
            SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
            skillRuntimes.Add(skillRuntime);
        }
    }
}
public struct FightMonster : IFightCharacter
{
    public int instanceId { get; set; }
    public int behaviorId { get; set; }

    public List<SkillRuntime> skillRuntimes { get => _skillRuntimes; set => _skillRuntimes = value; }
    CharacterProperty IFightCharacter.characterProperty { get => characterProperty;}

    private List<SkillRuntime> _skillRuntimes;
    public int dataId;


    public CharacterProperty characterProperty;
    public Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
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
    public bool CheckAction()
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        if (character.CharacterProperty.HP > 0)
        {
            return true;
        }

        return false;
    }
    public async void CreatSkillRuntime(IGameData gameData)
    {
        MonsterData monsterData=gameData as MonsterData;
        if(monsterData!=null)
        {
            skillRuntimes = new List<SkillRuntime>();
            for (int i = 0; i < monsterData.skills.Count; i++)
            {
                int skillId = monsterData.skills[i];
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime);
            }
        }
    }
}

public enum HurtResultType
{
    Default=0,暴击=1,Miss=2
}
public class FightManager :Singleton<FightManager>
{
    MyInstance myInstance = new MyInstance(); 

    Dictionary<int, IFightCharacter> fightCharacters = new Dictionary<int, IFightCharacter>();
    List<int> fightPlayers = new List<int>();
    List<int> fightMonsters = new List<int>();

    public FightController fightController;
    
    public override void Init()
    {
        base.Init();

        maxRundCount = Enum.GetValues(typeof(FightRundType)).Length;

        GameActionManager.instance.AddListener<CreatFightPlayer>(CreatFightPlayer); 
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
            fightMonster.characterProperty.HP = monsterData.HP;
            fightMonster.characterProperty.AT = monsterData.AT;
            fightMonster.characterProperty.DF = monsterData.DF;
            fightMonster.characterProperty.Crit = monsterData.Crit;
            fightMonster.characterProperty.Dodge = monsterData.Dodge;
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
                                    IFightCharacter tagetFighter = fightCharacters[t];
                                    int hurt = HurtValue(fightCharacter.characterProperty.AT, tagetFighter.characterProperty.DF,
                                        fightCharacter.characterProperty.Crit, fightCharacter.characterProperty.Dodge,
                                        tagetFighter.characterProperty.DF, out var hurtResultType);
                                    hurt = math.clamp(hurt, 0, 1);
                                    hurtValue += (1 - tagetFighter.characterProperty.HP / hurt) *(1 - GameCommon.HurtUtlility) + GameCommon.HurtUtlility;

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

    List<int> GetTarget(TargetType targetType,IFightCharacter fightCharacter,int targetCount)
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

        List<IFightCharacter> nowFighrCharacters = new List<IFightCharacter>();
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
    IFightCharacter fightSource;
    List<IFightCharacter> fightTargets = new List<IFightCharacter>();

    void SelectFightTarget(IFightCharacter fightCharacter, TargetType targetType,int count,bool repeatedSelect=false)
    {
        fightTargets.Clear();
        switch(targetType)
        {
            case TargetType.自身:
                fightTargets.Add(fightCharacter);
                break;
            case TargetType.敌方:

                break;
        }
    }
}

public enum FightRundType
{
    Player,Monster
}
