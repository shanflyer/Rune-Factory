using System;
using System.Collections.Generic;
using System.Threading.Tasks; 
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    private Dictionary<Type, BaseReference> gamePanels = new Dictionary<Type, BaseReference>();
    private Dictionary<Type, List<BaseReference>> mulitPanels = new Dictionary<Type, List<BaseReference>>();

    private Transform canvasParent;
    private CanvasGroup canvasGroup;

    static HashSet<Type> pluralUISet = new HashSet<Type>
    {
        { typeof(CharacterResponsePanel)},
        {typeof(ItemCostSelectPanel) },
        {typeof(CostSelectPanel) }
    };
    public static bool IsPluralUI(Type type)
    {
        return pluralUISet.Contains(type);
    }
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

    public bool filmUI { get; private set; }
    static HashSet<Type> filmHidePanel = new HashSet<Type>
    {
        typeof(MainPanel),typeof(PlayerTopPanel),typeof(ScreenControllerPanel)
    };
    public bool CheckPanelCanvas(Type type)
    {
        if(!filmUI)
        {
            return true;
        }
        if (filmHidePanel.Contains(type))
        {
            return false;
        }
        return true;
    }
    public void SetFilmUI(bool show)
    {
        filmUI = show;
        if (show)
        {
            ShowGamePanel<FilmPanel>();
            using (var e = filmHidePanel.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (gamePanels.TryGetValue(e.Current, out var baseReference))
                    {
                        baseReference.canvas.enabled = false;
                        if (baseReference.raycaster != null)
                        {
                            baseReference.raycaster.enabled = false;
                        }
                    }
                }
            }
        }
        else
        {
            CloseGamePanel<FilmPanel>();
            using(var e = filmHidePanel.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (gamePanels.TryGetValue(e.Current, out var baseReference) &&
                      baseReference.show)
                    {
                        baseReference.canvas.enabled = true;
                        if (baseReference.raycaster != null)
                        {
                            baseReference.raycaster.enabled = true;
                        }
                    }
                }
            }
        }
    }
    public override async void Init()
    {
        base.Init();
        filmUI = false;
        BaseReference.UILayer = LayerMask.NameToLayer("UI");
        BaseReference.HideLayer = LayerMask.NameToLayer("Hide");

        // canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        GameActionManager.instance.AddListener<ClosePanelAction>(ClosePanel);
        GameActionManager.instance.AddListener<OpenPanelAction>(OpenPanel);
        GameActionManager.instance.AddListener<HidePanel>(HidePanel);
        GameActionManager.instance.AddListener<HidePanels>(HidePanels);
        GameActionManager.instance.AddListener<HideAllPanel>(HideAllPanel);
        GameActionManager.instance.AddListener<HidePanelGroup>(HidePanelGroup);
        GameActionManager.instance.AddListener<SetFilmUI>(SetFilmUI);

        Selectable.setStringAction = UIAudioForTag; 

        TagAudioDataList tagAudioDataList = await GameSourceManager.instance.GetScriptableObject<TagAudioDataList>("Data/TagAudioData");
        tagUIAudioDic.Clear();
        for(int i = 0; i < tagAudioDataList.tagAudioDatas.Count; i++)
        {
            tagUIAudioDic[tagAudioDataList.tagAudioDatas[i].tag] = tagAudioDataList.tagAudioDatas[i].audioClip;
        }
    } 
    Dictionary<string, AudioClip> tagUIAudioDic = new Dictionary<string, AudioClip>();

     void SetFilmUI(SetFilmUI setFilmUI)
    {
        SetFilmUI(setFilmUI.display);
    }
    public void UIAudioForTag(string tag)
    {
        if(tagUIAudioDic.TryGetValue(tag,out var se))
        {
            AudioController.instance.PlaySE(se,Group:SEGroup.UI.ToString());
        }
    }
    public void HidePanelGroup(HidePanelGroup hidePanelGroup)
    {
        canvasGroup.alpha=hidePanelGroup.hide?0:1;
        canvasGroup.blocksRaycasts = !hidePanelGroup.hide;
    }
    public void HideAllPanel(HideAllPanel hideAllPanel)
    {
        if (hideAllPanel.hide)
        {
            foreach (var panel in gamePanels.Values)
            {
                if (panel.show&&panel.GetType()!=typeof(TalkPanel) && panel.GetType() != typeof(FilmPanel))
                {
                    if(panel.canvas)
                        panel.canvas.enabled = false;
                    if (panel.raycaster)
                        panel.raycaster.enabled = false;
                } 
            }
        }
        else
        {
            foreach (var panel in gamePanels.Values)
            {
                if (panel.show)
                {
                    if (panel.canvas)
                        panel.canvas.enabled = true;
                    if (panel.raycaster)
                        panel.raycaster.enabled = true;
                }
            }
        }
          
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
    public async Task<T> GetGamePanel<T, V>(V data, bool force = false, int layer = -1, Transform parent = null) where T : GamePanel<V> where V : IReferenceData
    {
        if (gamePanels.TryGetValue(typeof(T), out var gamePanel) && gamePanel != null)
        {
            return (T)gamePanel;
        }
        else if (force)
        {
            return await ShowGamePanel<T,V>(data,layer,parent);
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
        Debug.Log($"ShowPanel:{type}");
        return  gamePanel as T;
    }

    public async Task<T> ShowGamePanel<T, V>(V data, int layer = -1, Transform parent = null) where T : GamePanel<V> where V : IReferenceData
    {
        var type = typeof(T);
        var gamePanel = await ShowGamePanel(type, data, layer, parent);
        Debug.Log($"ShowPanel:{type}");
        return (T)gamePanel;
    }
    public  T ShowGamePanelImmediately<T, V>(V data, int layer = -1, Transform parent = null) where T : GamePanel<V> where V : IReferenceData
    {
        var type = typeof(T);
        var gamePanel =  ShowGamePanelImmediately(type, data, layer, parent);
        Debug.Log($"ShowPanelImmediately:{type}");
        return (T)gamePanel;
    }

    public bool RemoveGamePanel(Type type)
    {
        return openedPanels.Remove(type);
    }
    private GamePanel<V> ShowGamePanelImmediately<V>(Type type, V data, int layer = -1, Transform parent = null) where V : IReferenceData
    {

        if (!gamePanels.TryGetValue(type, out BaseReference panel) || panel == null)
        {
            string path = $"{DataPath.UIPath}{type}";
            var gamePanelObj =  GameSourceManager.instance.GetPrefabImmediately(path);
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

            if (!IsPluralUI(type))
            {
                if (gamePanels.TryGetValue(type, out var _panel))
                {
                    if (_panel != gamePanel)
                    {
                        _panel.Close();
                    }
                }
                gamePanels[type] = gamePanel;
            }
                 
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
    private async Task<GamePanel<V>> ShowGamePanel<V>(Type type, V data, int layer = -1, Transform parent = null) where V : IReferenceData
    {
        
        if (!gamePanels.TryGetValue(type, out BaseReference panel) || panel == null)
        {
            string path = $"{DataPath.UIPath}{type}";
            var gamePanelObj = await GameSourceManager.instance.GetPrefab(path);
            var async=GameObject.InstantiateAsync(gamePanelObj, parent == null ? canvasParent : parent);
            await async;
            if (!Application.isPlaying || SingletonType.Cleared)
            {
                GameObject.Destroy(gamePanelObj);
                return null;
            }
            var _Panel = async.Result[0];
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

            if (!IsPluralUI(type))
            {
                if (gamePanels.TryGetValue(type, out var _panel))
                {
                    if (_panel != gamePanel)
                    {
                        _panel.Close();
                    }
                }
                gamePanels[type] = gamePanel;
            }
                

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
    private void HidePanels(HidePanels hidePanel)
    {
        for(int i = 0; i < hidePanel.type.Count; i++)
        {
            var type = hidePanel.type[i];
            var gamePanel = GetGamePanel(type);
            if (gamePanel == null)
            {
                continue;
            }
            if (gamePanel.canvas)
                gamePanel.canvas.enabled = !hidePanel.hide;
            if (gamePanel.raycaster)
                gamePanel.raycaster.enabled = !hidePanel.hide;
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
            gamePanel.canvas.enabled = !hidePanel.hide;
        if (gamePanel.raycaster)
            gamePanel.raycaster.enabled = !hidePanel.hide;
    }

    private async void OpenPanel(OpenPanelAction openPanelEvent)
    {
        Debug.Log($"OpenPanelAction :{openPanelEvent.type}");
        await ShowGamePanel(openPanelEvent.type, openPanelEvent.dataId);
    }

    HashSet<Type> openedPanels = new HashSet<Type>();
    private async Task<BaseReference> ShowGamePanel(Type type, string dataKey = null, int layer = -1, Transform parent = null)
    {
        /*if (!IsPluralUI(type))
        {
            if (openedPanels.Contains(type))
            {
                return null;
            }
            else
            {
                openedPanels.Add(type);
            }
        }*/
        if (!gamePanels.TryGetValue(type, out BaseReference gamePanel) || gamePanel == null || IsPluralUI(type))
        {
            string path = $"{DataPath.UIPath}{type}";
            var gamePanelObj = await GameSourceManager.instance.GetPrefab(path); 
            var async = GameObject.InstantiateAsync(gamePanelObj, parent == null ? canvasParent : parent);
            await async;

            if (!Application.isPlaying||SingletonType.Cleared)
            {
                GameObject.Destroy(gamePanelObj);
                return null;
            }

            var _Panel = async.Result[0]; 
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
            if (!IsPluralUI(type))
            {
                if(gamePanels.TryGetValue(type,out var _panel))
                {
                    if (_panel != gamePanel)
                    {
                        _panel.Close();
                    }
                }
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
        if (!IsPluralUI(type))
        {
            openedPanels.Remove(type);
        }
        if (gamePanels.TryGetValue(type, out BaseReference gamePanel))
        {
            if (gamePanel == null)
            {
                gamePanels.Remove(type);
                return;
            }
            if (gamePanel.show)
            {
                gamePanel.Close();
            }
           
        }
    }

    private void ClosePanel(ClosePanelAction closePanelEvent)
    {
        if (gamePanels.TryGetValue(closePanelEvent.type, out BaseReference gamePanel))
        {
            if (!IsPluralUI(closePanelEvent.type))
            {
                openedPanels.Remove(closePanelEvent.type);
            }
            if (gamePanel == null)
            {
                gamePanels.Remove(closePanelEvent.type);
                return;
            }
            gamePanel.Close();
        }
    }
}