using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using OldName;

[System.Serializable]
public class Employer
{
    public int id;
    public string name;
    public string charactorImage, ObjName;
    public int level;
    public int profession;
    public int openBattleId;
    public int cost;
    public int skillId;
    public int weapon, clothes;
    public bool isHired;
    public AttributeType attributeType;
    public EmployType employType;
    [HideInInspector] public CharacterProperty property;

    public void AttachTeamPlayer()
    {
        
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;

        if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0 && gamePlayer.TeamPlayer0.id  == id )
        {
            isHired = true;
        }
        else if(gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0 && gamePlayer.TeamPlayer1.id == id)
        {
            isHired = true;
        }
        else
        {
            isHired = false;
        }
    }
   public void InitProperty()
   {

       AttachTeamPlayer();
       // ProfessionData professionData = CharactorDataAction.professionDatas.Find(p => p.id == profession);
       // property = professionData.ZeroProperty + professionData.GetPropertyFromLevel(level);
    }
    public Employer() { }

    
    async void InitEquip()
    {
        if (weapon != 0)
        {
            ItemData oldItemData =
                await GameDataManager.instance.GetAsyncData<ItemData>(weapon);
            property += oldItemData.property;
        }
        if (clothes != 0)
        {
            ItemData oldItemData =
                await GameDataManager.instance.GetAsyncData<ItemData>(clothes);
            property += oldItemData.property;
        }
    }
    public Employer(Animal animal)
    {
        
        //InitProperty();

    }
}
public class EmployerManger : MonoBehaviour
{
    public List<Employer> Employers;
	// Use this for initialization
	void Start ()
	{
	    //JsonToData();

	}

    void InitNpcEmployer()
    {  
    }

    public void AddAnimal(Animal animal)
    {
       
    }
    public void AnimalDead(int animalId)
    {
        Employer employer = Employers.Find(e => e.id == animalId);
        if (employer != null)
        {
            GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
            if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0 && gamePlayer.TeamPlayer0.id == employer.id)
            {
                gamePlayer.TeamPlayer0 = null;
            }
            if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0 && gamePlayer.TeamPlayer1.id == employer.id)
            {
                gamePlayer.TeamPlayer1 = null;
            }
            employer.isHired = false;
            //GameComponentData.gameData.intelligencePanelAction.InitIntelligenceData();
            Employers.Remove(employer);
        }
        
        
    }
    public void InitEmployers()
    {
        Employers = new List<Employer>();
        InitNpcEmployer();
        InitAnimalEmployer();
    }
    void InitAnimalEmployer()
    {
        List<Pasture> pastures = GameComponentData.gameData.pastureAction.Pastures;
        List<Animal> animals = new List<Animal>();
        foreach (var pasture in pastures)
        {
            if (pasture.Animals != null)
            {
                animals.AddRange(pasture.Animals);
            }

        }
        foreach (var animal in animals)
        {

            Employer employer = new Employer(animal);
            Employers.Add(employer);
        }
    }
    public void DataToJson()
    {
        string filePath = Application.dataPath + @"/Resources/Datas/" + "Employers.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        string jsonStr = JsonMapper.ToJson(Employers);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset file = Resources.Load<TextAsset>("Datas/Employers");
        if (file != null)
        {
            Employers = JsonMapper.ToObject<List<Employer>>(file.text);

            if (Application.systemLanguage == SystemLanguage.ChineseSimplified || Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseTraditional)
            {

            }
            else
            {
            }
            foreach (var employer in Employers)
            {
                employer.InitProperty();
            }
        }
        else
        {
            Debug.Log(file.name + "不存在");
        }


    }
    // Update is called once per frame
    void Update () {
		
	}
}
