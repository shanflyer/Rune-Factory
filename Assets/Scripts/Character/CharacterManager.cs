 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public delegate void MoveEndAction();

public struct CharacterRuntimeObj
{
    public RuntimeObj runtimeObj;
    public Animator animator;
    public Transform model;

    public void SetAnimationDirection(float2 direction) 
    {
        if (animator != null)
        {
            if (direction.x == 0 && direction.y == 0)
            {
                return;
            }
            animator.SetFloat(CharacterAnimatorParameter.Dir_X, direction.x);
            animator.SetFloat(CharacterAnimatorParameter.Dir_Y, direction.y);
        }
    }
    public void SetAnimationSpeed(float speed) 
    {
        if (animator != null)
        {
            animator.SetFloat(CharacterAnimatorParameter.Speed, speed); 
        }
    }
}

public class CharacterManager : Singleton<CharacterManager>
{
    public const float moveSpeed = 1f;
    public const float updataMoveSpeed = 0.5f;

    private MyInstance myInstance;

    private Dictionary<int, Character> characters = new Dictionary<int, Character>();
    private Dictionary<int,List<int>> characterInstances=new Dictionary<int, List<int>>();

    public Player player;
    private CharacterData playerData;
    public List<Character> teamPlayers = new List<Character>();
    //private Vector2 playerMoveDirction;

    private HashSet<int> instanceIds = new HashSet<int>();

    //角色运行显示实体
    private Dictionary<Character, CharacterRuntimeObj> characterRuntionObjs = new Dictionary<Character, CharacterRuntimeObj>();

    public override void Init()
    {
        base.Init();

        myInstance = new MyInstance();

        GameActionManager.instance.AddListener<SetCharacterProperty>(SetCharacterValue);
        GameActionManager.instance.AddListener<ChangeCharacterProperty>(ChangeCharacterValue);
        GameActionManager.instance.AddListener<SetCharacterCoordinate>(SetCharacterCoordiante);
        GameActionManager.instance.AddListener<CreatTeamPlayer>(CreatTeam);
        GameActionManager.instance.AddListener<CreatCharacter>(CreatCharacter);
        GameActionManager.instance.AddListener<CreatDefaultNPC>(CreatDefaultNPC);

        GameActionManager.instance.AddListener<InitInputAction>(InitInputAction);
        
        
    }

