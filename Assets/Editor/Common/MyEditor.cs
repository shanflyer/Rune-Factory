using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ObjSearchType
{
    名字搜索, Tag搜索
}

public enum DisplaySortType
{
    升序 = 1, 降序 = -1
}

public enum ObjDisplayType
{
    名字, Id
}

public class MyEditor : EditorWindow
{
    public bool animationRecording;
    public static bool canSelect = true;

    public delegate void DropDownObjEvent<T, TT>(T sourceObj, TT valueObj);

    public delegate void DropDownEvent<T>(T obj);

    public delegate void ButtonEvent();

    public delegate void ButtonEvent<T>(T obj);

    public delegate void ToggleEvent(bool value);

    public delegate void ToggleObjEvent<T>(bool value, T obj);

    public delegate void SelectToggleEvent<T>(T obj);

    public delegate void SelectEnum(Enum selected);

    public delegate void SelectEnumObj<T>(Enum selected, T obj);

    public delegate void FloatEvent(float value);

    public delegate void FloatEvent<T>(float value, T obj);

    public delegate void IntEvent(int value);

    public delegate void IntFieldObjEvent<T>(int value, T obj);

    public delegate void TextEvent<T>(string value, T obj);

    public delegate void TextEvent(string value);

    public delegate void ColorFieldEvent(Color color);

    public delegate void Vector2Event(Vector2 value);

    public virtual void SetLight(bool ON)
    {
    }

    public virtual void HideShadow(bool hide)
    {
    }

    public virtual void SetMuSelect(bool mu)
    {
    }

    public static void Initexture(string path)
    {
        TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;

        try
        {
            if (ti != null)
            {
                ti.mipmapEnabled = false;

                ti.filterMode = FilterMode.Point;
                ti.compressionQuality = 100;
                ti.crunchedCompression = false;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.SaveAndReimport();
                //AssetDatabase.ImportAsset(path);
            }
        }
        catch
        {
        }

        //
    }

    public static void InitPCTexture(string path, TextureImporterFormat textureImporterFormat)
    {
        Debug.Log("path:" + path);
        TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;

        try
        {
            if (ti != null)
            {
                ti.mipmapEnabled = false;

                ti.filterMode = FilterMode.Point;
                TextureImporterPlatformSettings importerSettings_PC = new TextureImporterPlatformSettings();
                importerSettings_PC.name = "Standalone";
                importerSettings_PC.overridden = true;
                importerSettings_PC.format = textureImporterFormat;
                importerSettings_PC.compressionQuality = 100;
                ti.SetPlatformTextureSettings(importerSettings_PC);

                ti.SaveAndReimport();
                //AssetDatabase.ImportAsset(path);
            }
        }
        catch
        {
        }

        //
    }

    public float GetNowTime()
    {
        return 0;
    }

    public void OnDestroy()
    {
        MyEditor.canSelect = true;
    }

    public static void DrawEnum(Enum selected, string title, SelectEnum selectEnum, int titleWidth = 60, int Width = 60, int height = 20)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var _type = EditorGUILayout.EnumPopup(selected, GUILayout.Width(Width), GUILayout.Height(height));
        if (_type.ToString() != selected.ToString())
        {
            selectEnum(_type);
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void DrawEnumObj<T>(Enum selected, string title, SelectEnumObj<T> selectEnum, T obj, int titleWidth = 60, int Width = 60, int height = 20)
    {
        EditorGUILayout.LabelField(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var _type = EditorGUILayout.EnumPopup(selected, GUILayout.Width(Width), GUILayout.Height(height));
        if (_type.ToString() != selected.ToString())
        {
            selectEnum(_type, obj);
        }
    }

    public static void DrawToggleEnumGUI(Enum selected, SelectEnum selectEnumEvent, bool vertial, int posX, int posY, int width = 40, int height = 20)
    {
        var enumTypes = selected.GetType().GetEnumValues();
        Vector2 pos = new Vector2(posX, posY);

        foreach (var t in enumTypes)
        {
            //int index = i;
            bool select = selected.ToString() == t.ToString();
            var _select = GUI.Toggle(new Rect(pos, new Vector2(width, height)), select, t.ToString(), CommonItem.selectButton);
            if (select != _select && _select)
            {
                selectEnumEvent((Enum)t);
            }
            if (vertial)
            {
                pos.y += height;
            }
            else
            {
                pos.x += width;
            }
        }
    }

    public static void DrawToggleGroupGUI<T>(List<T> objs, List<string> titles, int nowIndex,
        SelectToggleEvent<T> selectToggleEvent, bool vertial, int posX, int posY, int width = 40, int height = 20)
    {
        Vector2 pos = new Vector2(posX, posY);
        for (int i = 0; i < objs.Count; i++)
        {
            int index = i;
            bool select = index == nowIndex;
            var _select = GUI.Toggle(new Rect(pos, new Vector2(width, height)), select, titles[index]);
            if (select != _select && _select)
            {
                selectToggleEvent(objs[index]);
            }
            if (vertial)
            {
                pos.y += height;
            }
            else
            {
                pos.x += width;
            }
        }
    }

    public static void DrawToggleGroup<T>(List<T> objs, List<string> titles, int nowIndex, SelectToggleEvent<T> selectToggleEvent, bool vertial, int width = 40, int heigth = 20)
    {
        if (vertial)
        {
            EditorGUILayout.BeginVertical(GUI.skin.button);
        }
        else
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.button);
        }
        for (int i = 0; i < objs.Count; i++)
        {
            int index = i;
            bool select = index == nowIndex;
            var _select = GUILayout.Toggle(select, titles[index], GUILayout.Width(width), GUILayout.Height(heigth));
            if (select != _select && _select)
            {
                selectToggleEvent(objs[index]);
            }
        }
        if (vertial)
        {
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.EndHorizontal();
        }
    }

