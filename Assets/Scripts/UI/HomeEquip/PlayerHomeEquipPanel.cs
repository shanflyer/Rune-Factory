using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHomeEquipPanel : GamePanel<HomeEquipList>
{
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

    private async void SelectAction()
    {
        if (SelectHomeEquip.equipDataId != 0)
        {
            if (SelectHomeEquip.mapInstance <= 0)
            {
              
                waiteSetHomeEquip = !waiteSetHomeEquip;
                if (waiteSetHomeEquip)
                {
                    ActionName.text = "取消";
                    var renference = EquipBoxs.GetReference(SelectHomeEquip);
                    if (renference)
                    {
                        renference.SetAnimationIcon(true);
                    }
                }
                else
                {
                    ActionName.text = "布置";
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
            }
        }
    }

    async void SetActionState()
    {
        var homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(SelectHomeEquip.equipDataId);
        if (SelectHomeEquip.mapInstance <= 0)
        {
            ActionButton.transform.localScale = homeEquipmentData.canSetMaps.Contains(WorldMapObjManager.instance.displayMap) ? Vector3.one : Vector3.zero;
        }
        else
        {
            ActionButton.transform.localScale = Vector3.one;
        }
    }
    private async void UnSetHomeEquipAsync(bool value)
    {
        if (value)
        {
            SelectHomeEquip.coordinate = int2.zero;
            SelectHomeEquip.mapInstance = -1;
            EquipBoxs.SetSelectData(SelectHomeEquip, SelectEquip, EquipSelectGroup);
            selectMapItemRuntimeObj = null;
            ActionName.text = "布置";
            ActionImage.sprite = setSprite;
            var homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(SelectHomeEquip.equipDataId);
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
        InfoButton.onClick.AddListener(() =>
        {
            ItemInfo itemInfo = new ItemInfo
            {
                itemId = SelectHomeEquip.instanceId,
                dataId = SelectHomeEquip.equipDataId,
                showClose = true,
                OffsetPos = infoOffsetY,
                otherValue = 1
            };
            UIManager.instance.ShowGamePanel<ItemInfoPanel, ItemInfo>(itemInfo);
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
        cancleSelectButton.onClick.AddListener(CancleSelect);
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
        typeof(OtherFuntionPanel),
        typeof(ItemInfoPanel),
        typeof(MainPanel),
        typeof(PermissionPanel),
        typeof(ScreenControllerPanel)
        }
    };
    void CancleSelect()
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
        cameraValue.text = $"x{(1.0f + (cameraValueIndex - 1) )}";
        SetCameraPixelValue setCameraPixelValue = new SetCameraPixelValue
        {
            pixelValue = 400 - (cameraValueIndex - 1) * 100,
        };
        GameActionManager.instance.QueueAction(setCameraPixelValue,true);
    }
    public override async void InitReferenceData(HomeEquipList v)
    {
        base.InitReferenceData(v);
        EquipBoxs.InitListData(v.homeEquips, SelectEquip, EquipSelectGroup);
        EquipBoxs.ClearSelect();
        hidePanels.hide = true;
        InfoButton.transform.localScale = ActionButton.transform.localScale = Vector3.zero;
        ItemName.text = "";
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

        cameraValue.text = "x1";
        var mapDataId = WorldMapManager.instance.GerMapDataName(WorldMapObjManager.instance.displayMap);
        mapData = await GameDataManager.instance.GetAsyncData<MapRoomData>(mapDataId);
        if (mapData.fixedCamera)
        {
            cameraValueIndex = 1;
            CameraChange.localScale = Vector3.zero;
            SetFixedCamera setFixedCamera = new SetFixedCamera
            {
                fixedCamera = true,
                fixedPos = mapData.fixedCameraPos,
            };
            GameActionManager.instance.QueueAction(setFixedCamera, true);
        }
        else
        {
            cameraValueIndex = 2;
            cameraValue.text = "x2";
            CameraChange.localScale=Vector3.one;
            SetFixedCamera setFixedCamera = new SetFixedCamera
            {
                fixedCamera = true,
                fixedPos =new Vector3(-1000,-1000,-1000),
                pixelValue = 300
            };
            GameActionManager.instance.QueueAction(setFixedCamera, true);
        }

        upState = true;
        animator.SetTrigger("UP");

        InputManager.instance.AddInputActionDelegate(MyInputNameData.Other_CameraMove, CameraMove);
        InputManager.instance.AddInputActionDelegate(MyInputNameData.Other_Pointer, MousePos);
    }

    private MapRoomData mapData;

    public override void Close()
    {
        base.Close();
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
        SetFixedCamera setFixedCamera = new SetFixedCamera
        {
            fixedCamera = mapData.fixedCamera,
            fixedPos = mapData.fixedCameraPos,
            flowCameraType = mapData.flowCameraType
        };
        GameActionManager.instance.QueueAction(setFixedCamera, true);

        InputManager.instance.RemoveInputActionDelegate(MyInputNameData.Other_Pointer, MousePos);
        InputManager.instance.RemoveInputActionDelegate(MyInputNameData.Other_CameraMove, CameraMove);
    }
    bool canMoveCamera = false;

    MapItemRuntimeObj selectMapItemRuntimeObj;
    TempMapItem TempMapItem;
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
                        dataId = SelectHomeEquip.mapItemDataId,
                        setResult = (value) =>
                        {
                            if (!value)
                            {
                                CreatControllerTempMapItem creatControllerTempMapItem = new CreatControllerTempMapItem
                                {
                                    coordinate = coordinate,
                                    dataId = SelectHomeEquip.mapItemDataId,
                                    instanceId = SelectHomeEquip.instanceId,
                                    setResult =(bool value)=>
                                    {
                                        if(value&&WorldMapObjManager.instance.GetTempRuntimeMapItemObj(SelectHomeEquip.instanceId,out var mapItemRuntimeObj))
                                        {
                                           mapItemRuntimeObj.SetLayer(GameCommon.RedObjLayer);
                                            GameTimerController.instance.DeleyActionMain(500, () =>
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
                                if(WorldMapObjManager.instance.GetRuntimeMapItemObj(SelectHomeEquip.instanceId, out var mapItemRuntimeObj))
                                {
                                    selectMapItemRuntimeObj= mapItemRuntimeObj;
                                    selectMapItemRuntimeObj.SetLayer(GameCommon.GreenObjLayer);
                                }
                                SelectHomeEquip.mapInstance=WorldMapObjManager.instance.displayMap;
                                SelectHomeEquip.coordinate=coordinate;
                                EquipBoxs.SetSelectData(SelectHomeEquip, SelectEquip, EquipSelectGroup);

                                waiteSetHomeEquip = false;
                                ActionName.text ="收回";
                                ActionImage.sprite = unSetSprite;
                            }
                        }
                    };
                    GameActionManager.instance.QueueAction(trySetMapItem, true);
                }
                else
                {
                    if (WorldMapObjManager.instance.GetClickMapItemRuntimeObj(mouseWorldPos, out var _selectMapItemRuntimeObj))
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
                            dataId = selectMapItemRuntimeObj.dataId,
                            setResult = (value) =>
                            {
                                if (!value)
                                {
                                    GameTimerController.instance.DeleyActionMain(500, () =>
                                    {
                                        int2 oldCoordinate = selectMapItemRuntimeObj.coordinate;
                                        selectMapItemRuntimeObj.SetCoordinate(coordinate);
                                        selectMapItemRuntimeObj.SetLayer(GameCommon.RedObjLayer);
                                        GameTimerController.instance.DeleyActionMain(500, () =>
                                        {
                                            selectMapItemRuntimeObj.SetCoordinate(oldCoordinate);
                                            selectMapItemRuntimeObj.SetLayer(GameCommon.GreenObjLayer);
                                        });
                                    }); 
                                }
                            }
                        };
                        GameActionManager.instance.QueueAction(trySetMapItem, true);

                        //selectMapItemRuntimeObj.SetCoordinate(coordinate);
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
    private async void SelectEquip(HomeEquip HomeEquip, bool selected = true)
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
                HomeEquipmentData homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(HomeEquip.equipDataId);
                ItemName.text = $"{homeEquipmentData.equipmentName}"; 
                ActionName.text = HomeEquip.mapInstance <= 0 ? "布置" : "收回";
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
            ItemName.text = "";
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