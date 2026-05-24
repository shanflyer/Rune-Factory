using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public enum GamePanelLifecycleState
    {
        Closed,
        Loading,
        Opened,
        Closing,
        Destroyed
    }

    private readonly Dictionary<Type, BaseReference> gamePanels = new(); 
    private readonly Dictionary<Type, GamePanelLifecycleState> panelStates = new();
    private readonly Dictionary<Type, int> panelVersions = new();
    private readonly Dictionary<Type, CancellationTokenSource> panelCancellationSources = new();

    private Transform canvasParent;
    private CanvasGroup canvasGroup;
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;
    public override IReadOnlyList<Type> InitializationDependencies => new[] { typeof(GameSourceManager) };

    public Color JoyStickColor=>joyStickColor;
    private  Color joyStickColor=new Color(0.03f,0.87f,1.0f,0.15f);
    static HashSet<Type> pluralUISet = new HashSet<Type>
    {
        { typeof(CharacterResponsePanel)},
        {typeof(ItemCostSelectPanel) },
        typeof(CostSelectPanel)
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
            // 电影模式来自同步状态切换，面板加载失败统一记录。
            AsyncTaskRunner.Run(ShowGamePanel<FilmPanel>(), nameof(SetFilmUI));
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
    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        filmUI = false;
        BaseReference.UILayer = LayerMask.NameToLayer("UI");
        BaseReference.HideLayer = LayerMask.NameToLayer("Hide");
        BaseReference.WorldLayer = LayerMask.NameToLayer("WorldUI");

        // canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        GameActionManager.instance.AddListener<ClosePanelAction>(ClosePanel);
        GameActionManager.instance.AddListener<OpenPanelAction>(OpenPanel);
        GameActionManager.instance.AddListener<HidePanel>(HidePanel);
        GameActionManager.instance.AddListener<HidePanels>(HidePanels);
        GameActionManager.instance.AddListener<HideAllPanel>(HideAllPanel);
        GameActionManager.instance.AddListener<HidePanelGroup>(HidePanelGroup);
        GameActionManager.instance.AddListener<SetFilmUI>(SetFilmUI);

        SelectableGuideRegistry.SetStringAction = UIAudioForTag;

        TagAudioDataList tagAudioDataList = await GameSourceManager.instance.GetScriptableObject<TagAudioDataList>("Data/TagAudioData");
        tagUIAudioDic.Clear();
        if (tagAudioDataList == null)
        {
            Debug.LogError("UIManager init failed: missing Data/TagAudioData");
        }
        else
        {
            for(int i = 0; i < tagAudioDataList.tagAudioDatas.Count; i++)
            {
                tagUIAudioDic[tagAudioDataList.tagAudioDatas[i].tag] = tagAudioDataList.tagAudioDatas[i].audioClip;
            }
        }

        string JoyStickColorStr=PlayerPrefs.GetString("JoyStickColor");
        if (!string.IsNullOrEmpty(JoyStickColorStr))
        {
            var colorS = JoyStickColorStr.Split(',');
            joyStickColor = new Color(float.Parse(colorS[0]), float.Parse(colorS[1]), float.Parse(colorS[2]), float.Parse(colorS[3]));
            var ScreenControllerPanel = await GetGamePanel<ScreenControllerPanel>();
            if (ScreenControllerPanel)
            {
                ScreenControllerPanel.RefreshJoyStickColor();
            }
        }
    } 
    public void SetJoyStickColor(Color color)
    {
        AsyncTaskRunner.Run(() => SetJoyStickColorAsync(color), nameof(SetJoyStickColor));
    }

    public async System.Threading.Tasks.Task SetJoyStickColorAsync(Color color)
    {
        joyStickColor = color;
        PlayerPrefs.SetString("JoyStickColor", $"{color.r},{color.g},{color.b},{color.a}");
        var ScreenControllerPanel = await GetGamePanel<ScreenControllerPanel>();
        if (ScreenControllerPanel)
        {
            ScreenControllerPanel.RefreshJoyStickColor();
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
                if (panel.show && panel.GetType() != typeof(SimpleTalkPanel) && panel.GetType() != typeof(FilmPanel))
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

    public GamePanelLifecycleState GetPanelState(Type type)
    {
        return panelStates.TryGetValue(type, out var state) ? state : GamePanelLifecycleState.Closed;
    }

    private int GetPanelVersion(Type type)
    {
        return panelVersions.TryGetValue(type, out var version) ? version : 0;
    }

    private (int version, CancellationToken token) BeginPanelOperation(Type type, GamePanelLifecycleState state)
    {
        if (IsPluralUI(type))
        {
            SetPanelState(type, state);
            return (GetPanelVersion(type), CancellationToken.None);
        }

        CancelPanelCancellation(type);
        var version = GetPanelVersion(type) + 1;
        panelVersions[type] = version;
        SetPanelState(type, state);
        var cancellationSource = new CancellationTokenSource();
        panelCancellationSources[type] = cancellationSource;
        return (version, cancellationSource.Token);
    }

    private void CancelPanelCancellation(Type type)
    {
        if (!panelCancellationSources.TryGetValue(type, out var cancellationSource))
        {
            return;
        }

        panelCancellationSources.Remove(type);
        if (!cancellationSource.IsCancellationRequested)
        {
            cancellationSource.Cancel();
        }
        cancellationSource.Dispose();
    }

    private bool IsPanelVersionCurrent(Type type, int version)
    {
        return GetPanelVersion(type) == version;
    }

    private bool IsPanelOperationCurrent(Type type, int version, CancellationToken cancellationToken)
    {
        return !cancellationToken.IsCancellationRequested && (IsPluralUI(type) || IsPanelVersionCurrent(type, version));
    }

    private void SetPanelState(Type type, GamePanelLifecycleState state)
    {
        panelStates[type] = state;
    }

    private void CancelPanelCallbacks(Type type, GamePanelLifecycleState state)
    {
        if (!IsPluralUI(type))
        {
            CancelPanelCancellation(type);
            panelVersions[type] = GetPanelVersion(type) + 1;
        }
        panelStates[type] = state;
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
        if (GameDataManager.instance.GlobalData.debug)
            Debug.Log($"ShowPanel:{type}");
        return  gamePanel as T;
    }

    public T ShowGamePanelImmediately<T>(string dataKey = null, int layer = -1, Transform parent = null)
        where T : BaseReference
    {
        var type = typeof(T);
        var gamePanel = ShowGamePanelImmediately(type, dataKey, layer, parent);
        if (GameDataManager.instance.GlobalData.debug)
            Debug.Log($"ShowPanel:{type}");
        return gamePanel as T;
    }

    public async Task<T> ShowGamePanel<T, V>(V data, int layer = -1, Transform parent = null) where T : GamePanel<V> where V : IReferenceData
    {
        var type = typeof(T);
        var gamePanel = await ShowGamePanel(type, data, layer, parent);
        if (GameDataManager.instance.GlobalData.debug)
            Debug.Log($"ShowPanel:{type}");
        return (T)gamePanel;
    }
    public  T ShowGamePanelImmediately<T, V>(V data, int layer = -1, Transform parent = null) where T : GamePanel<V> where V : IReferenceData
    {
        var type = typeof(T);
        var gamePanel =  ShowGamePanelImmediately(type, data, layer, parent);
        if (GameDataManager.instance.GlobalData.debug)
            Debug.Log($"ShowPanelImmediately:{type}");
        return (T)gamePanel;
    }

    public bool RemoveGamePanel(Type type)
    {
        return openedPanels.Remove(type);
    }
    private GamePanel<V> ShowGamePanelImmediately<V>(Type type, V data, int layer = -1, Transform parent = null) where V : IReferenceData
    {
        var panelOperation = BeginPanelOperation(type, GamePanelLifecycleState.Loading);
        int panelVersion = panelOperation.version;
        var cancellationToken = panelOperation.token;

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

            if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
            {
                GameObject.Destroy(_Panel);
                return null;
            }
                 
            gamePanel.Show(layer);
            SetPanelState(type, GamePanelLifecycleState.Opened);
            gamePanel.InitReferenceData(data, cancellationToken);
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
            if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
            {
                return null;
            }
            gamePanel.Show(layer);
            SetPanelState(type, GamePanelLifecycleState.Opened);
            gamePanel.InitReferenceData(data, cancellationToken);
            return gamePanel;
        }
    }
    private async Task<GamePanel<V>> ShowGamePanel<V>(Type type, V data, int layer = -1, Transform parent = null) where V : IReferenceData
    {
        var panelOperation = BeginPanelOperation(type, GamePanelLifecycleState.Loading);
        int panelVersion = panelOperation.version;
        var cancellationToken = panelOperation.token;
        if (!gamePanels.TryGetValue(type, out BaseReference panel) || panel == null)
        {
            string path = $"{DataPath.UIPath}{type}";
            var gamePanelObj = await GameSourceManager.instance.GetPrefab(path);
            if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
            {
                return null;
            }
            var async=GameObject.InstantiateAsync(gamePanelObj, parent == null ? canvasParent : parent);
            await async;
            if (!Application.isPlaying || SingletonType.Cleared)
            {
                GameObject.Destroy(gamePanelObj);
                SetPanelState(type, GamePanelLifecycleState.Destroyed);
                return null;
            }
            if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
            {
                GameObject.Destroy(async.Result[0]);
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

            if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
            {
                GameObject.Destroy(_Panel);
                return null;
            }
            gamePanel.gameObject.SetActive(true);
            gamePanel.Show(layer);
            SetPanelState(type, GamePanelLifecycleState.Opened);
            gamePanel.InitReferenceData(data, cancellationToken);
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
            if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
            {
                return null;
            }
            gamePanel.gameObject.SetActive(true);
            gamePanel.Show(layer);
            SetPanelState(type, GamePanelLifecycleState.Opened);
            gamePanel.InitReferenceData(data, cancellationToken);
           
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

    private void OpenPanel(OpenPanelAction openPanelEvent)
    {
        // Action 回调保持同步签名，异步打开异常统一进入 AsyncTaskRunner 日志。
        AsyncTaskRunner.Run(ShowGamePanel(openPanelEvent.type, openPanelEvent.dataId), nameof(OpenPanel));
    }

    HashSet<Type> openedPanels = new HashSet<Type>();
    private async Task<BaseReference> ShowGamePanel(Type type, string dataKey = null, int layer = -1, Transform parent = null)
    { 
        var panelOperation = BeginPanelOperation(type, GamePanelLifecycleState.Loading);
        int panelVersion = panelOperation.version;
        var cancellationToken = panelOperation.token;
        if (!gamePanels.TryGetValue(type, out BaseReference gamePanel) || gamePanel == null || IsPluralUI(type))
        {
            string path = $"{DataPath.UIPath}{type}";
            var gamePanelObj = await GameSourceManager.instance.GetPrefab(path);
            if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
            {
                return null;
            }
            var async = GameObject.InstantiateAsync(gamePanelObj, parent == null ? canvasParent : parent);
            await async;
            
            

            if (!Application.isPlaying||SingletonType.Cleared)
            {
                GameObject.DestroyImmediate(gamePanelObj);
                SetPanelState(type, GamePanelLifecycleState.Destroyed);
                return null;
            }else
            if ( SingletonType.Cleared)
            {
                GameObject.Destroy(gamePanelObj);
                SetPanelState(type, GamePanelLifecycleState.Destroyed);
                return null;
            }
            if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
            {
                GameObject.Destroy(async.Result[0]);
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

            if (parent != null)
            {
                var allT = parent.GetComponentsInChildren(type, true);
                for (var i = 0; i < allT.Length; i++)
                {
                    var oldT = allT[i];
                    if (oldT != gamePanel) GameObject.Destroy(oldT.gameObject);
                }
            }
            if (!IsPluralUI(type))
            {
                if(gamePanels.TryGetValue(type,out var _panel))
                {
                    if (_panel != null && _panel != gamePanel)
                    {
                        _panel.Close();
                    }
                }
                gamePanels[type] = gamePanel;
            }
        }
        if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
        {
            if (gamePanel != null && IsPluralUI(type))
            {
                GameObject.Destroy(gamePanel.gameObject);
            }
            return null;
        }
        if (parent != null)
        {
            gamePanel.transform.SetParent(parent);
            gamePanel.transform.localPosition = Vector3.zero;
        }
        gamePanel.Show(layer);
        SetPanelState(type, GamePanelLifecycleState.Opened);
        gamePanel.gameObject.SetActive(true);
        try
        {
            await gamePanel.InitData(dataKey, cancellationToken);
            if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
            {
                return null;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch
        {
            if (IsPanelOperationCurrent(type, panelVersion, cancellationToken))
            {
                SetPanelState(type, GamePanelLifecycleState.Closed);
            }
            throw;
        }
      
        return gamePanel;
    }

    private BaseReference ShowGamePanelImmediately(Type type, string dataKey = null, int layer = -1,
        Transform parent = null)
    {
        var panelOperation = BeginPanelOperation(type, GamePanelLifecycleState.Loading);
        int panelVersion = panelOperation.version;
        var cancellationToken = panelOperation.token;
        if (!gamePanels.TryGetValue(type, out var gamePanel) || gamePanel == null || IsPluralUI(type))
        {
            var path = $"{DataPath.UIPath}{type}";
            var gamePanelObj = GameSourceManager.instance.GetPrefabImmediately(path);
            var _Panel = GameObject.Instantiate(gamePanelObj, parent == null ? canvasParent : parent);


            if (!Application.isPlaying || SingletonType.Cleared)
            {
                GameObject.DestroyImmediate(gamePanelObj);
                return null;
            }

            if (SingletonType.Cleared)
            {
                GameObject.Destroy(gamePanelObj);
                return null;
            }

            _Panel.transform.localPosition = Vector3.zero;
            var gamePanelComponent = _Panel.GetComponent(type);

            if (gamePanelComponent == null)
                gamePanel = (BaseReference)_Panel.AddComponent(type);
            else
                gamePanel = (BaseReference)gamePanelComponent;
            if (!IsPluralUI(type))
            {
                if (gamePanels.TryGetValue(type, out var _panel))
                    if (_panel != null && _panel != gamePanel)
                        _panel.Close();

                gamePanels[type] = gamePanel;
            }
        }

        if (!IsPanelOperationCurrent(type, panelVersion, cancellationToken))
        {
            if (gamePanel != null && IsPluralUI(type))
            {
                GameObject.Destroy(gamePanel.gameObject);
            }
            return null;
        }

        if (parent != null)
        {
            gamePanel.transform.SetParent(parent);
            gamePanel.transform.localPosition = Vector3.zero;
        }

        gamePanel.Show(layer);
        SetPanelState(type, GamePanelLifecycleState.Opened);
        gamePanel.gameObject.SetActive(true);
        // Immediately 接口同步返回，异步初始化仍绑定本次打开 token，避免旧回调覆盖新状态。
        AsyncTaskRunner.Run(gamePanel.InitData(dataKey, cancellationToken), nameof(ShowGamePanelImmediately));

        return gamePanel;
    }

    public void UnLoadPanel(List<Type> panels)
    {
        for (var i = 0; i < panels.Count; i++)
        {
            var type = panels[i];
            if (gamePanels.TryGetValue(type, out var gamePanel))
            {
                if (gamePanel == null)
                {
                    gamePanels.Remove(type);
                    return;
                }

            if (gamePanel.show) gamePanel.Close();
            CancelPanelCallbacks(type, GamePanelLifecycleState.Destroyed);
            GameObject.Destroy(gamePanel);
        }
        }

        Resources.UnloadUnusedAssets();
    }

    public void UnLoadPanel<T>()
    {
        var type = typeof(T);
        if (gamePanels.TryGetValue(type, out var gamePanel))
        {
            if (gamePanel == null)
            {
                gamePanels.Remove(type);
                return;
            }

            if (gamePanel.show) gamePanel.Close();
            CancelPanelCallbacks(type, GamePanelLifecycleState.Destroyed);
            GameObject.Destroy(gamePanel);
            Resources.UnloadUnusedAssets();
        }
    }
    public void CloseGamePanel<T>()
    {
        var type = typeof(T);
        if (!IsPluralUI(type))
        {
            openedPanels.Remove(type);
        }
        CancelPanelCallbacks(type, GamePanelLifecycleState.Closing);
        if (gamePanels.TryGetValue(type, out BaseReference gamePanel))
        {
            if (gamePanel == null)
            {
                gamePanels.Remove(type);
                SetPanelState(type, GamePanelLifecycleState.Closed);
                return;
            }
            if (gamePanel.show)
            {
                gamePanel.Close();
            }
            SetPanelState(type, GamePanelLifecycleState.Closed);
           
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
            CancelPanelCallbacks(closePanelEvent.type, GamePanelLifecycleState.Closing);
            if (gamePanel == null)
            {
                gamePanels.Remove(closePanelEvent.type);
                SetPanelState(closePanelEvent.type, GamePanelLifecycleState.Closed);
                return;
            }
            gamePanel.Close();
            SetPanelState(closePanelEvent.type, GamePanelLifecycleState.Closed);
        }
        else
        {
            CancelPanelCallbacks(closePanelEvent.type, GamePanelLifecycleState.Closed);
        }
    }
}
public static class RectTransformPresets
{
    public enum Preset
    {
        // 9宫
        TopLeft, TopCenter, TopRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        BottomLeft, BottomCenter, BottomRight,

        // 常用拉伸（和面板里那几种一致）
        StretchTop,
        StretchMiddle,
        StretchBottom, // 横向拉伸
        StretchLeft,
        StretchCenter,
        StretchRight, // 纵向拉伸
        StretchAll // 全拉伸
    }

    /// <summary>
    /// 模拟 Anchor Presets 的点击/Alt/Shift/Alt+Shift。
    /// keepPosition = Shift；alsoSetPivot = Alt。
    /// </summary>
    public static void Apply(RectTransform rt, Preset preset, bool keepPosition = false, bool alsoSetPivot = false)
    {
        if (rt == null || rt.parent == null) return;
        var parentRT = rt.parent as RectTransform;
        if (parentRT == null) return;

        // 记录旧锚/偏移，为 Shift 计算做准备
        Vector2 oldAnchorMin = rt.anchorMin;
        Vector2 oldAnchorMax = rt.anchorMax;
        Vector2 oldOffsetMin = rt.offsetMin;
        Vector2 oldOffsetMax = rt.offsetMax;

        // 预设 -> 目标锚与目标 pivot
        (Vector2 aMin, Vector2 aMax, Vector2 pivot) = GetPresetAnchorsAndPivot(preset);

        // 设置 anchors
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;

        // Shift：保持位置/尺寸不动（等价编辑器里按住 Shift）
        if (keepPosition)
        {
            // 关键：offset 要加上锚变化 * 父尺寸
            Vector2 parentSize = parentRT.rect.size;
            Vector2 deltaMin = (aMin - oldAnchorMin) * parentSize;
            Vector2 deltaMax = (aMax - oldAnchorMax) * parentSize;
            rt.offsetMin = oldOffsetMin + deltaMin;
            rt.offsetMax = oldOffsetMax + deltaMax;
        }

        // Alt：同步 pivot
        if (alsoSetPivot)
        {
            rt.pivot = pivot;
        }
    }

    private static (Vector2 aMin, Vector2 aMax, Vector2 pivot) GetPresetAnchorsAndPivot(Preset p)
    {
        switch (p)
        {
            // ===== 9宫 =====
            case Preset.TopLeft: return (new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
            case Preset.TopCenter: return (new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            case Preset.TopRight: return (new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));

            case Preset.MiddleLeft: return (new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            case Preset.MiddleCenter: return (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            case Preset.MiddleRight: return (new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));

            case Preset.BottomLeft: return (new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
            case Preset.BottomCenter: return (new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            case Preset.BottomRight: return (new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));

            // ===== 横向拉伸（Y 锚固定，上中下）=====
            case Preset.StretchTop: return (new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            case Preset.StretchMiddle: return (new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f));
            case Preset.StretchBottom: return (new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));

            // ===== 纵向拉伸（X 锚固定，左中右）=====
            case Preset.StretchLeft: return (new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f));
            case Preset.StretchCenter: return (new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f));
            case Preset.StretchRight: return (new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));

            // ===== 全拉伸 =====
            case Preset.StretchAll: return (new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        }

        // 兜底
        return (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
    }
}
