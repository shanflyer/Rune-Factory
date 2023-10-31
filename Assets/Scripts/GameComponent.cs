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
    public LoveAction loveAction; 
    public HeritageAction heritageAction;
    public GameObject BookPanelObj;
    public CharactorTitleAction charactorTitleAction;

    public OldName.GameManager gameManager;
    public GameObject huiFuEffectPro;   
    public EquipmentManager equipmentManager;
    void Awake()
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
