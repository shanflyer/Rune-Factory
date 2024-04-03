using System.Collections;
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
    public MyShadowPolygon myShadow;
    public SpriteRenderer equipRenderer;

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
    public override bool NeedUpdata => true;

    public const float moveSpeed = 15f;
    public const float updataMoveSpeed = 1f;

    private MyInstance myInstance;
    private Dictionary<int, Character> characters = new Dictionary<int, Character>();
    private Dictionary<int, List<int>> characterInstances = new Dictionary<int, List<int>>();

    private List<int> npcInstances = new List<int>();

    public Player player;
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

    public override void Init()
    {
        base.Init();
        myInstance = new MyInstance();

        GameActionManager.instance.AddListener<SetCharacterProperty>(SetCharacterValue);
        GameActionManager.instance.AddListener<ChangeCharacterProperty>(ChangeCharacterValue);
        GameActionManager.instance.AddListener<SetCharacterCoordinate>(SetCharacterCoordiante);

        GameActionManager.instance.AddListener<CreatCharacter>(CreatCharacter);
        GameActionManager.instance.AddListener<CreatTempCharacter>(CreatTempCharacter);
        GameActionManager.instance.AddListener<DestoryCharacter>(DestoryCharacter);

        GameActionManager.instance.AddListener<CreatDefaultNPC>(CreatDefaultNPC);
        GameActionManager.instance.AddListener<SetCharacterAnimator>(SetCharacterAnimator);
        GameActionManager.instance.AddListener<InitInputAction>(InitInputAction);

        GameActionManager.instance.AddListener<SetDirection>(SetDirection);
        GameActionManager.instance.AddListener<SetTargetDirection>(SetTargetDirection);
        GameActionManager.instance.AddListener<GetCharacterDataId>(GetCharacterDataId);

        GameActionManager.instance.AddListener<CheckCharacterTemp>(CheckCharacterTemp);
        GameActionManager.instance.AddListener<StartCharacterMove>(StartCharacterMove);
        GameActionManager.instance.AddListener<StopCharacterMove>(StopCharacterMove);
        GameActionManager.instance.AddListener<RemoveCharacterMove>(RemoveCharacterMove);
        GameActionManager.instance.AddListener<ChangeEquip>(ChangeEquip);
        GameActionManager.instance.AddListener<ClearEquip>(ClearEquip);
        GameActionManager.instance.AddListener<ChangeCharacter>(ChangeCharacter);
        GameActionManager.instance.AddListener<DisplayOrHideCharacter>(DisplayOrHideCharacter);

        GameActionManager.instance.AddListener<SetCharacterTempPos>(SetCharacterTempPos);
        GameActionManager.instance.AddListener<SetCharacterRandomPos>(SetCharacterRandomPos);
        GameActionManager.instance.AddListener<SetCharacterRandomCoordinate>(SetCharacterRandomCoordinate);
        GameActionManager.instance.AddListener<ChangeCharacterNewMap>(ChangeCharacterNewMap);

        GameActionManager.instance.AddListener<VisitNPC>(VisitNPC);
        GameActionManager.instance.AddListener<DisplayCharacterItemRenderer>(DisplayCharacterItemRenderer);
    }
    void ChangeCharacterNewMap(ChangeCharacterNewMap ChangeCharacterNewMap)
    {
        Character character = GetCharacter(ChangeCharacterNewMap.characterInstance);
        if (character != null)
        {
            int2 coordinate = character.coordinate;
            int3 newCoordiante = new int3(coordinate, ChangeCharacterNewMap.newMap);
            if (!MapCellController.instance.CheckIsWalk(newCoordiante))
            {
                int2 randomCoordinate = MapCellController.instance.GetRandomRoomCell(ChangeCharacterNewMap.newMap);
                newCoordiante = new int3(randomCoordinate, ChangeCharacterNewMap.newMap);
            } 

            character.RemoveMove();
            character.SetCoordinate(newCoordiante);
            ReStartCharacterBehavior reStartCharacterBehavior = new ReStartCharacterBehavior
            {
                characterId = character.instanceId
            };
            GameActionManager.instance.QueueAction(reStartCharacterBehavior);
            RefreshNpcRuntimeObj(character);

            if (ChangeCharacterNewMap.setResult != null)
            {
                ChangeCharacterNewMap.setResult(true);
            }
        }
    }
    private async void DisplayCharacterItemRenderer(DisplayCharacterItemRenderer displayCharacterItemRenderer)
    {
        Character character = GetCharacter(displayCharacterItemRenderer.characterId);
        if (character != null)
        {
            if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
            {
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(displayCharacterItemRenderer.itemId);
                if (itemData != null)
                {
                    characterRuntimeObj.equipRenderer.sprite = itemData.icon;
                    characterRuntimeObj.equipRenderer.enabled = true;
                }
                else
                {
                    characterRuntimeObj.equipRenderer.sprite = null;
                    characterRuntimeObj.equipRenderer.enabled = false;
                }
            }
        }
    }

    private void VisitNPC(VisitNPC visitNPC)
    {
        Character sourceCharacter = GetCharacter(visitNPC.sourceId);
        Character targetCharacter = GetCharacter(visitNPC.targetId);
        if (sourceCharacter != null && targetCharacter != null)
        {
            sourceCharacter.MoveCrossMap(targetCharacter.mapInstance, targetCharacter.coordinate);
        }
    }

    private void SetCharacterRandomCoordinate(SetCharacterRandomCoordinate setCharacterRandomCoordinate)
    {
        Character character = GetCharacter(setCharacterRandomCoordinate.characterId);
        if (character != null)
        {
            int2 targetCoordinate = MapCellController.instance.GetRandomWalkable(new int3(setCharacterRandomCoordinate.Coordinate.xy, character.mapInstance),
                setCharacterRandomCoordinate.range);

            character.RemoveMove();
            character.SetCoordinate(new int3(targetCoordinate, character.mapInstance));
            RefreshNpcRuntimeObj(character);

            if (setCharacterRandomCoordinate.setResult != null)
            {
                setCharacterRandomCoordinate.setResult(true);
            }
        }
        if (setCharacterRandomCoordinate.setResult != null)
        {
            setCharacterRandomCoordinate.setResult(false);
        }
    }

    private void SetCharacterRandomPos(SetCharacterRandomPos SetCharacterRandomPos)
    {
        Character character = GetCharacter(SetCharacterRandomPos.characterId);
        if (character != null && characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            int2 coordinate = GameCommon.GetMapCoordinateInt(SetCharacterRandomPos.pos);
            int2 targetCoordinate = MapCellController.instance.GetRandomWalkable(new int3(coordinate.xy, character.mapInstance), SetCharacterRandomPos.range);

            character.RemoveMove();
            if (character != null)
            {
                character.SetCoordinate(new int3(targetCoordinate, character.mapInstance));

                RefreshNpcRuntimeObj(character);
            }

            if (SetCharacterRandomPos.setResult != null)
            {
                SetCharacterRandomPos.setResult(true);
            }
        }
        if (SetCharacterRandomPos.setResult != null)
        {
            SetCharacterRandomPos.setResult(false);
        }
    }

    private void SetCharacterTempPos(SetCharacterTempPos setCharacterTempPos)
    {
        if (characters.TryGetValue(setCharacterTempPos.characterId, out var character))
        {
            if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
            {
                characterRuntimeObj.animator.transform.position = setCharacterTempPos.pos;
                if (setCharacterTempPos.setResult != null)
                {
                    setCharacterTempPos.setResult(true);
                }
                character.StopMove();
            }
        }

        if (setCharacterTempPos.setResult != null)
        {
            setCharacterTempPos.setResult(false);
        }
    }

    private void DisplayOrHideCharacter(DisplayOrHideCharacter displayOrHideCharacter)
    {
        var character = GetCharacterForDataId(displayOrHideCharacter.characterId);
        if (character == null)
        {
            characters.TryGetValue(displayOrHideCharacter.characterId, out character);
        }
        if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            characterRuntimeObj.animator.gameObject.SetActive(displayOrHideCharacter.display);
        }
    }

    private async void ChangeCharacter(ChangeCharacter ChangeCharacter)
    {
        if (characters.TryGetValue(ChangeCharacter.instanceId, out var character))
        {
            if (character.dataId != ChangeCharacter.newDataId)
            {
                character.dataId = ChangeCharacter.newDataId;
                if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
                {
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
                    EmoteManager.instance.TryRecycleCharacterEmote(character.instanceId);
                    characterRuntionObjs.Remove(character);

                    RuntimeObj runtimeObj = await CreatCharacterRuntimeObj(character.dataId, character.instanceId, character.coordinate);
                    Transform transform = runtimeObj.obj as Transform;
                    characterRuntimeObj = new CharacterRuntimeObj
                    {
                        runtimeObj = runtimeObj,
                        animator = transform.GetComponentInChildren<Animator>(),
                        equipRenderer = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>(),
                        model = transform.Find("Body")
                    };
                    characterRuntionObjs.Add(character, characterRuntimeObj);
                }
            }
        }
    }

    private void ClearEquip(ClearEquip clearEquip)
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
            if (itemId != 0 && clearEquip.outPackageId != 0)
            {
                PackageManager.instance.SetItemInPackage(new Item(itemId, 1), clearEquip.outPackageId);
            }
        }
    }

    private async void ChangeEquip(ChangeEquip changeEquip)
    {
        if (characters.TryGetValue(changeEquip.characterId, out var character))
        {
            PackageManager.instance.GetOutItenFromPackage(changeEquip.outPackageId, changeEquip.itemId, 1);
            ItemData itemData;
            if (changeEquip.outPackageId != 0)
            {
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

    public void RemoveCharacterMove(RemoveCharacterMove removeCharacterMove)
    {
        if (characters.TryGetValue(removeCharacterMove.characterId, out var character))
        {
            character.RemoveMove();
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
        // InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_ClickPos, MapClickAction);
        InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_Move, MoveAction, true);
    }

    public Sprite PlayerHead => controllerCharacter.characterData.head.sprite;
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
                if (SceneManager.instance.Now == "World")
                {
                    UIManager.instance.ShowGamePanel<PlayerTopPanel>();
                    UIManager.instance.ShowGamePanel<ShortcutPanel, ShortcutPackage>(
                    ShortcutManager.instance.GetShortcutPackage(_controllerCharacter.instanceId));
                }
            }
        }
        get
        {
            return _controllerCharacter;
        }
    }

    public Transform controllerTransform
    {
        get
        {
            if (controllerCharacter != null)
            {
                if (characterRuntionObjs.TryGetValue(controllerCharacter, out var characterRuntimeObj))
                {
                    var transform = characterRuntimeObj.animator.transform;
                    return transform;
                }
            }
            return null;
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
            setCharacterAnimator.SetAnimator(animator);
        }
    }

    public async Task CreatPlayer(int id, int bag)
    {
        var playerData = await GameDataManager.instance.GetAsyncData<CharacterData>(id);
        int instanceId = myInstance.CreatInstanceId();
        player = new Player(playerData, instanceId);
        controllerCharacter = player;
        AddCharacter(player);
    }

    /// <summary>
    /// 销毁角色
    /// </summary>
    /// <param name="DestoryTempCharacter"></param>
    private void DestoryCharacter(DestoryCharacter destoryCharacter)
    {
        if (characters.TryGetValue(destoryCharacter.characterId, out var character))
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
            EmoteManager.instance.TryRecycleCharacterEmote(character.instanceId);
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
        Character character;
        if (creatCharacter.isPlayer)
        {
            await CreatPlayer(creatCharacter.characterId, 0);
            character = player;
        }
        else
        {
            var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(creatCharacter.characterId);
            character = new Character(characterData, myInstance.CreatInstanceId());
            AddCharacter(character);
        }

        character.SetObjCoordinate(creatCharacter.mapInstance,
            new int2(creatCharacter.coordinateX, creatCharacter.coordinateY));
        RefreshNpcRuntimeObj(character, creatCharacter.controller);
        if (creatCharacter.controller)
        {
            controllerCharacter = character;
        }
        if (!characterInstances.TryGetValue(creatCharacter.characterId, out var ints))
        {
            ints = new List<int>();
        }
        ints.Add(character.instanceId);
        characterInstances[creatCharacter.characterId] = ints;
        if (creatCharacter.setResult != null)
        {
            creatCharacter.setResult(true);
        }
        if (creatCharacter.setValue != null)
        {
            creatCharacter.setValue(character.instanceId);
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
        if (obj != null)
        {
            //Debug.Log("MoveA:" + obj);
            var moveValue = (Vector2)obj;

            SetControllerCharacterMoveDirection(moveValue);
        }
        else
        {
            SetControllerCharacterMoveDirection(Vector2.zero);
        }
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
        if (characterId == 0)
        {
            character = player;
        }
        else
        {
            characters.TryGetValue(characterId, out character);
        }
        if (character == null)
        {
            if (characterInstances.TryGetValue(characterId, out var ints))
            {
                characters.TryGetValue(ints[0], out character);
            }
        }
        return character;
    }

    private void SetCharacterCoordiante(SetCharacterCoordinate setCharacterCoordinate)
    {
        Character character = GetCharacter(setCharacterCoordinate.characterId);
        character.RemoveMove();
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
            return true;
        }
        coordinate = int3.zero;
        return false;
    }

    private async void SetPlayerPos(Character character)
    {
        if (characterRuntionObjs.TryGetValue(character, out CharacterRuntimeObj characterRuntimeObj))
        {
            if (character.mapInstance != WorldMapObjManager.instance.displayMap)
            {
                EmoteManager.instance.TryRecycleCharacterEmote(character.instanceId);
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
        else if (character.mapInstance == WorldMapObjManager.instance.displayMap)
        {
            var runtimeObj = await CreatCharacterRuntimeObj(character.dataId, character.instanceId, character.coordinate);
            Vector2 pos = GameCommon.GetMapPos(character.coordinate);
            var transform = characterRuntimeObj.runtimeObj.obj as Transform;
            transform.position = pos;
            characterRuntimeObj = new CharacterRuntimeObj
            {
                runtimeObj = runtimeObj,
                animator = transform.GetComponentInChildren<Animator>(),
                equipRenderer = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>(),
                model = transform.Find("Body")
            };
            characterRuntionObjs.Add(character, characterRuntimeObj);
        }
    }

    public void SetCharacterObjPos(Character character, Vector2 pos)
    {
        if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            //SetCharacterAnimationSpeed(0, characterRuntimeObj);
            var transform = characterRuntimeObj.runtimeObj.obj as Transform;
            Vector3 targetPos = new Vector3(pos.x, pos.y, transform.position.z);
            transform.position = targetPos;
            SetCharacterAnimationSpeed(1, characterRuntimeObj);
        }
    }

    public void MoveCharacterObj(Character character, float2 moveValue, ref int2 coordinate)
    {
        if (moveValue.x == float.NaN || moveValue.y == float.NaN)
        {
            return;
        }
        try
        {
            if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
            {
                if (moveValue.Equals(float2.zero))
                {
                    SetCharacterAnimationSpeed(0, characterRuntimeObj);
                }
                else
                {
                    SetCharacterAnimationSpeed(1, characterRuntimeObj);
                    var transform = characterRuntimeObj.runtimeObj.obj as Transform;
                    Vector3 TranslateValue = new Vector3(moveValue.x, moveValue.y, 0);
                    transform.Translate(TranslateValue);

                    var nowCoordinate = GameCommon.GetMapCoordinate(transform.position);
                    coordinate = new int2(nowCoordinate.x, nowCoordinate.y);
                }
            }
        }
        catch { }
    }

    public void SetCharacterObj(Character character, Vector2 pos)
    {
        if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            //SetCharacterAnimationSpeed(0, characterRuntimeObj);
            var transform = characterRuntimeObj.runtimeObj.obj as Transform;
            Vector3 targetPos = new Vector3(pos.x, pos.y, transform.position.z);
            Vector3 offsetPos = targetPos - transform.position;

            IEnumerator LerpPos()
            {
                float timeValue = 0;
                while (timeValue < 0.2f)
                {
                    Vector3 pos = offsetPos * Time.deltaTime / 0.2f;
                    transform.Translate(pos);
                    yield return 0;
                    timeValue += Time.deltaTime;
                }
            }
            GameObjectCurveController.instance.UpDataComponent.StartCoroutine(LerpPos());
        }
    }

    /// <summary>
    /// 移动到一个格子
    /// </summary>
    /// <param name="character"></param>
    /// <param name="targetCoordinate"></param>
    /// <param name="EndAction"></param>
    public void CharacterMoveTarget(Character character, int2 targetCoordinate, MoveEndAction EndAction = null, MoveEndAction changeCoordinateAction = null, float overrideSpeed = 0)
    {
        CharacterRuntimeObj runtimeObj;
        characterRuntionObjs.TryGetValue(character, out runtimeObj);

        Vector2 targetPos = GameCommon.GetMapPos(targetCoordinate);

        var transform = runtimeObj.runtimeObj.obj as Transform;

        Vector2 startPos = transform ? transform.position :
            GameCommon.GetMapPos(character.coordinate);
        bool slant = targetCoordinate.x != character.coordinate.x && targetCoordinate.y != character.coordinate.y;

        character.moveDirection = math.normalizesafe(targetCoordinate - character.coordinate, character.moveDirection);
        float distance = GameCommon.GetCellTrueDistance(targetCoordinate, character.coordinate);

        // var direction = GameCommon.GetCharacterDirect(character.objCoordinate.coordinate, targetCoordinate, character.direction);
        float lineSpeed = slant ? moveSpeed * GameCommon.slantValue : moveSpeed;
        if (overrideSpeed != 0)
        {
            character.nowSpeed = updataMoveSpeed * math.length(character.moveDirection) / distance;
        }
        Vector2Int offsetCoordinate = Vector2Int.zero;
        character.moveEnumeratorId =
        GameObjectCurveController.instance.Line(character.nowSpeed, startPos, targetPos, (Vector2 pos) =>
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
                character.nowSpeed = 0;
                SetCharacterAnimationSpeed(0, runtimeObj);
                CrossMap(targetCoordinate, character, out int3 newMap, EndAction);
                if (changeCoordinateAction != null)
                {
                    changeCoordinateAction.Invoke();
                }
            }
      );
    }

    public void CharacterMoveTarget(Character character, Stack<int2> pathNodes, MoveEndAction EndAction = null,
        MoveEndAction changeCoordinateAction = null, MoveEndAction failedMoveAction = null)
    {
        var targetCoordinate = pathNodes.Pop();
        CharacterRuntimeObj runtimeObj;
        characterRuntionObjs.TryGetValue(character, out runtimeObj);

        Vector2 targetPos = GameCommon.GetMapPos(targetCoordinate);

        var transform = runtimeObj.runtimeObj.obj as Transform;

        Vector2 startPos = transform ? transform.position :
            GameCommon.GetMapPos(character.coordinate);
        bool slant = targetCoordinate.x != character.coordinate.x && targetCoordinate.y != character.coordinate.y;

        character.moveDirection = math.normalizesafe(targetCoordinate - character.coordinate, character.moveDirection);
        // var direction = GameCommon.GetCharacterDirect(character.objCoordinate.coordinate, targetCoordinate, character.direction);
        if (!MapCellController.instance.CheckIsWalk(targetCoordinate, character.mapInstance))
        {
            if (failedMoveAction != null)
            {
                failedMoveAction();
            }
            return;
        }

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

                    CharacterMoveTarget(character, pathNodes, EndAction, changeCoordinateAction, failedMoveAction);
                }
                else
                {
                    SetCharacterAnimationSpeed(0, runtimeObj);
                    CrossMap(targetCoordinate, character, out int3 newMap, EndAction);
                }
                if (changeCoordinateAction != null)
                {
                    changeCoordinateAction.Invoke();
                }
            }
             );
    }

    public bool CrossMap(int2 targetCoordinate, Character character, out int3 newMap, MoveEndAction EndAction = null)
    {
        // int2 offsetCoordinate = targetCoordinate - character.coordinate;
        character.SetCoordinate(new int3(targetCoordinate.xy, character.mapInstance));

        if (character.CanMoveCrossMap)
        {
            MapCellController.instance.ChangeMapAction(targetCoordinate, character.direction, character.mapInstance, ChangeMapAction);

            void ChangeMapAction(int3 newMap, int afterAction)
            {
                int targetMap = newMap.z;
                targetCoordinate = new int2(newMap.x, newMap.y);

                if (character == controllerCharacter)
                {
                    character.StopMove();
                    character.canMove = false;
                    LerpScreenCycleValue lerpScreenCycleValue = new LerpScreenCycleValue
                    {
                        cyclePos = GameCommon.GetMapPos(character.coordinate),
                        minCycleValue = 0,
                        maxCycleValue = 1,
                        lerpTime = GameCommon.mapChangeLerpTime
                    };
                    GameActionManager.instance.QueueAction(lerpScreenCycleValue, true);
                    character.SetCoordinate(new int3(targetCoordinate, targetMap));

                    GameTimerController.instance.DeleyActionMain((int)(GameCommon.mapChangeLerpTime * 1000), async () =>
                    {
                        WorldMapObjManager.instance.RecycleMap();
                        SetPlayerPos(character);
                        await WorldMapObjManager.instance.DisplayMap(targetMap);

                        GameTimerController.instance.DeleyActionMain((int)(GameCommon.mapChangeLerpTime * 1000), () =>
                        {
                            LerpScreenCycleValue lerpScreenCycleValue = new LerpScreenCycleValue
                            {
                                cyclePos = GameCommon.GetMapPos(character.coordinate),
                                minCycleValue = 1,
                                maxCycleValue = 0,
                                lerpTime = GameCommon.mapChangeLerpTime,
                                setResult = AfterLerpScreenCycle
                            };

                            async void AfterLerpScreenCycle(bool value)
                            {
                                if (afterAction != 0)
                                {
                                    var dataAction = await GameDataManager.instance.GetAsyncData<GameActionData>();
                                    if (dataAction)
                                    {
                                        dataAction.Action();
                                    }
                                }

                                character.canMove = true;
                            }
                            GameActionManager.instance.QueueAction(lerpScreenCycleValue, true);
                        });
                    });
                }
                else
                {
                    SetPlayerPos(character);
                }
            }
        }
        /*
        if (character.CanMoveCrossMap &&
            MapCellController.instance.ChangeMap(targetCoordinate, character.direction, character.mapInstance, out newMap))
        {
            int targetMap = newMap.z;
            targetCoordinate = new int2(newMap.x, newMap.y);

            if (character == controllerCharacter)
            {
                character.StopMove();
                LerpScreenCycleValue lerpScreenCycleValue = new LerpScreenCycleValue
                {
                    cyclePos = GameCommon.GetMapPos(character.coordinate),
                    minCycleValue = 0,
                    maxCycleValue = 1,
                    lerpTime = GameCommon.mapChangeLerpTime
                };
                GameActionManager.instance.QueueAction(lerpScreenCycleValue,true);
                character.SetCoordinate(new int3(targetCoordinate, targetMap));

                GameTimerController.instance.DeleyActionMain((int)(GameCommon.mapChangeLerpTime * 1000), async () =>
                {
                    WorldMapObjManager.instance.RecycleMap();
                    await   WorldMapObjManager.instance.DisplayMap(targetMap);
                    SetPlayerPos(character);

                    GameTimerController.instance.DeleyActionMain((int)(GameCommon.mapChangeLerpTime * 1000), () =>
                    {
                        LerpScreenCycleValue lerpScreenCycleValue = new LerpScreenCycleValue
                        {
                            cyclePos = GameCommon.GetMapPos(character.coordinate),
                            minCycleValue = 1,
                            maxCycleValue = 0,
                            lerpTime = GameCommon.mapChangeLerpTime
                        };
                        GameActionManager.instance.QueueAction(lerpScreenCycleValue, true);
                    });
                });
            }
            else
            {
                SetPlayerPos(character);
            }
        }*/
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

    private void CreatPlayer(CharacterSaveData characterSaveData)
    {
        player = new Player(characterSaveData);
        player.SetCoordinate(new int3(int2.zero, WorldMapObjManager.instance.displayMap));
        controllerCharacter = player;
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
        if (NPCManager.instance.GetNPC(mapNpcData.dataId, out var npc))
        {
            if (!characters.TryGetValue(npc.characterId, out var character))
            {
                var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(mapNpcData.dataId);
                character = new Character(characterData, npc.characterId);
                characters.Add(npc.characterId, character);

                if (!characterInstances.TryGetValue(character.dataId, out var ints))
                {
                    ints = new List<int>();
                }
                ints.Add(character.instanceId);
                characterInstances[character.dataId] = ints;
            }
            character.SetCoordinate(new int3(mapNpcData.beginCoordinate, mapNpcData.beginMap));
            RefreshNpcRuntimeObj(character);

            if (mapNpcData.externalBehavior && mapNpcData.beginMap > 0)
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
            if (character.mapInstance != WorldMapObjManager.instance.displayMap)
            {
                GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
                EmoteManager.instance.TryRecycleCharacterEmote(character.instanceId);
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
            if (character.mapInstance == WorldMapObjManager.instance.displayMap)
            {
                RuntimeObj runtimeObj = await CreatCharacterRuntimeObj(character.dataId, character.instanceId, character.coordinate);
                Transform transform = runtimeObj.obj as Transform;
                characterRuntimeObj = new CharacterRuntimeObj
                {
                    runtimeObj = runtimeObj,
                    animator = transform.GetComponentInChildren<Animator>(),
                    equipRenderer = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>(),
                    model = transform.Find("Body"),
                    // myShadow=transform.GetComponentInChildren<MyShadowPolygon>(),
                };
                /*
                if (characterRuntimeObj.myShadow)
                {
                    characterRuntimeObj.myShadow.CreatMesh();
                }*/
                characterRuntionObjs.Add(character, characterRuntimeObj);
                if (controller)
                {
                    CameraManager.instance.SetFollowTarget(transform);
                }
            }
        }
    }

    public void RecycleCharacter()
    {
        foreach (var characterRuntime in characterRuntionObjs)
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntime.Value.runtimeObj);
        }
        characterRuntionObjs.Clear();
    }

    public async Task RefreshNpcRuntimeObj()
    {
        using (var e = characters.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var character = e.Current.Value;
                if (character.mapInstance != WorldMapObjManager.instance.displayMap
                    && characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
                {
                    EmoteManager.instance.TryRecycleCharacterEmote(character.instanceId);
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
                    characterRuntionObjs.Remove(character);
                }
                if (character.mapInstance == WorldMapObjManager.instance.displayMap)
                {
                    if (!characterRuntionObjs.TryGetValue(character, out characterRuntimeObj))
                    {
                        var runtimeObj = await CreatCharacterRuntimeObj(character.dataId, character.instanceId, character.coordinate);
                        Transform transform = runtimeObj.obj as Transform;
                        characterRuntimeObj = new CharacterRuntimeObj
                        {
                            runtimeObj = runtimeObj,
                            animator = transform.GetComponentInChildren<Animator>(),
                            equipRenderer = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>(),
                            model = transform.Find("Body")
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
        controllerCharacter.StopMove();
        controllerCharacter.canMove = false;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        int2 targetCoordinate = GameCommon.GetMapCoordinateInt(mousePos);
        int2 startCoordinate = controllerCharacter.coordinate;
        Stack<int2> pathNodes = MapCellController.instance.FindPathNode(startCoordinate, targetCoordinate,
            controllerCharacter.mapInstance, true);
        controllerCharacter.PlayerMove(pathNodes);
    }

    /*
    protected override void UpData()
    {
        base.UpData();
        if (controllerCharacter == null)
        {
            return;
        }
        if (controllerCharacter.canMove&& !controllerCharacter.moveDirection.Equals(float2.zero))
        {
            var playerRuntimeObj = characterRuntionObjs[controllerCharacter].runtimeObj;
            Transform characterTransform = playerRuntimeObj.obj as Transform;
            float distance = updataMoveSpeed * Time.deltaTime;
            TryTeamLeaderMove tryTeamLeaderMove = new TryTeamLeaderMove
            {
                characterId = controllerCharacter.instanceId,
                length = distance
            };
            GameActionManager.instance.QueueAction(tryTeamLeaderMove, true);
            Vector2 directValue = controllerCharacter.moveDirection;
            characterTransform.Translate(directValue * distance);
            int2 targetCoordinate = GameCommon.GetMapCoordinateInt(characterTransform.position);

            if (controllerCharacter.coordinate.x != targetCoordinate.x ||
            controllerCharacter.coordinate.y != targetCoordinate.y)
            {
                CrossMap(targetCoordinate, controllerCharacter, out int3 newMap);
                // Debug.Log($"targetCoordinate:{targetCoordinate}");
                TryTeamLeaderSetCoordinate tryTeamLeaderSetCoordinate = new TryTeamLeaderSetCoordinate
                {
                    characterId = controllerCharacter.instanceId
                };
                GameActionManager.instance.QueueAction(tryTeamLeaderSetCoordinate, true);
            }
        }
    }*/

    //设置可操控的角色移动方向
    public void SetControllerCharacterMoveDirection(Vector2 moveDirection)
    {
        if (controllerCharacter == null)
        {
            return;
        }
        if (characterRuntionObjs.TryGetValue(controllerCharacter, out var playerRuntimeObj))
        {
            float speed = moveDirection.magnitude;
            controllerCharacter.nowSpeed = speed;

            controllerCharacter.moveDirection = moveDirection;

            Transform characterTransform = playerRuntimeObj.animator.transform;

            Vector2 _playerMoveDirction = moveDirection;
            Vector2 targetPos = characterTransform.position;
            float distance = updataMoveSpeed * Time.deltaTime;
            if (!WorldMapManager.instance.InitSmoothMove(ref _playerMoveDirction, characterTransform.position,
                controllerCharacter.mapInstance, distance))
            {
                TryTeamLeaderMove tryTeamLeaderMove = new TryTeamLeaderMove
                {
                    characterId = controllerCharacter.instanceId,
                    length = 0
                };
                GameActionManager.instance.QueueAction(tryTeamLeaderMove, true);

                GameObjectCurveController.instance.StopObjectMove(playerRuntimeObj.runtimeObj.linkId);
            }
            else
            {
                GameObjectCurveController.instance.ObjectMove(
                    () => { return characterTransform.position; },
                    () => { return controllerCharacter.moveDirection; },
                    (int2 targetCoordinate, Vector2 targetPos) =>
                    {
                        if (controllerCharacter.canMove)
                        {
                            float length = Vector2.Distance(targetPos, new Vector2(characterTransform.position.x, characterTransform.position.y));
                            TryTeamLeaderMove tryTeamLeaderMove = new TryTeamLeaderMove
                            {
                                characterId = controllerCharacter.instanceId,
                                length = distance
                            };
                            GameActionManager.instance.QueueAction(tryTeamLeaderMove, true);
                            characterTransform.position = new Vector3(targetPos.x, targetPos.y, characterTransform.position.z);
                            if (controllerCharacter.coordinate.x != targetCoordinate.x ||
                            controllerCharacter.coordinate.y != targetCoordinate.y)
                            {
                                CrossMap(targetCoordinate, controllerCharacter, out int3 newMap);
                                // Debug.Log($"targetCoordinate:{targetCoordinate}");
                                TryTeamLeaderSetCoordinate tryTeamLeaderSetCoordinate = new TryTeamLeaderSetCoordinate
                                {
                                    characterId = controllerCharacter.instanceId
                                };
                                GameActionManager.instance.QueueAction(tryTeamLeaderSetCoordinate, true);
                            }
                        }
                    },
                    WorldMapObjManager.instance.displayMap, playerRuntimeObj.runtimeObj.linkId, true);
            }
        }
    }

    private void SetTargetDirection(SetTargetDirection SetTargetDirection)
    {
        if (characters.TryGetValue(SetTargetDirection.characterId, out var character))
        {
            character.moveDirection = math.normalizesafe(SetTargetDirection.targetCoordinate - character.coordinate, character.moveDirection);
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