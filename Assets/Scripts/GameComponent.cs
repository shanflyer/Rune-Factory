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
    public GameObject SaveButtonObj;
    public LoveAction loveAction; 
    public HeritageAction heritageAction;
    public GameObject BookPanelObj;
    public CharactorTitleAction charactorTitleAction;
    public FishManager fishManager; 
    public CharactorShop charactorShop;
    public EmployerManger employerManger;
    public AdventurePanelAction adventurePanelAction; 

    public GameObject calenderPanel; 
    public OldName.GameManager gameManager;
    public FilmManager filmManager;
    public GameObject huiFuEffectPro; 
    public Transform NpcParent;  
    public InfluenceAction influenceAction;  
    public EquipmentManager equipmentManager;
    void Awake()
    {
        
    }

    
    public void DisplayPrompt(string content)
    { 
    }
  
    public void InitData()
    { 
        GameComponentData.gameData = this;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
