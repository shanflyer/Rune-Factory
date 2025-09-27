using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHomeEquipPanel : GamePanel<HomeEquipList>
{
    [SerializeField] private List<int> CanSetMaps = new();

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Button TitleButton;

    [SerializeField]
    private TextMeshProUGUI ItemName;

    [SerializeField]
    private Button ActionButton, ReturnButton, InfoButton;

    [SerializeField]
    private TextMeshProUGUI ActionName;

    [SerializeField]
    private HomeEquipReference homeEquipReference;

    [SerializeField]
    private Transform EquipParent;

    [SerializeField]
    private ToggleGroup EquipSelectGroup;

    [SerializeField]
    private Sprite setSprite, unSetSprite;

    [SerializeField]
    private Image ActionImage;
    [SerializeField]
    private Button cameraChangeButton,cancleSelectButton;
    [SerializeField]
    Transform CameraChange;
    [SerializeField]
    private TextMeshProUGUI cameraValue;
   
    [SerializeField]
    private float infoOffsetY = 2;

    private DisplayList<HomeEquipReference, HomeEquip> EquipBoxs;

    [SerializeField]
    private GameEventData setEventData;

    private HomeEquip SelectHomeEquip;
    private Vector2 defaultInfoIconSize;
    private bool waiteSetHomeEquip=false;
    private bool upState=true;

    private void SelectAction()
    {
        if (SelectHomeEquip.equipDataId != 0)
        {
            if (SelectHomeEquip.mapInstance <= 0)
            {
              
                waiteSetHomeEquip = !waiteSetHomeEquip;
                if (waiteSetHomeEquip)
                {
                    ActionName.SetSWText("取消");
                    var renference = EquipBoxs.GetReference(SelectHomeEquip);
                    if (renference)
                    {
                        renference.SetAnimationIcon(true);
                    }
                }
                else
                {
                    ActionName.SetSWText("布置");
                   var renference= EquipBoxs.GetReference(SelectHomeEquip);
                    if (renference)
                    {
                        renference.SetAnimationIcon(false);
                    }
                    SetActionState();
                }

                /* HomeEquipmentData homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(SelectHomeEquip.equipDataId);
                if (homeEquipmentData.canSetMaps == null || homeEquipmentData.canSetMaps.Count == 0 ||
                    homeEquipmentData.canSetMaps.Contains(CharacterManager.instance.controllerCharacter.mapInstance))
                {
                   

                   
                    List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>
                    {
                        new EventReferenceData
                        {
                            name="CharacterId",
                            value=CharacterManager.instance.controllerCharacter.instanceId,
                        },
                        new EventReferenceData
                        {
                            name="TargetItem",
                            value=SelectHomeEquip.instanceId,
                        },
                        new EventReferenceData
                        {
                            name="TargetValue",
                            value=homeEquipmentData.mapItemDataId,
                        },
                    };

                    GameEventManager.instance.AddGameEvent(setEventData, eventReferenceDatas);
                    // Close();
                }*/
            }
            else
            {
                UnSetHomeEquip unSetHomeEquip = new UnSetHomeEquip
                {
                    instanceId = SelectHomeEquip.instanceId,
                    setResult = UnSetHomeEquipAsync
                };
                GameActionManager.instance.QueueAction(unSetHomeEquip, true);
                moveHomeEquipItemSet.Remove(SelectHomeEquip.instanceId);
            }
        }
    }

    void SetActionState()
    {
        var homeEquipmentData = SelectHomeEquip.homeEquipmentData;
        if (SelectHomeEquip.mapInstance <= 0)
        {
            ActionButton.transform.localScale = homeEquipmentData.canSetMaps.Contains(WorldMapObjManager.instance.displayMap) ? Vector3.one : Vector3.zero;
        }
        else
        {
            ActionButton.transform.localScale = Vector3.one;
        }
    }
    private  void UnSetHomeEquipAsync(bool value)
    {
        if (value)
        {
            SelectHomeEquip.coordinate = int2.zero;
            SelectHomeEquip.mapInstance = -1;
            EquipBoxs.SetSelectData(SelectHomeEquip, SelectEquip, EquipSelectGroup);
            selectMapItemRuntimeObj = null;
            ActionName.SetSWText("布置");
            ActionImage.sprite = setSprite;
            var homeEquipmentData =SelectHomeEquip.homeEquipmentData;
            ActionButton.transform.localScale = homeEquipmentData.canSetMaps.Contains(WorldMapObjManager.instance.displayMap) ? Vector3.one : Vector3.zero;
        }
    }


    RectTransform image;
    protected override void Awake()
    {
        image = transform.GetChild(0) as RectTransform;

        base.Awake();
        ReturnButton.onClick.AddListener(Close);
        EquipBoxs = new DisplayList<HomeEquipReference, HomeEquip>(homeEquipReference, EquipParent);

        ActionButton.onClick.AddListener(SelectAction);
        InfoButton.onClick.AddListener(async () =>
        {
            ItemInfo itemInfo = new ItemInfo
            {
                item = new Item
                {
                    instanceId = SelectHomeEquip.instanceId,
                    dataId = SelectHomeEquip.equipDataId,
                    itemType = ItemType.家具,
                } ,
                showClose = true,
                OffsetPos = infoOffsetY, 
            };
           await UIManager.instance.ShowGamePanel<ItemInfoPanel, ItemInfo>(itemInfo);
        });

        TitleButton.onClick.AddListener(() =>
        {
            if (upState)
            {
                upState = false;
                animator.SetTrigger("DOWN");
            }
            else
            {
                upState = true;
                animator.SetTrigger("UP");
            }
        });
        cameraChangeButton.onClick.AddListener(ChangeCameraValue);
        cancleSelectButton.onClick.AddListener(CancelSelect);
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        ItemName = FindChildGameObject<TextMeshProUGUI>("SelectName");
        ActionButton = FindChildGameObject<Button>("ActionButton");
        ActionName = FindChildGameObject<TextMeshProUGUI>("ActionName");
        InfoButton = FindChildGameObject<Button>("InfoButton");
        homeEquipReference = FindChildGameObject<HomeEquipReference>("HomeEquipReference");
        EquipParent = FindChildGameObject("HomeEquipParent");
        EquipSelectGroup = FindChildGameObject<ToggleGroup>("HomeEquipParent");
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        ActionImage = ActionButton.GetComponent<Image>();

        cameraValue = FindChildGameObject<TextMeshProUGUI>("CameraValue");

        animator = GetComponent<Animator>();
        TitleButton = FindChildGameObject<Button>("DisplayButton");
        cameraChangeButton = FindChildGameObject<Button>("CameraChangeButton"); 
        CameraChange = FindChildGameObject("CameraChange");

        cancleSelectButton=FindChildGameObject<Button>("CancleSelectButton");
    }

    private static HidePanels hidePanels = new HidePanels
    {
        type = new List<Type>
        {
        typeof(ShortcutPanel),
        typeof(OperateButtonPanel),
        typeof(CharacterButtonPanel),
        typeof(OtherFuntionPanel),
        typeof(ItemInfoPanel),
        typeof(MainPanel),
        typeof(PermissionPanel),
        typeof(ScreenControllerPanel)
        }
    };
    void CancelSelect()
    {
        if (selectMapItemRuntimeObj != null)
        {
            selectMapItemRuntimeObj.SetLayer(GameCommon.BlueObjLayer);
            selectMapItemRuntimeObj = null;
            canMoveCamera = true;
        }
    }
    int cameraValueIndex = 1;
    private void ChangeCameraValue()
    {
        cameraValueIndex++;
        if (cameraValueIndex > 3)
            cameraValueIndex = 1;
        cameraValue.SetSWText($"x{(1.0f + (cameraValueIndex - 1))}");
        SetCameraPixelValue setCameraPixelValue = new SetCameraPixelValue
        {
            pixelValue = 400 - (cameraValueIndex - 1) * 100,
        };
        GameActionManager.instance.QueueAction(setCameraPixelValue,true);
    }
    public override void InitReferenceData(HomeEquipList v)
    {
        base.InitReferenceData(v);
        moveHomeEquipItemSet.Clear();

        EquipBoxs.InitListData(v.homeEquips, SelectEquip, EquipSelectGroup);
        EquipBoxs.ClearSelect();
        hidePanels.hide = true;
        InfoButton.transform.localScale = ActionButton.transform.localScale = Vector3.zero;
        ItemName.SetSWText("");
        GameActionManager.instance.QueueAction(hidePanels, true);
        waiteSetHomeEquip = false;

        for (int i = 0; i < v.homeEquips.Count; i++)
        {
            var homeEquip = v.homeEquips[i];
            if (homeEquip.mapInstance == WorldMapObjManager.instance.displayMap)
            {
                ChangeMapItemObjLayer changeMapItemObjLayer = new ChangeMapItemObjLayer
                {
                    layerId = GameCommon.BlueObjLayer,
                    mapItemId = homeEquip.instanceId
                };
                GameActionManager.instance.QueueAction(changeMapItemObjLayer, true);
            }
        }

        cameraValue.SetSWText("x1");

        if (CanSetMaps.Contains(WorldMapObjManager.instance.displayMap))
        {
            mapData = WorldMapObjManager.instance.DisplayMapRoomData;
            if (mapData.fixedCamera)
            {
                cameraValueIndex = 1;
                CameraChange.localScale = Vector3.zero;
                var setFixedCamera = new SetFixedCamera
                {
                    fixedCamera = true,
                    fixedPos = mapData.fixedCameraPos
                };
                GameActionManager.instance.QueueAction(setFixedCamera, true);
            }
            else
            {
                cameraValueIndex = 2;
                cameraValue.SetSWText("x2");
                CameraChange.localScale = Vector3.one;
                var setFixedCamera = new SetFixedCamera
                {
                    fixedCamera = true,
                    fixedPos = new Vector3(-1000, -1000, -1000),
                    pixelValue = 300
                };
                GameActionManager.instance.QueueAction(setFixedCamera, true);
            }

            TitleButton.interactable = true;
            InputManager.instance.AddInputActionDelegate(MyInputNameData.Other_Pointer, MousePos);
        }
        else
        {
            TitleButton.interactable = false;
        }
        

        upState = true;
        animator.SetTrigger("UP");

        // InputManager.instance.AddInputActionDelegate(MyInputNameData.Other_CameraMove, CameraMove);
     
    }

    private MapRoomData mapData;

    public override void Close()
    {
        base.Close(); 
        if (SingletonType.Cleared)
        {
            return;
        }
        hidePanels.hide = false;
        GameActionManager.instance.QueueAction(hidePanels, true);
        for (int i = 0; i < data.homeEquips.Count; i++)
        {
            var homeEquip = data.homeEquips[i];
            if (homeEquip.mapInstance == WorldMapObjManager.instance.displayMap)
            {
                ChangeMapItemObjLayer changeMapItemObjLayer = new ChangeMapItemObjLayer
                {
                    layerId = 0,
                    mapItemId = homeEquip.instanceId
                };
                GameActionManager.instance.QueueAction(changeMapItemObjLayer, true);
            }
        }

        if (mapData != null)
        {
            var setFixedCamera = new SetFixedCamera
            {
                fixedCamera = mapData.fixedCamera,
                fixedPos = mapData.fixedCameraPos,
                flowCameraType = mapData.flowCameraType
            };
            GameActionManager.instance.QueueAction(setFixedCamera, true);
            InputManager.instance.RemoveInputActionDelegate(MyInputNameData.Other_Pointer, MousePos);
        }
        //InputManager.instance.RemoveInputActionDelegate(MyInputNameData.Other_CameraMove, CameraMove);
    }
    bool canMoveCamera = false;

    MapItemRuntimeObj selectMapItemRuntimeObj
    {
        get
        {
            return _selectMapItemRuntimeObj;
        }
        set
        {
            cancleSelectButton.interactable = value != null;
            _selectMapItemRuntimeObj = value;
        }
    }
    MapItemRuntimeObj _selectMapItemRuntimeObj;
    TempMapItem TempMapItem;

    Dictionary<int,int2> moveHomeEquipItemSet = new Dictionary<int, int2>();
    void MousePos(object obj)
    {
        if (obj != null)
        {
            var mousePos = (Vector2)obj;
           // canMoveCamera = false;
            CameraManager.ScreenPointToUILocalPoint(image, mousePos, out var localPos);

            if (localPos.y > image.rect.min.y & localPos.y < image.rect.max.y)
            {
              
            }
            else
            { 
                Vector2 mouseWorldPos = CameraManager.ScreenPointToWorldPoint(mousePos, 0); 
                if (waiteSetHomeEquip)
                {
                    int2 coordinate = GameCommon.GetMapCoordinateInt(mouseWorldPos);
                    TrySetMapItem trySetMapItem = new TrySetMapItem
                    {
                        coordinate = coordinate,
                        mapInstance = WorldMapObjManager.instance.displayMap,
                        mapItemInstanceId = SelectHomeEquip.instanceId,
                        dataId = SelectHomeEquip.homeEquipmentData.mapItemDataId,
                        setResult = (value) =>
                        {
                            if (!value)
                            {
                                CreatControllerTempMapItem creatControllerTempMapItem = new CreatControllerTempMapItem
                                {
                                    coordinate = coordinate,
                                    dataId = SelectHomeEquip.homeEquipmentData.mapItemDataId,
                                    instanceId = SelectHomeEquip.instanceId,
                                    setResult =(bool value)=>
                                    {
                                        if(value&&WorldMapObjManager.instance.GetTempRuntimeMapItemObj(SelectHomeEquip.instanceId,out var mapItemRuntimeObj))
                                        {
                                           mapItemRuntimeObj.SetLayer(GameCommon.RedObjLayer);
                                            GameTimerController.instance.DelayAction(500, () =>
                                            {
                                                DestoryTempMapItem destoryTempMapItem = new DestoryTempMapItem
                                                {
                                                    instanceId = SelectHomeEquip.instanceId,
                                                };
                                                GameActionManager.instance.QueueAction(destoryTempMapItem, true);
                                            });
                                        } 
                                    }
                                };

                            }
                            else
                            {
                                GameTimerController.instance.DelayAction(500, () =>
                                {
                                    if (WorldMapObjManager.instance.GetRuntimeMapItemObj(SelectHomeEquip.instanceId, out var mapItemRuntimeObj))
                                    {
                                        selectMapItemRuntimeObj = mapItemRuntimeObj;
                                        selectMapItemRuntimeObj.SetLayer(GameCommon.GreenObjLayer);
                                    }
                                });
                               
                                SelectHomeEquip.mapInstance=WorldMapObjManager.instance.displayMap;
                                SelectHomeEquip.coordinate=coordinate;
                                EquipBoxs.SetSelectData(SelectHomeEquip, SelectEquip, EquipSelectGroup);

                                waiteSetHomeEquip = false;
                                ActionName.SetSWText("收回");
                                ActionImage.sprite = unSetSprite;
                            }
                        },
                        setValue=(int instance) =>
                        {
                            SetItemAnimation setItemAnimation = new SetItemAnimation
                            {
                                id = instance,
                                keyX = 0,
                                keyY = 0
                            };
                            GameActionManager.instance.QueueAction(setItemAnimation);
                            if (SelectHomeEquip.mapItemInstance == 0)
                            {
                                SelectHomeEquip.mapItemInstance = instance;
                            }
                        }
                        
                    };
                    GameActionManager.instance.QueueAction(trySetMapItem, true);
                }
                else
                {
                    if (WorldMapObjManager.instance.GetClickMapItemRuntimeObj(mouseWorldPos,
                            out var _selectMapItemRuntimeObj) &&
                        HomeEquipManager.instance.GetHomeEquip(_selectMapItemRuntimeObj.instanceId, out var equip))
                    {
                        if (_selectMapItemRuntimeObj != selectMapItemRuntimeObj)
                        {
                            if (selectMapItemRuntimeObj != null)
                            {
                                selectMapItemRuntimeObj.SetLayer(GameCommon.BlueObjLayer);
                            }
                            selectMapItemRuntimeObj = _selectMapItemRuntimeObj;
                            selectMapItemRuntimeObj.SetLayer(GameCommon.GreenObjLayer);
                        }
                    }
                    else if (selectMapItemRuntimeObj != null)
                    { 
                        int2 coordinate = GameCommon.GetMapCoordinateInt(mouseWorldPos);
                        TrySetMapItem trySetMapItem = new TrySetMapItem
                        {
                            coordinate = coordinate,
                            mapInstance = WorldMapObjManager.instance.displayMap,
                            mapItemInstanceId = selectMapItemRuntimeObj.instanceId,
                            dataId = selectMapItemRuntimeObj.mapItemData.id,
                            setResult = (value) =>
                            {
                                if (!value)
                                {
                                    int2 oldCoordinate = selectMapItemRuntimeObj.coordinate;
                                    Vector3 pos = GameCommon.GetMapPos(coordinate);
                                    selectMapItemRuntimeObj.transform.position = pos;
                                   // selectMapItemRuntimeObj.SetCoordinate(coordinate);
                                    selectMapItemRuntimeObj.SetLayer(GameCommon.RedObjLayer);
                                    GameTimerController.instance.DelayAction(200, () =>
                                    {
                                        selectMapItemRuntimeObj.SetCoordinate(oldCoordinate);
                                        selectMapItemRuntimeObj.SetLayer(GameCommon.GreenObjLayer);
                                    });
                                }
                            },
                            setValue = (int instance) =>
                            {
                                if (SelectHomeEquip.mapItemInstance != 0)
                                {
                                    SelectHomeEquip.mapItemInstance = instance;
                                }
                            }
                        };
                        GameActionManager.instance.QueueAction(trySetMapItem, true);

                        //moveHomeEquipItemSet[selectMapItemRuntimeObj.instanceId]=coordinate;
                        //selectMapItemRuntimeObj.SetCoordinate(coordinate);
                       // selectMapItemRuntimeObj.SetLayer(GameCommon.GreenObjLayer);
                    }
                    canMoveCamera = selectMapItemRuntimeObj == null;
                }  
            }
        }
    }
    void CameraMove(object obj)
    {
        if (obj != null)
        {
            var moveDelta = (Vector2)obj;
            var movePos = moveDelta/200.0f;
            //Debug.Log($"movePos:{movePos}--moveDelta:{moveDelta}");
            if (canMoveCamera)
            {
                //  Debug.Log($"movePos:{movePos}--moveDelta:{moveDelta}");
                CameraManager.instance.MoveFixedCamera(-moveDelta*0.01f);
            }
            
        }
    }
    private  void SelectEquip(HomeEquip HomeEquip, int index, bool selected = true)
    {
        if (selected)
        {
            if (HomeEquip.equipDataId == 0)
            {
                UIManager.instance.CloseGamePanel<ItemInfoPanel>();
            }
            else
            {
                waiteSetHomeEquip = false;
                SelectHomeEquip = HomeEquip;
                HomeEquipmentData homeEquipmentData = HomeEquip.homeEquipmentData;
                ItemName.SetSWText(homeEquipmentData.equipmentName); 
                ActionName.SetSWText(HomeEquip.mapInstance <= 0 ? "布置" : "收回");
                ActionImage.sprite = HomeEquip.mapInstance <= 0 ? setSprite : unSetSprite;
                InfoButton.transform.localScale =  Vector3.one;

                if (HomeEquip.mapInstance == WorldMapObjManager.instance.displayMap)
                {
                    ChangeMapItemObjLayer changeMapItemObjLayer = new ChangeMapItemObjLayer
                    {
                        layerId = GameCommon.GreenObjLayer,
                        mapItemId = HomeEquip.instanceId
                    };
                    GameActionManager.instance.QueueAction(changeMapItemObjLayer, true);
                }
                if(selectMapItemRuntimeObj != null&&selectMapItemRuntimeObj.instanceId!=HomeEquip.instanceId)
                {
                    selectMapItemRuntimeObj.SetLayer(GameCommon.BlueObjLayer);
                    selectMapItemRuntimeObj = null;
                }
                SetActionState(); 
            }
        }
        else if (HomeEquip.instanceId == SelectHomeEquip.instanceId)
        {
            InfoButton.transform.localScale = ActionButton.transform.localScale = Vector3.zero;
            ItemName.SetSWText("");
            UIManager.instance.CloseGamePanel<ItemInfoPanel>();

            if (HomeEquip.mapInstance == WorldMapObjManager.instance.displayMap)
            {
                ChangeMapItemObjLayer changeMapItemObjLayer = new ChangeMapItemObjLayer
                {
                    layerId = GameCommon.BlueObjLayer,
                    mapItemId = HomeEquip.instanceId
                };
                GameActionManager.instance.QueueAction(changeMapItemObjLayer, true);
            }
        }
    }
}