using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OldName;
public static class GameComponentData
{
    public static GameComponent gameData;
}
public class GameComponent : MonoBehaviour
{
    public GuideController guideController;
    public GameDebugAction gameDebugAction;
    public EventManager eventManager;
    public GameObject sleepObj;
    public GameObject SaveButtonObj;
    public LoveAction loveAction;
    public NPCFunctionPanel npcFunctionPanel;
    public SalesetPanelAction salesetPanelAction;
    public LianjinAction lianjinAction;
    public GameObject InputObj;
    public CoinAction coinAction;
    public GameObject informationObj;
    public HeritageAction heritageAction;
    public GameObject BookPanelObj;
    public CharactorTitleAction charactorTitleAction;
    public FishManager fishManager;
    public NPCListDataPanelAction NpcListDataPanelAction;
    public NPCManager NpcManager;
    public IntelligencePanelAction intelligencePanelAction;
    public CharactorShop charactorShop;
    public EmployerManger employerManger;
    public AdventurePanelAction adventurePanelAction; 
    public GameObject GroundItemPro;  
    public ShopManager shopManager;
    public GameObject calenderPanel; 
    public OldName.GameManager gameManager;
    public FilmManager filmManager;
    public GameTimeManager gameTimeManager;
    public FestivalManager festivalManager;  
    public GameObject warehouseObj;
    public GameObject boxSelectFunctionObj; 
    public GameObject huiFuEffectPro;
    public Transform mapParent;
    public Transform PlantParent;
    public Transform NpcParent;
    public MapEditAction mapEditAction;
    public PeopleAcion peopleAction;
    public ShopGoldDeskAction shopGoldDeskAction;
    public CharactorDataAction charactorDataAction; 
    public PassDataManager passDataManager;
    public InfluenceAction influenceAction;
    public FarmAction farmAction;
    public PlantAction plantAction;
    public PastureAction pastureAction;
    public PastureItemPanelAction pastureItemPanelAction;
    public PasturePanelAction pasturePanelAction;
    public AnimalSetPanelAction animalSetPanelAction;
    public ShopPanelAction shopPanelAction;
    public GameObject  PromptObj; 
    public EquipmentManager equipmentManager;
    public static List<Sprite> charactorIcon;
    public static List<Sprite> headIcons;
    public static List<Sprite> PlantSprites;
    public static List<GameObject> models;
    public static List<GameObject> Effects;
    public static List<GameObject> monsterobjs;
    public static int ActionObjCode;
    void Awake()
    {
        
    }

    public void DisplayWaitPanelData(WaitType _waitType, string _text)
    {
        sleepObj.SetActive(true);
        sleepObj.GetComponent<WaitPanelAction>().InitData(_waitType,_text);
    }
    public void DisplayPrompt(string content)
    {
        PromptObj.SetActive(true);
        PromptObj.GetComponent<PromptAction>().AddInformation(content);
    }
  
    public void InitData()
    { 
        charactorIcon = Resources.LoadAll<Sprite>("Charactor/Image/").ToList();
        models = Resources.LoadAll<GameObject>("Charactor/").ToList();
        headIcons = Resources.LoadAll<Sprite>("Charactor/Head").ToList();
        PlantSprites = Resources.LoadAll<Sprite>("Plant/").ToList();
        Effects = Resources.LoadAll<GameObject>("effect/").ToList();
        monsterobjs = Resources.LoadAll<GameObject>("Monster/").ToList();
        GameComponentData.gameData = this;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