    public static void DrawColorField(Color color, string title, ColorFieldEvent colorFieldEvent, bool HDR = false, int titleWidth = 40, int width = 40, int height = 20)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Width(titleWidth + width));
        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var value = EditorGUILayout.ColorField(color, GUILayout.Width(width), GUILayout.Height(height));
        if (value != color)
        {
            colorFieldEvent(value);
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void DrawTextField(ref string value, string title, int titleWidth = 40, int width = 40, int height = 20)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Width(titleWidth + width));
        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        value = EditorGUILayout.TextField(value, GUILayout.Width(width), GUILayout.Height(height));
        EditorGUILayout.EndHorizontal();
    }

    public static void DrawTextField(string value, string title, TextEvent textEvent, int titleWidth = 40, int width = 40, int height = 20)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Width(titleWidth + width));
        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var _value = EditorGUILayout.TextField(value, GUILayout.Width(width), GUILayout.Height(height));
        if (_value != value)
        {
            textEvent(_value);
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void DrawTextField<T>(string value, string title, TextEvent<T> textEvent, T obj, int titleWidth = 40, int width = 40, int height = 20)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Width(titleWidth + width));
        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var _value = EditorGUILayout.TextField(value, GUILayout.Width(width), GUILayout.Height(height));
        if (_value != value)
        {
            textEvent(_value, obj);
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void DrawTextField<T>(string value, TextEvent<T> textEvent, T obj, int width = 40, int height = 20)
    {
        var _value = EditorGUILayout.TextField(value, GUILayout.Width(width), GUILayout.Height(height));
        if (_value != value)
        {
            textEvent(_value, obj);
        }
    }

    public static void DrawLableButton(string title, int width = 40, int height = 20)
    {
        EditorGUILayout.LabelField(title, GUI.skin.button, GUILayout.Width(width), GUILayout.Height(height));
    }

    public static void DrawLable(string title, int width = 40, int height = 20)
    {
        EditorGUILayout.LabelField(title, GUILayout.Width(width), GUILayout.Height(height));
    }

    public static void DrawIntField(ref int value, int width = 40, int height = 20)
    {
        value = EditorGUILayout.IntField(value, GUILayout.Width(40), GUILayout.Height(20));
    }

    public static void DrawFloatField(float value, FloatEvent floatField, int width = 40, int height = 20)
    {
        var _value = EditorGUILayout.FloatField(value, GUILayout.Width(40), GUILayout.Height(20));
        if (_value != value)
        {
            floatField(_value);
        }
    }

    public static void DrawFloatField(string title, float value, FloatEvent floatField, int titleWidth = 40, int width = 40, int height = 20)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Width(titleWidth + width));
        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var _value = EditorGUILayout.FloatField(value, GUILayout.Width(40), GUILayout.Height(20));
        if (_value != value)
        {
            floatField(_value);
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void DrawSlider(float value, float leftValue, float rightValue, FloatEvent floatField, int width = 40, int height = 20)
    {
        var _value = EditorGUILayout.Slider(value, leftValue, rightValue, GUILayout.Width(40), GUILayout.Height(20));
        if (_value != value)
        {
            floatField(_value);
        }
    }

    public static void DrawSlider(string title, int value, int leftValue, int rightValue, IntEvent intField, int titleWidth = 40, int width = 40, int height = 20)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Width(titleWidth + width));
        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var _value = EditorGUILayout.Slider(value, leftValue, rightValue, GUILayout.Width(40), GUILayout.Height(20));
        if (_value != value)
        {
            intField((int)(_value));
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void DrawSlider<T>(float value, float leftValue, float rightValue, T obj, FloatEvent<T> floatField, int width = 40, int height = 20)
    {
        var _value = EditorGUILayout.Slider(value, leftValue, rightValue, GUILayout.Width(width), GUILayout.Height(20));
        if (_value != value)
        {
            floatField(_value, obj);
        }
    }

    public static void DrawVector2(string title, Vector2 value, Vector2Event vector2Field, int titleWidth = 40, int width = 40, int height = 20)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var _value = EditorGUILayout.Vector2Field("", value, GUILayout.Width(width), GUILayout.Height(20));
        if (_value != value)
        {
            vector2Field(_value);
        }
        GUILayout.EndHorizontal();
    }

    public static void DrawSlider(string title, float value, float leftValue, float rightValue, FloatEvent floatField, int titleWidth = 40, int width = 40, int height = 20)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var _value = EditorGUILayout.Slider(value, leftValue, rightValue, GUILayout.Width(width), GUILayout.Height(20));
        if (_value != value)
        {
            floatField(_value);
        }
        GUILayout.EndHorizontal();
    }

    public static void DrawSlider(string title, float value, ref float leftValue, ref float rightValue, FloatEvent floatField, bool resetMinValue = false, bool resetMaxValue = false, int titleWidth = 40, int width = 40, int height = 20)
    {
        GUILayout.BeginHorizontal();
        if (resetMinValue)
        {
            leftValue = EditorGUILayout.FloatField(leftValue, GUILayout.Width(30));
        }

        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var _value = EditorGUILayout.Slider(value, leftValue, rightValue, GUILayout.Width(width), GUILayout.Height(20));
        if (_value != value)
        {
            floatField(_value);
        }

        if (resetMaxValue)
        {
            rightValue = EditorGUILayout.FloatField(rightValue, GUILayout.Width(30));
        }
        GUILayout.EndHorizontal();
    }

    public static void DrawIntField<T>(int value, T obj, string title, IntFieldObjEvent<T> intFieldObjEvent, int titleWidth = 40, int width = 40, int height = 20)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        var _value = EditorGUILayout.IntField(value, GUILayout.Width(width), GUILayout.Height(height));
        if (_value != value)
        {
            intFieldObjEvent(_value, obj);
        }
        GUILayout.EndHorizontal();
    }

    public static void DrawIntField(ref int value, string title, int titleWidth = 40, int width = 40, int height = 20)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(title, GUILayout.Width(titleWidth), GUILayout.Height(height));
        value = EditorGUILayout.IntField(value, GUILayout.Width(width), GUILayout.Height(height));
        EditorGUILayout.EndHorizontal();
    }

    public static void DrawToggle(bool nowValue, string title, ToggleEvent toggleEvent, bool buttonStyle = false, bool CheckSelect = false, int width = 100, int height = 20)
    {
        bool value = GUILayout.Toggle(nowValue, title, buttonStyle ? CommonItem.selectButton : GUI.skin.toggle, GUILayout.Width(width), GUILayout.Height(height));
        if (value != nowValue)
        {
            if (!CheckSelect || MyEditor.canSelect)
            {
                toggleEvent(value);
            }
        }
    }

    public static void DrawToggle<T>(bool nowValue, string title, ToggleObjEvent<T> toggleEvent, T obj, GUIStyle gUIStyle, bool CheckSelect = false, int width = 100, int height = 20)
    {
        bool value = GUILayout.Toggle(nowValue, title, gUIStyle, GUILayout.Width(width), GUILayout.Height(height));
        if (value != nowValue)
        {
            if (!CheckSelect || canSelect)
            {
                toggleEvent(value, obj);
            }
        }
    }

    public static void DrawToggle<T>(bool nowValue, string title, ToggleObjEvent<T> toggleEvent, T obj, bool buttonStyle = false, bool CheckSelect = false, int width = 100, int height = 20)
    {
        bool value = GUILayout.Toggle(nowValue, title, buttonStyle ? CommonItem.selectButton : GUI.skin.toggle, GUILayout.Width(width), GUILayout.Height(height));
        if (value != nowValue)
        {
            if (!CheckSelect || canSelect)
            {
                toggleEvent(value, obj);
            }
        }
    }

    public static void DrawButton<T>(string title, T obj, ButtonEvent<T> buttonEvent, int width, int height = 20, bool checkSelect = false)
    {
        if (GUILayout.Button(title, GUILayout.Width(width), GUILayout.Height(height)))
        {
            if (!checkSelect || canSelect)
            {
                buttonEvent(obj);
            }
        }
    }

    public static void DrawButton(string title, ButtonEvent buttonEvent, int width, int height = 20, bool checkSelect = false)
    {
        if (GUILayout.Button(title, GUILayout.Width(width), GUILayout.Height(height)))
        {
            if (!checkSelect || canSelect)
            {
                buttonEvent();
            }
        }
    }

    public static void DrawDropdownObjButton<T, TT>(List<T> objects, string nowTitle, DropDownObjEvent<T, TT> _dropEvent, TT sourceObj, int width, int height = 20)
    {
        if (EditorGUILayout.DropdownButton(new GUIContent(nowTitle), FocusType.Keyboard, GUILayout.Width(width), GUILayout.Height(height)))
        {
            GenericMenu _menu = new GenericMenu();
            for (int i = 0; i < objects.Count; i++)
            {
                var item = objects[i].ToString();
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                var obj = objects[i];
                //添加菜单
                _menu.AddItem(new GUIContent(item), nowTitle.Equals(item), (object value) =>
                {
                    if (canSelect)
                    {
                        _dropEvent(obj, sourceObj);
                    }
                }, item);
            }

            _menu.ShowAsContext();//显示菜单
        }
    }

    public static void DrawDropdownButton<T>(List<T> objects, List<string> titles, string nowTitle, DropDownEvent<T> _dropEvent, int width, int height = 20)
    {
        if (EditorGUILayout.DropdownButton(new GUIContent(nowTitle), FocusType.Keyboard, GUILayout.Width(width), GUILayout.Height(height)))
        {
            GenericMenu _menu = new GenericMenu();
            for (int i = 0; i < titles.Count; i++)
            {
                var item = titles[i];
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                var obj = objects[i];
                //添加菜单
                _menu.AddItem(new GUIContent(item), nowTitle.Equals(item), (object value) =>
                {
                    if (canSelect)
                    {
                        _dropEvent(obj);
                    }
                }, item);
            }

            _menu.ShowAsContext();//显示菜单
        }
    }

    public static void DrawDropdownButton<T>(List<T> objects, string nowTitle, DropDownEvent<T> _dropEvent, int width, int height = 20)
    {
        if (EditorGUILayout.DropdownButton(new GUIContent(nowTitle), FocusType.Keyboard, GUILayout.Width(width), GUILayout.Height(height)))
        {
            GenericMenu _menu = new GenericMenu();
            for (int i = 0; i < objects.Count; i++)
            {
                var item = objects[i].ToString();
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                var obj = objects[i];
                //添加菜单
                _menu.AddItem(new GUIContent(item), nowTitle.Equals(item), (object value) =>
                {
                    _dropEvent(obj);
                }, item);
            }

            _menu.ShowAsContext();//显示菜单
        }
    }

    public virtual void SavePrefab()
    {
    }

    private EditorWindow gameEditor
    {
        get
        {
            if (_gameEditor == null)
            {
                _gameEditor = UnityEditorWindowController.Instance.GetUnityWindow("GameView");
            }

            return _gameEditor;
        }
    }

    private EditorWindow _gameEditor;
    public bool forceGame;

    public void RepaintGame()
    {
        gameEditor.Repaint();
    }

    public void FocusGame()
    {
        gameEditor.Focus();
    }

    public virtual new void Repaint()
    {
        if (forceGame)
        {
            RepaintGame();
        }
        base.Repaint();
    }

    public virtual void Default()
    { }

    public virtual void SetTestLightON(bool ON)
    {
    }

    public virtual void MoveTestLight(Vector2 offset)
    {
    }
}

