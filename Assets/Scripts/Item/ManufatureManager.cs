using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;

public class Formula
{
    public FormulaData formulaData;
    public ItemData product => formulaData.ProductItem;
    public bool opened;
    public int id => formulaData.id;
    public Formula(FormulaData formulaData,bool opened)
    {
        this.formulaData = formulaData;
        this.opened = opened;

    }
}
public class ManufactureManager : Singleton<ManufactureManager>
{
    private Dictionary<int,Manufature> Manufactures = new Dictionary<int, Manufature> ();
    private MyDic<int, Formula> formulas = new MyDic<int, Formula>();
    public bool GetFormula(int id,out Formula formula)
    {
       return  formulas.TryGetValue(id, out formula);
    }

    public Formula CheckFormula(List<int> items)
    {
        for(int i = 0; i < formulas.length; i++)
        {
            var formula = formulas[i];
            if (formula.formulaData.Check(items))
            {
                return formula;
            }
        }

        return null;
    }
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddAsyncListener<CreatManufature>(CreatManufatureAsync, nameof(CreatManufature));
        GameActionManager.instance.AddListener<ClearManufature>(ClearManufature);
        GameActionManager.instance.AddListener<OpenFormula>(OpenFormula);
        GameActionManager.instance.AddListener<SetManufature>(SetManufature);
        Manufactures.Clear();
        // 管理器初始化入口保持同步，配方加载异常统一进入异步日志。
        AsyncTaskRunner.Run(InitDataAsync(), nameof(InitData));
    }

    Task InitData()
    {
        return InitDataAsync();
    }

    async Task InitDataAsync()
    {
        var datas =await GameDataManager.instance.GetAllAsyncData<FormulaData>();
        HashSet<int> openFormulas = GameDataSaveManager.instance.UserGameSaveData.openFormulas.ToHashSet();

        for(int i = 0; i < datas.Count; i++)
        {
            Formula formula = new Formula(datas[i], openFormulas.Contains(datas[i].id));
            formulas.Add(datas[i].id, formula);
        }
    }
    public void InitSaveOpenFormula(List<int> datas)
    {
        for (int i = 0; i < datas.Count; i++)
        {
            if (formulas.TryGetValue(datas[i],out var formula))
            {
                formula.opened=true;
            }
        }
    }
    protected override void Clear()
    {
        base.Clear();
        foreach(var manufature in Manufactures)
        {
            manufature.Value.Dispose();
        }
        Manufactures.Clear();
    }

    void SetManufature(SetManufature setManufature)
    {
        Manufactures[setManufature.manufature.instanceId] = setManufature.manufature;
        RefreshManufature refreshManufature = new RefreshManufature
        {
            manufature = setManufature.manufature
        };
        GameActionManager.instance.QueueAction(refreshManufature, true);

        GameDataSaveManager.instance.UserGameSaveData.SetManufature(setManufature.manufature);
    }
    public Manufature GetManufature(int instanceId)
    {
        Manufactures.TryGetValue(instanceId, out Manufature manufature);
        return manufature;
    }

    void OpenFormula(OpenFormula openFormula)
    {
        if (formulas.TryGetValue(openFormula.formulaId, out var formula) )
        {
            formula.opened = true;
            GameDataSaveManager.instance.UserGameSaveData.openFormulas.Add(openFormula.formulaId);
            ItemResultInfo itemResultInfo = new ItemResultInfo
            {
                icon = formula.product.icon,
                info0 = LanguageManage.SwitchStr("新配方获得!"),
                info1 = string.Format(LanguageManage.SwitchStr("发现了制作<color=blue>{0}</color>的配方"), LanguageManage.SwitchStr(formula.formulaData.formulaName))
            };
            GameNotificationManager.instance.ShowItemResultInfo(itemResultInfo);
            //UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);
        }
    }
    public bool IsFormulaOpened(int formulaId)
    {
        if (GameDataSaveManager.instance.UserGameSaveData.openFormulas.Contains(formulaId))
        {
            return true;
        }

        return false;
    }
    void ClearManufature(ClearManufature clearManufature)
    {
        if(Manufactures.TryGetValue(clearManufature.manufatureId,out var manufature))
        {
            manufature.ClearProduct();
            RefreshManufature refreshManufature = new RefreshManufature
            {
                manufature = manufature
            };
            GameActionManager.instance.QueueAction(refreshManufature, true);

            GameDataSaveManager.instance.UserGameSaveData.SetManufature(manufature);
        }
    }

    public void CreatManufature(ManufatureSaveData manufatureSaveData)
    {
        int saveId = SaveRuntimeResolver.instance.EnsureSaveId(SaveEntityKind.Manufacture, manufatureSaveData.instanceId);
        int instanceId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.Manufacture, saveId);
        if (instanceId == 0)
        {
            instanceId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.HomeEquip, saveId);
        }
        if (instanceId == 0)
        {
            instanceId = MyInstance.instance.Uid;
        }
        SaveRuntimeResolver.instance.Bind(SaveEntityKind.Manufacture, saveId, instanceId);

        Manufature manufature = new Manufature
        {
            instanceId = instanceId,
            saveId = saveId,
            dataId = manufatureSaveData.dataId,
            waitTime = manufatureSaveData.waitTime,
            startTime = manufatureSaveData.startTime,
            product=manufatureSaveData.product,


            materials = new int2[4]
        };
        GetFormula(manufatureSaveData.matchFormula, out manufature.matchFormula);
        for (var i = 0; i < manufatureSaveData.materials.Length; i++)
            manufature.materials[i] = manufatureSaveData.materials[i];

        Manufactures[instanceId] = manufature;
    }

    private async System.Threading.Tasks.Task CreatManufatureAsync(CreatManufature creatManufature)
    {
        var manufatureData = await GameDataManager.instance.GetAsyncData<ManufactureData>(creatManufature.manufatureId);
        int instanceId = creatManufature.instanceId;
        int saveId = creatManufature.saveId;
        if (saveId == 0 && instanceId != 0)
        {
            saveId = SaveRuntimeResolver.instance.GetSaveId(SaveEntityKind.Manufacture, instanceId);
        }
        if (saveId == 0 && instanceId != 0)
        {
            saveId = SaveRuntimeResolver.instance.GetSaveId(SaveEntityKind.HomeEquip, instanceId);
        }
        saveId = SaveRuntimeResolver.instance.EnsureSaveId(SaveEntityKind.Manufacture, saveId);
        if (instanceId == 0)
        {
            instanceId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.Manufacture, saveId);
        }
        if (instanceId == 0)
        {
            instanceId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.HomeEquip, saveId);
        }
        if (instanceId == 0)
        {
            instanceId = MyInstance.instance.Uid;
        }
        SaveRuntimeResolver.instance.Bind(SaveEntityKind.Manufacture, saveId, instanceId);

        if (!Manufactures.ContainsKey(instanceId))
        {
            Manufature manufature = new Manufature
            {
                instanceId = instanceId,
                saveId = saveId,
                dataId = creatManufature.manufatureId,
                materials = new int2[4],
                open = false
            };
            Manufactures.Add(instanceId, manufature);
            for (int i = 0; i < manufatureData.linkFormulas.Count; i++)
            {

                manufature.formulas.Add(manufatureData.linkFormulas[i].x);
            }

            GameDataSaveManager.instance.UserGameSaveData.SetManufature(manufature);
        }

    }

    public List<Formula> GetManufatureAllFormulas(int id)
    {
        List<Formula> formulas = new List<Formula>();
        if (Manufactures.TryGetValue(id, out var manufature))
        {
            foreach (var f in manufature.formulas)
            {
                if(GetFormula(f,out var formula))
                {
                    formulas.Add(formula);
                }
            }
        }
        return formulas;
    }

}


public class Manufature :  IReferenceData
{
    public int instanceId;
    public int saveId;
    public int dataId;
    public bool open;
    public int2[] materials;
    public int3 product;
    public int waitTime;
    public int startTime;
    public Formula matchFormula;
    public List<int> formulas=new List<int>();


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
    }
}
