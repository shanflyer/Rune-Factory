using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FunctionsPanel :GamePanel
{
    [SerializeField]
    Transform funcParent;
    [SerializeField]
    FuncReference funcReference;
    [SerializeField]
    Button secondSelectButton;

    
    protected override void Awake()
    {
        base.Awake();
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        funcParent = FindChildGameObject("Content");
        funcReference = FindChildGameObject<FuncReference>("FuncReference");

        secondSelectButton = FindChildGameObject<Button>("SecondSelectButton");
    }

    public override async Task InitData(string dataKay)
    {
        var funcDatas =await GameDataManager.instance.GetAllAsyncData<FunctionData>();
        
        for(int i = 0; i < funcDatas.Count;i++)
        {
            FuncReference funcReference = Instantiate(this.funcReference, funcParent);
            funcReference.InitFunction(funcDatas[i], secondSelectButton);
        }
    }
    public override void Close()
    {
        base.Close();
    }
     
}
