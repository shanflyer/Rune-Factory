using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class TeamManager : Singleton<TeamManager>
{
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

    private Dictionary<int, Team> teams = new Dictionary<int, Team>();

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
        foreach (var t in teams)
        {
            if (t.Value.CheckCharacter(characterId))
            {
                return t.Value;
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
        foreach (var t in teams)
        {
            if (t.Value.CheckCharacter(character.instanceId))
            {
                return t.Value;
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

    public async Task<bool> LeaveTeam(int characterId)
    {
        if (teams.TryGetValue(characterId, out var team))
        {
           await team.RemoveCharacter(characterId);
            return true;
        }

        foreach (var t in teams)
        {
            if (t.Value.CheckCharacter(characterId))
            {
                team = t.Value;
                break;
            }
        }
        if (team != null)
        {
            if (await team.RemoveCharacter(characterId))
            {
            }
            return true;
        }
        return false;
    }

    public bool IsInTeam(int characterId)
    {
        foreach (var team in teams)
        {
            if (team.Value.CheckCharacter(characterId))
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
        GameActionManager.instance.AddListener<TryTeamLeaderSetCoordinate>(TryTeamLeaderSetCoordinate);
        GameActionManager.instance.AddListener<TryTeamLeaderStop>(TryTeamLeaderStop);
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
    private async void LeaveTeam(LeaveTeam leaveTeam)
    {
        bool result = await LeaveTeam(leaveTeam.teamCharacterId);
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

    private void TryTeamLeaderSetCoordinate(TryTeamLeaderSetCoordinate tryTeamLeaderSetCoordinate)
    {
        if (teams.TryGetValue(tryTeamLeaderSetCoordinate.characterId, out var team))
        {
            team.TeamLeaderSetCoordinate();
        }
    }

    private void TryTeamLeaderStop(TryTeamLeaderStop tryTeamLeaderStop)
    {
        if (teams.TryGetValue(tryTeamLeaderStop.characterId, out var team))
        {
            team.TeamLeaderStop();
        }
    }

    private void TryTeamLeaderMove(TryTeamLeaderMove tryTeamLeaderMove)
    {
        if (teams.TryGetValue(tryTeamLeaderMove.characterId, out var team))
        {
            team.TeamLeaderMove(tryTeamLeaderMove.length);
        }
    }
    private async void DestroyTeam(DestroyTeam destroyTeam)
    {
        int id = destroyTeam.teamCharacterId;
        if (id == 0)
        {
            id = CharacterManager.instance.controllerCharacter.instanceId;
        }
        if (teams.TryGetValue(id, out var team))
        {
            await team.Clear();
            teams.Remove(id);
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
        foreach (var t in teams)
        {
            if (t.Value.CheckCharacter(joinTeam.teamCharacterId))
            {
                bool result = t.Value.AddCharacter(joinTeam.characterId, joinTeam.holdDisplay);
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

    private async void RemoveCharacter(DestoryCharacter destoryCharacter)
    {
      await  LeaveTeam(destoryCharacter.characterId);
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

    private int3 lastCoordinate
    {
        get
        {
            if (Teamers.Count > 1)
            {
                return Teamers[Teamers.Count - 1].character.ObjCoordinate;
            }
            else
            {
                return leader.ObjCoordinate;
            }
        }
    }

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
            if (Teamers.Count > 0)
            {
                Teamers[Teamers.Count - 1].nextTeamer = teamer;

                Character forwardCharacter = Teamers[Teamers.Count - 1].character;
                List<float4> coordinates;
                float4 targetCoordinate = SetLastCoordianteAndDircet(forwardCharacter, out coordinates);
                teamer.SetNowCoordinate(targetCoordinate.xy, targetCoordinate.zw,holdDisplay);
                // teamer.character.SetDirection(direction);
                // teamer.character.SetCellOffset(forwardCharacter.GetCellOffset());

                // var lastTeamer = characters[characters.Count - 1];

                for (int i = 0; i < coordinates.Count; i++)
                {
                    teamer.queueCoordinate.Enqueue(coordinates[i]);
                }
                /*
                teamer.queueCoordinate.Enqueue(new float4(lastTeamer.nowCoordinate.xy,lastTeamer.character.moveDirection.xy));
                foreach (var coordinate in lastTeamer.queueCoordinate)
                {
                    teamer.queueCoordinate.Enqueue(coordinate);
                }*/
                Teamers.Add(teamer);
                teamer.character.JoinTeam(this);
                teamer.SetTeamCoordinate();
            }
            StopCharacterBehavior(characterId);
            GameActionManager.instance.QueueAction(default(RefreshTeam));
            teamer.index = Teamers.Count - 1;
            return true;
        }
        return false;
    }

    private float4 SetLastCoordianteAndDircet(Character forwardCharacter, out List<float4> coordinates)
    {
        int2 lastCoordinate = forwardCharacter.coordinate;
        coordinates = new List<float4>();
        float2 moveDirction = float2.zero;
        switch (forwardCharacter.direction)
        {
            case Direction.UP:
                lastCoordinate.y -= 4;
                for (int i = 3; i > 0; i--)
                {
                    var targetCoordinate = new float4(forwardCharacter.coordinate.x, forwardCharacter.coordinate.y - i, 0, 1);
                    coordinates.Add(targetCoordinate);
                }
                moveDirction = new float2(0, 1);
                break;

            case Direction.LEFT:
                lastCoordinate.x += 4;
                for (int i = 3; i > 0; i--)
                {
                    var targetCoordinate = new float4(forwardCharacter.coordinate.x + i, forwardCharacter.coordinate.y, -1, 0);
                    coordinates.Add(targetCoordinate);
                }
                moveDirction = new float2(-1, 0);
                break;

            case Direction.RIGHT:
                lastCoordinate.x -= 4;
                for (int i = 3; i > 0; i--)
                {
                    var targetCoordinate = new float4(forwardCharacter.coordinate.x - i, forwardCharacter.coordinate.y, 1, 0);
                    coordinates.Add(targetCoordinate);
                }
                moveDirction = new float2(1, 0);
                break;

            case Direction.DOWN:
                lastCoordinate.y += 4;

                for (int i = 3; i > 0; i--)
                {
                    var targetCoordinate = new float4(forwardCharacter.coordinate.x, forwardCharacter.coordinate.y + i, 0, -1);
                    coordinates.Add(targetCoordinate);
                }
                moveDirction = new float2(0, -1);
                break;
        }
        if (!MapCellController.instance.CheckIsWalk(lastCoordinate, forwardCharacter.mapInstance))
        {
            switch (forwardCharacter.direction)
            {
                case Direction.UP:
                case Direction.DOWN:

                    lastCoordinate = forwardCharacter.coordinate + new int2(-4, 0);
                    for (int i = 3; i > 0; i--)
                    {
                        var targetCoordinate = new float4(forwardCharacter.coordinate.x - i, forwardCharacter.coordinate.y, 1, 0);
                        coordinates.Add(targetCoordinate);
                    }
                    moveDirction = new float2(1, 0);
                    break;

                case Direction.LEFT:
                case Direction.RIGHT:
                    lastCoordinate = forwardCharacter.coordinate + new int2(0, -4);

                    for (int i = 3; i > 0; i--)
                    {
                        var targetCoordinate = new float4(forwardCharacter.coordinate.x, forwardCharacter.coordinate.y - i, forwardCharacter.coordinate.y, 1);
                        coordinates.Add(targetCoordinate);
                    }
                    moveDirction = new float2(0, 1);
                    break;
            }
            if (!MapCellController.instance.CheckIsWalk(lastCoordinate, forwardCharacter.mapInstance))
            {
                switch (forwardCharacter.direction)
                {
                    case Direction.UP:
                    case Direction.DOWN:
                        lastCoordinate = forwardCharacter.coordinate + new int2(+4, 0);
                        for (int i = 3; i > 0; i--)
                        {
                            var targetCoordinate = new float4(forwardCharacter.coordinate.x + i, forwardCharacter.coordinate.y, -1, 0);
                            coordinates.Add(targetCoordinate);
                        }
                        moveDirction = new float2(-1, 0);
                        break;

                    case Direction.LEFT:
                    case Direction.RIGHT:
                        lastCoordinate = forwardCharacter.coordinate + new int2(0, +4);
                        for (int i = 3; i > 0; i--)
                        {
                            var targetCoordinate = new float4(forwardCharacter.coordinate.x, forwardCharacter.coordinate.y + i, 0, -1);
                            coordinates.Add(targetCoordinate);
                        }
                        moveDirction = new float2(0, -1);
                        break;
                }
                if (!MapCellController.instance.CheckIsWalk(lastCoordinate, forwardCharacter.mapInstance))
                {
                    lastCoordinate = forwardCharacter.coordinate;
                }
            }
        }
        return new float4(lastCoordinate.xy, moveDirction.xy);
        // nowCharacter.SetObjCoordinate(forwardCharacter.mapInstance, lastCoordinate.xy);
        // nowCharacter.SetDirection(direction);
        //  CharacterManager.instance.RefreshNpcRuntimeObj(nowCharacter);
    }

    public async Task Clear()
    {
        var characterIds = characterInstances.ToList();
        for(int i = 0; i < characterIds.Count; i++)
        {
           await RemoveCharacter(characterIds[i]);
        }
    }
    public async Task<bool> RemoveCharacter(int characterid)
    {
        bool isLeader = leader.instanceId == characterid;
        int oldCharacterId = leader.instanceId;
        if (characterInstances.Remove(characterid))
        {
            int index = Teamers.FindIndex(c => c.character.instanceId == characterid);
            if (index >= 0)
            {
                if (index > 0)
                {
                    Teamers[index - 1].nextTeamer = null;
                    if (index < Teamers.Count - 1)
                    {
                        Teamers[index - 1].nextTeamer = Teamers[index + 1].nextTeamer;
                    }
                }

                if (index < Teamers.Count - 1)
                {
                    for (int i = Teamers.Count - 1; i > index; i--)
                    {
                        var nextCharacter = Teamers[i];
                        nextCharacter.index--;
                        var forwardCharacter = Teamers[i - 1];
                        nextCharacter.queueCoordinate = forwardCharacter.queueCoordinate;
                        nextCharacter.SetNowCoordinate(forwardCharacter.character.coordinate, forwardCharacter.character.moveDirection,false);
                       await CharacterManager.instance.RefreshNpcRuntimeObj(nextCharacter.character);
                    }
                }
                Teamers[index].character.LeaveTeam();
                Teamers.RemoveAt(index); 
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

    public void TeamLeaderStop()
    {
        if (Teamers.Count > 1)
        {
            for (int i = 1; i < Teamers.Count; i++)
            {
                CharacterManager.instance.SetCharacterAnimationSpeed(0, Teamers[i].character);
            }
        }
    }

    public void TeamLeaderMove(float length)
    {
        if (Teamers.Count > 1)
        {
            for (int i = 1; i < Teamers.Count; i++)
            {
                Teamers[i].Move(length);
            }
        }
    }

    public void TeamLeaderSetCoordinate()
    {
        if (Teamers.Count > 1)
        {
            if (lastCoordinate.z != leader.mapInstance)
            {
                for (int i = 1; i < Teamers.Count; i++)
                {
                    Teamers[i].SetNewMapCoordinate(leader.ObjCoordinate, leader.moveDirection);
                }
            }
            else
            {
                var character = Teamers[1];
                var forwardCharacter = Teamers[0];
                character.AddQueueCoordinate(forwardCharacter.character.coordinate, forwardCharacter.character.moveDirection);
            }
        }
    }
}

public class Teamer
{
    public int index = 0;
    public int2 nowCoordinate;
    private float2 direction;
    public Queue<float4> queueCoordinate = new Queue<float4>();
    public Character character;
    public Teamer nextTeamer;

    public async void SetNowCoordinate(float2 nowCoordinate, float2 directionValue,bool holdDisplay)
    {
        this.nowCoordinate = (int2)nowCoordinate;
        character.SetCoordinate(this.nowCoordinate);
        if (!holdDisplay)
        {
            await CharacterManager.instance.RefreshNpcRuntimeObj(character);
        }
       
        character.moveDirection = directionValue;
        direction = directionValue;
    }

    public Teamer(Character character)
    {
        this.character = character; 
        nowCoordinate = character.coordinate;
    }

    public void AddQueueCoordinate(float2 coordinate, float2 direction)
    {
        queueCoordinate.Enqueue(new float4(coordinate.xy, direction.xy));
        if (!canMove)
        {
            SetTeamCoordinate();
        }
    }

    private bool canMove = false;

    public void SetTeamCoordinate()
    {
        character.SetCoordinate(nowCoordinate);
        if (queueCoordinate.Count > 3)
        {
            var targetCoordinate = queueCoordinate.Dequeue();
            direction = math.normalizesafe(targetCoordinate.xy - nowCoordinate);
            // Debug.Log($"targetCoordinate：{targetCoordinate}--nowCoordinate:{nowCoordinate}--direction {direction}");
            startPos = GameCommon.GetMapPos(nowCoordinate);
            nowCoordinate = (int2)targetCoordinate.xy;
            endPos = GameCommon.GetMapPos(nowCoordinate);
            timeValue = 0;

            SetDirection setDirection = new SetDirection
            {
                characterId = character.instanceId,
                direction = targetCoordinate.zw
            };
            GameActionManager.instance.QueueAction(setDirection, true);
            canMove = true;
        }
        else
        {
            canMove = false;
        }
    }

    public async void SetNewMapCoordinate(int3 coordinate, float2 directionValue)
    {
        nowCoordinate = coordinate.xy;
        queueCoordinate.Clear();
        float4 targetCoordinate = new float4(coordinate.xy, directionValue.xy);
        for (int i = 0; i < 3; i++)
        {
            queueCoordinate.Enqueue(targetCoordinate);
        }
        character.SetCoordinate(coordinate);
       await CharacterManager.instance.RefreshNpcRuntimeObj(character);
        canMove = true;
    }

    private Vector2 startPos, endPos;
    private float timeValue;

    public const float perCellTime = GameCommon.cellSize * 2;
    private float timeSpeed = 1 / perCellTime;

    public void Move(float length)
    {
        if (canMove)
        {
            // int2 coordinate = character.coordinate;
            float speed = queueCoordinate.Count / (3.0f);
            timeValue += Time.deltaTime * speed * timeSpeed;
            Vector2 pos = (endPos - startPos) * timeValue + startPos;
            if (timeValue >= 1)
            {
                timeValue = 1;
                pos = endPos;

                if (nextTeamer != null)
                {
                    nextTeamer.AddQueueCoordinate(character.coordinate, character.moveDirection);
                }
                SetTeamCoordinate();
            }
            //if (character.name == "玛德琳")
            {
                // Debug.Log($"pos:{pos}--endPos:{endPos}--startPos{startPos}--timeValue{timeValue}");
            }

            CharacterManager.instance.SetCharacterObjPos(character, pos);

            /*
             CharacterManager.instance.MoveCharacterObj(character, length * direction* speed, ref coordinate);
            if (!coordinate.Equals(character.coordinate))
            {
                character.SetCoordinate(new int3(coordinate.xy, character.mapInstance));
                SetTeamCoordinate();
            }*/
        }
    }
}