using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TeamManager : Singleton<TeamManager>
{
    public override bool NeedUpdate => true;

    protected override void Update()
    {
        base.Update();
        for (var i = 0; i < teams.length; i++) teams[i].Update();
    }

    public Team playerTeam
    {
        get
        {
            try
            {
                if (teams.TryGetValue(CharacterManager.instance.controllerCharacter.instanceId, out var team))
                {
                    return team;
                }
                CreatTeam(CharacterManager.instance.controllerCharacter);
                return teams[CharacterManager.instance.controllerCharacter.instanceId];
            }
            catch
            {
            }

            return null;
        }
    }

    public TeamerEquipAndProperty TeamerEquipAndProperty
    {
        get
        {
            TeamerEquipAndProperty teamer = new TeamerEquipAndProperty();
            if (playerTeam != null)
            {
                teamer.characterEquipAndPropertyDatas = new CharacterEquipAndPropertyData[playerTeam.Teamers.Count];

                for (int i = 0; i < playerTeam.Teamers.Count; i++)
                {
                    teamer.characterEquipAndPropertyDatas[i] = playerTeam.Teamers[i].character.CharacterEquipAndPropertyData;
                }
            }
            else
            {
                teamer.characterEquipAndPropertyDatas = new CharacterEquipAndPropertyData[1];
                teamer.characterEquipAndPropertyDatas[0] = CharacterManager.instance.controllerCharacter.CharacterEquipAndPropertyData;
            }
            return teamer;
        }
    }

    private readonly MyDic<int, Team> teams = new();

    public CharacterInformationDataList GetMyTeamCharacterInfo()
    {
        if (CharacterManager.instance.controllerCharacter == null)
        {
            return default(CharacterInformationDataList);
        }
        if (teams.TryGetValue(CharacterManager.instance.controllerCharacter.instanceId, out var team))
        {
            return team.GetTeamCharacterInfo();
        }

        return default(CharacterInformationDataList);
    }

    public Team GetTeam(int characterId)
    {
        if (teams.TryGetValue(characterId, out var team))
        {
            return team;
        }

        for (var i = 0; i < teams.length; i++)
        {
            if (teams[i].CheckCharacter(characterId))
            {
                return teams[i];
            }
        } 
        return null;
    }

    public Team GetTeam(Character character)
    {
        if (teams.TryGetValue(character.instanceId, out var team))
        {
            return team;
        }

        for (var i = 0; i < teams.length; i++)
        {
            if (teams[i].CheckCharacter(character.instanceId))
            {
                return teams[i];
            }
        } 
        
        return null;
    }

    private RefreshTeam refreshTeam = default(RefreshTeam);

    public void ChangeTeamLeader(int oldLeaderId, int newLeaderId)
    {
        if (teams.TryGetValue(oldLeaderId, out var team))
        {
            teams.Remove(oldLeaderId);
            if (newLeaderId != 0)
            {
                teams.Add(newLeaderId, team);
            }
            GameActionManager.instance.QueueAction(refreshTeam);
        }
    }

    public void RemoveTeam(int id)
    {
        teams.Remove(id);
    }

    public bool LeaveTeam(int characterId)
    {
        if (teams.TryGetValue(characterId, out var team))
        {
            team.RemoveCharacter(characterId);
            return true;
        }

        for (var i = 0; i < teams.length; i++)
        {
            team = teams[i];
            if (team.RemoveCharacter(characterId)) return true;
        }
    
        return false;
    }

    public bool IsInTeam(int characterId)
    {
        for (var i = 0; i < teams.length; i++)
        {
            if (teams[i].CheckCharacter(characterId))
            {
                return true;
            }
        }   
        return false;
    }

    public void CreatTeam(Character character)
    {
        if (!teams.ContainsKey(character.instanceId))
        {
            Team team = new Team(character);
            teams.Add(character.instanceId, team);
        }
    }

    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<DestoryCharacter>(RemoveCharacter);
        GameActionManager.instance.AddListener<JoinTeam>(JoinTeam);
        GameActionManager.instance.AddListener<TryTeamLeaderMove>(TryTeamLeaderMove); 
        GameActionManager.instance.AddListener<LeaveTeam>(LeaveTeam);
        GameActionManager.instance.AddListener<CreatTeamPlayer>(CreateTeam);
        GameActionManager.instance.AddListener<CheckIsNotInTeam>(CheckInTeam);
        GameActionManager.instance.AddListener<DestroyTeam>(DestroyTeam);
    }
    private void CheckInTeam(CheckIsNotInTeam checkInTeam)
    {
        if (IsInTeam(checkInTeam.characterId))
        {
            checkInTeam.setResult(false);
            return;
        }
        checkInTeam.setResult(true);
    }

    private void LeaveTeam(LeaveTeam leaveTeam)
    {
        var result = LeaveTeam(leaveTeam.teamCharacterId);
        RefreshAnimalPos refreshAnimalPos = new RefreshAnimalPos
        {
            animalId = leaveTeam.teamCharacterId
        };
        GameActionManager.instance.QueueAction(refreshAnimalPos);
        if (leaveTeam.setResult != null)
        {
            leaveTeam.setResult(result);
        }
        GameActionManager.instance.QueueAction(refreshTeam);
    }

   
   

    private void TryTeamLeaderMove(TryTeamLeaderMove tryTeamLeaderMove)
    {
        if (teams.TryGetValue(tryTeamLeaderMove.characterId, out var team))
        {
            team.AddTeamPos(tryTeamLeaderMove.targetPos, tryTeamLeaderMove.targetCoordinate);
        }
    }

    private void DestroyTeam(DestroyTeam destroyTeam)
    {
        int id = destroyTeam.teamCharacterId;
        if (id == 0)
        {
            id = CharacterManager.instance.controllerCharacter.instanceId;
        }
        if (teams.TryGetValue(id, out var team))
        {
            team.Clear(); 
        }
    }
    private void CreateTeam(CreatTeamPlayer createTeamPlayer)
    {
        for (int i = 0; i < createTeamPlayer.players.Count; i++)
        {
            int id = createTeamPlayer.players[i];
            if (id == CharacterManager.instance.controllerCharacter.instanceId || id == 0)
            {
                continue;
            }
            JoinTeam joinTeam = new JoinTeam
            {
                characterId = id,
                teamCharacterId = CharacterManager.instance.controllerCharacter.instanceId,
                holdDisplay=createTeamPlayer.holdDisplay
            };
            GameActionManager.instance.QueueAction(joinTeam, true);
        }
    }

    private void JoinTeam(JoinTeam joinTeam)
    {
        if(joinTeam.teamCharacterId==0|| joinTeam.teamCharacterId == int.MinValue)
        {
            joinTeam.teamCharacterId = CharacterManager.instance.controllerCharacter.instanceId;
        }
        if (teams.TryGetValue(joinTeam.teamCharacterId, out var team))
        {
            bool result = team.AddCharacter(joinTeam.characterId, joinTeam.holdDisplay);
            if (result)
            {
                RefreshOperateCharacter refreshOperateCharacter = new RefreshOperateCharacter
                {
                    characterId = joinTeam.characterId,
                    join = false
                };
                GameActionManager.instance.QueueAction(refreshOperateCharacter);
            }
            if (joinTeam.setResult != null)
                joinTeam.setResult(result);
            return;
        }

        for (var i = 0; i < teams.length; i++)
        {
            team = teams[i];
            if (team.CheckCharacter(joinTeam.teamCharacterId))
            {
                var result = team.AddCharacter(joinTeam.characterId, joinTeam.holdDisplay);
                if (result)
                {
                    var refreshOperateCharacter = new RefreshOperateCharacter
                    {
                        characterId = joinTeam.teamCharacterId,
                        join = false
                    };
                    GameActionManager.instance.QueueAction(refreshOperateCharacter);
                }

                if (joinTeam.setResult != null)
                    joinTeam.setResult(result);
                return;
            }
        } 
        Character character = CharacterManager.instance.GetCharacter(joinTeam.teamCharacterId);
        if (character != null)
        {
            Team team1 = new Team(character);
            teams.Add(character.instanceId, team1);

            bool result = team1.AddCharacter(joinTeam.characterId,joinTeam.holdDisplay);
            if (result)
            {
                RefreshOperateCharacter refreshOperateCharacter = new RefreshOperateCharacter
                {
                    characterId = joinTeam.teamCharacterId,
                    join = false
                };
                GameActionManager.instance.QueueAction(refreshOperateCharacter);
            }
            if (joinTeam.setResult != null)
                joinTeam.setResult(result);
            return;
        }
        if (joinTeam.setResult != null)
            joinTeam.setResult(false);
    }

    private void RemoveCharacter(DestoryCharacter destoryCharacter)
    {
        LeaveTeam(destoryCharacter.characterId);
    }

    protected override void Clear()
    { 
        base.Clear();
        teams.Clear(); 
    }
}

