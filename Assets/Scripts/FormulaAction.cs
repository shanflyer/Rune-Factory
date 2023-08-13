using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

[System.Serializable]
public enum FormulaType
{
    装备=0,
    药剂=1,
    酒水=2,
    冷食=3,
    热食 = 4
}
[System.Serializable]
public class FormulasStr
{
    public string name;
    public int id;
    public FormulaType formulaType;
    public string Stuffs;
    public int Product;
    public int PowerCost;
    public int isOpen;
    public FormulasStr() { }

    public FormulasStr(Formula formula)
    {
        name = formula.name;
        id = formula.id;
        formulaType = formula.formulaType;
        Stuffs = "";
        foreach (var formulaStuff in formula.Stuffs)
        {
            Stuffs += formulaStuff + ",";
        }
        Product = formula.Product;
        PowerCost = formula.PowerCost;
        if (formula.isOpen)
        {
            isOpen = 1;
        }
        else
        {
            isOpen = 0;
        }
    }
   
}
[System.Serializable]
public class Formula
{
    public string name;
    public int id;
    public FormulaType formulaType;
    public List<int> Stuffs;
    public int Product;
    public int PowerCost;
    public bool isOpen;
    public Formula() { }

    public Formula(FormulasStr formulasStr)
    {
        name = formulasStr.name;
        id = formulasStr.id;
        formulaType = formulasStr.formulaType;
        Stuffs=new List<int>();
        var x = formulasStr.Stuffs.Split(',');
        foreach (var s in x)
        {
            Stuffs.Add(int.Parse(s));
        }
        Product = formulasStr.Product;
        PowerCost = formulasStr.PowerCost;
        if (formulasStr.isOpen == 0)
        {
            isOpen = false;
        }
        else
        {
            isOpen = true;
        }
    }
    public bool IsMatch(List<int> itemIds)
    {
        if (Stuffs.Count == itemIds.Count)
        {
            foreach (var stuff in Stuffs)
            {
                if (itemIds.Exists(i => i / 1000 == stuff / 1000))
                {
                    
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
}
public class FormulaAction : MonoBehaviour
{
    
    public List<Formula> Formulas;
    [HideInInspector]
    public List<FormulasStr> FormulasStrs;
    public GameObject MamufacturePanel;

	// Use this for initialization
	void Start () {
		
	}
    public void DataToJson()
    {
        string filePath = Application.dataPath + @"/Resources/Datas/" + "Formulas.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        FormulasStrs = new List<FormulasStr>();
        foreach (var formula in Formulas)
        {
            FormulasStr formulaStr=new FormulasStr(formula);
            FormulasStrs.Add(formulaStr);
        }
        string jsonStr = JsonMapper.ToJson(FormulasStrs);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset file = Resources.Load<TextAsset>("Datas/Formulas");
        if (file != null)
        {
            FormulasStrs = JsonMapper.ToObject<List<FormulasStr>>(file.text);

            Formulas=new List<Formula>();
            foreach (var formulasStr in FormulasStrs)
            {
               Formula formula=new Formula(formulasStr);
                formula.name = LanguageManage.SwitchStr(formula.name);
                Formulas.Add(formula);
            }
        }
        else
        {
            Debug.Log(file.name + "不存在");
        }


    }

    public void OpenFormula(int typeValue,int fId)
    {
        //临时注释
       /*
        var x = GameComponentData.gameData.itemsManager.ItemDataList.FindAll(i =>i.Type==ItemType.其他物品&&i.typeValue == typeValue
        &&i.Id!=fId);
        int index = Random.Range(0, x.Count-1);
        ItemData itemData = x[index];
        Formula formula = Formulas.Find(f => f.id == itemData.Id);
        formula.isOpen = true;
        string notice=  LanguageManage.SwitchStr("获得") + formula.name +
            LanguageManage.SwitchStr("的配方");
        if (formula.formulaType == FormulaType.装备)
        {
            notice+="  " + formula.name + LanguageManage.SwitchStr("开始在工具店出售");
        }
        GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("获得配方"), notice);*/
    }
    public void DisplayManufacturePanel(int i)
    {
        FormulaType formulaType = (FormulaType) i;
        MamufacturePanel.SetActive(true);
        MamufacturePanel.GetComponent<ManufacturingAction>().InitMarufacturingData(formulaType);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
