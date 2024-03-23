using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;

public class ManufatureManager : Singleton<ManufatureManager>
{
    private MyNativeData<Manufature> Manufatures = new MyNativeData<Manufature>();

    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<CreatManufature>(CreatManufature);
        GameActionManager.instance.AddListener<ClearManufature>(ClearManufature);
        GameActionManager.instance.AddListener<OpenFormula>(OpenFormula);
        GameActionManager.instance.AddListener<SetManufature>(SetManufature);
        Manufatures.Init(8);
    }

    protected override void Clear()
    {
        base.Clear();
        Manufatures.Dispose();
    }

    void SetManufature(SetManufature setManufature)
    {
        Manufatures.SetData(setManufature.manufature);
    }
    public Manufature GetManufature(int instanceId)
    {
        Manufatures.GetData(instanceId, out Manufature manufature);
        return manufature;
    }

    void OpenFormula(OpenFormula openFormula)
    {
        foreach(Manufature item in Manufatures)
        {
            Manufature manufature = item;
            if (manufature.formulas.TryGetValue(openFormula.formulaId,out var formula))
            {
                formula.isOpen = true;
                manufature.formulas[openFormula.formulaId] = formula;
                Manufatures.SetData(manufature);
            }
        }
    }
    void ClearManufature(ClearManufature clearManufature)
    {
        if(Manufatures.GetData(clearManufature.manufatureId,out var manufature))
        {
            manufature.ClearProduct();
            Manufatures.SetData(manufature);
            RefreshManufature refreshManufature = new RefreshManufature
            {
                manufature = manufature
            };
            GameActionManager.instance.QueueAction(refreshManufature);
        }
    }
    private async void CreatManufature(CreatManufature creatManufature)
    {
        var manufatureData = await GameDataManager.instance.GetAsyncData<ManufactureData>(creatManufature.manufatureId);

        Manufature Manufature = new Manufature
        {
            instanceId = creatManufature.instanceId,
            dataId = creatManufature.manufatureId,
            formulas = new NativeHashMap<int, Formula>(8, Allocator.Persistent),
            materials=new NativeArray<int2>(4,Allocator.Persistent),
            open = false
        };
        for (int i = 0; i < manufatureData.linkFormulas.Count; i++)
        {
            Manufature.formulas.Add(manufatureData.linkFormulas[i].x, new Formula { id = manufatureData.linkFormulas[i].x, isOpen = manufatureData.linkFormulas[i].y == 1 });
        }
        Manufatures.SetData(Manufature);
    }

    public List<Formula> GetManufatureAllFormulas(int id)
    {
        List<Formula> formulas = new List<Formula>();
        if (Manufatures.GetData(id, out var manufature))
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
        if (Manufatures.GetData(manufatureId, out var manufature))
        {
            if (manufature.formulas.TryGetValue(formulaId, out var formula))
            {
                formula.isOpen = true;
                manufature.formulas[formulaId] = formula;
            }
            Manufatures.SetData(manufature);
        }
    }
   
}

public struct Formula
{
    public int id;
    public bool isOpen;
}

public struct Manufature : INativeData, IReferenceData
{
    public int instanceId;
    public int dataId;
    public bool open;
    public NativeArray<int2> materials;
    public int3 product;
    public int waitTime;
    public int startTime;
    public int matchFormula;
    public NativeHashMap<int, Formula> formulas;
    public int Key => instanceId;




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
        formulas.Dispose();
        materials.Dispose();
    }
}