public class Team
{
    public float speed => leader.CharacterProperty.Speed *0.01f;
    public Character leader => Teamers[0].character;
    public List<Teamer> Teamers = new List<Teamer>();
    private HashSet<int> characterInstances = new HashSet<int>();
    public HashSet<int> TeamCharacters => characterInstances;

    private readonly RingQueue<Vector3> teamPositions = new(23);
    private readonly RingQueue<int2> teamCoordinates = new(23);
 
    public CharacterInformationDataList GetTeamCharacterInfo()
    {
        CharacterInformationDataList characterInformationDataList = new CharacterInformationDataList
        {
            characterInformationDatas = new List<CharacterInformationData>()
        };
        for (int i = 0; i < Teamers.Count; i++)
        {
            characterInformationDataList.characterInformationDatas.Add(Teamers[i].character.GetInformation());
        }
        return characterInformationDataList;
    }

    public Team(Character leader)
    {
        this.Teamers.Clear();
        Teamer teamer = new Teamer(leader);
        Teamers.Add(teamer);
        teamer.character.JoinTeam(this);
        StopCharacterBehavior(leader.instanceId);

        teamCoordinates.Enqueue(leader.coordinate);
        if (CharacterManager.instance.GetRuntimeCharacterObj(leader.instanceId, out var obj))
            teamPositions.Enqueue(obj.transform.position);
        else
            teamPositions.Enqueue(GameCommon.GetMapPos(leader.coordinate));
    }

