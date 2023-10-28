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

    public void SetAnimationDirection(float2 direction)
    {
        if (direction.x == float.NaN || direction.y == float.NaN)
        {
            return;
        }
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

public struct TeamerEquipAndProperty : IReferenceData
{
    public CharacterEquipAndPropertyData[] characterEquipAndPropertyDatas;
}
public class CharacterManager : Singleton<CharacterManager>
{
    public const float moveSpeed = 4f;
    public const float updataMoveSpeed = 1f;
    private MyInstance myInstance;
    private Dictionary<int, Character> characters = new Dictionary<int, Character>();
    private Dictionary<int, List<int>> characterInstances = new Dictionary<int, List<int>>();

    private List<int> npcInstances = new List<int>();

    public Player player;
    private CharacterData playerData;
    public List<Character> teamPlayers = new List<Character>();
    //private Vector2 playerMoveDirction;

    public int GetCharacterInstance()
    {
        return myInstance.CreatInstanceId();
    }

    public void RemoveInstance(int id)
    {
        myInstance.RemoveInstance(id);
    }


    //角色运行显示实体
    private Dictionary<Character, CharacterRuntimeObj> characterRuntionObjs = new Dictionary<Character, CharacterRuntimeObj>();

    public TeamerEquipAndProperty TeamerEquipAndProperty
    {
        get
        {
            TeamerEquipAndProperty teamer = new TeamerEquipAndProperty
            {
                characterEquipAndPropertyDatas = new CharacterEquipAndPropertyData[teamPlayers.Count]
            };
            for (int i = 0; i < teamPlayers.Count; i++)
            {
                teamer.characterEquipAndPropertyDatas[i] = teamPlayers[i].CharacterEquipAndPropertyData;
            }
            return teamer;
        }
    }

    public override void Init()
    {
        base.Init();
        myInstance = new MyInstance();

        GameActionManager.instance.AddListener<SetCharacterProperty>(SetCharacterValue);
        GameActionManager.instance.AddListener<ChangeCharacterProperty>(ChangeCharacterValue);
        GameActionManager.instance.AddListener<SetCharacterCoordinate>(SetCharacterCoordiante);
        GameActionManager.instance.AddListener<CreatTeamPlayer>(CreatTeam);
        GameActionManager.instance.AddListener<CreatCharacter>(CreatCharacter);
        GameActionManager.instance.AddListener<CreatTempCharacter>(CreatTempCharacter);
        GameActionManager.instance.AddListener<DestoryTempCharacter>(DestoryTempCharacter);

        GameActionManager.instance.AddListener<CreatDefaultNPC>(CreatDefaultNPC);
        GameActionManager.instance.AddListener<SetCharacterAnimator>(SetCharacterAnimator);
        GameActionManager.instance.AddListener<InitInputAction>(InitInputAction);

        GameActionManager.instance.AddListener<SetDirection>(SetDirection);
        GameActionManager.instance.AddListener<SetTargetDirection>(SetTargetDirection);
        GameActionManager.instance.AddListener<GetCharacterDataId>(GetCharacterDataId);

        GameActionManager.instance.AddListener<CheckCharacterTemp>(CheckCharacterTemp);
        GameActionManager.instance.AddListener<StartCharacterMove>(StartCharacterMove);
        GameActionManager.instance.AddListener<StopCharacterMove>(StopCharacterMove);
        GameActionManager.instance.AddListener<ChangeEquip>(ChangeEquip);
        GameActionManager.instance.AddListener<ClearEquip>(ClearEquip);
    }
    void ClearEquip(ClearEquip clearEquip)
    {
        if (characters.TryGetValue(clearEquip.characterId, out var character))
        {
            character.ClearEquip(clearEquip.itemType);
            int itemId = 0;
            switch (clearEquip.itemType)
            {
                case ItemType.武器:
                    itemId = character.Equip.weapon.x;
                    break;
                case ItemType.防具:
                    itemId = character.Equip.clothes.x;
                    break;
            }
            if (itemId != 0&&clearEquip.outPackageId!=0)
            {
                PackageManager.instance.SetItemInPackage(new Item(itemId,1), clearEquip.outPackageId);
            } 
        }
    }
    async void ChangeEquip(ChangeEquip changeEquip)
    {
        if (characters.TryGetValue(changeEquip.characterId, out var character))
        {
            PackageManager.instance.GetOutItenFromPackage(changeEquip.outPackageId, changeEquip.itemId, 1);
            ItemData itemData;
            if (changeEquip.outPackageId!= 0){
                itemData = await GameDataManager.instance.GetAsyncData<ItemData>(changeEquip.itemId);
            }
            else
            {
                Item item = PackageManager.instance.GetItemFromInstanceId(changeEquip.outPackageId, changeEquip.itemId);
                itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
            }
            if (itemData != null)
            {
                character.ChangeEquip(itemData, changeEquip.outPackageId);
            }
        }
    }
    public void StartCharacterMove(StartCharacterMove startCharacterMove)
    {
        if (characters.TryGetValue(startCharacterMove.characterId, out var character))
        {
            character.StartMove();
        }
    }

    public void StopCharacterMove(StopCharacterMove stopCharacterMove)
    {
        if (characters.TryGetValue(stopCharacterMove.characterId, out var character))
        {
            character.StopMove();
        }
    }

    public bool IsTempCharacter(int characterId)
    {
        if (characters.TryGetValue(characterId, out var character))
        {
            if (character is TempCharacter)
            {
                return true;
            }
        }
        return false;
    }

    private void CheckCharacterTemp(CheckCharacterTemp checkCharacterTemp)
    {
    }

    private void GetCharacterDataId(GetCharacterDataId getCharacterDataId)
    {
        if (characters.TryGetValue(getCharacterDataId.characterId, out var character))
        {
            getCharacterDataId.SetValue(character.dataId);
        }
    }

    private void InitInputAction(InitInputAction initInputAction)
    {
        //InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_ClickPos, MapClickAction);
        InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_Move, MoveAction, true);
    }

    public string PlayerName => player.name;
    public Sprite PlayerHead => playerData.head;
    private Character _controllerCharacter;

    public Character controllerCharacter
    {
        set
        {
            var playerOperateManager = PlayerOperateManager.instance;
            if (value != _controllerCharacter)
            {
                if (_controllerCharacter != null)
                {
                    _controllerCharacter.SetController(false);
                }
                _controllerCharacter = value;
                if (_controllerCharacter != null)
                {
                    _controllerCharacter.SetController(true);
                }
            }

            UIManager.instance.ShowGamePanel<PlayerTopPanel>();
        }
        get
        {
            return _controllerCharacter;
        }
    }

    public float GetDistanceController(int2 coordinate)
    {
        return math.distance(controllerCharacter.coordinate, coordinate);
    }

    public void SetControllerCharacter(int id)
    {
    }

    private void SetCharacterAnimator(SetCharacterAnimator setCharacterAnimator)
    {
        if (GetRuntimeCharacterObj(setCharacterAnimator.characterId, out CharacterRuntimeObj characterRuntimeObj))
        {
            Animator animator = characterRuntimeObj.animator;
            switch (setCharacterAnimator.parameterType)
            {
                case ParameterType.BOOL:
                    animator.SetBool(setCharacterAnimator.parameter, setCharacterAnimator.boolValue);
                    break;

                case ParameterType.INT:
                    animator.SetInteger(setCharacterAnimator.parameter, setCharacterAnimator.intValue);
                    break;

                case ParameterType.FLOAT:
                    animator.SetFloat(setCharacterAnimator.parameter, setCharacterAnimator.floatValue);
                    break;
            }
        }
    }

    private void CreatTeam(CreatTeamPlayer creatTeamPlayer)
    {
        teamPlayers.Clear();
        for (int i = 0; i < creatTeamPlayer.players.Count; i++)
        {
            int id = creatTeamPlayer.players[i];
            if (characters.TryGetValue(id, out var character))
            {
                teamPlayers.Add(character);
            }
        }
    }  
    public async void CreatPlayer(int id, int bag)
    {
        playerData = await GameDataManager.instance.GetAsyncData<CharacterData>(id);
        int instanceId = myInstance.CreatInstanceId();
        player = new Player(playerData, instanceId);
        AddCharacter(player);
    }

    /// <summary>
    /// 销毁消费者
    /// </summary>
    /// <param name="DestoryTempCharacter"></param>
    private void DestoryTempCharacter(DestoryTempCharacter destoryTempCharacter)
    {
        if (characters.TryGetValue(destoryTempCharacter.characterId, out var character))
        {
            RemoveCharacter(character);
        }
    }

    private void RemoveCharacter(Character character)
    {
        if (characterInstances.TryGetValue(character.dataId, out List<int> instances))
        {
            instances.Remove(character.instanceId);
            characters.Remove(character.instanceId);
        }

        if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
            characterRuntionObjs.Remove(character);
        }
        MapCellController.instance.RemoveCharacterCoordinate(character.ObjCoordinate, character.instanceId);
        CharacterBehaviorManager.instance.DestroyBehavior(character.instanceId);
    }

    /// <summary>
    /// 创建消费者
    /// </summary>
    /// <param name="creatTempCharacter"></param>
    private async void CreatTempCharacter(CreatTempCharacter creatTempCharacter)
    {
        var tempCharacterData = await GameDataManager.instance.GetAsyncData<TempCharacterData>(creatTempCharacter.characterId);
        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(tempCharacterData.linkCharacterId);
        int level = TempCharacterManager.instance.level;

        TempCharacter character = new TempCharacter(characterData, myInstance.CreatInstanceId(), tempCharacterData.id);

        AddCharacter(character);
        character.SetObjCoordinate(creatTempCharacter.mapInstance,
            new int2(creatTempCharacter.coordinateX, creatTempCharacter.coordinateY));

        character.templevel = level;
        RefreshNpcRuntimeObj(character);
    }

    private async void CreatCharacter(CreatCharacter creatCharacter)
    {
        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(creatCharacter.characterId);
        Character character = new Character(characterData, myInstance.CreatInstanceId());
        AddCharacter(character);
        character.SetObjCoordinate(creatCharacter.mapInstance,
            new int2(creatCharacter.coordinateX, creatCharacter.coordinateY));
        RefreshNpcRuntimeObj(character, creatCharacter.controller);
        if (creatCharacter.controller)
        {
            controllerCharacter = character;
        }
    }
   

    private void AddCharacter(Character character)
    {
        if (!characterInstances.TryGetValue(character.dataId, out List<int> instances))
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
            if (characterRuntionObjs.TryGetValue(character, out characterRuntimeObj))
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
        if (characterInstances.TryGetValue(dataId, out var instances))
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

    private void SetCharacterCoordiante(SetCharacterCoordinate setCharacterCoordinate)
    {
        Character character = GetCharacter(setCharacterCoordinate.characterId);
        character.StopMove();
        if (character != null)
        {
            character.SetCoordinate(setCharacterCoordinate.coordinate);

            RefreshNpcRuntimeObj(character);
        }
    }

    private void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        if (characters.TryGetValue(setCharacterProperty.characterId, out Character character))
        {
            character.SetProperty(setCharacterProperty);
        }
        else if (FightManager.instance.GetFightCharacter(setCharacterProperty.characterId, out FightCharacter fightCharacter))
        {
            fightCharacter.SetCharacterValue(setCharacterProperty);
        }
    }

    private void ChangeCharacterValue(ChangeCharacterProperty changeCharacterProperty)
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

    public bool GetCharacterCoordiante(int id, out int3 coordinate)
    {
        if (characters.TryGetValue(id, out Character character))
        {
            coordinate = character.ObjCoordinate;
        }
        coordinate = int3.zero;
        return false;
    }

    private async void SetPlayerPos(Character character)
    {
        if (characterRuntionObjs.TryGetValue(character, out CharacterRuntimeObj characterRuntimeObj))
        {
            if (character.mapInstance != WorldMapManager.instance.displayMap)
            {
                GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
                characterRuntionObjs.Remove(character);
            }
            else
            {
                if (characterRuntimeObj.runtimeObj.obj)
                {
                    Vector2 pos = GameCommon.GetMapPos(character.coordinate);
                    var transform = characterRuntimeObj.runtimeObj.obj as Transform;
                    transform.position = pos;
                    //transform.Translate(new Vector3(0, 0, -100));
                }
            }
        }
        else if (character.mapInstance == WorldMapManager.instance.displayMap)
        {
            var runtimeObj = await CreatCharacterRuntimeObj(character.dataId, character.instanceId, character.coordinate);
            Vector2 pos = GameCommon.GetMapPos(character.coordinate);
            var transform = characterRuntimeObj.runtimeObj.obj as Transform;
            transform.position = pos;
            characterRuntimeObj = new CharacterRuntimeObj
            {
                runtimeObj = runtimeObj,
                animator = transform.GetComponentInChildren<Animator>(),
                model = transform.Find("Model")
            };
            characterRuntionObjs.Add(character, characterRuntimeObj);
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
            GameCommon.GetMapPos(character.coordinate);
        bool slant = targetCoordinate.x != character.coordinate.x && targetCoordinate.y != character.coordinate.y;

        character.moveDirection = math.normalize(targetCoordinate - character.coordinate);
        // var direction = GameCommon.GetCharacterDirect(character.objCoordinate.coordinate, targetCoordinate, character.direction);

        Vector2Int offsetCoordinate = Vector2Int.zero;
        character.moveEnumeratorId =
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
                // Debug.Log($"pathNodes.count:{pathNodes.Count}");
                if (pathNodes.Count > 0)
                {
                    character.SetCoordinate(new int3(targetCoordinate.xy, character.mapInstance));
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
        int2 offsetCoordinate = targetCoordinate - character.coordinate;
        character.SetCoordinate(new int3(targetCoordinate.xy, character.mapInstance));
        if (MapCellController.instance.ChangeMap(targetCoordinate, offsetCoordinate, character.mapInstance, out newMap))
        {
            int targetMap = newMap.x;
            targetCoordinate = new int2(newMap.y, newMap.z);

            character.SetCoordinate(new int3(targetCoordinate, targetMap));

            if (character == controllerCharacter)
            {
                WorldMapManager.instance.RecycleMap();
                WorldMapManager.instance.DisplayMap(targetMap);
            }
            SetPlayerPos(character);
        }
        EndAction?.Invoke();
        newMap = int3.zero;
        return false;
    }

    public CharacterData GetCharacterDataFromInstance(int Id)
    {
        if (characters.TryGetValue(Id, out var character))
        {
            return character.characterData;
        }

        return null;
    }

    private void CreatPlayer(string characterName)
    {
        player = new Player(characterName);
        player.SetCoordinate(new int3(int2.zero, WorldMapManager.instance.displayMap));
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
        player.SetCoordinate(new int3(int2.zero, WorldMapManager.instance.displayMap));

        AddCharacter(player);

        //BehaviorTree behaviorTree = BehaviorManager.Instance.CreatBehaviorTree(GameManager.instance.testTreeData);
        /*
        RuntimeObj runtimeObj=await GameRuntimeObjManager.Instance.CreatCharacterRuntimeObj(player);
        characterRuntionObjs.Add(player, runtimeObj); */
    }

    private async void CreatDefaultNPC(CreatDefaultNPC creatDefaultNPC)
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
        if(NPCManager.instance.GetNPC(mapNpcData.dataId,out var npc))
        {
            if (!characters.TryGetValue(npc.characterId, out var character))
            {
                var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(mapNpcData.dataId);
                character = new Character(characterData, npc.characterId);
                characters.Add(npc.characterId, character);
            }
            character.SetCoordinate(new int3(mapNpcData.beginCoordinate, mapNpcData.beginMap));
            RefreshNpcRuntimeObj(character);

            if (mapNpcData.externalBehavior)
            {
                CharacterBehaviorManager.instance.AddBehavior(npc.characterId, mapNpcData.externalBehavior);
            }
        }

       
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

    private async Task<RuntimeObj> CreatCharacterRuntimeObj(int characterDataId, int instacneId, int2 coordiante)
    {
        Vector3 pos = GameCommon.GetMapPos(coordiante);
        //pos.z = -100;

        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(characterDataId);
        if (characterData != null)
        {
            var runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.CHARACTER.ToString(), characterData.objName,
               characterData.obj.transform, instacneId);
            (runtimeObj.obj as Transform).position = pos;
            return runtimeObj;
        }
        return default(RuntimeObj);
    }

    public async void RefreshNpcRuntimeObj(Character character, bool controller = false)
    {
        CharacterRuntimeObj characterRuntimeObj;
        if (characterRuntionObjs.TryGetValue(character, out characterRuntimeObj))
        {
            if (character.mapInstance != WorldMapManager.instance.displayMap)
            {
                GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
                characterRuntionObjs.Remove(character);
            }
            else
            {
                Vector3 pos = GameCommon.GetMapPos(character.coordinate);
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
            if (character.mapInstance == WorldMapManager.instance.displayMap)
            {
                RuntimeObj runtimeObj = await CreatCharacterRuntimeObj(character.dataId, character.instanceId, character.coordinate);
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

    public async Task RefreshNpcRuntimeObj()
    {
        using (var e = characters.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var character = e.Current.Value;
                if (character.mapInstance != WorldMapManager.instance.displayMap
                    && characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
                {
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
                    characterRuntionObjs.Remove(character);
                }
                if (character.mapInstance == WorldMapManager.instance.displayMap)
                {
                    if (!characterRuntionObjs.TryGetValue(character, out characterRuntimeObj))
                    {
                        var runtimeObj = await CreatCharacterRuntimeObj(character.dataId, character.instanceId, character.coordinate);
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
                        Vector3 pos = GameCommon.GetMapPos(character.coordinate);
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
        int2 startCoordinate = controllerCharacter.coordinate;
        Stack<int2> pathNodes = MapCellController.instance.FindPathNode(startCoordinate, targetCoordinate,
            controllerCharacter.mapInstance);
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

        controllerCharacter.moveDirection = moveDirection;
        var playerRuntimeObj = characterRuntionObjs[controllerCharacter].runtimeObj;

        Transform characterTransform = playerRuntimeObj.obj as Transform;

        Vector2 _playerMoveDirction = moveDirection;
        if (!WorldMapManager.instance.InitSmoothMove(ref _playerMoveDirction, characterTransform.position,
            controllerCharacter.mapInstance))
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
                    if (controllerCharacter.coordinate.x != targetCoordinate.x ||
                    controllerCharacter.coordinate.y != targetCoordinate.y)
                    {
                        CrossMap(targetCoordinate, controllerCharacter, out int3 newMap);
                    }
                },
                WorldMapManager.instance.displayMap, playerRuntimeObj.linkId, true);
        }
    }

    private void SetTargetDirection(SetTargetDirection SetTargetDirection)
    {
        if (characters.TryGetValue(SetTargetDirection.characterId, out var character))
        {
            character.moveDirection = math.normalize(SetTargetDirection.targetCoordinate - character.coordinate);
        }
    }

    private void SetDirection(SetDirection SetCharacterDirection)
    {
        if (characters.TryGetValue(SetCharacterDirection.characterId, out var character))
        {
            character.moveDirection = SetCharacterDirection.direction;
        }
    }
}