public interface CommonObj
{
    public CommonObj CreatNew(int count);

    public void Delete();

    public void ChecckSearch(string key, ObjSearchType objSearchType);

    public bool GetSearch();

    public void InitSearch();

    public void SelectAction();

    public void NoSelectAction();

    public bool CheckHide();

    public void Save();

    public void SetHide();

    public void SetNoHide();

    public string GetName();

    public string GetId();

    public void DrawTextureWithTexCoords(float posX, float posY, int texSize);
}

public class CommonEditor : Editor
{
    public void InitData(MyEditor myEditor, GUIStyle textStyle)
    {
        this.myEditor = myEditor;
        this.textStyle = textStyle;
    }

    /*
    public CommonEditor(MyEditor myEditor, GUIStyle textStyle)
    {
        this.myEditor = myEditor;
        this.textStyle = textStyle;
    }
    */
    private MyEditor myEditor;
    private Vector2 scrollPos;

    public CommonObj selectObj
    {
        get
        {
            return _selectObj;
        }
        set
        {
            if (value != null && value != _selectObj)
            {
                value.SelectAction();
            }
            _selectObj = value;
            myEditor.Repaint();
        }
    }

    private CommonObj _selectObj;
    private List<CommonObj> selectObjects = new List<CommonObj>();

    private List<CommonObj> AllObjects = new List<CommonObj>();
    private bool selectObjList = false;
    private ObjDisplayType objDisplayType = ObjDisplayType.名字;
    private ObjSearchType objSearchType;
    private string objSearchKey = "";
    private string objSerachTag = "Default";
    public CommonObj model = null;
    private GUIStyle textStyle;

