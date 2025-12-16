using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using UnityTexture2D = UnityEngine.Texture2D;

namespace UnityEditor.U2D.Sprites
{
    /// <summary>
    ///     Interface for providing a ISpriteEditorDataProvider instance.
    /// </summary>
    /// <typeparam name="T">The object type the implemented interface is interested in.</typeparam>
    public interface ISpriteDataProviderFactory<T>
    {
        /// <summary>
        ///     Implement the method to provide an instance of ISpriteEditorDataProvider for a given object.
        /// </summary>
        /// <param name="obj">The object that requires an instance of ISpriteEditorDataProvider.</param>
        /// <returns>An instance of ISpriteEditorDataProvider or null if not supported by the interface.</returns>
        ISpriteEditorDataProvider CreateDataProvider(T obj);
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class SpriteEditorAssetPathProviderAttribute : Attribute
    {
        [RequiredSignature]
        private static string GetAssetPath(Object obj)
        {
            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class SpriteObjectProviderAttribute : Attribute
    {
        [RequiredSignature]
        private static Sprite GetSpriteObject(Object obj)
        {
            return null;
        }
    }

    /// <summary>
    ///     Utility class that collects methods with SpriteDataProviderFactoryAttribute and
    ///     SpriteDataProviderAssetPathProviderAttribute.
    /// </summary>
    public class SpriteDataProviderFactories
    {
        private struct SpriteDataProviderFactory
        {
            public object instance;
            public MethodInfo method;
            public Type methodType;
        }

        private static SpriteDataProviderFactory[] s_Factories;
        private static TypeCache.MethodCollection s_AssetPathProvider;
        private static TypeCache.MethodCollection s_SpriteObjectProvider;

        private static SpriteDataProviderFactory[] GetFactories()
        {
            CacheDataProviders();
            return s_Factories;
        }

        private static TypeCache.MethodCollection GetAssetPathProvider()
        {
            CacheDataProviders();
            return s_AssetPathProvider;
        }

        private static TypeCache.MethodCollection GetSpriteObjectProvider()
        {
            CacheDataProviders();
            return s_SpriteObjectProvider;
        }

        private static void CacheDataProviders()
        {
            if (s_Factories != null)
                return;

            var factories = TypeCache.GetTypesDerivedFrom(typeof(ISpriteDataProviderFactory<>));
            var factoryList = new List<SpriteDataProviderFactory>();
            foreach (var factory in factories)
                try
                {
                    var ins = Activator.CreateInstance(factory);
                    foreach (var i in factory.GetInterfaces())
                    {
                        var genericArguments = i.GetGenericArguments();
                        if (genericArguments.Length == 1)
                        {
                            var s = new SpriteDataProviderFactory();
                            s.instance = ins;
                            var method = i.GetMethod("CreateDataProvider");
                            s.method = method;
                            s.methodType = genericArguments[0];
                            factoryList.Add(s);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogAssertion(ex);
                }

            s_Factories = factoryList.ToArray();
            s_AssetPathProvider = TypeCache.GetMethodsWithAttribute<SpriteEditorAssetPathProviderAttribute>();
            s_SpriteObjectProvider = TypeCache.GetMethodsWithAttribute<SpriteObjectProviderAttribute>();
        }

        /// <summary>
        ///     Initialized and collect methods with SpriteDataProviderFactoryAttribute and
        ///     SpriteDataProviderAssetPathProviderAttribute.
        /// </summary>
        public void Init()
        {
            CacheDataProviders();
        }

        /// <summary>
        ///     Given a UnityEngine.Object, determine the ISpriteEditorDataProvider associate with the object by going
        ///     going through the methods with SpriteDataProviderFactoryAttribute.
        /// </summary>
        /// <remarks>
        ///     When none of the methods is able to provide ISpriteEditorDataProvider for the object, the method will
        ///     try to cast the AssetImporter of the object to ISpriteEditorDataProvider.
        /// </remarks>
        /// <param name="obj">The UnityEngine.Object to query.</param>
        /// <returns>The ISpriteEditorDataProvider associated with the object.</returns>
        public ISpriteEditorDataProvider GetSpriteEditorDataProviderFromObject(Object obj)
        {
            if (obj != null)
            {
                var objType = obj.GetType();
                foreach (var factory in GetFactories())
                    try
                    {
                        if (factory.methodType == objType)
                        {
                            var dataProvider =
                                factory.method.Invoke(factory.instance, new[] { obj }) as ISpriteEditorDataProvider;
                            if (dataProvider != null && !dataProvider.Equals(null))
                                return dataProvider;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogAssertion(ex);
                    }

                if (obj is ISpriteEditorDataProvider)
                    return (ISpriteEditorDataProvider)obj;
                // now we try the importer
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
                return importer as ISpriteEditorDataProvider;
            }

            return null;
        }

        /// <summary>
        ///     Given a UnityEngine.Object, determine the asset path associate with the object by going
        ///     going through the methods with SpriteDataProviderAssetPathProviderAttribute.
        /// </summary>
        /// <remarks>When none of the methods is able to provide the asset path for the object, the method will return null</remarks>
        /// <param name="obj">The UnityEngine.Object to query</param>
        /// <returns>The asset path for the object</returns>
        internal string GetAssetPath(Object obj)
        {
            foreach (var assetPathProvider in GetAssetPathProvider())
                try
                {
                    var path = assetPathProvider.Invoke(null, new object[] { obj }) as string;
                    if (!string.IsNullOrEmpty(path))
                        return path;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

            return null;
        }

        /// <summary>
        ///     Given a UnityEngine.Object, determine the Sprite object associate with the object by going
        ///     going through the methods with SpriteObjectProviderAttribute.
        /// </summary>
        /// <remarks>When none of the methods is able to provide a Sprite object, the method will return null</remarks>
        /// <param name="obj">The UnityEngine.Object to query</param>
        /// <returns>The Sprite object</returns>
        internal Sprite GetSpriteObject(Object obj)
        {
            if (obj is Sprite)
                return (Sprite)obj;
            foreach (var spriteObjectProvider in GetSpriteObjectProvider())
                try
                {
                    var sprite = spriteObjectProvider.Invoke(null, new object[] { obj }) as Sprite;
                    if (sprite != null && !sprite.Equals(null))
                        return sprite;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

            return null;
        }
    }

    [InitializeOnLoad]
    internal class SpriteEditorWindow : SpriteUtilityWindow, ISpriteEditor, ISupportsOverlays
    {
        static SpriteEditorWindow()
        {
            UnityEditor.SpriteUtilityWindow.SetShowSpriteEditorWindowWithObject(x =>
            {
                GetWindow(x);
                return true;
            });
        }

        private class SpriteEditorWindowStyles
        {
            public static readonly GUIContent editingDisableMessageBecausePlaymodeLabel =
                EditorGUIUtility.TrTextContent("Editing is disabled during play mode");

            public static readonly GUIContent editingDisableMessageBecauseNonEditableLabel =
                EditorGUIUtility.TrTextContent("Editing is disabled because the asset is not editable.");

            public static readonly GUIContent revertButtonLabel = EditorGUIUtility.TrTextContent("Revert");
            public static readonly GUIContent applyButtonLabel = EditorGUIUtility.TrTextContent("Apply");

            public static readonly GUIContent pendingChangesDialogContent =
                EditorGUIUtility.TrTextContent(
                    "The asset was modified outside of Sprite Editor Window.\nDo you want to apply pending changes?");

            public static readonly GUIContent applyRevertDialogTitle =
                EditorGUIUtility.TrTextContent("Sprite Editor Window");

            public static readonly GUIContent applyRevertDialogContent =
                EditorGUIUtility.TrTextContent("'{0}' has unapplied changes.\nDo you want to apply or revert them?");

            public static readonly GUIContent noSelectionWarning =
                EditorGUIUtility.TrTextContent(
                    "No Texture or Sprite selected.\nPlease select one from your project to start.");

            public static readonly GUIContent selectionNotEditableBySpriteEditor =
                EditorGUIUtility.TrTextContent("Selected object is not supported by Sprite Editor Window.");

            public static readonly GUIContent noModuleWarning =
                EditorGUIUtility.TrTextContent("No Sprite Editor module available");

            public static readonly GUIContent applyRevertModuleDialogTitle =
                EditorGUIUtility.TrTextContent("Sprite Editor Window");

            public static readonly GUIContent applyRevertModuleDialogContent =
                EditorGUIUtility.TrTextContent(
                    "You have unapplied changes from the current module. Do you want to apply or revert them?");

            public static readonly GUIContent revertConfirmationDialogTitle =
                EditorGUIUtility.TrTextContent("Revert Changes");

            public static readonly GUIContent revertConfirmationDialogContent =
                EditorGUIUtility.TrTextContent("Are you sure you want to revert the changes?");

            public static readonly GUIContent applyConfirmationDialogTitle =
                EditorGUIUtility.TrTextContent("Apply Changes");

            public static readonly GUIContent applyConfirmationDialogContent =
                EditorGUIUtility.TrTextContent("Are you sure you want to apply the changes?");

            public static readonly GUIContent yesLabel = EditorGUIUtility.TrTextContent("Yes");
            public static readonly GUIContent noLabel = EditorGUIUtility.TrTextContent("No");

            public static readonly string styleSheetPath =
                "Packages/com.unity.2d.sprite/Editor/UI/SpriteEditor/SpriteEditor.uss";

            public static readonly string toolBarStyleSheetPath =
                "Packages/com.unity.2d.sprite/Editor/UI/SpriteEditor/SpriteEditorToolbar.uss";
        }

        private class CurrentResetContext
        {
            public string assetPath;
        }

        private const float k_MarginForFraming = 0.05f;
        private const float k_WarningMessageWidth = 250f;
        private const float k_WarningMessageHeight = 40f;
        private const float k_ModuleListWidth = 90f;
        private const string k_RefreshOnNextRepaintCommandEvent = "RefreshOnNextRepaintCommand";
        private bool m_ResetOnNextRepaint;
        private bool m_ResetCommandSent;

        private List<SpriteRect> m_RectsCache;

        private bool m_RequestRepaint;

        public static bool s_OneClickDragStarted;
        private string m_SelectedAssetPath;
        private bool m_AssetNotEditable;

        private readonly IEventSystem m_EventSystem;
        private readonly IUndoSystem m_UndoSystem;
        private readonly IAssetDatabase m_AssetDatabase;
        private readonly IGUIUtility m_GUIUtility;
        private UnityTexture2D m_OutlineTexture;
        private UnityTexture2D m_ReadableTexture;
        private readonly Dictionary<Type, RequireSpriteDataProviderAttribute> m_ModuleRequireSpriteDataProvider = new();
        private readonly Dictionary<Type, List<Type>> m_ModuleMode = new();

        private VisualElement m_ToolbarContainer;
        private VisualElement m_ModuleToolbarContainer;
        private VisualElement m_AlphaZoomToolbarElement;
        private VisualElement m_ModuleDropDownUI;
        private IMGUIContainer m_ModuleToolbarIMGUIElement;
        private IMGUIContainer m_MainViewIMGUIElement;
        private VisualElement m_ModuleViewElement;
        private VisualElement m_MainViewElement;
        private SpriteDataProviderFactories m_SpriteDataProviderFactories;

        [SerializeField] private Object m_SelectedObject;

        [SerializeField] private string m_SelectedSpriteRectGUID;

        internal Func<string, string, bool> onHandleApplyRevertDialog = ShowHandleApplyRevertDialog;

        private CurrentResetContext m_CurrentResetContext;

        private readonly EditorGUIUtility.EditorLockTracker m_LockTracker = new();

        public static void GetWindow(Object obj)
        {
            var window = GetWindow<SpriteEditorWindow>();
            window.selectedObject = obj;
        }

        public SpriteEditorWindow()
        {
            m_EventSystem = new EventSystem();
            m_UndoSystem = new UndoSystem();
            m_AssetDatabase = new AssetDatabaseSystem();
            m_GUIUtility = new GUIUtilitySystem();
        }

        private void ModifierKeysChanged()
        {
            if (focusedWindow == this) Repaint();
        }

        private void OnFocus()
        {
            if (selectedObject != Selection.activeObject)
                OnSelectionChange();
            if (selectedProviderChanged)
                RefreshSpriteEditorWindow();
        }

        internal Object selectedObject
        {
            get => m_SelectedObject;
            set
            {
                m_SelectedObject = value;
                RefreshSpriteEditorWindow();
            }
        }

        private string selectedAssetPath
        {
            get => m_SelectedAssetPath;
            set
            {
                m_SelectedAssetPath = value;
                m_AssetNotEditable = !AssetDatabase.IsOpenForEdit(m_SelectedAssetPath);
            }
        }

        public void RefreshPropertiesCache()
        {
            Undo.ClearUndo(this);
            m_Texture = null;
            var obj = AssetDatabase.LoadMainAssetAtPath(selectedAssetPath);
            spriteEditorDataProvider = spriteDataProviderFactories.GetSpriteEditorDataProviderFromObject(obj);
            if (!IsSpriteDataProviderValid())
            {
                selectedAssetPath = "";
                var s = m_SpriteDataProviderFactories.GetSpriteObject(Selection.activeObject);
                if (s != null)
                {
                    var t = SpriteInspector.BuildPreviewTexture(s, null, false, (int)s.rect.width, (int)s.rect.height);
                    SetPreviewTexture(t, t.width, t.height);
                }

                return;
            }


            spriteEditorDataProvider.InitSpriteEditorDataProvider();

            var textureProvider = spriteEditorDataProvider.GetDataProvider<ITextureDataProvider>();
            if (textureProvider != null)
            {
                int width = 0, height = 0;
                textureProvider.GetTextureActualWidthAndHeight(out width, out height);
                m_Texture = textureProvider.previewTexture == null
                    ? null
                    : new PreviewTexture2D(textureProvider.previewTexture, width, height);
            }
        }

        internal string GetSelectionAssetPath()
        {
            var path = spriteDataProviderFactories.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(path))
                path = m_AssetDatabase.GetAssetPath(selectedObject);
            return path;
        }

        public void InvalidatePropertiesCache()
        {
            spriteRects = null;
            spriteEditorDataProvider = null;
        }

        private Rect spriteEditorMessageRect =>
            new(
                k_InspectorWindowMargin,
                k_InspectorWindowMargin,
                k_WarningMessageWidth,
                k_WarningMessageHeight);

        public SpriteImportMode spriteImportMode => !IsSpriteDataProviderValid()
            ? SpriteImportMode.None
            : spriteEditorDataProvider.spriteImportMode;

        private bool activeDataProviderSelected => spriteEditorDataProvider != null;

        public bool textureIsDirty
        {
            get => hasUnsavedChanges;
            set => hasUnsavedChanges = value;
        }

        public bool selectedProviderChanged
        {
            get
            {
                var assetPath = GetSelectionAssetPath();
                var dataProvider = spriteDataProviderFactories.GetSpriteEditorDataProviderFromObject(selectedObject);
                return dataProvider != null && selectedAssetPath != assetPath;
            }
        }

        private void OnSelectionChange()
        {
            if (m_LockTracker.isLocked)
                return;

            selectedObject = Selection.activeObject;
            RefreshSpriteEditorWindow();
        }

        private void RefreshSpriteEditorWindow()
        {
            // In case of changed of texture/sprite or selected on non texture object
            var updateModules = false;
            var selectedSprite = spriteDataProviderFactories.GetSpriteObject(selectedObject);
            var assetPath = GetSelectionAssetPath();
            var dataProvider = spriteDataProviderFactories.GetSpriteEditorDataProviderFromObject(selectedObject);
            if ((dataProvider != null && selectedAssetPath != assetPath) ||
                (dataProvider == null && selectedSprite != null))
            {
                HandleApplyRevertDialog(SpriteEditorWindowStyles.applyRevertDialogTitle.text,
                    string.Format(SpriteEditorWindowStyles.applyRevertDialogContent.text, selectedAssetPath));
                selectedAssetPath = assetPath;
                ResetWindow();
                ResetZoomAndScroll();
                RefreshPropertiesCache();
                RefreshRects();
                updateModules = true;
            }

            if (m_RectsCache != null) UpdateSelectedSpriteRectFromSelection();

            // We only update modules when data provider changed
            if (updateModules)
                UpdateAvailableModules();
            if (m_ModuleDropDownUI != null)
                m_ModuleDropDownUI.style.display = m_Texture == null || activatedModules?.Count <= 0
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            Repaint();
        }

        private void UpdateSelectedSpriteRectFromSelection()
        {
            if (Selection.activeObject is Sprite)
            {
                UpdateSelectedSpriteRect(Selection.activeObject as Sprite);
            }
            else
            {
                var sprite = spriteDataProviderFactories.GetSpriteObject(Selection.activeObject);
                UpdateSelectedSpriteRect(sprite);
            }
        }

        public void ResetWindow()
        {
            InvalidatePropertiesCache();
            textureIsDirty = false;
            saveChangesMessage = SpriteEditorWindowStyles.applyRevertModuleDialogContent.text;
        }

        public void ResetZoomAndScroll()
        {
            m_Zoom = -1;
            m_ScrollPosition = Vector2.zero;
        }

        private SpriteDataProviderFactories spriteDataProviderFactories
        {
            get
            {
                if (m_SpriteDataProviderFactories == null)
                {
                    m_SpriteDataProviderFactories = new SpriteDataProviderFactories();
                    m_SpriteDataProviderFactories.Init();
                }

                return m_SpriteDataProviderFactories;
            }
        }

        protected void SetTitleContent(string windowTitle, string windowIconPath)
        {
            if (string.IsNullOrEmpty(windowTitle))
                return;

            Texture2D iconTex = null;
            if (!string.IsNullOrEmpty(windowIconPath))
            {
                if (EditorGUIUtility.isProSkin)
                {
                    var newName = "d_" + Path.GetFileName(windowIconPath);
                    var iconDirName = Path.GetDirectoryName(windowIconPath);
                    if (!string.IsNullOrEmpty(iconDirName))
                        newName = $"{iconDirName}/{newName}";

                    windowIconPath = newName;
                }

                if (EditorGUIUtility.pixelsPerPoint > 1)
                    windowIconPath = $"{windowIconPath}@2x";

                iconTex = EditorGUIUtility.Load(windowIconPath + ".png") as Texture2D;
            }

            titleContent = new GUIContent(windowTitle, iconTex);
        }

        private void OnEnable()
        {
            name = "SpriteEditorWindow";
            SetTitleContent(L10n.Tr("Sprite Editor"), "Packages/com.unity.2d.sprite/Editor/Assets/SpriteEditor");
            selectedObject = Selection.activeObject;
            minSize = new Vector2(360, 200);
            m_UndoSystem.RegisterUndoCallback(UndoRedoPerformed);
            EditorApplication.modifierKeysChanged += ModifierKeysChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorApplicationQuit;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            if (selectedProviderChanged)
                selectedAssetPath = GetSelectionAssetPath();

            ResetWindow();
            RefreshPropertiesCache();
            var noSelectedSprite = string.IsNullOrEmpty(m_SelectedSpriteRectGUID);
            RefreshRects();
            if (noSelectedSprite)
                UpdateSelectedSpriteRectFromSelection();
            UnityEditor.SpriteUtilityWindow.SetApplySpriteEditorWindow(RebuildCache);
        }

        private void CreateGUI()
        {
            if (m_MainViewElement == null)
                if (SetupVisualElements())
                    InitModules();
        }

        private void ShowButton(Rect r)
        {
            EditorGUI.BeginChangeCheck();
            m_LockTracker.ShowButton(r, "IN LockButton");
            if (!EditorGUI.EndChangeCheck())
                return;
            OnSelectionChange();
        }

        private bool SetupVisualElements()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(SpriteEditorWindowStyles.styleSheetPath);
            if (styleSheet != null && rootVisualElement != null && rootVisualElement.styleSheetList != null)
            {
                m_ToolbarContainer = new VisualElement
                {
                    name = "spriteEditorWindowToolbarContainer"
                };
                m_ModuleToolbarContainer = new VisualElement
                {
                    name = "spriteEditorWindowModuleToolbarContainer"
                };
                m_ModuleDropDownUI = new IMGUIContainer(DoModuleDropDownGUI)
                {
                    name = "spriteEditorWindowModuleDropDown"
                };
                m_ModuleDropDownUI.style.display = m_Texture == null || activatedModules?.Count <= 0
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
                var moduleApplyRevertGUI = new IMGUIContainer(DoApplyRevertGUI)
                {
                    name = "spriteEditorWindowApplyRevert"
                };
                m_AlphaZoomToolbarElement = new IMGUIContainer(DoAlphaZoomToolbarGUI)
                {
                    name = "spriteEditorWindowAlphaZoom"
                };
                m_ModuleToolbarIMGUIElement = new IMGUIContainer(DoToolbarGUI)
                {
                    name = "spriteEditorWindowModuleToolbarIMGUI"
                };
                m_ToolbarContainer.Add(m_ModuleDropDownUI);
                m_ToolbarContainer.Add(m_ModuleToolbarIMGUIElement);
                m_ToolbarContainer.Add(m_ModuleToolbarContainer);
                m_ToolbarContainer.Add(moduleApplyRevertGUI);
                m_ToolbarContainer.Add(m_AlphaZoomToolbarElement);
                m_MainViewIMGUIElement = new IMGUIContainer(DoTextureAndModulesGUI)
                {
                    name = "mainViewIMGUIElement"
                };
                m_MainViewElement = new VisualElement
                {
                    name = "spriteEditorWindowMainView"
                };
                m_ModuleViewElement = new VisualElement
                {
                    name = "moduleViewElement",
                    pickingMode = PickingMode.Ignore
                };
                m_MainViewElement.Add(m_MainViewIMGUIElement);
                m_MainViewElement.Add(m_ModuleViewElement);
                var root = rootVisualElement;
                root.styleSheetList.Add(styleSheet);
                m_ToolbarContainer.styleSheets.Add(
                    AssetDatabase.LoadAssetAtPath<StyleSheet>(SpriteEditorWindowStyles.toolBarStyleSheetPath));
                baseRootVisualElement.Insert(0, m_ToolbarContainer);
                root.Add(m_MainViewElement);

                TryGetOverlay("Overlays/OverlayMenu", out var overlay);
                if (overlay != null)
                    overlay.displayed = false;
                return true;
            }

            return false;
        }

        private void UndoRedoPerformed()
        {
            // Was selected texture changed by undo?
            if (selectedProviderChanged)
                RefreshSpriteEditorWindow();

            InitSelectedSpriteRect();

            Repaint();
        }

        private void InitSelectedSpriteRect()
        {
            SpriteRect newSpriteRect = null;
            if (m_RectsCache != null && m_RectsCache.Count > 0)
            {
                if (selectedSpriteRect != null)
                    newSpriteRect = m_RectsCache.FirstOrDefault(x => x.spriteID == selectedSpriteRect.spriteID) != null
                        ? selectedSpriteRect
                        : m_RectsCache[0];
                else
                    newSpriteRect = m_RectsCache[0];
            }

            selectedSpriteRect = newSpriteRect;
        }

        private void OnGUI()
        {
            CreateGUI();
        }

        public override void SaveChanges()
        {
            var oldDelegate = onHandleApplyRevertDialog;
            onHandleApplyRevertDialog = (x, y) => true;
            HandleApplyRevertDialog(SpriteEditorWindowStyles.applyRevertDialogTitle.text,
                string.Format(SpriteEditorWindowStyles.applyRevertDialogContent.text, selectedAssetPath));
            onHandleApplyRevertDialog = oldDelegate;
            base.SaveChanges();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            InvalidatePropertiesCache();
            EditorApplication.modifierKeysChanged -= ModifierKeysChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.quitting -= OnEditorApplicationQuit;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;

            if (m_OutlineTexture != null)
            {
                DestroyImmediate(m_OutlineTexture);
                m_OutlineTexture = null;
            }

            if (m_ReadableTexture)
            {
                DestroyImmediate(m_ReadableTexture);
                m_ReadableTexture = null;
            }

            if (m_CurrentModule != null)
                m_CurrentModule.OnModuleDeactivate();
            UnityEditor.SpriteUtilityWindow.SetApplySpriteEditorWindow(null);
            if (m_MainViewElement != null && rootVisualElement.Contains(m_MainViewElement))
            {
                rootVisualElement.Remove(m_MainViewElement);
                m_MainViewElement = null;
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange playModeState)
        {
            if (PlayModeStateChange.EnteredPlayMode == playModeState ||
                PlayModeStateChange.EnteredEditMode == playModeState) RebuildCache();
        }

        private void OnEditorApplicationQuit()
        {
            HandleApplyRevertDialog(SpriteEditorWindowStyles.applyRevertDialogTitle.text,
                string.Format(SpriteEditorWindowStyles.applyRevertDialogContent.text, selectedAssetPath));
        }

        private void OnBeforeAssemblyReload()
        {
            HandleApplyRevertDialog(SpriteEditorWindowStyles.applyRevertDialogTitle.text,
                string.Format(SpriteEditorWindowStyles.applyRevertDialogContent.text, selectedAssetPath));
        }

        private static bool ShowHandleApplyRevertDialog(string dialogTitle, string dialogContent)
        {
            return EditorUtility.DisplayDialog(dialogTitle, dialogContent,
                SpriteEditorWindowStyles.applyButtonLabel.text, SpriteEditorWindowStyles.revertButtonLabel.text);
        }

        private void HandleApplyRevertDialog(string dialogTitle, string dialogContent)
        {
            if (textureIsDirty && IsSpriteDataProviderValid())
            {
                if (onHandleApplyRevertDialog(dialogTitle, dialogContent))
                    DoApply();
                else
                    DoRevert();

                SetupModule(m_CurrentModuleIndex);
            }
        }

        private bool IsSpriteDataProviderValid()
        {
            return spriteEditorDataProvider != null && !spriteEditorDataProvider.Equals(null);
        }

        private Dictionary<string, Sprite> spriteDic;

        private void RefreshRects()
        {
            spriteRects = null;
            if (IsSpriteDataProviderValid()) m_RectsCache = spriteEditorDataProvider.GetSpriteRects().ToList();

            InitSelectedSpriteRect();
            var targetObject = spriteEditorDataProvider.targetObject;
            if (targetObject is TextureImporter textureImporter)
            {
                var assetPath = textureImporter.assetPath;
                var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToList();
                spriteDic = new Dictionary<string, Sprite>();
                for (var i = 0; i < sprites.Count; i++)
                {
                    var sprite = sprites[i];
                    spriteDic.Add(sprite.name, sprite);
                }
            }
        }

        private void UpdateAssetSelectionChange()
        {
            if (selectedProviderChanged) ResetOnNextRepaint();

            if (m_ResetCommandSent || (UnityEngine.Event.current.type == EventType.ExecuteCommand &&
                                       UnityEngine.Event.current.commandName == k_RefreshOnNextRepaintCommandEvent))
            {
                m_ResetCommandSent = false;
                if (selectedProviderChanged || !IsSpriteDataProviderValid())
                    selectedAssetPath = GetSelectionAssetPath();
                RebuildCache();
            }
        }

        internal void ResetOnNextRepaint()
        {
            //Because we can't show dialog in a repaint/layout event, we need to send event to IMGUI to trigger this.
            //The event is now sent through the Update loop.
            m_ResetOnNextRepaint = true;
            if (textureIsDirty)
            {
                // We can't depend on the existing data provider to set data because a reimport might cause
                // the data provider to be invalid. We store up the current asset path so that in DoApply()
                // the modified data can be set correctly to correct asset.
                if (m_CurrentResetContext != null)
                    Debug.LogError("Existing reset not completed for " + m_CurrentResetContext.assetPath);
                m_CurrentResetContext = new CurrentResetContext
                {
                    assetPath = selectedAssetPath
                };
            }
        }

        private void Update()
        {
            if (m_ResetOnNextRepaint)
            {
                m_ResetOnNextRepaint = false;
                m_ResetCommandSent = true;
                var e = EditorGUIUtility.CommandEvent(k_RefreshOnNextRepaintCommandEvent);
                SendEvent(e);
            }
        }

        private void RebuildCache()
        {
            HandleApplyRevertDialog(SpriteEditorWindowStyles.applyRevertDialogTitle.text,
                SpriteEditorWindowStyles.pendingChangesDialogContent.text);
            ResetWindow();
            RefreshPropertiesCache();
            RefreshRects();
            UpdateAvailableModules();
        }

        private void ShowEditorMessage(string message, MessageType messageType)
        {
            GUILayout.BeginArea(spriteEditorMessageRect);
            EditorGUILayout.HelpBox(message, messageType);
            GUILayout.EndArea();
        }

        private void DoTextureAndModulesGUI()
        {
            // Don't do anything until reset event is sent
            if (m_ResetOnNextRepaint)
                return;
            InitStyles();
            UpdateAssetSelectionChange();
            if (m_ResetCommandSent)
                return;
            textureViewRect = new Rect(0f, 0f, m_MainViewIMGUIElement.layout.width - k_ScrollbarMargin,
                m_MainViewIMGUIElement.layout.height - k_ScrollbarMargin);
            if (!activeDataProviderSelected)
            {
                if (m_Texture != null)
                {
                    DoTextureGUI();
                    ShowEditorMessage(SpriteEditorWindowStyles.selectionNotEditableBySpriteEditor.text,
                        MessageType.Info);
                }
                else
                {
                    ShowEditorMessage(SpriteEditorWindowStyles.noSelectionWarning.text, MessageType.Info);
                }

                return;
            }

            if (m_CurrentModule == null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Label(SpriteEditorWindowStyles.noModuleWarning);
                }

                return;
            }

            textureViewRect = new Rect(0f, 0f, m_MainViewIMGUIElement.layout.width - k_ScrollbarMargin,
                m_MainViewIMGUIElement.layout.height - k_ScrollbarMargin);
            var oldHandlesMatrix = Handles.matrix;
            DoTextureGUI();
            // Warning message if applicable
            DoEditingDisabledMessage();
            m_CurrentModule.DoPostGUI();
            Handles.matrix = oldHandlesMatrix;
            if (m_RequestRepaint)
            {
                Repaint();
                m_RequestRepaint = false;
            }
        }

        protected override void DoTextureGUIExtras()
        {
            if (activeDataProviderSelected)
            {
                HandleFrameSelected();

                if (m_EventSystem.current.type == EventType.Repaint)
                {
                    SpriteEditorUtility.BeginLines(new Color(1f, 1f, 1f, 0.5f));
                    var selectedRect = selectedSpriteRect != null ? selectedSpriteRect.spriteID : new GUID();
                    for (var i = 0; i < m_RectsCache.Count; i++)
                        if (m_RectsCache[i].spriteID != selectedRect)
                            SpriteEditorUtility.DrawBox(m_RectsCache[i].rect);

                    SpriteEditorUtility.EndLines();
                }

                m_CurrentModule.DoMainGUI();
            }
        }

        private void DoModuleDropDownGUI()
        {
            InitStyles();

            if (!activeDataProviderSelected || m_CurrentModule == null)
                return;

            if (activatedModules.Count > 1)
            {
                var module = EditorGUILayout.Popup(m_CurrentModuleIndex, m_RegisteredModuleNames,
                    EditorStyles.toolbarPopup);
                if (module != m_CurrentModuleIndex)
                {
                    if (textureIsDirty)
                    {
                        // Have pending module edit changes. Ask user if they want to apply or revert
                        if (EditorUtility.DisplayDialog(SpriteEditorWindowStyles.applyRevertModuleDialogTitle.text,
                                SpriteEditorWindowStyles.applyRevertModuleDialogContent.text,
                                SpriteEditorWindowStyles.applyButtonLabel.text,
                                SpriteEditorWindowStyles.revertButtonLabel.text))
                            DoApply();
                        else
                            DoRevert();
                    }

                    SetupModule(module);
                }
            }
        }

        private void DoApplyRevertGUI()
        {
            InitStyles();
            GUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(!textureIsDirty))
            {
                if (GUILayout.Button(SpriteEditorWindowStyles.revertButtonLabel, EditorStyles.toolbarButton))
                {
                    var revert = true;
                    if (SpriteEditorWindowSettings.showRevertConfirmation)
                        revert = EditorUtility.DisplayDialog(
                            SpriteEditorWindowStyles.revertConfirmationDialogTitle.text,
                            SpriteEditorWindowStyles.revertConfirmationDialogContent.text,
                            SpriteEditorWindowStyles.yesLabel.text, SpriteEditorWindowStyles.noLabel.text);
                    if (revert)
                    {
                        DoRevert();
                        SetupModule(m_CurrentModuleIndex);
                    }
                }

                if (GUILayout.Button(SpriteEditorWindowStyles.applyButtonLabel, EditorStyles.toolbarButton))
                {
                    var apply = true;
                    if (SpriteEditorWindowSettings.showApplyConfirmation)
                        apply = EditorUtility.DisplayDialog(SpriteEditorWindowStyles.applyConfirmationDialogTitle.text,
                            SpriteEditorWindowStyles.applyConfirmationDialogContent.text,
                            SpriteEditorWindowStyles.yesLabel.text, SpriteEditorWindowStyles.noLabel.text);
                    if (apply)
                    {
                        DoApply();
                        SetupModule(m_CurrentModuleIndex);
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DoAlphaZoomToolbarGUI()
        {
            InitStyles();
            var toolbarRect = new Rect(0, 0, m_AlphaZoomToolbarElement.resolvedStyle.width,
                m_AlphaZoomToolbarElement.resolvedStyle.height);
            DoAlphaZoomToolbarGUI(toolbarRect);
        }

        private void DoToolbarGUI()
        {
            InitStyles();
            var toolbarRect = new Rect(0, 0, m_ModuleToolbarIMGUIElement.resolvedStyle.width,
                m_ModuleToolbarIMGUIElement.resolvedStyle.height);
            m_CurrentModule?.DoToolbarGUI(toolbarRect);
        }

        private void DoEditingDisabledMessage()
        {
            if (editingDisabled)
            {
                var disableMessage = m_AssetNotEditable
                    ? SpriteEditorWindowStyles.editingDisableMessageBecauseNonEditableLabel.text
                    : SpriteEditorWindowStyles.editingDisableMessageBecausePlaymodeLabel.text;
                ShowEditorMessage(disableMessage, MessageType.Warning);
            }
        }

        private void DoApply()
        {
            textureIsDirty = false;
            var reimport = true;
            var dataProvider = spriteEditorDataProvider;
            if (m_CurrentResetContext != null)
            {
                spriteEditorDataProvider =
                    m_SpriteDataProviderFactories.GetSpriteEditorDataProviderFromObject(
                        AssetDatabase.LoadMainAssetAtPath(m_CurrentResetContext.assetPath));
                spriteEditorDataProvider.InitSpriteEditorDataProvider();
                m_CurrentResetContext = null;
            }

            if (spriteEditorDataProvider != null)
            {
                if (m_CurrentModule != null)
                    reimport = m_CurrentModule.ApplyRevert(true);
                spriteEditorDataProvider.Apply();
            }

            spriteEditorDataProvider = dataProvider;
            // Do this so that asset change save dialog will not show
            var originalValue = EditorPrefs.GetBool("VerifySavingAssets", false);
            EditorPrefs.SetBool("VerifySavingAssets", false);
            AssetDatabase.ForceReserializeAssets(new[] { selectedAssetPath },
                ForceReserializeAssetsOptions.ReserializeMetadata);
            EditorPrefs.SetBool("VerifySavingAssets", originalValue);

            if (reimport)
                DoTextureReimport(selectedAssetPath);
            Repaint();
            RefreshRects();
        }

        private void DoRevert()
        {
            textureIsDirty = false;
            RefreshRects();
            GUI.FocusControl("");
            if (m_CurrentModule != null)
                m_CurrentModule.ApplyRevert(false);
        }


        private Vector2 m_DragStartPos;
        private bool m_IsDragging;

        public bool HandleSpriteSelection()
        {
            var changed = false;

            if (m_EventSystem.current.type == EventType.MouseDown && m_EventSystem.current.button == 0 &&
                GUIUtility.hotControl == 0 && !m_EventSystem.current.alt)
            {
                var oldSelected = selectedSpriteRect;

                var triedRect = TrySelect(m_EventSystem.current.mousePosition);
                if (triedRect != oldSelected)
                {
                    Undo.RegisterCompleteObjectUndo(this, "Sprite Selection");

                    selectedSpriteRect = triedRect;
                    changed = true;
                }

                if (selectedSpriteRect != null)
                    s_OneClickDragStarted = true;
                else
                    RequestRepaint();

                if (changed && selectedSpriteRect != null)
                {
                    m_DragStartPos = m_EventSystem.current.mousePosition;
                    m_EventSystem.current.Use();
                }
            }

            if (m_EventSystem.current.type == EventType.MouseDrag && selectedSpriteRect != null &&
                spriteDic != null && spriteDic.TryGetValue(selectedSpriteRect.name, out var sprite))
                //if (!m_IsDragging)
            {
                // m_IsDragging = true;

                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] { sprite };
                DragAndDrop.StartDrag(sprite.name);
                m_EventSystem.current.Use();
            }

            // (m_EventSystem.current.type == EventType.MouseUp) m_IsDragging = false;

            return changed;
        }

        private void HandleFrameSelected()
        {
            var evt = m_EventSystem.current;

            if ((evt.type == EventType.ValidateCommand || evt.type == EventType.ExecuteCommand)
                && evt.commandName == EventCommandNames.FrameSelected)
            {
                if (evt.type == EventType.ExecuteCommand)
                {
                    // Do not do frame if there is none selected
                    if (selectedSpriteRect == null)
                        return;

                    var rect = selectedSpriteRect.rect;

                    // Calculate the require pixel to display the frame, then get the zoom needed.
                    var targetZoom = m_Zoom;
                    if (rect.width < rect.height)
                        targetZoom = textureViewRect.height /
                                     (rect.height + textureViewRect.height * k_MarginForFraming);
                    else
                        targetZoom = textureViewRect.width / (rect.width + textureViewRect.width * k_MarginForFraming);

                    // Apply the zoom
                    zoomLevel = targetZoom;

                    // Calculate the scroll values to center the frame
                    m_ScrollPosition.x = (rect.center.x - m_Texture.width * 0.5f) * m_Zoom;
                    m_ScrollPosition.y = (rect.center.y - m_Texture.height * 0.5f) * m_Zoom * -1.0f;

                    Repaint();
                }

                evt.Use();
            }
        }

        private void UpdateSelectedSpriteRect(Sprite sprite)
        {
            if (m_RectsCache == null || sprite == null || sprite.Equals(null))
                return;

            var spriteGUID = sprite.GetSpriteID();
            for (var i = 0; i < m_RectsCache.Count; i++)
                if (spriteGUID == m_RectsCache[i].spriteID)
                {
                    selectedSpriteRect = m_RectsCache[i];
                    return;
                }

            selectedSpriteRect = null;
        }

        private SpriteRect TrySelect(Vector2 mousePosition)
        {
            var selectedSize = float.MaxValue;
            SpriteRect currentRect = null;
            mousePosition = Handles.inverseMatrix.MultiplyPoint(mousePosition);

            for (var i = 0; i < m_RectsCache.Count; i++)
            {
                var sr = m_RectsCache[i];
                if (sr.rect.Contains(mousePosition))
                {
                    // If we clicked inside an already selected spriterect, always persist that selection
                    if (sr == selectedSpriteRect)
                        return sr;

                    var width = sr.rect.width;
                    var height = sr.rect.height;
                    var newSize = width * height;
                    if (width > 0f && height > 0f && newSize < selectedSize)
                    {
                        currentRect = sr;
                        selectedSize = newSize;
                    }
                }
            }

            return currentRect;
        }

        public void DoTextureReimport(string path)
        {
            if (spriteEditorDataProvider != null)
                try
                {
                    AssetDatabase.StartAssetEditing();
                    AssetDatabase.ImportAsset(path);
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
        }

        private GUIContent[] m_RegisteredModuleNames;
        private List<SpriteEditorModuleBase> m_AllRegisteredModules;
        private SpriteEditorModuleBase m_CurrentModule;
        private int m_CurrentModuleIndex;
        [SerializeField] private string m_LastUsedModuleTypeName;

        internal void SetupModule(int newModuleIndex)
        {
            if (m_CurrentModule != null)
                m_CurrentModule.OnModuleDeactivate();

            m_CurrentModule = null;
            m_ModuleViewElement.Clear();
            m_ModuleToolbarContainer.Clear();

            if (activatedModules.Count > newModuleIndex && newModuleIndex >= 0)
            {
                m_CurrentModuleIndex = newModuleIndex;
                m_CurrentModule = activatedModules[newModuleIndex];
                // if there are any modes for the module
                List<Type> modes;
                if (m_CurrentModule is SpriteEditorModuleModeSupportBase moduleWithModes &&
                    m_ModuleMode.TryGetValue(moduleWithModes.GetType(), out modes))
                    moduleWithModes.SetModuleModes(modes);
                m_LastUsedModuleTypeName = m_CurrentModule.GetType().FullName;
                m_CurrentModule.OnModuleActivate();
            }

            if (m_MainViewElement != null)
                m_MainViewElement.MarkDirtyRepaint();
            if (m_ModuleViewElement != null)
                m_ModuleViewElement.MarkDirtyRepaint();
        }

        private void UpdateAvailableModules()
        {
            if (m_AllRegisteredModules == null)
                return;
            activatedModules = new List<SpriteEditorModuleBase>();
            var lastUsedModuleIndex = -1;
            if (activeDataProviderSelected)
            {
                foreach (var module in m_AllRegisteredModules)
                    if (module.CanBeActivated())
                    {
                        RequireSpriteDataProviderAttribute attribute = null;
                        m_ModuleRequireSpriteDataProvider.TryGetValue(module.GetType(), out attribute);
                        if (attribute == null || attribute.ContainsAllType(spriteEditorDataProvider))
                            activatedModules.Add(module);
                    }

                lastUsedModuleIndex = 0;
                m_RegisteredModuleNames = new GUIContent[activatedModules.Count];
                for (var i = 0; i < activatedModules.Count; i++)
                {
                    m_RegisteredModuleNames[i] = new GUIContent(activatedModules[i].moduleName);
                    if (activatedModules[i].GetType().FullName.Equals(m_LastUsedModuleTypeName))
                        lastUsedModuleIndex = i;
                }
            }

            if (m_ModuleDropDownUI != null)
            {
                m_ModuleDropDownUI.style.display =
                    m_RegisteredModuleNames?.Length > 1 ? DisplayStyle.Flex : DisplayStyle.None;
                m_ModuleDropDownUI.style.position =
                    m_RegisteredModuleNames?.Length > 1 ? Position.Relative : Position.Absolute;
            }

            SetupModule(lastUsedModuleIndex);
        }

        private void InitModules()
        {
            m_AllRegisteredModules = new List<SpriteEditorModuleBase>();
            m_ModuleRequireSpriteDataProvider.Clear();
            m_ModuleMode.Clear();

            if (m_OutlineTexture == null)
            {
                m_OutlineTexture = new UnityTexture2D(1, 16, TextureFormat.RGBA32, false);
                m_OutlineTexture.SetPixels(new[]
                {
                    new Color(0.5f, 0.5f, 0.5f, 0.5f), new Color(0.5f, 0.5f, 0.5f, 0.5f),
                    new Color(0.8f, 0.8f, 0.8f, 0.8f), new Color(0.8f, 0.8f, 0.8f, 0.8f),
                    Color.white, Color.white, Color.white, Color.white,
                    new Color(.8f, .8f, .8f, 1f), new Color(.5f, .5f, .5f, .8f), new Color(0.3f, 0.3f, 0.3f, 0.5f),
                    new Color(0.3f, .3f, 0.3f, 0.5f),
                    new Color(0.3f, .3f, 0.3f, 0.3f), new Color(0.3f, .3f, 0.3f, 0.3f),
                    new Color(0.1f, 0.1f, 0.1f, 0.1f), new Color(0.1f, .1f, 0.1f, 0.1f)
                });
                m_OutlineTexture.Apply();
                m_OutlineTexture.hideFlags = HideFlags.HideAndDontSave;
            }

            var outlineTexture = new Texture2DWrapper(m_OutlineTexture);

            // Add your modules here
            RegisterModule(new SpriteFrameModule(this, m_EventSystem, m_UndoSystem, m_AssetDatabase));
            RegisterModule(new SpritePolygonModeModule(this, m_EventSystem, m_UndoSystem, m_AssetDatabase));
            RegisterModule(new SpriteOutlineModule(this, m_EventSystem, m_UndoSystem, m_AssetDatabase, m_GUIUtility,
                new ShapeEditorFactory(), outlineTexture));
            RegisterModule(new SpritePhysicsShapeModule(this, m_EventSystem, m_UndoSystem, m_AssetDatabase,
                m_GUIUtility, new ShapeEditorFactory(), outlineTexture));
            RegisterCustomModules();
            UpdateAvailableModules();
        }

        private void RegisterModule(SpriteEditorModuleBase module)
        {
            var type = module.GetType();
            var showAsModule = true;
            if (module is SpriteEditorModeBase)
            {
                var moduleMode = type.GetCustomAttributes(typeof(SpriteEditorModuleModeAttribute), false);
                if (moduleMode.Length == 1)
                {
                    var modeAttribute = (SpriteEditorModuleModeAttribute)moduleMode[0];
                    showAsModule = modeAttribute.showAsModule;
                    foreach (var modeForModule in modeAttribute.GetModuleTypes())
                    {
                        List<Type> modulesList;
                        if (!m_ModuleMode.TryGetValue(modeForModule, out modulesList))
                            modulesList = new List<Type>();
                        modulesList.Add(module.GetType());
                        m_ModuleMode[modeForModule] = modulesList;
                    }
                }
            }

            if (showAsModule)
            {
                var attributes = type.GetCustomAttributes(typeof(RequireSpriteDataProviderAttribute), false);
                if (attributes.Length == 1)
                    m_ModuleRequireSpriteDataProvider.Add(type, (RequireSpriteDataProviderAttribute)attributes[0]);
                m_AllRegisteredModules.Add(module);
            }
        }

        private void RegisterCustomModules()
        {
            foreach (var moduleClassType in TypeCache.GetTypesDerivedFrom<SpriteEditorModuleBase>())
                if (!moduleClassType.IsAbstract)
                {
                    var moduleFound = false;
                    foreach (var module in m_AllRegisteredModules)
                        if (module.GetType() == moduleClassType)
                        {
                            moduleFound = true;
                            break;
                        }

                    if (!moduleFound)
                    {
                        var constructorType = new Type[0];
                        // Get the public instance constructor that takes ISpriteEditorModule parameter.
                        var constructorInfoObj = moduleClassType.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public, null,
                            CallingConventions.HasThis, constructorType, null);
                        if (constructorInfoObj != null)
                            try
                            {
                                var newInstance = constructorInfoObj.Invoke(new object[0]) as SpriteEditorModuleBase;
                                if (newInstance != null)
                                {
                                    newInstance.spriteEditor = this;
                                    RegisterModule(newInstance);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning("Unable to instantiate custom module " + moduleClassType.FullName +
                                                 ". Exception:" + ex);
                            }
                        else
                            Debug.LogWarning(moduleClassType.FullName + " does not have a parameterless constructor");
                    }
                }
        }

        internal List<SpriteEditorModuleBase> activatedModules { get; private set; }

        public List<SpriteRect> spriteRects
        {
            set
            {
                m_RectsCache = value;
                m_CachedSelectedSpriteRect = null;
            }
        }

        private SpriteRect m_CachedSelectedSpriteRect;

        public SpriteRect selectedSpriteRect
        {
            get
            {
                // Always return null if editing is disabled to prevent all possible action to selected frame.
                if (editingDisabled || m_RectsCache == null || string.IsNullOrEmpty(m_SelectedSpriteRectGUID))
                    return null;

                var guid = new GUID(m_SelectedSpriteRectGUID);
                if (m_CachedSelectedSpriteRect == null || m_CachedSelectedSpriteRect.spriteID != guid)
                    m_CachedSelectedSpriteRect = m_RectsCache.FirstOrDefault(x => x.spriteID == guid);
                return m_CachedSelectedSpriteRect;
            }
            set
            {
                if (editingDisabled)
                    return;

                var oldSelected = m_SelectedSpriteRectGUID;
                m_SelectedSpriteRectGUID = value != null ? value.spriteID.ToString() : new GUID().ToString();
                if (oldSelected != m_SelectedSpriteRectGUID)
                {
                    if (m_MainViewIMGUIElement != null)
                        m_MainViewIMGUIElement.MarkDirtyRepaint();
                    if (m_MainViewElement != null)
                    {
                        m_MainViewElement.MarkDirtyRepaint();
                        using (var e = SpriteSelectionChangeEvent.GetPooled())
                        {
                            e.target = m_ModuleViewElement;
                            m_MainViewElement.SendEvent(e);
                        }
                    }
                }
            }
        }

        public ISpriteEditorDataProvider spriteEditorDataProvider { get; private set; }

        public bool enableMouseMoveEvent
        {
            set => wantsMouseMove = value;
        }

        public void RequestRepaint()
        {
            if (focusedWindow != this)
                Repaint();
            else
                m_RequestRepaint = true;
        }

        public void SetDataModified()
        {
            textureIsDirty = true;
        }

        public Rect windowDimension => textureViewRect;

        public ITexture2D previewTexture => m_Texture;

        public bool editingDisabled => EditorApplication.isPlayingOrWillChangePlaymode || m_AssetNotEditable;

        public void SetPreviewTexture(UnityTexture2D texture, int width, int height)
        {
            m_Texture = new PreviewTexture2D(texture, width, height);
        }

        public void ApplyOrRevertModification(bool apply)
        {
            if (apply)
                DoApply();
            else
                DoRevert();
        }

        internal class PreviewTexture2D : Texture2DWrapper
        {
            private readonly int m_ActualWidth;
            private readonly int m_ActualHeight;

            public PreviewTexture2D(UnityTexture2D t, int width, int height)
                : base(t)
            {
                m_ActualWidth = width;
                m_ActualHeight = height;
            }

            public override int width => m_ActualWidth;

            public override int height => m_ActualHeight;
        }

        public T GetDataProvider<T>() where T : class
        {
            return spriteEditorDataProvider != null ? spriteEditorDataProvider.GetDataProvider<T>() : null;
        }

        public VisualElement GetMainVisualContainer()
        {
            return m_ModuleViewElement;
        }

        public VisualElement GetToolbarRootElement()
        {
            return m_ModuleToolbarContainer;
        }

        internal static void OnTextureReimport(SpriteEditorWindow win, string path)
        {
            if (win.selectedAssetPath == path) win.ResetOnNextRepaint();
        }

        [MenuItem("Window/2D/Sprite Editor", priority = 0)]
        private static void OpenSpriteEditorWindow()
        {
            GetWindow(Selection.activeObject);
        }
    }

    internal class SpriteEditorTexturePostprocessor : AssetPostprocessor
    {
        public override int GetPostprocessOrder()
        {
            return 1;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            var wins = Resources.FindObjectsOfTypeAll(typeof(SpriteEditorWindow));
            var win = wins.Length > 0 ? (EditorWindow)wins[0] as SpriteEditorWindow : null;
            if (win != null)
            {
                foreach (var deletedAsset in deletedAssets)
                    SpriteEditorWindow.OnTextureReimport(win, deletedAsset);

                var rebuildTracker = false;
                var assetPaths = ActiveEditorTracker.sharedTracker.activeEditors.Where(x => x.target is AssetImporter)
                    .Select(y => ((AssetImporter)y.target).assetPath);
                foreach (var importedAsset in importedAssets)
                {
                    SpriteEditorWindow.OnTextureReimport(win, importedAsset);
                    if (!rebuildTracker) rebuildTracker = assetPaths.Contains(importedAsset);
                }

                // Since Inspector Window doesn't rebuild anymore, we need to do it manually (https://github.cds.internal.unity3d.com/unity/unity/pull/13828)
                if (rebuildTracker)
                    ActiveEditorTracker.sharedTracker.ForceRebuild();
            }
        }
    }

    internal class SpriteSelectionChangeEvent : EventBase<SpriteSelectionChangeEvent>
    {
    }
}