    private void ReStartCharacterBehavior(int characterId)
    {
        StartCharacterBehavior startCharacterBehavior = new StartCharacterBehavior
        {
            characterId = characterId
        };
        GameActionManager.instance.QueueAction(startCharacterBehavior, true);
    }

    private void StopCharacterBehavior(int characterId)
    {
        RemoveCharacterMove removeCharacterMove = new RemoveCharacterMove
        {
            characterId = characterId
        };
        GameActionManager.instance.QueueAction(removeCharacterMove, true);

        StopCharacterBehavior stopCharacterBehavior = new StopCharacterBehavior
        {
            characterId = characterId
        };
        GameActionManager.instance.QueueAction(stopCharacterBehavior, true);
    }

    public bool CheckCharacter(int characterId)
    {
        return characterInstances.Contains(characterId);
    }

    public bool AddCharacter(int characterId,bool holdDisplay)
    {
        if (characterInstances.Add(characterId))
        {
            Character character = CharacterManager.instance.GetCharacter(characterId);
            Teamer teamer = new Teamer(character);
            teamer.SetNowCoordinate(teamCoordinates[0], holdDisplay);
            teamer.SetMoveTarget(teamPositions[0], teamCoordinates[0]);

            Teamers.Add(teamer);
            teamer.character.JoinTeam(this); 
            StopCharacterBehavior(characterId);
            GameActionManager.instance.QueueAction(default(RefreshTeam)); 
            return true;
        }
        return false;
    }


    public void Clear()
    {
        teamCoordinates.Clear();
        teamPositions.Clear();
        TeamManager.instance.RemoveTeam(leader.instanceId);
        characterInstances.Clear();
        for (var i = 0; i < Teamers.Count; i++)
        {
            Teamers[i].character.LeaveTeam();
            ReStartCharacterBehavior(Teamers[i].character.instanceId);
        }
    }

    public bool RemoveCharacter(int characterid)
    {
        bool isLeader = leader.instanceId == characterid;
        int oldCharacterId = leader.instanceId;
        if (characterInstances.Remove(characterid))
        {
            for (var i = Teamers.Count - 1; i >= 0; i--)
                if (Teamers[i].character.instanceId == characterid)
                {
                    Teamers[i].character.LeaveTeam();
                    Teamers.RemoveAt(i);
                    break;
                }

            ReStartCharacterBehavior(characterid);
        }
        if (isLeader)
        {
            TeamManager.instance.ChangeTeamLeader(oldCharacterId, Teamers.Count > 0 ? leader.instanceId : 0);
        }
        GameActionManager.instance.QueueAction(default(RefreshTeam));
        return Teamers.Count > 0;
    }