    public void SetObjDisplayType(ObjDisplayType objDisplayType)
    {
        this.objDisplayType = objDisplayType;
    }

    public void UpDataSelect(List<CommonObj> selectObjects)
    {
        this.selectObjects = selectObjects;
        selectObj = selectObjects[0];
    }

    public void UpDataSelect(CommonObj selectObject)
    {
        List<CommonObj> selectObjects = new List<CommonObj>
        {
            selectObject
        };
        this.selectObjects = selectObjects;
        selectObj = selectObjects[0];
    }

    public void DisplayCommonObjList<T>(int width, int height, List<CommonObj> commonObjs, int widthCount, bool defaultSelect = true,
         bool hideSet = false, bool addAndDelete = false, bool displayChangeSelect = false, bool save = false,
         bool isSearch = false, bool changeSearch = false, int texSize = 20)
    {
        if (model == null)
        {
            model = (CommonObj)(System.Activator.CreateInstance<T>());
        }

        AllObjects = commonObjs;
        width += 10;

        if (isSearch)
        {
            ShowSerach(changeSearch);
        }

        //this.model = model;
        int itemWidth = (int)((width - 20) / widthCount);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(width), GUILayout.Height(height));
        if (defaultSelect)
        {
            if (selectObj == null && commonObjs.Count > 0)
            {
                selectObj = commonObjs[0];
            }
        }

