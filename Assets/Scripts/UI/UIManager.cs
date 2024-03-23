using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    private Dictionary<Type, BaseReference> gamePanels = new Dictionary<Type, BaseReference>();
    private Dictionary<Type, List<BaseReference>> mulitPanels = new Dictionary<Type, List<BaseReference>>();

    private Transform canvasParent;
    private CanvasGroup canvasGroup;

    //private Canvas canvas;
    public void InitClosePanelParent(BaseReference baseReference)
    {
        baseReference.transform.parent = canvasParent;
    }

    public void SetParent(Transform parent)
    {
        canvasParent = parent;
        canvasGroup = parent.GetComponent<CanvasGroup>();
    }

    public override void Init()
    {
        base.Init();

        BaseReference.UILayer = LayerMask.NameToLayer("UI");
        BaseReference.HideLayer = LayerMask.NameToLayer("Hide");

        // canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        GameActionManager.instance.AddListener<ClosePanelAction>(ClosePanel);
        GameActionManager.instance.AddListener<OpenPanelAction>(OpenPanel);
        GameActionManager.instance.AddListener<HidePanel>(HidePanel);
    }

    public bool GamePanelIsShow<T>() where T : BaseReference
    {
        if (gamePanels.TryGetValue(typeof(T), out var gamePanel) && gamePanel != null)
        {
            return gamePanel.enabled;
        }
        return false;
    }

    public async Task<T> GetGamePanel<T>(bool force = false) where T : BaseReference
    {
        if (gamePanels.TryGetValue(typeof(T), out var gamePanel) && gamePanel != null)
        {
            return (T)gamePanel;
        }
        else if (force)
        {
            return await ShowGamePanel<T>();
        }
        return null;
    }

    private BaseReference GetGamePanel(Type t)
    {
        if (gamePanels.TryGetValue(t, out var gamePanel) && gamePanel != null)
        {
            return gamePanel;
        }

        return null;
    }

    public async Task<T> ShowGamePanel<T>(string dataKey = null, int layer = -1, Transform parent = null) where T : BaseReference
    {
        var type = typeof(T);
        var gamePanel = await ShowGamePanel(type, dataKey, layer, parent);
        return (T)gamePanel;
    }

    public async Task<T> ShowGamePanel<T, V>(V data, int layer = -1, Transform parent = null) where T : GamePanel<V> where V : IReferenceData
    {
        var type = typeof(T);
        var gamePanel = await ShowGamePanel(type, data, layer, parent);
        return (T)gamePanel;
    }

    private async Task<GamePanel<V>> ShowGamePanel<V>(Type type, V data, int layer = -1, Transform parent = null) where V : IReferenceData
    {
        if (!gamePanels.TryGetValue(type, out BaseReference panel) || panel == null)
        {
            string path = $"{DataPath.UIPath}{type}";
            var gamePanelObj = await GameSourceManager.instance.GetPrefab(path);
            var _Panel = GameObject.Instantiate(gamePanelObj, parent == null ? canvasParent : parent);
            var gamePanelComponent = _Panel.GetComponent(type);
            _Panel.transform.localPosition = Vector3.zero;
            GamePanel<V> gamePanel;
            if (gamePanelComponent == null)
            {
                gamePanel = (GamePanel<V>)_Panel.AddComponent(type);
            }
            else
            {
                gamePanel = (GamePanel<V>)gamePanelComponent;
            }

            if (gamePanel.pluralUI == false)
                gamePanels[type] = gamePanel;

            gamePanel.Show(layer);
            gamePanel.InitReferenceData(data);
            return gamePanel;
        }
        else
        {
            if (parent != null)
            {
                panel.transform.SetParent(parent);
                panel.transform.localPosition = Vector3.zero;
            }
            var gamePanel = panel as GamePanel<V>;
            gamePanel.Show(layer);
            gamePanel.InitReferenceData(data);
            return gamePanel;
        }
    }

    private void HidePanel(HidePanel hidePanel)
    {
        var gamePanel = GetGamePanel(hidePanel.type);
        if (gamePanel == null)
        {
            return;
        }
        if (gamePanel.canvas)
        {
            gamePanel.canvas.enabled = !hidePanel.hide;
        }
    }

    private async void OpenPanel(OpenPanelAction openPanelEvent)
    {
        await ShowGamePanel(openPanelEvent.type, openPanelEvent.dataId);
    }

    private async Task<BaseReference> ShowGamePanel(Type type, string dataKey = null, int layer = -1, Transform parent = null)
    {
        if (!gamePanels.TryGetValue(type, out BaseReference gamePanel) || gamePanel == null || gamePanel.pluralUI)
        {
            string path = $"{DataPath.UIPath}{type}";
            var gamePanelObj = await GameSourceManager.instance.GetPrefab(path);
            var _Panel = GameObject.Instantiate(gamePanelObj, parent == null ? canvasParent : parent);
            _Panel.transform.localPosition = Vector3.zero;
            var gamePanelComponent = _Panel.GetComponent(type);

            if (gamePanelComponent == null)
            {
                gamePanel = (BaseReference)_Panel.AddComponent(type);
            }
            else
            {
                gamePanel = (BaseReference)gamePanelComponent;
            }
            if (gamePanel.pluralUI == false)
            {
                gamePanels[type] = gamePanel;
            }
        }
        if (parent != null)
        {
            gamePanel.transform.SetParent(parent);
            gamePanel.transform.localPosition = Vector3.zero;
        }
        gamePanel.Show(layer);
        await gamePanel.InitData(dataKey);
        return gamePanel;
    }

    public void CloseGamePanel<T>()
    {
        var type = typeof(T);
        if (gamePanels.TryGetValue(type, out BaseReference gamePanel))
        {
            if (gamePanel == null)
            {
                gamePanels.Remove(type);
                return;
            }
            gamePanel.Close();
        }
    }

    private void ClosePanel(ClosePanelAction closePanelEvent)
    {
        if (gamePanels.TryGetValue(closePanelEvent.type, out BaseReference gamePanel))
        {
            if (gamePanel == null)
            {
                gamePanels.Remove(closePanelEvent.type);
                return;
            }
            gamePanel.Close();
        }
    }
}