using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
 
public class UIManager:Singleton<UIManager>
{ 
    private Dictionary<Type, BaseReference> gamePanels = new Dictionary<Type, BaseReference>();
    private Transform canvasParent;
    //private Canvas canvas;
   
    public void SetParent(Transform parent) { canvasParent = parent; }
    public override void Init()
    {
        base.Init();

        BaseReference.UILayer = LayerMask.NameToLayer("UI");
        BaseReference.HideLayer = LayerMask.NameToLayer("Hide");
         
       // canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        GameActionManager.instance.AddListener<ClosePanelAction>(ClosePanel);
        GameActionManager.instance.AddListener<OpenPanelAction>(OpenPanel);
         
    }
    public bool GamePanelIsShow<T>() where T : BaseReference
    {
        if (gamePanels.TryGetValue(typeof(T), out var gamePanel))
        {
            return gamePanel.enabled;
        }
        return false;
    }
    public T GetGamePanel<T>() where T:BaseReference
    { 
        if(gamePanels.TryGetValue(typeof(T),out var gamePanel))
        {
            return (T)gamePanel;
        }
        return null;
    }
    public async Task<T> ShowGamePanel<T>(string dataKey = null,int layer=-1) where T : BaseReference
    {
        var type = typeof(T);
        var gamePanel=  await ShowGamePanel(type, dataKey, layer);
        return (T)gamePanel;
    }
    public async Task<T> ShowGamePanel<T,V>(V data, int layer = -1) where T : GamePanel<V> where V:IReferenceData
    {
        var type = typeof(T);
        var gamePanel = await ShowGamePanel(type, data, layer);
        return (T)gamePanel;
    }
    async Task<GamePanel<V>> ShowGamePanel<V>(Type type, V data, int layer = -1)where V:IReferenceData
    {
        if (!gamePanels.TryGetValue(type, out BaseReference panel))
        { 
            var gamePanel = panel as GamePanel<V>;
            string path = $"{DataPath.UIPath}{type}";
            var gamePanelObj = await GameSourceManager.instance.GetPrefab(path);
            var _Panel = GameObject.Instantiate(gamePanelObj, canvasParent);

            var gamePanelComponent = _Panel.GetComponent(type);

            if (gamePanelComponent == null)
            {
                gamePanel = (GamePanel<V>)_Panel.AddComponent(type);
            }
            else
            {
                gamePanel = (GamePanel<V>)gamePanelComponent;
            }


            gamePanels[type] = gamePanel;

            gamePanel.Show(layer);
            gamePanel.InitReferenceData(data);
            return gamePanel;
        }
        return null;
    }


    private async void OpenPanel(OpenPanelAction openPanelEvent)
    {
        await ShowGamePanel(openPanelEvent.type, openPanelEvent.dataId);
    }
    async Task<BaseReference> ShowGamePanel(Type type, string dataKey = null, int layer = -1)
    {
        if (!gamePanels.TryGetValue(type, out BaseReference gamePanel))
        {
            string path = $"{DataPath.UIPath}{type}";
            var gamePanelObj = await GameSourceManager.instance.GetPrefab(path);
            var _Panel = GameObject.Instantiate(gamePanelObj, canvasParent);

           var gamePanelComponent= _Panel.GetComponent(type);

            if(gamePanelComponent==null)
            {
                gamePanel = (BaseReference)_Panel.AddComponent(type);
            }
            else
            {
                gamePanel = (BaseReference)gamePanelComponent;
            }
            

            gamePanels[type] = gamePanel;
        }
        gamePanel.Show(layer); 
        await gamePanel.InitData(dataKey);
        return gamePanel;
    }

    public void CloseGamePanel<T>()
    {
        var type = typeof(T);
        if(gamePanels.TryGetValue(type,out BaseReference gamePanel))
        {
            gamePanel.Close();
        }
    }

    private void ClosePanel(ClosePanelAction closePanelEvent)
    {
        if (gamePanels.TryGetValue(closePanelEvent.type, out BaseReference gamePanel))
        {
            gamePanel.Close();
        }
    }

   
  
}