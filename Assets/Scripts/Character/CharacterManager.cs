using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyGame;
using Unity.Mathematics;
using UnityEngine;

public delegate void MoveEndAction(); 
 
public struct TeamerEquipAndProperty : IReferenceData
{
    public CharacterEquipAndPropertyData[] characterEquipAndPropertyDatas;
}

public class CharacterManager : Singleton<CharacterManager>
{
    public override bool NeedUpdate => true;

    public const float moveSpeed = 15f;
    public const float updataMoveSpeed = 1f;
    public bool hideCharacter { get; private set; } 
    private MyDic<int, Character> characters = new MyDic<int, Character>();
    private Dictionary<int, int> characterDataToInstances = new Dictionary<int, int>(); 

    public Player player;
    //private Vector2 playerMoveDirction;

    public List<Character> GetAllCharacters()
    {
        return characters.GetValueList();
    }
 
    protected override void Clear()
    {
        base.Clear();
        RecycleCharacter();
        characterDataToInstances.Clear(); 
        characters.Clear();
    }
    //角色运行显示实体
    private Dictionary<Character, CharacterRuntimeObj> characterRuntionObjs = new Dictionary<Character, CharacterRuntimeObj>();
    private HashSet<Character> displayCharacters = new HashSet<Character>();

    private void RecycleCharacterObj(Character character)
    {
       // Debug.Log($"RecycleCharacterObj:{character.name}");
       if (character == controllerCharacter) CameraManager.instance.SetCameraListener(true);
       displayCharacters.Remove(character);
        GameActionManager.instance.QueueAction(new TryRecycleCharacterEmote { id = character.instanceId }, true);
        if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            characterRuntimeObj.Clear();
            characterRuntionObjs.Remove(character);
        }
        if (character is TempCharacter)
        {
            DestoryCharacter destoryCharacter = new DestoryCharacter
            {
                characterId = character.instanceId,
                isTemp = true
            };
            GameActionManager.instance.QueueAction(destoryCharacter, true);
        }

        if (WorldMapManager.instance.GetRuntimeMapItem(character.linkItem, out var linkItem))
            linkItem.linkCharacter = 0;

