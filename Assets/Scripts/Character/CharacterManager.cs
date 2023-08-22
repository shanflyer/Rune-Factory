using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore.Text;

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

    public Player player;
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

        InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_ClickPos, MapClickAction);
        InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_Move, MoveAction, true);
    }

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
    public async void CreatZeroNPC()
    {
        teamPlayers.Clear();
        for(int i = 0; i < GameCommon.zeroNPC.Count; i++)
        {
            int id = GameCommon.zeroNPC[i];
           var character=await CreatCharacter(id, -1);
            teamPlayers.Add(character);
        }
    }
    public async void CreatPlayer(int id,int bag)
    { 
        var characterData = await GameDataManager.instance.GetAsyncObjectData<CharacterData>(id);
        int instanceId = CreatCaracterInstanceId();
        player=new Player(characterData, instanceId);
        characters.Add(instanceId, player);
    }

    async Task<Character> CreatCharacter(int characterId,int bag)
    {
        var characterData=await GameDataManager.instance.GetAsyncObjectData<CharacterData>(characterId);
        int instanceId = CreatCaracterInstanceId();
        Character character = new Character(characterData, instanceId);
       
        character.SetLevel(characterData.level);

        characters.Add(character.instanceId,character);

        return character;
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

    public Character GetCharacter(int characterId)
    {
        Character character = null;
        if (characters.TryGetValue(characterId, out character)) ;
        return character;
    }

    public void SetCharacterCoordiante(SetCharacterCoordinate setCharacterCoordinate)
    {
        Character character = GetCharacter(setCharacterCoordinate.characterId);
        character.StopMove();
        if (character != null)
        {
            character.SetObjCoordinate(setCharacterCoordinate.mapId, setCharacterCoordinate.coordinate);

            RefreshNpcRuntimeObj(character);
        }
    }

    public void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        if (characters.TryGetValue(setCharacterProperty.characterId, out Character character))
        {
            character.SetProperty(setCharacterProperty);
        }
    }

    public void ChangeCharacterValue(ChangeCharacterProperty changeCharacterProperty)
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
            runtimeObj.runtimeObj.obj.transform.position = pos;
            runtimeObj.runtimeObj.obj.transform.Translate(new Vector3(0, 0, -100));
        }
    }

    public void CharacterMoveTarget(Character character, Stack<int2> pathNodes, MoveEndAction EndAction = null)
    {
        var targetCoordinate = pathNodes.Pop();
        CharacterRuntimeObj runtimeObj;
        characterRuntionObjs.TryGetValue(character, out runtimeObj);

        Vector2 targetPos = GameCommon.GetMapPos(targetCoordinate);
        Vector2 startPos = runtimeObj.runtimeObj.obj ? runtimeObj.runtimeObj.obj.transform.position :
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

                 if (runtimeObj.runtimeObj.obj)
                 {
                     runtimeObj.runtimeObj.obj.transform.position = pos;
                     runtimeObj.runtimeObj.obj.transform.Translate(new Vector3(0, 0, -100));
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

        characters.Add(player.instanceId, player);

        //BehaviorTree behaviorTree = BehaviorManager.Instance.CreatBehaviorTree(GameManager.instance.testTreeData);
        /*
        RuntimeObj runtimeObj=await GameRuntimeObjManager.Instance.CreatCharacterRuntimeObj(player);
        characterRuntionObjs.Add(player, runtimeObj); */
    }

    private void CreatPlayer(CharacterSaveData characterSaveData)
    {
        player = new Player(characterSaveData);
        player.SetObjCoordinate(MapController.instance.nowMap, int2.zero);

        characters.Add(player.instanceId, player);

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
        var characterData = await GameDataManager.instance.GetAsyncObjectData<CharacterData>(id);
        return characterData.icon;
    }

    async Task<RuntimeObj> CreatCharacterRuntimeObj(int characterDataId,int instacneId,int2 coordiante)
    {
        Vector3 pos = GameCommon.GetMapPos(coordiante);
        pos.z = -100;

        var characterData = await GameDataManager.instance.GetAsyncObjectData<CharacterData>(characterDataId);
        if (characterData != null)
        { 
           return  GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.CHARACTER.ToString(), characterData.objName,
               characterData.obj, instacneId);
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
                Vector3 oldPos = characterRuntimeObj.runtimeObj.obj.transform.position;
                pos.z = oldPos.z;
                characterRuntimeObj.runtimeObj.obj.transform.position = pos;
            }
        }
        else
        {
            if (character.objCoordinate.mapInstance == MapController.instance.nowMap)
            {
                RuntimeObj runtimeObj = await CreatCharacterRuntimeObj(character.dataId,character.instanceId,character.objCoordinate.coordinate);
                characterRuntimeObj = new CharacterRuntimeObj
                {
                    runtimeObj = runtimeObj,
                    animator = runtimeObj.obj.GetComponentInChildren<Animator>(),
                    model = runtimeObj.obj.transform.Find("Model")
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
                CharacterRuntimeObj characterRuntimeObj = new CharacterRuntimeObj
                {
                    runtimeObj = runtimeObj,
                    animator = runtimeObj.obj.GetComponentInChildren<Animator>(),
                    model = runtimeObj.obj.transform.Find("Model")
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

        Transform characterTransform = playerRuntimeObj.obj.transform;

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