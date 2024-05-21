using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHomeEquipPanel : GamePanel<HomeEquipList>
{ 
    [SerializeField]
    Transform ItemInformation;
     
    [SerializeField] 
    Image ItemIcon;
    [SerializeField]
    TextMeshProUGUI ItemName;
    
    [SerializeField]
    TextMeshProUGUI Info;
    [SerializeField]
    Button ActionButton, ReturnButton;
    [SerializeField]
    TextMeshProUGUI ActionName;  
    [SerializeField]
    HomeEquipReference homeEquipReference;
    [SerializeField]
    Transform EquipParent;
    [SerializeField]
    ToggleGroup EquipSelectGroup;
    [SerializeField]
    TextMeshProUGUI EquipType;
    [SerializeField]
    TextMeshProUGUI RoomeValue;
    [SerializeField]
    Sprite setSprite, unSetSprite;
    [SerializeField]
    Image ActionImage;

    DisplayList<HomeEquipReference, HomeEquip> EquipBoxs;

    [SerializeField]
    GameEventData setEventData;
     
    HomeEquip SelectHomeEquip;
    private Vector2 defaultInfoIconSize;
    async void SelectAction()
    {
        if (SelectHomeEquip.equipDataId!= 0)
        {
            if (SelectHomeEquip.mapInstance == 0)
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
                GameActionManager.instance.QueueAction(unSetHomeEquip);
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
        }
    }
    protected override void Awake()
    {
        base.Awake();
        defaultInfoIconSize = ItemIcon.rectTransform.sizeDelta;
        ReturnButton.onClick.AddListener(Close);
        EquipBoxs = new DisplayList<HomeEquipReference, HomeEquip>(homeEquipReference, EquipParent);
         
        ActionButton.onClick.AddListener(SelectAction); 
        ItemInformation.localScale = Vector3.zero; 
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
        ItemIcon = FindChildGameObject<Image>("ItemIcon");
        ItemName = FindChildGameObject<TextMeshProUGUI>("ItemName"); 
        Info = FindChildGameObject<TextMeshProUGUI>("Info");
        ActionButton = FindChildGameObject<Button>("ActionButton");
        ActionName = FindChildGameObject<TextMeshProUGUI>("ActionName");
        homeEquipReference = FindChildGameObject<HomeEquipReference>("HomeEquipReference");
        EquipParent = FindChildGameObject("HomeEquipParent");
        EquipSelectGroup = FindChildGameObject<ToggleGroup>("HomeEquipParent");
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        ItemInformation = FindChildGameObject("InformationObj");
        EquipType = FindChildGameObject<TextMeshProUGUI>("EquipType");
        RoomeValue = FindChildGameObject<TextMeshProUGUI>("RoomValue");
        ActionImage = ActionButton.GetComponent<Image>();
    }
    public override void InitReferenceData(HomeEquipList v)
    {
        base.InitReferenceData(v); 
        EquipBoxs.InitListData(v.homeEquips, SelectEquip, EquipSelectGroup);
          
    }
    public override void Close()
    {
        base.Close(); 
    }  
    async void SelectEquip(HomeEquip HomeEquip, bool selected = true)
    {
        if (selected)
        {
            if (HomeEquip.equipDataId == 0)
            {
                ItemInformation.localScale = Vector3.zero; 
            }
            else
            {
                ItemInformation.localScale = Vector3.one;
                SelectHomeEquip = HomeEquip;
                HomeEquipmentData homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(HomeEquip.equipDataId);
                 
                ItemIcon.sprite = homeEquipmentData.icon;
                ItemIcon.rectTransform.sizeDelta = GameCommon.SetImageSize(ItemIcon.sprite, defaultInfoIconSize);
                ItemIcon.enabled = true; 
                ItemName.text = $"+ {homeEquipmentData.equipmentName} +"; 
                Info.text = homeEquipmentData.info;
                ActionName.text = HomeEquip.mapInstance == 0 ? "布置" : "收回";
                ActionImage.sprite=HomeEquip.mapInstance == 0 ? setSprite : unSetSprite;

               
                EquipType.text = homeEquipmentData.homeEquipType.ToString();
                string roomValueText = "所有地方";
                if(homeEquipmentData.canSetMaps != null && homeEquipmentData.canSetMaps.Count > 0)
                {
                    roomValueText = "";
                    for (int i = 0; i < homeEquipmentData.canSetMaps.Count; i++)
                    {
                        int roomId = homeEquipmentData.canSetMaps[i];
                        roomValueText += WorldMapManager.instance.GerMapDataName(roomId); 
                        if (i < homeEquipmentData.canSetMaps.Count - 1)
                        {
                            roomValueText += ",";
                        }
                    }
                }
                RoomeValue.text = roomValueText;

            }
        }
        else if(HomeEquip.instanceId==SelectHomeEquip.instanceId)
        {
            ItemInformation.localScale = Vector3.zero;
        }
    }
    
    
}