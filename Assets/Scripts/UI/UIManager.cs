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
    public Color JoyStickColor=>joyStickColor;
    private  Color joyStickColor=new Color(0.03f,0.87f,1.0f,0.15f);
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
        BaseReference.WorldLayer = LayerMask.NameToLayer("WorldUI");

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
    public async void SetJoyStickColor(Color color)
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
        if (GameDataManager.instance.GlobalData.debug)
            Debug.Log($"ShowPanel:{type}");
        return  gamePanel as T;
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

            gamePanel.gameObject.SetActive(true);
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
            gamePanel.gameObject.SetActive(true);
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
        //Debug.Log($"OpenPanelAction :{openPanelEvent.type}");
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
                GameObject.DestroyImmediate(gamePanelObj);
                return null;
            }else
            if ( SingletonType.Cleared)
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

public static class GridSimLayout
{
    /// 把 items（真实在 actualParent 下）摆成“好像在 grid 下”的布局。
    /// 视觉等效 GridLayoutGroup（包含 padding/spacing/startCorner/startAxis/constraint/childAlignment）
    /// 建议：items 的 anchorMin=anchorMax=(0,1)，pivot=(0,1)（左上），最稳。
    public static void Apply(
        GridLayoutGroup grid,
        List<RectTransform> items,
        RectTransform actualParent,
        bool forceChildSizeToCell = true)
    {
        if (grid == null || actualParent == null || items == null) return;
        var gridRT = (RectTransform)grid.transform;
        var n = items.Count;
        if (n == 0) return;

        // 可选：禁用 grid，避免它自己去重建布局（不影响取参）
        grid.enabled = false;

        for (var i = 0; i < n; i++)
        {
            var rt = items[i];
            if (!rt) continue;

            // —— 推荐统一成左上锚点/枢轴，避免额外换算 —— //
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);

            if (forceChildSizeToCell)
                rt.sizeDelta = grid.cellSize;

            // 1) 在“grid 左上参考系”下计算第 i 个应当位置
            var posInGridTopLeft = GetGridAnchoredPosition(grid, i, n);

            // 2) grid 左上参考系 -> grid 本地坐标（原点 = grid pivot）
            var gridTopLeftLocal = new Vector2(
                -gridRT.pivot.x * gridRT.rect.width,
                (1f - gridRT.pivot.y) * gridRT.rect.height);
            var localInGrid = new Vector3(
                gridTopLeftLocal.x + posInGridTopLeft.x,
                gridTopLeftLocal.y + posInGridTopLeft.y, 0f);

            // 3) 本地->世界->actualParent本地
            var world = gridRT.TransformPoint(localInGrid);
            var localInParent = actualParent.InverseTransformPoint(world);

            // 4) actualParent 本地坐标 -> 左上参考系 anchoredPosition
            var parentTopLeftLocal = new Vector2(
                -actualParent.pivot.x * actualParent.rect.width,
                (1f - actualParent.pivot.y) * actualParent.rect.height);

            rt.anchoredPosition = (Vector2)localInParent - parentTopLeftLocal;
        }
    }

    // === 与官方 GridLayoutGroup 等效的“第 index 个在网格内的左上锚点坐标” ===
    private static Vector2 GetGridAnchoredPosition(GridLayoutGroup g, int index, int childCount)
    {
        var content = (RectTransform)g.transform;
        var cell = g.cellSize;
        var spacing = g.spacing;
        var pad = g.padding;
        var corner = g.startCorner;
        var axis = g.startAxis;

        var W = content.rect.width;
        var H = content.rect.height;

        // 列/行数
        int cols, rows;
        switch (g.constraint)
        {
            case GridLayoutGroup.Constraint.FixedColumnCount:
                cols = Mathf.Max(1, g.constraintCount);
                rows = Mathf.CeilToInt((float)childCount / cols);
                break;
            case GridLayoutGroup.Constraint.FixedRowCount:
                rows = Mathf.Max(1, g.constraintCount);
                cols = Mathf.CeilToInt((float)childCount / rows);
                break;
            default: // Flexible
            {
                const float EPS = 0.001f; // 边界余量，贴近官方行为
                var usableW = W - pad.left - pad.right + spacing.x + EPS;
                cols = Mathf.Max(1, Mathf.FloorToInt(usableW / (cell.x + spacing.x)));
                rows = Mathf.CeilToInt((float)childCount / cols);
                break;
            }
        }

        cols = Mathf.Max(1, cols);
        rows = Mathf.Max(1, rows);

        // 行列索引（受 startAxis 影响）
        int r, c;
        if (axis == GridLayoutGroup.Axis.Horizontal)
        {
            r = index / cols;
            c = index % cols;
        }
        else
        {
            r = index % rows;
            c = index / rows;
        }

        // 网格整体尺寸（不含 padding）
        var reqX = cols * cell.x + (cols - 1) * spacing.x;
        var reqY = rows * cell.y + (rows - 1) * spacing.y;

        // childAlignment（等价 GetStartOffset）
        var (ax, ay) = Alignment01(g.childAlignment);
        var surplusX = Mathf.Max(0, W - pad.left - pad.right - reqX);
        var surplusY = Mathf.Max(0, H - pad.top - pad.bottom - reqY);

        var originX = pad.left + surplusX * ax; // 左上参考系 X 向右为正
        var originYUp = pad.top + surplusY * ay; // 顶边（正值；最终 y 取负）

        var rightSide = corner == GridLayoutGroup.Corner.UpperRight || corner == GridLayoutGroup.Corner.LowerRight;
        var lowerSide = corner == GridLayoutGroup.Corner.LowerLeft || corner == GridLayoutGroup.Corner.LowerRight;

        var colOffX = rightSide
            ? reqX - cell.x - c * (cell.x + spacing.x) // 从右往左
            : c * (cell.x + spacing.x); // 从左往右

        var rowOffY = lowerSide
            ? reqY - cell.y - r * (cell.y + spacing.y) // 从下往上（最终取负）
            : r * (cell.y + spacing.y); // 从上往下

        var x = originX + colOffX;
        var y = -(originYUp + rowOffY); // 左上参考系：向下为负
        return new Vector2(x, y);
    }

    private static (float ax, float ay) Alignment01(TextAnchor a)
    {
        switch (a)
        {
            case TextAnchor.UpperLeft: return (0f, 0f);
            case TextAnchor.UpperCenter: return (0.5f, 0f);
            case TextAnchor.UpperRight: return (1f, 0f);
            case TextAnchor.MiddleLeft: return (0f, 0.5f);
            case TextAnchor.MiddleCenter: return (0.5f, 0.5f);
            case TextAnchor.MiddleRight: return (1f, 0.5f);
            case TextAnchor.LowerLeft: return (0f, 1f);
            case TextAnchor.LowerCenter: return (0.5f, 1f);
            case TextAnchor.LowerRight: return (1f, 1f);
        }

        return (0f, 0f);
    }
}

