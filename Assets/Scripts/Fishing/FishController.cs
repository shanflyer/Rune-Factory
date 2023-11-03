using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FishController:Singleton<FishController>
{
    public override bool NeedUpdata => true;
    public override void Init()
    {
        base.Init();
    }
    protected override void Clear()
    {
        base.Clear();
    }
}