using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
 
public class UIManager:Singleton<UIManager>
{ 
    private Dictionary<Type, GamePanel> gamePanels = new Dictionary<Type, GamePanel>();
    private Transform canvasParent;
    //private Canvas canvas;
   
    public void SetParent(Transform parent) { canvasParent = parent; }
    public override void Init()
    {
        base.Init();

        GamePanel.UILayer = LayerMask.NameToLayer("UI");
        GamePanel.HideLayer = LayerMask.NameToLayer("Hide");
         
       // canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        GameActionManager.instance.AddListener<ClosePanelAction>(ClosePanel);
        GameActionManager.instance.AddListener<OpenPanelAction>(OpenPanel);
         
    }
    public bool GamePanelIsShow<T>() where T : GamePanel
    {
        if (gamePanels.TryGetValue(typeof(T), out var gamePanel))
        {
            return gamePanel.enabled;
        }
        return false;
    }
    public T GetGamePanel<T>() where T:GamePanel
    { 
        if(gamePanels.TryGetValue(typeof(T),out var gamePanel))
        {
            return (T)gamePanel;
        }
        return null;
    }
    public async Task<T> ShowGamePanel<T>(string dataKey = null,int layer=-1) where T : GamePanel
    {
        var type = typeof(T);
        var gamePanel=  await ShowGamePanel(type, dataKey, layer);
        return (T)gamePanel;
    }
    private async void OpenPanel(OpenPanelAction openPanelEvent)
    {
        await ShowGamePanel(openPanelEvent.type, openPanelEvent.dataId.ToString());
    }
    async Task<GamePanel> ShowGamePanel(Type type, string dataKey = null, int layer = -1)
    {
        if (!gamePanels.TryGetValue(type, out GamePanel gamePanel))
        {
            string path = $"{DataPath.UIPath}{type}";
            var gamePanelObj = await GameSourceManager.instance.GetPrefab(path);
            var _Panel = GameObject.Instantiate(gamePanelObj, canvasParent);

           var gamePanelComponent= _Panel.GetComponent(type);

            if(gamePanelComponent==null)
            {
                gamePanel = (GamePanel)_Panel.AddComponent(type);
            }
            else
            {
                gamePanel = (GamePanel)gamePanelComponent;
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
        if(gamePanels.TryGetValue(type,out GamePanel gamePanel))
        {
            gamePanel.Close();
        }
    }

    private void ClosePanel(ClosePanelAction closePanelEvent)
    {
        if (gamePanels.TryGetValue(closePanelEvent.type, out GamePanel gamePanel))
        {
            gamePanel.Close();
        }
    }

   
  
}