using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AnimalData : ScriptableObject, IGameData
{ 
    public string animalName;
    public int id;
    public int goodId;
    public int linkCharacter;
    public int age;
    public int produceCycle;
    public int product;
    public int cycleStage;
    public List<GrowthStage> growthStages = new List<GrowthStage>();
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
    public void SetReferenceData()
    {
    }
}