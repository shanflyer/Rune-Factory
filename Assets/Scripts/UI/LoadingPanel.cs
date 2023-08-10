using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class LoadingPanel : GamePanel
{
    [SerializeField]
    Slider slider;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        slider = FindChildGameObject<Slider>("Slider");
    }
    public override Task InitData(int dataId)
    {
        this.AsyncOperation = ExtensionsResources.LoadSceneAsync("001",UnityEngine.SceneManagement.LoadSceneMode.Additive);
        return base.InitData(dataId); 
    }
    void SetYieldInstruction(AsyncOperation AsyncOperation)
    {
        this.AsyncOperation = AsyncOperation;
    }
    AsyncOperation AsyncOperation;
    private void Update()
    {
        if (AsyncOperation == null)
        {
            return;
        }
        slider.value = AsyncOperation.progress;
        if (AsyncOperation.progress >= 1)
        {
            Close();
        }
        
    }
 
}