        int index = 0;
        for (int i = 0; i < commonObjs.Count; i++)
        {
            if (commonObjs[i].GetSearch())
            {
                if (index % widthCount == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                string title = commonObjs[i].GetName();
                if (objDisplayType == ObjDisplayType.Id)
                {
                    title = commonObjs[i].GetId();
                }
                if (!selectObjList)
                {
                    bool select = selectObj == commonObjs[i];

                    MyEditor.DrawToggle(select, title, (bool value) =>
                    {
                        if (value)
                        {
                            if (selectObj != null)
                            {
                                selectObj.NoSelectAction();
                            }
                            selectObj = commonObjs[i];
                            myEditor.Default();
                            myEditor.Repaint();
                        }
                    }, true, true, itemWidth, texSize + 2);
                }
                else
                {
                    var _myObject = commonObjs[i];

                    bool select = selectObjects.Contains(_myObject);
                    MyEditor.DrawToggle(select, title, (bool value) =>
                    {
                        if (value)
                        {
                            selectObj = _myObject;
                            selectObjects.Add(_myObject);
                            myEditor.Default();
                        }
                        else
                        {
                            _myObject.NoSelectAction();
                            selectObjects.Remove(_myObject);
                        }
                        myEditor.Repaint();
                    }, true, true, itemWidth, texSize + 2);
                }

                float posX = itemWidth * (index % widthCount);
                float posY = (texSize + 6) * (index / widthCount);
                if (hideSet)
                {
                    GUI.DrawTexture(new Rect(new Vector2(posX + 4, posY + 4), new Vector2(texSize, texSize)),
                        commonObjs[i].CheckHide() ? CommonItem.eye1 : CommonItem.eye);
                    posX += 30;
                }

                if (index % widthCount == widthCount - 1)
                {
                    EditorGUILayout.EndHorizontal();
                }
                index++;
            }
        }
        if (index % widthCount != 0)
        {
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        int buttonCount = 0;
        buttonCount = hideSet ? buttonCount + 2 : buttonCount;
        buttonCount = addAndDelete ? buttonCount + 2 : buttonCount;
        buttonCount = displayChangeSelect ? buttonCount + 1 : buttonCount;
        buttonCount = save ? buttonCount + 1 : buttonCount;
        buttonCount = Mathf.Max(buttonCount, 1);
        int buttonWidth = width / buttonCount;
        if (hideSet)
        {
            ShowHideButton(buttonWidth);
        }
        if (addAndDelete)
        {
            ShowAddAndDelete(buttonWidth);
        }
        if (displayChangeSelect)
        {
            ShowChangeSelect(displayChangeSelect, buttonWidth);
        }
        if (save)
        {
            MyEditor.DrawButton("保存", () => { model.Save(); }, buttonWidth);
        }
        EditorGUILayout.EndHorizontal();
    }

    public void DisplayTexCommonObjList<T>(int width, int height, List<CommonObj> commonObjs, int widthCount, bool defaultSelect = true,
        bool hideSet = false, bool addAndDelete = false, bool displayChangeSelect = false, bool save = false,
        bool isSearch = false, bool changeSearch = false, int texSize = 26)
    {
        if (model == null)
        {
            model = (CommonObj)(System.Activator.CreateInstance<T>());
        }
        AllObjects = commonObjs;
        width += 10;

        if (isSearch)
        {
            ShowSerach(changeSearch);
        }

        //this.model = model;
        int itemWidth = (int)(width / widthCount);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(width), GUILayout.Height(height));
        if (defaultSelect)
        {
            if (selectObj == null && commonObjs.Count > 0)
            {
                selectObj = commonObjs[0];
            }
        }

        int index = 0;
        for (int i = 0; i < commonObjs.Count; i++)
        {
            if (commonObjs[i].GetSearch())
            {
                if (index % widthCount == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }

                if (!selectObjList)
                {
                    bool select = selectObj == commonObjs[i];

                    MyEditor.DrawToggle(select, "", (bool value) =>
                    {
                        if (value)
                        {
                            if (selectObj != null)
                            {
                                selectObj.NoSelectAction();
                            }
                            selectObj = commonObjs[i];
                            myEditor.Default();
                            myEditor.Repaint();
                        }
                    }, true, false, itemWidth, texSize + 2);
                }
                else
                {
                    var _myObject = commonObjs[i];

                    bool select = selectObjects.Contains(_myObject);
                    MyEditor.DrawToggle(select, "", (bool value) =>
                    {
                        if (value)
                        {
                            selectObjects.Add(_myObject);
                            selectObj = commonObjs[i];
                            myEditor.Default();
                        }
                        else
                        {
                            _myObject.NoSelectAction();
                            selectObjects.Remove(_myObject);
                        }
                        myEditor.Repaint();
                    }, true, false, itemWidth, texSize + 2);
                }

                float posX = itemWidth * (index % widthCount);
                float posY = (texSize + 6) * (index / widthCount);
                //GUI.DrawTexture(new Rect(new Vector2(posX + 4* (index % widthCount), posY + 4), new Vector2(texSize, texSize)), MapInitData.b1);
                string title = commonObjs[i].GetName();
                if (objDisplayType == ObjDisplayType.Id)
                {
                    title = commonObjs[i].GetId();
                }

                if (textStyle == null)
                {
                    textStyle = GUI.skin.label;
                }
                GUI.Label(new Rect(new Vector2(posX + 30 + 4 * (index % widthCount), posY + 2), new Vector2(itemWidth - 32, texSize)), title, textStyle);
                commonObjs[i].DrawTextureWithTexCoords(posX + 2 * (index % widthCount), posY, texSize);

                if (index % widthCount == widthCount - 1)
                {
                    EditorGUILayout.EndHorizontal();
                }
                index++;
            }
        }
        if (index % widthCount != 0)
        {
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        int buttonCount = 0;
        buttonCount = hideSet ? buttonCount + 2 : buttonCount;
        buttonCount = addAndDelete ? buttonCount + 2 : buttonCount;
        buttonCount = displayChangeSelect ? buttonCount + 1 : buttonCount;
        buttonCount = save ? buttonCount + 1 : buttonCount;
        buttonCount = Mathf.Max(buttonCount, 1);
        int buttonWidth = width / buttonCount;
        if (hideSet)
        {
            ShowHideButton(buttonWidth);
        }
        if (addAndDelete)
        {
            ShowAddAndDelete(buttonWidth);
        }
        if (displayChangeSelect)
        {
            ShowChangeSelect(displayChangeSelect, buttonWidth);
        }
        if (save)
        {
            MyEditor.DrawButton("保存", () => { model.Save(); }, buttonWidth);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void AddCommonObj()
    {
        int count = 0;
        if (selectObjects != null)
        {
            count = selectObjects.Count;
        }
        var newObj = this.model.CreatNew(count);
        selectObjects.Add(newObj);
        selectObj = newObj;
        myEditor.Repaint();
    }

    private void DeleteCommonObj()
    {
        this.model.Delete();
        selectObjects.Clear();
        myEditor.Repaint();
    }

    private void SwitchSelectType()
    {
        if (selectObjList)
        {
            if (selectObj != null)
            {
                selectObjects.Add(selectObj);
            }
        }
        selectObjList = !selectObjList;
    }

    private void ShowHideButton(int buttonWidth)
    {
        MyEditor.DrawButton("隐藏", model.SetHide, buttonWidth);
        MyEditor.DrawButton("显示", model.SetNoHide, buttonWidth);
    }

    private void ShowChangeSelect(bool displayChangeSelect, int buttonWidth)
    {
        if (displayChangeSelect)
        {
            MyEditor.DrawButton(selectObjList ? "多选" : "单选", SwitchSelectType, buttonWidth);
            myEditor.Repaint();
        }
    }

    private void ShowAddAndDelete(int buttonWidth)
    {
        if (MyEditor.canSelect)
        {
            MyEditor.DrawButton("新增", AddCommonObj, buttonWidth);
            MyEditor.DrawButton("删除", DeleteCommonObj, buttonWidth);
        }
    }

    private void ShowSerach(bool changeSerach)
    {
        EditorGUILayout.BeginHorizontal("Button", GUILayout.Height(20));
        if (changeSerach)
        {
            objSearchType = (ObjSearchType)EditorGUILayout.EnumPopup(objSearchType, GUILayout.Width(70));
            switch (objSearchType)
            {
                case ObjSearchType.名字搜索:
                    MyEditor.DrawTextField(ref objSearchKey, "", 0, 100);
                    break;

                case ObjSearchType.Tag搜索:

                    //MyEditor.DrawDropdownButton(OutDataManager.instance.tags, objSerachTag, (string obj) => { objSerachTag = obj ; }, 100);
                    break;
            }
        }
        else
        {
            objSearchType = ObjSearchType.名字搜索;
            MyEditor.DrawTextField(ref objSearchKey, "", 0, 70);
        }
        MyEditor.DrawButton("搜索", () =>
        {
            foreach (var myObject in AllObjects)
            {
                if (objSearchType == ObjSearchType.名字搜索)
                {
                    myObject.ChecckSearch(objSearchKey, objSearchType);
                }
                if (objSearchType == ObjSearchType.Tag搜索)
                {
                    myObject.ChecckSearch(objSerachTag, objSearchType);
                }
            }
            myEditor.Repaint();
        }, 40);
        MyEditor.DrawButton("取消", () =>
        {
            foreach (var myObject in AllObjects)
            {
                myObject.InitSearch();
            }
            myEditor.Repaint();
        }, 40);

        EditorGUILayout.EndHorizontal();
    }

    public void SetTextStyle(GUIStyle textStyle)
    {
        this.textStyle = textStyle;
    }

    public void SetSelectObjList(bool _selectObjList)
    {
        selectObjList = _selectObjList;
    }

    public void ClearSelect()
    {
        selectObj = null;
        selectObjects.Clear();
    }
}