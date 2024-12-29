using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics; 

public class ManufatureManager : Singleton<ManufatureManager>
{
    private Dictionary<int,Manufature> Manufatures = new Dictionary<int, Manufature> ();

    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<CreatManufature>(CreatManufature);
        GameActionManager.instance.AddListener<ClearManufature>(ClearManufature);
        GameActionManager.instance.AddListener<OpenFormula>(OpenFormula);
        GameActionManager.instance.AddListener<SetManufature>(SetManufature);
        Manufatures.Clear();
    }

    protected override void Clear()
    {
        base.Clear();
        foreach(var manufature in Manufatures)
        {
            manufature.Value.Dispose();
        }
        Manufatures.Clear();
    }

    void SetManufature(SetManufature setManufature)
    { 
        Manufatures[setManufature.manufature.instanceId] = setManufature.manufature;
        RefreshManufature refreshManufature = new RefreshManufature
        {
            manufature = setManufature.manufature
        };
        GameActionManager.instance.QueueAction(refreshManufature);

        GameDataSaveManager.instance.UserGameSaveData.SetManufature(setManufature.manufature);
    }
    public Manufature GetManufature(int instanceId)
    {
        Manufatures.TryGetValue(instanceId, out Manufature manufature);
        return manufature;
    }

    void OpenFormula(OpenFormula openFormula)
    {
        foreach(var item in Manufatures)
        {
            Manufature manufature = item.Value;
            if (manufature.formulas.TryGetValue(openFormula.formulaId,out var formula))
            {
                formula.isOpen = true;
                manufature.formulas[openFormula.formulaId] = formula; 
            }

            GameDataSaveManager.instance.UserGameSaveData.SetManufature(manufature);
        }
    }
    void ClearManufature(ClearManufature clearManufature)
    {
        if(Manufatures.TryGetValue(clearManufature.manufatureId,out var manufature))
        {
            manufature.ClearProduct(); 
            RefreshManufature refreshManufature = new RefreshManufature
            {
                manufature = manufature
            };
            GameActionManager.instance.QueueAction(refreshManufature);

            GameDataSaveManager.instance.UserGameSaveData.SetManufature(manufature);
        }
    }

    public void CreatManufature(ManufatureSaveData manufatureSaveData)
    {
        Manufature manufature = new Manufature
        {
            instanceId = manufatureSaveData.instanceId,
            dataId = manufatureSaveData.dataId,
            waitTime = manufatureSaveData.waitTime,
            startTime = manufatureSaveData.startTime,
            matchFormula = manufatureSaveData.matchFormula,
            product=manufatureSaveData.product,

            formulas = new Dictionary<int, Formula>(),
            materials = new NativeArray<int2>(4, Allocator.Persistent)
        };
        manufature.materials.CopyFrom(manufatureSaveData.materials);
        for (int i = 0; i < manufatureSaveData.formulas.Count; i++)
        {
            manufature.formulas.Add(manufatureSaveData.formulas[i].id, manufatureSaveData.formulas[i]);
        }
        Manufatures.Add(manufatureSaveData.instanceId, manufature);
    }

    private async void CreatManufature(CreatManufature creatManufature)
    {
        var manufatureData = await GameDataManager.instance.GetAsyncData<ManufactureData>(creatManufature.manufatureId);
        if (!Manufatures.ContainsKey(creatManufature.instanceId))
        {
            Manufature manufature = new Manufature
            {
                instanceId = creatManufature.instanceId,
                dataId = creatManufature.manufatureId,
                formulas = new Dictionary<int, Formula>(),
                materials = new NativeArray<int2>(4, Allocator.Persistent),
                open = false
            };
            Manufatures.Add(creatManufature.instanceId, manufature);
            for (int i = 0; i < manufatureData.linkFormulas.Count; i++)
            {
                manufature.formulas.Add(manufatureData.linkFormulas[i].x, new Formula { id = manufatureData.linkFormulas[i].x, isOpen = manufatureData.linkFormulas[i].y == 1 });
            }

            GameDataSaveManager.instance.UserGameSaveData.SetManufature(manufature);
        }
        
    }

    public List<Formula> GetManufatureAllFormulas(int id)
    {
        List<Formula> formulas = new List<Formula>();
        if (Manufatures.TryGetValue(id, out var manufature))
        {
            foreach (var f in manufature.formulas)
            {
                formulas.Add(f.Value);
            }
        }
        return formulas;
    }

    public void OpenFormula(int manufatureId, int formulaId)
    {
        if (Manufatures.TryGetValue(manufatureId, out var manufature))
        {
            if (manufature.formulas.TryGetValue(formulaId, out var formula))
            {
                formula.isOpen = true;
                manufature.formulas[formulaId] = formula;
            } 
        }
    }
   
}

public struct Formula
{
    public int id;
    public bool isOpen;
}

public class Manufature :  IReferenceData
{
    public int instanceId;
    public int dataId;
    public bool open;
    public NativeArray<int2> materials;
    public int3 product;
    public int waitTime;
    public int startTime;
    public int matchFormula;
    public Dictionary<int, Formula> formulas; 




    public override string ToString()
    {
        return instanceId.ToString();
    }
    public void ClearProduct()
    {
        waitTime = 0;
        product = 0;
        for(int i = 0; i < materials.Length; i++)
        {
            materials[i] = int2.zero;
        }
    }

    public bool Equals(IReferenceData other)
    {
        if (other is HomeEquip homeEquip)
        {
            return homeEquip.instanceId == instanceId;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return instanceId;
    }

    public void Dispose()
    {
        formulas.Clear();
        materials.Dispose();
    }
}