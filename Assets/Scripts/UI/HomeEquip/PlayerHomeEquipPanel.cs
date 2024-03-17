using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    DisplayList<HomeEquipReference, HomeEquip> EquipBoxs; 
     
    HomeEquip SelectHomeEquip;
    private Vector2 defaultInfoIconSize;
    async void SelectAction()
    {
        if (SelectHomeEquip.itemDataId != 0)
        {
            if (SelectHomeEquip.mapInstance == 0)
            {
                HomeEquipmentData homeEquipmentData=await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(SelectHomeEquip.equipDataId);
                if (homeEquipmentData.canSetMaps==null||homeEquipmentData.canSetMaps.Count==0||
                    homeEquipmentData.canSetMaps.Contains(CharacterManager.instance.controllerCharacter.mapInstance))
                {
                    CreatTempMapItem creatTempMapItem = new CreatTempMapItem
                    {
                        characterId = CharacterManager.instance.controllerCharacter.instanceId,
                        instanceId = homeEquipmentData.mapItemDataId
                    };
                    GameActionManager.instance.QueueAction(creatTempMapItem);
                }
            }
            else
            {

            }
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
            if (HomeEquip.itemDataId == 0)
            {
                ItemInformation.localScale = Vector3.zero; 
            }
            else
            {
                ItemInformation.localScale = Vector3.one;
                SelectHomeEquip = HomeEquip;
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(HomeEquip.itemDataId);
                ItemIcon.sprite = itemData.icon;
                ItemIcon.rectTransform.sizeDelta = GameCommon.SetImageSize(ItemIcon.sprite, defaultInfoIconSize);
                ItemIcon.enabled = true; 
                ItemName.text = $"+ {itemData.itemName} +"; 
                Info.text = itemData.info;
                ActionName.text = HomeEquip.mapInstance == 0 ? "≤º÷√" : " ’ªÿ";
            }
        }
    }
    
    
}