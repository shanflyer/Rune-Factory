using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public delegate void MoveEndAction(); 
 
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
    private MyDic<int, Character> characters = new MyDic<int, Character>();
    private Dictionary<int, int> characterDataToInstances = new Dictionary<int, int>();
    private HashSet<int> tempInstances = new HashSet<int>();

    public Player player;
    //private Vector2 playerMoveDirction;

    void SetPlayerNeighboor()
    {

    }

    public List<Character> GetAllCharacters()
    {
        return characters.GetValueList();
    }

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

    private void RecycleCharacterObj(Character character)
    {
        if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            characterRuntimeObj.Clear();
            characterRuntionObjs.Remove(character);
        }
        EmoteManager.instance.TryRecycleCharacterEmote(character.instanceId);
        FishController.instance.RecycleFisherObj(character.instanceId);
    }

    private async Task CreatCharacterObjAsync(Character character, bool controller = false)
    {
        var runtimeObj = await CreatCharacterRuntimeObj(character.dataId, character.instanceId, character.coordinate);
        CharacterRuntimeObj characterRuntimeObj = runtimeObj.obj as CharacterRuntimeObj;
        characterRuntimeObj.runtimeObj = runtimeObj;
        characterRuntionObjs.Add(character, characterRuntimeObj);

        //Vector2 pos = GameCommon.GetMapPos(character.coordinate);
        //transform.position = pos;

        if (controller || character == controllerCharacter)
        {
            CameraManager.instance.SetFollowTarget(characterRuntimeObj.transform);
            ControllerRuntimeObj = characterRuntimeObj;
        }
    }

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
        GameActionManager.instance.AddListener<ClearTempCharacter>(ClearTempCharacter);

        GameActionManager.instance.AddListener<TempCharacterTalk>(TempCharacterTalk);
        GameActionManager.instance.AddListener<SetTempCharacterTarget>(SetTempCharacterTarget);
    }
    void SetTempCharacterTarget(SetTempCharacterTarget setTempCharacterTarget)
    {
        if(characters.TryGetValue(setTempCharacterTarget.characterId,out var character))
        {
            if(character is TempCharacter tempCharacter)
            {
                tempCharacter.SetTargetArea(setTempCharacterTarget.area, setTempCharacterTarget.targetCoordinate);
            }
        }
    }
    void TempCharacterTalk(TempCharacterTalk tempCharacterTalk)
    {
        Character character = GetCharacter(tempCharacterTalk.characterId);
        if (character != null)
        {
            if(character is TempCharacter tempCharacter)
            {
                int talkId = tempCharacter.GetTalk();
                 
                SimpleTalk simpleTalk = new SimpleTalk
                {
                    characterId = tempCharacter.instanceId,
                    talkId = talkId,
                    endAction=tempCharacterTalk.endAction
                };
                GameActionManager.instance.QueueAction(simpleTalk);
            }

        }
    }
    private void ChangeCharacterNewMap(ChangeCharacterNewMap ChangeCharacterNewMap)
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

                   await CreatCharacterObjAsync(character);
                }
            }
        }
    }

    private void ClearEquip(ClearEquip clearEquip)
    {
        if (characters.TryGetValue(clearEquip.characterId, out var character))
        {
            int itemId = 0;
            switch (clearEquip.itemType)
            {
                case ItemType.武器:
                    itemId = character.Equip.weapon.x;
                    break;

                case ItemType.防具:
                    itemId = character.Equip.clothes.x;
                    break;

                case ItemType.鞋子:
                    itemId = character.Equip.shoes.x;
                    break;
                case ItemType.帽子:
                    itemId = character.Equip.headgear.x;
                    break;
            }
            character.ClearEquip(clearEquip.itemType);
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
            // PackageManager.instance.GetOutItenFromPackage(changeEquip.outPackageId, changeEquip.itemId, 1);
            ItemData itemData;
            if (changeEquip.outPackageId == 0)
            {
                itemData = await GameDataManager.instance.GetAsyncData<ItemData>(changeEquip.itemId);
            }
            else
            {
                Item item = PackageManager.instance.GetItemFromInstanceId(changeEquip.outPackageId, changeEquip.itemId);
                itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
            }

            RemovePackageItemInstance removePackageItemInstance = new RemovePackageItemInstance
            {
                itemInstanceId = changeEquip.itemId,
                packageId = changeEquip.outPackageId
            };
            GameActionManager.instance.QueueAction(removePackageItemInstance, true);

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
                    UIManager.instance.ShowGamePanel<CharacterButtonPanel>();
                    var shortcutPackage = ShortcutManager.instance.GetShortcutPackage(_controllerCharacter.instanceId);
                    UIManager.instance.ShowGamePanel<ShortcutPanel, ShortcutPackage>(shortcutPackage);
                }
                characterRuntionObjs.TryGetValue(controllerCharacter, out var _ControllerRuntimeObj);
                ControllerRuntimeObj = _ControllerRuntimeObj;
            }
        }
        get
        {
            return _controllerCharacter;
        }
    }

    public Transform controllerTransform;
    private CharacterRuntimeObj controllerRuntimeObj;

    public CharacterRuntimeObj ControllerRuntimeObj
    {
        get
        {
            if (controllerRuntimeObj == null)
            {
                if (controllerCharacter != null)
                {
                    characterRuntionObjs.TryGetValue(controllerCharacter, out controllerRuntimeObj);
                }
            }
            return controllerRuntimeObj;
        }
        set
        {
            controllerRuntimeObj = value;
            if (value != null)
            {
                controllerTransform = controllerRuntimeObj.animator.transform;
            }
        }
    }

    public float GetDistanceController(int2 coordinate)
    {
        return math.distance(controllerCharacter.coordinate, coordinate);
    }

    private void SetCharacterAnimator(SetCharacterAnimator setCharacterAnimator)
    {
        if (GetRuntimeCharacterObj(setCharacterAnimator.characterId, out CharacterRuntimeObj characterRuntimeObj))
        {
            Animator animator = characterRuntimeObj.animator;
            setCharacterAnimator.SetAnimator(animator);
        }
    }

    public async Task CreatPlayer(int id, int bag, int instanceId = 0)
    {
        var playerData = await GameDataManager.instance.GetAsyncData<CharacterData>(id);
        if (instanceId == 0)
        {
            instanceId = myInstance.CreatInstanceId();
        }
        ProfessionData professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(playerData.profession);
        player = new Player(playerData, instanceId, professionData);
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
    void DestoryCharacter(int destoryCharacter)
    {
        if (characters.TryGetValue(destoryCharacter, out var character))
        {
            RemoveCharacter(character);
        }
    }

    private void ClearTempCharacter(ClearTempCharacter clearTempCharacter)
    {
        var tems = tempInstances.ToArray();
        for(int i = 0; i < tems.Length; i++)
        {
            if (characters.TryGetValue(tems[i], out var character))
            {
                RemoveCharacter(character);
            }
        }
        tempInstances.Clear();
    }

    private void RemoveCharacter(Character character)
    {
        if (character is TempCharacter)
        {
            tempInstances.Remove(character.instanceId);
        }
        else
        {
            characterDataToInstances.Remove(character.dataId);
        }

        if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj);
            EmoteManager.instance.TryRecycleCharacterEmote(character.instanceId);
            characterRuntionObjs.Remove(character);
        }
        MapCellController.instance.RemoveCharacterCoordinate(character.ObjCoordinate, character.instanceId);
        CharacterBehaviorManager.instance.DestroyBehavior(character.instanceId);
        characters.Remove(character.instanceId);
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
        var professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
        TempCharacter character = new TempCharacter(characterData, professionData, myInstance.CreatInstanceId(), tempCharacterData);

        AddCharacter(character);
        character.SetObjCoordinate(creatTempCharacter.mapInstance,
            new int2(creatTempCharacter.coordinateX, creatTempCharacter.coordinateY));
        await RefreshNpcRuntimeObj(character);

        character.templevel = level; 
        if (creatTempCharacter.setValue != null)
        {
            creatTempCharacter.setValue(character.instanceId);
        }
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
            int instanceId = creatCharacter.instanceId;
            if (instanceId == 0)
            {
                instanceId = myInstance.CreatInstanceId();
            }
            var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(creatCharacter.characterId);
            var professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
            character = new Character(characterData, professionData, instanceId);
            AddCharacter(character);
        }

        character.SetObjCoordinate(creatCharacter.mapInstance,
            new int2(creatCharacter.coordinateX, creatCharacter.coordinateY));
        RefreshNpcRuntimeObj(character, creatCharacter.controller);
        if (creatCharacter.controller)
        {
            controllerCharacter = character;
        }
        characterDataToInstances[creatCharacter.characterId] = character.instanceId;
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
        if (character is TempCharacter)
        {
            tempInstances.Add(character.instanceId);
        }
        else
        {
            characterDataToInstances.Add(character.dataId, character.instanceId);
        }
        characters.TrySetValue(character.instanceId,character);
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
        characterRuntimeObj = null;

        return false;
    }

    public Character GetCharacterForDataId(int dataId)
    {
        if (dataId == 0)
        {
            return player;
        }
        if (characterDataToInstances.TryGetValue(dataId, out var instance))
        {
            return characters[instance];
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
            if (characterDataToInstances.TryGetValue(characterId, out var instanceId))
            {
                characters.TryGetValue(instanceId, out character);
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
        else if (FightManager.instance.GetFightCharacter(changeCharacterProperty.characterId, out FightCharacter fightCharacter))
        {
            fightCharacter.ChangeCharacterValue(changeCharacterProperty);
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
                RecycleCharacterObj(character);
            }
            else
            {
                if (characterRuntimeObj)
                {
                    Vector2 pos = GameCommon.GetMapPos(character.coordinate);
                    var transform = characterRuntimeObj.transform;
                    transform.position = pos;
                    //transform.Translate(new Vector3(0, 0, -100));
                }
            }
        }
        else if (character.mapInstance == WorldMapObjManager.instance.displayMap)
        {
          await  CreatCharacterObjAsync(character);
        }
    }

    public void SetCharacterObjPos(Character character, Vector2 pos)
    {
        if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            //SetCharacterAnimationSpeed(0, characterRuntimeObj);
            var transform = characterRuntimeObj.transform;
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
                    var transform = characterRuntimeObj.transform;
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
            var transform = characterRuntimeObj.transform;
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
        Vector2 targetPos = GameCommon.GetMapPos(targetCoordinate);
        Vector2 startPos = GameCommon.GetMapPos(character.coordinate);
        if (characterRuntionObjs.TryGetValue(character, out CharacterRuntimeObj runtimeObj))
        {
            var transform = runtimeObj.transform;
            startPos = transform.position;
        }

        bool slant = targetCoordinate.x != character.coordinate.x && targetCoordinate.y != character.coordinate.y;

        character.moveDirection = math.normalizesafe(targetCoordinate - character.coordinate, character.moveDirection);
        float distance = GameCommon.GetCellTrueDistance(targetCoordinate, character.coordinate);

        // var direction = GameCommon.GetCharacterDirect(character.objCoordinate.coordinate, targetCoordinate, character.direction);
        float lineSpeed = slant ? moveSpeed * GameCommon.slantValue : moveSpeed;
        if (overrideSpeed != 0)
        {
            character.nowSpeed = character.propertySpeed * updataMoveSpeed * math.length(character.moveDirection) / distance;
        }
        Vector2Int offsetCoordinate = Vector2Int.zero;
        character.moveEnumeratorId =
        GameObjectCurveController.instance.Line(character.nowSpeed, startPos, targetPos, (Vector2 pos) =>
        {
            if (runtimeObj != null&&runtimeObj.gameObject.activeSelf)
            {
                SetCharacterAnimationSpeed(1, runtimeObj);
                var transform = runtimeObj.transform;
                transform.transform.position = pos;
                //transform.Translate(Vector3.zero);
            }
        },
            () =>
            {
                character.nowSpeed = 0;
                if (runtimeObj != null)
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
        Vector2 targetPos = GameCommon.GetMapPos(targetCoordinate);
        Vector2 startPos = GameCommon.GetMapPos(character.coordinate);
        if (characterRuntionObjs.TryGetValue(character, out var runtimeObj))
        {
            var transform = runtimeObj.transform;
            startPos = transform.position;
        }
       // Debug.Log($"next cell:{targetCoordinate}");
        bool slant = targetCoordinate.x != character.coordinate.x && targetCoordinate.y != character.coordinate.y;
        character.moveDirection = math.normalizesafe(targetCoordinate - character.coordinate, character.moveDirection);
        // var direction = GameCommon.GetCharacterDirect(character.objCoordinate.coordinate, targetCoordinate, character.direction);
        if (!MapCellController.instance.CheckIsWalk(targetCoordinate, character.mapInstance))
        {
            Debug.Log($"不可走！");
            if (failedMoveAction != null)
            {
                failedMoveAction();
            }
            return;
        }

        Vector2Int offsetCoordinate = Vector2Int.zero;

        var lineSpeed = slant ? moveSpeed * GameCommon.slantValue : moveSpeed;
        lineSpeed *= character.propertySpeed;
        character.moveEnumeratorId =
        GameObjectCurveController.instance.Line(lineSpeed, startPos, targetPos, (Vector2 pos) =>
             {
                 if (runtimeObj != null)
                 {
                     SetCharacterAnimationSpeed(1, runtimeObj);
                     try
                     {
                         var transform = runtimeObj.transform;
                         if (transform)
                         {
                             transform.transform.position = pos;
                             //transform.Translate(Vector3.zero);
                         }
                     }
                     catch(Exception e)
                     {
                         Debug.Log(e);
                     }
                    
                 }
                 else
                 {
                     if (runtimeObj != null)
                         runtimeObj.runtimeObj = null; 
                     runtimeObj = null;
                 }
             },
            () =>
            {
               //Debug.Log($"pathNodes.count:{pathNodes.Count}");
                if (pathNodes.Count > 0)
                {
                    character.SetCoordinate(new int3(targetCoordinate.xy, character.mapInstance));

                    CharacterMoveTarget(character, pathNodes, EndAction, changeCoordinateAction, failedMoveAction);
                }
                else
                {
                    if (runtimeObj != null)
                    {
                        SetCharacterAnimationSpeed(0, runtimeObj);
                    }
                    else
                    {
                        if (runtimeObj != null)
                            runtimeObj.runtimeObj = null; 
                        runtimeObj = null;
                    }
                        
                    CrossMap(targetCoordinate, character, out int3 newMap, EndAction);
                }
                if (changeCoordinateAction != null)
                {
                    changeCoordinateAction.Invoke();
                }
            }
             );
    }

    public void FixedTransMap(Character character, int3 coordinate)
    {
        ChangeMapAction(character, coordinate, 0);
    }

    private void ChangeMapAction(Character character, int3 newMap, int afterAction)
    {
        int targetMap = newMap.z;
        var targetCoordinate = new int2(newMap.x, newMap.y);
        
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

            GameTimerController.instance.DelayAction((int)(GameCommon.mapChangeLerpTime * 1000), async () =>
            {
                WorldMapObjManager.instance.RecycleMap();
                SetPlayerPos(character);
                await WorldMapObjManager.instance.DisplayMap(targetMap); 
            });

            GameTimerController.instance.DelayAction((int)(GameCommon.mapChangeLerpTime * 2000), () =>
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
        }
        else
        {
            character.SetCoordinate(new int3(targetCoordinate, targetMap));
            SetPlayerPos(character);
        }
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
                this.ChangeMapAction(character, newMap, afterAction);
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

    private async void CreatPlayer(CharacterSaveData characterSaveData)
    {
        CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(characterSaveData.dataId);
        ProfessionData professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
        player = new Player(characterData, characterSaveData.instanceId, professionData);
        player.SetCoordinate(new int3(int2.zero, WorldMapObjManager.instance.displayMap));
        controllerCharacter = player;
        AddCharacter(player);

        //BehaviorTree behaviorTree = BehaviorManager.Instance.CreatBehaviorTree(GameManager.instance.testTreeData);
        /*
        RuntimeObj runtimeObj=await GameRuntimeObjManager.Instance.CreatCharacterRuntimeObj(player);
        characterRuntionObjs.Add(player, runtimeObj); */
    }
  
    public async Task CreatNpc(MapNpcData mapNpcData)
    {
        if (NPCManager.instance.GetNPC(mapNpcData.dataId, out var npc))
        {
            npc.isActive = mapNpcData.beginMap > 0;
              
            if (!characters.TryGetValue(npc.characterInstance, out var character))
            { 
                var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(mapNpcData.dataId);
                var professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
                character = new Character(characterData, professionData, npc.characterInstance);
                characters.Add(npc.characterInstance, character); 
                characterDataToInstances[character.dataId] = character.instanceId;
            }
            character.SetCoordinate(new int3(mapNpcData.beginCoordinate, mapNpcData.beginMap));
            await RefreshNpcRuntimeObj(character);  
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

    private async Task<RuntimeObj> CreatCharacterRuntimeObj(int characterDataId, int instacneId, int2 coordiante)
    {
        Vector3 pos = GameCommon.GetMapPos(coordiante);
        //pos.z = -100;

        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(characterDataId);
        if (characterData != null)
        {
            var runtimeObj =await GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.CHARACTER.ToString(), characterData.obj.name,
               characterData.obj, instacneId);
            (runtimeObj.obj as CharacterRuntimeObj).transform.position = pos;
            return runtimeObj;
        }
        return null;
    }

    public async Task RefreshNpcRuntimeObj(Character character, bool controller = false)
    {
        CharacterRuntimeObj characterRuntimeObj;
        if (characterRuntionObjs.TryGetValue(character, out characterRuntimeObj))
        {
            if (character.mapInstance != WorldMapObjManager.instance.displayMap)
            {
                RecycleCharacterObj(character);
                if (character == controllerCharacter)
                {
                    ControllerRuntimeObj = null;
                }
            }
            else
            {
                Vector3 pos = GameCommon.GetMapPos(character.coordinate);
                var transform = characterRuntimeObj.transform;
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
                await CreatCharacterObjAsync(character); 
            }
        } 
    }

    public void RecycleCharacter()
    {
        foreach (var characterRuntime in characterRuntionObjs)
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntime.Value.runtimeObj);
            FishController.instance.RecycleFisherObj(characterRuntime.Key.instanceId);
        }
        characterRuntionObjs.Clear();
    }

    public async Task RefreshNpcRuntimeObj()
    {
        for(int i = characters.length-1; i >=0; i--)
        {
            var character = characters[i];
            if (character.mapInstance != WorldMapObjManager.instance.displayMap)
            {
                if(characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
                {
                    RecycleCharacterObj(character);
                }
                if(character is TempCharacter tempCharacter)
                {
                    DestoryCharacter(character.instanceId);
                }
            }
            else
            {
                if (!characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
                {
                    await CreatCharacterObjAsync(character);
                }
                else
                {
                    Vector3 pos = GameCommon.GetMapPos(character.coordinate);
                    Transform transform = characterRuntimeObj.transform;
                    transform.localPosition = pos;
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

    /// <summary>
    /// 平滑移动目标
    /// </summary>
    /// <param name="startCoordinate"></param>
    /// <param name="direction"></param>
    /// <param name="nowPos"></param>
    /// <param name="mapId"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    public bool CheckSmoothMove(int2 startCoordinate, ref Vector2 direction, Vector2 nowPos, int mapId, float speed)
    {
        if (direction == Vector2.zero)
        {
            return false;
        }
        Vector2 _direction = direction;
        Vector2 checkTargetPos = nowPos + direction * speed * GameCommon.freedomMoveValue * Time.deltaTime;

        /*
         _direction.x *= GameCommon.cellWidth;
         _direction.y *= GameCommon.cellHigh;

         Vector2 checkTargetPos = nowPos + _direction;*/
        int2 checkTargetCoordinate = GameCommon.GetMapCoordinateInt(checkTargetPos);
        // Debug.Log($"{startCoordinate}-checkTargetCoordinate{checkTargetCoordinate}");
        if (MapCellController.instance.CheckIsWalk(checkTargetCoordinate, mapId))
        {
            // Debug.Log($"-WALK!!");
            return true;
        }
        // Debug.Log($"checkTargetCoordinate{false}");
        float absX = math.abs(_direction.x);
        float absY = math.abs(_direction.y);
        if (absX != 0 && absY != 0)
        {
            if (absX >= absY)
            {
                int2 direction1 = new int2(_direction.x < 0 ? -1 : 1, 0);
                checkTargetCoordinate = startCoordinate + direction1;
                //Debug.Log($"direction1{checkTargetCoordinate}");
                if (MapCellController.instance.CheckTryMoveTarget(startCoordinate, checkTargetCoordinate, mapId))
                {
                    direction.x = direction1.x;
                    direction.y = direction1.y;
                    return true;
                }

                int2 direction2 = new int2(0, _direction.y < 0 ? -1 : 1);
                checkTargetCoordinate = startCoordinate + direction2;
                // Debug.Log($"direction2{checkTargetCoordinate}");
                if (MapCellController.instance.CheckTryMoveTarget(startCoordinate, checkTargetCoordinate, mapId))
                {
                    direction.x = direction2.x;
                    direction.y = direction2.y;
                    return true;
                }

                int2 direction3 = new int2(0, _direction.y < 0 ? 1 : -1);
                checkTargetCoordinate = startCoordinate + direction3;
                // Debug.Log($"direction3{checkTargetCoordinate}");
                if (MapCellController.instance.CheckTryMoveTarget(startCoordinate, checkTargetCoordinate, mapId))
                {
                    direction.x = direction3.x;
                    direction.y = direction3.y;
                    return true;
                }
            }
            else
            {
                int2 direction2 = new int2(0, _direction.y < 0 ? -1 : 1);
                checkTargetCoordinate = startCoordinate + direction2;
                // Debug.Log($"directionx1{checkTargetCoordinate}");
                if (MapCellController.instance.CheckTryMoveTarget(startCoordinate, checkTargetCoordinate, mapId))
                {
                    direction.x = direction2.x;
                    direction.y = direction2.y;
                    return true;
                }

                int2 direction1 = new int2(_direction.x < 0 ? -1 : 1, 0);
                checkTargetCoordinate = startCoordinate + direction1;
                // Debug.Log($"directionx2{checkTargetCoordinate}");
                if (MapCellController.instance.CheckTryMoveTarget(startCoordinate, checkTargetCoordinate, mapId))
                {
                    direction.x = direction1.x;
                    direction.y = direction1.y;
                    return true;
                }

                int2 direction3 = new int2(_direction.x < 0 ? 1 : -1, 0);
                checkTargetCoordinate = startCoordinate + direction3;
                //  Debug.Log($"directionx3{checkTargetCoordinate}");
                if (MapCellController.instance.CheckTryMoveTarget(startCoordinate, checkTargetCoordinate, mapId))
                {
                    direction.x = direction3.x;
                    direction.y = direction3.y;
                    return true;
                }
            }
        }
        else
        {
            if (absX == 0)
            {
                int2 direction1 = new int2(1, 0);
                checkTargetCoordinate = startCoordinate + direction1;
                if (MapCellController.instance.CheckTryMoveTarget(startCoordinate, checkTargetCoordinate, mapId))
                {
                    direction.x = direction1.x;
                    direction.y = direction1.y;
                    return true;
                }
                int2 direction2 = new int2(-1, 0);
                checkTargetCoordinate = startCoordinate + direction2;
                if (MapCellController.instance.CheckTryMoveTarget(startCoordinate, checkTargetCoordinate, mapId))
                {
                    direction.x = direction2.x;
                    direction.y = direction2.y;
                    return true;
                }
            }
            else
            {
                int2 direction1 = new int2(0, 1);
                checkTargetCoordinate = startCoordinate + direction1;
                if (MapCellController.instance.CheckTryMoveTarget(startCoordinate, checkTargetCoordinate, mapId))
                {
                    direction.x = direction1.x;
                    direction.y = direction1.y;
                    return true;
                }
                int2 direction2 = new int2(0, -1);
                checkTargetCoordinate = startCoordinate + direction2;
                if (MapCellController.instance.CheckTryMoveTarget(startCoordinate, checkTargetCoordinate, mapId))
                {
                    direction.x = direction2.x;
                    direction.y = direction2.y;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 角色移动方向
    /// </summary>
    /// <param name="_moveDirection"></param>
    /// <returns></returns>
    private bool CheckNowMoveTarget(out Vector2 _moveDirection)
    {
        float speed = originalMoveDirection.magnitude;
        controllerCharacter.nowSpeed = speed;
        _moveDirection = originalMoveDirection;
        if (_moveDirection == Vector2.zero)
        {
            controllerCharacter.moveDirection = _moveDirection;
            GameObjectCurveController.instance.StopObjectMove(ControllerRuntimeObj.runtimeObj.linkId);
            return true;
        }
        return CheckSmoothMove(controllerCharacter.coordinate, ref _moveDirection, controllerTransform.position,
        controllerCharacter.mapInstance, controllerCharacter.propertySpeed);
    }

    //角色移动
    private void ControllerMove(Vector2 _moveDirection)
    {
        controllerCharacter.moveDirection = _moveDirection;
        //Debug.Log($" Set_moveDirection{_moveDirection}");
        if (_moveDirection == Vector2.zero)
        {
            return;
        }
        GameObjectCurveController.instance.ObjectMove(
            controllerCharacter,
            () =>
            {
                return controllerTransform.position;
            },
            (int2 targetCoordinate, Vector2 targetPos) =>
            {
                if (controllerCharacter.canMove)
                {
                    float length = Vector2.Distance(targetPos, new Vector2(controllerTransform.position.x, controllerTransform.position.y));
                    TryTeamLeaderMove tryTeamLeaderMove = new TryTeamLeaderMove
                    {
                        characterId = controllerCharacter.instanceId,
                        length = length
                    };
                    GameActionManager.instance.QueueAction(tryTeamLeaderMove, true);
                    controllerTransform.position = new Vector3(targetPos.x, targetPos.y, controllerTransform.position.z);
                    if (controllerCharacter.coordinate.x != targetCoordinate.x ||
                    controllerCharacter.coordinate.y != targetCoordinate.y)
                    {
                        CrossMap(targetCoordinate, controllerCharacter, out int3 newMap);
                        //  Debug.Log($"targetCoordinate:{targetCoordinate}-controllerCharacter:{controllerCharacter.coordinate}");
                        TryTeamLeaderSetCoordinate tryTeamLeaderSetCoordinate = new TryTeamLeaderSetCoordinate
                        {
                            characterId = controllerCharacter.instanceId
                        };
                        GameActionManager.instance.QueueAction(tryTeamLeaderSetCoordinate, true);

                        CheckNowMoveTarget(out _moveDirection);
                        controllerCharacter.moveDirection = _moveDirection;
                    }
                }
            }, ControllerRuntimeObj.runtimeObj.linkId);
    }

    //原始输入方向参数
    private Vector2 originalMoveDirection;

    //设置可操控的角色移动方向
    public void SetControllerCharacterMoveDirection(Vector2 moveDirection)
    {
        originalMoveDirection = moveDirection;
        if (controllerTransform == null)
        {
            return;
        }

        Vector2 _moveDirection = moveDirection;
        //Debug.Log($" _moveDirection1{_moveDirection}");
        if (!CheckNowMoveTarget(out _moveDirection))
        {
            // Debug.Log($"no way!!");
            controllerCharacter.moveDirection = Vector2.zero;
            TryTeamLeaderMove tryTeamLeaderMove = new TryTeamLeaderMove
            {
                characterId = controllerCharacter.instanceId,
                length = 0
            };
            GameActionManager.instance.QueueAction(tryTeamLeaderMove, true);

            GameObjectCurveController.instance.StopObjectMove(ControllerRuntimeObj.runtimeObj.linkId);
        }
        else
        {
            ControllerMove(_moveDirection);
        }
    }

    private const float freedomMoveValue = 1.0f;

    private void FreedomMoving(Transform controllerCharacterObj)
    {
        Vector2 nowPos = controllerCharacterObj.position;
        Vector2 direcrion = controllerCharacter.moveDirection;

        Vector2 targetPos = nowPos + direcrion * controllerCharacter.propertySpeed * freedomMoveValue * Time.deltaTime;
        // Debug.Log($"targetPos：{targetPos}");
        int2 targetCoordinate = GameCommon.GetMapCoordinateInt(targetPos);
        /// Debug.Log($"targetCoordinate：{targetCoordinate}");
        int2 trueTargetCoordinate = MapCellController.instance.GetTrueFreedomTarget(controllerCharacter.coordinate, targetCoordinate, controllerCharacter.mapInstance);
        if (!trueTargetCoordinate.Equals(targetCoordinate))
        {
            if (!targetCoordinate.Equals(controllerCharacter.coordinate))
            {
                targetPos = GameCommon.GetMapPos(trueTargetCoordinate);
            }
            else
            {
                targetPos = nowPos;
            }
        }
        controllerCharacterObj.transform.position = targetPos;
        //Debug.Log($"direcrion:{direcrion},nowPos:{nowPos},targetPos：{targetPos}--targetCoordinate{targetCoordinate}-controllerCharacter.coordinate{controllerCharacter.coordinate}");
        controllerCharacter.SetCoordinate(trueTargetCoordinate);
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