    public void AddTeamPos(Vector3 pos, int2 coordinate)
    {
        teamPositions.Enqueue(pos);
        teamCoordinates.Enqueue(coordinate);
    }

    public void ChangeMap(int2 coordinate, int map)
    {
        teamCoordinates.Clear();
        teamCoordinates.Enqueue(coordinate);
        teamPositions.Clear();
        teamPositions.Enqueue(GameCommon.GetMapPos(coordinate));

        for (var i = 0; i < Teamers.Count; i++)
        {
            var teamer = Teamers[i];
            teamer.character.SetCoordinate(new int3(coordinate, map));
            teamer.SetMoveTarget(teamPositions[0], teamCoordinates[0]);
        }
    }

    public void Update()
    {
        for (var i = 1; i < Teamers.Count; i++)
        {
            var teamer = Teamers[i];
            teamer.Move();
            if (teamer.MoveEnd)
            {
                var index = i * 3 + 1;
                index = index >= teamCoordinates.Count ? teamCoordinates.Count : index;
                index = teamCoordinates.Count - index;
                teamer.SetMoveTarget(teamPositions[index], teamCoordinates[index]);

                if (i == Teamers.Count - 1 && index > 0)
                {
                    teamCoordinates.Discard(index);
                    teamPositions.Discard(index);
                }
            }
        }
    }
}

public class Teamer
{
    public Character character;
    public bool holdDisplay;

    public bool MoveEnd => character.coordinate.x == targetCoordinate.x && character.coordinate.y == targetCoordinate.y;

    public async void SetNowCoordinate(int2 nowCoordinate, bool holdDisplay)
    {
        character.SetCoordinate(nowCoordinate, !holdDisplay);
        this.holdDisplay = holdDisplay;
        if (!holdDisplay)
        {
            await CharacterManager.instance.RefreshNpcRuntimeObj(character,RefreshMapTemp:false);
        } 
    }

    public Teamer(Character character)
    {
        this.character = character;

        targetCoordinate = character.coordinate;
        targetPos = character.pos;
    }

     
    private Vector3 startPos;
    private float timeValue;
  
    public const float perCellTime = GameCommon.cellSize * 2;
    private float timeSpeed = 1 / perCellTime;

    private Vector3 targetPos;
    private int2 targetCoordinate;

    public void SetMoveTarget(Vector3 targetPos, int2 targetCoordinate)
    {
        this.targetCoordinate = targetCoordinate;
        this.targetPos = GameCommon.SetMapPosZ(targetPos);
        timeValue = 0;
        if (CharacterManager.instance.GetRuntimeCharacterObj(character.instanceId, out var characterRuntimeObj))
        {
            startPos = characterRuntimeObj.transform.position;
        }
        else
        {
            startPos = GameCommon.GetMapPos(character.coordinate);
        }

        character.moveDirection = math.normalizesafe(targetCoordinate - character.coordinate, character.moveDirection);
    }

    private bool moving;

    public void Move()
    {
        if (holdDisplay)
        {
            character.SetCoordinate(targetCoordinate, false);
            return;
        }

        if (character.coordinate.x != targetCoordinate.x || character.coordinate.y != targetCoordinate.y)
        {
            if (!moving)
            {
                CharacterManager.instance.SetCharacterAnimationSpeed(1, character);
                moving = true;
            }

            float speed = 1;
            timeValue += Time.deltaTime * speed * timeSpeed;
            var pos = GameCommon.SetMapPosZ((targetPos - startPos) * timeValue + startPos);
            if (timeValue >= 1)
            {
                timeValue = 1;
                pos = targetPos;
                character.SetCoordinate(targetCoordinate, false);
            }
            CharacterManager.instance.SetCharacterObjPos(character, pos);
        }
        else if (moving)
        {
            CharacterManager.instance.SetCharacterAnimationSpeed(0, character);
            moving = false;
        }
    }
}
