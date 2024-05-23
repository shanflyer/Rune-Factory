using System;
using System.Collections.Generic;
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
    private Button cameraChangeButton;
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

    private bool upState=true;

    private async void SelectAction()
    {
        if (SelectHomeEquip.equipDataId != 0)
        {
            if (SelectHomeEquip.mapInstance <= 0)
            {
                HomeEquipmentData homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(SelectHomeEquip.equipDataId);
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
                    Close();
                }
            }
            else
            {
                UnSetHomeEquip unSetHomeEquip = new UnSetHomeEquip
                {
                    instanceId = SelectHomeEquip.instanceId,
                    setResult = UnSetHomeEquip
                };
                GameActionManager.instance.QueueAction(unSetHomeEquip, true);
            }
        }
    }

    private void UnSetHomeEquip(bool value)
    {
        if (value)
        {
            SelectHomeEquip.coordinate = int2.zero;
            SelectHomeEquip.mapInstance = 0;
            EquipBoxs.SetSelectData(SelectHomeEquip, SelectEquip, EquipSelectGroup);
            ActionName.text = "布置";
            ActionImage.sprite = setSprite;
        }
    }

    protected override void Awake()
    {
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
        hidePanels.hide = true;
        InfoButton.transform.localScale = ActionButton.transform.localScale = Vector3.zero;
        ItemName.text = "";
        GameActionManager.instance.QueueAction(hidePanels, true);

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
                SelectHomeEquip = HomeEquip;
                HomeEquipmentData homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(HomeEquip.equipDataId);
                ItemName.text = $"{homeEquipmentData.equipmentName}";
                ActionName.text = HomeEquip.mapInstance == 0 ? "布置" : "收回";
                ActionImage.sprite = HomeEquip.mapInstance == 0 ? setSprite : unSetSprite;
                InfoButton.transform.localScale = ActionButton.transform.localScale = Vector3.one;
            }
        }
        else if (HomeEquip.instanceId == SelectHomeEquip.instanceId)
        {
            InfoButton.transform.localScale = ActionButton.transform.localScale = Vector3.zero;
            ItemName.text = "";
            UIManager.instance.CloseGamePanel<ItemInfoPanel>();
        }
    }
}