    void InitInputAction(InitInputAction initInputAction)
    {
        //InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_ClickPos, MapClickAction);
        InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_Move, MoveAction, true);
    }
    public string PlayerName => player.name;
    public Sprite PlayerIcon => playerData.icon;
    private Character controllerCharacter;
   
    public void SetControllerCharacter(int id)
    {

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
        int instanceId = myInstance.CreatInstanceId();
        player=new Player(playerData, instanceId);
        AddCharacter(player); 
    }


    async void CreatCharacter(CreatCharacter creatCharacter)
    {
        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(creatCharacter.characterId);
        Character character = new Character(characterData, myInstance.CreatInstanceId());
        AddCharacter(character);
        character.objCoordinate = new ObjCoordinate
        {
            mapInstance = creatCharacter.mapInstance,
            x = creatCharacter.coordinateX,
            y = creatCharacter.coordinateY
        };
        RefreshNpcRuntimeObj(character,creatCharacter.controller);
        if (creatCharacter.controller)
        {
            controllerCharacter = character;
        }
    }

    Character CreatCharacter(CharacterData characterData,int bag=-1)
    {
        int instanceId = myInstance.CreatInstanceId();
        Character character = new Character(characterData, instanceId);
        character.SetLevel(characterData.level,true);
        AddCharacter(character);

        return character;
    }
    async Task<Character> CreatCharacter(int characterId,int bag)
    {
        var characterData=await GameDataManager.instance.GetAsyncData<CharacterData>(characterId);
        int instanceId = myInstance.CreatInstanceId();
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
        ControllerCharacterMove(mouseScreenPos);
    }

    private void MoveAction(object obj)
    {
        //Debug.Log(obj);
        var moveValue = (Vector2)obj; 
        
        SetControllerCharacterMoveDirection(moveValue);
    }

    public bool GetRuntimeCharacterObj(int instanceId, out CharacterRuntimeObj characterRuntimeObj)
    {
        if (characters.TryGetValue(instanceId, out Character character))
        {
            if (characterRuntionObjs.TryGetValue(character, out  characterRuntimeObj))
            { 
                return true;
            }
        }
        characterRuntimeObj = default(CharacterRuntimeObj);

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
            //transform.Translate(new Vector3(0, 0, -100));
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

        character.moveDirection =math.normalize(targetCoordinate - character.objCoordinate.coordinate);
       // var direction = GameCommon.GetCharacterDirect(character.objCoordinate.coordinate, targetCoordinate, character.direction);
         

        Vector2Int offsetCoordinate = Vector2Int.zero;
        character.moveEnumerator =
        GameObjectCurveController.instance.Line(slant ? moveSpeed * GameCommon.slantValue : moveSpeed, startPos, targetPos, (Vector2 pos) =>
             {
                 SetCharacterAnimationSpeed(1, runtimeObj);

                 var transform = runtimeObj.runtimeObj.obj as Transform;
                 if (transform)
                 {
                     transform.transform.position = pos;
                     //transform.Translate(Vector3.zero);
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

            if (character == controllerCharacter)
            {
                WorldMapManager.instance.RecycleMap();
                WorldMapManager.instance.DisplayMap(targetMap);
            }
        }

        newMap = int3.zero;
        return false;
    }

    private void CreatPlayer(string characterName)
    {
        player = new Player(characterName);
        player.SetObjCoordinate(WorldMapManager.instance.displayMap, int2.zero);
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
        player.SetObjCoordinate(WorldMapManager.instance.displayMap, int2.zero);

        AddCharacter(player);

        //BehaviorTree behaviorTree = BehaviorManager.Instance.CreatBehaviorTree(GameManager.instance.testTreeData);
        /*
        RuntimeObj runtimeObj=await GameRuntimeObjManager.Instance.CreatCharacterRuntimeObj(player);
        characterRuntionObjs.Add(player, runtimeObj); */
    }

    async void CreatDefaultNPC(CreatDefaultNPC creatDefaultNPC)
    {
        var mapNpcDatas = await GameDataManager.instance.GetAllAsyncData<MapNpcData>();
        foreach (var mapNpc in mapNpcDatas)
        {
            if (mapNpc.initialBegin)
            {
                CreatNpc(mapNpc);
            }
        }
    }

    private async void CreatNpc(MapNpcData mapNpcData)
    { 
        if(!characters.TryGetValue(mapNpcData.id, out var npc))
        {
            var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(mapNpcData.dataId);
            npc = new NPC(characterData, mapNpcData.id);
            characters.Add(mapNpcData.id, npc);
        }
        npc.SetObjCoordinate(mapNpcData.beginMap, mapNpcData.beginCoordinate);
        RefreshNpcRuntimeObj(npc);
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
        //pos.z = -100;

        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(characterDataId);
        if (characterData != null)
        { 
           return  GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.CHARACTER.ToString(), characterData.objName,
               characterData.obj.transform, instacneId);
        }
        return default(RuntimeObj);
    }

    public async void RefreshNpcRuntimeObj(Character character,bool controller=false)
    {
        CharacterRuntimeObj characterRuntimeObj;
        if (characterRuntionObjs.TryGetValue(character, out characterRuntimeObj))
        {
            if (character.objCoordinate.mapInstance != WorldMapManager.instance.displayMap)
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
                if (controller)
                {
                    CameraManager.instance.SetFollowTarget(transform);
                }
            }
        }
        else
        {
            if (character.objCoordinate.mapInstance == WorldMapManager.instance.displayMap)
            {
                RuntimeObj runtimeObj = await CreatCharacterRuntimeObj(character.dataId,character.instanceId,character.objCoordinate.coordinate);
                Transform transform = runtimeObj.obj as Transform;
                characterRuntimeObj = new CharacterRuntimeObj
                {
                    runtimeObj = runtimeObj,
                    animator = transform.GetComponentInChildren<Animator>(),
                    model = transform.Find("Model")
                }; 
                characterRuntionObjs.Add(character, characterRuntimeObj);
                if (controller)
                {
                    CameraManager.instance.SetFollowTarget(transform);
                }
            }
        }
    }

    public async System.Threading.Tasks.Task RefreshNpcRuntimeObj()
    {

        using(var e = characters.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var character = e.Current.Value;
                if (character.objCoordinate.mapInstance != WorldMapManager.instance.displayMap
                    && characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
                {
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
                    characterRuntionObjs.Remove(character);
                }
                if (character.objCoordinate.mapInstance == WorldMapManager.instance.displayMap)
                {
                    if (!characterRuntionObjs.TryGetValue(character, out characterRuntimeObj))
                    {
                        var runtimeObj = await CreatCharacterRuntimeObj(character.dataId, character.instanceId, character.objCoordinate.coordinate);
                        Transform transform = runtimeObj.obj as Transform;
                        characterRuntimeObj = new CharacterRuntimeObj
                        {
                            runtimeObj = runtimeObj,
                            animator = transform.GetComponentInChildren<Animator>(),
                            model = transform.Find("Model")
                        }; 
                        characterRuntionObjs.Add(character, characterRuntimeObj);
                    }
                    else
                    {
                        Vector3 pos = GameCommon.GetMapPos(character.objCoordinate.coordinate);
                        Transform transform = characterRuntimeObj.runtimeObj.obj as Transform;
                        transform.localPosition = pos; 
                    }
                }
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


    public void SetCharacterAnimationSpeed(float speed, CharacterRuntimeObj characterRuntimeObj)
    {
        if (characterRuntimeObj.animator)
        {
            characterRuntimeObj.animator.SetFloat(CharacterAnimatorParameter.Speed, speed);
        }
    }

     

    public void ControllerCharacterMove(Vector2 mouseScreenPos)
    {
        if (controllerCharacter == null)
        {
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        int2 targetCoordinate = GameCommon.GetMapCoordinateInt(mousePos);
        int2 startCoordinate = controllerCharacter.objCoordinate.coordinate;
        Stack<int2> pathNodes = MapCellController.instance.FindPathNode(startCoordinate, targetCoordinate,
            controllerCharacter.objCoordinate.mapInstance);
        controllerCharacter.PlayerMove(pathNodes);
    }

    //设置可操控的角色移动方向
    public void SetControllerCharacterMoveDirection(Vector2 moveDirection)
    {
        if (controllerCharacter == null)
        {
            return;
        }
        float speed = moveDirection.magnitude;
        controllerCharacter.nowSpeed = speed;


        controllerCharacter.moveDirection= moveDirection;
        var playerRuntimeObj = characterRuntionObjs[controllerCharacter].runtimeObj;

        Transform characterTransform = playerRuntimeObj.obj as Transform;

        Vector2 _playerMoveDirction = moveDirection;
        if (!WorldMapManager.instance.InitSmoothMove(ref _playerMoveDirction, characterTransform.position,
            controllerCharacter.objCoordinate.mapInstance))
        {
            GameObjectCurveController.instance.StopObjectMove(playerRuntimeObj.linkId);
        }
        else
        {
            GameObjectCurveController.instance.ObjectMove(
                () => { return characterTransform.position; },
                () => { return controllerCharacter.moveDirection; },
                (int2 targetCoordinate, Vector2 targetPos) =>
                {
                    characterTransform.position = new Vector3(targetPos.x, targetPos.y, characterTransform.position.z);
                    if (controllerCharacter.objCoordinate.coordinate.x != targetCoordinate.x ||
                    controllerCharacter.objCoordinate.coordinate.y != targetCoordinate.y)
                    {
                        CrossMap(targetCoordinate, controllerCharacter, out int3 newMap);
                    }
                },
                WorldMapManager.instance.displayMap, playerRuntimeObj.linkId, true);
        }
    }
}