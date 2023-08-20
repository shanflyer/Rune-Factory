
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


public class UnityEditorWindowController : EditorWindow
{
    static Dictionary<string,Type> windowList = new Dictionary<string, Type>();

    public static UnityEditorWindowController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = CreateInstance<UnityEditorWindowController>();
            }
            return _instance;
        }
    }
    private static UnityEditorWindowController _instance;

    public UnityEditorWindowController()
    {
        windowList.Clear();
        var _types = getWindowAll();
        foreach(var t in _types)
        {
            windowList.Add(t.Name, t);
        }
    }
    
    
    static List<Type> getWindowAll()
    {
        Assembly assembly = typeof(EditorWindow).Assembly; //获取UnityEditor程序集，当然你也可以直接加载UnityEditor程序集来获取，我这里图方便,具体方法看一下程序集的加载Assembly.Load();
        Type[] types = assembly.GetTypes();
        List<Type> list = new List<Type>();
        for (int i = 0; i < types.Length; i++)
        {
            if (isEditorWindow(types[i]))
            {
                if (types[i].Name == "GameView")
                {
                  
                   // Debug.Log(types[i].FullName);
                }

                if (types[i].Name == "SceneView")
                {
                   // Debug.Log(types[i].FullName);
                }
                list.Add(types[i]);
            }

        }
        list.Sort((a, b) => { return string.Compare(a.Name, b.Name); });  //排序
        return list;
    }
   

    static bool isEditorWindow(Type type)
    {
        int i = 0;
        Type temp = type;
        while (null != temp && i < 10000)
        {
            i++;
            if (temp.BaseType == typeof(EditorWindow))
            {
                return true;
            }
            temp = temp.BaseType;
        }
        return false;
    }
    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    void closeWindowAll()
    {
        
    }
    void showWindowAll()
    {
        
    }
   
    public void Test()
    {

    }
    public EditorWindow GetUnityWindow(string typeName)
    {
        if (windowList.ContainsKey(typeName))
        {
            EditorWindow editorWindow = EditorWindow.GetWindow(windowList[typeName]);
            return editorWindow;
        }
        return null;
    }
    public EditorWindow GetUnityWindow(Type type)
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(type);
        return editorWindow;
    }
    
}