        FishController.instance.RecycleFisherObj(character.instanceId);
    }

    private async Task CreateCharacterObjAsync(Character character, bool controller = false)
    {
        if (hideCharacter)
            return;

        if (displayCharacters.Contains(character))
        {
            return;
        }
        displayCharacters.Add(character);
        var runtimeObj = await CreateCharacterRuntimeObj(character.dataId, character.instanceId, character.coordinate);
        CharacterRuntimeObj characterRuntimeObj = runtimeObj.obj as CharacterRuntimeObj;
        characterRuntimeObj.runtimeObj = runtimeObj;
        if (characterRuntionObjs.TryAdd(character, characterRuntimeObj))
        {
            characterRuntimeObj.SetAnimationDirection(character.moveDirection, character.direction);
            characterRuntimeObj.enabled = true;
            //Vector2 pos = GameCommon.GetMapPos(character.coordinate);
            //transform.position = pos;

            if (controller || character == controllerCharacter)
            { 
                CameraManager.instance.SetFollowTarget(characterRuntimeObj.transform);
                ControllerRuntimeObj = characterRuntimeObj;
                if (!ControllerRuntimeObj.TryGetComponent(out AudioListener audioListener))
                {
                    audioListener = ControllerRuntimeObj.gameObject.AddComponent<AudioListener>();
                }
                audioListener.enabled = true;
                CameraManager.instance.SetCameraListener(false);
                RefreshMapTempCharacter refreshMapTempCharacter = new RefreshMapTempCharacter
                {
                    characterId = character.instanceId,
                };
                GameActionManager.instance.QueueAction(refreshMapTempCharacter, true);
            }
            else if (NPCManager.instance.GetNPCFormInstance(character.instanceId, out var npc))
            {
                if (npc.startSleepHour >= 0)
                {
                    instance.RefreshSleep(character);
                }
            }

           // Debug.Log($"CreateCharacter:{character.name}");
        }
        else
        {
            displayCharacters.Remove(character);
            characterRuntimeObj.Clear();
        }
         
       
    }

    public override void Init()
    {
        base.Init(); 

        GameActionManager.instance.AddListener<SetCharacterProperty>(SetCharacterValue);
        GameActionManager.instance.AddListener<ChangeCharacterProperty>(ChangeCharacterValue);
        GameActionManager.instance.AddListener<SetCharacterCoordinate>(SetCharacterCoordinate);

        GameActionManager.instance.AddListener<CreatCharacter>(CreateCharacter);
        GameActionManager.instance.AddListener<CreatTempCharacter>(CreateTempCharacter);
        GameActionManager.instance.AddListener<DestoryCharacter>(DestroyCharacter);
         
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
        GameActionManager.instance.AddListener<RefreshCharacterPos>(RefreshCharacterPos);

        GameActionManager.instance.AddListener<VisitNPC>(VisitNPC);
        GameActionManager.instance.AddListener<DisplayCharacterItemRenderer>(DisplayCharacterItemRenderer);
        GameActionManager.instance.AddListener<ClearTempCharacter>(ClearTempCharacter);

        GameActionManager.instance.AddListener<TempCharacterTalk>(TempCharacterTalk);
        GameActionManager.instance.AddListener<SetTempCharacterTarget>(SetTempCharacterTarget);
        GameActionManager.instance.AddListener<SetCharacterStopCreate>(SetCharacterStopCreate);
        GameActionManager.instance.AddListener<ChangeMap>(ChangeMap);

        GameActionManager.instance.AddListener<SetCharacterTriggerItem>(SetCharacterTriggerItem);
    }
    void ChangeMap(ChangeMap  changeMap)
    {
        var character = controllerCharacter;
        ChangeMapAction(character, changeMap.mapValue,0);
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
            character.SetCoordinate(newCoordiante,refreshMapTemp:false);
            ReStartCharacterBehavior reStartCharacterBehavior = new ReStartCharacterBehavior
            {
                characterId = character.instanceId
            };
            GameActionManager.instance.QueueAction(reStartCharacterBehavior);
            //await RefreshNpcRuntimeObj(character);

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
                    characterRuntimeObj.SetEquipSprite(itemData.icon); 
                }
                else
                {
                    characterRuntimeObj.SetEquipSprite(null);
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
            sourceCharacter.TryMove(targetCharacter.mapInstance, targetCharacter.coordinate);
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
            //await RefreshNpcRuntimeObj(character);

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

    private  void SetCharacterRandomPos(SetCharacterRandomPos SetCharacterRandomPos)
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

             // await  RefreshNpcRuntimeObj(character);
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
    void RefreshCharacterPos(RefreshCharacterPos RefreshCharacterPos)
    {
        if (characters.TryGetValue(RefreshCharacterPos.characterId, out var character))
        {
            if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
            {
                var pos = GameCommon.GetMapPos(character.coordinate);
                if (GameDataManager.instance.GlobalData.debug)
                {
                    float dX = math.abs(characterRuntimeObj.transform.position.x - pos.x);
                    if (dX >= 1.5)
                    {
                        Debug.Log($"Waring:{character.name}--oldPos{characterRuntimeObj.transform.position}--newPos{pos}");
                    }
                }
                
                characterRuntimeObj.transform.position = pos;
                if (RefreshCharacterPos.setResult != null)
                {
                    RefreshCharacterPos.setResult(true);
                }
                character.StopMove();
            }
        }
        if (RefreshCharacterPos.setResult != null)
        {
            RefreshCharacterPos.setResult(false);
        }

    }
    private void SetCharacterTempPos(SetCharacterTempPos setCharacterTempPos)
    {
        if (characters.TryGetValue(setCharacterTempPos.characterId, out var character))
        {
            if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
            {
                if (GameDataManager.instance.GlobalData.debug)
                {
                    float dX = math.abs(characterRuntimeObj.transform.position.x - setCharacterTempPos.pos.x);
                    if (dX >= 1.5)
                    {
                        Debug.Log($"Waring:{character.name}--oldPos{characterRuntimeObj.transform.position}--newPos{setCharacterTempPos.pos}");
                    }
                }
                characterRuntimeObj.transform.position = setCharacterTempPos.pos;
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
        Character character=null;
        if (displayOrHideCharacter.characterId == 0||displayOrHideCharacter.characterId==int.MinValue)
        {
            character = controllerCharacter;
        }
        else
        {
            character = GetCharacterForDataId(displayOrHideCharacter.characterId);
            if (character == null)
            {
                characters.TryGetValue(displayOrHideCharacter.characterId, out character);
            }
        }
        
        if (character!=null&&characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            characterRuntimeObj.gameObject.SetActive(displayOrHideCharacter.display);
            if (character == controllerCharacter)
            {
                CameraManager.instance.SetCameraListener(!displayOrHideCharacter.display);
            }
        }
    }

    private async void ChangeCharacter(ChangeCharacter ChangeCharacter)
    {
        if (characters.TryGetValue(ChangeCharacter.instanceId, out var character))
        {
            if (character.dataId != ChangeCharacter.newDataId)
            {
                character.ChangeData(ChangeCharacter.newDataId);
                if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
                {
                    GameActionManager.instance.QueueAction(new TryRecycleCharacterEmote { id = character.instanceId }, true);
                    characterRuntimeObj.Clear();
                    characterRuntionObjs.Remove(character);
                    displayCharacters.Remove(character);
                   await CreateCharacterObjAsync(character);
                }
            }
        }
    }

    private async void ClearEquip(ClearEquip clearEquip)
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
               await PackageManager.instance.SetItemInPackage(new Item(itemId, 1), clearEquip.outPackageId);
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

    public SpriteResourceRenference PlayerHead => controllerCharacter.characterData.head;
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
                     //UIManager.instance.ShowGamePanel<CharacterButtonPanel>();
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
                EnvironmentManger.instance.UpDataAudio2DPolygon();
                controllerTransform = controllerRuntimeObj.transform;
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
            Animator animator = characterRuntimeObj.Animator;
            setCharacterAnimator.SetAnimator(animator);
        }
    }

    public async Task CreatePlayer(int id, string playerName,int bag, int instanceId = 0)
    {
        var playerData = await GameDataManager.instance.GetAsyncData<CharacterData>(id);
        if (instanceId == 0)
        {
            instanceId = MyInstance.instance.CharacterId;
        }
        ProfessionData professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(playerData.profession);
        player = new Player(playerData,  instanceId, playerName, professionData);
        controllerCharacter = player;
        AddCharacter(player);
        RefreshShortcut refreshShortcut = new RefreshShortcut
        {
            packageId = controllerCharacter.characterPackage
        };
        GameActionManager.instance.QueueAction(refreshShortcut); 
    }

    /// <summary>
    /// 销毁角色
    /// </summary>
    /// <param name="DestoryTempCharacter"></param>
    private void DestroyCharacter(DestoryCharacter destoryCharacter)
    {
        if (characters.TryGetValue(destoryCharacter.characterId, out var character))
        {
            RemoveCharacter(character);
        }
    }
    void DestroyCharacter(int destoryCharacter)
    {
        if (characters.TryGetValue(destoryCharacter, out var character))
        {
            RemoveCharacter(character);
        }
    }

    private void ClearTempCharacter(ClearTempCharacter clearTempCharacter)
    {
        for(int i = 0; i < characters.length; i++)
        {
            var character = characters[i];
            if(character is TempCharacter)
            {
                RemoveCharacter(character);
            }

        }
 
    }
 


    private void RemoveCharacter(Character character)
    {
        characterDataToInstances.Remove(character.dataId);

        if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            GameActionManager.instance.QueueAction(new TryRecycleCharacterEmote { id = character.instanceId }, true);
            characterRuntimeObj.Clear();
           //Debug.Log($"RecycleTempCharacterObj:{character.name}");
            //GameRuntimeObjManager.instance.RecycleRuntimeObj(characterRuntimeObj.runtimeObj); 
            characterRuntionObjs.Remove(character);
            displayCharacters.Remove(character);
        }

        character.RemoveMove();
        MapCellController.instance.RemoveCharacterCoordinate(character.ObjCoordinate, character.instanceId, character is TempCharacter);
        CharacterBehaviorManager.instance.DestroyBehavior(character.instanceId);
        characters.Remove(character.instanceId);
        
        NPCTaskScheduleManager.instance.RemoveBehavior(character.instanceId);
    }

    /// <summary>
    /// 创建消费者
    /// </summary>
    /// <param name="creatTempCharacter"></param>
    private async void CreateTempCharacter(CreatTempCharacter creatTempCharacter)
    {
        var tempCharacterData = await GameDataManager.instance.GetAsyncData<TempCharacterData>(creatTempCharacter.characterId);
        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(tempCharacterData.linkCharacterId);
        int level = TempCharacterManager.instance.level;
        var professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
        TempCharacter character = new TempCharacter(characterData, professionData, MyInstance.instance.TempUid, tempCharacterData);

        AddCharacter(character);
        character.SetObjCoordinate(creatTempCharacter.mapInstance,
            new int2(creatTempCharacter.coordinateX, creatTempCharacter.coordinateY));
        await RefreshNpcRuntimeObj(character,RefreshMapTemp:false);

        character.templevel = level; 
        if (creatTempCharacter.setValue != null)
        {
            creatTempCharacter.setValue(character.instanceId);
        }
    }
    public void AddAnimal()
    {

    }
    private async void CreateCharacter(CreatCharacter creatCharacter)
    {
        Character character;
        if (creatCharacter.isPlayer)
        {
            await CreatePlayer(creatCharacter.characterId,creatCharacter.playerName, 0);
            character = player;
        }
        else
        {
            int instanceId = creatCharacter.instanceId;
            if (instanceId == 0)
            {
                instanceId = MyInstance.instance.CharacterId; 
            }
            var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(creatCharacter.characterId);
            var professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
            character = new Character(characterData, professionData, instanceId,true);
            AddCharacter(character,creatCharacter.hideData);
        }
        if (creatCharacter.mapInstance != 0)
        {
            character.SetObjCoordinate(creatCharacter.mapInstance,
            new int2(creatCharacter.coordinateX, creatCharacter.coordinateY));
            RefreshNpcRuntimeObj(character, creatCharacter.controller);
        }
        
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

    private void AddCharacter(Character character,bool hideData=false)
    {
        if (!hideData)
        {
            if (!(character is TempCharacter))
            { 
                characterDataToInstances.Add(character.dataId, character.instanceId);
            }
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
        if (instanceId == 0)
        {
            instanceId = controllerCharacter.instanceId;
        }
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
            if(characters.TryGetValue(instance,out var character))
            {
                return character;
            } 
        }
        return null;
    }

    public Character GetCharacter(int characterId)
    {
        Character character = null;
        if (characterId == 0||characterId==1||characterId==int.MinValue)
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

    void SetCharacterStopCreate(SetCharacterStopCreate setCharacterStopCreate)
    {
        hideCharacter = setCharacterStopCreate.hide;
    }

    private async void SetCharacterCoordinate(SetCharacterCoordinate setCharacterCoordinate)
    {
        Character character = GetCharacter(setCharacterCoordinate.characterId);
       
        if (character != null)
        {
            character.RemoveMove();
            character.SetCoordinate(setCharacterCoordinate.coordinate); 
           //await RefreshNpcRuntimeObj(character);
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
        if (changeCharacterProperty.characterId == 0)
        {
            changeCharacterProperty.characterId = controllerCharacter.instanceId;
        }
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

    public bool GetCharacterCoordinate(int id, out int3 coordinate)
    {
        if (characters.TryGetValue(id, out Character character))
        {
            coordinate = character.ObjCoordinate;
            return true;
        }
        coordinate = int3.zero;
        return false;
    }

    private async Task SetPlayerPos(Character character,bool refreshDirection = false)
    {
        if (characterRuntionObjs.TryGetValue(character, out CharacterRuntimeObj characterRuntimeObj))
        {
            if (character.mapInstance != WorldMapObjManager.instance.displayMap)
            {
                GameActionManager.instance.QueueAction(new TryRecycleCharacterEmote { id = character.instanceId }, true);
                RecycleCharacterObj(character);
            }
            else
            {
                if (characterRuntimeObj)
                {
                    Vector2 pos = GameCommon.GetMapPos(character.coordinate);
                    if (GameDataManager.instance.GlobalData.debug)
                    {
                        float dX = math.abs(characterRuntimeObj.transform.position.x - pos.x);
                        if (dX >= 1.5)
                        {
                            Debug.Log($"Waring:{character.name}--oldPos{characterRuntimeObj.transform.position}--newPos{pos}");
                        }
                    }
                   
                    characterRuntimeObj.SetPosition(pos);
                    
                    if (refreshDirection)
                    {
                        characterRuntimeObj.SetAnimationDirection(character.moveDirection,character.direction); 
                    }
                    //transform.Translate(new Vector3(0, 0, -100));
                }
            }
        }
        else if (character.mapInstance == WorldMapObjManager.instance.displayMap&&!ExploreManager.instance.isExplore)
        {
          await  CreateCharacterObjAsync(character);
        }
    }

    public void SetCharacterObjPos(Character character, Vector2 pos)
    {
        if (characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
        {
            //SetCharacterAnimationSpeed(0, characterRuntimeObj);
            var transform = characterRuntimeObj.transform;
            Vector3 targetPos = new Vector3(pos.x, pos.y, transform.position.z);
           // transform.position = targetPos;
            SetCharacterAnimationSpeed(1, characterRuntimeObj);
            if (GameDataManager.instance.GlobalData.debug)
            {
                float dX = math.abs(characterRuntimeObj.transform.position.x - pos.x);
                if (dX >= 1.5)
                {
                    Debug.Log($"Waring:{character.name}--oldPos{characterRuntimeObj.transform.position}--newPos{pos}");
                }
            }
            characterRuntimeObj.SetPosition(targetPos);
           // transform.position = pos;
           
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
            GameObjectCurveController.instance.StartIEnumerator(LerpPos());
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

        if (character.moveEnumeratorId != 0)
        {
            Debug.Log($"Waring:{character.name}--noStop");
        }
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
                character.moveEnumeratorId = 0;
                character.nowSpeed = 0;
                if (runtimeObj != null)
                    SetCharacterAnimationSpeed(0, runtimeObj);
                CrossMap(targetCoordinate, character, EndAction);
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
       //Debug.Log($"next cell:{targetCoordinate}");
        bool slant = targetCoordinate.x != character.coordinate.x && targetCoordinate.y != character.coordinate.y;
        character.moveDirection = math.normalize(targetCoordinate - character.coordinate);
       //  Debug.Log($"targetCoordinate:{targetCoordinate}-character.coordinate{character.coordinate}-moveDirection: {character.moveDirection}");
        // var direction = GameCommon.GetCharacterDirect(character.objCoordinate.coordinate, targetCoordinate, character.direction);
        if (!MapCellController.instance.CheckIsWalk(targetCoordinate, character.mapInstance))
        {
            Debug.Log($"不可走！character:{character.name}--character.mapInstance:{character.mapInstance}");
            if (failedMoveAction != null)
            {
                failedMoveAction();
            }
            return;
        }
        if (character==controllerCharacter&& character.linkItem != 0)
        {
            TryRemoveLinkMapItemCharacter tryRemoveLinkMapItemCharacter = new TryRemoveLinkMapItemCharacter
            {
                linkInstanceId = character.instanceId,
                mapItemInstanceId = character.linkItem
            };
            GameActionManager.instance.QueueAction(tryRemoveLinkMapItemCharacter);            
        }
       

        Vector2Int offsetCoordinate = Vector2Int.zero;

        var lineSpeed = slant ? moveSpeed * GameCommon.slantValue : moveSpeed;
        lineSpeed *= character.propertySpeed;
        if(character.moveEnumeratorId!=0)
            Debug.Log($"Waring:{character.name}--noStop");
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
                             if (GameDataManager.instance.GlobalData.debug)
                             {
                                 float dX = math.abs(transform.position.x - pos.x);
                                 if (dX >= 2.5)
                                 {
                                     Debug.Log($"Waring:{character.name}--oldPos{transform.position}--newPos{pos}--startPos{startPos}--targetPos{targetPos}");
                                 }
                             }
                             runtimeObj.SetPosition(pos);
                             //transform.Translate(Vector3.zero);

                             //Debug.Log($"{character.name}--SetObjCoordinate0:{character.coordinate}--pos{pos}");
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
                character.moveEnumeratorId = 0;
               // Debug.Log($"pathNodes.count:{pathNodes.Count}");
                if (pathNodes.Count > 0)
                {
                    character.SetCoordinate(new int3(targetCoordinate.xy, character.mapInstance), refreshMapTemp: false);

                    CharacterMoveTarget(character, pathNodes, EndAction, changeCoordinateAction, failedMoveAction);
                }
                else
                {
                 //  Debug.Log($"character:{character.name}--tryCorssMap");
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

                    CrossMap(targetCoordinate, character, EndAction, true);
                }
                if (changeCoordinateAction != null)
                {
                    changeCoordinateAction.Invoke();
                }
            }
             );
    }

    private void SetCharacterTriggerItem(SetCharacterTriggerItem SetCharacterTriggerItem)
    {
        Character character = null;
        if (SetCharacterTriggerItem.characterId == 0)
        {
            character = controllerCharacter;
        }
        else if (characters.TryGetValue(SetCharacterTriggerItem.characterId, out character))
        {
        }

        if (character != null)
            if (WorldMapManager.instance.GetRuntimeMapItem(SetCharacterTriggerItem.mapItemEditId, out var runtimeObj))
                character.SetTriggerMapItem(runtimeObj.instanceId, runtimeObj.mapItemData.playerTriggerEvent);
    }
    public void FixedTransMap(Character character, int3 coordinate)
    {
        ChangeMapAction(character, coordinate, 0);
    }

    private async void ChangeMapAction(Character character, int3 newMap, int afterAction)
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
           
            GameTimerController.instance.DelayAction((int)(GameCommon.mapChangeLerpTime * 1000), async () =>
            {
                character.SetCoordinate(new int3(targetCoordinate, targetMap), false);
                await SetPlayerPos(character,true);
                await WorldMapObjManager.instance.DisplayMap(targetMap);
                 

                GameTimerController.instance.DelayAction((int)(GameCommon.mapChangeLerpTime * 1000), () =>
                {
                    LerpScreenCycleValue lerpScreenCycleValue = new LerpScreenCycleValue
                    {
                        cyclePos = GameCommon.GetMapPos(character.coordinate),
                        minCycleValue = 1,
                        maxCycleValue = 0,
                        lerpTime = GameCommon.mapChangeLerpTime,
                        setResult = AfterLerpScreenCycle
                    };
                    //EnvironmentManger.instance.SkyEnviromentMono.PlayWeather();
                    async void AfterLerpScreenCycle(bool value)
                    {
                        
                        if (afterAction != 0)
                        {
                            var dataAction = await GameDataManager.instance.GetAsyncData<GameActionData>(afterAction);
                            if (dataAction)
                            {
                                dataAction.Action();
                            }
                        }

                        await SetPlayerPos(character, true);
                         
                        character.canMove = true;
                    }
                    GameActionManager.instance.QueueAction(lerpScreenCycleValue, true);
                });
            });
            
        }
        else if (!(character is TempCharacter))
        {
            character.moveEnumeratorId = 0;
            character.SetCoordinate(new int3(targetCoordinate, targetMap),refreshMapTemp:false);
            await SetPlayerPos(character);
        }
    }

    public bool CrossMap(int2 targetCoordinate, Character character, MoveEndAction EndAction = null,
        bool defaultDirection = false)
    {
        // int2 offsetCoordinate = targetCoordinate - character.coordinate;
        character.SetCoordinate(new int3(targetCoordinate.xy, character.mapInstance), !character.isController,false);
        var crossed = false;
        if (!(character is TempCharacter))
        {
            crossed = MapCellController.instance.ChangeMapAction(targetCoordinate,
                defaultDirection ? Direction.Default : character.direction,
                character.mapInstance, ChangeMapAction,isPlayer:character==controllerCharacter);

            void ChangeMapAction(int3 newMap, int afterAction)
            {
                this.ChangeMapAction(character, newMap, afterAction);
            }
        }
        EndAction?.Invoke();
        return crossed;
    }

    public CharacterData GetCharacterDataFromInstance(int Id)
    {
        if (characters.TryGetValue(Id, out var character))
        {
            return character.characterData;
        }

        return null;
    }

 
  
    public async Task CreateNpc(MapNpcData mapNpcData)
    {
        if (NPCManager.instance.GetNPC(mapNpcData.dataId, out var npc))
        {
            npc.isActive = mapNpcData.beginMap > 0;
              
            if (!characters.TryGetValue(npc.characterInstance, out var character))
            { 
                var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(mapNpcData.dataId);
                var professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
                character = new Character(characterData, professionData, npc.characterInstance, true);
                characters.Add(npc.characterInstance, character); 
                characterDataToInstances[character.dataId] = character.instanceId;
            }
            character.SetCoordinate(new int3(mapNpcData.beginCoordinate, mapNpcData.beginMap));
           // await RefreshNpcRuntimeObj(character);  
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

    private async Task<RuntimeObj> CreateCharacterRuntimeObj(int characterDataId, int instacneId, int2 coordiante)
    {
        Vector3 pos = GameCommon.GetMapPos(coordiante);
        //pos.z = -100;

        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(characterDataId);
        if (characterData != null)
        {
            var runtimeObj =await GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.CHARACTER.ToString(), characterData.obj.name,
               characterData.obj, instacneId); 

            (runtimeObj.obj as CharacterRuntimeObj).SetPosition(pos);
            return runtimeObj;
        }
        return null;
    }
    public void RefreshSleep(Character character)
    {
        if (WorldMapManager.instance.GetRuntimeMapItem(character.linkItem, out var mapItem))
        {
            //character.SetCoordinate(new  int3(mapItem.coordinate.xy, mapItem.mapInstanceId)); 

            SetCharacterAnimator setCharacterAnimator = new SetCharacterAnimator
            {
                characterId = character.instanceId,
                parameter = "State",
                parameterType = ParameterType.INT,
                intValue = 1
            };
            GameActionManager.instance.QueueAction(setCharacterAnimator);

            var sleepPos = mapItem.mapItemData.offsetLinkPos+ (Vector3)mapItem.pos; 
            SetCharacterTempPos SetCharacterTempPos = new SetCharacterTempPos
            {
                characterId = character.instanceId,
                pos = sleepPos
            };
            GameActionManager.instance.QueueAction(SetCharacterTempPos);

            SetDirection setDirection = new SetDirection
            {
                directionEnum = Direction.DOWN,
                characterId = character.instanceId,
            };
            GameActionManager.instance.QueueAction(setDirection);
        }

    }
    public async Task RefreshNpcRuntimeObj(Character character, bool controller = false,bool RefreshMapTemp=true)
    {
        CharacterRuntimeObj characterRuntimeObj;

        //Debug.Log($"RefreshNpcRuntimeObj:{character.name}");

        if (characterRuntionObjs.TryGetValue(character, out characterRuntimeObj))
        {
            if (character.mapInstance != WorldMapObjManager.instance.displayMap
                || ExploreManager.instance.isExplore)
            { 
                RecycleCharacterObj(character);
                if (character == controllerCharacter)
                {
                    ControllerRuntimeObj = null;
                }
                //Debug.Log($"{character.name}--SetObjCoordinate:RecycleCharacterOb");
            }
            else 
            {
                Vector3 pos = GameCommon.GetMapPos(character.coordinate);
                var transform = characterRuntimeObj.transform;
                Vector3 oldPos = transform.position;
                pos.z = oldPos.z;
                if (GameDataManager.instance.GlobalData.debug)
                {
                    float dX = math.abs(characterRuntimeObj.transform.position.x - pos.x);
                    if (dX >= 1.5)
                    {
                        Debug.Log($"Waring:{character.name}--oldPos{characterRuntimeObj.transform.position}--newPos{pos}");
                    }
                }
                characterRuntimeObj.SetPosition(pos);
                if (controller)
                { 
                    CameraManager.instance.SetFollowTarget(transform);
                }
                if (RefreshMapTemp)
                {
                    RefreshMapTempCharacter refreshMapTempCharacter = new RefreshMapTempCharacter
                    {
                        characterId = character.instanceId,
                    };
                    GameActionManager.instance.QueueAction(refreshMapTempCharacter);
                }
               

               //Debug.Log($"{character.name}--SetObjCoordinate:{character.coordinate}--pos{pos}");
            }
        }
        else
        {
             
            if (character.mapInstance == WorldMapObjManager.instance.displayMap && !ExploreManager.instance.isExplore)
            {
                if(character is TempCharacter)
                {
                    AddCharacter(character);
                }

                await CreateCharacterObjAsync(character); 
            }
        } 
    }

    public void RecycleCharacter()
    {
        if (!SingletonType.Cleared)
        {
            foreach (var characterRuntime in characterRuntionObjs)
            {
                characterRuntime.Value.Clear(); 
                FishController.instance.RecycleFisherObj(characterRuntime.Key.instanceId);
            }
        }
        else
        {
            foreach (var characterRuntime in characterRuntionObjs)
            {
                characterRuntime.Value.Dispose();
              
            }
        }
       
        characterRuntionObjs.Clear();
        displayCharacters.Clear();
    }

    public async Task RefreshNpcRuntimeObj()
    {
        for(int i = characters.length-1; i >=0; i--)
        {
            var character = characters[i];
            if (character.mapInstance != WorldMapObjManager.instance.displayMap||ExploreManager.instance.isExplore)
            {
                if(characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
                {
                    RecycleCharacterObj(character);
                }
                if(character is TempCharacter tempCharacter)
                {
                    DestroyCharacter(character.instanceId);
                }
            }
            else if(!displayCharacters.Contains(character))
            {
                if (!characterRuntionObjs.TryGetValue(character, out var characterRuntimeObj))
                {
                    await CreateCharacterObjAsync(character);
                }
                else
                {
                    if (NPCManager.instance.GetNPCFormInstance(character.instanceId, out var npc))
                    {
                        if (npc.startSleepHour >= 0)
                        {
                            RefreshSleep(character);
                        }
                    }
                    else
                    {
                        Vector3 pos = GameCommon.GetMapPos(character.coordinate);
                        if (GameDataManager.instance.GlobalData.debug)
                        {
                            float dX = math.abs(characterRuntimeObj.transform.position.x - pos.x);
                            if (dX >= 1.5)
                            {
                                Debug.Log($"Waring:{character.name}--oldPos{characterRuntimeObj.transform.position}--newPos{pos}");
                            }
                        }
                        Transform transform = characterRuntimeObj.transform;
                        transform.localPosition = pos;
                    }
                   
                }
            } 
        }

        //CharacterManager.SetShaderPlayerPos(ControllerRuntimeObj.transform.position);
    }

    public void SetCharacterAnimationSpeed(float speed, Character character)
    {
        if (characterRuntionObjs.TryGetValue(character, out CharacterRuntimeObj characterRuntimeObj))
        {
            characterRuntimeObj.SetAnimationFloat(CharacterAnimatorParameter.Speed, speed);
        }
    }

    public void SetCharacterAnimationSpeed(float speed, CharacterRuntimeObj characterRuntimeObj)
    {
        characterRuntimeObj.SetAnimationFloat(CharacterAnimatorParameter.Speed, speed);
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
        MapCellController.instance.FindPathNodeNearest(startCoordinate, targetCoordinate,
            controllerCharacter.mapInstance, (Stack<int2> pathNodes, int map, int2 start, int2 end) =>
            {
                controllerCharacter.PlayerMove(pathNodes);
            }); 
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
      // Debug.Log($" Set_moveDirection{_moveDirection}");
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
                    ControllerRuntimeObj.SetPosition(new Vector3(targetPos.x, targetPos.y, controllerTransform.position.z)); 
                    //Debug.Log("SetShaderPlayerPos7");

                    if (controllerCharacter.coordinate.x != targetCoordinate.x ||
                    controllerCharacter.coordinate.y != targetCoordinate.y)
                    {
                        CrossMap(targetCoordinate, controllerCharacter);
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
        Character character = controllerCharacter;
        if (SetCharacterDirection.characterId != 0)
        {
            characters.TryGetValue(SetCharacterDirection.characterId, out character);
        }
        if (character!=null)
        {
            if (SetCharacterDirection.directionEnum != Direction.Default)
            {
                character.SetDirection(SetCharacterDirection.directionEnum);
            }
            else
            {
                character.moveDirection = SetCharacterDirection.direction;
            }
           
        }
    }
}