 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;  

public delegate void MoveEndAction();

public struct CharacterRuntimeObj
{
    public RuntimeObj runtimeObj;
    public Animator animator;
    public Transform model;
}

public class CharacterManager : Singleton<CharacterManager>
{
    public const float moveSpeed = 5f;
    public const float updataMoveSpeed = 2f;

    private Dictionary<int, Character> characters = new Dictionary<int, Character>();
    private Dictionary<int,List<int>> characterInstances=new Dictionary<int, List<int>>();

    public Player player;
    private CharacterData playerData;
    public List<Character> teamPlayers = new List<Character>();
    private Vector2 playerMoveDirction;

    private HashSet<int> instanceIds = new HashSet<int>();

    //角色运行显示实体
    private Dictionary<Character, CharacterRuntimeObj> characterRuntionObjs = new Dictionary<Character, CharacterRuntimeObj>();

    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<SetCharacterProperty>(SetCharacterValue);
        GameActionManager.instance.AddListener<ChangeCharacterProperty>(ChangeCharacterValue);
        GameActionManager.instance.AddListener<SetCharacterCoordinate>(SetCharacterCoordiante);
        GameActionManager.instance.AddListener<CreatTeamPlayer>(CreatTeam);

        InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_ClickPos, MapClickAction);
        InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_Move, MoveAction, true);
    }
    public string PlayerName => player.name;
    public Sprite PlayerIcon => playerData.icon;

    public int CreatCaracterInstanceId()
    {
        var guid= Guid.NewGuid();
        int instanceId=guid.GetHashCode();
        while (instanceIds.Contains(instanceId))
        {
            guid = Guid.NewGuid();
            instanceId = guid.GetHashCode();
        }
        instanceIds.Add(instanceId);
        return instanceId;
    }

    void CreatTeam(CreatTeamPlayer creatTeamPlayer)
    {
        teamPlayers.Clear();
        for(int i = 0; i < creatTeamPlayer.players.Count; i++)
        {
            int id = creatTeamPlayer.players[i];
            if(characters.TryGetValue(id,out var character))
            {
                teamPlayers.Add(character);
            } 
        }
    }
    public async void CreatZeroNPC()
    {
        var characterDatas =await GameDataManager.instance.GetAllAsyncData<CharacterData>();
        for(int i = 0; i < characterDatas.Count; i++)
        {
            var characterData = characterDatas[i];
            if (characterData.zeroCreate)
            {
                CreatCharacter(characterData);
            }
        }
    }
    public async void CreatPlayer(int id,int bag)
    { 
        playerData = await GameDataManager.instance.GetAsyncData<CharacterData>(id);
        int instanceId = CreatCaracterInstanceId();
        player=new Player(playerData, instanceId);
        AddCharacter(player); 
    }

    Character CreatCharacter(CharacterData characterData,int bag=-1)
    {
        int instanceId = CreatCaracterInstanceId();
        Character character = new Character(characterData, instanceId);
        character.SetLevel(characterData.level,true);
        AddCharacter(character);

        return character;
    }
    async Task<Character> CreatCharacter(int characterId,int bag)
    {
        var characterData=await GameDataManager.instance.GetAsyncData<CharacterData>(characterId);
        int instanceId = CreatCaracterInstanceId();
        Character character = new Character(characterData, instanceId); 
        character.SetLevel(characterData.level);
        AddCharacter(character);

        return character;
    }

    void AddCharacter(Character character) 
    {
        if(!characterInstances.TryGetValue(character.dataId,out List<int> instances))
        {
            instances = new List<int>();
            characterInstances.Add(character.dataId, instances); 
        }
        if (!instances.Contains(character.instanceId))
        {
            instances.Add(character.instanceId);
        }
        characters[character.instanceId] = character;
    }
    void RemoveCharacter(Character character) 
    {
        if(characterInstances.TryGetValue(character.dataId,out List<int> instances))
        {
            instances.Remove(character.instanceId);
            characters.Remove(character.instanceId);
        }
    }


    private void MapClickAction(object obj)
    {
        Vector2 mouseScreenPos = (Vector2)obj;
        PlayerMove(mouseScreenPos);
    }

    private void MoveAction(object obj)
    {
        if (MapController.instance.MapRunning)
        {
            var moveValue = (Vector2)obj;
            SetPlayerMoveDirection(moveValue);
        }
    }

    public bool GetRuntimeCharacterObj(int instanceId, out RuntimeObj runtimeObj)
    {
        if (characters.TryGetValue(instanceId, out Character character))
        {
            if (characterRuntionObjs.TryGetValue(character, out CharacterRuntimeObj characterRuntimeObj))
            {
                runtimeObj = characterRuntimeObj.runtimeObj;
                return true;
            }
        }
        runtimeObj = new RuntimeObj();

        return false;
    }
    public Character GetCharacterForDataId(int dataId)
    {
        if (dataId == 0)
        {
            return player;
        }
        if(characterInstances.TryGetValue(dataId,out var instances))
        {
            if (instances.Count > 0)
            {
                return characters[instances[0]];
            }
        }
        return null;
    }
    public Character GetCharacter(int characterId)
    {
        Character character = null;
        if (characters.TryGetValue(characterId, out character)) ;
        return character;
    }

    void SetCharacterCoordiante(SetCharacterCoordinate setCharacterCoordinate)
    {
        Character character = GetCharacter(setCharacterCoordinate.characterId);
        character.StopMove();
        if (character != null)
        {
            character.SetObjCoordinate(setCharacterCoordinate.mapId, setCharacterCoordinate.coordinate);

            RefreshNpcRuntimeObj(character);
        }
    }

    void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        if (characters.TryGetValue(setCharacterProperty.characterId, out Character character))
        {
            character.SetProperty(setCharacterProperty);
        }
        else if(FightManager.instance.GetFightCharacter(setCharacterProperty.characterId,out FightCharacter fightCharacter))
        {
            fightCharacter.SetCharacterValue(setCharacterProperty);
        }
    }

    void ChangeCharacterValue(ChangeCharacterProperty changeCharacterProperty)
    {
        if (characters.TryGetValue(changeCharacterProperty.characterId, out Character character))
        {
            character.AddProperty(changeCharacterProperty);
        }
    }

    public int GetCharacterProperty(int characterId, CharacterPropertyType propertyType)
    {
        if (characters.TryGetValue(characterId, out Character character))
        {
            return character.CharacterProperty.GetValue(propertyType);
        }
        return -1;
    }

    public bool GetCharacterCoordiante(int id, out ObjCoordinate coordinate)
    {
        if (characters.TryGetValue(id, out Character character))
        {
            coordinate = character.objCoordinate;
        }
        coordinate = new ObjCoordinate();
        return false;
    }

    public void SetPlayerPos(Character character)
    {
        CharacterRuntimeObj runtimeObj;
        characterRuntionObjs.TryGetValue(character, out runtimeObj);
        if (runtimeObj.runtimeObj.obj)
        {
            Vector2 pos = GameCommon.GetMapPos(character.objCoordinate.coordinate);
            var transform = runtimeObj.runtimeObj.obj as Transform;
            transform.position = pos;
            transform.Translate(new Vector3(0, 0, -100));
        }
    }

    public void CharacterMoveTarget(Character character, Stack<int2> pathNodes, MoveEndAction EndAction = null)
    {
        var targetCoordinate = pathNodes.Pop();
        CharacterRuntimeObj runtimeObj;
        characterRuntionObjs.TryGetValue(character, out runtimeObj);

        Vector2 targetPos = GameCommon.GetMapPos(targetCoordinate);

        var transform = runtimeObj.runtimeObj.obj as Transform;

        Vector2 startPos = transform ? transform.position :
            GameCommon.GetMapPos(character.objCoordinate.coordinate);
        bool slant = targetCoordinate.x != character.objCoordinate.coordinate.x && targetCoordinate.y != character.objCoordinate.coordinate.y;

        var direction = GameCommon.GetCharacterDirect(character.objCoordinate.coordinate, targetCoordinate, character.direction);

        if (direction != Direction.Default && character.direction != direction)
        {
            character.direction = direction;
            SetCharacterAnimationDirection((float)character.direction, character);
            // Debug.Log($"{character.direction}{"-S"}{character.coordinate}{"-E"}{targetCoordinate}");
        }

        Vector2Int offsetCoordinate = Vector2Int.zero;
        character.moveEnumerator =
        GameObjectCurveController.instance.Line(slant ? moveSpeed * GameCommon.slantValue : moveSpeed, startPos, targetPos, (Vector2 pos) =>
             {
                 SetCharacterAnimationSpeed(1, runtimeObj);

                 var transform = runtimeObj.runtimeObj.obj as Transform;
                 if (transform)
                 {
                     transform.transform.position = pos;
                     transform.Translate(new Vector3(0, 0, -100));
                 }
             },
            () =>
            {
                if (pathNodes.Count > 0)
                {
                    character.SetObjCoordinate(character.objCoordinate.mapInstance, targetCoordinate);
                    CharacterMoveTarget(character, pathNodes, EndAction);
                }
                else
                {
                    SetCharacterAnimationSpeed(0, runtimeObj);
                    CrossMap(targetCoordinate, character, out int3 newMap, EndAction);
                }
            }
             );
    }

    public bool CrossMap(int2 targetCoordinate, Character character, out int3 newMap, MoveEndAction EndAction = null)
    {
        int2 offsetCoordinate = targetCoordinate - character.objCoordinate.coordinate;
        character.SetObjCoordinate(character.objCoordinate.mapInstance, targetCoordinate);
        if (MapCellController.instance.ChangeMap(targetCoordinate, offsetCoordinate, character.objCoordinate.mapInstance, out newMap))
        {
            int targetMap = newMap.x;
            targetCoordinate = new int2(newMap.y, newMap.z);

            character.SetObjCoordinate(targetMap, targetCoordinate);
            SetPlayerPos(character);

            EndAction?.Invoke();

            if (character.GetType() == typeof(Player))
            {
                //测试
                MapController.instance.nowMap = targetMap;
                WorldMapContorller.instance.RecycleMap();
                WorldMapContorller.instance.DisplayMap(targetMap);
            }
        }

        newMap = int3.zero;
        return false;
    }

    private void CreatPlayer(string characterName)
    {
        player = new Player(characterName);
        player.SetObjCoordinate(MapController.instance.nowMap, int2.zero);
        //player.mapInstance = GameManager.instance.nowMap;

        foreach (var item in GameController.instance.testPlayerItems)
        {
            PackageManager.instance.SetItemInPackage(item, player.bag);
        }

        AddCharacter(player); 

        //BehaviorTree behaviorTree = BehaviorManager.Instance.CreatBehaviorTree(GameManager.instance.testTreeData);
        /*
        RuntimeObj runtimeObj=await GameRuntimeObjManager.Instance.CreatCharacterRuntimeObj(player);
        characterRuntionObjs.Add(player, runtimeObj); */
    }

    private void CreatPlayer(CharacterSaveData characterSaveData)
    {
        player = new Player(characterSaveData);
        player.SetObjCoordinate(MapController.instance.nowMap, int2.zero);

        AddCharacter(player);

        //BehaviorTree behaviorTree = BehaviorManager.Instance.CreatBehaviorTree(GameManager.instance.testTreeData);
        /*
        RuntimeObj runtimeObj=await GameRuntimeObjManager.Instance.CreatCharacterRuntimeObj(player);
        characterRuntionObjs.Add(player, runtimeObj); */
    }

    private async void CretaDefaultNpc()
    {
        var mapNpcDatas = await GameDataManager.instance.GetAllAsyncData<MapNpcData>();
        foreach (var mapNpc in mapNpcDatas)
        {
            CreatNpc(mapNpc);
        }
    }

    private void CreatNpc(MapNpcData mapNpcData)
    {
        /*
        NPC npc = new NPC
        {
            name = mapNpcData.name,
            instanceId = mapNpcData.id
        };
        npc.SetObjCoordinate(mapNpcData.beingMap, mapNpcData.beingCoordinate);
        characters.Add(npc.instanceId, npc);*/

        // LogTask logTask = new LogTask("logTask", behaviorTree, npc.name);
        //  behaviorTree.startTask.AddChildTask(logTask);
    }

    public void StopCharacterMove(int id)
    {
        if (characters.TryGetValue(id, out Character character))
        {
            character.StopMove();
        }
    }

    public void Init(string characterName)
    {
        CreatPlayer(characterName);
        //  CretaDefaultNpc();
    }

    public void Init(CharacterSaveData characterSaveData)
    {
        CreatPlayer(characterSaveData);
        // CretaDefaultNpc();
    }

    public async Task<Sprite> GetPlayerIcon()
    {
        return await GetCharacterIcon(player.dataId);
    }

    public async Task<Sprite> GetCharacterIcon(int id)
    {
        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(id);
        return characterData.icon;
    }

    async Task<RuntimeObj> CreatCharacterRuntimeObj(int characterDataId,int instacneId,int2 coordiante)
    {
        Vector3 pos = GameCommon.GetMapPos(coordiante);
        pos.z = -100;

        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(characterDataId);
        if (characterData != null)
        { 
           return  GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.CHARACTER.ToString(), characterData.objName,
               characterData.obj.transform, instacneId);
        }
        return default(RuntimeObj);
    }

    public async System.Threading.Tasks.Task RefreshNpcRuntimeObj(Character character)
    {
        CharacterRuntimeObj characterRuntimeObj;
        if (characterRuntionObjs.TryGetValue(character, out characterRuntimeObj))
        {
            if (character.objCoordinate.mapInstance != MapController.instance.nowMap)
            {
                GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
                characterRuntionObjs.Remove(character);
            }
            else
            {
                Vector3 pos = GameCommon.GetMapPos(character.objCoordinate.coordinate);
                var transform = characterRuntimeObj.runtimeObj.obj as Transform;
                Vector3 oldPos = transform.position;
                pos.z = oldPos.z;
                transform.position = pos;
            }
        }
        else
        {
            if (character.objCoordinate.mapInstance == MapController.instance.nowMap)
            {
                RuntimeObj runtimeObj = await CreatCharacterRuntimeObj(character.dataId,character.instanceId,character.objCoordinate.coordinate);
                Transform transform = runtimeObj.obj as Transform;
                characterRuntimeObj = new CharacterRuntimeObj
                {
                    runtimeObj = runtimeObj,
                    animator = transform.GetComponentInChildren<Animator>(),
                    model = transform.Find("Model")
                };
                SetCharacterAnimationDirection((float)character.direction, characterRuntimeObj);
                characterRuntionObjs.Add(character, characterRuntimeObj);
            }
        }
    }

    public async System.Threading.Tasks.Task RefreshNpcRuntimeObj()
    {
        foreach (var obj in characterRuntionObjs.Values)
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(obj.runtimeObj);
        }
        characterRuntionObjs.Clear();
        foreach (var character in characters.Values)
        {
            if (character.objCoordinate.mapInstance == MapController.instance.nowMap)
            {
                RuntimeObj runtimeObj = await CreatCharacterRuntimeObj(character.dataId,character.instanceId,character.objCoordinate.coordinate);
                Transform transform = runtimeObj.obj as Transform;
                CharacterRuntimeObj characterRuntimeObj = new CharacterRuntimeObj
                {
                    runtimeObj = runtimeObj,
                    animator = transform.GetComponentInChildren<Animator>(),
                    model = transform.Find("Model")
                };
                SetCharacterAnimationDirection((float)character.direction, characterRuntimeObj);
                characterRuntionObjs.Add(character, characterRuntimeObj);
            }
        }
    }

    public void SetCharacterAnimationSpeed(float speed, Character character)
    {
        if (characterRuntionObjs.TryGetValue(character, out CharacterRuntimeObj characterRuntimeObj))
        {
            if (characterRuntimeObj.animator)
            {
                characterRuntimeObj.animator.SetFloat(CharacterAnimatorParameter.Speed, speed);
            }
        }
    }

    public void SetCharacterAnimationDirection(float direction, Character character)
    {
        if (characterRuntionObjs.TryGetValue(character, out CharacterRuntimeObj characterRuntimeObj))
        {
            if (characterRuntimeObj.animator)
            {
                float scaleX = 1;
                if (direction == 3)
                {
                    scaleX = -1;
                    direction = 1;
                }
                characterRuntimeObj.animator.transform.localScale = new Vector3(scaleX, 1, 1);
                characterRuntimeObj.animator.SetFloat(CharacterAnimatorParameter.Direction, direction);
            }
        }
    }

    public void SetCharacterAnimationSpeed(float speed, CharacterRuntimeObj characterRuntimeObj)
    {
        if (characterRuntimeObj.animator)
        {
            characterRuntimeObj.animator.SetFloat(CharacterAnimatorParameter.Speed, speed);
        }
    }

    public void SetCharacterAnimationDirection(float direction, CharacterRuntimeObj characterRuntimeObj)
    {
        if (characterRuntimeObj.animator)
        {
            float scaleX = 1;
            if (direction == 3)
            {
                scaleX = -1;
                direction = 1;
            }
            characterRuntimeObj.animator.transform.localScale = new Vector3(scaleX, 1, 1);
            characterRuntimeObj.animator.SetFloat(CharacterAnimatorParameter.Direction, direction);
        }
    }

    public void PlayerMove(Vector2 mouseScreenPos)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        int2 targetCoordinate = GameCommon.GetMapCoordinateInt(mousePos);
        int2 startCoordinate = player.objCoordinate.coordinate;
        Stack<int2> pathNodes = MapCellController.instance.FindPathNode(startCoordinate, targetCoordinate,
            player.objCoordinate.mapInstance);
        player.PlayerMove(pathNodes);
    }

    public void SetPlayerMoveDirection(Vector2 moveDirection)
    {
        playerMoveDirction = moveDirection;
        var playerRuntimeObj = characterRuntionObjs[player].runtimeObj;

        Transform characterTransform = playerRuntimeObj.obj as Transform;

        Vector2 _playerMoveDirction = playerMoveDirction;
        if (!WorldMapContorller.instance.InitSmoothMove(ref _playerMoveDirction, characterTransform.position,
            player.objCoordinate.mapInstance))
        {
            GameObjectCurveController.instance.StopObjectMove(playerRuntimeObj.linkId);
        }
        else
        {
            GameObjectCurveController.instance.ObjectMove(
                () => { return characterTransform.position; },
                () => { return playerMoveDirction; },
                (int2 targetCoordinate, Vector2 targetPos) =>
                {
                    characterTransform.position = new Vector3(targetPos.x, targetPos.y, characterTransform.position.z);
                    if (player.objCoordinate.coordinate.x != targetCoordinate.x ||
                    player.objCoordinate.coordinate.y != targetCoordinate.y)
                    {
                        CrossMap(targetCoordinate, player, out int3 newMap);
                    }
                },
                MapController.instance.nowMap, playerRuntimeObj.linkId, true);
        }
    }
}