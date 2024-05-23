using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHomeEquipPanel : GamePanel<HomeEquipList>
{ 
     
    [SerializeField]
    TextMeshProUGUI ItemName;
    
    [SerializeField]
    Button ActionButton, ReturnButton,InfoButton;
    [SerializeField]
    TextMeshProUGUI ActionName;  
    [SerializeField]
    HomeEquipReference homeEquipReference;
    [SerializeField]
    Transform EquipParent;
    [SerializeField]
    ToggleGroup EquipSelectGroup; 
    [SerializeField]
    Sprite setSprite, unSetSprite;
    [SerializeField]
    Image ActionImage;
    [SerializeField]
    float infoOffsetY = 2;

    DisplayList<HomeEquipReference, HomeEquip> EquipBoxs;

    [SerializeField]
    GameEventData setEventData;
     
    HomeEquip SelectHomeEquip;
    private Vector2 defaultInfoIconSize;
    async void SelectAction()
    {
        if (SelectHomeEquip.equipDataId!= 0)
        {
            if (SelectHomeEquip.mapInstance <=0)
            {
                HomeEquipmentData homeEquipmentData=await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(SelectHomeEquip.equipDataId);
                if (homeEquipmentData.canSetMaps==null||homeEquipmentData.canSetMaps.Count==0||
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
                    setResult= UnSetHomeEquip
                };
                GameActionManager.instance.QueueAction(unSetHomeEquip, true);
            }
        }
    }
    void UnSetHomeEquip(bool value)
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
    }

    
    static HidePanels hidePanels = new HidePanels
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
    public override void InitReferenceData(HomeEquipList v)
    {
        base.InitReferenceData(v); 
        EquipBoxs.InitListData(v.homeEquips, SelectEquip, EquipSelectGroup);
        hidePanels.hide = true;
        InfoButton.transform.localScale = ActionButton.transform.localScale = Vector3.zero;
        ItemName.text = "";
        GameActionManager.instance.QueueAction(hidePanels, true);

        for(int i = 0; i < v.homeEquips.Count; i++)
        {
            var homeEquip = v.homeEquips[i];
            if(homeEquip.mapInstance== WorldMapObjManager.instance.displayMap)
            {
                ChangeMapItemObjLayer changeMapItemObjLayer = new ChangeMapItemObjLayer
                {
                    layerId = GameCommon.BlueObjLayer,
                    mapItemId = homeEquip.instanceId
                };
                GameActionManager.instance.QueueAction(changeMapItemObjLayer, true);
            }
        }
    }
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
    }  
    async void SelectEquip(HomeEquip HomeEquip, bool selected = true)
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
                ActionImage.sprite=HomeEquip.mapInstance == 0 ? setSprite : unSetSprite;
                InfoButton.transform.localScale = ActionButton.transform.localScale= Vector3.one; 
            }
        }
        else if(HomeEquip.instanceId==SelectHomeEquip.instanceId)
        {
            InfoButton.transform.localScale = ActionButton.transform.localScale = Vector3.zero;
            ItemName.text = "";
            UIManager.instance.CloseGamePanel<ItemInfoPanel>();
        }
    }
    
    
}